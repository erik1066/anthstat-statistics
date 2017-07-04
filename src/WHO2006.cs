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
                case Indicator.ArmCircumferenceForAge:
                case Indicator.HeadCircumferenceForAge:
                case Indicator.LengthForAge:
                case Indicator.SubscapularSkinfoldForAge:
                case Indicator.TricepsSkinfoldForAge:
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
                case Indicator.SubscapularSkinfoldForAge:
                case Indicator.TricepsSkinfoldForAge:
                case Indicator.ArmCircumferenceForAge:
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
        /// Computes a z-score for the given indicator, age in days (or height/length depending on the indicator), 
        /// measurement value, and gender. A return value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">The first measurement; typically age of the child in days. Must be in metric units and greater than or equal to zero.</param>
        /// <param name="measurement2">The second measurement value. Must be in metric units and must be greater than or equal to zero.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="z">The computed z-score for the given set of inputs</param>
        /// <returns>bool; whether the computation succeeded or failed</return>
        public bool TryCalculateZScore(Indicator indicator, double measurement1, double measurement2, Sex sex, ref double z)
        {
            bool success = false;
            if (IsValidMeasurement(indicator, measurement1) && measurement2 >= 0)
            {
                try 
                {
                    z = CalculateZScore(indicator, measurement1, measurement2, sex);
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
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">The first measurement; typically age of the child in days. Must be in metric units. Automatically rounded to 5 decimal values.</param>
        /// <param name="measurement2">The second measurement value. Must be in metric units.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double CalculateZScore(Indicator indicator, double measurement1, double measurement2, Sex sex)
        {
            if (measurement2 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(measurement2));
            }
            if (!IsValidMeasurement(indicator, measurement1))
            {
                throw new ArgumentOutOfRangeException(nameof(measurement1));
            }

            measurement1 = Math.Round(measurement1, 5);
            
            List<Lookup> reference = null;
            bool shouldRound = true;

            switch (indicator)
            {
                case Indicator.BMIForAge:
                    reference = WHO2006_BMI;
                    break;
                case Indicator.WeightForLength:
                    reference = WHO2006_WeightForLength;
                    shouldRound = false;
                    break;
                case Indicator.WeightForHeight:
                    reference = WHO2006_WeightForHeight;
                    shouldRound = false;
                    break;
                case Indicator.WeightForAge:
                    reference = WHO2006_WeightForAge;
                    break;
                case Indicator.ArmCircumferenceForAge:
                    reference = WHO2006_ArmCircumference;
                    break;
                case Indicator.HeadCircumferenceForAge:
                    reference = WHO2006_HeadCircumference;
                    break;
                case Indicator.HeightForAge:
                case Indicator.LengthForAge:
                    reference = WHO2006_LengthHeightForAge;
                    break;
                case Indicator.SubscapularSkinfoldForAge:
                    reference = WHO2006_SubscapularSkinfoldForAge;
                    break;
                case Indicator.TricepsSkinfoldForAge:
                    reference = WHO2006_TricepsSkinfoldForAge;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indicator));
            }

            if (shouldRound && !StatHelper.IsWholeNumber(measurement1))
            {
                measurement1 = Math.Round(measurement1, 0);
            }

            var lookupRef = new Lookup(sex, measurement1, 0, 0, 0);
            int index = reference.BinarySearch(lookupRef, new LookupComparer());

            if (index >= 0)
            {
                // found it
                var lookup = reference[index];
                return StatHelper.CalculateZScore(measurement2, lookup.L, lookup.M, lookup.S, true);
            }
            else if (index <= -1 && (indicator == Indicator.WeightForLength || indicator == Indicator.WeightForHeight))
            {
                // Only Weight-for-Length and Height have decimal values in the lookup table, so interpolate here if needed. The other
                // indicators are age-based and go day-by-day, so it is not worth interpolating between days and we'll just use whole
                // numbers there.
                var interpolatedLMS = StatHelper.InterpolateLMS(measurement1, sex, reference, InterpolationMode.Tenths);
                return StatHelper.CalculateZScore(measurement2, interpolatedLMS.Item1, interpolatedLMS.Item2, interpolatedLMS.Item3, true);
            }
            else 
            {
                throw new InvalidOperationException($"Could not find a lookup match for value {Math.Round(measurement1, 2).ToString("N2")}");
            }
        }
    }
}
