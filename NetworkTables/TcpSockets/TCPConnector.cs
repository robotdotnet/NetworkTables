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

                var addressEntry = Dns.GetHostEntry(hostName);
                addr = addressEntry.AddressList;

            }
            catch (SocketException e)
            {
                addr = null;
                return e.NativeErrorCode;
            }
            return 0;
        }

        public static NtNetworkStream Connect(string server, int port, int timeout = 0)
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
                    client.Close();
                    return null;
                }
                return new NtNetworkStream(client.Client);
            }

            //Connect with time limit
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
                    client.Close();
                    return null;
                }
                //Connected
                if (client.Connected)
                {
                    return new NtNetworkStream(client.Client);
                }
                Error($"Timeout connect to {server} port {port} did not connect properly.");
                return null;
            }
            catch (SocketException ex)
            {
                //Failed to connect
                Error($"Connect()  to {server} port {port} error {ex.NativeErrorCode} - {ex.SocketErrorCode}");
                client.Close();
                return null;
            }
        }
    }
}
