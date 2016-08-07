using System.Net;
using System.Net.Sockets;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TcpAcceptor : INetworkAcceptor
    {
        private NtTcpListener m_server;

        private readonly int m_port;
        private readonly string m_address;

        private bool m_shutdown;
        private bool m_listening;

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

            var address = !string.IsNullOrEmpty(m_address) ? IPAddress.Parse(m_address) : IPAddress.Any;

            m_server = new NtTcpListener(address, m_port);

            try
            {
                m_server.Start(5);
            }
            catch (SocketException ex)
            {
                Error($"TcpListener Start(): failed {ex.SocketErrorCode}");
                return (int)ex.SocketErrorCode;
            }

            m_listening = true;
            return 0;
        }

        public void Shutdown()
        {
            m_shutdown = true;

            //Force wakeup with non-blocking connect to ourselves
            var address = !string.IsNullOrEmpty(m_address) ? IPAddress.Parse(m_address) : IPAddress.Loopback;

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

        public IClient Accept()
        {
            if (!m_listening || m_shutdown) return null;

            SocketError error;
            Socket socket = m_server.Accept(out error);
            if (socket == null)
            {
                if (!m_shutdown) Error($"Accept() failed: {error}");
                return null;
            }
            if (m_shutdown)
            {
                socket.Dispose();
                return null;
            }
            return new NtTcpClient(socket);
        }
    }
}
