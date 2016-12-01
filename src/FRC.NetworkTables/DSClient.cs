using NetworkTables.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Extensions;
using NetworkTables.TcpSockets;
using System.Net.Sockets;
using System.IO;
using NetworkTables.Streams;
using System.Text;
using static NetworkTables.Logging.Logger;
using System.Net;
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

        public void Dispose()
        {
            Stop();
        }

        public void Start(int port)
        {
            lock (m_mutex)
            {
                m_port = port;
                if (m_task == null)
                {
                    m_active = true;
                    m_task = Task.Factory.StartNew(TaskMain, TaskCreationOptions.LongRunning);
                }
            }
        }

        public void Stop()
        {
            m_active = false;
            m_task?.WaitAndUnwrapException();
            m_task = null;
        }

        private DsClient()
        {

        }

        private int m_port;

        private Task m_task;

        private bool m_active;

        private readonly object m_mutex = new object();
        private readonly AutoResetEvent m_cond = new AutoResetEvent(false);

        NtTcpClient m_client; 

        private void TaskMain()
        {
            uint oldIp = 0;
            Logger logger = new Logger(); // To silence log messages

            while (m_active)
            {
                int port;
                bool lockEntered = false;
                try
                {
                    Monitor.Enter(m_mutex, ref lockEntered);
                    m_cond.WaitTimeout(m_mutex, ref lockEntered, TimeSpan.FromMilliseconds(500), () => !m_active);
                    port = m_port;
                }
                finally
                {
                    if (lockEntered) Monitor.Exit(m_mutex);
                }

                if (!m_active) goto done;

                m_client = TcpConnector.Connect("127.0.0.1", 1742, logger, 1);
                if (!m_active) goto done;
                if (m_client == null) continue;

                Logger.Debug3(Logger.Instance, "connected to DS");
                Stream stream = m_client.GetStream();
                while (m_active && stream.CanRead)
                {
                    StringBuilder json = new StringBuilder(128);
                    byte ch = 0;
                    do
                    {
                        bool success = stream.ReceiveByte(out ch);
                        if (!success) break;
                        if (!m_active) goto done;
                    } while (ch != (byte)'{');
                    json.Append('{');

                    if (!stream.CanRead)
                    {
                        m_client = null;
                        break;
                    }

                    // Read characters until }
                    do
                    {
                        bool success = stream.ReceiveByte(out ch);
                        if (!success) break;
                        if (!m_active) goto done;
                        json.Append((char)ch);
                    } while (ch != (byte)'}');

                    if (!stream.CanRead)
                    {
                        m_client = null;
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
                    uint ip;
                    if (!uint.TryParse(ipString, out ip)) continue;

                    if (BitConverter.IsLittleEndian)
                    {
                        ip = (uint)IPAddress.NetworkToHostOrder((int)ip);
                    }

                    // If 0 clear the override
                    if (ip == 0)
                    {
                        Dispatcher.Instance.ClearServerOverride();
                        oldIp = 0;
                        continue;
                    }

                    // If unchanged, don't reconnect
                    if (ip == oldIp) continue;
                    oldIp = ip;

                    json.Clear();

                    IPAddress address = new IPAddress(oldIp);
                    Info(Logger.Instance, $"client: DS overriding server IP to {address.ToString()}");
                    Dispatcher.Instance.SetServerOverride(address.ToString(), port);


                }

                Dispatcher.Instance.ClearServerOverride();
                oldIp = 0;

            }

            done:
            {
                Dispatcher.Instance.ClearServerOverride();
            }

        }
    }
}
