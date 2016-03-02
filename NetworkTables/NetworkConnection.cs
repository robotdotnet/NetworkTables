using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetworkTables.Streams;
using NetworkTables.Support;
using NetworkTables.Wire;
using static NetworkTables.Logging.Logger;

namespace NetworkTables
{
    internal class NetworkConnection : IDisposable
    {
        private struct Pair
        {
            public int First { get; private set; }
            public int Second { get; private set; }

            public void SetFirst(int first)
            {
                First = first;
            }

            public void SetSecond(int second)
            {
                Second = second;
            }
        }

        public uint ProtoRev { get; set; }


        public enum State { Created, Init, Handshake, Synchronized, Active, Dead };

        public delegate bool HandshakeFunc(NetworkConnection conn, Func<Message> getMsg, Action<Message[]> sendMsgs);

        public delegate void ProcessIncomingFunc(Message msg, NetworkConnection conn);

        private static long s_uid;

        private readonly NtNetworkStream m_stream;

        private readonly Notifier m_notifier;

        private readonly BlockingCollection<List<Message>> m_outgoing = new BlockingCollection<List<Message>>();

        private readonly HandshakeFunc m_handshake;

        private readonly Message.GetEntryTypeFunc m_getEntryType;

        private ProcessIncomingFunc m_processIncoming;

        private Thread m_readThread;
        private Thread m_writeThread;

        private bool m_active;
        private State m_state;

        private string m_remoteId;

        private DateTime m_lastPost = DateTime.UtcNow;

        private readonly object m_pendingMutex = new object();

        private readonly object m_remoteIdMutex = new object();

        private readonly List<Message> m_pendingOutgoing = new List<Message>();

        private readonly List<Pair> m_pendingUpdate = new List<Pair>();

        public NetworkConnection(NtNetworkStream stream, Notifier notifier, HandshakeFunc handshake,
            Message.GetEntryTypeFunc getEntryType)
        {
            Uid = (uint)Interlocked.Increment(ref s_uid) - 1;
            m_stream = stream;
            m_notifier = notifier;
            m_handshake = handshake;
            m_getEntryType = getEntryType;

            m_active = false;
            ProtoRev = 0x0300;
            m_state = State.Created;

            // turns of Nagle, as we bundle packets ourselves
            m_stream.SetNoDelay();
        }

        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            Stop();
            Disposed = true;
        }

        public void SetProcessIncoming(ProcessIncomingFunc func)
        {
            m_processIncoming = func;
        }

        public void Start()
        {
            if (m_active) return;
            m_active = true;
            m_state = State.Init;
            // clear queue
            while (m_outgoing.Count != 0) m_outgoing.Take();

            m_writeThread = new Thread(WriteThreadMain);
            m_writeThread.IsBackground = true;
            m_writeThread.Name = "Connection Write Thread";
            m_writeThread.Start();

            m_readThread = new Thread(ReadThreadMain);
            m_readThread.IsBackground = true;
            m_readThread.Name = "Connection Read Thread";
            m_readThread.Start();
        }

        public void Stop()
        {
            m_state = State.Dead;

            m_active = false;
            //Closing stream to terminate read thread
            m_stream?.Close();
            List<Message> temp = new List<Message>();
            //Send an empty message to terminate the write thread
            m_outgoing.Add(temp);

            //Wait for our threads to detach from each.
            m_writeThread?.Join();
            m_readThread?.Join();

            // clear the queue
            while (m_outgoing.Count != 0) m_outgoing.Take();
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(RemoteId, m_stream.PeerIP, m_stream.PeerPort, (long)LastUpdate, (int)ProtoRev);
        }

        public bool Active()
        {
            return m_active;
        }

        public NtNetworkStream Stream()
        {
            return m_stream;
        }

        public void QueueOutgoing(Message msg)
        {
            lock (m_pendingMutex)
            {
                //Merge with previouse
                Message.MsgType type = msg.Type;
                switch (type)
                {
                    case Message.MsgType.EntryAssign:
                    case Message.MsgType.EntryUpdate:
                        {
                            // don't do this for unassigned id's
                            uint id = msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }
                            if (id < m_pendingUpdate.Count && m_pendingUpdate[(int)id].First != 0)
                            {
                                var oldmsg = m_pendingOutgoing[m_pendingUpdate[(int)id].First];
                                if (oldmsg != null && oldmsg.Is(Message.MsgType.EntryAssign) &&
                                    msg.Is(Message.MsgType.EntryUpdate))
                                {
                                    // need to update assignement
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First] = Message.EntryAssign(oldmsg.Str, id, msg.SeqNumUid, msg.Val,
                                        (EntryFlags)oldmsg.Flags);

                                }
                                else
                                {
                                    // new but remember it
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First] = msg;
                                }
                            }
                            else
                            {
                                // new but don't remember it
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id >= m_pendingUpdate.Count) m_pendingUpdate.Add(new Pair());
                                m_pendingUpdate[(int)id].SetFirst(pos + 1);
                            }
                            break;
                        }
                    case Message.MsgType.EntryDelete:
                        {
                            uint id = msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count)
                            {
                                if (m_pendingUpdate[(int)id].First != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].First - 1] = new Message();
                                    m_pendingUpdate[(int)id].SetFirst(0);
                                }
                                if (m_pendingUpdate[(int)id].Second != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[(int)id].Second - 1] = new Message();
                                    m_pendingUpdate[(int)id].SetSecond(0);
                                }
                            }

                            m_pendingOutgoing.Add(msg);
                            break;
                        }
                    case Message.MsgType.FlagsUpdate:
                        {
                            uint id = msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count && m_pendingUpdate[(int)id].Second != 0)
                            {
                                m_pendingOutgoing[m_pendingUpdate[(int)id].Second - 1] = msg;
                            }
                            else
                            {
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id > m_pendingUpdate.Count) m_pendingUpdate.Add(new Pair());
                                m_pendingUpdate[(int)id].SetSecond(pos + 1);

                            }
                            break;
                        }
                    case Message.MsgType.ClearEntries:
                        {
                            for (int i = 0; i < m_pendingOutgoing.Count; i++)
                            {
                                var message = m_pendingOutgoing[i];
                                if (message == null) continue;
                                var t = message.Type;
                                if (t == Message.MsgType.EntryAssign || t == Message.MsgType.EntryUpdate
                                    || t == Message.MsgType.FlagsUpdate || t == Message.MsgType.EntryDelete
                                    || t == Message.MsgType.ClearEntries)
                                {
                                    m_pendingOutgoing[i] = new Message();
                                }
                            }
                            m_pendingUpdate.Clear();
                            m_pendingOutgoing.Add(msg);
                            break;
                        }
                    default:
                        m_pendingOutgoing.Add(msg);
                        break;
                }
            }
        }

        public void PostOutgoing(bool keepAlive)
        {
            lock (m_pendingMutex)
            {
                var now = DateTime.UtcNow;
                if (m_pendingOutgoing.Count == 0)
                {
                    if (!keepAlive) return;
                    // send keep-alives once a second (if no other messages have been sent)
                    if ((now - m_lastPost) < TimeSpan.FromSeconds(1)) return;
                    m_outgoing.Add(new List<Message> { Message.KeepAlive() });
                }
                else
                {
                    m_outgoing.Add(new List<Message>(m_pendingOutgoing));
                    m_pendingOutgoing.Clear();
                    m_pendingUpdate.Clear();

                }
                m_lastPost = DateTime.UtcNow;
            }
        }

        public uint Uid { get; }

        public State GetState()
        {
            return m_state;
        }

        public void SetState(State state)
        {
            m_state = state;
        }

        public string RemoteId
        {
            get
            {
                lock (m_remoteIdMutex)
                {
                    return m_remoteId;
                }
            }
            set
            {
                lock (m_remoteIdMutex)
                {
                    m_remoteId = value;
                }
            }
        }

        public long LastUpdate { get; private set; }


        private void ReadThreadMain()
        {
            WireDecoder decoder = new WireDecoder(m_stream, ProtoRev);

            m_state = State.Handshake;

            if (!m_handshake(this, () =>
            {
                decoder.ProtoRev = ProtoRev;
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null && decoder.Error != null)
                {
                    Debug($"error reading in handshake: {decoder.Error}");
                }
                return msg;
            }, messages =>
            {
                m_outgoing.Add(messages.ToList());
            }))
            {
                m_state = State.Dead;
                m_active = false;
                return;
            }

            m_state = State.Active;
            m_notifier.NotifyConnection(true, GetConnectionInfo());
            while (m_active)
            {
                if (m_stream == null) break;
                decoder.ProtoRev = ProtoRev;
                decoder.Reset();
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null)
                {
                    if (decoder.Error != null) Info($"read error: {decoder.Error}");
                    m_stream?.Close();
                    break;
                }
                Debug3($"received type={msg.Type} with str={msg.Str} id={msg.Id} seqNum={msg.SeqNumUid}");
                LastUpdate = Timestamp.Now();
                m_processIncoming(msg, this);
            }

            Debug2($"read thread died ({this})");
            if (m_state != State.Dead) m_notifier.NotifyConnection(false, GetConnectionInfo());
            m_state = State.Dead;
            m_active = false;
            m_outgoing.Add(new List<Message>()); // Also kill write thread
        }

        private void WriteThreadMain()
        {
            WireEncoder encoder = new WireEncoder(ProtoRev);

            while (m_active)
            {
                var msgs = m_outgoing.Take();
                Debug4("write thread woke up");
                if (msgs.Count == 0) continue;
                encoder.ProtoRev = ProtoRev;
                encoder.Reset();
                Debug3($"sending {msgs.Count} messages");
                foreach (var message in msgs)
                {
                    if (message != null)
                    {
                        Debug3($"sending type={message.Type} with str={message.Str} id={message.Id} seqNum={message.SeqNumUid}");
                        message.Write(encoder);
                    }
                }
                if (m_stream == null) break;
                if (encoder.Size() == 0) continue;
                if (m_stream.Send(encoder.Buffer, 0, encoder.Size()) == 0) break;
                Debug4($"sent {encoder.Size()} bytes");
            }
            Debug2($"write thread died ({this})");
            if (m_state != State.Dead) m_notifier.NotifyConnection(false, GetConnectionInfo());
            m_state = State.Dead;
            m_active = false;
            m_stream?.Close();
        }

    }
}
