using System;
using System.Collections.Generic;

namespace AnthStat.Statistics
{
    /// <summary>
    /// Class for computing z-scores using the WHO 2006 Child Growth Standards (ages 0 to 5 years)
    /// </summary>
    /// <remarks>
    /// See http://www.who.int/childgrowth/standards/en/
    /// </remarks>
    public sealed partial class WHO2006 : IGrowthReference
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the WHO 2006 Child Growth Standards
        /// </summary>
        /// <param name="indicator">The indicator to check for validity</param>
        /// <returns>bool; whether or not the provided indicator is valid for the WHO 2006 Growth Standards</return>
        public bool IsValidIndicator(Indicator indicator)
        {
            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
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
        /// Determines whether the second measurement (in the form of measurement1-for-measurement2, as 
        /// in "Height-for-Age") is valid for a given indicator.
        /// </summary>
        /// <param name="indicator">The indicator to which the measurements belong</param>
        /// <param name="measurement">
        /// Typically age of the child in days. For example, if the indicator is 
        /// 'Height-for-Age', then measurement2 represents the child's age. If  the indicator is instead 
        /// 'Weight-for-Length' or 'Weight-for-Height' then measurement2 represents the child's length or 
        /// height (respectively) and must be a non-zero value provided in centimeters.
        /// </param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the WHO 2006 Growth Standards.</return>
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
        /// Calculates a z-score for a given indicator, pair of measurements (measurement1-for-measurement2, as 
        /// in "BMI-for-Age"), and gender. A return value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">
        /// The first measurement value. Must be in metric units and must be greater than or equal to zero. For 
        /// example, if the indicator is 'Height-for-Age', then measurement1 represents the child's height in 
        /// centimeters. Note that subscapular skinfold and triceps skinfold require measurement1 be provided
        /// in millimeters.
        /// </param>
        /// <param name="measurement2">
        /// The second measurement. Typically age of the child in days. For example, if the indicator is 
        /// 'Height-for-Age', then measurement2 represents the child's age. If  the indicator is instead 
        /// 'Weight-for-Length' or 'Weight-for-Height' then measurement2 represents the child's length or 
        /// height (respectively) and must be a non-zero value provided in centimeters. Automatically 
        /// rounded to 5 decimal values.
        /// </param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="z">The z-score for the given set of inputs</param>
        /// <returns>bool; whether the calculation succeeded or failed</return>
        public bool TryCalculateZScore(Indicator indicator, double measurement1, double measurement2, Sex sex, ref double z)
        {
            bool success = false;
            if (IsValidMeasurement(indicator, measurement2) && measurement1 >= 0)
            {
                try 
                {
                    z = CalculateZScore(indicator: indicator, measurement1: measurement1, measurement2: measurement2, sex: sex);
                    success = true;
                }
                catch (Exception)
                {                    
                }
            }
            return success;
        }

        /// <summary>
        /// Calculates a z-score for a given indicator, pair of measurements (measurement1-for-measurement2, as 
        /// in "BMI-for-Age"), and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">
        /// The first measurement value. Must be in metric units and must be greater than or equal to zero. For 
        /// example, if the indicator is 'Height-for-Age', then measurement1 represents the child's height in 
        /// centimeters. Note that subscapular skinfold and triceps skinfold require measurement1 be provided
        /// in millimeters.
        /// </param>
        /// <param name="measurement2">
        /// The second measurement. Typically age of the child in days. For example, if the indicator is 
        /// 'Height-for-Age', then measurement2 represents the child's age. If the indicator is instead 
        /// 'Weight-for-Length' or 'Weight-for-Height' then measurement2 represents the child's length or 
        /// height (respectively) and must be a non-zero value provided in centimeters. Automatically 
        /// rounded to 5 decimal values if measuring height or length and automatically rounded to a whole 
        /// number if measuring age in days.
        /// </param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        internal double CalculateZScore(Indicator indicator, double measurement1, double measurement2, Sex sex)
        {
            if (measurement1 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(measurement1));
            }
            if (!IsValidMeasurement(indicator, measurement2))
            {
                throw new ArgumentOutOfRangeException(nameof(measurement2));
            }

            measurement2 = Math.Round(measurement2, 5);
            
            Dictionary<int, Lookup> reference = null;
            bool shouldRound = true;

            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
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

            if (shouldRound && !StatisticsHelper.IsWholeNumber(measurement2))
            {
                measurement2 = Math.Round(measurement2, 0);
            }

            int key = BuildKey(sex, measurement2);
            Lookup lookup = null;
            bool found = reference.TryGetValue(key, out lookup);

            if (found)
            {
                return StatisticsHelper.CalculateZScore(measurement1, lookup.L, lookup.M, lookup.S, true);
            }
            else if (indicator == Indicator.WeightForLength || indicator == Indicator.WeightForHeight)
            {
                var interpolatedLMS = InterpolateLMS(sex, measurement2, reference); 
                return StatisticsHelper.CalculateZScore(measurement1, interpolatedLMS.Item1, interpolatedLMS.Item2, interpolatedLMS.Item3, true);
            }
            else
            {
                throw new InvalidOperationException($"Could not find a lookup match for value {Math.Round(measurement2, 2).ToString("N2")}");
            }
        }

        /// <summary>
        /// Builds a dictionary key using a provided sex and measurement value.
        /// </summary>
        /// <param name="measurement">The measurement in metric units</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>int; represents the integer dictionary key for the given sex and measurement values</returns>
        internal int BuildKey(Sex sex, double measurement)
        {
            if (StatisticsHelper.IsWholeNumber(measurement) || Math.Round(measurement, 1) == measurement)
            {
                int sexKeyPart = sex == Sex.Male ? 1 : 2;
                int measurementKeyPart = (int)(measurement * 100) + sexKeyPart;
                return measurementKeyPart;
            }
            
            return -1;
        }

        /// <summary>
        /// Interpolates the L, M, and S values for a given measurement using the closest neighbors to that measurement. For
        /// example, if the lookup table has LMS entries for 24.0 and 25.0, and the measurement provided is 24.2, then the
        /// lower LMS values will be multiplied by 0.8 and added to the upper LMS values, which will be multiplied by 0.2.
        /// The interpolated LMS values are then returned in a 3-tuple.
        /// </summary>        
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="measurement">The measurement in metric units</param>
        /// <param name="reference">The lookup table to use to find the closest neighbors to the measurement value</param>
        /// <returns>3-tuple of double representing the interpolated L, M, and S values</return>
        internal Tuple<double, double, double> InterpolateLMS(Sex sex, double ageMonths, IDictionary<int, Lookup> reference)
        {
            double ageLower = 0.0;
            double ageUpper = 0.0;

            bool tenths = false;

            if (reference == WHO2006_WeightForHeight || reference == WHO2006_WeightForLength)
            {
                double rounded = Math.Round(ageMonths, 1);                

                if (rounded > ageMonths)
                {
                    ageUpper = rounded;
                    ageLower = Math.Round(ageUpper - 0.1, 1);
                }
                else
                {
                    ageLower = rounded;
                    ageUpper = Math.Round(ageLower + 0.1, 1);
                }

                tenths = true;
            }
            else 
            {
                ageLower = (double)(int)ageMonths;
                ageUpper = ageLower + 1.0;
            }

            var lookupLowerKey = BuildKey(sex, ageLower);
            var lookupUpperKey = BuildKey(sex, ageUpper);
            
            var lookupLower = reference[lookupLowerKey];
            var lookupUpper = reference[lookupUpperKey];      

            double lowerModifier = Math.Round((ageUpper - ageMonths), 5);

            if (tenths)
            {
                lowerModifier *= 10;
            }

            double upperModifier = 1 - lowerModifier;

            double L = ((lookupLower.L * lowerModifier) + (lookupUpper.L * upperModifier));
            double M = ((lookupLower.M * lowerModifier) + (lookupUpper.M * upperModifier));
            double S = ((lookupLower.S * lowerModifier) + (lookupUpper.S * upperModifier));

            return new Tuple<double, double, double>(L, M, S);
        }
    }
}
