using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class StorageEmptyTest : StorageBaseTest
    {
        public StorageEmptyTest(bool server)
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
        }

        [Test]
        public void EntryInit()
        {
            
            var entry = GetEntry("foo");
            Assert.That(entry.Value, Is.Null);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.None));
            Assert.That(entry.Name, Is.EqualTo("foobar"));
            Assert.That(entry.Id, Is.EqualTo(0xffffu));
            Assert.That(new SequenceNumber() == entry.SeqNum);
        }

        [Test]
        public void GetEntryValueNotExist()
        {
            
            Assert.That(storage.GetEntryValue("foo"), Is.Null);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void GetEntyValueExist()
        {
            
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            outgoing.Clear();
            Assert.That(value, Is.EqualTo(storage.GetEntryValue("foo")));
        }

        [Test]
        public void SetEntryTypeValueAssignNew()
        {
            
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("foo", value);
            Assert.That(value, Is.EqualTo(GetEntry("foo").Value));
            if (m_server)
            {
                Assert.That(IdMap, Has.Count.EqualTo(1));
                Assert.That(value, Is.EqualTo(IdMap[0].Value));
            }
            else
            {
                Assert.That(IdMap, Is.Empty);
            }

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);

            var msg = outgoing[0].msg;

            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryAssign));

            Assert.That(msg.Str, Is.EqualTo("foo"));

            if (m_server)
            {
                Assert.That(msg.Id, Is.EqualTo(0u));
            }
            else
            {
                Assert.That(msg.Id, Is.EqualTo(0xffffu));
            }

            Assert.That(msg.SeqNumUid, Is.EqualTo(1u));
            Assert.That(msg.Val, Is.EqualTo(value));
            Assert.That(msg.Flags, Is.EqualTo(0u));
        }

        [Test]
        public void SetEntryTypeValueEmptyName()
        {
            
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue("", value);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryTypeValueNullName()
        {
            
            var value = Value.MakeBoolean(true);
            storage.SetEntryTypeValue(null, value);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryValueEmptyValue()
        {
            
            storage.SetEntryTypeValue(null, null);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetDefaultEntryAssignNew()
        {
            var value = Value.MakeBoolean(true);
            var retVal = storage.SetDefaultEntryValue("foo", value);
            Assert.That(retVal, Is.True);
            Assert.That(value, Is.EqualTo(GetEntry("foo").Value));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryAssign));
            Assert.That(msg.Str, Is.EqualTo("foo"));
            if (m_server)
            {
                Assert.That(msg.Id, Is.EqualTo(0));//Assigned as server
            }
            else
            {
                Assert.That(msg.Id, Is.EqualTo(0xffff));
            }
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Val, Is.EqualTo(value));
            Assert.That(msg.Flags, Is.EqualTo(0));
        }

        [Test]
        public void SetDefaultEntryEmptyName()
        {
            var value = Value.MakeBoolean(true);
            var retVal = storage.SetDefaultEntryValue("", value);
            Assert.That(retVal, Is.False);
            var entry = GetEntry("foo");
            Assert.That(entry.Value, Is.Null);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.None));
            Assert.That("foobar", Is.EqualTo(entry.Name)); // since GetEntry uses the tmp_entry
            Assert.That(0xffff, Is.EqualTo(entry.Id));
            Assert.That(new SequenceNumber(), Is.EqualTo(entry.SeqNum));
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetDefaultEntryNullName()
        {
            var value = Value.MakeBoolean(true);
            var retVal = storage.SetDefaultEntryValue(null, value);
            Assert.That(retVal, Is.False);
            var entry = GetEntry("foo");
            Assert.That(entry.Value, Is.Null);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.None));
            Assert.That("foobar", Is.EqualTo(entry.Name)); // since GetEntry uses the tmp_entry
            Assert.That(0xffff, Is.EqualTo(entry.Id));
            Assert.That(new SequenceNumber(), Is.EqualTo(entry.SeqNum));
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetDefaultEntryEmptyValue()
        {
            var retVal = storage.SetDefaultEntryValue("", null);
            Assert.That(retVal, Is.False);
            var entry = GetEntry("foo");
            Assert.That(entry.Value, Is.Null);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.None));
            Assert.That("foobar", Is.EqualTo(entry.Name)); // since GetEntry uses the tmp_entry
            Assert.That(0xffff, Is.EqualTo(entry.Id));
            Assert.That(new SequenceNumber(), Is.EqualTo(entry.SeqNum));
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryValueAssignNew()
        {
            
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue("foo", value), Is.True);
            Assert.That(value, Is.EqualTo(GetEntry("foo").Value));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryAssign));
            Assert.That(msg.Str, Is.EqualTo("foo"));
            if (m_server)
            {
                Assert.That(msg.Id, Is.EqualTo(0));//Assigned as server
            }
            else
            {
                Assert.That(msg.Id, Is.EqualTo(0xffff));
            }
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Val, Is.EqualTo(value));
            Assert.That(msg.Flags, Is.EqualTo(0));
        }

        [Test]
        public void SetEntryValueEmptyName()
        {
            
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue("", value), Is.True);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryValueNullName()
        {
            
            var value = Value.MakeBoolean(true);
            Assert.That(storage.SetEntryValue(null, value), Is.True);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryFlagsNew()
        {
            
            storage.SetEntryFlags("foo", 0);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryFlagsEmptyName()
        {
            
            storage.SetEntryFlags("", EntryFlags.None);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void SetEntryFlagsNullName()
        {
            
            storage.SetEntryFlags(null, EntryFlags.None);
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void GetEntryFlagsNotExist()
        {
            
            Assert.That(storage.GetEntryFlags("foo"), Is.EqualTo(EntryFlags.None));
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void DeleteEntryNotExist()
        {
            
            storage.DeleteEntry("foo");
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void DeleteAllEntriesEmpty()
        {
            
            storage.DeleteAllEntries();
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void LoadPersistentBadHeader()
        {
            
            int lastLine = -1;
            string lastString = string.Empty;



            Action<int, string> warnFunc = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.That(storage.LoadPersistent(stream, warnFunc), Is.False);
                Assert.That(lastLine, Is.EqualTo(1));
                Assert.That(lastString, Is.EqualTo("header line mismatch, ignoring rest of file"));
            }

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables")))
            {
                Assert.That(storage.LoadPersistent(stream, warnFunc), Is.False);
                Assert.That(lastLine, Is.EqualTo(1));
                Assert.That(lastString, Is.EqualTo("header line mismatch, ignoring rest of file"));
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void LoadPersistentCommentHeader()
        {
            
            int lastLine = -1;
            string lastString = string.Empty;

            Action<int, string> warnFunc = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("\n; comment\n# comment\n[NetworkTables Storage 3.0]\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warnFunc), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void LoadPersistentEmptyName()
        {
            
            int lastLine = -1;
            string lastString = string.Empty;

            Action<int, string> warnFunc = (int line, string msg) =>
            {
                lastLine = line;
                lastString = msg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"\"=true\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warnFunc), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }

        [Test]
        public void LoadPersistentAssign()
        {
            
            int lastLine = -1;
            string lastString = string.Empty;

            Action<int, string> warnFunc = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"foo\"=true\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warnFunc), Is.True);
                Assert.That(lastLine, Is.EqualTo(-1));
                Assert.That(lastString, Is.Empty);
            }
            var entry = GetEntry("foo");
            Assert.That(Value.MakeBoolean(true) == entry.Value);
            Assert.That(entry.Flags, Is.EqualTo(EntryFlags.Persistent));

            Assert.That(outgoing, Has.Count.EqualTo(1));
            Assert.That(outgoing[0].only, Is.Null);
            Assert.That(outgoing[0].except, Is.Null);
            var msg = outgoing[0].msg;
            Assert.That(msg.Type, Is.EqualTo(Message.MsgType.EntryAssign));
            Assert.That(msg.Str, Is.EqualTo("foo"));
            if (m_server)
            {
                Assert.That(msg.Id, Is.EqualTo(0));
            }
            else
            {
                Assert.That(msg.Id, Is.EqualTo(0xffff));
            }
            Assert.That(msg.SeqNumUid, Is.EqualTo(1));
            Assert.That(Value.MakeBoolean(true) == msg.Val);

            Assert.That(msg.Flags, Is.EqualTo((uint)EntryFlags.Persistent));
        }

        [Test]
        public void LoadPersistent()
        {
            

            int lastLine = -1;
            string lastString = String.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            string specialLargeDigits = "\0\xAE\xFF\n";

            string i = "[NetworkTables Storage 3.0]\n";
            i += "boolean \"\\x00\\x03\\x05\\n\"=true\n";
            i += "boolean \"\\x00\\xAE\\xFF\\n\"=false\n";
            i += "boolean \"boolean/false\"=false\n";
            i += "boolean \"boolean/true\"=true\n";
            i += "array boolean \"booleanarr/empty\"=\n";
            i += "array boolean \"booleanarr/one\"=true\n";
            i += "array boolean \"booleanarr/two\"=true,false\n";
            i += "double \"double/big\"=1.3e+08\n";
            i += "double \"double/neg\"=-1.5\n";
            i += "double \"double/zero\"=0\n";
            i += "array double \"doublearr/empty\"=\n";
            i += "array double \"doublearr/one\"=0.5\n";
            i += "array double \"doublearr/two\"=0.5,-0.25\n";
            i += "raw \"raw/empty\"=\n";
            i += "raw \"raw/normal\"=aGVsbG8=\n";
            i += "raw \"raw/special\"=AAMFCg==\n";
            i += "raw \"raw/speciallarge\"=AMKuw78K\n";
            i += "string \"string/empty\"=\"\"\n";
            i += "string \"string/normal\"=\"hello\"\n";
            i += "string \"string/special\"=\"\\x00\\x03\\x05\\n\"\n";
            i += "string \"string/speciallarge\"=\"\\x00\\xAE\\xFF\\n\"\n";
            i += "string \"string/paranth\"=\"M\\\"Q\"\n";
            i += "array string \"stringarr/empty\"=\n";
            i += "array string \"stringarr/one\"=\"hello\"\n";
            i += "array string \"stringarr/two\"=\"hello\",\"world\\n\"\n";

            using (MemoryStream iss = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(iss);
                writer.Write(i);
                writer.Flush();
                iss.Position = 0;
                Assert.That(storage.LoadPersistent(iss, warn_func), Is.True);
            }
            Assert.That(Entries, Has.Count.EqualTo(25));
            Assert.That(outgoing, Has.Count.EqualTo(25));

            string s = "\0\x03\x05\n";

            Assert.That(Value.MakeBoolean(true) == storage.GetEntryValue("boolean/true"));
            Assert.That(Value.MakeBoolean(false) == storage.GetEntryValue("boolean/false"));
            Assert.That(Value.MakeDouble(-1.5) == storage.GetEntryValue("double/neg"));
            Assert.That(Value.MakeDouble(0.0) == storage.GetEntryValue("double/zero"));
            Assert.That(Value.MakeDouble(1.3e8) == storage.GetEntryValue("double/big"));
            Assert.That(Value.MakeString("") == storage.GetEntryValue("string/empty"));
            Assert.That(Value.MakeString("hello") == storage.GetEntryValue("string/normal"));
            Assert.That(Value.MakeString(s) == storage.GetEntryValue("string/special"));
            Assert.That(Value.MakeString(specialLargeDigits) == storage.GetEntryValue("string/speciallarge"));
            Assert.That(Value.MakeString("M\"Q") == storage.GetEntryValue("string/paranth"));
            Assert.That(Value.MakeRaw() == storage.GetEntryValue("raw/empty"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes("hello")) == storage.GetEntryValue("raw/normal"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes(s)) == storage.GetEntryValue("raw/special"));
            Assert.That(Value.MakeRaw(Encoding.UTF8.GetBytes(specialLargeDigits)) == storage.GetEntryValue("raw/speciallarge"));
            Assert.That(Value.MakeBooleanArray() == storage.GetEntryValue("booleanarr/empty"));
            Assert.That(Value.MakeBooleanArray(true) == storage.GetEntryValue("booleanarr/one"));
            Assert.That(Value.MakeBooleanArray(true, false) == storage.GetEntryValue("booleanarr/two"));
            Assert.That(Value.MakeDoubleArray() == storage.GetEntryValue("doublearr/empty"));
            Assert.That(Value.MakeDoubleArray(0.5) == storage.GetEntryValue("doublearr/one"));
            Assert.That(Value.MakeDoubleArray(0.5, -0.25) == storage.GetEntryValue("doublearr/two"));
            Assert.That(Value.MakeStringArray() == storage.GetEntryValue("stringarr/empty"));
            Assert.That(Value.MakeStringArray("hello") == storage.GetEntryValue("stringarr/one"));
            Assert.That(Value.MakeStringArray("hello", "world\n") == storage.GetEntryValue("stringarr/two"));
            Assert.That(Value.MakeBoolean(true) == storage.GetEntryValue(s));
            Assert.That(Value.MakeBoolean(false) == storage.GetEntryValue(specialLargeDigits));
        }

        [Test]
        public void LoadPersistentWarn()
        {
            

            int lastLine = -1;
            string lastString = string.Empty;

            Action<int, string> warn_func = (int line, string mg) =>
            {
                lastLine = line;
                lastString = mg;
            };

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("[NetworkTables Storage 3.0]\nboolean \"foo\"=foo\n")))
            {
                Assert.That(storage.LoadPersistent(stream, warn_func), Is.True);
                Assert.That(lastLine, Is.EqualTo(2));
                Assert.That(lastString, Is.EqualTo("unrecognized boolean value, not 'true' or 'false'"));
            }
            Assert.That(Entries, Is.Empty);
            Assert.That(IdMap, Is.Empty);
            Assert.That(outgoing, Is.Empty);
        }
    }
}
