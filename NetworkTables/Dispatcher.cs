using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables.TcpSockets;

namespace NetworkTables
{
    internal class Dispatcher : DispatcherBase
    {
        private static Dispatcher s_instance;

        private Dispatcher() : this(Storage.Instance, Notifier.Instance)
        {
        }

        public Dispatcher(Storage storage, Notifier notifier)
            : base(storage, notifier)
        {
            
        }

        /// <summary>
        /// Gets the local instance of Dispatcher
        /// </summary>
        public static Dispatcher Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Dispatcher d = new Dispatcher();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        public void StartServer(string persistentFilename, string listenAddress, int port)
        {
            StartServer(persistentFilename, new TcpAcceptor(port, listenAddress));
        }

        public void StartClient(string serverName, int port)
        {
            StartClient((token) => TcpConnector.Connect(serverName, port, token, 1));
        }

        public void StartClient(IList<NtIPAddress> servers)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var server in servers)
            {
                connectors.Add((token) => TcpConnector.Connect(server.IpAddress, server.Port, token, 1));
            }
            StartClient(connectors);
        }
    }
}
