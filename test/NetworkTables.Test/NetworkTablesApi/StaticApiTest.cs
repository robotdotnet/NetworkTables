using System;
using NUnit.Framework;

namespace NetworkTables.Test.NetworkTablesApi
{
    [TestFixture]
    public class StaticApiTest
    {
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            NetworkTable.Shutdown();
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
            NetworkTable.Shutdown();
            NetworkTable.SetIPAddress("localhost");
            NetworkTable.SetPersistentFilename(NetworkTable.DefaultPersistentFileName);
        }

        [SetUp]
        public void Setup()
        {
            NetworkTable.Shutdown();
            NetworkTable.SetIPAddress("localhost");
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
            Assert.That(NetworkTable.GetIPAddresses()[0], Is.EqualTo("roboRIO-1234-FRC.local"));
        }

        [Test]
        public void TestSetIpAddress()
        {
            NetworkTable.SetIPAddress("127.0.0.1");
            NetworkTable.SetIPAddress("10.12.34.2");
            Assert.That(NetworkTable.GetIPAddresses()[0], Is.EqualTo("10.12.34.2"));
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

        [Test]
        public void TestLeadingSlash()
        {
            NetworkTable.GlobalDeleteAll();
            var nt = NetworkTable.GetTable("leadingslash");
            var nt2 = NetworkTable.GetTable("/leadingslash");
            Assert.That(nt.ContainsKey("testkey"), Is.False);
            nt2.PutNumber("testkey", 5);
            Assert.That(nt.ContainsKey("testkey"), Is.True);
        }
    }
}
