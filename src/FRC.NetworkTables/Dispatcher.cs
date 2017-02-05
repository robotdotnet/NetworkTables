using System.Collections.Generic;
using System.Net;
using System.Threading;
using NetworkTables.Interfaces;
using NetworkTables.TcpSockets;
using NetworkTables.Logging;

namespace NetworkTables
{
    internal class Dispatcher : DispatcherBase, IServerOverridable
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


        public void SetServer(string serverName, int port)
        {
            SetConnector(() => TcpConnector.Connect(serverName, port, Logger.Instance, 1));
        }

        public void SetServer(IList<NtIPAddress> servers)
        {
            List<Connector> connectors = new List<Connector>();
            foreach (var server in servers)
            {
                connectors.Add(() => TcpConnector.Connect(server.IpAddress, server.Port, Logger.Instance, 1));
            }
            SetConnector(connectors);
        }

        public void SetServerOverride(IPAddress address, int port)
        {
            SetConnectorOverride(() => TcpConnector.Connect(address.ToString(), port, Logger.Instance, 1));
        }

        public void ClearServerOverride()
        {
            ClearConnectorOverride();
        }
    }
}
