using NetworkTables.Core.Native;
using NetworkTables.Exceptions;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeGettersNoDefaultParameter : TestBase
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
            Assert.Throws<TableKeyNotDefinedException>(() => CoreMethods.GetEntryString(key));
        }

        [Test]
        public void TestGetBooleanNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryBoolean(key));
        }

        [Test]
        public void TestGetDoubleNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryDouble(key));
        }

        [Test]
        public void TestGetRawNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryRaw(key));
        }

        [Test]
        public void TestGetDoubleArrayNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryDoubleArray(key));
        }

        [Test]
        public void TestGetStringArrayNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryStringArray(key));
        }

        [Test]
        public void TestGetBooleanArrayNonExistant()
        {
            string key = "mykey";
            Assert.Throws<TableKeyNotDefinedException>(() =>CoreMethods.GetEntryBooleanArray(key));
        }



        [Test]
        public void TestGetStringWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryString(key));
        }

        [Test]
        public void TestGetBooleanWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryDouble(key, 5.86);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryBoolean(key));
        }

        [Test]
        public void TestGetDoubleWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryDouble(key));
        }

        [Test]
        public void TestGetRawWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryRaw(key));
        }

        [Test]
        public void TestGetDoubleArrayWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryDoubleArray(key));
        }

        [Test]
        public void TestGetStringArrayWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() =>CoreMethods.GetEntryStringArray(key));
        }

        [Test]
        public void TestGetBooleanArrayWrongType()
        {
            string key = "mykey";
            CoreMethods.SetEntryBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => CoreMethods.GetEntryBooleanArray(key));
        }




        [Test]
        public void TestGetStringValid()
        {
            string key = "mykey";
            string actualVal = "Im Valid";
            CoreMethods.SetEntryString(key, actualVal);

            Assert.That(CoreMethods.GetEntryString(key), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetBooleanValid()
        {
            string key = "mykey";
            bool actualVal = false;
            CoreMethods.SetEntryBoolean(key, actualVal);

            Assert.That(CoreMethods.GetEntryBoolean(key), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetDoubleValid()
        {
            string key = "mykey";
            double actualVal = 1213.323;
            CoreMethods.SetEntryDouble(key, actualVal);

            Assert.That(CoreMethods.GetEntryDouble(key), Is.EqualTo(actualVal));
        }

        [Test]
        public void TestGetRawValid()
        {
            string key = "mykey";

            byte[] actualVal = { 42, 28, 142, 0, 22, 0, 132 };
            CoreMethods.SetEntryRaw(key, actualVal);

            byte[] retVal = CoreMethods.GetEntryRaw(key);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for (int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }

        [Test]
        public void TestGetDoubleArrayValid()
        {
            string key = "mykey";

            double[] actualVal = { 3.54538, 43536.825, 34534454.34554, 3423413.23432 };
            CoreMethods.SetEntryDoubleArray(key, actualVal);

            double[] retVal = CoreMethods.GetEntryDoubleArray(key);

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

            string[] actualVal = { "writeas1", "wriasdte2", "writasde3", "adsadsd4" };
            CoreMethods.SetEntryStringArray(key, actualVal);

            string[] retVal = CoreMethods.GetEntryStringArray(key);

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

            bool[] actualVal = { false, false, true, true, false };
            CoreMethods.SetEntryBooleanArray(key, actualVal);

            bool[] retVal = CoreMethods.GetEntryBooleanArray(key);

            Assert.That(retVal.Length, Is.EqualTo(actualVal.Length));

            for (int i = 0; i < actualVal.Length; i++)
            {
                Assert.That(retVal[i], Is.EqualTo(actualVal[i]));
            }
        }
    }
}
