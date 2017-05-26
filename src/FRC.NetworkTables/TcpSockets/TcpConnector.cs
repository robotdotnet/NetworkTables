﻿using NetworkTables.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using static NetworkTables.Logging.Logger;
using Nito.AsyncEx;

namespace NetworkTables.TcpSockets
{
    internal class TcpConnector
    {
        public static TcpClient Connect(IList<(string server, int port)> servers, Logger logger, TimeSpan timeout)
        {
            if (servers.Count == 0)
            {
                return null;
            }

            return AsyncContext.Run(async () => {
                TcpClient toReturn = null;
                var clientTcp = new List<TcpClient>();
                var clientTask = new List<Task>();
                try
                {
                    for (int i = 0; i < servers.Count; i++)
                    {
                        TcpClient client = new TcpClient();
                        Task connectTask = client.ConnectAsync(servers[i].server, servers[i].port);
                        clientTcp.Add(client);
                        clientTask.Add(connectTask);
                    }

                    // 10 second timeout
                    var delayTask = Task.Delay(timeout);

                    clientTask.Add(delayTask);

                    while (clientTcp.Count != 0)
                    {
                        var finished = await Task.WhenAny(clientTask);

                        var index = clientTask.IndexOf(finished);
                        if (finished == delayTask)
                        {
                            return null;
                        }
                        else if (finished.IsCompleted && !finished.IsFaulted && !finished.IsCanceled)
                        {
                            toReturn = clientTcp[index];
                            return toReturn;
                        }
                        var remove = clientTcp[index];
                        clientTcp.RemoveAt(index);
                        remove.Dispose();
                        clientTask.RemoveAt(index);
                    }
                    return null;
                }
                finally
                {
                    for (int i = 0; i < clientTcp.Count; i++)
                    {
                        if (clientTcp[i] != toReturn)
                        {
                            try
                            {
                                clientTcp[i].Dispose();
                            }
                            catch (Exception e)
                            {
                                // Ignore exception
                            }
                        }
                    }
                }
            });
        }

        /*
        public static NtTcpClient Connect(string server, int port, Logger logger, int timeout = 0)
        {
            if (ResolveHostName(server, out IPAddress[] addr) != 0)
            {
                try
                {
                    addr = new IPAddress[1];
                    addr[0] = IPAddress.Parse(server);
                }
                catch (FormatException)
                {
                    Error(logger, $"could not resolve {server} address");
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
                    Error(logger, $"Connect() to {server} port {port.ToString()} failed: {ex.SocketErrorCode}");
                    ((IDisposable)client).Dispose();
                    return null;
                }
                return client;
            }

            //Connect with time limit
            bool connectedWithTimeout = client.ConnectWithTimeout(addr, port, logger, timeout);
            if (!connectedWithTimeout)
            {
                ((IDisposable)client).Dispose();
                return null;
            }
            return client;
        }
        */
    }
}
