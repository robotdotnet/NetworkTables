﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetworkTables.Streams;
using NetworkTables.Support;
using NetworkTables.TcpSockets;
using NetworkTables.Wire;
using static NetworkTables.Logging.Logger;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using NetworkTables.Logging;

namespace NetworkTables
{
    internal class NetworkConnection : IDisposable
    {
        public int ProtoRev { get; set; }

        public enum State { Created, Init, Handshake, Synchronized, Active, Dead };

        public delegate bool HandshakeFunc(NetworkConnection conn, Func<Message> getMsg, Action<List<Message>> sendMsgs);

        public delegate void ProcessIncomingFunc(Message msg, NetworkConnection conn);

        private static long s_uid;

        private readonly Stream m_stream;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IClient m_client;

        public int PeerPort { get; }
        public string PeerIP { get; }

        private readonly Notifier m_notifier;

        private readonly BlockingCollection<List<Message>> m_outgoing = new BlockingCollection<List<Message>>();

        private readonly HandshakeFunc m_handshake;

        private readonly Message.GetEntryTypeFunc m_getEntryType;

        private ProcessIncomingFunc m_processIncoming;

        private Task m_readThread;
        private Task m_writeThread;

        private State m_state;

        private string m_remoteId;

        private DateTime m_lastPost = DateTime.UtcNow;

        private readonly object m_pendingMutex = new object();

        private readonly object m_remoteIdMutex = new object();

        private readonly List<Message> m_pendingOutgoing = new List<Message>();

        private readonly List<ValueTuple<int, int>> m_pendingUpdate = new List<ValueTuple<int, int>>();

        public NetworkConnection(IClient client, Notifier notifier, HandshakeFunc handshake,
            Message.GetEntryTypeFunc getEntryType)
        {
            Uid = (uint)Interlocked.Increment(ref s_uid) - 1;
            m_client = client;
            m_stream = client.GetStream();
            m_notifier = notifier;
            m_handshake = handshake;
            m_getEntryType = getEntryType;

            Active = false;
            ProtoRev = 0x0300;
            m_state = State.Created;
            LastUpdate = 0;

            IPEndPoint ipEp = m_client.RemoteEndPoint as IPEndPoint;
            if (ipEp != null)
            {
                PeerIP = ipEp.Address.ToString();
                PeerPort = ipEp.Port;
            }
            else
            {
                PeerIP = "";
                PeerPort = 0;
            }

            // turns of Nagle, as we bundle packets ourselves
            m_client.NoDelay = true;
        }

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Stop();
            // Set process incoming to null to make sure any closures don't hold references.
            m_processIncoming = null;
            Disposed = true;
        }

        public void SetProcessIncoming(ProcessIncomingFunc func)
        {
            m_processIncoming = func;
        }

        public void Start()
        {
            if (Active) return;
            Active = true;
            m_state = State.Init;
            // clear queue
            while (m_outgoing.Count != 0) m_outgoing.Take();

            //Start our tasks
            m_writeThread = Task.Factory.StartNew(WriteThreadMain, TaskCreationOptions.LongRunning);
            m_readThread = Task.Factory.StartNew(ReadThreadMain, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            Debug2(Logger.Instance, $"NetworkConnection stopping ({this})");
            m_state = State.Dead;

            Active = false;
            //Closing stream to terminate read thread
            m_stream?.Dispose();
            m_client?.Dispose();
            //Send an empty message to terminate the write thread
            m_outgoing.Add(new List<Message>());

            //Wait for our threads to detach from each.
            m_writeThread?.WaitAndUnwrapException();
            m_readThread?.WaitAndUnwrapException();

            // clear the queue
            while (m_outgoing.Count != 0) m_outgoing.Take();
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(RemoteId, PeerIP, PeerPort, LastUpdate, ProtoRev);
        }

        private readonly object m_stateMutex = new object();

        public void NotifyIfActive(ConnectionListenerCallback callback)
        {
            lock (m_stateMutex)
            {
                if (m_state == State.Active) m_notifier.NotifyConnection(true, GetConnectionInfo(), callback);
            }
        }

        public State GetState()
        {
            lock (m_stateMutex)
            {
                return m_state;
            }
        }

        public void SetState(State state)
        {
            lock (m_stateMutex)
            {
                // Don't update state any more once we've died
                if (m_state == State.Dead) return;
                // One-shot notify state changes
                if (m_state != State.Active && state == State.Active)
                    m_notifier.NotifyConnection(true, GetConnectionInfo());
                if (m_state != State.Dead && state == State.Dead)
                    m_notifier.NotifyConnection(false, GetConnectionInfo());
                m_state = state;

            }
        }

        public bool Active { get; private set; }

        public Stream GetStream()
        {
            return m_stream;
        }

        private void ResizePendingUpdate(int newSize)
        {
            int currentSize = m_pendingUpdate.Count;

            if (newSize > currentSize)
            {
                if (newSize > m_pendingUpdate.Capacity)
                    m_pendingUpdate.Capacity = newSize;
                m_pendingUpdate.AddRange(Enumerable.Repeat(default(ValueTuple<int, int>), newSize - currentSize));
            }
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
                            int id = (int)msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }
                            if (id < m_pendingUpdate.Count && m_pendingUpdate[id].Item1 != 0)
                            {
                                var oldmsg = m_pendingOutgoing[m_pendingUpdate[id].Item1 - 1];
                                if (oldmsg != null && oldmsg.Is(Message.MsgType.EntryAssign) &&
                                    msg.Is(Message.MsgType.EntryUpdate))
                                {
                                    // need to update assignement
                                    m_pendingOutgoing[m_pendingUpdate[id].Item1] = Message.EntryAssign(oldmsg.Str, (uint)id, msg.SeqNumUid, msg.Val,
                                        (EntryFlags)oldmsg.Flags);

                                }
                                else
                                {
                                    // new but remember it
                                    m_pendingOutgoing[m_pendingUpdate[id].Item1] = msg;
                                }
                            }
                            else
                            {
                                // new but don't remember it
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id >= m_pendingUpdate.Count) ResizePendingUpdate(id + 1);
                                m_pendingUpdate[id] = new ValueTuple<int, int>(pos + 1, m_pendingUpdate[id].Item2);
                            }
                            break;
                        }
                    case Message.MsgType.EntryDelete:
                        {
                            //Don't do this for unnasigned uid's
                            int id = (int)msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count)
                            {
                                if (m_pendingUpdate[id].Item1 != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[id].Item1 - 1] = new Message();
                                    m_pendingUpdate[id] = new ValueTuple<int, int>(0, m_pendingUpdate[id].Item2);
                                }
                                if (m_pendingUpdate[id].Item2 != 0)
                                {
                                    m_pendingOutgoing[m_pendingUpdate[id].Item2 - 1] = new Message();
                                    m_pendingUpdate[id] = new ValueTuple<int, int>(m_pendingUpdate[id].Item1, 0);
                                }
                            }
                            //Add deletion
                            m_pendingOutgoing.Add(msg);
                            break;
                        }
                    case Message.MsgType.FlagsUpdate:
                        {
                            //Don't do this for unassigned uids
                            int id = (int)msg.Id;
                            if (id == 0xffff)
                            {
                                m_pendingOutgoing.Add(msg);
                                break;
                            }

                            if (id < m_pendingUpdate.Count && m_pendingUpdate[id].Item2 != 0)
                            {
                                //Overwrite the previous one for this uid
                                m_pendingOutgoing[m_pendingUpdate[id].Item2 - 1] = msg;
                            }
                            else
                            {
                                int pos = m_pendingOutgoing.Count;
                                m_pendingOutgoing.Add(msg);
                                if (id > m_pendingUpdate.Count) ResizePendingUpdate(id + 1);
                                m_pendingUpdate[id] = new ValueTuple<int, int>(m_pendingUpdate[id].Item1, pos + 1);

                            }
                            break;
                        }
                    case Message.MsgType.ClearEntries:
                        {
                            //Knock out all previous assignes/updates
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
                m_lastPost = now;
            }
        }

        public uint Uid { get; }

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

            SetState(State.Handshake);

            if (!m_handshake(this,() =>
            {
                decoder.ProtoRev = ProtoRev;
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null && decoder.Error != null)
                {
                    Debug(Logger.Instance, $"error reading in handshake: {decoder.Error}");
                }
                return msg;
            }, messages =>
            {
                m_outgoing.Add(messages);
            }))
            {
                SetState(State.Dead);
                Active = false;
                return;
            }

            SetState(State.Active);
            m_notifier.NotifyConnection(true, GetConnectionInfo());
            while (Active)
            {
                if (m_stream == null) break;
                decoder.ProtoRev = ProtoRev;
                decoder.Reset();
                var msg = Message.Read(decoder, m_getEntryType);
                if (msg == null)
                {
                    if (decoder.Error != null) Info(Logger.Instance, $"read error: {decoder.Error}");
                    //terminate connection on bad message
                    m_stream?.Dispose();
                    break;
                }
                // ToString on the enum type does not stop the boxing
                Debug3(Logger.Instance, $"received type={msg.Type.GetString()} with str={msg.Str} id={msg.Id.ToString()} seqNum={msg.SeqNumUid.ToString()}");
                LastUpdate = Timestamp.Now();
                m_processIncoming(msg, this);
            }

            Debug2(Logger.Instance, $"read thread died ({this})");
            if (m_state != State.Dead) m_notifier.NotifyConnection(false, GetConnectionInfo());
            SetState(State.Dead);
            Active = false;
            m_outgoing.Add(new List<Message>()); // Also kill write thread
        }

        private void WriteThreadMain()
        {
            WireEncoder encoder = new WireEncoder(ProtoRev);

            while (Active)
            {
                var msgs = m_outgoing.Take();
                Debug4(Logger.Instance, "write thread woke up");
                if (msgs.Count == 0) continue;
                encoder.ProtoRev = ProtoRev;
                encoder.Reset();
                Debug3(Logger.Instance, $"sending {msgs.Count.ToString()} messages");
                foreach (var message in msgs)
                {
                    if (message != null)
                    {
                        Debug3(Logger.Instance, $"sending type={message.Type.GetString()} with str={message.Str} id={message.Id.ToString()} seqNum={message.SeqNumUid.ToString()}");
                        message.Write(encoder);
                    }
                }
                if (m_stream == null) break;
                if (encoder.Count == 0) continue;
                if (m_stream.Send(encoder.Buffer, 0, encoder.Count) == 0) break;
                Debug4(Logger.Instance, $"sent {encoder.Count.ToString()} bytes");
            }
            Debug2(Logger.Instance, $"write thread died ({this})");
            SetState(State.Dead);
            Active = false;
            m_stream?.Dispose(); // Also kill read thread
        }
    }
}
