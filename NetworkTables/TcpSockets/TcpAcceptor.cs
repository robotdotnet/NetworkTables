using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TcpAcceptor : INetworkAcceptor
    {
        private TcpListener m_server;

        private readonly int m_port;
        private readonly string m_address;

        private bool m_shutdown;
        private bool m_listening;

        private CancellationTokenSource m_tokenSource;

        public TcpAcceptor(int port, string address)
        {
            m_port = port;
            m_address = address;
        }

        public int Start()
        {
            if (m_listening) return 0;

            m_tokenSource = new CancellationTokenSource();

            var address = !string.IsNullOrEmpty(m_address) ? IPAddress.Parse(m_address) : IPAddress.Any;

            m_server = new TcpListener(address, m_port);

            try
            {
                m_server.Start(5);
            }
            catch (SocketException ex)
            {
                Error($"TcpListener Start(): failed {ex.SocketErrorCode}");
                Console.WriteLine(ex.StackTrace);
                return (int)ex.SocketErrorCode;
            }

            m_listening = true;
            return 0;
        }

        public void Shutdown()
        {
            m_shutdown = true;

            m_tokenSource?.Cancel();

            m_server?.Stop();
            m_server = null;
            m_tokenSource = null;
            m_listening = false;
        }

        public TcpClient Accept()
        {
            if (!m_listening || m_shutdown) return null;

            var tokenSource = m_tokenSource;

            if (tokenSource == null || tokenSource.IsCancellationRequested)
                return null;
            try
            {
                var task = m_server.AcceptTcpClientAsync();
                task.Wait(tokenSource.Token);
                if (task.IsCompleted)
                {
                    return task.Result;
                }
                else
                {
                    return null;
                }
            }
            catch (AggregateException)
            {
                // TODO: Figure out how to handle this
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
    }
}
