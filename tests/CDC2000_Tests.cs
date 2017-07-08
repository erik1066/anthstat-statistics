using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [Theory]
        [InlineData(24.1)]
        [InlineData(24.01)]
        [InlineData(24.001)]
        [InlineData(24.0001)]
        [InlineData(24.00001)]
        [InlineData(24.000001)]
        [InlineData(24.2)]
        [InlineData(24.22)]
        [InlineData(24.222)]
        [InlineData(24.2222)]
        [InlineData(24.2001)]
        [InlineData(24.02)]
        [InlineData(24.002)]
        [InlineData(24.0002)]
        [InlineData(24.3)]
        [InlineData(24.03)]
        [InlineData(24.003)]
        [InlineData(24.0003)]
        [InlineData(24.4)]
        [InlineData(24.04)]
        [InlineData(24.004)]
        [InlineData(24.0004)]        
        [InlineData(24.49)]
        [InlineData(24.499)]
        [InlineData(24.4999)]
        [InlineData(24.49999)]
        [InlineData(24.51)]
        [InlineData(24.501)]
        [InlineData(24.5001)]
        [InlineData(24.50001)]
        [InlineData(24.500001)]
        [InlineData(24.05)]
        [InlineData(24.005)]
        [InlineData(24.0005)]
        public void BuildKey_Invalid(double measurement)
        {
            var result = _fixture.CDC2000.BuildKey(Sex.Male, measurement);
            Assert.True(result == -1);
        }

        [Theory]
        [InlineData(24.5)]
        [InlineData(24)]
        [InlineData(25.5)]
        [InlineData(240.5)]
        [InlineData(25.5)]
        [InlineData(0.5)]
        [InlineData(0)]
        public void BuildKey_Valid(double measurement)
        {
            var result = _fixture.CDC2000.BuildKey(Sex.Male, measurement);
            Assert.True(result == (int)(measurement * 100) + 1);
        }

        [Fact]
        public void InterpolateLMS_HighPrecision_Success()
        {
            Dictionary<int, Lookup> reference = new Dictionary<int, Lookup>();

            reference.Add(2401, new Lookup(Sex.Male, 24.0, 24.0, 18.5057, 0.12988));
            reference.Add(3401, new Lookup(Sex.Male, 34.0, 34.0, 18.5057, 0.12988));

            int upperLimit = 6351;

            for(int i = 2451; i <= upperLimit; i = i + 100 )
            {
                int index = (i / 100);
                double value = (double)index + 0.5;
                reference.Add(i, new Lookup(Sex.Male, value, value, 18.5057, 0.12988));
            }

            Parallel.ForEach(reference, (kvp) =>
            {            
                double startValue = kvp.Value.L + 0.00001;
                double endValue = kvp.Value.L + 1;
                int keyValue = kvp.Key;

                if (keyValue >= upperLimit || startValue >= upperLimit) return;

                for(double i = startValue; i < endValue; i = i + 0.00001)
                {
                    var result = _fixture.CDC2000.InterpolateLMS(Sex.Male, i, reference);
                    Assert.True(Math.Abs(result.Item1 - i) < TOLERANCE);
                }            
            });
        }

        [Fact]
        public void IsValidIndicator_Success()
        {
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.BodyMassIndexForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.HeightForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.HeadCircumferenceForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.LengthForAge));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForHeight));
            Assert.True(_fixture.CDC2000.IsValidIndicator(Indicator.WeightForLength));
            Assert.False(_fixture.CDC2000.IsValidIndicator(Indicator.SubscapularSkinfoldForAge));
            Assert.False(_fixture.CDC2000.IsValidIndicator(Indicator.TricepsSkinfoldForAge));
            Assert.False(_fixture.CDC2000.IsValidIndicator(Indicator.ArmCircumferenceForAge));
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
                _fixture.CDC2000.CalculateZScore(Indicator.BodyMassIndexForAge, bmis[i], ageMonths[i], sexes[i]);
            }

            sw.Stop();

            Assert.True(sw.Elapsed.TotalMilliseconds >= 0.0);
            Assert.True(sw.Elapsed.TotalMilliseconds <= 5000.0); // we have problems if this takes more than this long
        }

        [Theory]
        [InlineData(24, 15.25, Sex.Female, -0.89998579698103833912656059986439)]
        [InlineData(25, 15.0, Sex.Female, -1.0693184563449251569543525091431)]
        [InlineData(240.5, 21.5, Sex.Female, -0.06782127757198954073396495854781)]
        [InlineData(24.25, 15.25, Sex.Female, -0.88927079421466268379009544748404)]
        [InlineData(240.125, 22, Sex.Female, 0.08296951497400838075264190214788)]
        [InlineData(24, 15.5, Sex.Male, -0.89076945049192844341949062350922)]
        [InlineData(24.1, 16, Sex.Male, -0.45013562284768823413221156006599)]
        [InlineData(24.4, 16, Sex.Male, -0.43838189952771375580587585820972)]
        [InlineData(25, 15.0, Sex.Male, -1.334079758137899497294611102369)]
        [InlineData(238.6, 22.3, Sex.Male, -0.22348600382594950704977156123708)]
        [InlineData(239.6, 22.3, Sex.Male, -0.23771370527759415957509210527469)]
        [InlineData(240.1, 22.3, Sex.Male, -0.24473565954120004641979066474253)]
        [InlineData(240.5, 22.3, Sex.Male, -0.25031381193856607306216062869678)]
        [InlineData(240.5, 28, Sex.Male, 1.2157897308560679273003157713821)]
        [InlineData(240.5, 35, Sex.Male, 2.1641348570977560688006974622955)]
        [InlineData(240.5, 55, Sex.Male, 3.2177147927155639378143012025565)]
        [InlineData(240.5, 16, Sex.Male, -3.861917414824408430553929083166)]
        [InlineData(240.125, 22, Sex.Male, -0.35319017913424296280251821932759)]
        public void ComputeZScore_Bmi_Success(double ageMonths, double bmi, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: bmi, measurement2: ageMonths, sex: sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: bmi, measurement2: ageMonths, sex: sex, z: ref z));
        }

        [Theory]
        [InlineData(Sex.Female, 23.5)]
        [InlineData(Sex.Female, 23.99999)]
        [InlineData(Sex.Female, 23.9999999999)]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, -23.999999)]
        [InlineData(Sex.Female, 240.511111)]
        [InlineData(Sex.Female, 240.6)]
        [InlineData(Sex.Female, 241)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, 23.5)]
        [InlineData(Sex.Male, 23.99999)]
        [InlineData(Sex.Male, 23.9999999999)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, -23.999999)]
        [InlineData(Sex.Male, 240.511111)]
        [InlineData(Sex.Male, 240.6)]
        [InlineData(Sex.Male, 241)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_Bmi_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: 16.9, measurement2: ageMonths, sex: sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: 16.9, measurement2: ageMonths, sex: sex, z: ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(0, 36, Sex.Male, 0.10061611037407461443909017517158)]
        [InlineData(0.1, 36, Sex.Male, -0.04832587062310737115261418476264)]
        [InlineData(0.6, 36, Sex.Male, -0.75264879218625302211897557084067)]
        [InlineData(1.1, 37, Sex.Male, -0.79421284965903478383762100285665)]
        [InlineData(36, 50, Sex.Male, 0.19600185277546031314716123032251)]
        [InlineData(0, 35, Sex.Female, 0.17548045698599557709310304242613)]
        [InlineData(36, 49, Sex.Female, 0.23713112594777027575544725157751)]
        public void ComputeZScore_HeadCircumference_Success(double ageMonths, double circumference, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(indicator: Indicator.HeadCircumferenceForAge, measurement1: circumference, measurement2: ageMonths, sex: sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(indicator: Indicator.HeadCircumferenceForAge, measurement1: circumference, measurement2: ageMonths, sex: sex, z: ref z));
        }

        [Theory]
        [InlineData(42, Sex.Female, -0.00001)]
        [InlineData(42, Sex.Female, -1)]
        [InlineData(42, Sex.Female, -23.999999)]
        [InlineData(42, Sex.Female, 36.00001)]
        [InlineData(42, Sex.Female, 36.5)]
        [InlineData(42, Sex.Female, 220)]
        [InlineData(42, Sex.Female, 2500)]
        [InlineData(42, Sex.Male, -0.00001)]
        [InlineData(42, Sex.Male, -1)]
        [InlineData(42, Sex.Male, -23.999999)]
        [InlineData(42, Sex.Male, 36.00001)]
        [InlineData(42, Sex.Male, 36.5)]
        [InlineData(42, Sex.Male, 220)]
        [InlineData(-1, Sex.Male, 12)]
        [InlineData(42, Sex.Male, 2500)]
        public void ComputeZScore_HeadCircumference_Out_of_Range(double circumference, Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(indicator: Indicator.HeadCircumferenceForAge, measurement1: circumference, measurement2: ageMonths, sex: sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(indicator: Indicator.HeadCircumferenceForAge, measurement1: circumference, measurement2: ageMonths, sex: sex, z: ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(0, 50, Sex.Male, 0.00418688126503143130789745649004)]
        [InlineData(35.5, 96, Sex.Male, 0.14213978751143433671016268208901)]
        [InlineData(0, 50, Sex.Female, 0.28435009141031476854216759291299)]
        [InlineData(35.5, 96, Sex.Female, 0.39378003327433646896479980436778)]
        public void ComputeZScore_LengthForAge_Success(double ageMonths, double length, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(indicator: Indicator.LengthForAge, measurement1: length, measurement2: ageMonths, sex: sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(indicator: Indicator.LengthForAge, measurement1: length, measurement2: ageMonths, sex: sex, z: ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -23.999999)]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, -0.00001)]
        [InlineData(Sex.Female, 35.50001)]
        [InlineData(Sex.Female, 35.6)]
        [InlineData(Sex.Female, 36)]
        [InlineData(Sex.Female, 36.5)]
        [InlineData(Sex.Female, 220)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -23.999999)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, -0.00001)]
        [InlineData(Sex.Male, 35.50001)]
        [InlineData(Sex.Male, 35.6)]
        [InlineData(Sex.Male, 36)]
        [InlineData(Sex.Male, 36.5)]
        [InlineData(Sex.Male, 220)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_LengthForAge_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(Indicator.LengthForAge, 42, ageMonths, sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(Indicator.LengthForAge, 42, ageMonths, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(24, 87, Sex.Male, 0.15711870954014771387774711449819)]
        [InlineData(26.5, 88, Sex.Male, -0.11763477124587994993286765639376)]
        [InlineData(26.75, 88, Sex.Male, -0.16950317211628110809536732531402)]
        [InlineData(240, 177, Sex.Male, 0.02111943090198769197653633371963)]
        [InlineData(24, 87, Sex.Female, 0.58454144574638665069122147068065)]
        [InlineData(26.5, 88, Sex.Female, 0.23388839999180375781062920504457)]
        [InlineData(26.75, 88, Sex.Female, 0.17510156835524552001703906594492)]
        [InlineData(240, 177, Sex.Female, 2.119514173096064192005892294081)]
        public void ComputeZScore_HeightForAge_Success(double ageMonths, double height, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(Indicator.HeightForAge, height, ageMonths, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(Indicator.HeightForAge, height, ageMonths, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -23.999999)]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, -0.00001)]
        [InlineData(Sex.Female, 0)]
        [InlineData(Sex.Female, 23.5)]
        [InlineData(Sex.Female, 23.9)]
        [InlineData(Sex.Female, 23.9999)]
        [InlineData(Sex.Female, 240.00001)]
        [InlineData(Sex.Female, 240.1)]
        [InlineData(Sex.Female, 240.5)]
        [InlineData(Sex.Female, 241)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -23.999999)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, -0.00001)]
        [InlineData(Sex.Male, 0)]
        [InlineData(Sex.Male, 23.5)]
        [InlineData(Sex.Male, 23.9)]
        [InlineData(Sex.Male, 23.9999)]
        [InlineData(Sex.Male, 240.00001)]
        [InlineData(Sex.Male, 240.1)]
        [InlineData(Sex.Male, 240.5)]
        [InlineData(Sex.Male, 241)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_HeightForAge_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(Indicator.HeightForAge, 95, ageMonths, sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(Indicator.HeightForAge, 95, ageMonths, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(24, 13, Sex.Male, 0.23661706727665810477527237764342)]
        [InlineData(24, 13, Sex.Male, 0.23661706727665810477527237764342)]
        [InlineData(24.5, 13, Sex.Male, 0.18525167636500161271583406887912)]
        [InlineData(24.75, 13, Sex.Male, 0.1600392493847863320735770266397)]
        [InlineData(238.51, 71, Sex.Male, 0.04761783330795486144276862550047)]
        [InlineData(238.6125, 71, Sex.Male, 0.04673396210224925642557030752107)]
        [InlineData(238.6, 71, Sex.Male, 0.04684172654702198520713840860624)]
        [InlineData(240, 71, Sex.Male, 0.03510563254505139845943837691245)]        
        [InlineData(24, 13, Sex.Female, 0.68353013727719231811600292602077)]
        [InlineData(24.5, 13, Sex.Female, 0.62314237148042863909343316257226)]
        [InlineData(24.75, 13, Sex.Female, 0.59357460401418089521395547246476)]
        [InlineData(240, 71, Sex.Female, 1.0287507199035549922743068005002)]        
        public void ComputeZScore_WeightForAge_Success(double ageMonths, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(Indicator.WeightForAge, weight, ageMonths, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForAge, weight, ageMonths, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -23.999999)]
        [InlineData(Sex.Female, -1)]
        [InlineData(Sex.Female, -0.00001)]
        [InlineData(Sex.Female, 240.00001)]
        [InlineData(Sex.Female, 240.1)]
        [InlineData(Sex.Female, 240.5)]
        [InlineData(Sex.Female, 241)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -23.999999)]
        [InlineData(Sex.Male, -1)]
        [InlineData(Sex.Male, -0.00001)]
        [InlineData(Sex.Male, 240.00001)]
        [InlineData(Sex.Male, 240.1)]
        [InlineData(Sex.Male, 240.5)]
        [InlineData(Sex.Male, 241)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_WeightForAge_Out_of_Range(Sex sex, double ageMonths)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(Indicator.WeightForAge, 45, ageMonths, sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForAge, 45, ageMonths, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(45, 2.3, Sex.Male, 0.03000312167769061660016810310108)]
        [InlineData(103.5, 17, Sex.Male, 0.41316797424842034104217535427253)]
        [InlineData(45, 2.3, Sex.Female, -0.01386007738002724285466516799032)]
        [InlineData(103.5, 17, Sex.Female, 0.52186208516798431258478098458335)]
        public void ComputeZScore_WeightForLength_Success(double length, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(Indicator.WeightForLength, weight, length, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForLength, weight, length, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -14.5)]
        [InlineData(Sex.Female, -44.99999)]
        [InlineData(Sex.Female, 103.500001)]
        [InlineData(Sex.Female, 104)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -14.5)]
        [InlineData(Sex.Male, -44.99999)]
        [InlineData(Sex.Male, 103.500001)]
        [InlineData(Sex.Male, 104)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_WeightForLength_Out_of_Range(Sex sex, double length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(Indicator.WeightForLength, 13, length, sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForLength, 13, length, sex, ref z));
            Assert.True(z == -99);
        }

        [Theory]
        [InlineData(77, 10.3, Sex.Male, 0.03222330872952046657980460530646)]
        [InlineData(121.5, 22.8, Sex.Male, 0.00752883854055974186429771683978)]
        [InlineData(77, 10.3, Sex.Female, 0.253740944296596305102408225582)]
        [InlineData(121.5, 22.8, Sex.Female, -0.06282781130115447381457454626206)]
        public void ComputeZScore_WeightForHeight_Success(double height, double weight, Sex sex, double zExpected)
        {
            double z = _fixture.CDC2000.CalculateZScore(Indicator.WeightForHeight, weight, height, sex);
            Assert.True(Math.Abs(z - zExpected) < TOLERANCE);
            Assert.True(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForHeight, weight, height, sex, ref z));
        }

        [Theory]
        [InlineData(Sex.Female, -14.5)]
        [InlineData(Sex.Female, -76.99999)]
        [InlineData(Sex.Female, 121.500001)]
        [InlineData(Sex.Female, 122)]
        [InlineData(Sex.Female, 2500)]
        [InlineData(Sex.Male, -14.5)]
        [InlineData(Sex.Male, -76.99999)]
        [InlineData(Sex.Male, 121.500001)]
        [InlineData(Sex.Male, 122)]
        [InlineData(Sex.Male, 2500)]
        public void ComputeZScore_WeightForHeight_Out_of_Range(Sex sex, double height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                _fixture.CDC2000.CalculateZScore(Indicator.WeightForHeight, 18, height, sex);
            });
            double z = -99;
            Assert.False(_fixture.CDC2000.TryCalculateZScore(Indicator.WeightForHeight, 18, height, sex, ref z));
            Assert.True(z == -99);
        }
    }
}