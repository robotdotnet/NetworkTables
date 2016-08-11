// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.TcpSockets
{
    internal class NtTcpClient : IDisposable, IClient
    {
        private Socket m_clientSocket;
        private NetworkStream m_dataStream;
        private bool m_cleanedUp;
        private bool m_active;

        public NtTcpClient() : this(AddressFamily.InterNetwork) { }

        public bool NoDelay
        {
            get { return m_clientSocket.NoDelay; }
            set { m_clientSocket.NoDelay = value; }
        }

        public EndPoint RemoteEndPoint => m_clientSocket.RemoteEndPoint;

        public NtTcpClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Invalid TCP Family", nameof(family));
            }

            m_clientSocket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
        }

        internal NtTcpClient(Socket acceptedSocket)
        {
            m_clientSocket = acceptedSocket;
            m_active = true;
        }

        public bool Active
        {
            get { return m_active; }
            set { m_active = value; }
        }

        public Stream GetStream()
        {
            if (m_cleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (!m_clientSocket.Connected)
            {
                throw new InvalidOperationException("Not Connected");
            }
            return m_dataStream ?? (m_dataStream = new NetworkStream(m_clientSocket, true));
        }

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            m_clientSocket.Connect(ipAddresses, port);
            m_active = true;
        }

        public bool ConnectWithTimeout(IPAddress[] ipAddresses, int port, int timeout)
        {
            if (ipAddresses == null)
                throw new ArgumentNullException(nameof(ipAddresses), "IP Addresses cannot be null");
            if (ipAddresses.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(ipAddresses), "IP Adresses must have values internally");

            IPEndPoint ipEp = new IPEndPoint(ipAddresses[0], port);
            bool isProperlySupported = RuntimeDetector.GetRuntimeHasProperSockets();
            try
            {
                m_clientSocket.Blocking = false;
                if (isProperlySupported)
                {
                    m_clientSocket.Connect(ipAddresses, port);
                }
                else
                {
                    m_clientSocket.Connect(ipEp);
                }
                //We have connected
                m_active = true;
                return true;

            }
            catch (SocketException ex)
            {

                if (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.InProgress)
                {
                    DateTime waitUntil = DateTime.UtcNow + TimeSpan.FromSeconds(timeout);
                    try
                    {
                        while (true)
                        {
                            if (m_clientSocket.Poll(1000, SelectMode.SelectWrite))
                            {
                                if (!isProperlySupported)
                                    m_clientSocket.Connect(ipEp);
                                // We have connected
                                m_active = true;
                                return true;
                            }
                            else
                            {
                                if (DateTime.UtcNow >= waitUntil)
                                {
                                    // We have timed out
                                    Info($"Connect() to {ipAddresses[0]} port {port} timed out");
                                    break;
                                }
                            }
                        }
                    }
                    catch (SocketException ex2)
                    {
                        if (!isProperlySupported)
                        {
                            if (ex2.SocketErrorCode == SocketError.IsConnected)
                            {
                                m_active = true;
                                return true;
                            }
                        }
                        Error($"Select() to {ipAddresses[0]} port {port} error {ex2.SocketErrorCode}");
                    }
                }
                else
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        // A connection refused is an unexceptional case
                        Info($"Connect() to {ipAddresses[0]} port {port} timed out");
                    }
                    Error($"Connect() to {ipAddresses[0]} port {port} error {ex.SocketErrorCode}");
                }

            }
            finally
            {
                m_clientSocket.Blocking = true;
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_cleanedUp)
            {
                return;
            }

            if (disposing)
            {
                IDisposable dataStream = m_dataStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                else
                {
                    Socket chkClientSocket = m_clientSocket;
                    if (chkClientSocket != null)
                    {
                        try
                        {
                            chkClientSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch (SocketException) { } // Ignore any socket exception
                        finally
                        {
                            chkClientSocket.Dispose();
                            m_clientSocket = null;
                        }
                    }
                }

                GC.SuppressFinalize(this);
            }

            m_cleanedUp = true;
        }

        ~NtTcpClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Connected => m_clientSocket.Connected;
    }

}
