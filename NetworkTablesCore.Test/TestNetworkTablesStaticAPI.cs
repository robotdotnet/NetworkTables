using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using NUnit.Framework;

namespace NetworkTablesCore.Test
{
    [TestFixture]
    public class TestNetworkTablesStaticApi : TestBase
    {
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            NetworkTable.Shutdown();
        }

        [TestFixtureTearDown]
        public void FixtureTeardown()
        {
            NetworkTable.Shutdown();
            NetworkTable.SetIPAddress("127.0.0.1");
            NetworkTable.SetPersistentFilename(NetworkTable.DefaultPersistentFileName);
            NetworkTable.SetPort(10000);
            NetworkTable.SetServerMode();
            NetworkTable.Initialize();
        }

        [SetUp]
        public void Setup()
        {
            NetworkTable.Shutdown();
            NetworkTable.SetIPAddress("127.0.0.1");
        }

        [Test]
        public void TestDoubleInitializeServer()
        {
            NetworkTable.SetServerMode();
            Assert.That(NetworkTable.Client, Is.False);
            NetworkTable.Initialize();
            Assert.DoesNotThrow(NetworkTable.Initialize);
        }

        [Test]
        public void TestDoubleInitializeClient()
        {
            NetworkTable.SetClientMode();
            Assert.That(NetworkTable.Client, Is.True);
            NetworkTable.Initialize();
            Assert.DoesNotThrow(NetworkTable.Initialize);
        }

        [Test]
        public void TestClientShutdown()
        {
            NetworkTable.SetClientMode();
            NetworkTable.Initialize();
            Assert.That(NetworkTable.Running, Is.True);
            NetworkTable.Shutdown();
            Assert.That(NetworkTable.Running, Is.False);
        }

        [Test]
        public void TestServerShutdown()
        {
            NetworkTable.SetServerMode();
            NetworkTable.Initialize();
            Assert.That(NetworkTable.Running, Is.True);
            NetworkTable.Shutdown();
            Assert.That(NetworkTable.Running, Is.False);
        }

        [Test]
        public void TestSetClientModeWhileStopped()
        {
            NetworkTable.SetServerMode();
            Assert.That(NetworkTable.Client, Is.False);
            NetworkTable.SetClientMode();
            Assert.That(NetworkTable.Client, Is.True);
            NetworkTable.SetClientMode();
            Assert.That(NetworkTable.Client, Is.True);
        }

        [Test]
        public void TestSetClientModeWhileRunningServer()
        {
            NetworkTable.SetServerMode();
            Assert.That(NetworkTable.Client, Is.False);
            NetworkTable.Initialize();
            Assert.Throws<InvalidOperationException>(NetworkTable.SetClientMode);
        }

        [Test]
        public void TestSetTeam()
        {
            NetworkTable.SetTeam(1234);
            Assert.That(NetworkTable.IPAddress, Is.EqualTo("roboRIO-1234-FRC.local"));
        }

        [Test]
        public void TestSetIpAddress()
        {
            NetworkTable.SetIPAddress("127.0.0.1");
            NetworkTable.SetIPAddress("10.12.34.2");
            Assert.That(NetworkTable.IPAddress, Is.EqualTo("10.12.34.2"));
        }

        [Test]
        public void TestGetTableCausesInitialization()
        {
            Assert.That(NetworkTable.Running, Is.False);
            NetworkTable.GetTable("empty");
            Assert.That(NetworkTable.Running, Is.True);
        }

        [Test]
        public void TestSetPersistentFilename()
        {
            NetworkTable.SetPersistentFilename(NetworkTable.DefaultPersistentFileName);
            NetworkTable.SetPersistentFilename(NetworkTable.DefaultPersistentFileName);
            Assert.That(NetworkTable.PersistentFilename, Is.EqualTo(NetworkTable.DefaultPersistentFileName));
            NetworkTable.SetPersistentFilename("TestFile.txt");
            Assert.That(NetworkTable.PersistentFilename, Is.EqualTo("TestFile.txt"));
            NetworkTable.SetPersistentFilename(NetworkTable.DefaultPersistentFileName);
            Assert.That(NetworkTable.PersistentFilename, Is.EqualTo(NetworkTable.DefaultPersistentFileName));
        }
    }
}
