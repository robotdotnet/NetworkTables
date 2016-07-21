// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal class NtTcpListener
    {
        private IPEndPoint m_serverSocketEP;
        private Socket m_serverSocket;
        private bool m_active;
        private bool m_exclusiveAddressUse;

        public NtTcpListener(IPAddress localaddr, int port)
        {
            if (localaddr == null)
            {
                throw new ArgumentNullException(nameof(localaddr));
            }
            m_serverSocketEP = new IPEndPoint(localaddr, port);
            m_serverSocket = new Socket(m_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            Start((int) SocketOptionName.MaxConnections);
        }

        public void Start(int backlog)
        {
            if (backlog > (int) SocketOptionName.MaxConnections || backlog < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(backlog));
            }

            if (m_serverSocket == null)
            {
                throw new InvalidOperationException("Invalid Socket Handle");
            }

            if (m_active)
            {
                return;
            }

            m_serverSocket.Bind(m_serverSocketEP);
            try
            {
                m_serverSocket.Listen(backlog);
            }
            catch (SocketException)
            {
                Stop();
                throw;
            }

            m_active = true;
        }

        public void Stop()
        {
            if (m_serverSocket != null)
            {
                m_serverSocket.Dispose();
                m_serverSocket = null;
            }
            m_active = false;
            m_serverSocket = new Socket(m_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (m_exclusiveAddressUse)
            {
                m_serverSocket.ExclusiveAddressUse = true;
            }
        }

        public Socket Accept(out SocketError errorCode)
        {
            Socket socket = null;
            try
            {
                socket = m_serverSocket.Accept();
            }
            catch (SocketException ex)
            {
                errorCode = ex.SocketErrorCode;
                return null;
            }
            errorCode = 0;
            return socket;
        }
    }
}
