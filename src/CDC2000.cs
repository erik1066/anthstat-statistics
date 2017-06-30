using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
                case Indicator.HCForAge:
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
                case Indicator.HCForAge:
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
        /// Gets a z-score for the given indicator, age in months, measurement value, and gender.
        /// </summary>
        /// <param name="indicator">The indicator to use for computing the z-score (e.g. BMI, Height-for-Age, Weight-for-Age, etc.)</param>
        /// <param name="measurement1">The first measurement; typically age of the child in months. If not age, must be in metric units.</param>
        /// <param name="measurement2">The second measurement value. Must be in metric units.</param>
        /// <param name="sex">Whether the child is male or female</param>
        /// <returns>double; the z-score for the given inputs</return>
        public double ComputeZScore(Indicator indicator, double measurement1, double measurement2, Sex sex)
        {
            if (!IsValidMeasurement(indicator, measurement1))
            {
                throw new ArgumentOutOfRangeException(nameof(measurement1));
            }

            IDictionary<string, Lookup> reference = null;

            switch (indicator)
            {
                case Indicator.BMIForAge:
                    reference = CDC2000_BMI;
                    break;
                case Indicator.HCForAge:
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

            var flag = 0.0;

            string key = BuildKey(sex, measurement1);

            Lookup lookup = null;

            bool found = reference.TryGetValue(key, out lookup);

            if (found)
            {
                return StatHelper.GetZ(measurement2, lookup.L, lookup.M, lookup.S, ref flag);
            }            
            else 
            {
                var interpolatedValues = InterpolateLMS(sex, measurement1, reference);
                return StatHelper.GetZ(measurement2, interpolatedValues.Item1, interpolatedValues.Item2, interpolatedValues.Item3, ref flag);
            }
        }

        private string BuildKey(Sex sex, double measurement)
        {
            string sexStr = sex == Sex.Male ? "Male" : "Female";
            return sexStr + "-" + measurement.ToString();
        }

        private Tuple<double, double, double> InterpolateLMS(Sex sex, double measurement, IDictionary<string, Lookup> reference)
        {
            double rounded = Math.Round(measurement);

            double nextUpper = -1;
            double nextLower = -1;

            Lookup lookupUpper = null;
            Lookup lookupLower = null;

            string initialKeyAttempt = BuildKey(sex, rounded);

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

            if (nextUpper == -1) nextUpper = Math.Round(measurement, 0) + 0.5;
            if (nextLower == -1) nextLower = Math.Round(measurement, 0) - 0.5;

            var keyUpper = BuildKey(sex, nextUpper);
            var keyLower = BuildKey(sex, nextLower);

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

            double lowerModifier = Math.Round((lookupUpper.Measurement - measurement), 4);
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
