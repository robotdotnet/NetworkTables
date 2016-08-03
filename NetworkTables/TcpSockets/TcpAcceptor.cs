using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkTables.Streams;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TcpAcceptor : INetworkAcceptor
    {
        private NtTcpListener m_server;

        private readonly int m_port;
        private readonly string m_address;

        private bool m_shutdown = false;
        private bool m_listening = false;

        public TcpAcceptor(int port, string address)
        {
            m_port = port;
            m_address = address;
        }

        public void Dispose()
        {
            if (m_server != null)
            {
                Shutdown();
            }
        }

        public int Start()
        {
            if (m_listening) return 0;

            IPAddress address = null;
            if (!string.IsNullOrEmpty(m_address))
            {
                address = IPAddress.Parse(m_address);
            }
            else
            {
                address = IPAddress.Any;
            }

            m_server = new NtTcpListener(address, m_port);

            try
            {
                m_server.Start(5);
            }
            catch (SocketException ex)
            {
                Error($"TcpListener Start(): failed {ex.SocketErrorCode}");
                return ex.NativeErrorCode;
            }

            m_listening = true;
            return 0;
        }

        public void Shutdown()
        {
            m_shutdown = true;

            //Force wakeup with non-blocking connect to ourselves
            IPAddress address = null;
            if (!string.IsNullOrEmpty(m_address))
            {
                address = IPAddress.Parse(m_address);
            }
            else
            {
                address = IPAddress.Loopback;
            }

            Socket connectSocket;
            try
            {
                connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (SocketException)
            {
                return;
            }

            connectSocket.Blocking = false;

            try
            {
                connectSocket.Connect(address, m_port);
                connectSocket.Dispose();
            }
            catch (SocketException)
            {
            }

            m_listening = false;
            m_server?.Stop();
            m_server = null;
        }

        public Task<NtTcpClient> Accept()
        {
            if (!m_listening || m_shutdown) return null;

            return m_server.AcceptTcpClientAsync();
        }
    }
}
