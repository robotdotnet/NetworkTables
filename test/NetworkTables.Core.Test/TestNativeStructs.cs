using System;
using NetworkTables.Core.Native;
using NUnit.Framework;
using FRC;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeStructs : TestBase
    {
        [Test]
        public void TestNtStringWriteCreateAndDispose()
        {
            DisposableNativeString write = new DisposableNativeString("TestString");
            write.Dispose();
        }

        [Test]
        public void TestNtStringWriteToString()
        {
            const string testStr = "TestString";
            using (DisposableNativeString write = new DisposableNativeString(testStr))
            {
                Assert.AreEqual(testStr, write.ToString());
            }
        }

        [Test]
        public void TestNtStringReadToString()
        {
            const string testStr = "TestString";
            using (DisposableNativeString write = new DisposableNativeString(testStr))
            {
                NtStringRead read = new NtStringRead(write.Buffer, write.Length);
                Assert.AreEqual(testStr, read.ToString());
                GC.KeepAlive(write);
            }
        }
    }
}
