using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeGettersDefaultParameter : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [Test]
        public void TestGetStringNonExistant()
        {
            string key = "mykey";
            string testVal = "Hello Test";
            Assert.That(CoreMethods.GetEntryString(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetBooleanNonExistant()
        {
            string key = "mykey";
            bool testVal = true;
            Assert.That(CoreMethods.GetEntryBoolean(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetDoubleNonExistant()
        {
            string key = "mykey";
            double testVal = 5.86;
            Assert.That(CoreMethods.GetEntryDouble(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetRawNonExistant()
        {
            string key = "mykey";
            byte[] testVal = { 56, 88, 92, 0, 0, 1 };
            Assert.That(CoreMethods.GetEntryRaw(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetDoubleArrayNonExistant()
        {
            string key = "mykey";
            double[] testVal = { 3.58, 6.825, 454.54 };
            Assert.That(CoreMethods.GetEntryDoubleArray(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetStringArrayNonExistant()
        {
            string key = "mykey";
            string[] testVal = { "write1", "write2", "write3" };
            Assert.That(CoreMethods.GetEntryStringArray(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetBooleanArrayNonExistant()
        {
            string key = "mykey";
            bool[] testVal = { true, false, true };
            Assert.That(CoreMethods.GetEntryBooleanArray(key, testVal), Is.EqualTo(testVal));
        }



        [Test]
        public void TestGetStringWrongType()
        {
            string key = "mykey";
            string testVal = "Hello Test";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryString(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetBooleanWrongType()
        {
            string key = "mykey";
            bool testVal = true;
            CoreMethods.SetEntryDouble(key, 5.86);

            Assert.That(CoreMethods.GetEntryBoolean(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetDoubleWrongType()
        {
            string key = "mykey";
            double testVal = 5.86;
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryDouble(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetRawWrongType()
        {
            string key = "mykey";
            byte[] testVal = { 56, 88, 92, 0, 0, 1 };
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryRaw(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetDoubleArrayWrongType()
        {
            string key = "mykey";
            double[] testVal = { 3.58, 6.825, 454.54 };
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryDoubleArray(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetStringArrayWrongType()
        {
            string key = "mykey";
            string[] testVal = { "write1", "write2", "write3" };
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryStringArray(key, testVal), Is.EqualTo(testVal));
        }

        [Test]
        public void TestGetBooleanArrayWrongType()
        {
            string key = "mykey";
            bool[] testVal = { true, false, true };
            CoreMethods.SetEntryBoolean(key, true);

            Assert.That(CoreMethods.GetEntryBooleanArray(key, testVal), Is.EqualTo(testVal));
        }




        [Test]
        public void TestGetStringValid()
        {
            string key = "mykey";
            string testVal = "Hello Test";
            string actualVal = "Im Valid";
            CoreMethods.SetEntryString(key, actualVal);

            Assert.That(CoreMethods.GetEntryString(key, testVal), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetBooleanValid()
        {
            string key = "mykey";
            bool testVal = true;
            bool actualVal = false;
            CoreMethods.SetEntryBoolean(key, actualVal);

            Assert.That(CoreMethods.GetEntryBoolean(key, testVal), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetDoubleValid()
        {
            string key = "mykey";
            double testVal = 5.86;
            double actualVal = 1213.323;
            CoreMethods.SetEntryDouble(key, actualVal);

            Assert.That(CoreMethods.GetEntryDouble(key, testVal), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetRawValid()
        {
            string key = "mykey";
            byte[] testVal = { 56, 88, 92, 0, 0, 1 };

            byte[] actualVal = { 42, 28, 142, 0, 22, 0, 132 };
            CoreMethods.SetEntryRaw(key, actualVal);

            byte[] retVal = CoreMethods.GetEntryRaw(key, testVal);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for(int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }

        [Test]
        public void TestGetDoubleArrayValid()
        {
            string key = "mykey";
            double[] testVal = { 3.58, 6.825, 454.54 };

            double[] actualVal = { 3.54538, 43536.825, 34534454.34554, 3423413.23432 };
            CoreMethods.SetEntryDoubleArray(key, actualVal);

            double[] retVal = CoreMethods.GetEntryDoubleArray(key, testVal);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for (int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }

        [Test]
        public void TestGetStringArrayValid()
        {
            string key = "mykey";
            string[] testVal = { "write1", "write2", "write3" };

            string[] actualVal = { "writeas1", "wriasdte2", "writasde3", "adsadsd4" };
            CoreMethods.SetEntryStringArray(key, actualVal);

            string[] retVal = CoreMethods.GetEntryStringArray(key, testVal);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for (int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }

        [Test]
        public void TestGetBooleanArrayValid()
        {
            string key = "mykey";
            bool[] testVal = { true, false, true };

            bool[] actualVal = { false, false, true, true, false };
            CoreMethods.SetEntryBooleanArray(key, actualVal);

            bool[] retVal = CoreMethods.GetEntryBooleanArray(key, testVal);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for (int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }

    }
}
