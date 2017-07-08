using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnthStat.Statistics.Tests")]

namespace AnthStat.Statistics
{
    /// <summary>
    /// Class for computing z-scores using the CDC 2000 Growth Charts (ages 0 to 20 years)
    /// </summary>
    /// <remarks>
    /// See https://www.cdc.gov/growthcharts/index.htm
    /// </remarks>
    public sealed partial class CDC2000 : IGrowthReference
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the CDC 2000 Growth Charts
        /// </summary>
        /// <param name="indicator">The indicator to check for validity</param>
        /// <returns>bool; whether or not the provided indicator is valid</return>
        public bool IsValidIndicator(Indicator indicator)
        {
            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
                case Indicator.WeightForAge:
                case Indicator.HeightForAge:
                case Indicator.HeadCircumferenceForAge:
                case Indicator.LengthForAge:
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
        /// Age of the child in months. For example, if the indicator is 'Height-for-Age', then measurement
        /// represents the child's age. If  the indicator is instead 'Weight-for-Length' or 'Weight-for-Height'
        /// then measurement represents the child's length or height (respectively) and must be a non-zero 
        /// value provided in centimeters.
        /// </param>
        /// <returns>bool; whether or not the provided measurements is valid for the given indicator. Also returns false if the indicator is invalid for the CDC 2000 Growth Charts.</return>
        public bool IsValidMeasurement(Indicator indicator, double measurement)
        {
            if (!IsValidIndicator(indicator)) 
            {
                return false;
            }

            double cutoffLower = 0;
            double cutoffUpper = 240.5;

            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
                    cutoffLower = 24;
                    cutoffUpper = 240.5;
                    break;
                case Indicator.HeadCircumferenceForAge:
                    cutoffLower = 0;
                    cutoffUpper = 36;
                    break;
                case Indicator.LengthForAge:
                    cutoffLower = 0;
                    cutoffUpper = 35.5;
                    break;
                case Indicator.HeightForAge:
                    cutoffLower = 24;
                    cutoffUpper = 240.0;
                    break;
                case Indicator.WeightForAge:
                    cutoffLower = 0;
                    cutoffUpper = 240.0;
                    break;
                case Indicator.WeightForHeight:
                    cutoffLower = 77;
                    cutoffUpper = 121.5;
                    break;
                case Indicator.WeightForLength:
                    cutoffLower = 45;
                    cutoffUpper = 103.5;
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
        /// Computes a z-score for the given indicator, age in months (or height/length depending on the indicator), 
        /// measurement value, and gender. A return value indicates whether the computation succeeded or failed.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">
        /// The first measurement value. Must be in metric units. For example, if the indicator is Height-for-Age, 
        /// then measurement1 represents the child's height in centimeters.
        /// </param>
        /// <param name="measurement2">
        /// The second measurement. Typically age of the child in months. For example, if the indicator is 
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
        /// Calculates a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI-for-age, Height-for-Age, Weight-for-Age, etc.)</param>        
        /// <param name="measurement1">
        /// The first measurement value. Must be in metric units. For example, if the indicator is Height-for-Age, 
        /// then measurement1 represents the child's height in centimeters.
        /// </param>
        /// <param name="measurement2">
        /// The second measurement. Typically age of the child in months. For example, if the indicator is 
        /// 'Height-for-Age', then measurement2 represents the child's age. If  the indicator is instead 
        /// 'Weight-for-Length' or 'Weight-for-Height' then measurement2 represents the child's length or 
        /// height (respectively) and must be a non-zero value provided in centimeters. Automatically 
        /// rounded to 5 decimal values.
        /// </param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the calculated z-score for the given inputs</return>
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

            IDictionary<int, Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BodyMassIndexForAge:
                    reference = CDC2000_BMI;
                    break;
                case Indicator.HeadCircumferenceForAge:
                    reference = CDC2000_HeadCircumference;
                    break;
                case Indicator.LengthForAge:
                    reference = CDC2000_LengthForAge;
                    break;
                case Indicator.HeightForAge:
                    reference = CDC2000_HeightForAge;
                    break;
                case Indicator.WeightForAge:
                    reference = CDC2000_WeightForAge;
                    break;
                case Indicator.WeightForLength:
                    reference = CDC2000_WeightForLength;
                    break;
                case Indicator.WeightForHeight:
                    reference = CDC2000_WeightForHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indicator));
            }

            int key = BuildKey(sex, measurement2);
            Lookup lookup = null;
            bool found = reference.TryGetValue(key, out lookup);

            if (found)
            {
                return StatisticsHelper.CalculateZScore(measurement1, lookup.L, lookup.M, lookup.S, false);
            }            
            else 
            {
                var interpolatedValues = InterpolateLMS(sex, measurement2, reference);
                return StatisticsHelper.CalculateZScore(measurement1, interpolatedValues.Item1, interpolatedValues.Item2, interpolatedValues.Item3, false);
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
            if (StatisticsHelper.IsWholeNumber(measurement) || measurement % 0.5 == 0.0)
            {
                int sexKeyPart = sex == Sex.Male ? 1 : 2;
                int measurementKeyPart = (int)(measurement * 100) + sexKeyPart;
                return measurementKeyPart;
            }
            
            return -1;
        }

        /// <summary>
        /// Interpolates the L, M, and S values for a given measurement using the closest neighbors to that measurement. For
        /// example, if the lookup table has LMS entries for 24.5 and 25.5, and the measurement provided is 24.7, then the
        /// lower LMS values will be multiplied by 0.8 and added to the upper LMS values, which will be multiplied by 0.2.
        /// The interpolated LMS values are then returned in a 3-tuple.
        /// </summary>        
        /// <remarks>
        /// The CDC 2000 Growth Chart data points are mostly spaced at 1.0 intervals, e.g. 24.5 to 25.5 to 26.5, etc.
        /// However, a small handful of points are spaced 0.5 apart such as the BMI-for-age data values at 24.0 and
        /// 24.5. Some extra logic is included to find the proper neighbor in this case (since one cannot assume that
        /// the neighbors will always be +/- 1 away) and then to adjust the modifiers applied to the upper and lower
        /// LMS values. This means a 24.1 measurement for age will see the lower LMS values multiplied by 0.8, since
        /// 0.1 is 20% of the distance to 0.5. If the measurement were instead 25.1, then the lower LMS values would
        /// be multiplied by 0.1 since 0.1 is 10% of the distance to 1.0.
        /// </remarks>
        /// <param name="sex">Whether the child is male or female</param>
        /// <param name="measurement">The measurement in metric units</param>
        /// <param name="reference">The lookup table to use to find the closest neighbors to the measurement value</param>
        /// <returns>3-tuple of double representing the interpolated L, M, and S values</return>
        internal Tuple<double, double, double> InterpolateLMS(Sex sex, double measurement, IDictionary<int, Lookup> reference)
        {
            double rounded = Math.Round(measurement);

            double nextUpper = -1;
            double nextLower = -1;

            Lookup lookupUpper = null;
            Lookup lookupLower = null;

            int initialKeyAttempt = BuildKey(sex, rounded);

            if (reference.ContainsKey(initialKeyAttempt))
            {
                if (rounded > measurement)
                {
                    nextUpper = rounded;
                    lookupUpper = reference[initialKeyAttempt];
                }
                else 
                {
                    nextLower = rounded;      
                    lookupLower = reference[initialKeyAttempt];
                }
            }

            var keyUpper = -1;
            var keyLower = -1;

            if (nextUpper == -1) 
            {
                nextUpper = Math.Round(measurement, 0) + 0.5;
                keyUpper = BuildKey(sex, nextUpper);
            }
            if (nextLower == -1) 
            {
                nextLower = Math.Round(measurement, 0) - 0.5;
                keyLower = BuildKey(sex, nextLower);
            }

            if (reference.ContainsKey(keyUpper)) 
            {
                lookupUpper = reference[keyUpper];
                nextUpper = lookupUpper.Measurement;
            }
            if (reference.ContainsKey(keyLower)) 
            {
                lookupLower = reference[keyLower];
                nextLower = lookupUpper.Measurement;
            }

            if (nextUpper == -1) 
            {
                nextUpper = Math.Round(measurement, 0) + 1;
                keyUpper = BuildKey(sex, nextUpper);

                if (reference.ContainsKey(keyUpper)) 
                {
                    lookupUpper = reference[keyUpper];
                    nextUpper = lookupUpper.Measurement;
                }
            }
            if (nextLower == -1) 
            {
                nextLower = Math.Round(measurement, 0) - 1;
                keyLower = BuildKey(sex, nextLower);

                if (reference.ContainsKey(keyLower)) 
                {
                    lookupLower = reference[keyLower];
                    nextLower = lookupUpper.Measurement;
                }
            }
            
            double upperL = lookupUpper.L;
            double upperM = lookupUpper.M;
            double upperS = lookupUpper.S;

            double lowerL = lookupLower.L;
            double lowerM = lookupLower.M;
            double lowerS = lookupLower.S;

            double diff = lookupUpper.Measurement - lookupLower.Measurement;
            double globalModifier = 100 / (100 * diff);

            double lowerModifier = Math.Round((lookupUpper.Measurement - measurement), 5);
            double upperModifier = (diff - lowerModifier);

            lowerModifier *= globalModifier;
            upperModifier *= globalModifier;

            double L = ((lookupLower.L * lowerModifier) + (lookupUpper.L * upperModifier));
            double M = ((lookupLower.M * lowerModifier) + (lookupUpper.M * upperModifier));
            double S = ((lookupLower.S * lowerModifier) + (lookupUpper.S * upperModifier));

            return new Tuple<double, double, double>(L, M, S);
        }
    }
}