using System.Threading;
using NetworkTables.Core.Native;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestNativeListeners : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            DeleteAllWithPersistent();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            CoreMethods.StopNotifier();
        }

        [Test]
        public void TestAddEntryListener()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyLocal;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                recievedValue = value;
                receivedFlags = flags;
            }, f);

            string toWrite2 = "NewNumber";
            CoreMethods.SetEntryString(key1, toWrite2);

            Thread.Sleep(20);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(recievedValue, Is.Not.Null);

            Assert.That(recievedValue.IsString());
            string retValue = recievedValue.GetString();
            Assert.That(retValue, Is.EqualTo(toWrite2));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyLocal));

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestAddEntryListenerImmediateNotify()
        {
            string key1 = "testKey";
            string toWrite1 = "written";
            CoreMethods.SetEntryString(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value receivedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                receivedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(20);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(receivedValue, Is.Not.Null);

            Assert.That(receivedValue.IsString());
            string retValue = receivedValue.GetString();
            Assert.That(retValue, Is.Not.Null);
            Assert.That(retValue, Is.EqualTo(toWrite1));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            CoreMethods.RemoveEntryListener(listener);
        }

        [TestCase(true)]
        [TestCase(3.5)]
        [TestCase("MyString")]
        public void TestAddEntryListenerDefaultTypes(object val)
        {
            string key1 = "testKey";

            if (val is double) CoreMethods.SetEntryDouble(key1, (double)val);
            else if (val is bool)
                CoreMethods.SetEntryBoolean(key1, (bool)val);
            else if (val is string) CoreMethods.SetEntryString(key1, (string)val);

            int count = 0;
            string recievedKey = "";
            Value recievedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
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
                double retValue = recievedValue.GetDouble();
                Assert.That(retValue, Is.EqualTo((double)val));
                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else if (val is bool)
            {
                Assert.That(recievedValue.IsBoolean());
                bool retValue = recievedValue.GetBoolean();
                Assert.That(retValue, Is.EqualTo((bool)val));

                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else if (val is string)
            {
                Assert.That(recievedValue.IsString());
                string retValue = recievedValue.GetString();
                Assert.That(retValue, Is.EqualTo((string)val));

                Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));
            }
            else
            {
                CoreMethods.RemoveEntryListener(listener);
                Assert.Fail("Unknown type");
                return;
            }

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerStringArray()
        {
            string key1 = "testKey";
            string[] toWrite1 = { "write1", "write2", "write3"};
            CoreMethods.SetEntryStringArray(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value receivedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                receivedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(receivedValue, Is.Not.Null);

            Assert.That(receivedValue.IsStringArray());
            string[] retValue = receivedValue.GetStringArray();
            Assert.That(retValue, Is.Not.Null);

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }
            //Assert.That(retValue, Is.EqualTo(toWrite1));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerRaw()
        {
            string key1 = "testKey";
            byte[] toWrite1 = { 56, 88, 92, 0, 0, 1 };
            CoreMethods.SetEntryRaw(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value receivedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                receivedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(receivedValue, Is.Not.Null);

            Assert.That(receivedValue.IsRaw());
            byte[] retValue = receivedValue.GetRaw();
            Assert.That(retValue, Is.Not.Null);

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerDoubleArray()
        {
            string key1 = "testKey";
            double[] toWrite1 = { 3.58, 6.825, 454.54};
            CoreMethods.SetEntryDoubleArray(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value receivedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                receivedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(receivedValue, Is.Not.Null);

            Assert.That(receivedValue.IsDoubleArray());
            double[] retValue = receivedValue.GetDoubleArray();
            Assert.That(retValue, Is.Not.Null);

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }
            //Assert.That(retValue, Is.EqualTo(toWrite1));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestEntryListenerBooleanArray()
        {
            string key1 = "testKey";
            bool[] toWrite1 = { true, true, true };
            CoreMethods.SetEntryBooleanArray(key1, toWrite1);

            int count = 0;
            string recievedKey = "";
            Value receivedValue = null;
            NotifyFlags receivedFlags = 0;

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (true)
                f |= NotifyFlags.NotifyImmediate;

            int listener = CoreMethods.AddEntryListener(key1, (uid, key, value, flags) =>
            {
                count++;
                recievedKey = key;
                receivedValue = value;
                receivedFlags = flags;
            }, f);

            Thread.Sleep(300);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(recievedKey, Is.EqualTo(key1));
            Assert.That(receivedValue, Is.Not.Null);

            Assert.That(receivedValue.IsBooleanArray());
            bool[] retValue = receivedValue.GetBooleanArray();
            Assert.That(retValue, Is.Not.Null);

            for (int i = 0; i < retValue.Length; i++)
            {
                Assert.That(retValue[i], Is.EqualTo(toWrite1[i]));
            }
            //Assert.That(retValue, Is.EqualTo(toWrite1));

            Assert.That(receivedFlags.HasFlag(NotifyFlags.NotifyImmediate));

            CoreMethods.RemoveEntryListener(listener);
        }

        [Test]
        public void TestAddRemoveConnectionListener()
        {
            ConnectionListenerCallback callback = (uid, connected, conn) =>
            {

            };

            int id = CoreMethods.AddConnectionListener(callback, true);

            Assert.That(id, Is.Not.EqualTo(0));

            CoreMethods.RemoveConnectionListener(id);
        }
    }
}
