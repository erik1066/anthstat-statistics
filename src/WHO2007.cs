using System;
using System.Collections.Generic;

namespace AnthStat.Statistics
{
    /// <summary>
    /// Class for computing z-scores using the WHO 2007 Growth Reference
    /// </summary>
    /// <remarks>
    ///     See http://www.who.int/growthref/en/
    /// </remarks>
    public partial class WHO2007
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the WHO 2007 Growth Reference
        /// </summary>
        /// <param name="indicator">The indicator to check for validity within the growth reference</param>
        /// <returns>bool; whether or not the provided indicator is valid for this reference</return>
        public bool IsValidIndicator(Indicator indicator)
        {
            switch (indicator)
            {
                case Indicator.BMIForAge:
                case Indicator.WeightForAge:
                case Indicator.HeightForAge:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="age">The age of the child in months</param>
        /// <param name="measurement">The raw measurement value in metric units</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double ComputeZScore(Indicator indicator, double measurement1, double measurement2, Sex sex)
        {
            double flag = 0;
            return ComputeZScore(indicator, measurement1, measurement2, sex, ref flag);
        }

        /// <summary>
        /// Gets a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="age">The age of the child in months</param>
        /// <param name="measurement">The raw measurement value in metric units</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="flag">A flag used for computing whether the z-score falls inside or outside a certain predetermined measurement range</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double ComputeZScore(Indicator indicator, double age, double measurement, Sex sex, ref double flag)
        {
            List<Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BMIForAge:
                    reference = WHO2007_BMI;
                    break;
                case Indicator.WeightForAge:
                    reference = WHO2007_WeightAge;
                    break;
                case Indicator.HeightForAge:
                    reference = WHO2007_HeightAge;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indicator));
            }

            if (StatHelper.IsWholeNumber(age))
            {
                // If it's a whole number, then the value should be found in the list
                var lookupRef = new Lookup(sex, age, 0, 0, 0);
                int index = reference.BinarySearch(lookupRef);

                if (index >= 0)
                {
                    // found it
                    var lookup = reference[index];
                    return StatHelper.GetZ(measurement, lookup.L, lookup.M, lookup.S, ref flag, true);
                }
                else 
                {
                    throw new InvalidOperationException("Value out of range");
                }
            }
            else 
            {
                var interpolatedLMS = StatHelper.InterpolateLMS(age, sex, reference);
                return StatHelper.GetZ(measurement, interpolatedLMS.Item1, interpolatedLMS.Item2, interpolatedLMS.Item3, ref flag, true);
            }
        }
    }
}
