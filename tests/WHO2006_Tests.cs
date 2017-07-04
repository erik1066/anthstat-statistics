using System;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class WHO2006_Tests : IClassFixture<AnthStatFixture>
    {
        private readonly AnthStatFixture _fixture;
        private static double TOLERANCE = 0.0000001;

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
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.ArmCircumferenceForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.HeadCircumferenceForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.LengthForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.SubscapularSkinfoldForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.TricepsSkinfoldForAge));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.WeightForHeight));
            Assert.True(_fixture.WHO2006.IsValidIndicator(Indicator.WeightForLength));
        }

        [Fact]
        public void Compute_Performance_Success()
        {
            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 10000;

            int [] ageDays = new int[loopIterations];
            double [] bmis = new double[loopIterations];
            Sex [] sexes = new Sex[loopIterations];

            for (int i = 0; i < loopIterations; i++)
            {
                ageDays[i] = rnd.Next(0, 1856);
                bmis[i] = rnd.NextDouble() * 25;
                sexes[i] = ageDays[i] % 2 == 0 ? Sex.Female : Sex.Male;
            }

            sw.Start();

            for (int i = 0; i < loopIterations; i++)
            {                
                _fixture.WHO2006.CalculateZScore(Indicator.BMIForAge, ageDays[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Assert.True(sw.Elapsed.TotalMilliseconds >= 0.0);
            Assert.True(sw.Elapsed.TotalMilliseconds <= 5000.0); // we have problems if this takes more than this long
        }

        // Note: These z-scores were calculated using Windows calculator and the formula at http://www.who.int/childgrowth/standards/Chap_7.pdf. Keep
        // in mind that for WHO z-scores, a z-score > 3 or < -3 uses a different formula, which is also found in the same PDF document. See page 
        // numbered 302.
        
        [Theory]
        [InlineData(0.0, 16.9, Sex.Female, 2.5351948017104595832200737120844)]
        [InlineData(0.4, 16.9, Sex.Female, 2.5351948017104595832200737120844)] // rounds down to 0.0
        [InlineData(0.6, 16.9, Sex.Female, 2.5554397881274597389255441580724)] // rounds up to 1.0
        [InlineData(1.0, 16.9, Sex.Female, 2.5554397881274597389255441580724)]
        [InlineData(1856, 19, Sex.Female, 2.0860332469456686513526025662104)]
        [InlineData(15, 17.8, Sex.Female, 3.0325847159162528421113690449965)]
        [InlineData(15, 9.4, Sex.Female, -3.4017596805071390108663953082457)]
        [InlineData(0, 13.1, Sex.Female, -0.19291951810719225993362490649157)]
        [InlineData(1, 11.3, Sex.Female, -1.7506771691296057225008679776202)]
        [InlineData(0, 16.9, Sex.Male, 2.3383852398458963129137559171807)]
        [InlineData(1, 16.9, Sex.Male, 2.3681904235271979513747283079005)]
        [InlineData(1856, 19, Sex.Male, 2.3679427793494169336926696348487)]
        [InlineData(15, 22.5, Sex.Male, 6.1003421244398328948597199712218)]
        [InlineData(15, 9.4, Sex.Male, -3.6831532463944048076547252044075)]
        [InlineData(0, 13.1, Sex.Male, -0.24308915070120925814059695957408)]
        [InlineData(1, 13.1, Sex.Male, -0.23455795334480738910148700514505)]
        public void ComputeZScore_Bmi_Success(double ageDays, double bmi, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.BMIForAge, ageDays, bmi, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.BMIForAge, ageDays, bmi, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -10)]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -10)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_Bmi_Out_of_Range(Sex sex, int ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.BMIForAge, ageDays, 16.9, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.BMIForAge, ageDays, 16.9, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(45, 3, Sex.Female, 2.1134935536236982591548727107661)]
        [InlineData(110, 16, Sex.Female, -1.4859353052199340839944555784493)]
        [InlineData(49.9, 3.50, Sex.Male, 0.62961642936973316222835417730078)]
        [InlineData(45, 2.4, Sex.Male, -0.18503233427487158999085862877748)]
        [InlineData(110, 16.5, Sex.Male, -1.184325245059326230973893688293)]
        [InlineData(48.11, 3, Sex.Male, 0.12086456422878706425391774812276)]
        [InlineData(48.12, 3, Sex.Male, 0.11423445276668658923783528848242)]
        [InlineData(48.14, 3, Sex.Male, 0.10097930917477510889387908318526)]
        [InlineData(48.15, 3, Sex.Male, 0.09435427263638165932776181841041)]
        [InlineData(48.16, 3, Sex.Male, 0.08773092333637678467749638108213)]
        [InlineData(48.1625, 3, Sex.Male, 0.08607534938498995730954412438918)]
        [InlineData(48.165, 3, Sex.Male, 0.08441988071448012043777759061347)]
        public void ComputeZScore_WeightLength_Success(double length, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.WeightForLength, length, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForLength, length, weight, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 110.0001)]
        [InlineData(Sex.Female, 110.1)]
        [InlineData(Sex.Female, 111.0)]
        [InlineData(Sex.Female, 2000)]
        [InlineData(Sex.Male, 110.0001)]
        [InlineData(Sex.Male, 110.1)]
        [InlineData(Sex.Male, 111.0)]
        [InlineData(Sex.Male, 2000)]
        [InlineData(Sex.Female, 44.999)]
        [InlineData(Sex.Female, 44.5)]
        [InlineData(Sex.Female, 44.0)]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Male, 44.999)]
        [InlineData(Sex.Male, 44.5)]
        [InlineData(Sex.Male, 44.0)]
        [InlineData(Sex.Male, 0)]
        public void ComputeZScore_WeightLength_Out_of_Range(Sex sex, double length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.WeightForLength, length, 16.50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForLength, length, 16.50, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(65, 8.0, Sex.Female, 1.0743800294516307983167802998867)]
        [InlineData(120, 21, Sex.Female, -0.85806737835475871013745232431556)]
        [InlineData(65, 8, Sex.Male, 0.88363276925258927969686943523615)]
        [InlineData(65.365, 7.6, Sex.Male, 0.13072210313950426077703597885603)]
        [InlineData(65.39, 7.6, Sex.Male, 0.12125168507130893864622645927044)]
        [InlineData(120, 21, Sex.Male, -0.66988002648118219306594721792577)]
        public void ComputeZScore_WeightHeight_Success(double height, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.WeightForHeight, height, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForHeight, height, weight, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 64.999)]
        [InlineData(Sex.Female, 64.5)]
        [InlineData(Sex.Female, 64.0)]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 120.0001)]
        [InlineData(Sex.Female, 120.1)]
        [InlineData(Sex.Female, 121.0)]
        [InlineData(Sex.Female, 2000)]
        [InlineData(Sex.Male, 64.999)]
        [InlineData(Sex.Male, 64.5)]
        [InlineData(Sex.Male, 64.0)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 120.0001)]
        [InlineData(Sex.Male, 120.1)]
        [InlineData(Sex.Male, 121.0)]
        [InlineData(Sex.Male, 2000)]
        public void ComputeZScore_WeightHeight_Out_of_Range(Sex sex, double height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.WeightForHeight, height, 8.50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForHeight, height, 8.50, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(91, 13.47, Sex.Female, 0.40589404468054411701689483303183)]
        [InlineData(1856, 16.52, Sex.Female, -0.24320811728720011655502617969338)]
        [InlineData(91, 10.0, Sex.Male, -3.7349383304814340206542860924399)]
        [InlineData(91, 13.47, Sex.Male, -0.0078438450518674409912225165818)]
        [InlineData(91, 18, Sex.Male, 4.14902572751170699082570728562)]
        [InlineData(1856, 16.52, Sex.Male, -0.0213971396073446674965169466961)]
        public void ComputeZScore_ACForAge_Success(int ageDays, double circumference, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.ArmCircumferenceForAge, ageDays, circumference, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.ArmCircumferenceForAge, ageDays, circumference, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 90)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2000)]        
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 90)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2000)]
        public void ComputeZScore_ACForAge_Out_of_Range(Sex sex, int ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.ArmCircumferenceForAge, ageDays, 21.00, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.ArmCircumferenceForAge, ageDays, 21.00, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(0.0, 34, Sex.Female, 0.10241478078755314955626554581229)]
        [InlineData(0.4, 34, Sex.Female, 0.10241478078755314955626554581229)]
        [InlineData(1856, 50, Sex.Female, 0.02445522308245640089097517490151)]
        [InlineData(0, 25, Sex.Male, -7.4486998645416401940428746905989)]
        [InlineData(0, 34, Sex.Male, -0.36354706265671747887389286733164)]
        [InlineData(0, 38.5, Sex.Male, 3.179029338285743878710598044302)]
        [InlineData(0.0, 48.5, Sex.Male, 11.051421340380102451120577847932)]
        [InlineData(0.4, 48.5, Sex.Male, 11.051421340380102451120577847932)]
        [InlineData(1856, 50, Sex.Male, -0.51788656191369656140211656443226)]
        public void ComputeZScore_HCForAge_Success(double ageDays, double circumference, Sex sex, double zExpected)
        {            
            double z = _fixture.WHO2006.CalculateZScore(Indicator.HeadCircumferenceForAge, ageDays, circumference, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.HeadCircumferenceForAge, ageDays, circumference, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, -0.000001)]
        [InlineData(Sex.Female, 1856.00001)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]        
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, -0.000001)]
        [InlineData(Sex.Male, 1856.00001)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_HCForAge_Out_of_Range(Sex sex, double ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.HeadCircumferenceForAge, ageDays, 50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.HeadCircumferenceForAge, ageDays, 50, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(0, 49, Sex.Female, -0.07929359105980168560136240669803)]
        [InlineData(1856, 110, Sex.Female, 0.01352542775910235322705902688034)]        
        [InlineData(0, 49, Sex.Male, -0.46706327321797969208676755180545)]
        [InlineData(0, 56, Sex.Male, 3.23056499247514159790121351881)]
        [InlineData(1856, 110, Sex.Male, -0.10641170700920362584779158876099)]
        public void ComputeZScore_HeightForAge_Success(int ageDays, double height, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.HeightForAge, ageDays, height, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.HeightForAge, ageDays, height, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_HeightForAge_Out_of_Range(Sex sex, int ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            {
                _fixture.WHO2006.CalculateZScore(Indicator.HeightForAge, ageDays, 50, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.HeightForAge, ageDays, 50, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]        
        [InlineData(91, 7.7, Sex.Female, -0.06131747268213446329236853256366)]
        [InlineData(1856, 6.1, Sex.Female, 0.00799538263959151494094926122301)]
        [InlineData(91, 7.6, Sex.Male, -0.0708299062152796216572904695866)]
        [InlineData(1856, 5.3, Sex.Male, -0.05088789162859622106978046123322)]
        public void ComputeZScore_SSFForAge_Success(int ageDays, double size, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.SubscapularSkinfoldForAge, ageDays, size, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.SubscapularSkinfoldForAge, ageDays, size, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 90)]
        [InlineData(Sex.Female, 90.99)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 90)]
        [InlineData(Sex.Male, 90.99)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_SSFForAge_Out_of_Range(Sex sex, double ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            {
                _fixture.WHO2006.CalculateZScore(Indicator.SubscapularSkinfoldForAge, ageDays, 5.3, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.SubscapularSkinfoldForAge, ageDays, 5.3, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(91, 5.0, Sex.Female, -3.5142247600701533771333268236099)]
        [InlineData(91, 9.7, Sex.Female, -0.03125237709867160097513556893842)]
        [InlineData(1856, 8.8, Sex.Female, -0.0230172170416324610038564541254)]
        [InlineData(91, 9.7, Sex.Male, -0.04069912797972332197733952426044)]
        [InlineData(1856, 7.5, Sex.Male, -0.03016340611257267952018295944132)]
        public void ComputeZScore_TSFForAge_Success(int ageDays, double size, Sex sex, double zExpected)
        {
            double z = _fixture.WHO2006.CalculateZScore(Indicator.TricepsSkinfoldForAge, ageDays, size, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.TricepsSkinfoldForAge, ageDays, size, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 90)]
        [InlineData(Sex.Female, 90.99)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 90)]
        [InlineData(Sex.Male, 90.99)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_TSFForAge_Out_of_Range(Sex sex, double ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.TricepsSkinfoldForAge, ageDays, 5.3, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.TricepsSkinfoldForAge, ageDays, 5.3, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(0, 3.3, Sex.Female, 0.14707318200802968181756145307487)]
        [InlineData(1856, 19, Sex.Female, 0.21822735544234463823321703627513)]
        [InlineData(0, 3.4, Sex.Male, 0.10912474665759200634554969904103)]
        [InlineData(0, 5.7, Sex.Male, 4.09500520234497951877138782449)]
        [InlineData(1856, 19, Sex.Male, 0.19724633372363914420000515095729)]
        public void ComputeZScore_WeightForAge_Success(int ageDays, double weight, Sex sex, double zExpected)
        {      
            double z = _fixture.WHO2006.CalculateZScore(Indicator.WeightForAge, ageDays, weight, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForAge, ageDays, weight, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, 1857)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, 1857)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_WeightForAge_Out_of_Range(Sex sex, int ageDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.WHO2006.CalculateZScore(Indicator.WeightForAge, ageDays, 5.3, sex);
            });
            double z = -99;
            Assert.False(_fixture.WHO2006.TryCalculateZScore(Indicator.WeightForAge, ageDays, 5.3, sex, ref z));
            Assert.True(z == -99);
        }
    }
}
