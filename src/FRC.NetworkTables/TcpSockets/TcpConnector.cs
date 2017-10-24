using NetworkTables.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using static NetworkTables.Logging.Logger;
using Nito.AsyncEx;
using System.IO;

namespace NetworkTables.TcpSockets
{
    internal class TcpConnector
    {
        public class TcpClientNt : IClient
        {
            private readonly TcpClient m_client;

            internal TcpClientNt(TcpClient client)
            {
                m_client = client;
            }

            public Stream GetStream()
            {
                return m_client.GetStream();
            }
            public EndPoint RemoteEndPoint
            {
                get
                {
                    return m_client.Client.RemoteEndPoint;
                }
            }
            public bool NoDelay
            {
                set
                {
                }
            }

            public void Dispose()
            {
                m_client.Dispose();
            }
        }

        private static void PrintConnectFailList(IList<(string server, int port)> servers, Logger logger)
        {
            Logger.Error(logger, "Failed to connect to the following IP Addresses:");
            foreach (var item in servers)
            {
                Logger.Error(logger, $"    Server: {item.server} Port: {item.port}");
            }
        }

        public static IClient ConnectParallel(IList<(string ip, int port)> conns, Logger logger, TimeSpan timeout)
        {
            return ConnectParallelAsync(conns, logger, timeout).Result;
        }

        public static Task<IClient> ConnectParallelAsync(IList<(string ip, int port)> conns, Logger logger, TimeSpan timeout)
        {
            List<TcpClient> clients = new List<TcpClient>();
            List<Task> tasks = new List<Task>();

            foreach (var item in conns)
            {
                var client = new TcpClient();
                Task connectTask;
                try
                {
                    connectTask = client.ConnectAsync(item.ip, item.port);

                }
                catch (ArgumentOutOfRangeException aore)
                {
                    // TODO: Log
                    Logger.Error(logger, $"Bad argument {aore}");
                    continue;
                }
                catch (SocketException se)
                {
                    // TODO: Log
                    Logger.Warning(logger, $"Socket connect failed {se}");
                    continue;
                }
                clients.Add(client);
                tasks.Add(connectTask);
            }

            var delayTask = Task.Delay(timeout);
            tasks.Add(delayTask);

            async Task<IClient> ConnectAsyncInternal()
            {
                while (tasks.Count > 0)
                {
                    var task = await Task.WhenAny(tasks);
                    if (task == delayTask)
                    {
                        return null;
                    }
                    var index = tasks.IndexOf(task);
                    var client = clients[index];
                    if (client.Connected)
                    {
                        return new TcpClientNt(client);
                    }
                    clients.RemoveAt(index);
                    tasks.RemoveAt(index);
                }
                return null;
            }

            return ConnectAsyncInternal();
        }

    }
}