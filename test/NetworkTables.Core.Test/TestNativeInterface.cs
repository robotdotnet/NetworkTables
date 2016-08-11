using System.IO;
using System.Threading;
using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeInterface : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [Test]
        public void TestGetEntries()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            string key2 = "testKey2";
            double toWrite2 = 3.58;
            CoreMethods.SetEntryDouble(key2, toWrite2);
            CoreMethods.SetEntryFlags(key2, EntryFlags.Persistent);

            string key3 = "testKey3";
            bool toWrite3 = true;
            CoreMethods.SetEntryBoolean(key3, toWrite3);

            var entries = CoreMethods.GetEntryInfo("", 0);

            Assert.That(3, Is.EqualTo(entries.Count));

            Assert.That(entries[0].Name, Is.EqualTo(key1));
            Assert.That(entries[0].Type, Is.EqualTo(NtType.String));
            Assert.That(entries[0].Flags, Is.EqualTo(EntryFlags.None));
            Assert.That(entries[0].LastChange, Is.GreaterThan(0));
            Assert.That(entries[1].Name, Is.EqualTo(key2));
            Assert.That(entries[1].Type, Is.EqualTo(NtType.Double));
            Assert.That(entries[1].Flags, Is.EqualTo(EntryFlags.Persistent));
            Assert.That(entries[1].LastChange, Is.GreaterThan(0));
            Assert.That(entries[2].Name, Is.EqualTo(key3));
            Assert.That(entries[2].Type, Is.EqualTo(NtType.Boolean));
            Assert.That(entries[2].Flags, Is.EqualTo(EntryFlags.None));
            Assert.That(entries[2].LastChange, Is.GreaterThan(0));
        }

        [Test]
        public void TestGetEntriesOnlyString()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            string key2 = "testKey2";
            double toWrite2 = 3.58;
            CoreMethods.SetEntryDouble(key2, toWrite2);

            string key3 = "testKey3";
            bool toWrite3 = true;
            CoreMethods.SetEntryBoolean(key3, toWrite3);

            Thread.Sleep(20);

            var entries = CoreMethods.GetEntryInfo("", NtType.String);

            Assert.That(1, Is.EqualTo(entries.Count));

            Assert.That(entries[0].Name, Is.EqualTo(key1));
            Assert.That(entries[0].Type, Is.EqualTo(NtType.String));
            Assert.That(entries[0].Flags, Is.EqualTo(EntryFlags.None));
            Assert.That(entries[0].LastChange, Is.GreaterThan(0));
        }

        [Test]
        public void TestGetConnections()
        {
            var connection = CoreMethods.GetConnections();
            Assert.That(connection.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestSavePersistent()
        {
            string key1 = "key1";
            string toWrite1 = "val1";
            CoreMethods.SetEntryString(key1, toWrite1);
            CoreMethods.SetEntryFlags(key1, EntryFlags.Persistent);

            string key2 = "key2";
            CoreMethods.SetEntryBoolean(key2, true);
            CoreMethods.SetEntryFlags(key2, EntryFlags.Persistent);

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            CoreMethods.SavePersistent(fileName);

            string[] lines = File.ReadAllLines(fileName);

            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(lines[0], Contains.Substring("[NetworkTables Storage 3.0]"));
            Assert.That(lines[1], Contains.Substring($"string \"{key1}\"=\"{toWrite1}\""));
            Assert.That(lines[2], Contains.Substring($"boolean \"{key2}\"=true"));
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
                $"string \"{key1}\"=\"{val1}\"",
                $"boolean \"{key2}\"=true",
                ""
            };

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllLines(fileName, toWrite);

            string[] errors = NtCore.LoadPersistent(fileName);

            Assert.That(errors.Length, Is.EqualTo(0));

            var entries = CoreMethods.GetEntryInfo("", 0);

            Assert.That(entries.Count, Is.EqualTo(2));

            Assert.That(CoreMethods.GetEntryString(key1, ""), Is.EqualTo(val1));
            Assert.That(CoreMethods.GetEntryBoolean(key2, false), Is.EqualTo(true));
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
                $"string \"{key1}\"=\"{val1}\"",
                $"boolean \"{key2}\"=invalid",
                ""
            };

            string fileName = "testfile.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllLines(fileName, toWrite);

            string[] errors = NtCore.LoadPersistent(fileName);

            Assert.That(errors.Length, Is.EqualTo(1));
            Assert.That(errors[0], Contains.Substring("3: unrecognized boolean value, not 'true' or 'false'"));
        }

        [Test]
        public void TestDeleteEntry()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            string key2 = "testKey2";
            double toWrite2 = 3.58;
            CoreMethods.SetEntryDouble(key2, toWrite2);

            Assert.That(CoreMethods.GetEntryInfo("", 0).Count, Is.EqualTo(2));

            CoreMethods.DeleteEntry(key1);

            Assert.That(CoreMethods.GetEntryInfo("", 0).Count, Is.EqualTo(1));

            const string err = "error";
            Assert.That(CoreMethods.GetEntryString(key1, err), Is.EqualTo(err));
        }

        [Test]
        public void TestDeleteAllEntries()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            string key2 = "testKey2";
            double toWrite2 = 3.58;
            CoreMethods.SetEntryDouble(key2, toWrite2);

            Assert.That(CoreMethods.GetEntryInfo("", 0).Count, Is.EqualTo(2));

            DeleteAllWithPersistent();

            Assert.That(CoreMethods.GetEntryInfo("", 0).Count, Is.EqualTo(0));

            const string err = "error";
            Assert.That(CoreMethods.GetEntryString(key1, err), Is.EqualTo(err));
            Assert.That(CoreMethods.GetEntryString(key2, err), Is.EqualTo(err));
        }

        [Test]
        public void TestGetTypeSuccess()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            Assert.That(CoreMethods.GetType(key1), Is.EqualTo(NtType.String));
        }

        [Test]
        public void TestGetTypeNonExistentKey()
        {
            Assert.That(CoreMethods.GetType("testKey"), Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void TestContainsKeySuccess()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            Assert.That(CoreMethods.ContainsKey(key1));
        }

        [Test]
        public void TestContainsKeyNonExistentKey()
        {
            Assert.That(!CoreMethods.ContainsKey("testKey"));
        }

        [Test]
        public void TestGetEntryFlags()
        {
            string key = "testKey";
            CoreMethods.SetEntryString(key, "value");

            EntryFlags flags = CoreMethods.GetEntryFlags(key);

            Assert.That(flags, Is.EqualTo(EntryFlags.None));

            CoreMethods.SetEntryFlags(key, EntryFlags.Persistent);

            flags = CoreMethods.GetEntryFlags(key);

            Assert.That(flags, Is.EqualTo(EntryFlags.Persistent));
        }

        [Test]
        public void TestSetNetworkIdentity()
        {
            CoreMethods.SetNetworkIdentity("UnitTests");

            Assert.Pass();
        }

        [Test]
        public void TestFlush()
        {
            CoreMethods.Flush();

            Assert.Pass();
        }

        [Test]
        public void TestNow()
        {
            Assert.That(CoreMethods.Now(), Is.GreaterThan(0));
        }

        [Test]
        public void TestSetUpdateRate()
        {
            CoreMethods.SetUpdateRate(100);

            Assert.Pass();
        }

        

    }
}
