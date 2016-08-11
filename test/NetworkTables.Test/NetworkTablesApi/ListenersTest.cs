using System;
using System.Threading;
using NetworkTables.Tables;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTablesApi
{
    internal class MockTableListener : ITableListener
    {
        public ITable Source = null;
        public string Key = null;
        public Value Value = null;
        public NotifyFlags Flags = 0;

        public void ValueChanged(ITable source, string key, Value value, NotifyFlags flags)
        {
            Source = source;
            Key = key;
            Value = value;
            Flags = flags;
        }
    }

    internal class MockConnectionListener : IRemoteConnectionListener
    {
        public IRemote ConnectedRemote = null;
        public IRemote DisconnectedRemote = null;

        public void Connected(IRemote remote, ConnectionInfo info)
        {
            ConnectedRemote = remote;
        }

        public void Disconnected(IRemote remote, ConnectionInfo info)
        {
            DisconnectedRemote = remote;
        }
    }



    [TestFixture(true)]
    [TestFixture(false)]
    public class ListenersTest
    {
        public ListenersTest(bool server)
        {
            NetworkTable.SetIPAddress("localhost");
            NetworkTable.SetPort(10000);
            if (server)
            {
                NetworkTable.SetServerMode();
            }
            else
            {
                NetworkTable.SetClientMode();
            }

            NetworkTable.Initialize();
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            NetworkTable.Shutdown();
        }

        private NetworkTable m_table;

        [SetUp]
        public void SetUp()
        {
            TestBase.DeleteAllWithPersistent();
            m_table = NetworkTable.GetTable("Table");
        }

        [Test]
        public void TestAddTableListenerExListenerFlags()
        {
            MockTableListener listener = new MockTableListener();

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal | NotifyFlags.NotifyImmediate;

            string key = "key";
            string value = "Value";

            m_table.PutString(key, value);

            m_table.AddTableListenerEx(listener, f);

            Thread.Sleep(20);

            Assert.That(listener.Source, Is.EqualTo(m_table));
            Assert.That(listener.Key, Is.EqualTo(key));
            Assert.That(listener.Value.IsString());
            Assert.That(listener.Value.GetString(), Is.EqualTo(value));
            Assert.That(listener.Flags, Is.EqualTo(NotifyFlags.NotifyImmediate));

            string key2 = "key2";
            string val2 = "value2";

            m_table.PutString(key2, val2);

            Thread.Sleep(20);

            Assert.That(listener.Source, Is.EqualTo(m_table));
            Assert.That(listener.Key, Is.EqualTo(key2));
            Assert.That(listener.Value.IsString());
            Assert.That(listener.Value.GetString(), Is.EqualTo(val2));
            Assert.That(listener.Flags, Is.EqualTo(NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddTableListenerExKeyListenerFlags()
        {
            MockTableListener listener = new MockTableListener();

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal | NotifyFlags.NotifyImmediate;

            string key = "key";
            string value = "Value";

            m_table.PutString(key, value);

            m_table.AddTableListenerEx(key, listener, f);

            m_table.PutString("Key2", "Value2");

            Thread.Sleep(20);

            Assert.That(listener.Source, Is.EqualTo(m_table));
            Assert.That(listener.Key, Is.EqualTo(key));
            Assert.That(listener.Value.IsString());
            Assert.That(listener.Value.GetString(), Is.EqualTo(value));
            Assert.That(listener.Flags, Is.EqualTo(NotifyFlags.NotifyImmediate));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddSubTableListenerListenerLocalNotify()
        {
            MockTableListener listener = new MockTableListener();

            string subTableName = "SubTable";

            ITable subTable = m_table.GetSubTable("SubTable");

            m_table.AddSubTableListener(listener, true);

            string key = "key";
            string value = "Value";

            subTable.PutString(key, value);

            Thread.Sleep(20);

            Assert.That(listener.Source, Is.EqualTo(m_table));
            Assert.That(listener.Key, Is.EqualTo(subTableName));
            Assert.That(listener.Value, Is.Null);
            Assert.That(listener.Flags, Is.EqualTo(NotifyFlags.NotifyLocal | NotifyFlags.NotifyNew));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddTableListenerEntireTableImmediateNotify()
        {
            MockTableListener listener = new MockTableListener();

            m_table.AddTableListener(listener, true);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerEntireTable()
        {
            MockTableListener listener = new MockTableListener();

            m_table.AddTableListener(listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerKeyListenerImmediateNotify()
        {
            MockTableListener listener = new MockTableListener();

            m_table.AddTableListener("key", listener, true);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerKeyListener()
        {
            MockTableListener listener = new MockTableListener();

            m_table.AddTableListener("key", listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddSubTableListenerNotLocal()
        {
            MockTableListener listener = new MockTableListener();

            m_table.AddSubTableListener(listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerExListenerFlagsDelegate()
        {
            ITable Source = null;
            string Key = null;
            Value Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, Value, NotifyFlags> listener = (s, k, v, _f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = _f;
            };


            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal | NotifyFlags.NotifyImmediate;

            string key = "key";
            string value = "Value";

            m_table.PutString(key, value);

            m_table.AddTableListenerEx(listener, f);

            Thread.Sleep(20);

            Assert.That(Source, Is.EqualTo(m_table));
            Assert.That(Key, Is.EqualTo(key));
            Assert.That(Value.IsString());
            Assert.That(Value.GetString(), Is.EqualTo(value));
            Assert.That(Flags, Is.EqualTo(NotifyFlags.NotifyImmediate));

            string key2 = "key2";
            string val2 = "value2";

            m_table.PutString(key2, val2);

            Thread.Sleep(20);

            Assert.That(Source, Is.EqualTo(m_table));
            Assert.That(Key, Is.EqualTo(key2));
            Assert.That(Value.IsString());
            Assert.That(Value.GetString(), Is.EqualTo(val2));
            Assert.That(Flags, Is.EqualTo(NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddTableListenerExKeyListenerFlagsDelegate()
        {
            ITable Source = null;
            string Key = null;
            Value Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, Value, NotifyFlags> listener = (s, k, v, _f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = _f;
            };

            NotifyFlags f = NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal | NotifyFlags.NotifyImmediate;

            string key = "key";
            string value = "Value";

            m_table.PutString(key, value);

            m_table.AddTableListenerEx(key, listener, f);

            m_table.PutString("Key2", "Value2");

            Thread.Sleep(20);

            Assert.That(Source, Is.EqualTo(m_table));
            Assert.That(Key, Is.EqualTo(key));
            Assert.That(Value.IsString());
            Assert.That(Value.GetString(), Is.EqualTo(value));
            Assert.That(Flags, Is.EqualTo(NotifyFlags.NotifyImmediate));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddSubTableListenerListenerLocalNotifyDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            string subTableName = "SubTable";

            ITable subTable = m_table.GetSubTable("SubTable");

            m_table.AddSubTableListener(listener, true);

            string key = "key";
            string value = "Value";

            subTable.PutString(key, value);

            Thread.Sleep(20);

            Assert.That(Source, Is.EqualTo(m_table));
            Assert.That(Key, Is.EqualTo(subTableName));
            Assert.That(Value, Is.Null);
            Assert.That(Flags, Is.EqualTo(NotifyFlags.NotifyLocal | NotifyFlags.NotifyNew));

            m_table.RemoveTableListener(listener);
        }

        [Test]
        public void TestAddTableListenerEntireTableImmediateNotifyDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            m_table.AddTableListener(listener, true);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerEntireTableDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            m_table.AddTableListener(listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerKeyListenerImmediateNotifyDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            m_table.AddTableListener("key", listener, true);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddTableListenerKeyListenerDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            m_table.AddTableListener("key", listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }

        [Test]
        public void TestAddSubTableListenerNotLocalDelegate()
        {
            ITable Source = null;
            string Key = null;
            object Value = null;
            NotifyFlags Flags = 0;

            Action<ITable, string, object, NotifyFlags> listener = (s, k, v, f) =>
            {
                Source = s;
                Key = k;
                Value = v;
                Flags = f;
            };

            m_table.AddSubTableListener(listener);

            Thread.Sleep(20);

            m_table.RemoveTableListener(listener);

            Assert.Pass();
        }


        [Test]
        public void TestConnectionListener()
        {
            MockConnectionListener listener = new MockConnectionListener();

            m_table.AddConnectionListener(listener, true);

            Thread.Sleep(20);
            m_table.RemoveConnectionListener(listener);

            Assert.Pass("If we have gotten here without an exception we pass. Cannot test more without a remote.");
        }
    }
}