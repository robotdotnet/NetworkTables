using System;
using NetworkTables.Exceptions;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNetworkTableErrors : TestBase
    {
        private NetworkTable m_nt;

        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
            m_nt = NetworkTable.GetTable("");
        }

        [Test]
        public void TestBadPersistentFileRead()
        {
            Assert.Throws<PersistentException>(() =>
            {
                NetworkTable.LoadPersistent("invalid.txt");
            });
        }

        [Test]
        public void TestPutEmptyName()
        {
            bool retVal = m_nt.PutNumber("emptyName", 3.14);
            Assert.IsTrue(retVal);
        }

        [Test]
        public void TestPutNullName()
        {
            bool retVal = m_nt.PutNumber(null, 3.14);
            Assert.IsTrue(retVal);
        }
        [Test]
        public void TestPutNullString()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                m_nt.PutString("nullString", null);
            }, "Null string should not be allowed");

        }
        [Test]
        public void TestPutEmptyString()
        {
            bool retVal = m_nt.PutString("emptyString", "");
            Assert.IsTrue(retVal);
        }
        [Test]
        public void TestPutNullStringArray()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                m_nt.PutStringArray("nullStringArray", null);
            }, "Null array should not be allowed");
        }
        [Test]
        public void TestPutEmptyStringArray()
        {
            bool retVal = m_nt.PutStringArray("emptyStringArray", new string[0]);
            Assert.IsTrue(retVal);
        }
        [Test]
        public void TestPutNullBooleanArray()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                m_nt.PutBooleanArray("nullBoolArray", null);
            }, "Null array should not be allowed");
        }
        [Test]
        public void TestPutEmptyBooleanArray()
        {
            bool retVal = m_nt.PutBooleanArray("emptyBoolArray", new bool[0]);
            Assert.IsTrue(retVal);
        }
        [Test]
        public void TestPutNullDoubleArray()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                m_nt.PutNumberArray("nullNumberArray", null);
            }, "Null array should not be allowed");
        }
        [Test]
        public void TestPutEmptyDoubleArray()
        {
            bool retVal = m_nt.PutNumberArray("emptyNumberArray", new double[0]);
            Assert.IsTrue(retVal);
        }
    }
}
