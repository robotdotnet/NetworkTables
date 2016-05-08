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
        private bool m_exclusiveAddressUse;

        public NtTcpListener(IPAddress localaddr, int port)
        {
            if (localaddr == null)
            {
                throw new ArgumentNullException(nameof(localaddr));
            }
            m_serverSocketEP = new IPEndPoint(localaddr, port);
            Server = new Socket(m_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public Socket Server { get; private set; }

        public bool Active { get; private set; }

        public EndPoint LocalEndpoint => Active ? Server.LocalEndPoint : m_serverSocketEP;

        public bool ExclusiveAddressUse
        {
            get
            {
                return Server.ExclusiveAddressUse;
            }
            set
            {
                if (Active)
                {
                    throw new InvalidOperationException("TcpListener must be stopped to set Exclusive Use");
                }

                Server.ExclusiveAddressUse = value;
                m_exclusiveAddressUse = value;
            }
        }

        public void Start()
        {
            Start((int)SocketOptionName.MaxConnections);
        }

        public void Start(int backlog)
        {
            if (backlog > (int)SocketOptionName.MaxConnections || backlog < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(backlog));
            }

            if (Server == null)
            {
                throw new InvalidOperationException("Invalid Socket Handle");
            }

            if (Active)
            {
                return;
            }

            Server.Bind(m_serverSocketEP);

            try
            {
                Server.Listen(backlog);
            }
            catch (SocketException)
            {
                Stop();
                throw;
            }

            Active = true;
        }

        public void Stop()
        {
            Server?.Dispose();
            Server = null;

            Active = false;
            Server = new Socket(m_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (m_exclusiveAddressUse)
            {
                Server.ExclusiveAddressUse = true;
            }
        }
    }
}
