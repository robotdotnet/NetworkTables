using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeForceSet : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [Test]
        public void TestForceSetString()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));

            string setVal = "SecondSet";

            CoreMethods.SetEntryString(key, setVal, true);

            string retVal = CoreMethods.GetEntryString(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestForceSetDouble()
        {
            string key = "MyKey";

            string setVal1 = "FirstSet";

            CoreMethods.SetEntryString(key, setVal1);

            string retVal1 = CoreMethods.GetEntryString(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));

            double setVal = 6.32121;

            CoreMethods.SetEntryDouble(key, setVal, true);

            double retVal = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestForceSetBoolean()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));

            bool setVal = false;

            CoreMethods.SetEntryBoolean(key, setVal, true);

            bool retVal = CoreMethods.GetEntryBoolean(key);
            Assert.That(retVal, Is.EqualTo(setVal));
        }

        [Test]
        public void TestForceRawSet()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));


            byte[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            CoreMethods.SetEntryRaw(key, secondWrite, true);

            byte[] retVal = CoreMethods.GetEntryRaw(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestForceBooleanArraySet()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));


            bool[] secondWrite = {
                false, true, false, false, true, true, false
            };
            CoreMethods.SetEntryBooleanArray(key, secondWrite, true);

            bool[] retVal = CoreMethods.GetEntryBooleanArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestForceDoubleArraySet()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));


            double[] secondWrite = {
                85, 234, 211, 36, 0, 0, 45
            };
            CoreMethods.SetEntryDoubleArray(key, secondWrite, true);

            double[] retVal = CoreMethods.GetEntryDoubleArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }

        [Test]
        public void TestForceStringArraySet()
        {
            string key = "MyKey";

            double setVal1 = 5.58;

            CoreMethods.SetEntryDouble(key, setVal1);

            double retVal1 = CoreMethods.GetEntryDouble(key);
            Assert.That(retVal1, Is.EqualTo(setVal1));


            string[] secondWrite = {
                "85", "234", "211", "36", "0", "0", "45"
            };
            CoreMethods.SetEntryStringArray(key, secondWrite, true);

            string[] retVal = CoreMethods.GetEntryStringArray(key);

            for (int i = 0; i < secondWrite.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(secondWrite[i]));
            }
        }
    }
}
