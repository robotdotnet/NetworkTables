using System;
using Xunit;

namespace FRC.NetworkTables.Core.Test
{
    public class LinkTest
    {
        [Fact]
        public void EnsureNativeLinkTest()
        {
            var inst = NetworkTableInstance.Default;
            inst.Flush();
        }
    }
}
