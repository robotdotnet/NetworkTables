using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeMultipleSets : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [Test]
        public void TestMultipleSetString()
        {
            string key = "MyKey";

            string setVal = "FirstSet";

            CoreMethods.SetEntryString(key, setVal);

            string retVal = CoreMethods.GetEntryString(key);
            Assert.That(retVal, Is.EqualTo(setVal));

            setVal = "SecondSet";

            CoreMethods.SetEntryString(key, setVal);

            retVal = CoreMethods.GetEntryString(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestMultipleSetDouble()
        {
            string key = "MyKey";

            double setVal = 3.58;

            CoreMethods.SetEntryDouble(key, setVal);

            double retVal = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal, Is.EqualTo(setVal));

            setVal = 6.32121;

            CoreMethods.SetEntryDouble(key, setVal);

            retVal = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestMultipleSetBoolean()
        {
            string key = "MyKey";

            bool setVal = true;

            CoreMethods.SetEntryBoolean(key, setVal);

            bool retVal = CoreMethods.GetEntryBoolean(key);
            Assert.That(retVal, Is.EqualTo(setVal));

            setVal = false;

            CoreMethods.SetEntryBoolean(key, setVal);

            retVal = CoreMethods.GetEntryBoolean(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestMultipleRawSet()
        {
            string key = "MyKey";

            byte[] firstWrite = {
                32, 86, 45, 156
            };
            CoreMethods.SetEntryRaw(key, firstWrite);


            byte[] retVal = CoreMethods.GetEntryRaw(key);

            for(int i = 0; i < firstWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(firstWrite[i]));
            }


            byte[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            CoreMethods.SetEntryRaw(key, secondWrite);

            retVal = CoreMethods.GetEntryRaw(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestMultipleBooleanArraySet()
        {
            string key = "MyKey";

            bool[] firstWrite = {
                true, false, true, true
            };
            CoreMethods.SetEntryBooleanArray(key, firstWrite);


            bool[] retVal = CoreMethods.GetEntryBooleanArray(key);

            for (int i = 0; i < firstWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(firstWrite[i]));
            }


            bool[] secondWrite = {
                false, true, false, false, true, true, false
            };
            CoreMethods.SetEntryBooleanArray(key, secondWrite);

            retVal = CoreMethods.GetEntryBooleanArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestMultipleDoubleArraySet()
        {
            string key = "MyKey";

            double[] firstWrite = {
                32, 86, 45, 156
            };
            CoreMethods.SetEntryDoubleArray(key, firstWrite);


            double[] retVal = CoreMethods.GetEntryDoubleArray(key);

            for (int i = 0; i < firstWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(firstWrite[i]));
            }


            double[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            CoreMethods.SetEntryDoubleArray(key, secondWrite);

            retVal = CoreMethods.GetEntryDoubleArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestMultipleStringArraySet()
        {
            string key = "MyKey";

            string[] firstWrite = {
                "32", "86", "45", "156"
            };
            CoreMethods.SetEntryStringArray(key, firstWrite);


            string[] retVal = CoreMethods.GetEntryStringArray(key);

            for (int i = 0; i < firstWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(firstWrite[i]));
            }


            string[] secondWrite = {
                "85", "234", "211", "36", "0", "0", "45"
            };
            CoreMethods.SetEntryStringArray(key, secondWrite);

            retVal = CoreMethods.GetEntryStringArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }
    }
}
