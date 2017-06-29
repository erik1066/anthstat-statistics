using System;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class WHO2007_Tests : IClassFixture<AnthStatFixture>
    {
        private readonly AnthStatFixture _fixture;
        private static double TOLERANCE = 0.00000001;

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

            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.ACForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.HCForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.LengthForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.SSFForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.TSFForAge));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.WeightForHeight));
            Assert.False(_fixture.WHO2007.IsValidIndicator(Indicator.WeightForLength));
        }
        
        [Fact]
        public void ComputeZScore_Female_Bmi_Success()
        {
            // arrange
            double correctAnswer0 = 0.00399188592946362171415191703895;
            double correctAnswer1 = -0.1671289054811777288751676405857;
            double correctAnswer2 = 0.02355411236719574110048857287015;

            // act
            double flag = 0.0;

            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, 61, 15.25, Sex.Female, ref flag);
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, 61.5, 15.0, Sex.Female, ref flag);
            double z2 = _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, 228, 21.5, Sex.Female, ref flag);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
            Assert.True(Math.Abs(z2 - correctAnswer2) < TOLERANCE);
        }

        [Fact]
        public void ComputeZScore_Male_Bmi_Success()
        {
            // arrange
            double correctAnswer0 = 0.18176176947909595711292281530935;
            double correctAnswer1 = 0.03870061632191970322614664167712;

            // act
            double flag = 0.0;

            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, 61, 15.5, Sex.Male, ref flag);
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, 228, 22.3, Sex.Male, ref flag);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Theory]
        [InlineData(60)]
        [InlineData(32)]
        [InlineData(0)]
        public void ComputeZScore_Bmi_Under_61_Fail(int ageMonths)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, ageMonths, 16.9, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(229)]
        [InlineData(2500)]
        public void ComputeZScore_Female_Bmi_Over_228_Fail(int ageMonths)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, ageMonths, 16.9, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(229)]
        [InlineData(2500)]
        public void ComputeZScore_Male_Bmi_Over_228_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.BMIForAge, ageDays, 16.9, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_HeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.29297216591791439484043165392626;
            double correctAnswer1 = 0.2821028894574666272694560566999;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, 61, 111, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, 228, 165, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Fact]
        public void ComputeZScore_Male_HeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.16014646066168220837429482534521;
            double correctAnswer1 = 0.06258995346449017524404321602369;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, 61, 111, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, 228, 177, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Theory]
        [InlineData(229)]
        [InlineData(2500)]
        public void ComputeZScore_Female_HeightForAge_Over_228_Fail(int ageMonths)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, ageMonths, 50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(229)]
        [InlineData(2500)]
        public void ComputeZScore_Male_HeightForAge_Over_1856_Fail(int ageMonths)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.HeightForAge, ageMonths, 50, Sex.Male, ref flag);
            });
        }


        [Fact]
        public void ComputeZScore_Female_WeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.27612384686525184921242050991054;
            double correctAnswer1 = 0.02576612892148216178216280320772;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, 61, 19, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, 120, 32, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Fact]
        public void ComputeZScore_Male_WeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.20241701808371923536898311855956;
            double correctAnswer1 = 0.16195602651114914198095832485062;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, 61, 19.0, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, 120, 32.0, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Theory]
        [InlineData(121)]
        [InlineData(2500)]
        public void ComputeZScore_Female_WeightForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, ageDays, 50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(121)]
        [InlineData(2500)]
        public void ComputeZScore_Male_WeightForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2007.ComputeZScore(Indicator.WeightForAge, ageDays, 50, Sex.Male, ref flag);
            });
        }
    }
}
