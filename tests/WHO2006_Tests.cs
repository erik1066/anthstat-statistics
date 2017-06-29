using System;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class WHO2006_Tests : IClassFixture<AnthStatFixture>
    {
        private readonly AnthStatFixture _fixture;

        public WHO2006_Tests(AnthStatFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IsValidIndicator_Success()
        {
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.BMIForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.WeightForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.HeightForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.ACForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.HCForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.LengthForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.SSFForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.TSFForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.WeightForHeight));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.WeightForLength));
        }
        
        [Fact]
        public void ComputeZScore_Female_Bmi_Success()
        {
            // Note: These z-scores were calculated using Windows calculator and the formula at http://www.who.int/childgrowth/standards/Chap_7.pdf. Keep
            // in mind that for WHO z-scores, a z-score > 3 or < -3 uses a different formula, which is also found in the same PDF document. See page 
            // numbered 302.

            // arrange
            double correctAnswer0_high = 2.5351948017104595832200737120844; // day 0, z > 0
            double correctAnswer1_high = 2.5554397881274597389255441580724; // day 1, z > 0
            double correctAnswer1856_high = 2.0860332469456686513526025662104; // day 1856, z > 0

            double correctAnswer15_high_over3 = 3.0325847159162528421113690449965; // day 15, z > 3 (uses different z-score calc routine)
            double correctAnswer15_low_under3 = -3.4017596805071390108663953082457; // day 15, z < -3 (uses different z-score calc routine)

            double correctAnswer0_low = -0.19291951810719225993362490649157; // day 0, z < 0
            double correctAnswer1_low = -1.7506771691296057225008679776202; // day 1, z < 0

            // act
            double flag0h = 0.0;
            double z0_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 0, 16.9, Sex.Female, ref flag0h);

            double flag1h = 0.0;
            double z1_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1, 16.9, Sex.Female, ref flag1h);            

            double flag1856h = 0.0;
            double z1856_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1856, 19.0, Sex.Female, ref flag1856h);            

            double flag15h = 0.0;
            double z15_high_over3 = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 15, 17.8, Sex.Female, ref flag15h);

            double flag15l = 0.0;
            double z15_low_under3 = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 15, 9.4, Sex.Female, ref flag15l);

            double flag0l = 0.0;
            double z0_low = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 0, 13.1, Sex.Female, ref flag0l);

            double flag1l = 0.0;
            double z1_low = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1, 11.3, Sex.Female, ref flag1l);     
            
            // assert
            Assert.True(Math.Abs(z0_high - correctAnswer0_high) < 0.00000001);
            Assert.True(Math.Abs(z1_high - correctAnswer1_high) < 0.00000001);
            Assert.True(Math.Abs(z1856_high - correctAnswer1856_high) < 0.00000001);

            Assert.True(Math.Abs(z15_high_over3 - correctAnswer15_high_over3) < 0.00000001);
            Assert.True(Math.Abs(z15_low_under3 - correctAnswer15_low_under3) < 0.00000001);

            Assert.True(Math.Abs(z0_low - correctAnswer0_low) < 0.00000001);
            Assert.True(Math.Abs(z1_low - correctAnswer1_low) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_Bmi_Success()
        {
            // Note: These z-scores were calculated using Windows calculator and the formula at http://www.who.int/childgrowth/standards/Chap_7.pdf. Keep
            // in mind that for WHO z-scores, a z-score > 3 or < -3 uses a different formula, which is also found in the same PDF document. See page 
            // numbered 302.
            
            // arrange
            double correctAnswer0_high = 2.3383852398458963129137559171807; // day 0, z > 0
            double correctAnswer1_high = 2.3681904235271979513747283079005; // day 1, z > 0
            double correctAnswer1856_high = 2.3679427793494169336926696348487; // day 1856, z > 0

            double correctAnswer15_high_over3 = 6.1003421244398328948597199712218; // day 15, z > 3 (uses different z-score calc routine)
            double correctAnswer15_low_under3 = -3.6831532463944048076547252044075; // day 15, z < -3 (uses different z-score calc routine)

            double correctAnswer0_low = -0.24308915070120925814059695957408; // day 0, z < 0
            double correctAnswer1_low = -0.23455795334480738910148700514505; // day 1, z < 0

            // act
            double flag0h = 0.0;
            double z0_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 0, 16.9, Sex.Male, ref flag0h);

            double flag1h = 0.0;
            double z1_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1, 16.9, Sex.Male, ref flag1h);            

            double flag1856h = 0.0;
            double z1856_high = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1856, 19.0, Sex.Male, ref flag1856h);            

            double flag15h = 0.0;
            double z15_high_over3 = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 15, 22.5, Sex.Male, ref flag15h);

            double flag15l = 0.0;
            double z15_low_under3 = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 15, 9.4, Sex.Male, ref flag15l);

            double flag0l = 0.0;
            double z0_low = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 0, 13.1, Sex.Male, ref flag0l);

            double flag1l = 0.0;
            double z1_low = _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, 1, 13.1, Sex.Male, ref flag1l);     
            
            // assert
            Assert.True(Math.Abs(z0_high - correctAnswer0_high) < 0.00000001);
            Assert.True(Math.Abs(z1_high - correctAnswer1_high) < 0.00000001);
            Assert.True(Math.Abs(z1856_high - correctAnswer1856_high) < 0.00000001);

            Assert.True(Math.Abs(z15_high_over3 - correctAnswer15_high_over3) < 0.00000001);
            Assert.True(Math.Abs(z15_low_under3 - correctAnswer15_low_under3) < 0.00000001);

            Assert.True(Math.Abs(z0_low - correctAnswer0_low) < 0.00000001);
            Assert.True(Math.Abs(z1_low - correctAnswer1_low) < 0.00000001);
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-1)]
        public void ComputeZScore_Bmi_Under_Zero_Fail(int ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, ageDays, 16.9, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Female_Bmi_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, ageDays, 16.9, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Male_Bmi_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.BMIForAge, ageDays, 16.9, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_WeightLength_Success()
        {            
            // arrange
            double correctAnswer0 = 2.1134935536236982591548727107661;
            double correctAnswer1 = -1.4859353052199340839944555784493;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, 45, 3.00, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, 110, 16.00, Sex.Female, ref flag1);  
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_WeightLength_Success()
        {            
            // arrange
            double correctAnswer0 = 0.62961642936973316222835417730078;
            double correctAnswer1 = -0.18503233427487158999085862877748;
            double correctAnswer2 = -1.184325245059326230973893688293;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, 49.9, 3.50, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, 45.0, 2.40, Sex.Male, ref flag1);            

            double flag2 = 0.0;
            double z2 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, 110.0, 16.50, Sex.Male, ref flag2);
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
            Assert.True(Math.Abs(z2 - correctAnswer2) < 0.00000001);
        }

        [Theory]
        [InlineData(110.0001)]
        [InlineData(110.1)]
        [InlineData(111.0)]
        [InlineData(2000)]
        public void ComputeZScore_Female_WeightLength_Over_110_Fail(double length)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, length, 16.50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(110.0001)]
        [InlineData(110.1)]
        [InlineData(111.0)]
        [InlineData(2000)]
        public void ComputeZScore_Male_WeightLength_Over_110_Fail(double length)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, length, 16.50, Sex.Male, ref flag);
            });
        }

        [Theory]
        [InlineData(44.999)]
        [InlineData(44.5)]
        [InlineData(44.0)]
        [InlineData(0)]
        public void ComputeZScore_Female_WeightLength_Under_45_Fail(double length)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, length, 16.50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(44.999)]
        [InlineData(44.5)]
        [InlineData(44.0)]
        [InlineData(0)]
        public void ComputeZScore_Male_WeightLength_Under_45_Fail(double length)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForLength, length, 16.50, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_WeightHeight_Success()
        {            
            // arrange
            double correctAnswer0 = 1.0743800294516307983167802998867;
            double correctAnswer1 = -0.85806737835475871013745232431556;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, 65.0, 8.0, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, 120.0, 21.0, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_WeightHeight_Success()
        {            
            // arrange
            double correctAnswer0 = 0.88363276925258927969686943523615;
            double correctAnswer1 = -0.66988002648118219306594721792577;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, 65.0, 8.0, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, 120.0, 21.0, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(120.0001)]
        [InlineData(120.1)]
        [InlineData(121.0)]
        [InlineData(2000)]
        public void ComputeZScore_Female_WeightHeight_Over_120_Fail(double height)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, height, 21.00, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(120.0001)]
        [InlineData(120.1)]
        [InlineData(121.0)]
        [InlineData(2000)]
        public void ComputeZScore_Male_WeightHeight_Over_120_Fail(double height)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, height, 21.00, Sex.Male, ref flag);
            });
        }

        [Theory]
        [InlineData(64.999)]
        [InlineData(64.5)]
        [InlineData(64.0)]
        [InlineData(0)]
        public void ComputeZScore_Female_WeightHeight_Under_65_Fail(double height)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, height, 8.50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(64.999)]
        [InlineData(64.5)]
        [InlineData(64.0)]
        [InlineData(0)]
        public void ComputeZScore_Male_WeightHeight_Under_65_Fail(double height)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.WeightForHeight, height, 8.50, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_ACForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.40589404468054411701689483303183;
            double correctAnswer1 = -0.24320811728720011655502617969338;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, 91, 13.47, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, 1856, 16.52, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_ACForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.0078438450518674409912225165818;
            double correctAnswer1 = -0.0213971396073446674965169466961;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, 91, 13.47, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, 1856, 16.52, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2000)]
        public void ComputeZScore_Female_ACForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, ageDays, 21.00, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2000)]
        public void ComputeZScore_Male_ACForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, ageDays, 21.00, Sex.Male, ref flag);
            });
        }

        [Theory]
        [InlineData(90)]
        [InlineData(0)]
        public void ComputeZScore_Female_ACForAge_Under_91_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, ageDays, 8.50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(90)]
        [InlineData(0)]
        public void ComputeZScore_Male_ACForAge_Under_91_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.ACForAge, ageDays, 8.50, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_HCForAge_Success()
        {            
            // arrange
            double correctAnswer0 = 0.10241478078755314955626554581229;
            double correctAnswer1 = 0.02445522308245640089097517490151;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, 0, 34.0, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, 1856, 50.0, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_HCForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.36354706265671747887389286733164;
            double correctAnswer1 = -0.51788656191369656140211656443226;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, 0, 34.0, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, 1856, 50.0, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Female_HCForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, ageDays, 50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Male_HCForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.HCForAge, ageDays, 50, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_HeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.07929359105980168560136240669803;
            double correctAnswer1 = 0.01352542775910235322705902688034;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, 0, 49.0, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, 1856, 110.0, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_HeightForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.46706327321797969208676755180545;
            double correctAnswer1 = -0.10641170700920362584779158876099;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, 0, 49.0, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, 1856, 110.0, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Female_HeightForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, ageDays, 50, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Male_HeightForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.HeightForAge, ageDays, 50, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_SSFForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.06131747268213446329236853256366;
            double correctAnswer1 = 0.00799538263959151494094926122301;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, 91, 7.7, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, 1856, 6.1, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_SSFForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.0708299062152796216572904695866;
            double correctAnswer1 = -0.05088789162859622106978046123322;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, 91, 7.6, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, 1856, 5.3, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Female_SSFForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, ageDays, 5.3, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Male_SSFForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.SSFForAge, ageDays, 5.3, Sex.Male, ref flag);
            });
        }

        [Fact]
        public void ComputeZScore_Female_TSFForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.03125237709867160097513556893842;
            double correctAnswer1 = -0.0230172170416324610038564541254;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, 91, 9.7, Sex.Female, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, 1856, 8.8, Sex.Female, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Fact]
        public void ComputeZScore_Male_TSFForAge_Success()
        {            
            // arrange
            double correctAnswer0 = -0.04069912797972332197733952426044;
            double correctAnswer1 = -0.03016340611257267952018295944132;

            // act
            double flag0 = 0.0;
            double z0 = _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, 91, 9.7, Sex.Male, ref flag0);

            double flag1 = 0.0;
            double z1 = _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, 1856, 7.5, Sex.Male, ref flag1);            
            
            // assert
            Assert.True(Math.Abs(z0 - correctAnswer0) < 0.00000001);
            Assert.True(Math.Abs(z1 - correctAnswer1) < 0.00000001);
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Female_TSFForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, ageDays, 5.3, Sex.Female, ref flag);
            });
        }

        [Theory]
        [InlineData(1857)]
        [InlineData(2500)]
        public void ComputeZScore_Male_TSFForAge_Over_1856_Fail(int ageDays)
        {
            Assert.Throws<InvalidOperationException>(delegate 
            { 
                double flag = 0;
                _fixture.WHO2006.ComputeZScore(Indicator.TSFForAge, ageDays, 5.3, Sex.Male, ref flag);
            });
        }
    }
}
