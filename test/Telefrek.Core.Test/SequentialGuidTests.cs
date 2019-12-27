using System;
using System.Collections.Generic;
using Xunit;

namespace Telefrek.Core.Test
{
    public class SequentialGuidTests
    {
        [Fact]
        public void GuidsAreUnique()
        {
            var g1 = SequentialGuid.NextGuid();
            var g2 = SequentialGuid.NextGuid();

            Assert.True(g1 != g2);
            Assert.True(g1.ToString() != g2.ToString());
        }

        [Fact]
        public void GuidsPastCounterAreUnique()
        {
            var hashSet = new HashSet<String>();

            for (var i = 0; i < 100000; ++i)
                Assert.True(hashSet.Add(SequentialGuid.NextGuid().ToString()));
        }
    }
}
