using System.Collections.Generic;
using System.Net;
using System.Threading;
using NetworkTables.Interfaces;
using NetworkTables.TcpSockets;
using NetworkTables.Logging;
using System;

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
            SetConnector(() =>
            {
                return TcpConnector.ConnectParallel(new List<(string server, int port)> { (serverName, port) }, Logger.Instance, TimeSpan.FromSeconds(3));
            });
        }

        public void SetServer(IList<NtIPAddress> servers)
        {
            List<(string server, int port)> addresses = new List<(string server, int port)>(servers.Count);
            foreach (var server in servers)
            {
                addresses.Add((server.IpAddress, server.Port));
            }

            SetConnector(() => TcpConnector.ConnectParallel(addresses, Logger.Instance, TimeSpan.FromSeconds(3)));
        }

        public void SetServerOverride(IPAddress address, int port)
        {
            SetConnectorOverride(() => TcpConnector.ConnectParallel(new List<(string server, int port)> { (address.ToString(), port) }, Logger.Instance, TimeSpan.FromSeconds(3)));
        }

        public void ClearServerOverride()
        {
            ClearConnectorOverride();
        }
    }
}