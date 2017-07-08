using System;
using AnthStat.Statistics;
using System.Threading.Tasks;

namespace samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var cdc2000 = new CDC2000();
            var who2006 = new WHO2006();
            var who2007 = new WHO2007();

            // Calculates a BMI-for-age z-score using CDC 2000
            double ageMonths = 26; 
            double z = 0.0;
            double bmi = 18.0;

            if (cdc2000.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: bmi, measurement2: ageMonths, sex: Sex.Female, z: ref z))
            {
                double p = StatisticsHelper.CalculatePercentile(z);

                z = Math.Round(z, 2);
                p = Math.Round(p, 2);

                Console.WriteLine($"[CDC 2000] - {ageMonths} month old female with BMI = {bmi} has z-score of {z} and percentile of {p}");                
            }
            else
            {
                Console.WriteLine($"{ageMonths} is a valid age in months for the CDC 2000 BMI-for-age indicator.");
            }

            // Calculates a BMI-for-age z-score using WHO 2006
            double ageDays = 32; 
            bmi = 16;

            if (who2006.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement1: bmi, measurement2: ageDays, sex: Sex.Female, z: ref z))
            {
                double p = StatisticsHelper.CalculatePercentile(z);

                z = Math.Round(z, 2);
                p = Math.Round(p, 2);

                Console.WriteLine($"[WHO 2006] - {ageDays} day old female with BMI = {bmi} has z-score of {z} and percentile of {p}");
            }
            else
            {
                Console.WriteLine($"{ageMonths} is a valid age in days for the WHO 2006 BMI-for-age indicator.");
            }

            // Calculates a BMI-for-age z-score using WHO 2007
            ageMonths = 64; 
            bmi = 17;

            if (who2007.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement: bmi, age: ageMonths, sex: Sex.Female, z: ref z))
            {
                double p = StatisticsHelper.CalculatePercentile(z);

                z = Math.Round(z, 2);
                p = Math.Round(p, 2);

                Console.WriteLine($"[WHO 2007] - {ageMonths} month old male with BMI = {bmi} has z-score of {z} and percentile of {p}");
            }
            else
            {
                Console.WriteLine($"{ageMonths} is a valid age in months for the WHO 2007 BMI-for-age indicator.");
            }

            Console.WriteLine();

            // If interested in performance tests, see below
            TestCDC2000ComputeSpeed(true); // forces test to use interpolation of L, M, and S values (more computationally expensive)
            TestCDC2000ComputeSpeed(false); // forces test to never use interpolation

            Console.WriteLine();

            TestWHO2006ComputeSpeed(); // WHO 2006 standard doesn't typically need interpolation since age is measured in days, and trying to interpolate LMS values between e.g. day 66 and 67 is not worthwhile

            Console.WriteLine();

            TestWHO2007ComputeSpeed(true); // forces test to use interpolation of L, M, and S values (more computationally expensive)
            TestWHO2007ComputeSpeed(false); // forces test to never use interpolation
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
                double z = 0.0;
                bool success = who2007.TryCalculateZScore(indicator: Indicator.BodyMassIndexForAge, measurement: bmis[i], age: ageMonths[i], sex: sexes[i], z: ref z);
            }

            sw.Stop();

            Console.WriteLine($"[WHO 2007] - Computed {loopIterations} z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds [interpolate = {forceInterpolate}]");
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
                double z = 0.0;
                who2006.TryCalculateZScore(Indicator.BodyMassIndexForAge, bmis[i], ageDays[i], sexes[i], ref z);
            }

            sw.Stop();

            Console.WriteLine($"[WHO 2006] - Computed {loopIterations} z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds.");
        }

        private static void TestCDC2000ComputeSpeed(bool forceInterpolate)
        {
            var cdc2000 = new AnthStat.Statistics.CDC2000();

            var sw = new System.Diagnostics.Stopwatch();

            var rnd = new System.Random();
            int loopIterations = 1_000_000;
            int [] index = new int[loopIterations];
            double [] ageDays = new double[loopIterations];
            double [] bmis = new double[loopIterations];
            Sex [] sexes = new Sex[loopIterations];

            for (int i = 0; i < loopIterations; i++)
            {
                index[i] = i;
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
                double z = 0.0;  
                cdc2000.TryCalculateZScore(Indicator.BodyMassIndexForAge, bmis[i], ageDays[i], sexes[i], ref z);
            }

            sw.Stop();

            Console.WriteLine($"[CDC 2000] [Serial]   - Computed {loopIterations} z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds [interpolate = {forceInterpolate}]");

            // Shows how one might batch-process z-scores across several threads using .NET's task parallel library
            sw.Reset();
            sw.Start();
            Parallel.ForEach(index, (i) =>
            {
                double z = 0.0;
                cdc2000.TryCalculateZScore(Indicator.BodyMassIndexForAge, bmis[i], ageDays[i], sexes[i], ref z);
            });
            sw.Stop();

            Console.WriteLine($"[CDC 2000] [Parallel] - Computed {loopIterations} z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds [interpolate = {forceInterpolate}]");
        }
    }
}
