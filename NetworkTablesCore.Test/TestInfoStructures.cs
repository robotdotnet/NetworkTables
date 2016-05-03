using NetworkTables;
using NUnit.Framework;

namespace NetworkTablesCore.Test
{
    [TestFixture]
    public class TestInfoStructures
    {
        [Test]
        public void TestConnectionInfo()
        {
            string remoteId = "remote";
            string remoteName = "remoteName";
            int remotePort = 1234;
            long lastUpdate = 50;
            int protocolVersion = 3;
            ConnectionInfo info = new ConnectionInfo(remoteId, remoteName, remotePort, lastUpdate, protocolVersion);

            Assert.That(info.RemoteId, Is.EqualTo(remoteId));
            Assert.That(info.RemoteName, Is.EqualTo(remoteName));
            Assert.That(info.RemotePort, Is.EqualTo(remotePort));
            Assert.That(info.LastUpdate, Is.EqualTo(lastUpdate));
            Assert.That(info.ProtocolVersion, Is.EqualTo(protocolVersion));
        }

        [Test]
        public void TestEntryInfo()
        {
            string name = "TestEntryInfo";
            NtType type = NtType.Boolean;
            EntryFlags flags = EntryFlags.Persistent;
            long lastChange = 55;

            EntryInfo info = new EntryInfo(name, type, flags, lastChange);

            Assert.That(info.Name, Is.EqualTo(name));
            Assert.That(info.Type, Is.EqualTo(type));
            Assert.That(info.Flags, Is.EqualTo(flags));
            Assert.That(info.LastChange, Is.EqualTo(lastChange));
        }
    }
}
