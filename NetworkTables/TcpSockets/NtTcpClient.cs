using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal class NtTcpClient : IDisposable
    {
        private bool m_active;
        

        private readonly AddressFamily m_family = AddressFamily.InterNetwork;

        public NtTcpClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Address Family MUST be InterNetwork or InterNetworkV6");
            }

            m_family = family;

            Initialize();
        }

        private void Initialize()
        {
            Client = new Socket(m_family, SocketType.Stream, ProtocolType.Tcp);
            m_active = false;
        }

        public Socket Client { get; private set; }

        public bool Connected => Client.Connected;

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            Client.Connect(ipAddresses, port);
            m_active = true;
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            var result = Client.BeginConnect(addresses, port, requestCallback, state);
            return result;
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            Client.EndConnect(asyncResult);
            m_active = true;
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        private bool m_cleanedUp = false;

        protected virtual void Dispose(bool disposing)
        {
            if (m_cleanedUp)
            {
                return;
            }

            if (disposing)
            {
                Socket chkClientSocket = Client;
                if (chkClientSocket != null)
                {
                    chkClientSocket.Close();
                    Client = null;
                }

                GC.SuppressFinalize(this);
            }

            m_cleanedUp = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~NtTcpClient()
        {
            Dispose(false);
        }
    }

}
