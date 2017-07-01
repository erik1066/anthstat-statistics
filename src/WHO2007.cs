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
        /// Determines whether a given pair of measurements (measurement1-for-measurement2, as in "BMI for Age") are valid for a given indicator
        /// </summary>
        /// <param name="indicator">The indicator to which the measurements belong</param>
        /// <param name="age">The age of the child in months.</param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the growth reference.</return>
        public bool IsValidMeasurement(Indicator indicator, double age)
        {
            if (!IsValidIndicator(indicator)) 
            {
                return false;
            }

            double cutoffLower = 61;
            double cutoffUpper = 228;

            switch (indicator)
            {
                case Indicator.WeightForAge:
                    cutoffUpper = 120;
                    break;
            }

            if (age < cutoffLower || age > cutoffUpper)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Computes a z-score for the given indicator, age in months, measurement value, and gender. A return 
        /// value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="age">The age of the child in months</param>
        /// <param name="measurement">The raw measurement value in metric units</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="z">The computed z-score for the given set of inputs</param>
        /// <returns>bool; whether the computation succeeded or failed</return>
        public bool TryComputeZScore(Indicator indicator, double age, double measurement, Sex sex, ref double z)
        {
            bool success = false;
            if (IsValidMeasurement(indicator, age))
            {
                try 
                {
                    z = ComputeZScore(indicator, age, measurement, sex);
                    success = true;
                }
                catch (Exception)
                {                    
                }
            }
            return success;
        }

        /// <summary>
        /// Gets a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="age">The age of the child in months</param>
        /// <param name="measurement">The raw measurement value in metric units</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double ComputeZScore(Indicator indicator, double age, double measurement, Sex sex)
        {
            if (!IsValidMeasurement(indicator, age))
            {
                throw new ArgumentOutOfRangeException(nameof(age));
            }

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

            double flag = 0;

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
