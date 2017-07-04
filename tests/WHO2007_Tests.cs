using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class WHO2007_Tests : IClassFixture<AnthStatFixture>
    {
        private readonly AnthStatFixture _fixture;
        private static double TOLERANCE = 0.0000001;

        public WHO2007_Tests(AnthStatFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IsValidIndicator_Success()
        {
            Assert.True(_fixture.WHO2007.IsValidIndicator(Indicator.BMIForAge));
            Assert.True(_fixture.WHO2007.IsValidIndicator(Indicator.WeightForAge));
            Assert.True(_fixture.WHO2007.IsValidIndicator(Indicator.HeightForAge));

            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.ArmCircumferenceForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.HeadCircumferenceForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.LengthForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.SubscapularSkinfoldForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.TricepsSkinfoldForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.WeightForHeight));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.WeightForLength));
        }

        [Fact]
        public void InterpolateLMS_HighPrecision_Success()
        {
            Dictionary<int, Lookup> reference = new Dictionary<int, Lookup>();

            int upperLimit = 12001;

            for(int i = 6101; i <= upperLimit; i = i + 100 )
            {
                int index = (i / 100);
                reference.Add(i, new Lookup(Sex.Male, index, index, 18.5057, 0.12988));
            }

            Parallel.ForEach(reference, (kvp) =>
            {
                double startValue = kvp.Value.L + 0.00001;
                double endValue = kvp.Value.L + 1;

                if (kvp.Key >= upperLimit || startValue >= upperLimit) return;

                for(double i = startValue; i < endValue; i = i + 0.00001)
                {
                    var result = _fixture.WHO2007.InterpolateLMS(Sex.Male, i, reference);
                    Assert.True(Math.Abs(result.Item1 - i) < TOLERANCE);
                }            
            });
        }
        
        [Theory]
        [InlineData(61, 15.25, Sex.Female, 0.00399188592946362171415191703895)]
        [InlineData(61.25, 15.0, Sex.Female, -0.16744085028872915457502540888129)]
        [InlineData(61.5, 15.0, Sex.Female, -0.1671289054811777288751676405857)]
        [InlineData(61.75, 15.0, Sex.Female, -0.16681768091548360135928283926596)]
        [InlineData(228, 21.5, Sex.Female, 0.02355411236719574110048857287015)]
        [InlineData(61, 15.5, Sex.Male, 0.18176176947909595711292281530935)]
        [InlineData(228, 14, Sex.Male, -4.0916811844026938377310090778574)]
        [InlineData(228, 22.3, Sex.Male, 0.03870061632191970322614664167712)]
        [InlineData(228, 48, Sex.Male, 5.1521182359390728771444244525155)]
        public void ComputeZScore_Bmi_Success(double ageMonths, double bmi, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2007.CalculateZScore(Indicator.BMIForAge, ageMonths, bmi, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2007.TryCalculateZScore(Indicator.BMIForAge, ageMonths, bmi, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 60.999)]
        [InlineData(Sex.Female, 60)]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Male, 60.999)]
        [InlineData(Sex.Male, 60)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Female, 228.001)]
        [InlineData(Sex.Female, 229)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 228.001)]
        [InlineData(Sex.Male, 229)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_Bmi_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2007.CalculateZScore(Indicator.BMIForAge, ageMonths, 16.9, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2007.TryCalculateZScore(Indicator.BMIForAge, ageMonths, 16.9, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(61, 111, Sex.Female, 0.29297216591791439484043165392626)]
        [InlineData(228, 165, Sex.Female, 0.2821028894574666272694560566999)]
        [InlineData(61, 111, Sex.Male, 0.16014646066168220837429482534521)]
        [InlineData(228, 177, Sex.Male, 0.06258995346449017524404321602369)]
        public void ComputeZScore_HeightForAge_Success(double ageMonths, double height, Sex sex, double zExpected)
        {            
            double z = _fixture.WHO2007.CalculateZScore(Indicator.HeightForAge, ageMonths, height, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2007.TryCalculateZScore(Indicator.HeightForAge, ageMonths, height, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 60)]
        [InlineData(Sex.Female, 60.999)]
        [InlineData(Sex.Female, 228.001)]
        [InlineData(Sex.Female, 229)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 60)]
        [InlineData(Sex.Male, 60.999)]
        [InlineData(Sex.Male, 228.001)]
        [InlineData(Sex.Male, 229)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_HeightForAge_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2007.CalculateZScore(Indicator.HeightForAge, ageMonths, 50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2007.TryCalculateZScore(Indicator.HeightForAge, ageMonths, 50, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(61, 19, Sex.Female, 0.27612384686525184921242050991054)]
        [InlineData(120, 32, Sex.Female, 0.02576612892148216178216280320772)]
        [InlineData(61, 19, Sex.Male, 0.20241701808371923536898311855956)]
        [InlineData(120, 32, Sex.Male, 0.16195602651114914198095832485062)]
        public void ComputeZScore_WeightForAge_Success(double ageMonths, double weight, Sex sex, double zExpected)
        {            
            double z = _fixture.WHO2007.CalculateZScore(Indicator.WeightForAge, ageMonths, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2007.TryCalculateZScore(Indicator.WeightForAge, ageMonths, weight, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 60)]
        [InlineData(Sex.Female, 60.999)]
        [InlineData(Sex.Female, 120.001)]
        [InlineData(Sex.Female, 121)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 60)]
        [InlineData(Sex.Male, 60.999)]
        [InlineData(Sex.Male, 120.001)]
        [InlineData(Sex.Male, 121)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_WeightForAge_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2007.CalculateZScore(Indicator.WeightForAge, ageMonths, 50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2007.TryCalculateZScore(Indicator.WeightForAge, ageMonths, 50, sex, ref z));
            Assert.True(z == -99);
        }
    }
}
