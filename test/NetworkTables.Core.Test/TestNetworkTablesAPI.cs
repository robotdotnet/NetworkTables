using System.Collections.Generic;
using System.IO;
using NetworkTables.Exceptions;
using NetworkTables.Tables;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNetworkTablesApi : TestBase
    {
        [TestFixtureSetUp]
        public void FixtureSetup()
        {

        }

        private NetworkTable m_table;

        [SetUp]
        public void TestSetup()
        {
            DeleteAllWithPersistent();
            m_table = NetworkTable.GetTable("Table");
        }

        [Test]
        public void TestToString()
        {
            Assert.That(m_table.ToString(), Is.EqualTo("NetworkTable: /Table"));
        }

        [Test]
        public void TestIsServer()
        {
            Assert.That(m_table.IsServer);
        } 

        [Test]
        public void TestContainsKeyNo()
        {
            Assert.That(!m_table.ContainsKey("Key"));
        }

        [Test]
        public void TestContainsKeyYes()
        {
            string key = "key";
            m_table.PutString(key, "value");
            Assert.That(m_table.ContainsKey(key));
        }

        [Test]
        public void TestContainsSubTableFalse()
        {
            Assert.That(!m_table.ContainsSubTable("SubTable"));
        }

        [Test]
        public void TestContainsSubTableTrue()
        {
            string subTableName = "SubTable";

            ITable subTable = m_table.GetSubTable(subTableName);
            subTable.PutBoolean("MyKey", true);

            Assert.That(m_table.ContainsSubTable(subTableName));
        }

        [Test]
        public void TestGetKeysAll()
        {
            string key1 = "FirstKey";
            string key2 = "SecondKey";

            m_table.PutString(key1, "String1");
            m_table.PutString(key2, "String2");

            HashSet<string> keys = m_table.GetKeys();

            Assert.That(keys.Contains(key1));
            Assert.That(keys.Contains(key2));
        }

        [Test]
        public void TestGetKeysOnlyPersistent()
        {
            string key1 = "FirstKey";
            string key2 = "SecondKey";

            m_table.PutString(key1, "String1");
            m_table.PutNumber(key2, 3.586);
            m_table.SetPersistent(key2);

            HashSet<string> keys = m_table.GetKeys(NtType.Double);

            Assert.That(!keys.Contains(key1));
            Assert.That(keys.Contains(key2));
        }

        [Test]
        public void TestGetSubTables()
        {
            string subTableName = "SubTable";

            ITable subTable = m_table.GetSubTable(subTableName);
            subTable.PutBoolean("MyKey", true);

            string subTableName2 = "SubTable2";

            ITable subTable2 = m_table.GetSubTable(subTableName2);
            subTable2.PutBoolean("MyKey2", true);

            HashSet<string> subTables = m_table.GetSubTables();

            Assert.That(subTables.Contains(subTableName));
            Assert.That(subTables.Contains(subTableName2));
        }

        [Test]
        public void TestSetPersistent()
        {
            string key2 = "SecondKey";

            m_table.PutString(key2, "String2");
            m_table.SetPersistent(key2);

            Assert.That(m_table.IsPersistent(key2));
        }

        [Test]
        public void TestClearPersistent()
        {
            string key2 = "SecondKey";

            m_table.PutString(key2, "String2");
            m_table.SetPersistent(key2);

            Assert.That(m_table.IsPersistent(key2));

            m_table.ClearPersistent(key2);

            Assert.That(!m_table.IsPersistent(key2));
        }

        [Test]
        public void TestSetFlags()
        {
            string key2 = "SecondKey";

            m_table.PutString(key2, "String2");
            m_table.SetFlags(key2, EntryFlags.Persistent);

            Assert.That(m_table.GetFlags(key2), Is.EqualTo(EntryFlags.Persistent));
        }

        [Test]
        public void TestClearFlags()
        {
            string key2 = "SecondKey";

            m_table.PutString(key2, "String2");
            m_table.SetFlags(key2, EntryFlags.Persistent);

            Assert.That(m_table.GetFlags(key2), Is.EqualTo(EntryFlags.Persistent));

            m_table.ClearFlags(key2, EntryFlags.Persistent);

            Assert.That(m_table.GetFlags(key2), Is.Not.EqualTo(EntryFlags.Persistent));
        }

        [Test]
        public void TestDeleteKey()
        {
            string key = "MyKey";

            m_table.PutString(key, "Value");

            Assert.That(m_table.GetString(key), Is.EqualTo("Value"));

            m_table.Delete(key);

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetString(key));
        }

        [Test]
        public void TestDeleteAllEntries()
        {
            m_table.PutString("Test1", "val1");
            m_table.PutString("Test2", "val2");
            m_table.PutString("Test3", "val3");

            Assert.That(m_table.GetKeys().Count, Is.EqualTo(3));

            NetworkTable.GlobalDeleteAll();

            Assert.That(m_table.GetKeys().Count, Is.EqualTo(0));
        }

        [Test]
        public void TestSavePersistent()
        {
            string key1 = "key1";
            string toWrite1 = "val1";
            m_table.PutString(key1, toWrite1);
            m_table.SetFlags(key1, EntryFlags.Persistent);

            string key2 = "key2";
            m_table.PutBoolean(key2, true);
            m_table.SetFlags(key2, EntryFlags.Persistent);

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            NetworkTable.SavePersistent(fileName);

            string[] lines = File.ReadAllLines(fileName);

            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(lines[0], Contains.Substring("[NetworkTables Storage 3.0]"));
            Assert.That(lines[1], Contains.Substring($"string \"/Table/{key1}\"=\"{toWrite1}\""));
            Assert.That(lines[2], Contains.Substring($"boolean \"/Table/{key2}\"=true"));
        }

        [Test]
        public void TestLoadPersistent()
        {
            const string key1 = "key1";
            const string key2 = "key2";
            const string val1 = "val1";

            string[] toWrite = new[]
            {
                "[NetworkTables Storage 3.0]",
                $"string \"/Table/{key1}\"=\"{val1}\"",
                $"boolean \"/Table/{key2}\"=true",
                ""
            };

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllLines(fileName, toWrite);

            string[] errors = NetworkTable.LoadPersistent(fileName);

            Assert.That(errors.Length, Is.EqualTo(0));

            HashSet<string> entries = m_table.GetKeys();

            Assert.That(entries.Count, Is.EqualTo(2));

            Assert.That(m_table.GetString(key1, ""), Is.EqualTo(val1));
            Assert.That(m_table.GetBoolean(key2, false), Is.EqualTo(true));
        }

        [Test]
        public void TestPersistentLoadError()
        {
            const string key1 = "key1";
            const string key2 = "key2";
            const string val1 = "val1";

            string[] toWrite = new[]
            {
                "[NetworkTables Storage 3.0]",
                $"string \"/Table/{key1}\"=\"{val1}\"",
                $"boolean \"/Table/{key2}\"=invalid",
                ""
            };

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllLines(fileName, toWrite);

            string[] errors = NetworkTable.LoadPersistent(fileName);

            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0], Contains.Substring("3: unrecognized boolean value, not 'true' or 'false'"));
        }

#region Getters and Setters

        // TODO: Rewrite
        /*
        [Test]
        [TestCase(3.56, ExpectedResult = 3.56)]
        [TestCase(true, ExpectedResult = true)]
        [TestCase("Hello", ExpectedResult = "Hello")]
        [TestCase(new byte[] { 0, 5, 12, 0 }, ExpectedResult = new byte[] { 0, 5, 12, 0 })]
        [TestCase(new double[] { 3.56, 5547.545 }, ExpectedResult = new double[] { 3.56, 5547.545 })]
        [TestCase(new bool[] { false, true, true }, ExpectedResult = new bool[] { false, true, true })]
        public object TestGetValue(object value)
        {
            string key = "key";
            if (value is double)
            {
                m_table.PutNumber(key, (double)value);
            }
            else if (value is string)
            {
                m_table.PutString(key, (string)value);
            }
            else if (value is bool)
            {
                m_table.PutBoolean(key, (bool)value);
            }
            else if (value is byte[])
            {
                m_table.PutRaw(key, (byte[])value);
            }
            else if (value is double[])
            {
                m_table.PutNumberArray(key, (double[])value);
            }
            else if (value is bool[])
            {
                m_table.PutBooleanArray(key, (bool[])value);
            }
            else if (value is string[])
            {
                m_table.PutStringArray(key, (string[])value);
            }
            else
            {
                throw new ArgumentException("Value is either null or an invalid type.");
            }
            return m_table.GetValue(key);
        }
        */

        [Test]
        public void TestGetValueStringArray()
        {
            string key = "key";
            string[] array = new[] { "Hello1", "Hello2" };

            m_table.PutStringArray(key, array);

            var v = m_table.GetValue(key);
            Assert.That(v, Is.Not.Null);

            Assert.That(v.GetStringArray(), Is.EquivalentTo(array));
        }

        [Test]
        public void TestGetValueInvalid()
        {
            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetValue("key"));
        }

        // TODO: Rewrite
        /*
        [Test]
        [TestCase(3.56, ExpectedResult = 3.56)]
        [TestCase(true, ExpectedResult = true)]
        [TestCase("Hello", ExpectedResult = "Hello")]
        [TestCase(new byte[] { 0, 5, 12, 0 }, ExpectedResult = new byte[] { 0, 5, 12, 0 })]
        [TestCase(new double[] { 3.56, 5547.545 }, ExpectedResult = new double[] { 3.56, 5547.545 })]
        [TestCase(new bool[] { false, true, true }, ExpectedResult = new bool[] { false, true, true })]
        public object TestGetValueDefault(object value)
        {
            string key = "key";
            if (value is double)
            {
                m_table.PutNumber(key, (double)value);
            }
            else if (value is string)
            {
                m_table.PutString(key, (string)value);
            }
            else if (value is bool)
            {
                m_table.PutBoolean(key, (bool)value);
            }
            else if (value is byte[])
            {
                m_table.PutRaw(key, (byte[])value);
            }
            else if (value is double[])
            {
                m_table.PutNumberArray(key, (double[])value);
            }
            else if (value is bool[])
            {
                m_table.PutBooleanArray(key, (bool[])value);
            }
            else if (value is string[])
            {
                m_table.PutStringArray(key, (string[])value);
            }
            else
            {
                throw new ArgumentException("Value is either null or an invalid type.");
            }
            return m_table.GetValue(key, null);
        }
        */
        [Test]
        public void TestGetValueStringArrayDefault()
        {
            string key = "key";
            string[] array = new[] { "Hello1", "Hello2" };

            m_table.PutStringArray(key, array);

            var v = m_table.GetValue(key);
            Assert.That(v, Is.Not.Null);

            Assert.That(v.GetStringArray(), Is.EquivalentTo(array));
        }

        // TODO: REWRITE
        /*
        [Test]
        [TestCase(3.56, ExpectedResult = 3.56)]
        [TestCase(true, ExpectedResult = true)]
        [TestCase("Hello", ExpectedResult = "Hello")]
        [TestCase(new byte[] {0, 5, 12, 0}, ExpectedResult = new byte[] {0, 5, 12, 0})]
        [TestCase(new double[] {3.56, 5547.545}, ExpectedResult = new double[] {3.56, 5547.545})]
        [TestCase(new bool[] {false, true, true}, ExpectedResult = new bool[] {false, true, true})]
        public object TestPutValue(object value)
        {
            string key = "key";
            m_table.PutValue(key, value);
            return m_table.GetValue(key);
        }
        */

        [Test]
        public void TestPutValueStringArray()
        {
            string key = "key";
            string[] array = new[] { "Hello1", "Hello2" };

            m_table.PutValue(key, Value.MakeStringArray(array));

            var v = m_table.GetValue(key);
            Assert.That(v, Is.Not.Null);

            Assert.That(v.GetStringArray(), Is.EquivalentTo(array));
        }


        [Test]
        public void TestPutAndGetNumberValid()
        {
            string key = "key";
            double setVal = 5.88;
            Assert.That(m_table.PutNumber(key, setVal));

            Assert.That(m_table.GetNumber(key), Is.EqualTo(setVal).Within(0.01));
        }

        [Test]
        public void TestPutAndGetNumberValidDefault()
        {
            string key = "key";
            double setVal = 5.88;
            Assert.That(m_table.PutNumber(key, setVal));

            Assert.That(m_table.GetNumber(key, 3343.232), Is.EqualTo(setVal).Within(0.01));
        }

        [Test]
        public void TestPutNumberIntoWrongType()
        {
            string key = "key";
            double setVal = 5.88;
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutNumber(key, setVal));
        }

        [Test]
        public void TestGetNumberErrorDefault()
        {
            string key = "key";

            double defaultVal = 123322.32123;
            Assert.That(m_table.GetNumber(key, defaultVal), Is.EqualTo(defaultVal).Within(0.01));
        }

        [Test]
        public void TestGetNumberErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetNumber(key));
        }

        [Test]
        public void TestGetNumberErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetNumber(key));
        }






        [Test]
        public void TestPutAndGetBooleanValid()
        {
            string key = "key";
            bool setVal = true;
            Assert.That(m_table.PutBoolean(key, setVal));

            Assert.That(m_table.GetBoolean(key), Is.EqualTo(setVal));
        }

        [Test]
        public void TestPutAndGetBooleanValidDefault()
        {
            string key = "key";
            bool setVal = true;
            Assert.That(m_table.PutBoolean(key, setVal));

            Assert.That(m_table.GetBoolean(key, false), Is.EqualTo(setVal));
        }

        [Test]
        public void TestPutBooleanIntoWrongType()
        {
            string key = "key";
            bool setVal = true;
            m_table.PutString(key, "string");
            Assert.That(!m_table.PutBoolean(key, setVal));
        }

        [Test]
        public void TestGetBooleanErrorDefault()
        {
            string key = "key";

            bool defaultVal = true;
            Assert.That(m_table.GetBoolean(key, defaultVal), Is.EqualTo(defaultVal));
        }

        [Test]
        public void TestGetBooleanErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutString(key, "string");

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetBoolean(key));
        }

        [Test]
        public void TestGetBooleanErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetBoolean(key));
        }





        [Test]
        public void TestPutAndGetStringValid()
        {
            string key = "key";
            string setVal = "string";
            Assert.That(m_table.PutString(key, setVal));

            Assert.That(m_table.GetString(key), Is.EqualTo(setVal));
        }

        [Test]
        public void TestPutAndGetStringValidDefault()
        {
            string key = "key";
            string setVal = "string";
            Assert.That(m_table.PutString(key, setVal));

            Assert.That(m_table.GetString(key, "Default"), Is.EqualTo(setVal));
        }

        [Test]
        public void TestPutStringIntoWrongType()
        {
            string key = "key";
            string setVal = "string";
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutString(key, setVal));
        }

        [Test]
        public void TestGetStringErrorDefault()
        {
            string key = "key";

            string defaultVal = "string";
            Assert.That(m_table.GetString(key, defaultVal), Is.EqualTo(defaultVal));
        }

        [Test]
        public void TestGetStringErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetString(key));
        }

        [Test]
        public void TestGetStringErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetString(key));
        }



        [Test]
        public void TestPutAndGetStringArrayValid()
        {
            string key = "key";
            string[] setVal = {
                "string",
                "string2",
                "string3",
            };
            Assert.That(m_table.PutStringArray(key, setVal));

            Assert.That(m_table.GetStringArray(key), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutAndGetStringArrayValidDefault()
        {
            string key = "key";
            string[] setVal = {
                "string",
                "string2",
                "string3",
            };
            Assert.That(m_table.PutStringArray(key, setVal));

            Assert.That(m_table.GetStringArray(key, new[] { "Default" }), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutStringArrayIntoWrongType()
        {
            string key = "key";
            string[] setVal = {
                "string",
                "string2",
                "string3",
            };
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutStringArray(key, setVal));
        }

        [Test]
        public void TestGetStringArrayErrorDefault()
        {
            string key = "key";

            string[] defaultVal = {
                "string",
                "string2",
                "string3",
            };
            Assert.That(m_table.GetStringArray(key, defaultVal), Is.EquivalentTo(defaultVal));
        }

        [Test]
        public void TestGetStringArrayErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetStringArray(key));
        }

        [Test]
        public void TestGetStringArrayErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetStringArray(key));
        }




        [Test]
        public void TestPutAndGetNumberArrayValid()
        {
            string key = "key";
            double[] setVal = {
                123.4,
                5.8,
                6.2,
            };
            Assert.That(m_table.PutNumberArray(key, setVal));

            Assert.That(m_table.GetNumberArray(key), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutAndGetNumberArrayValidDefault()
        {
            string key = "key";
            double[] setVal = {
                123.4,
                5.8,
                6.2,
            };
            Assert.That(m_table.PutNumberArray(key, setVal));

            Assert.That(m_table.GetNumberArray(key, new[] { 0.0 }), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutNumberArrayIntoWrongType()
        {
            string key = "key";
            double[] setVal = {
                123.4,
                5.8,
                6.2,
            };
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutNumberArray(key, setVal));
        }

        [Test]
        public void TestGetNumberArrayErrorDefault()
        {
            string key = "key";

            double[] defaultVal = {
                123.4,
                5.8,
                6.2,
            };
            Assert.That(m_table.GetNumberArray(key, defaultVal), Is.EquivalentTo(defaultVal));
        }

        [Test]
        public void TestGetNumberArrayErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetNumberArray(key));
        }

        [Test]
        public void TestGetNumberArrayErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetNumberArray(key));
        }



        [Test]
        public void TestPutAndGetRawValid()
        {
            string key = "key";
            byte[] setVal = {
                123,
                5,
                6,
            };
            Assert.That(m_table.PutRaw(key, setVal));

            Assert.That(m_table.GetRaw(key), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutAndGetRawValidDefault()
        {
            string key = "key";
            byte[] setVal = {
                123,
                5,
                6,
            };
            Assert.That(m_table.PutRaw(key, setVal));

            Assert.That(m_table.GetRaw(key, new byte[] { 0 }), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutRawIntoWrongType()
        {
            string key = "key";
            byte[] setVal = {
                123,
                5,
                6,
            };
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutRaw(key, setVal));
        }

        [Test]
        public void TestGetRawErrorDefault()
        {
            string key = "key";

            byte[] defaultVal = {
                123,
                5,
                6,
            };
            Assert.That(m_table.GetRaw(key, defaultVal), Is.EquivalentTo(defaultVal));
        }

        [Test]
        public void TestGetRawErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetRaw(key));
        }

        [Test]
        public void TestGetRawErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetRaw(key));
        }




        [Test]
        public void TestPutAndGetBooleanArrayValid()
        {
            string key = "key";
            bool[] setVal = {
                true,
                false,
                true,
            };
            Assert.That(m_table.PutBooleanArray(key, setVal));

            Assert.That(m_table.GetBooleanArray(key), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutAndGetBooleanArrayValidDefault()
        {
            string key = "key";
            bool[] setVal = {
                true,
                false,
                true,
            };
            Assert.That(m_table.PutBooleanArray(key, setVal));

            Assert.That(m_table.GetBooleanArray(key, new[] { false }), Is.EquivalentTo(setVal));
        }

        [Test]
        public void TestPutBooleanArrayIntoWrongType()
        {
            string key = "key";
            bool[] setVal = {
                true,
                false,
                true,
            };
            m_table.PutBoolean(key, true);
            Assert.That(!m_table.PutBooleanArray(key, setVal));
        }

        [Test]
        public void TestGetBooleanArrayErrorDefault()
        {
            string key = "key";

            bool[] defaultVal = {
                true,
                false,
                true,
            };
            Assert.That(m_table.GetBooleanArray(key, defaultVal), Is.EquivalentTo(defaultVal));
        }

        [Test]
        public void TestGetBooleanArrayErrorThrowsWrongType()
        {
            string key = "key";
            m_table.PutBoolean(key, true);

            Assert.Throws<TableKeyDifferentTypeException>(() => m_table.GetBooleanArray(key));
        }

        [Test]
        public void TestGetBooleanArrayErrorThrowsNotDefined()
        {
            string key = "key";

            Assert.Throws<TableKeyNotDefinedException>(() => m_table.GetBooleanArray(key));
        }
#endregion

        // The below tests have no way to be checked, so we are just making sure they don't throw exceptions.


        [Test]
        public void TestSetNetworkIdentity()
        {
            NetworkTable.SetNetworkIdentity("UnitTests");

            Assert.Pass();
        }

        [Test]
        public void TestGetConnections()
        {
            Assert.That(NetworkTable.Connections(), Has.Count.EqualTo(0));
        }

        [Test]
        public void TestIsConnected()
        {
            Assert.That(!m_table.IsConnected);
        }

        [Test]
        public void TestFlush()
        {
            NetworkTable.Flush();

            Assert.Pass();
        }

        [Test]
        public void TestSetUpdateRate()
        {
            NetworkTable.SetUpdateRate(100);

            Assert.Pass();
        }
    }
}
