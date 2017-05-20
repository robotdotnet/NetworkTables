using NetworkTables.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Text;
using static NetworkTables.Logging.Logger;
using System.Net;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace NetworkTables
{
    internal class DsClient : IDisposable
    {
        private static DsClient s_instance;

        public static DsClient Instance
        {
            get
            {
                if (s_instance == null)
                {
                    DsClient d = new DsClient();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        //private IServerOverridable m_serverOverridable;

        public DsClient(/*IServerOverridable serverOverridable*/)
        {
            //m_serverOverridable = serverOverridable;
        }

        private int m_port;

        private Task m_task;
        private CancellationTokenSource m_tokenSource;

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            m_tokenSource?.Cancel();
            m_task?.WaitAndUnwrapException();
            m_task = null;
            m_tokenSource = null;
        }

        public void Start(int port)
        {
            Interlocked.Exchange(ref m_port, port);
            if (m_task == null)
            {
                if (m_tokenSource == null || m_tokenSource.IsCancellationRequested)
                {
                    m_tokenSource = new CancellationTokenSource();
                }
                m_task = Task.Factory.StartNew(ThreadMain, m_tokenSource.Token, TaskCreationOptions.LongRunning);
            }
        }

        public void ThreadMain(object token)
        {

            if (token is CancellationToken)
            {
                try
                {
                    AsyncContext.Run(async () =>
                    {
                        await ThreadMainAsync((CancellationToken)token);
                    });
                }
                catch (OperationCanceledException)
                {
                    // Ignore operation cancelled
                }
            }
        }

        public async Task ThreadMainAsync(CancellationToken token)
        {
            uint oldIp = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int port;

                    using (TcpClient client = new TcpClient())
                    {
                        Task connection = client.ConnectAsync("127.0.0.1", 1742);
                        Task delayTask = Task.Delay(2000, token);

                        try
                        {
                            var finished = await Task.WhenAny(connection, delayTask);
                            if (finished == delayTask)
                            {
                                // Timed Out
                                continue;
                            }
                            if (!finished.IsCompleted || finished.IsFaulted || finished.IsCanceled)
                            {
                                continue;
                            }
                        }

                        catch (OperationCanceledException)
                        {
                            goto done;
                        }

                        if (token.IsCancellationRequested)
                        {
                            goto done;
                        }

                        Logger.Debug3(Logger.Instance, "Connected to DS");
                        Stream stream = client.GetStream();

                        while (!token.IsCancellationRequested && stream.CanRead)
                        {
                            StringBuilder json = new StringBuilder();
                            int chars = 0;
                            byte[] ch = new byte[1];
                            do
                            {
                                chars = 0;
                                try
                                {
                                    chars = await stream.ReadAsync(ch, 0, 1, token);
                                }
                                catch (OperationCanceledException)
                                {
                                    goto done;
                                }
                                if (chars != 1) break;
                                if (token.IsCancellationRequested) goto done;
                            } while (ch[0] != (byte)'{');
                            json.Append('{');

                            if (!stream.CanRead || !client.Connected || chars != 1)
                            {
                                break;
                            }
                            
                            do
                            {
                                chars = 0;
                                try
                                {
                                    chars = await stream.ReadAsync(ch, 0, 1, token);
                                    if (chars != 1) break;
                                    if (token.IsCancellationRequested) goto done;
                                    json.Append((char)ch[0]);
                                }
                                catch (OperationCanceledException)
                                {
                                    goto done;
                                }
                            } while (ch[0] != (byte)'}');

                            if (!stream.CanRead || !client.Connected || chars != 1)
                            {
                                break;
                            }

                            string jsonString = json.ToString();
                            Debug3(Logger.Instance, $"json={jsonString}");

                            // Look for "robotIP":12345, and get 12345 portion
                            int pos = jsonString.IndexOf("\"robotIP\"");
                            if (pos < 0) continue; // could not find?
                            pos += 9;
                            pos = jsonString.IndexOf(':', pos);
                            if (pos < 0) continue; // could not find?
                                                   // Find first not of
                            int endpos = -1;
                            for (int i = pos + 1; i < jsonString.Length; i++)
                            {
                                if (jsonString[i] < '0' || jsonString[i] > '9')
                                {
                                    endpos = i;
                                    break;
                                }
                            }
                            string ipString = jsonString.Substring(pos + 1, endpos - (pos + 1));
                            Debug3(Logger.Instance, $"found robotIP={ipString}");

                            // Parse into number
                            if (!uint.TryParse(ipString, out uint ip)) continue;

                            if (BitConverter.IsLittleEndian)
                            {
                                ip = (uint)IPAddress.NetworkToHostOrder((int)ip);
                            }

                            if (ip == 0)
                            {
                                //m_serverOverridable.ClearServerOverride();
                                Dispatcher.Instance.ClearServerOverride();
                                oldIp = 0;
                                continue;
                            }

                            if (ip == oldIp) continue;
                            oldIp = ip;

                            json.Clear();

                            IPAddress address = new IPAddress(oldIp);
                            Info(Logger.Instance, $"client: DS overriding server IP to {address.ToString()}");
                            port = Interlocked.CompareExchange(ref m_port, 0, 0);
                            //m_serverOverridable.SetServerOverride(address, port);
                            Dispatcher.Instance.SetServerOverride(address, port);
                        }
                    }

                    Dispatcher.Instance.ClearServerOverride();
                    //m_serverOverridable.ClearServerOverride();
                    oldIp = 0;
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {
            }

            done:
            Dispatcher.Instance.ClearServerOverride();
            //m_serverOverridable.ClearServerOverride();
        }
    }
}
