using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnthStat.Statistics.Tests")]

namespace AnthStat.Statistics
{
    /// <summary>
    /// Class for computing z-scores using the WHO 2007 Growth Reference
    /// </summary>
    /// <remarks>
    /// See http://www.who.int/growthref/en/
    /// </remarks>
    public sealed partial class WHO2007 : IGrowthReference
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the WHO 2007 Growth Reference
        /// </summary>
        /// <param name="indicator">The indicator to check for validity</param>
        /// <returns>bool; whether or not the provided indicator is valid for the WHO 2007 Growth Reference</return>
        public bool IsValidIndicator(Indicator indicator)
        {
            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
                case Indicator.WeightForAge:
                case Indicator.HeightForAge:
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
        /// <param name="age">The age of the child in months.</param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the WHO 2007 Growth Reference.</return>
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
        /// Calculates a z-score for a given indicator, pair of measurements (measurement1-for-measurement2, as 
        /// in "BMI-for-Age"), and gender. A return value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="measurement">
        /// The first measurement value. Must be in metric units and must be greater than or equal to zero. For 
        /// example, if the indicator is 'Height-for-Age', then measurement represents the child's height in 
        /// centimeters. If using 'Weight-for-Age', then the measurement represents the child's weight in
        /// kilograms.
        /// </param>
        /// <param name="age">The age of the child in months. Must be greater than or equal to 61. Automatically rounded to 5 decimal values.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="z">The z-score for the given set of inputs</param>
        /// <returns>bool; whether the calculation succeeded or failed</return>
        public bool TryCalculateZScore(Indicator indicator, double measurement, double age, Sex sex, ref double z)
        {
            bool success = false;
            if (IsValidMeasurement(indicator, age) && measurement >= 0)
            {
                try 
                {
                    z = CalculateZScore(indicator: indicator, measurement: measurement, age: age, sex: sex);
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
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age)</param>
        /// <param name="measurement">
        /// The first measurement value. Must be in metric units and must be greater than or equal to zero. For 
        /// example, if the indicator is 'Height-for-Age', then measurement represents the child's height in 
        /// centimeters. If using 'Weight-for-Age', then the measurement represents the child's weight in
        /// kilograms.
        /// </param>
        /// <param name="age">The age of the child in months. Must be greater than or equal to 61. Automatically rounded to 5 decimal values.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        internal double CalculateZScore(Indicator indicator, double measurement, double age, Sex sex)
        {
            if (measurement < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(measurement));
            }
            if (!IsValidMeasurement(indicator, age))
            {
                throw new ArgumentOutOfRangeException(nameof(age));
            }

            age = Math.Round(age, 5);

            Dictionary<int, Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
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

            int key = BuildKey(sex, age);
            Lookup lookup = null;
            bool found = reference.TryGetValue(key, out lookup);

            if (found)
            {
                return StatisticsHelper.CalculateZScore(measurement, lookup.L, lookup.M, lookup.S, true);
            }
            else 
            {
                var interpolatedValues = InterpolateLMS(sex, age, reference);
                return StatisticsHelper.CalculateZScore(measurement, interpolatedValues.Item1, interpolatedValues.Item2, interpolatedValues.Item3, true);
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
            if (StatisticsHelper.IsWholeNumber(measurement))
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
            double ageLower = (double)(int)ageMonths;
            double ageUpper = ageLower + 1.0;

            var lookupLowerKey = BuildKey(sex, ageLower);
            var lookupUpperKey = BuildKey(sex, ageUpper);
            
            var lookupLower = reference[lookupLowerKey];
            var lookupUpper = reference[lookupUpperKey];      

            double lowerModifier = Math.Round((ageUpper - ageMonths), 5);

            double upperModifier = 1 - lowerModifier;

            double L = ((lookupLower.L * lowerModifier) + (lookupUpper.L * upperModifier));
            double M = ((lookupLower.M * lowerModifier) + (lookupUpper.M * upperModifier));
            double S = ((lookupLower.S * lowerModifier) + (lookupUpper.S * upperModifier));

            return new Tuple<double, double, double>(L, M, S);
        }
    }
}
