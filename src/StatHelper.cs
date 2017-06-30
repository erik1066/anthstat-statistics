﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnthStat.Statistics.Tests")]

namespace AnthStat.Statistics
{
    /// <summary>
    /// Helper class for computing z-scores and percentiles
    /// </summary>
    public static class StatHelper
    {
        /// <summary>
        /// Determines whether the provided number is a whole number or not
        /// </summary>
        /// <param name="number">The number to check</param>
        /// <returns>bool; whether or not the provided number is whole</return>
        public static bool IsWholeNumber(double number) => Math.Abs(number % 1) <= (Double.Epsilon * 100);

        /// <summary>
        /// Gets a z-score for a given raw value, L, M, and S values
        /// </summary>
        /// <remarks>
        ///     See https://www.cdc.gov/growthcharts/percentile_data_files.htm for LMS to Z equations
        /// </remarks>
        /// <param name="rawValue">The raw value</param>
        /// <param name="L">The L value, also known as the power in the Box-Cox transformation</param>
        /// <param name="M">The median value</param>
        /// <param name="S">The S value, also known as the generalized coefficient of variation. Must be non-zero.</param>
        /// <param name="flag">Flag indicating whether the z-score falls outside a certain pre-defined range</param>
        /// <param name="forceAdjustZ">Whether to recompute the z-score when it's over 3 or less than -3 using a special WHO-derived algorithm.</param>
        /// <returns>double; represents the z-score for the given inputs</return>
        public static double GetZ(double rawValue, double L, double M, double S, ref double flag, bool forceAdjustZ = true)
        {
            #region Input Validation
            if (S == 0)
            {
                throw new ArgumentException(nameof(S));
            }
            #endregion // Input Validation

            double Z = 0;
            
            // L = 0 and L != 0 equations for Z from LMS are from https://www.cdc.gov/growthcharts/percentile_data_files.htm
            // Note that we're not using the L = 0 equation because no L values from the datasets are equal to zero.
            Z = (rawValue / M);
            Z = Math.Pow(Z, L);
            Z = Z - 1;
            Z = Z / (L * S);

            // Re-calculates Z if the z-score is above 3 or below -3, as per http://www.who.int/childgrowth/standards/Chap_7.pdf
            // Note - Do not use (or at least, double-check if should be used) this code path if ever implementing CDC 2000 
            // growth references
            if (forceAdjustZ)
            {
                if (Z > 3)
                {
                    double SD3pos = 0;
                    double SD2pos = 0;
                    double SD23pos = 0;

                    SD3pos = M * Math.Pow((1 + L * S * 3), 1 / L);
                    SD2pos = M * Math.Pow((1 + L * S * 2), 1 / L);
                    SD23pos = SD3pos - SD2pos;

                    Z = 3 + ((rawValue - SD3pos) / SD23pos);
                }
                else if (Z < -3)
                {
                    double SD3neg = 0;
                    double SD2neg = 0;
                    double SD23neg = 0;

                    SD3neg = M * Math.Pow((1 + L * S * (-3)), 1 / L);
                    SD2neg = M * Math.Pow((1 + L * S * (-2)), 1 / L);
                    SD23neg = SD2neg - SD3neg;

                    Z = (-3) + ((rawValue - SD3neg) / SD23neg);
                }
            }

            double stddev = 0;
            if (rawValue < M)
            {
                stddev = M * (1 - Math.Pow((1 - 2 * L * S), (1 / L))) / 2;
            }
            else
            {
                stddev = M * (Math.Pow((1 + (2 * L * S)), (1 / L)) - 1) / 2;
            }

            flag = (rawValue - M) / stddev;

            return Z;
        }
        
        /// <summary>
        /// Gets the percentile for a given z-score
        /// </summary>
        /// <param name="z">The z-score to get a percentile for</param>
        /// <remarks>
        ///     Algorithm AS66 Applied Statistics (1973) vol22 no.3 - Computes P(Z < z). When z=1.96, the values returned are .975 (upper=false) or .025(upper=true) (roughly). 
        /// </remarks>
        /// <returns>double; the percentile that corresponds to the z-score</return>
        public static double GetPercentile(double z)
        {
            double y = 0;
            double alnorm = 0;

            // Algorithm AS66 Applied Statistics (1973) vol22 no.3
            // Computes P(Z<z)

            //When z=1.96, the values returned are .975 (upper=false) or .025(upper=true) (roughly).
            //z = parseFloat(z);            
            bool upper = false;

            double ltone = 7.0;
            double utzero = 18.66;
            double con = 1.28;
            double a1 = 0.398942280444;
            double a2 = 0.399903438504;
            double a3 = 5.75885480458;
            double a4 = 29.8213557808;
            double a5 = 2.62433121679;
            double a6 = 48.6959930692;
            double a7 = 5.92885724438;
            double b1 = 0.398942280385;
            double b2 = 3.8052 * Math.Exp(-8.0);
            double b3 = 1.00000615302;
            double b4 = 3.98064794e-4;
            double b5 = 1.986153813664;
            double b6 = 0.151679116635;
            double b7 = 5.29330324926;
            double b8 = 4.8385912808;
            double b9 = 15.1508972451;
            double b10 = 0.742380924027;
            double b11 = 30.789933034;
            double b12 = 3.99019417011;

            if (z < 0)
            {
                upper = !upper;
                z = -z;
            }
            if (z <= ltone || upper && z <= utzero)
            {
                y = 0.5 * z * z;
                if (z > con)
                {
                    alnorm = b1 * Math.Exp(-y) / (z - b2 + b3 / (z + b4 + b5 / (z - b6 + b7 / (z + b8 - b9 / (z + b10 + b11 / (z + b12))))));
                }
                else
                {
                    alnorm = 0.5 - z * (a1 - a2 * y / (y + a3 - a4 / (y + a5 + a6 / (y + a7))));
                }
            }
            else
            {
                alnorm = 0;
            }
            if (!upper) alnorm = 1 - alnorm;

            // After all that math is done, take our decimal and return it to the calling function
            return alnorm * 100;
        } 

        internal static Tuple<double, double, double> InterpolateLMS(double measurement, Sex sex, List<Lookup> reference, InterpolationMode mode = InterpolationMode.Ones)
        {
            double measurementLower = (int)measurement;
            double measurementUpper = measurementLower + 1.0;

            if (mode == InterpolationMode.Tenths)
            {
                measurementLower = (int)(measurement * 10);
                measurementLower = measurementLower / 10;

                measurementUpper = (int)((measurementLower + 0.1) * 10);
                measurementUpper = measurementUpper / 10;
            }

            var lookupLowerRef = new Lookup(sex, measurementLower, 0, 0, 0);
            int indexLower = reference.BinarySearch(lookupLowerRef);

            var lookupUpperRef = new Lookup(sex, measurementUpper, 0, 0, 0);
            int indexUpper = reference.BinarySearch(lookupUpperRef);

            if (indexLower >= 0 && indexUpper >= 0)
            {
                // found it
                var lowerLookup = reference[indexLower];
                var upperLookup = reference[indexUpper];

                double lowerModifier = Math.Round((measurementUpper - measurement), 4);

                if (mode == InterpolationMode.Tenths)
                {
                    lowerModifier *= 10;
                }

                double upperModifier = 1 - lowerModifier;

                double L = ((lowerLookup.L * lowerModifier) + (upperLookup.L * upperModifier));
                double M = ((lowerLookup.M * lowerModifier) + (upperLookup.M * upperModifier));
                double S = ((lowerLookup.S * lowerModifier) + (upperLookup.S * upperModifier));

                return new Tuple<double, double, double>(L, M, S);
            }
            else 
            {
                throw new InvalidOperationException("Value out of range");
            }
        }
    }
}
