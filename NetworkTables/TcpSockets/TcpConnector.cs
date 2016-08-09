using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
                List<IPAddress> addresses = new List<IPAddress>();
                foreach (var ipAddress in entries.Result)
                {
                    // Only allow IPV4 addresses for now
                    // Sockets don't all support IPV6
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addresses.Add(ipAddress);
                    }
                }
                addr = addresses.ToArray();

            }
            catch (SocketException e)
            {
                addr = null;
                return (int)e.SocketErrorCode;
            }
            return 0;
        }

        public static TcpClient Connect(string server, int port, CancellationToken token, int timeout = -1)
        {
            IPAddress[] addr;
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

            TcpClient client = new TcpClient(AddressFamily.InterNetwork);

            try
            {
                var task = client.ConnectAsync(addr, port);
                bool success = task.Wait(timeout, token);
                if (success && task.IsCompleted)
                {
                    return client;
                }
            }
            catch (AggregateException)
            {
                // TODO: Figure out how to handle this
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            ((IDisposable)client).Dispose();
            return null;


            /*
            //Create out client
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
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
            */
        }
    }
}
