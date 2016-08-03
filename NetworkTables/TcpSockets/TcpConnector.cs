using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

        public static async Task<NtTcpClient> Connect(string server, int port, CancellationToken token, TimeSpan timeout)
        {
            return await Task.Run( async () =>
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
                
                bool connectedWithTimeout = await client.ConnectWithTimeout(addr, port, token, timeout);
                if (!connectedWithTimeout)
                {
                    ((IDisposable) client).Dispose();
                    return null;
                }
                return client;
            }, token);
        }
    }
}
