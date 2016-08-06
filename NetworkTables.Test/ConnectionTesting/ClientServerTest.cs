using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NetworkTables.TcpSockets;
using System.Threading;

namespace NetworkTables.Test.ConnectionTesting
{
    /*
    internal class MockDispatcher : DispatcherBase
    {
        internal MockDispatcher(Storage storage, Notifier notifier)
            : base(storage, notifier)
        {

        }

        public void StartServer(string persistentFilename, string listenAddress, int port)
        {
            StartServer(persistentFilename, new TcpAcceptor(port, listenAddress));
        }

        public void StartClient(string serverName, int port)
        {
            StartClient(() => TcpConnector.Connect(serverName, port, 1));
        }
    }
    */

    [TestFixture]
    public class ClientServerTest
    {
        private RpcServer m_clientRpc;
        private Storage m_clientStorage;
        private Notifier m_clientNotifier;
        private Dispatcher m_clientDispatcher;

        private RpcServer m_serverRpc;
        private Storage m_serverStorage;
        private Notifier m_serverNotifier;
        private Dispatcher m_serverDispatcher;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_clientRpc = new RpcServer();
            m_clientNotifier = new Notifier();
            m_clientStorage = new Storage(m_clientNotifier, m_clientRpc);
            
            m_clientDispatcher = new Dispatcher(m_clientStorage, m_clientNotifier);

            m_clientDispatcher.Identity = "TestClient";

            m_serverRpc = new RpcServer();
            m_serverNotifier = new Notifier();
            m_serverStorage = new Storage(m_serverNotifier, m_serverRpc);

            m_serverDispatcher = new Dispatcher(m_serverStorage, m_serverNotifier);

            m_serverDispatcher.Identity = "TestServer";
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_clientDispatcher.Dispose();
            m_clientStorage.Dispose();
            m_clientNotifier.Dispose();
            m_clientRpc.Dispose();

            m_serverDispatcher.Dispose();
            m_serverStorage.Dispose();
            m_serverNotifier.Dispose();
            m_serverRpc.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            m_clientDispatcher.Stop();
            m_serverDispatcher.Stop();
        }

        [Test]
        public void ServerThenClientConnectionTest()
        {

            m_serverDispatcher.StartServer("", "", 9999);
            Thread.Sleep(500);
            m_clientDispatcher.StartClient("localhost", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(1));
            var serverConn = serverConnections[0];
            Assert.That(serverConn.RemoteId, Is.EqualTo("TestClient"));
            Assert.That(serverConn.ProtocolVersion, Is.EqualTo(0x0300));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(1));
            var clientConn = clientConnections[0];
            Assert.That(clientConn.RemoteId, Is.EqualTo("TestServer"));
            Assert.That(clientConn.ProtocolVersion, Is.EqualTo(0x0300));
        }

        [Test]
        public void ClientThenServerConnectionTest()
        {
            m_clientDispatcher.StartClient("localhost", 9999);
            Thread.Sleep(500);
            m_serverDispatcher.StartServer("", "", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(1));
            var serverConn = serverConnections[0];
            Assert.That(serverConn.RemoteId, Is.EqualTo("TestClient"));
            Assert.That(serverConn.ProtocolVersion, Is.EqualTo(0x0300));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(1));
            var clientConn = clientConnections[0];
            Assert.That(clientConn.RemoteId, Is.EqualTo("TestServer"));
            Assert.That(clientConn.ProtocolVersion, Is.EqualTo(0x0300));
        }

        [Test]
        public void ClientTestNoServer()
        {
            m_clientDispatcher.StartClient("localhost", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(0));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(0));
        }

        [Test]
        public void ServerTestNoClient()
        {
            m_serverDispatcher.StartServer("", "", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(0));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(0));
        }

        [Test]
        public void ServerTestClientDisconnect()
        {
            m_serverDispatcher.StartServer("", "", 9999);
            Thread.Sleep(500);
            m_clientDispatcher.StartClient("localhost", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(1));
            var serverConn = serverConnections[0];
            Assert.That(serverConn.RemoteId, Is.EqualTo("TestClient"));
            Assert.That(serverConn.ProtocolVersion, Is.EqualTo(0x0300));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(1));
            var clientConn = clientConnections[0];
            Assert.That(clientConn.RemoteId, Is.EqualTo("TestServer"));
            Assert.That(clientConn.ProtocolVersion, Is.EqualTo(0x0300));

            m_clientDispatcher.Stop();

            Thread.Sleep(2000);

            serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(0));

            clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(0));
        }

        [Test]
        public void ClientTestServerDisconnect()
        {
            m_serverDispatcher.StartServer("", "", 9999);
            Thread.Sleep(500);
            m_clientDispatcher.StartClient("localhost", 9999);

            Thread.Sleep(2000);

            var serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(1));
            var serverConn = serverConnections[0];
            Assert.That(serverConn.RemoteId, Is.EqualTo("TestClient"));
            Assert.That(serverConn.ProtocolVersion, Is.EqualTo(0x0300));

            var clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(1));
            var clientConn = clientConnections[0];
            Assert.That(clientConn.RemoteId, Is.EqualTo("TestServer"));
            Assert.That(clientConn.ProtocolVersion, Is.EqualTo(0x0300));

            m_serverDispatcher.Stop();

            Thread.Sleep(2000);

            serverConnections = m_serverDispatcher.GetConnections();
            Assert.That(serverConnections, Has.Count.EqualTo(0));

            clientConnections = m_clientDispatcher.GetConnections();
            Assert.That(clientConnections, Has.Count.EqualTo(0));
        }

        [Test]
        public void ServerTestConnectionListener()
        {
            m_serverDispatcher.StartServer("", "", 9999);
            Thread.Sleep(500);
            Notifier notifier = m_serverNotifier;

            int retUid = 16666;
            bool retConnected = false;
            ConnectionInfo retInfo = default(ConnectionInfo);

            // ReSharper disable once UnusedVariable
            int uid = notifier.AddConnectionListener((id, connected, connInfo) =>
            {
                retUid = id;
                retConnected = connected;
                retInfo = connInfo;
            });
            notifier.Start();
            m_clientDispatcher.StartClient("localhost", 9999);

            Thread.Sleep(2000);

            Assert.That(retUid, Is.EqualTo(1));
            Assert.That(retConnected, Is.True);
            Assert.That(retInfo.RemoteId, Is.EqualTo("TestClient"));

            m_clientDispatcher.Stop();

            Thread.Sleep(2000);

            Assert.That(retUid, Is.EqualTo(1));
            Assert.That(retConnected, Is.False);
            Assert.That(retInfo.RemoteId, Is.EqualTo("TestClient"));
        }

        [Test]
        public void ClientTestConnectionListener()
        {
            m_clientDispatcher.StartClient("localhost", 9999);
            
            Thread.Sleep(500);
            Notifier notifier = m_clientNotifier;

            int retUid = 16666;
            bool retConnected = false;
            ConnectionInfo retInfo = default(ConnectionInfo);

            int uid = notifier.AddConnectionListener((id, connected, connInfo) =>
            {
                retUid = id;
                retConnected = connected;
                retInfo = connInfo;
            });
            notifier.Start();

            m_serverDispatcher.StartServer("", "", 9999);
            Thread.Sleep(2000);

            Assert.That(retUid, Is.EqualTo(1));
            Assert.That(retConnected, Is.True);
            Assert.That(retInfo.RemoteId, Is.EqualTo("TestServer"));

            m_serverDispatcher.Stop();

            Thread.Sleep(2000);

            Assert.That(retUid, Is.EqualTo(1));
            Assert.That(retConnected, Is.False);
            Assert.That(retInfo.RemoteId, Is.EqualTo("TestServer"));
        }
    }
}
