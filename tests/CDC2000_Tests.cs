using System;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class CDC2000_Tests : IClassFixture<AnthStatFixture>
    {
        private readonly AnthStatFixture _fixture;
        private static double TOLERANCE = 0.00000001;

        public CDC2000_Tests(AnthStatFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IsValidIndicator_Success()
        {
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.BMIForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.HeightForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.HCForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.LengthForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForHeight));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForLength));
            Assert.False(_fixture.CDC2000.IsValidIndicator(Indicator.SSFForAge));
            Assert.False(_fixture.CDC2000.IsValidIndicator(Indicator.TSFForAge));
        }

        [Fact]
        public void Compute_Performance_Success()
        {
            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 10000;

            double [] ageMonths = new double[loopIterations];
            double [] bmis = new double[loopIterations];
            Sex [] sexes = new Sex[loopIterations];

            for (int i = 0; i < loopIterations; i++)
            {
                ageMonths[i] = rnd.Next(25, 239) + 0.5;
                bmis[i] = rnd.NextDouble() * 25;
                sexes[i] = (ageMonths[i] - 0.5) % 2 == 0 ? Sex.Female : Sex.Male;
            }

            sw.Start();

            for (int i = 0; i < loopIterations; i++)
            {                
                _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, ageMonths[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Assert.True(sw.Elapsed.TotalMilliseconds >= 0.0);
            Assert.True(sw.Elapsed.TotalMilliseconds <= 5000.0); // we have problems if this takes more than this long
        }

        [Fact]
        public void ComputeZScore_Female_Bmi_Success()
        {
            // arrange
            double correctAnswer0 = -0.89998579698103833912656059986439;
            double correctAnswer1 = -1.0693184563449251569543525091431;
            double correctAnswer2 = -0.06782127757198954073396495854781;
            double correctAnswer3 = -0.88927079421466268379009544748404;
            double correctAnswer4 = 0.08296951497400838075264190214788;

            // act
            double z0 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 24, 15.25, Sex.Female);
            double z1 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 25, 15.0, Sex.Female); // interpolation
            double z2 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 240.5, 21.5, Sex.Female);
            double z3 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 24.25, 15.25, Sex.Female); // interpolation
            double z4 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 240.125, 22, Sex.Female); // interpolation
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
            Assert.True(Math.Abs(z2 - correctAnswer2) < TOLERANCE);
            Assert.True(Math.Abs(z3 - correctAnswer3) < TOLERANCE);
            Assert.True(Math.Abs(z4 - correctAnswer4) < TOLERANCE);
        }

        [Fact]
        public void ComputeZScore_Male_Bmi_Success()
        {
            // arrange
            double correctAnswer0 = -0.89076945049192844341949062350922;
            double correctAnswer1 = -0.25031381193856607306216062869678;

            // act
            double z0 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 24, 15.5, Sex.Male);
            double z1 = _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, 240.5, 22.3, Sex.Male);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Theory]
        [InlineData(23.5)]
        [InlineData(23.99999)]
        [InlineData(23.9999999999)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-23.999999)]
        public void ComputeZScore_Bmi_Under_24_Fail(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, ageMonths, 16.9, Sex.Female);
            });
        }

        [Theory]
        [InlineData(240.511111)]
        [InlineData(240.6)]
        [InlineData(241)]
        [InlineData(2500)]
        public void ComputeZScore_Female_Bmi_Over_228_Fail(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, ageMonths, 16.9, Sex.Female);
            });
        }

        [Theory]
        [InlineData(240.511111)]
        [InlineData(240.6)]
        [InlineData(241)]
        [InlineData(2500)]
        public void ComputeZScore_Male_Bmi_Over_228_Fail(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.BMIForAge, ageMonths, 16.9, Sex.Male);
            });
        }

        [Fact]
        public void ComputeZScore_Female_HeadCircumference_Success()
        {
            // arrange
            double correctAnswer0 = 0.17548045698599557709310304242613;
            double correctAnswer1 = 0.23713112594777027575544725157751;

            // act
            double z0 = _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, 0, 35, Sex.Female);
            double z1 = _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, 36, 49, Sex.Female);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Fact]
        public void ComputeZScore_Male_HeadCircumference_Success()
        {
            // arrange
            double correctAnswer0 = 0.10061611037407461443909017517158;
            double correctAnswer1 = 0.19600185277546031314716123032251;

            // act
            double z0 = _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, 0, 36, Sex.Male);
            double z1 = _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, 36, 50, Sex.Male);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < TOLERANCE);
            Assert.True(Math.Abs(z1 - correctAnswer1) < TOLERANCE);
        }

        [Theory]
        [InlineData(-0.00001)]
        [InlineData(-1)]
        [InlineData(-23.999999)]
        public void ComputeZScore_HeadCircumference_Under_0_Fail(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, ageMonths, 42, Sex.Female);
            });
        }

        [Theory]
        [InlineData(36.00001)]
        [InlineData(36.5)]
        [InlineData(220)]
        [InlineData(2500)]
        public void ComputeZScore_HeadCircumference_Over_36_Fail(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.HCForAge, ageMonths, 42, Sex.Female);
            });
        }

        [Theory]
        [InlineData(0, 50, Sex.Male, 0.00418688126503143130789745649004)]
        [InlineData(35.5, 96, Sex.Male, 0.14213978751143433671016268208901)]
        [InlineData(0, 50, Sex.Female, 0.28435009141031476854216759291299)]
        [InlineData(35.5, 96, Sex.Female, 0.39378003327433646896479980436778)]
        public void ComputeZScore_LengthForAge_Success(double ageMonths, double length, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.ComputeZScore(Indicator.LengthForAge, ageMonths, length, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
        }

        [Theory]
        [InlineData(-23.999999)]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        [InlineData(35.50001)]
        [InlineData(35.6)]
        [InlineData(36)]
        [InlineData(36.5)]
        [InlineData(220)]
        [InlineData(2500)]
        public void ComputeZScore_LengthForAge_Out_of_Range(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.LengthForAge, ageMonths, 42, Sex.Female);
            });
        }

        [Theory]
        [InlineData(24, 87, Sex.Male, 0.15711870954014771387774711449819)]
        [InlineData(240, 177, Sex.Male, 0.02111943090198769197653633371963)]
        [InlineData(24, 87, Sex.Female, 0.58454144574638665069122147068065)]
        [InlineData(240, 177, Sex.Female, 2.119514173096064192005892294081)]
        public void ComputeZScore_HeightForAge_Success(double ageMonths, double height, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.ComputeZScore(Indicator.HeightForAge, ageMonths, height, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
        }

        [Theory]
        [InlineData(-23.999999)]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        [InlineData(0)]
        [InlineData(23.5)]
        [InlineData(23.9)]
        [InlineData(23.9999)]
        [InlineData(240.00001)]
        [InlineData(240.1)]
        [InlineData(240.5)]
        [InlineData(241)]
        [InlineData(2500)]
        public void ComputeZScore_HeightForAge_Out_of_Range(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.HeightForAge, ageMonths, 95, Sex.Female);
            });
        }

        [Theory]
        [InlineData(24, 13, Sex.Male, 0.23661706727665810477527237764342)]
        [InlineData(240, 71, Sex.Male, 0.03510563254505139845943837691245)]
        [InlineData(24, 13, Sex.Female, 0.68353013727719231811600292602077)]
        [InlineData(240, 71, Sex.Female, 1.0287507199035549922743068005002)]
        public void ComputeZScore_WeightForAge_Success(double ageMonths, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.ComputeZScore(Indicator.WeightForAge, ageMonths, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
        }

        [Theory]
        [InlineData(-23.999999)]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        [InlineData(240.00001)]
        [InlineData(240.1)]
        [InlineData(240.5)]
        [InlineData(241)]
        [InlineData(2500)]
        public void ComputeZScore_WeightForAge_Out_of_Range(double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.WeightForAge, ageMonths, 45, Sex.Female);
            });
        }

        [Theory]
        [InlineData(45, 2.3, Sex.Male, 0.03000312167769061660016810310108)]
        [InlineData(103.5, 17, Sex.Male, 0.41316797424842034104217535427253)]
        [InlineData(45, 2.3, Sex.Female, -0.01386007738002724285466516799032)]
        [InlineData(103.5, 17, Sex.Female, 0.52186208516798431258478098458335)]
        public void ComputeZScore_WeightForLength_Success(double length, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.ComputeZScore(Indicator.WeightForLength, length, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
        }

        [Theory]
        [InlineData(-14.5)]
        [InlineData(-44.99999)]
        [InlineData(103.500001)]
        [InlineData(104)]
        [InlineData(2500)]
        public void ComputeZScore_WeightForLength_Out_of_Range(double length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.WeightForLength, length, 13, Sex.Female);
            });
        }

        [Theory]
        [InlineData(77, 10.3, Sex.Male, 0.03222330872952046657980460530646)]
        [InlineData(121.5, 22.8, Sex.Male, 0.00752883854055974186429771683978)]
        [InlineData(77, 10.3, Sex.Female, 0.253740944296596305102408225582)]
        [InlineData(121.5, 22.8, Sex.Female, -0.06282781130115447381457454626206)]
        public void ComputeZScore_WeightForHeight_Success(double height, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.ComputeZScore(Indicator.WeightForHeight, height, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
        }

        [Theory]
        [InlineData(-14.5)]
        [InlineData(-76.99999)]
        [InlineData(121.500001)]
        [InlineData(122)]
        [InlineData(2500)]
        public void ComputeZScore_WeightForHeight_Out_of_Range(double height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.ComputeZScore(Indicator.WeightForHeight, height, 18, Sex.Female);
            });
        }
    }
}