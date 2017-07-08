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
        [InlineData(Sex.Male, 50, 131, 1, 1)]
        [InlineData(Sex.Male, 50, 1, 201, 1)]
        [InlineData(Sex.Male, 50, 1, 1, 101)]
        [InlineData(Sex.Male, 50, -131, 1, 1)]
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
    }
}
