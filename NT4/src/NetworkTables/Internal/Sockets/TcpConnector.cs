using NetworkTables.Internal.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Internal.Sockets
{
    public static class TcpConnector
    {
        public static TcpClient ConnectParallel(IList<(string ip, int port)> conns, Logger logger, double timeout = 5.0)
        {
            return ConnectParallelAsync(conns, logger, timeout).Result;
        }

        public static Task<TcpClient> ConnectParallelAsync(IList<(string ip, int port)> conns, Logger logger,double timeout = 5.0)
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

            var delayTask = Task.Delay(TimeSpan.FromSeconds(timeout));
            tasks.Add(delayTask);

            async Task<TcpClient> ConnectAsyncInternal()
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
                        return client;
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
