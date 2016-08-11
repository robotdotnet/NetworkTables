using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class StorageTestPopulated : StorageBaseTest
    {
        public StorageTestPopulated(bool server)
        {
            m_server = server;
        }

        [SetUp]
        public void Clear()
        {
            tmpEntry = new Storage.Entry("foobar");
            storage?.Dispose();
            storage = new Storage();
            outgoing.Clear();

            HookOutgoing();

            storage.SetEntryTypeValue("foo", Value.MakeBoolean(true));
            storage.SetEntryTypeValue("foo2", Value.MakeDouble(0.0));
            storage.SetEntryTypeValue("bar", Value.MakeDouble(1.0));
            storage.SetEntryTypeValue("bar2", Value.MakeBoolean(false));

            outgoing.Clear();
        }

        [Test]
        public void SetDefaultEntryEmptyName()
        {
            var value = Value.MakeBoolean(true);
            var retVal = storage.SetDefaultEntryValue("", value);
            Assert.That(retVal, Is.False);
            // assert that no entries got added
            Assert.That(Entries, Has.Count.EqualTo(4));
            if (m_server)
            {
                Assert.That(IdMap, Has.Count.EqualTo(4));
            }
            else
            {
                Assert.That(IdMap, Has.Count.EqualTo(0));
            }
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetDefaultEntryEmptyValue()
        {
            var retVal = storage.SetDefaultEntryValue("", null);
            Assert.That(retVal, Is.False);
            // assert that no entries got added
            Assert.That(Entries, Has.Count.EqualTo(4));
            if (m_server)
            {
                Assert.That(IdMap, Has.Count.EqualTo(4));
            }
            else
            {
                Assert.That(IdMap, Has.Count.EqualTo(0));
            }
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void TestStoragePopulatedSetEntryTypeValueDifferentValue()
        {

            var value = Value.MakeDouble(1.0);
            storage.SetEntryTypeValue("foo2", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo2").Value));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.SeqNumUid, Is.EqualTo(2));
                Assert.That(msg.Val, Is.EqualTo(value));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").SeqNum.Value, Is.EqualTo(2));
            }

        }


        [Test]
        public void TestStoragePopulatedSetEntryValueDifferentValue()
        {

            var value = Value.MakeDouble(1.0);
            Assert.That(storage.SetEntryValue("foo2", value), Is.True);
            var entry = GetEntry("foo2");
            Assert.That(entry.Value, Is.EqualTo(value));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.SeqNumUid, Is.EqualTo(2));
                Assert.That(msg.Val, Is.EqualTo(value));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").SeqNum.Value, Is.EqualTo(2));
            }
        }

        [Test]
        public void TestStoragePopulatedSetEntryFlagsDifferentValue()
        {

            storage.SetEntryFlags("foo2", EntryFlags.Persistent);
            Assert.That(GetEntry("foo2").Flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.FlagsUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.Flags, Is.EqualTo(1));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStoragePopulatedDeleteEntryExist()
        {

            storage.DeleteEntry("foo2");
            Assert.That(Entries.ContainsKey("foo2"), Is.False);

            if (m_server)
            {
                Assert.That(IdMap, Has.Count.GreaterThanOrEqualTo(2));
                Assert.That(IdMap[1], Is.Null);
            }

            if (m_server)
            {

                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryDelete));
                Assert.That(msg.Id, Is.EqualTo(1));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStoragePopulatedDeleteAllEntries()
        {

            storage.DeleteAllEntries();
            Assert.That(Entries, Is.Empty);

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.ClearEntries));
        }

        [Test]
        public void TestStoragePopulatedDeleteAllEntriesPersistent()
        {
            GetEntry("foo2").Flags = EntryFlags.Persistent;
            storage.DeleteAllEntries();
            Assert.That(Entries, Has.Count.EqualTo(1));
            Assert.That(Entries, Contains.Key("foo2"));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.ClearEntries));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoAll()
        {

            var info = storage.GetEntryInfo("", 0);
            Assert.That(info, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoAllNullPrefix()
        {

            var info = storage.GetEntryInfo(null, 0);
            Assert.That(info, Has.Count.EqualTo(4));
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoPrefix()
        {

            var info = storage.GetEntryInfo("foo", 0);
            Assert.That(info, Has.Count.EqualTo(2));

            if (info[0].Name == "foo")
            {
                Assert.That(info[0].Type, Is.EqualTo(NtType.Boolean));
                Assert.That(info[1].Name, Is.EqualTo("foo2"));
                Assert.That(info[1].Type, Is.EqualTo(NtType.Double));
            }
            else
            {
                Assert.That(info[0].Name, Is.EqualTo("foo2"));
                Assert.That(info[0].Type, Is.EqualTo(NtType.Double));
                Assert.That(info[1].Name, Is.EqualTo("foo"));
                Assert.That(info[1].Type, Is.EqualTo(NtType.Boolean));
            }
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoTypes()
        {


            var info = storage.GetEntryInfo("", NtType.Double);
            Assert.That(info, Has.Count.EqualTo(2));
            Assert.That(info[0].Type, Is.EqualTo(NtType.Double));
            Assert.That(info[1].Type, Is.EqualTo(NtType.Double));

            if (info[0].Name == "foo2")
            {
                Assert.That(info[1].Name, Is.EqualTo("bar"));
            }
            else
            {
                Assert.That(info[0].Name, Is.EqualTo("bar"));
                Assert.That(info[1].Name, Is.EqualTo("foo2"));
            }
        }

        [Test]
        public void TestStoragePopulatedGetEntryInfoPrefixTypes()
        {

            var info = storage.GetEntryInfo("bar", NtType.Boolean);
            Assert.That(info, Has.Count.EqualTo(1));
            Assert.That(info[0].Name, Is.EqualTo("bar2"));
            Assert.That(info[0].Type, Is.EqualTo(NtType.Boolean));
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateFlags()
        {

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=0.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(0.0) == entry.Value);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.FlagsUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.Flags, Is.EqualTo((uint)EntryFlags.Persistent));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
            }
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateValue()
        {

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            GetEntry("foo2").Flags = EntryFlags.Persistent;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=1.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(1.0) == entry.Value);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(1));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.SeqNumUid, Is.EqualTo(2));
                Assert.That(Value.MakeDouble(1.0) == msg.Val);
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").SeqNum.Value, Is.EqualTo(2));
            }
        }

        [Test]
        public void TestStoragePopulatedLoadPersistentUpdateValueFlags()
        {

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\ndouble \"foo2\"=1.0\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo2");
            Assert.That(Value.MakeDouble(1.0) == entry.Value);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.Persistent));

            if (m_server)
            {
                Assert.That(outgoing, Has.Count.EqualTo(2));
                Assert.That(outgoing[0].only, Is.Null);
                Assert.That(outgoing[0].except, Is.Null);
                var msg = outgoing[0].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.SeqNumUid, Is.EqualTo(2));
                Assert.That(Value.MakeDouble(1.0) == msg.Val);

                Assert.That(outgoing[1].only, Is.Null);
                Assert.That(outgoing[1].except, Is.Null);
                msg = outgoing[1].msg;
                Assert.That(msg.Type, Is.EqualTo(Message.MsgType.FlagsUpdate));
                Assert.That(msg.Id, Is.EqualTo(1));
                Assert.That(msg.Flags, Is.EqualTo((uint)EntryFlags.Persistent));
            }
            else
            {
                Assert.That(outgoing, Is.Empty);
                Assert.That(GetEntry("foo2").SeqNum.Value, Is.EqualTo(2));
            }
        }
    }
}
