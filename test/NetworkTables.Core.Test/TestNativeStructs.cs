using System;
using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeStructs : TestBase
    {
        [Test]
        public void TestNtStringWriteCreateAndDispose()
        {
            NtStringWrite write = new NtStringWrite("TestString");
            write.Dispose();
        }

        [Test]
        public void TestNtStringWriteToString()
        {
            const string testStr = "TestString";
            using (NtStringWrite write = new NtStringWrite(testStr))
            {
                Assert.AreEqual(testStr, write.ToString());
            }
        }

        [Test]
        public void TestNtStringReadToString()
        {
            const string testStr = "TestString";
            using (NtStringWrite write = new NtStringWrite(testStr))
            {
                NtStringRead read = new NtStringRead(write.str, write.len);
                Assert.AreEqual(testStr, read.ToString());
                GC.KeepAlive(write);
            }
        }
    }
}
