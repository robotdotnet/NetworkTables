using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetworkTables.Test.NtCoreTests
{
    [TestFixture]
    public class ListenersTest
    {
        [SetUp]
        public void Setup()
        {
            TestBase.DeleteAllWithPersistent();
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            NtCore.StopNotifier();
        }

        [Test]
        public void TestAddEntryListener()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            NtCore.SetEntryValue(key1, Value.MakeString(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyLocal;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            string toWrite2 = "NewNumber";
            NtCore.SetEntryValue(key1, Value.MakeString(toWrite2));

            Thread.Sleep(20);

            Assert.That(count, Is.GreaterThanOrEqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsString);

            var retValue = recievedValue.GetString();
            Assert.That(retValue, Is.Not.Null);
            Assert.That(retValue, Is.EqualTo(toWrite2));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyLocal));

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestAddEntryListenerImmediateNotify()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            NtCore.SetEntryValue(key1, Value.MakeString(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(20);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsString);

            var retValue = recievedValue.GetString();
            Assert.That(retValue, Is.Not.Null);
            Assert.That(retValue, Is.EqualTo(toWrite1));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            NtCore.RemoveEntryListener(listener);
        }

        [TestCase(true)]
        [TestCase(3.5)]
        [TestCase("MyString")]
        public void TestAddEntryListenerDefaultTypes(object val)
        {
            string key1 = "testKey";

            if (val is double) NtCore.SetEntryValue(key1, Value.MakeDouble((double)val));
            else if (val is bool)
                NtCore.SetEntryValue(key1, Value.MakeBoolean((bool)val));
            else if (val is string) NtCore.SetEntryValue(key1, Value.MakeString((string)val));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(20);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            if (val is double)
            {
                Assert.That(recievedValue.IsDouble());

                var retValue = recievedValue.GetDouble();
                Assert.That(retValue, Is.EqualTo((double)val));
                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else if (val is bool)
            {
                Assert.That(recievedValue.IsBoolean());

                var retValue = recievedValue.GetBoolean();
                Assert.That(retValue, Is.EqualTo((bool)val));

                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else if (val is string)
            {
                Assert.That(recievedValue.IsString);

                var retValue = recievedValue.GetString();
                Assert.That(retValue, Is.Not.Null);
                Assert.That(retValue, Is.EqualTo((string)val));

                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else
            {
                NtCore.RemoveEntryListener(listener);
                Assert.Fail("Unknown type");
                return;
            }

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerStringArray()
        {
            string key1 = "testKey";
            string[] toWrite1 = { "write1", "write2", "write3" };
            NtCore.SetEntryValue(key1, Value.MakeStringArray(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsStringArray());

            string[] retValue = recievedValue.GetStringArray();

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerRaw()
        {
            string key1 = "testKey";
            byte[] toWrite1 = { 56, 88, 92, 0, 0, 1 };
            NtCore.SetEntryValue(key1, Value.MakeRaw(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsRaw);

            byte[] retValue = recievedValue.GetRaw();

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerDoubleArray()
        {
            string key1 = "testKey";
            double[] toWrite1 = { 3.58, 6.825, 454.54 };
            NtCore.SetEntryValue(key1, Value.MakeDoubleArray(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsDoubleArray);

            double[] retValue = recievedValue.GetDoubleArray();

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerBooleanArray()
        {
            string key1 = "testKey";
            bool[] toWrite1 = { true, true, true };
            NtCore.SetEntryValue(key1, Value.MakeBooleanArray(toWrite1));

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = NtCore.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsBooleanArray());

            bool[] retValue = recievedValue.GetBooleanArray();

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            NtCore.RemoveEntryListener(listener);
        }

        [Test]
        public void TestAddRemoveConnectionListener()
        {
            ConnectionListenerCallback callback = (uid, connected, conn) =>
            {

            };

            int id = NtCore.AddConnectionListener(callback, true);

            Assert.That(id, Is.Not.EqualTo(0));

            NtCore.RemoveEntryListener(id);
        }
    }
}
