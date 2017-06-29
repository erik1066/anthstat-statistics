using System;
using System.Collections.Generic;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class Lookup_Tests
    {

        [Fact]
        public void Lookup_Construct_Success()
        {
            var sex = Sex.Male;
            var measurement = 25.55;
            var L = 1;
            var M = 25.1;
            var S = 0.098;

            var lookup = new Lookup(sex, measurement, L, M, S);

            Assert.True(lookup.L == L);
            Assert.True(lookup.M == M);
            Assert.True(lookup.S == S);
            Assert.True(lookup.Sex == sex);
            Assert.True(lookup.Measurement == measurement);
        }

        [Theory]
        [InlineData(Sex.Male, 50, 101, 1, 1)]
        [InlineData(Sex.Male, 50, 1, 201, 1)]
        [InlineData(Sex.Male, 50, 1, 1, 101)]
        [InlineData(Sex.Male, 50, -101, 1, 1)]
        [InlineData(Sex.Male, 50, 1, -201, 1)]
        [InlineData(Sex.Male, 50, 1, 1, -101)]
        [InlineData(Sex.Male, -1, 1, 1, 1)]
        public void Lookup_Construct_Fail_Input_Out_Of_Range(Sex sex, double measurement, double L, double M, double S)
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate 
            { 
                var lookup = new Lookup(sex, measurement, L, M, S);
            });
        }

        [Theory]
        [InlineData(Sex.Male, 25.5, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 25.51, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 25.5001, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 25.49, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 25.4999, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 0, 1, 25.0, 0.09)]
        public void Lookup_NotEquals(Sex sex, double measurement, double L, double M, double S)
        {
            var lookupA = new Lookup(Sex.Female, 25.5, 1, 25.0, 0.09);
            var lookupB = new Lookup(sex, measurement, L, M, S);

            Assert.False(lookupA.Equals(lookupB));
            Assert.False(lookupB.Equals(lookupA));
            Assert.False(lookupA.GetHashCode() == lookupB.GetHashCode());
            Assert.False(lookupA.CompareTo(lookupB) == 0);
        }

        [Theory]
        [InlineData(Sex.Female, 25.5, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 25.5, 5, 5, 5)]
        [InlineData(Sex.Female, 25.5, 0, 0, 0)]
        [InlineData(Sex.Female, 25.5, -0.405, 41.4, 0.098)]
        public void Lookup_Equals(Sex sex, double measurement, double L, double M, double S)
        {
            var lookupA = new Lookup(Sex.Female, 25.5, 1, 25.0, 0.09);
            var lookupB = new Lookup(sex, measurement, L, M, S);

            Assert.True(lookupA.Equals(lookupB));
            Assert.True(lookupB.Equals(lookupA));
            Assert.True(lookupA.GetHashCode() == lookupB.GetHashCode());
            Assert.True(lookupA.CompareTo(lookupB) == 0);
        }

        [Fact]
        public void Lookup_NotEquals_Null()
        {
            var lookupA = new Lookup(Sex.Female, 25.5, 1, 25.0, 0.09);

            Assert.False(lookupA.Equals(null));
        }

        [Theory]
        [InlineData(Sex.Male, 26.01, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 26, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 26, 0, 0, 0)]
        [InlineData(Sex.Male, 26, 50, 50, 50)]
        [InlineData(Sex.Male, 27, 5, 5, 5)]
        [InlineData(Sex.Male, 1000, 0, 0, 0)]
        public void CompareTo_Greater_Male_Male(Sex sex, double measurement, double L, double M, double S)
        {
            var lookupA = new Lookup(Sex.Male, 25, 1, 25.0, 0.09);
            var lookupB = new Lookup(sex, measurement, L, M, S);

            Assert.True(lookupB.CompareTo(lookupA) == 1);
            Assert.True(lookupA.CompareTo(lookupB) == -1);
        }

        [Theory]
        [InlineData(Sex.Female, 26.01, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 26, 1, 25.0, 0.09)]
        [InlineData(Sex.Female, 26, 0, 0, 0)]
        [InlineData(Sex.Female, 26, 50, 50, 50)]
        [InlineData(Sex.Female, 27, 5, 5, 5)]
        [InlineData(Sex.Female, 1000, 0, 0, 0)]
        public void CompareTo_Greater_Female_Female(Sex sex, double measurement, double L, double M, double S)
        {
            var lookupA = new Lookup(Sex.Female, 25, 1, 25.0, 0.09);
            var lookupB = new Lookup(sex, measurement, L, M, S);

            Assert.True(lookupB.CompareTo(lookupA) == 1);
            Assert.True(lookupA.CompareTo(lookupB) == -1);
        }

        [Theory]
        [InlineData(Sex.Male, 0, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 24.99, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 24.99, 10, 27.0, 0.1)]
        [InlineData(Sex.Male, 25.00, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 25.01, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 25.01, 10, 27.0, 0.1)]
        [InlineData(Sex.Male, 27, 1, 25.0, 0.09)]
        [InlineData(Sex.Male, 1000, 1, 25.0, 0.09)]
        public void CompareTo_Greater_Female_Male(Sex sex, double measurement, double L, double M, double S)
        {
            var lookupA = new Lookup(Sex.Female, 25, 1, 25.0, 0.09);
            var lookupB = new Lookup(sex, measurement, L, M, S);

            Assert.True(lookupB.CompareTo(lookupA) == -1);
            Assert.True(lookupA.CompareTo(lookupB) == 1);
        }
    }
}
