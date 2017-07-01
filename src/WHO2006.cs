using System;
using System.Collections.Generic;

namespace AnthStat.Statistics
{
    /// <summary>
    /// Class for computing z-scores using the WHO 2006 Child Growth Standards (ages 0 to 5 years)
    /// </summary>
    /// <remarks>
    ///     See http://www.who.int/childgrowth/standards/en/
    /// </remarks>
    public partial class WHO2006
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the WHO 2006 Child Growth Standards
        /// </summary>
        /// <param name="indicator">The indicator to check for validity within the growth standard</param>
        /// <returns>bool; whether or not the provided indicator is valid for this reference</return>
        public bool IsValidIndicator(Indicator indicator)
        {
            switch (indicator)
            {
                case Indicator.BMIForAge:
                case Indicator.WeightForAge:
                case Indicator.HeightForAge:
                case Indicator.ACForAge:
                case Indicator.HCForAge:
                case Indicator.LengthForAge:
                case Indicator.SSFForAge:
                case Indicator.TSFForAge:
                case Indicator.WeightForHeight:
                case Indicator.WeightForLength:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether a given pair of measurements (measurement1-for-measurement2, as in "BMI for Age") are valid for a given indicator
        /// </summary>
        /// <param name="indicator">The indicator to which the measurements belong</param>
        /// <param name="measurement">The first measurement; typically age of the child in days. Must be in metric units.</param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the growth reference.</return>
        public bool IsValidMeasurement(Indicator indicator, double measurement)
        {
            if (!IsValidIndicator(indicator)) 
            {
                return false;
            }

            double cutoffLower = 0;
            double cutoffUpper = 1856;

            switch (indicator)
            {
                case Indicator.SSFForAge:
                case Indicator.TSFForAge:
                case Indicator.ACForAge:
                    cutoffLower = 91;
                    break;
                case Indicator.WeightForHeight:
                    cutoffLower = 65;
                    cutoffUpper = 120;
                    break;
                case Indicator.WeightForLength:
                    cutoffLower = 45;
                    cutoffUpper = 110;
                    break;
            }

            if (measurement < cutoffLower || measurement > cutoffUpper)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">The first measurement; typically age of the child in days. Must be in metric units.</param>
        /// <param name="measurement2">The second measurement value. Must be in metric units.</param>
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
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">The first measurement; typically age of the child in days. Must be in metric units.</param>
        /// <param name="measurement2">The second measurement value. Must be in metric units.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="flag">A flag used for computing whether the z-score falls inside or outside a certain predetermined measurement range</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double ComputeZScore(Indicator indicator, double measurement1, double measurement2, Sex sex, ref double flag)
        {
            if (!IsValidMeasurement(indicator, measurement1))
            {
                throw new ArgumentOutOfRangeException(nameof(measurement1));
            }
            
            List<Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BMIForAge:
                    reference = WHO2006_BMI;
                    break;
                case Indicator.WeightForLength:
                    reference = WHO2006_WeightForLength;
                    break;
                case Indicator.WeightForHeight:
                    reference = WHO2006_WeightForHeight;
                    break;
                case Indicator.WeightForAge:
                    reference = WHO2006_WeightForAge;
                    break;
                case Indicator.ACForAge:
                    reference = WHO2006_ArmCircumference;
                    break;
                case Indicator.HCForAge:
                    reference = WHO2006_HeadCircumference;
                    break;
                case Indicator.HeightForAge:
                case Indicator.LengthForAge:
                    reference = WHO2006_LengthHeightForAge;
                    break;
                case Indicator.SSFForAge:
                    reference = WHO2006_SubscapularSkinfoldForAge;
                    break;
                case Indicator.TSFForAge:
                    reference = WHO2006_TricepsSkinfoldForAge;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indicator));
            }

            var lookupRef = new Lookup(sex, measurement1, 0, 0, 0);
            int index = reference.BinarySearch(lookupRef, new LookupComparer());

            if (index >= 0)
            {
                // found it
                var lookup = reference[index];
                return StatHelper.GetZ(measurement2, lookup.L, lookup.M, lookup.S, ref flag, true);
            }
            else if (index == -1 && (indicator == Indicator.WeightForLength || indicator == Indicator.WeightForHeight))
            {
                // Only Weight-for-Length and Height have decimal values in the lookup table, so interpolate here if needed. The other
                // indicators are age-based and go day-by-day, so it is not worth interpolating between days and we'll just use whole
                // numbers there.
                var interpolatedLMS = StatHelper.InterpolateLMS(measurement2, sex, reference, InterpolationMode.Tenths);
                return StatHelper.GetZ(measurement2, interpolatedLMS.Item1, interpolatedLMS.Item2, interpolatedLMS.Item3, ref flag, true);
            }
            else 
            {
                throw new InvalidOperationException("Value out of range");
            }
        }
    }
}
