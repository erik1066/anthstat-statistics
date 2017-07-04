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
    ///     See https://www.cdc.gov/growthcharts/index.htm
    /// </remarks>
    public partial class CDC2000
    {
        /// <summary>
        /// Determines whether the provided indicator is valid for the CDC 2000 Growth Charts
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
        /// Determines whether a given pair of measurements (measurement1-for-measurement2, as in "BMI for Age") are valid for a given indicator
        /// </summary>
        /// <param name="indicator">The indicator to which the measurements belong</param>
        /// <param name="measurement">The first measurement; typically age of the child in days. Must be in metric units.</param>
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
                case Indicator.BMIForAge:
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
        /// <param name="measurement1">The first measurement; typically age of the child in months. If not age, must be in metric units and greater than or equal to zero.</param>
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
        /// <param name="measurement1">The first measurement; typically age of the child in months. If not age, must be in metric units. Automatically rounded to 5 decimal values.</param>
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

            IDictionary<int, Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BMIForAge:
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

            int key = BuildKey(sex, measurement1);
            Lookup lookup = null;
            bool found = reference.TryGetValue(key, out lookup);

            if (found)
            {
                return StatHelper.CalculateZScore(measurement2, lookup.L, lookup.M, lookup.S, false);
            }            
            else 
            {
                var interpolatedValues = InterpolateLMS(sex, measurement1, reference);
                return StatHelper.CalculateZScore(measurement2, interpolatedValues.Item1, interpolatedValues.Item2, interpolatedValues.Item3, false);
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
            if (StatHelper.IsWholeNumber(measurement) || measurement % 0.5 == 0.0)
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
        ///     The CDC 2000 Growth Chart data points are mostly spaced at 1.0 intervals, e.g. 24.5 to 25.5 to 26.5, etc.
        ///     However, a small handful of points are spaced 0.5 apart such as the BMI-for-age data values at 24.0 and
        ///     24.5. Some extra logic is included to find the proper neighbor in this case (since one cannot assume that
        ///     the neighbors will always be +/- 1 away) and then to adjust the modifiers applied to the upper and lower
        ///     LMS values. This means a 24.1 measurement for age will see the lower LMS values multiplied by 0.8, since
        ///     0.1 is 20% of the distance to 0.5. If the measurement were instead 25.1, then the lower LMS values would
        ///     be multiplied by 0.1 since 0.1 is 10% of the distance to 1.0.
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
