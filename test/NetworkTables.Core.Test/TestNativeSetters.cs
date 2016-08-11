using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeSetters : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [Test]
        public void TestSetStringValid()
        {
            string key = "MyKey";

            string setVal = "FirstSet";

            Assert.That(CoreMethods.SetEntryString(key, setVal));
        }

        [Test]
        public void TestSetDoubleValid()
        {
            string key = "MyKey";

            double setVal = 3.58;

            Assert.That(CoreMethods.SetEntryDouble(key, setVal));
        }

        [Test]
        public void TestSetBooleanValid()
        {
            string key = "MyKey";

            bool setVal = true;

            Assert.That(CoreMethods.SetEntryBoolean(key, setVal));
        }

        [Test]
        public void TestSetRawValid()
        {
            string key = "MyKey";

            byte[] firstWrite = {
                32, 86, 45, 156
            };
            Assert.That(CoreMethods.SetEntryRaw(key, firstWrite));
        }

        [Test]
        public void TestSetBooleanArrayValid()
        {
            string key = "MyKey";

            bool[] firstWrite = {
                true, false, true, true
            };
            Assert.That(CoreMethods.SetEntryBooleanArray(key, firstWrite));
        }

        [Test]
        public void TestSetDoubleArrayValid()
        {
            string key = "MyKey";

            double[] firstWrite = {
                32, 86, 45, 156
            };
            Assert.That(CoreMethods.SetEntryDoubleArray(key, firstWrite));

        }

        [Test]
        public void TestSetStringArrayValid()
        {
            string key = "MyKey";

            string[] firstWrite = {
                "32", "86", "45", "156"
            };
            Assert.That(CoreMethods.SetEntryStringArray(key, firstWrite));
        }


            

        [Test]
        public void TestStringSetInvalid()
        {
            string key = "MyKey";

            string setVal = "FirstSet";

            CoreMethods.SetEntryDouble(key, 3.33);

            setVal = "SecondSet";

            Assert.That(!CoreMethods.SetEntryString(key, setVal));
        }

        [Test]
        public void TestDoubleSetInvalid()
        {
            string key = "MyKey";


            CoreMethods.SetEntryBoolean(key, true);

            double setVal = 6.32121;

            Assert.That(!CoreMethods.SetEntryDouble(key, setVal));
        }

        [Test]
        public void TestBooleanSetInvalid()
        {
            string key = "MyKey";

            CoreMethods.SetEntryDouble(key, 3.33);

            bool setVal = false;

            Assert.That(!CoreMethods.SetEntryBoolean(key, setVal));
        }

        [Test]
        public void TestRawSetInvalid()
        {
            string key = "MyKey";

            CoreMethods.SetEntryDouble(key, 3.33);


            byte[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            Assert.That(!CoreMethods.SetEntryRaw(key, secondWrite));
        }

        [Test]
        public void TestBooleanArraySetInvalid()
        {
            string key = "MyKey";

            CoreMethods.SetEntryDouble(key, 3.33);
            bool[] secondWrite = {
                false, true, false, false, true, true, false
            };
            Assert.That(!CoreMethods.SetEntryBooleanArray(key, secondWrite));
        }

        [Test]
        public void TestDoubleArraySetInvalid()
        {
            string key = "MyKey";

            CoreMethods.SetEntryDouble(key, 3.33);


            double[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            Assert.That(!CoreMethods.SetEntryDoubleArray(key, secondWrite));
        }

        [Test]
        public void TestStringArraySetInvalid()
        {
            string key = "MyKey";

            CoreMethods.SetEntryDouble(key, 3.33);


            string[] secondWrite = {
                "85", "234", "211", "36", "0", "0", "45"
            };
            Assert.That(!CoreMethods.SetEntryStringArray(key, secondWrite));
        }
    }
}
