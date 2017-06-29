using System;
using AnthStat.Statistics;

namespace samples
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWHO2006();
            DoWHO2007();
            TestComputeSpeed();
        }

        private static void DoWHO2006()
        {
            var who2006 = new AnthStat.Statistics.WHO2006();

            double bmi = 16.0;
            double ageDays = 32;

            double z = who2006.ComputeZScore(Indicator.BMIForAge, ageDays, bmi, Sex.Female);
            double p = StatHelper.GetPercentile(z);

            Console.WriteLine($"{ageDays} day old female with BMI = {bmi} has z-score of {z.ToString("N2")} and percentile of {p.ToString("N2")}");
        }

        private static void DoWHO2007()
        {
            var who2007 = new AnthStat.Statistics.WHO2007();

            double bmi = 17.0;
            double ageMonths = 64;

            double z = who2007.ComputeZScore(Indicator.BMIForAge, ageMonths, bmi, Sex.Male);
            double p = StatHelper.GetPercentile(z);

            Console.WriteLine($"{ageMonths} month old male with BMI = {bmi} has z-score of {z.ToString("N2")} and percentile of {p.ToString("N2")}");
        }

        private static void TestComputeSpeed()
        {
            var who2006 = new AnthStat.Statistics.WHO2006();

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
                who2006.ComputeZScore(Indicator.BMIForAge, ageDays[i], bmis[i], sexes[i]);
            }

            sw.Stop();

            Console.WriteLine($"Computed {loopIterations} z-scores in {sw.Elapsed.TotalMilliseconds.ToString("N0")} milliseconds.");
        }
    }
}
