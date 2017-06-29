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
    }
}
