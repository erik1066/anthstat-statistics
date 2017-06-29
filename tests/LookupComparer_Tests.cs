using System;
using System.Collections.Generic;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public class LookupComparer_Tests
    {
        [Fact]
        public void Compare_Success()
        {
            LookupComparer comparer = new LookupComparer();

            Lookup lookupA = null;
            Lookup lookupB = null;
            Lookup lookupC = new Lookup(Sex.Female, 25, 1, 1, 1);
            Lookup lookupD = new Lookup(Sex.Female, 26, 1, 1, 1);

            Assert.True(comparer.Compare(lookupA, lookupB) == 0);
            Assert.True(comparer.Compare(lookupA, lookupC) == -1);
            Assert.True(comparer.Compare(lookupC, lookupA) == 1);
            Assert.True(comparer.Compare(lookupC, lookupD) == -1);
            Assert.True(comparer.Compare(lookupD, lookupC) == 1);
        }
    }
}