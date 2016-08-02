using System;
using System.Net;
using System.Net.Sockets;
using NetworkTables.Streams;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.TcpSockets
{
    internal class TcpConnector
    {
        private static int ResolveHostName(string hostName, out IPAddress[] addr)
        {
            try
            {
                var entries = Dns.GetHostAddressesAsync(hostName);
                var success = entries.Wait(1000);
                if (!success)
                {
                    addr = null;
                    return 1;
                }
                addr = entries.Result;

            }
            catch (SocketException e)
            {
                addr = null;
                return e.NativeErrorCode;
            }
            return 0;
        }

        public static NtTcpClient Connect(string server, int port, int timeout = 0)
        {
            IPAddress[] addr = null;
            if (ResolveHostName(server, out addr) != 0)
            {
                try
                {
                    addr = new IPAddress[1];
                    addr[0] = IPAddress.Parse(server);
                }
                catch (FormatException)
                {
                    Error($"could not resolve {server} address");
                    return null;
                }
            }


            //Create out client
            NtTcpClient client = new NtTcpClient(AddressFamily.InterNetwork);
            // No time limit, connect forever
            if (timeout == 0)
            {
                try
                {
                    client.Connect(addr, port);
                }
                catch (SocketException ex)
                {
                    Error($"Connect() to {server} port {port} failed: {ex.SocketErrorCode}");
                    ((IDisposable)client).Dispose();
                    return null;
                }
                return client;
            }

            //Connect with time limit
            bool connectedWithTimeout = client.ConnectWithTimeout(addr, port, timeout);
            if (!connectedWithTimeout)
            {
                ((IDisposable)client).Dispose();
                return null;
            }
            return client;

            /*
            try
            {
                var result = client.BeginConnect(addr, port, null, null);
                if (!result.AsyncWaitHandle.WaitOne(timeout))
                {
                    try
                    {
                        client.EndConnect(result);
                    }
                    catch (SocketException)
                    {
                    }
                    //Timed out
                    Info($"Connect() to {server} port {port} timed out");
                    ((IDisposable)client).Dispose();
                    return null;
                }
                //Connected
                if (client.Connected)
                {
                    return client;
                }
                Error($"Timeout connect to {server} port {port} did not connect properly.");
                return null;
            }
            catch (SocketException ex)
            {
                //Failed to connect
                Error($"Connect()  to {server} port {port} error {ex.NativeErrorCode} - {ex.SocketErrorCode}");
                ((IDisposable)client).Dispose();
                return null;
            }
            */
        }
    }
}
