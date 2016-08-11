// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkTables.TcpSockets
{
    internal class NtTcpListener
    {
        private readonly IPEndPoint m_serverSocketEp;
        private Socket m_serverSocket;
        private bool m_active;


        public NtTcpListener(IPAddress localaddr, int port)
        {
            if (localaddr == null)
            {
                throw new ArgumentNullException(nameof(localaddr));
            }
            m_serverSocketEp = new IPEndPoint(localaddr, port);
            m_serverSocket = new Socket(m_serverSocketEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(int backlog = (int)SocketOptionName.MaxConnections)
        {
            if (backlog < 0)
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
            m_serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            m_serverSocket.Bind(m_serverSocketEp);
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
            m_serverSocket = new Socket(m_serverSocketEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public Socket Accept(out SocketError errorCode)
        {
            Socket socket;
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
