using System;
using Xunit;
using AnthStat.Statistics;

namespace AnthStat.Statistics.Tests
{
    public sealed class AnthStatFixture
    {
        public WHO2006 WHO2006 { get; set; } = new WHO2006();
        public WHO2007 WHO2007 { get; set; } = new WHO2007();
    }
}