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
    public class StorageTestPersistent : StorageBaseTest
    {
        public StorageTestPersistent(bool server)
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

            string s = "\0\x03\x05\n";
            string specialLargeDigits = "\0\xAE\xFF\n";

            storage.SetEntryTypeValue("boolean/true", Value.MakeBoolean(true));
            storage.SetEntryTypeValue("boolean/false", Value.MakeBoolean(false));
            storage.SetEntryTypeValue("double/neg", Value.MakeDouble(-1.5));
            storage.SetEntryTypeValue("double/zero", Value.MakeDouble(0.0));
            storage.SetEntryTypeValue("double/big", Value.MakeDouble(1.3e8));
            storage.SetEntryTypeValue("string/empty", Value.MakeString(""));
            storage.SetEntryTypeValue("string/normal", Value.MakeString("hello"));
            storage.SetEntryTypeValue("string/special",
                                      Value.MakeString(s));
            storage.SetEntryTypeValue("string/speciallarge",
                                      Value.MakeString(specialLargeDigits));
            storage.SetEntryTypeValue("string/paranth", Value.MakeString("M\"Q"));
            storage.SetEntryTypeValue("raw/empty", Value.MakeRaw());
            storage.SetEntryTypeValue("raw/normal", Value.MakeRaw(Encoding.UTF8.GetBytes("hello")));
            storage.SetEntryTypeValue("raw/special",
                                      Value.MakeRaw(Encoding.UTF8.GetBytes(s)));
            storage.SetEntryTypeValue("raw/speciallarge",
                                      Value.MakeRaw(Encoding.UTF8.GetBytes(specialLargeDigits)));
            storage.SetEntryTypeValue("booleanarr/empty",
                                      Value.MakeBooleanArray());
            storage.SetEntryTypeValue("booleanarr/one",
                                      Value.MakeBooleanArray(true));
            storage.SetEntryTypeValue("booleanarr/two",
                                      Value.MakeBooleanArray(true, false));
            storage.SetEntryTypeValue("doublearr/empty",
                                      Value.MakeDoubleArray());
            storage.SetEntryTypeValue("doublearr/one",
                                      Value.MakeDoubleArray(0.5));
            storage.SetEntryTypeValue(
                "doublearr/two",
                Value.MakeDoubleArray(0.5, -0.25));
            storage.SetEntryTypeValue(
                "stringarr/empty", Value.MakeStringArray());
            storage.SetEntryTypeValue(
                "stringarr/one",
                Value.MakeStringArray("hello"));
            storage.SetEntryTypeValue(
                "stringarr/two",
                Value.MakeStringArray("hello", "world\n"));
            storage.SetEntryTypeValue(s,
                                      Value.MakeBoolean(true));
            storage.SetEntryTypeValue(specialLargeDigits,
                                      Value.MakeBoolean(false));
            outgoing.Clear();
        }

        [Test]
        public void TestStoragePersistentSavePersistentEmpty()
        {
            using (var s = new MemoryStream())
            {
                storage.SavePersistent(s, false);
                s.Position = 0;
                using (var r = new StreamReader(s))
                {
                    Assert.That(r.ReadToEnd(), Is.EqualTo("[NetworkTables Storage 3.0]\n"));
                }
            }
        }

        [Test]
        public void TestStoragePersistentSavePersistent()
        {
            foreach (var entry in Entries)
            {
                entry.Value.Flags = EntryFlags.Persistent;
            }

            using (var s = new MemoryStream())
            {
                storage.SavePersistent(s, false);
                s.Position = 0;
                string o = "";
                using (var r = new StreamReader(s))
                {
                    o = r.ReadToEnd();
                }
                var rem = o;
                string[] split = rem.Split(new[] { '\n' }, 2);
                var line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("[NetworkTables Storage 3.0]"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"\\x00\\x03\\x05\\n\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"\\x00\\xAE\\xFF\\n\"=false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"boolean/false\"=false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("boolean \"boolean/true\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/one\"=true"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array boolean \"booleanarr/two\"=true,false"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/big\"=130000000"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/neg\"=-1.5"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("double \"double/zero\"=0"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/one\"=0.5"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array double \"doublearr/two\"=0.5,-0.25"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/normal\"=aGVsbG8="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/special\"=AAMFCg=="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("raw \"raw/speciallarge\"=AMKuw78K"));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/empty\"=\"\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/normal\"=\"hello\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/paranth\"=\"M\\\"Q\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/special\"=\"\\x00\\x03\\x05\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("string \"string/speciallarge\"=\"\\x00\\xAE\\xFF\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/empty\"="));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/one\"=\"hello\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                rem = split[1];
                Assert.That(line, Is.EqualTo("array string \"stringarr/two\"=\"hello\",\"world\\n\""));
                split = rem.Split(new[] { '\n' }, 2);
                line = split[0];
                Assert.That(line, Is.EqualTo(""));
            }
        }
    }
}
