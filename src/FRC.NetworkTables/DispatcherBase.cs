using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables.TcpSockets;
using static NetworkTables.Logging.Logger;
using NetworkTables.Extensions;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using NetworkTables.Logging;

namespace NetworkTables
{
    internal class DispatcherBase : IDisposable
    {
        public delegate NtTcpClient Connector();

        public const double MinimumUpdateTime = 0.01; //100ms
        public const double MaximumUpdateTime = 1.0; //1 second

        private static readonly TimeSpan s_saveDeltaTime = TimeSpan.FromSeconds(1);
        private readonly AutoResetEvent m_flushCv = new AutoResetEvent(false);

        private readonly object m_flushMutex = new object();
        private readonly Notifier m_notifier;

        private readonly AutoResetEvent m_reconnectCv = new AutoResetEvent(false);

        private readonly Storage m_storage;

        private readonly object m_userMutex = new object();

        private bool m_active;
        private Task m_clientServerThread;
        private List<NetworkConnection> m_connections = new List<NetworkConnection>();
        private Task m_dispatchThread;
        private bool m_doFlush;
        private bool m_doReconnect = true;
        private string m_identity = "";

        private IList<Connector> m_clientConnectors = new List<Connector>(); 

        private DateTime m_lastFlush;

        private string m_persistFilename;
        private int m_reconnectProtoRev = 0x0300;

        private bool m_server;

        private INetworkAcceptor m_serverAccepter;
        private double m_updateRate;

        private Connector m_clientConnectorOverride;

        protected DispatcherBase(Storage storage, Notifier notifier)
        {
            m_storage = storage;
            m_notifier = notifier;
            m_active = false;
            m_updateRate = MinimumUpdateTime;
        }

        public bool Active => m_active;

        public double UpdateRate
        {
            get { return m_updateRate; }
            set
            {
                //Don't allow update rates faster the 100ms or slower then 1 second
                if (value < MinimumUpdateTime)
                    value = MinimumUpdateTime;
                else if (value > MaximumUpdateTime)
                    value = MaximumUpdateTime;
                m_updateRate = value;
            }
        }

        public string Identity
        {
            get
            {
                lock (m_userMutex)
                {
                    return m_identity;
                }
            }
            set
            {
                lock (m_userMutex)
                {
                    m_identity = value;
                }
            }
        }

        //Disposable
        public void Dispose()
        {
            Instance.SetDefaultLogger();
            Stop();
        }

        public void StartServer(string persistentFilename, INetworkAcceptor acceptor)
        {
            lock (m_userMutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_server = true;
            m_persistFilename = persistentFilename;
            m_serverAccepter = acceptor;

            //Load persistent file, and ignore erros but pass along warnings.
            if (!string.IsNullOrEmpty(persistentFilename))
            {
                bool first = true;
                m_storage.LoadPersistent(persistentFilename, (line, msg) =>
                {
                    if (first)
                    {
                        first = false;
                        Warning(Logger.Instance, $"When reading initial persistent values from \" {persistentFilename} \":");
                    }
                    Warning(Logger.Instance, $"{persistentFilename} : {line.ToString()} : {msg}");
                });
            }

            //Bind SetOutgoing
            m_storage.SetOutgoing(QueueOutgoing, m_server);

            //Start our tasks
            m_dispatchThread = Task.Factory.StartNew(DispatchThreadMain, TaskCreationOptions.LongRunning);
            m_clientServerThread = Task.Factory.StartNew(ServerThreadMain, TaskCreationOptions.LongRunning);
        }

        public void StartClient()
        {
            lock (m_userMutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_server = false;

            //Bind SetOutgoing
            m_storage.SetOutgoing(QueueOutgoing, m_server);

            //Start our tasks
            m_dispatchThread = Task.Factory.StartNew(DispatchThreadMain, TaskCreationOptions.LongRunning);
            m_clientServerThread = Task.Factory.StartNew(ClientThreadMain, TaskCreationOptions.LongRunning);
        }

        public void SetConnector(Connector connector)
        {
            SetConnector(new List<Connector>() { connector });
        }

        public void SetConnector(IList<Connector> connectors)
        {
            lock (m_userMutex)
            {
                m_clientConnectors = connectors;
            }
        }

        public void SetConnectorOverride(Connector connector)
        {
            lock (m_userMutex)
            {
                m_clientConnectorOverride = connector;
            }
        }

        public void ClearConnectorOverride()
        {
            lock (m_userMutex)
            {
                m_clientConnectorOverride = null;
            }
        }

        public void Stop()
        {
            m_active = false;

            // Wake up dispatch thread with a flush
            m_flushCv.Set();

            //Wake up client thread with a reconnect
            lock (m_userMutex)
            {
                m_clientConnectors.Clear();
            }
            ClientReconnect();

            //Wake up server thread by a socket shutdown
            m_serverAccepter?.Shutdown();

            //Join our dispatch thread.
            m_dispatchThread?.WaitAndUnwrapException();
            //Join our Client Server Thread
            m_clientServerThread?.WaitAndUnwrapException();

            List<NetworkConnection> conns = new List<NetworkConnection>();
            lock (m_userMutex)
            {
                var tmp = m_connections;
                m_connections = conns;
                conns = tmp;
            }

            //Dispose and close all connections
            foreach (var networkConnection in conns)
            {
                networkConnection.Dispose();
            }

            m_connections.Clear();
        }

        public void Flush()
        {
            var now = DateTime.UtcNow;
            lock (m_flushMutex)
            {
                if (now - m_lastFlush < TimeSpan.FromSeconds(MinimumUpdateTime))
                {
                    return;
                }

                m_lastFlush = now;
                m_doFlush = true;
            }
            m_flushCv.Set();
        }

        public List<ConnectionInfo> GetConnections()
        {
            List<ConnectionInfo> conns = new List<ConnectionInfo>();
            if (!m_active) return conns;

            lock (m_userMutex)
            {
                foreach (var networkConnection in m_connections)
                {
                    if (networkConnection.GetState() != NetworkConnection.State.Active) continue;
                    conns.Add(networkConnection.GetConnectionInfo());
                }
            }

            return conns;
        }

        public void NotifyConnections(ConnectionListenerCallback callback)
        {
            lock (m_userMutex)
            {
                
                foreach (var conn in m_connections)
                {
                    conn.NotifyIfActive(callback);
                }
            }
        }

        private void DispatchThreadMain()
        {
            var timeoutTime = DateTime.UtcNow;

            var nextSaveTime = timeoutTime + s_saveDeltaTime;

            int count = 0;

            bool lockEntered = false;
            try
            {
                while (m_active)
                {
                    //Handle loop taking too long
                    var start = DateTime.UtcNow;
                    if (start > timeoutTime)
                        timeoutTime = start;
                    //Wait for periodic or when flushed
                    timeoutTime += TimeSpan.FromSeconds(m_updateRate);
                    TimeSpan waitTime = timeoutTime - start;
                    Monitor.Enter(m_flushMutex, ref lockEntered);
                    m_flushCv.WaitTimeout(m_flushMutex, ref lockEntered, waitTime,
                        () => !m_active || m_doFlush);
                    m_doFlush = false;
                    Monitor.Exit(m_flushMutex);
                    lockEntered = false;
                    if (!m_active) break; //in case we were woken up to terminate

                    if (m_server && !string.IsNullOrEmpty(m_persistFilename) && start > nextSaveTime)
                    {
                        nextSaveTime += s_saveDeltaTime;
                        //Handle loop taking too long
                        if (start > nextSaveTime) nextSaveTime = start + s_saveDeltaTime;
                        string err = m_storage.SavePersistent(m_persistFilename, true);
                        if (err != null)
                        {
                            Warning(Logger.Instance, $"periodic persistent save: {err}");
                        }
                    }

                    lock (m_userMutex)
                    {
                        bool reconnect = false;

                        if (++count > 10)
                        {
                            Debug(Logger.Instance, $"dispatch running {m_connections.Count.ToString()} connections");
                            count = 0;
                        }

                        foreach (var conn in m_connections)
                        {
                            //Post outgoing messages if connection is active
                            //only send keep-alives on client
                            if (conn.GetState() == NetworkConnection.State.Active)
                                conn.PostOutgoing(!m_server);

                            //if client, reconnect if connection died
                            if (!m_server && conn.GetState() == NetworkConnection.State.Dead)
                                reconnect = true;
                        }

                        //reconnect if we disconnected and a reconnect is not in progress
                        if (reconnect && !m_doReconnect)
                        {
                            m_doReconnect = true;
                            m_reconnectCv.Set();
                        }
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_flushMutex);
            }
        }

        private void ServerThreadMain()
        {
            if (m_serverAccepter.Start() != 0)
            {
                m_active = false;
                return;
            }

            while (m_active)
            {
                var stream = m_serverAccepter.Accept();
                if (stream == null)
                {
                    m_active = false;
                    return;
                }
                if (!m_active) return;

                if (stream.RemoteEndPoint is IPEndPoint ipEp)
                {
                    Debug(Logger.Instance, $"server: client connection from {ipEp.Address} port {ipEp.Port.ToString()}");
                }
                else
                {
                    Warning(Logger.Instance, "server: client connection from unknown IP address and Port");
                }

                var conn = new NetworkConnection(stream, m_notifier, ServerHandshake, m_storage.GetEntryType);
                conn.SetProcessIncoming(((msg, connection) =>
                {
                    m_storage.ProcessIncoming(msg, connection, new WeakReference<NetworkConnection>(connection));
                }));

                lock (m_userMutex)
                {
                    bool placed = false;
                    for (int i = 0; i < m_connections.Count; i++)
                    {
                        var c = m_connections[i];
                        if (c.GetState() == NetworkConnection.State.Dead)
                        {
                            m_connections[i] = conn;
                            placed = true;
                            break;
                        }
                    }

                    if (!placed) m_connections.Add(conn);
                    conn.Start();
                }
            }
        }

        private void ClientThreadMain()
        {
            int i = 0;
            while (m_active)
            {
                //Sleep between retries
                Task.Delay(TimeSpan.FromMilliseconds(250)).Wait();

                Connector connect;

                lock (m_userMutex)
                {
                    if (m_clientConnectorOverride != null)
                    {
                        connect = m_clientConnectorOverride;
                    }
                    else
                    {
                        if (m_clientConnectors.Count == 0) continue;
                        if (i >= m_clientConnectors.Count) i = 0;
                        connect = m_clientConnectors[i++];
                    }
                }

                Debug(Logger.Instance, "client trying to connect");
                var stream = connect();
                if (stream == null) continue; //keep retrying
                Debug(Logger.Instance, "client connected");

                bool lockEntered = false;
                try
                {
                    Monitor.Enter(m_userMutex, ref lockEntered);
                    var conn = new NetworkConnection(stream, m_notifier, ClientHandshake, m_storage.GetEntryType);
                    conn.SetProcessIncoming((msg, connection) =>
                    {
                        m_storage.ProcessIncoming(msg, connection, new WeakReference<NetworkConnection>(connection));
                    });
                    foreach (var s in m_connections) //Disconnect any current
                    {
                        s.Dispose();
                    }

                    m_connections.Clear();

                    m_connections.Add(conn);

                    conn.ProtoRev = m_reconnectProtoRev;

                    conn.Start();

                    // reconnect the next time starting with latest protocol revision
                    m_reconnectProtoRev = 0x0300;

                    m_doReconnect = false;
                    m_reconnectCv.Wait(m_userMutex, ref lockEntered, () => !m_active || m_doReconnect);
                }
                finally
                {
                    if (lockEntered) Monitor.Exit(m_userMutex);
                }
            }
        }

        private bool ClientHandshake(NetworkConnection conn, Func<Message> getMsg, Action<List<Message>> sendMsgs)
        {
            string selfId;
            lock (m_userMutex)
            {
                selfId = m_identity;
            }

            Debug(Logger.Instance, "client: sending hello");
            sendMsgs(new List<Message> { Message.ClientHello(selfId) });

            var msg = getMsg();
            if (msg == null)
            {
                //Disconnected
                Debug(Logger.Instance, "client: server disconnected before first response");
                return false;
            }

            if (msg.Is(Message.MsgType.ProtoUnsup))
            {
                if (msg.Id == 0x0200) ClientReconnect(0x0200);
                return false;
            }

            bool newServer = true;
            if (conn.ProtoRev >= 0x0300)
            {
                if (!msg.Is(Message.MsgType.ServerHello)) return false;
                conn.RemoteId = msg.Str;
                if ((msg.Flags & 1) != 0) newServer = false;
                msg = getMsg();
            }

            List<Message> incoming = new List<Message>();

            for (;;)
            {
                if (msg == null)
                {
                    //disconnected, retry
                    Debug(Logger.Instance, "client: server disconnected during initial entries");
                    return false;
                }
                Debug4(Logger.Instance, $"received init str={msg.Str} id={msg.Id.ToString()} seqNum={msg.SeqNumUid.ToString()}");

                if (msg.Is(Message.MsgType.ServerHelloDone)) break;
                if (msg.Is(Message.MsgType.KeepAlive))
                {
                    msg = getMsg();
                    continue;
                }
                if (!msg.Is(Message.MsgType.EntryAssign))
                {
                    //Unexpected
                    Debug(Logger.Instance,
                        $"client: received message ({msg.Type.GetString()}) other then entry assignment during initial handshake");
                    return false;
                }

                incoming.Add(msg);

                msg = getMsg();
            }

            List<Message> outgoing = new List<Message>();
            m_storage.ApplyInitialAssignments(conn, incoming.ToArray(), newServer, outgoing);

            if (conn.ProtoRev >= 0x0300)
            {
                outgoing.Add(Message.ClientHelloDone());
            }

            if (outgoing.Count != 0) sendMsgs(outgoing);

            Info(Logger.Instance, $"client: CONNECTED to server {conn.PeerIP} port {conn.PeerPort.ToString()}");

            return true;
        }

        private bool ServerHandshake(NetworkConnection conn, Func<Message> getMsg, Action<List<Message>> sendMsgs)
        {
            var msg = getMsg();

            if (msg == null)
            {
                Debug(Logger.Instance, "server: client disconnected before sending hello");
                return false;
            }

            if (!msg.Is(Message.MsgType.ClientHello))
            {
                Debug(Logger.Instance, "server: client initial message was not client hello");
                return false;
            }

            int protoRev = (int)msg.Id;

            if (protoRev > 0x0300)
            {
                Debug(Logger.Instance, "server: client requested proto > 0x0300");
                sendMsgs(new List<Message> { Message.ProtoUnsup() });
                return false;
            }

            if (protoRev >= 0x0300) conn.RemoteId = msg.Str;

            Debug(Logger.Instance, $"server: client protocol {protoRev.ToString()}");
            conn.ProtoRev = protoRev;

            List<Message> outgoing = new List<Message>();

            if (protoRev >= 0x0300)
            {
                lock (m_userMutex)
                {
                    outgoing.Add(Message.ServerHello(0, m_identity));
                }
            }

            m_storage.GetInitialAssignments(conn, outgoing);

            outgoing.Add(Message.ServerHelloDone());

            Debug(Logger.Instance, "server: sending initial assignments");
            sendMsgs(outgoing);

            if (protoRev >= 0x0300)
            {
                List<Message> incoming = new List<Message>();

                msg = getMsg();

                for (;;)
                {
                    if (msg == null)
                    {
                        //Disconnected Retry
                        Debug(Logger.Instance, "server: disconnected waiting for initial entries");
                        return false;
                    }

                    if (msg.Is(Message.MsgType.ClientHelloDone)) break;
                    if (msg.Is(Message.MsgType.KeepAlive))
                    {
                        msg = getMsg();
                        continue;
                    }
                    if (!msg.Is(Message.MsgType.EntryAssign))
                    {
                        Debug(Logger.Instance,
                            $"server: received message ({msg.Type.GetString()}) other than entry assignment during initial handshake");
                        return false;
                    }

                    incoming.Add(msg);

                    msg = getMsg();
                }

                foreach (var m in incoming)
                {
                    m_storage.ProcessIncoming(m, conn, new WeakReference<NetworkConnection>(conn));
                }
            }

            Info(Logger.Instance, $"server: client CONNECTED: {conn.PeerIP} port {conn.PeerPort.ToString()}");
            return true;
        }

        private void ClientReconnect(int protoRev = 0x0300)
        {
            if (m_server) return;
            lock (m_userMutex)
            {
                m_reconnectProtoRev = protoRev;
                m_doReconnect = true;
            }

            m_reconnectCv.Set();
        }

        private void QueueOutgoing(Message msg, NetworkConnection only, NetworkConnection except)
        {
            lock (m_userMutex)
            {
                foreach (var conn in m_connections)
                {
                    if (conn == except) continue;
                    if (only != null && conn != only) continue;
                    var state = conn.GetState();
                    if (state != NetworkConnection.State.Synchronized &&
                        state != NetworkConnection.State.Active) continue;
                    conn.QueueOutgoing(msg);
                }
            }
        }
    }
}
