using System;
using AnthStat.Statistics;

namespace samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // This is how you check to see if inputs are out-of-range before calling the method that computes the z-score
            double ageMonths = 17; 
            bool isValid = CheckMeasurementValidityCDC2000(ageMonths, Indicator.BMIForAge);
            if (!isValid)
            {
                Console.WriteLine($"{ageMonths} is not a valid age in months for the CDC 2000 BMI-for-age indicator.");
            }
            else 
            {
                Console.WriteLine($"{ageMonths} is a valid age in months for the CDC 2000 BMI-for-age indicator.");
            }

            Console.WriteLine();

            // Look at these methods for examples of how to generate z-scores and percentiles for a given set of inputs
            GetBodyMassIndexCDC2000();
            GetBodyMassIndexWHO2006();
            GetBodyMassIndexWHO2007();

            Console.WriteLine();

            // If interested in performance tests, see below
            TestCDC2000ComputeSpeed(true); // forces test to use interpolation of L, M, and S values (more computationally expensive)
            TestCDC2000ComputeSpeed(false); // forces test to never use interpolation

            TestWHO2006ComputeSpeed(); // WHO 2006 standard doesn't typically need interpolation since age is measured in days, and trying to interpolate LMS values between e.g. day 66 and 67 is not worthwhile

            TestWHO2007ComputeSpeed(true); // forces test to use interpolation of L, M, and S values (more computationally expensive)
            TestWHO2007ComputeSpeed(false); // forces test to never use interpolation
        }

        private static bool CheckMeasurementValidityCDC2000(double ageMonths, Indicator indicator)
        {
            var cdc2000 = new AnthStat.Statistics.CDC2000();
            bool isValid = cdc2000.IsValidMeasurement(indicator, ageMonths);
            return isValid;
        }

        private static void GetBodyMassIndexCDC2000()
        {
            var cdc2000 = new AnthStat.Statistics.CDC2000();

            double bmi = 16.0;
            double ageMonths = 32;

            double z = cdc2000.ComputeZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Female);
            double p = StatHelper.GetPercentile(z);

            Console.WriteLine($"[CDC 2000] - {ageMonths} month old female with BMI = {bmi} has z-score of {z.ToString("N2")} and percentile of {p.ToString("N2")}");
        }

        private static void GetBodyMassIndexWHO2006()
        {
            var who2006 = new AnthStat.Statistics.WHO2006();

            double bmi = 16.0;
            double ageDays = 32;

            double z = who2006.ComputeZScore(Indicator.BMIForAge, ageDays, bmi, Sex.Female);
            double p = StatHelper.GetPercentile(z);

            Console.WriteLine($"[WHO 2006] - {ageDays} day old female with BMI = {bmi} has z-score of {z.ToString("N2")} and percentile of {p.ToString("N2")}");
        }

        private static void GetBodyMassIndexWHO2007()
        {
            var who2007 = new AnthStat.Statistics.WHO2007();

            double bmi = 17.0;
            double ageMonths = 64;

            double z = who2007.ComputeZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Male);
            double p = StatHelper.GetPercentile(z);

            Console.WriteLine($"[WHO 2007] - {ageMonths} month old male with BMI = {bmi} has z-score of {z.ToString("N2")} and percentile of {p.ToString("N2")}");
        }

        private static void TestWHO2007ComputeSpeed(bool forceInterpolate)
        {
            var who2007 = new AnthStat.Statistics.WHO2007();

            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 1_000_000;

            double [] ageMonths = new double[loopIterations];
            double [] bmis = new double[loopIterations];
            Sex [] sexes = new Sex[loopIterations];

            for (int i = 0; i < loopIterations; i++)
            {
                ageMonths[i] = rnd.Next(61, 228);

                if (forceInterpolate)
                {
                    ageMonths[i] += 0.25;
                }
                bmis[i] = rnd.NextDouble() * 25;
                sexes[i] = ageMonths[i] % 2 == 0 ? Sex.Female : Sex.Male;
            }

            sw.Start();

            for (int i = 0; i < loopIterations; i++)
            {                
                who2007.ComputeZScore(Indicator.BMIForAge, ageMonths[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Console.WriteLine($"Computed {loopIterations} WHO 2007 z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds [interpolate = {forceInterpolate}]");
        }

        private static void TestWHO2006ComputeSpeed()
        {
            var who2006 = new AnthStat.Statistics.WHO2006();

            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 1_000_000;

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
                who2006.ComputeZScore(Indicator.BMIForAge, ageDays[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Console.WriteLine($"Computed {loopIterations} WHO 2006 z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds.");
        }

        private static void TestCDC2000ComputeSpeed(bool forceInterpolate)
        {
            var cdc2000 = new AnthStat.Statistics.CDC2000();

            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 1_000_000;

            double [] ageDays = new double[loopIterations];
            double [] bmis = new double[loopIterations];
            Sex [] sexes = new Sex[loopIterations];

            for (int i = 0; i < loopIterations; i++)
            {
                ageDays[i] = rnd.Next(24, 240);

                if (!forceInterpolate)
                {
                    ageDays[i] += 0.5;
                }

                bmis[i] = rnd.NextDouble() * 25;
                sexes[i] = ageDays[i] % 2 == 0 ? Sex.Female : Sex.Male;
            }

            sw.Start();

            for (int i = 0; i < loopIterations; i++)
            {                
                cdc2000.ComputeZScore(Indicator.BMIForAge, ageDays[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Console.WriteLine($"Computed {loopIterations} CDC 2000 z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds [interpolate = {forceInterpolate}]");
        }
    }
}
