using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnthStat.Statistics.Tests")]

namespace AnthStat.Statistics
{
    /// <summary>
    /// Represents a lookup table entry.
    /// </summary>
    /// <remarks>
    ///     Lookup tables are used to pull the appropriate L, M, and S values for a 
    ///     given measurement (the measurement is typically age in either days or 
    ///     months, but may also be height or length in centimeters) and sex (either
    ///     male or female). This class represents a row in such a table.
    /// </remarks>
    internal sealed class Lookup : IComparable<Lookup>
    {
        /// <summary>
        /// Gets whether the lookup represents a male or female
        /// </summary>
        public Sex Sex { get; }

        /// <summary>
        /// Gets the raw measurement value in metric units
        /// </summary>
        public double Measurement { get; }

        /// <summary>
        /// Gets the L value, also known as the power in the Box-Cox transformation
        /// </summary>
        public double L { get; }

        /// <summary>
        /// Gets the median value
        /// </summary>
        public double M { get; }

        /// <summary>
        /// Gets the S value, also known as the generalized coefficient of variation. Must be non-zero.
        /// </summary>
        public double S { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sex">Whether male or female</param>
        /// <param name="measurement">The raw measurement value in metric units</param>
        /// <param name="l">The L value, also known as the power in the Box-Cox transformation</param>
        /// <param name="m">The median value</param>
        /// <param name="s">The S value, also known as the generalized coefficient of variation. Must be non-zero.</param>
        public Lookup(Sex sex, double measurement, double l, double m, double s)
        {
            #region Input Validation
            if (measurement < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(measurement));
            }
            if (Math.Abs(l) > 130)
            {
                throw new ArgumentOutOfRangeException(nameof(l));
            }
            if (Math.Abs(m) > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(m));
            }
            if (Math.Abs(s) > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(s));
            }
            #endregion // Input Validation

            Sex = sex;
            Measurement = measurement;
            L = l;
            M = m;
            S = s;
        }

        public override bool Equals(object obj)
        {
            Lookup comparedItem = obj as Lookup;

            if (comparedItem == null) return false;

            return (comparedItem.Sex == this.Sex && comparedItem.Measurement == this.Measurement);
        }

        public override int GetHashCode()
        {
            return Sex == Sex.Male ? (int)(this.Measurement * 10000) : (int)(this.Measurement * 10000 * 10);
        }

        public int CompareTo(Lookup that)
        {
            if (this.Sex == that.Sex)
            {
                if (this.Measurement > that.Measurement) return 1;
                if (this.Measurement == that.Measurement) return 0;
                return -1;
            }
            else if (this.Sex == Sex.Male && that.Sex == Sex.Female)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}