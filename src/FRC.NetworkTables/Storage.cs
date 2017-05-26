using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Logging;
using Nito.AsyncEx;
using static NetworkTables.Logging.Logger;
using static NetworkTables.Message.MsgType;
using Nito.AsyncEx.Synchronous;

namespace NetworkTables
{
    internal partial class Storage : IDisposable
    {
        private static Storage s_instance;

        public static Storage Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Storage d = new Storage();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        internal Dictionary<string, Entry> Entries => m_entries;
        internal List<Entry> IdMap => m_idMap;


        internal Storage() : this(Notifier.Instance, RpcServer.Instance)
        {

        }


        public void Dispose()
        {
            Logger.Instance.SetDefaultLogger();
            m_terminating = true;
            using (m_monitor.Enter())
            {
                m_monitor.PulseAll();
            }
        }

        internal Storage(Notifier notifier, RpcServer rpcServer)
        {
            m_notifier = notifier;
            m_rpcServer = rpcServer;
        }

        internal class Entry
        {
            public Entry(string name)
            {
                Name = name;
                Flags = 0;
                Id = 0xffff;
                Value = null;
                SeqNum = new SequenceNumber();
                RpcCallback = null;
                RpcCallUid = 0;
            }

            public bool IsPersistent() => (Flags & EntryFlags.Persistent) != 0;

            public readonly string Name;
            public Value Value;
            public EntryFlags Flags;
            public uint Id;

            public SequenceNumber SeqNum;

            public RpcCallback RpcCallback;
            public uint RpcCallUid;

        }

        private Dictionary<string, Entry> m_entries = new Dictionary<string, Entry>();
        private readonly List<Entry> m_idMap = new List<Entry>();
        internal readonly Dictionary<(uint, uint), byte[]> m_rpcResults = new Dictionary<(uint, uint), byte[]>();
        private readonly HashSet<long> m_blockingRpcCalls = new HashSet<long>();

        private bool m_terminating;
        private readonly AsyncMonitor m_monitor = new AsyncMonitor();

        QueueOutgoingFunc m_queueOutgoing;
        bool m_server = true;

        bool m_persistentDirty;

        private readonly Notifier m_notifier;
        private readonly RpcServer m_rpcServer;




        public delegate void QueueOutgoingFunc(Message msg, NetworkConnection only, NetworkConnection except);

        public void SetOutgoing(QueueOutgoingFunc queueOutgoing, bool server)
        {
            using (m_monitor.Enter())
            {
                m_queueOutgoing = queueOutgoing;
                m_server = server;
            }
        }

        public void ClearOutgoing()
        {
            m_queueOutgoing = null;
        }

        public NtType GetEntryType(uint id)
        {
            using (m_monitor.Enter())
            {
                if (id >= m_idMap.Count) return NtType.Unassigned;
                Entry entry = m_idMap[(int)id];
                if (entry?.Value == null) return NtType.Unassigned;
                return entry.Value.Type;
            }
        }

        public void ProcessIncoming(Message msg, NetworkConnection conn, WeakReference<NetworkConnection> connWeak)
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                Message.MsgType type = msg.Type;
                SequenceNumber seqNum;
                Entry entry;
                uint id;
                
                switch (type)
                {
                    case KeepAlive:
                        break; // ignore
                    case ClientHello:
                    case ProtoUnsup:
                    case ServerHelloDone:
                    case ServerHello:
                    case ClientHelloDone:
                        // shouldn't get these, but ignore if we do
                        break;
                    case EntryAssign:
                        {
                            id = msg.Id;
                            string name = msg.Str;
                            bool mayNeedUpdate = false;
                            if (m_server)
                            {
                                if (id == 0xffff)
                                {
                                    if (m_entries.ContainsKey(name)) return;


                                    id = (uint)m_idMap.Count;
                                    entry = new Entry(name)
                                    {
                                        Value = msg.Val,
                                        Flags = (EntryFlags) msg.Flags,
                                        Id = id
                                    };
                                    m_entries[name] = entry;
                                    m_idMap.Add(entry);

                                    if (entry.IsPersistent()) m_persistentDirty = true;

                                    m_notifier.NotifyEntry(name, entry.Value, NotifyFlags.NotifyNew);

                                    if (m_queueOutgoing != null)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outMsg = Message.EntryAssign(name, id, entry.SeqNum.Value, msg.Val, (EntryFlags)msg.Flags);
                                        monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                        monitorToUnlock.Dispose();
                                        queueOutgoing(outMsg, null, null);
                                    }

                                    return;
                                }
                                if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                                {
                                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                    monitorToUnlock.Dispose();
                                    Debug(Logger.Instance, "server: received assignment to unknown entry");
                                    return;
                                }
                                entry = m_idMap[(int)id];
                            }
                            else
                            {
                                if (id == 0xffff)
                                {
                                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                    monitorToUnlock.Dispose();
                                    Debug(Logger.Instance, "client: received entry assignment request?");
                                    return;
                                }
                                if (id >= m_idMap.Count) ResizeIdMap(id + 1);
                                entry = m_idMap[(int)id];
                                if (entry == null)
                                {
                                    Entry newEntry;
                                    if (!m_entries.ContainsKey(name))
                                    {
                                        //Entry didn't exist at all.
                                        newEntry = new Entry(name)
                                        {
                                            Value = msg.Val,
                                            Flags = (EntryFlags) msg.Flags,
                                            Id = id
                                        };
                                        m_idMap[(int)id] = newEntry;
                                        m_entries[name] = newEntry;

                                        m_notifier.NotifyEntry(name, newEntry.Value, NotifyFlags.NotifyNew);
                                        return;
                                    }
                                    else
                                    {
                                        newEntry = m_entries[name];
                                    }
                                    mayNeedUpdate = true;
                                    entry = newEntry;
                                    entry.Id = id;
                                    m_idMap[(int)id] = entry;

                                    if ((EntryFlags)msg.Flags != entry.Flags)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outmsg = Message.FlagsUpdate(id, entry.Flags);
                                        monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                        monitorToUnlock.Dispose();
                                        queueOutgoing(outmsg, null, null);
                                        Interlocked.Exchange(ref monitor, m_monitor.Enter());
                                    }
                                }
                            }

                            seqNum = new SequenceNumber(msg.SeqNumUid);
                            if (seqNum < entry.SeqNum)
                            {
                                if (mayNeedUpdate)
                                {
                                    var queueOutgoing = m_queueOutgoing;
                                    var outmsg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, entry.Value);
                                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                    monitorToUnlock.Dispose();
                                    queueOutgoing(outmsg, null, null);
                                }
                                return;
                            }
                            //Sanity check. Name should match id
                            if (msg.Str != entry.Name)
                            {
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                Debug(Logger.Instance, "entry assignment for same id with different name?");
                                return;
                            }

                            NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate;

                            if (!mayNeedUpdate && conn.ProtoRev >= 0x0300)
                            {
                                if ((entry.Flags & EntryFlags.Persistent) != ((EntryFlags)msg.Flags & EntryFlags.Persistent))
                                {
                                    m_persistentDirty = true;
                                }
                                if (entry.Flags != (EntryFlags)msg.Flags)
                                {
                                    notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                }
                                entry.Flags = (EntryFlags)msg.Flags;
                            }

                            if (entry.IsPersistent() && entry.Value != msg.Val)
                            {
                                m_persistentDirty = true;
                            }

                            entry.Value = msg.Val;
                            entry.SeqNum = seqNum;

                            m_notifier.NotifyEntry(name, entry.Value, notifyFlags);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                var outmsg = Message.EntryAssign(entry.Name, id, msg.SeqNumUid, msg.Val, entry.Flags);
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                queueOutgoing(outmsg, null, conn);
                            }
                            break;
                        }
                    case EntryUpdate:
                        id = msg.Id;
                        if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                        {
                            monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                            monitorToUnlock.Dispose();
                            Debug(Logger.Instance, "received update to unknown entyr");
                            return;
                        }

                        entry = m_idMap[(int)id];

                        seqNum = new SequenceNumber(msg.SeqNumUid);

                        if (seqNum <= entry.SeqNum) return;

                        entry.Value = msg.Val;
                        entry.SeqNum = seqNum;

                        if (entry.IsPersistent()) m_persistentDirty = true;
                        m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyUpdate);

                        if (m_server && m_queueOutgoing != null)
                        {
                            var queueOutgoing = m_queueOutgoing;
                            monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                            monitorToUnlock.Dispose();
                            queueOutgoing(msg, null, conn);
                        }
                        break;
                    case FlagsUpdate:
                        {
                            id = msg.Id;
                            if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                            {
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                Debug(Logger.Instance, "reeived flags update to unknown entry");
                                return;
                            }

                            entry = m_idMap[(int)id];

                            if (entry.Flags == (EntryFlags)msg.Flags) return;

                            if ((entry.Flags & EntryFlags.Persistent) != ((EntryFlags)msg.Flags & EntryFlags.Persistent))
                                m_persistentDirty = true;

                            entry.Flags = (EntryFlags)msg.Flags;

                            m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyFlagsChanged);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case EntryDelete:
                        {
                            id = msg.Id;
                            if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                            {
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                Debug(Logger.Instance, "received delete to unknown entry");
                                return;
                            }


                            entry = m_idMap[(int)id];

                            if (entry.IsPersistent()) m_persistentDirty = true;

                            m_idMap[(int)id] = null;

                            if (m_entries.TryGetValue(entry.Name, out entry))
                            {
                                m_entries.Remove(entry.Name);

                                m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyDelete);
                            }

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case ClearEntries:
                        {
                            DeleteAllEntriesImpl();

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                                monitorToUnlock.Dispose();
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case ExecuteRpc:
                        if (!m_server) return;
                        id = msg.Id;
                        if (id >= m_idMap.Count || m_idMap[(int)id] == null)
                        {
                            monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                            monitorToUnlock.Dispose();
                            Debug(Logger.Instance, "received RPC call to unknown entry");
                            return;
                        }
                        entry = m_idMap[(int)id];
                        if (!entry.Value.IsRpc())
                        {
                            monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                            monitorToUnlock.Dispose();
                            Debug(Logger.Instance, "received RPC call to non-RPC entry");
                            return;
                        }
                        ConnectionInfo connInfo = conn.GetConnectionInfo();
                        m_rpcServer.ProcessRpc(entry.Name, msg, entry.RpcCallback, conn.Uid, message =>
                        {
                            connWeak.TryGetTarget(out NetworkConnection c);
                            if (c != null && !c.Disposed)
                                c.QueueOutgoing(message);
                        }, ref connInfo);
                        break;
                    case RpcResponse:
                        if (m_server) return;
                        if (!msg.Val.IsRpc()) return; //Not an RPC message
                        m_rpcResults.Add((msg.Id, msg.SeqNumUid), msg.Val.GetRpc());
                        m_monitor.PulseAll();
                        break;
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public void GetInitialAssignments(NetworkConnection conn, List<Message> msgs)
        {
            using(m_monitor.Enter())
            {
                conn.SetState(NetworkConnection.State.Synchronized);
                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    msgs.Add(Message.EntryAssign(i.Key, entry.Id, entry.SeqNum.Value, entry.Value, entry.Flags));
                }
            }
        }

        private void ResizeIdMap(uint newSize)
        {
            int currentSize = m_idMap.Count;

            if (newSize > currentSize)
            {
                if (newSize > m_idMap.Capacity)
                    m_idMap.Capacity = (int)newSize;
                m_idMap.AddRange(Enumerable.Repeat<Entry>(null, (int)newSize - currentSize));
            }
        }

        public void ApplyInitialAssignments(NetworkConnection conn, Message[] msgs, bool newServer, List<Message> outMsgs)
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (m_server) return;

                conn.SetState(NetworkConnection.State.Synchronized);

                List<Message> updateMsgs = new List<Message>();

                foreach (var i in m_entries) i.Value.Id = 0xffff;

                m_idMap.Clear();

                foreach (var msg in msgs)
                {
                    if (!msg.Is(EntryAssign))
                    {
                        Debug(Logger.Instance, "client: received non-entry assignment request?");
                        continue;
                    }

                    uint id = msg.Id;

                    if (id == 0xffff)
                    {
                        Debug(Logger.Instance, "client: received entry assignment request?");
                        continue;
                    }

                    SequenceNumber seqNum = new SequenceNumber(msg.SeqNumUid);
                    string name = msg.Str;


                    if (!m_entries.TryGetValue(name, out Entry entry))
                    {
                        entry = new Entry(name)
                        {
                            Value = msg.Val,
                            Flags = (EntryFlags)msg.Flags,
                            SeqNum = seqNum
                        };
                        m_notifier.NotifyEntry(name, entry.Value, NotifyFlags.NotifyNew);
                        m_entries.Add(name, entry);
                    }
                    else
                    {
                        if (!newServer && seqNum <= entry.SeqNum)
                        {
                            updateMsgs.Add(Message.EntryUpdate(entry.Id, entry.SeqNum.Value, entry.Value));
                        }
                        else
                        {
                            entry.Value = msg.Val;
                            entry.SeqNum = seqNum;
                            NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate;

                            if (conn.ProtoRev >= 0x0300)
                            {
                                if (entry.Flags != (EntryFlags)msg.Flags) notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                entry.Flags = (EntryFlags)msg.Flags;
                            }

                            m_notifier.NotifyEntry(name, entry.Value, notifyFlags);
                        }
                    }

                    entry.Id = id;
                    if (id >= m_idMap.Count) ResizeIdMap(id + 1);
                    m_idMap[(int)id] = entry;
                }

                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    if (entry.Id != 0xffff) continue;
                    outMsgs.Add(Message.EntryAssign(entry.Name, entry.Id, entry.SeqNum.Value, entry.Value, entry.Flags));
                }

                var queueOutgoing = m_queueOutgoing;
                IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                monitorToUnlock.Dispose();
                foreach (var msg in updateMsgs) queueOutgoing(msg, null, null);
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public Value GetEntryValue(string name)
        {
            using(m_monitor.Enter())
            {
                if (m_entries.TryGetValue(name, out Entry entry))
                {
                    //Grabbed
                    return entry.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool SetDefaultEntryValue(string name, Value value)
        {
            if (value == null) return false; // can't compare to a null value
            if (string.IsNullOrEmpty(name)) return false; // can't compare enpty name
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (m_entries.TryGetValue(name, out Entry newEntry)) // entry already exists
                {
                    var oldValue = newEntry.Value;
                    if (oldValue != null && oldValue.Type == value.Type) return true;
                    else return false;
                }

                // if we've gotten here, entry does not exist, and we can write it.
                newEntry = new Entry(name);
                m_entries.Add(name, newEntry);

                var entry = newEntry;

                entry.Value = value;

                // if we're the server, assign an id if it doesn't have one
                if (m_server && entry.Id == 0xffff)
                {
                    int id = m_idMap.Count;
                    entry.Id = (uint)id;
                    m_idMap.Add(entry);
                }

                // notify (for local listeners)
                if (m_notifier.LocalNotifiers())
                {
                    // always a new entry if we got this far
                    m_notifier.NotifyEntry(name, value, NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal);
                }

                // generate message
                if (m_queueOutgoing == null) return true;
                var queueOutgoing = m_queueOutgoing;
                var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);

                IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                monitorToUnlock.Dispose();
                queueOutgoing(msg, null, null);
                return true;
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public bool SetEntryValue(string name, Value value)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (value == null) return true;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    entry = new Entry(name);
                    m_entries.Add(name, entry);
                }
                var oldValue = entry.Value;
                if (oldValue != null && oldValue.Type != value.Type)
                {
                    return false; //Type mismatch error;
                }
                entry.Value = value;

                if (m_server && entry.Id == 0xffff)
                {
                    uint id = (uint)m_idMap.Count;
                    entry.Id = id;
                    m_idMap.Add(entry);
                }

                if (entry.IsPersistent() && oldValue != value) m_persistentDirty = true;

                if (m_notifier.LocalNotifiers())
                {
                    if (oldValue == null)
                    {
                        m_notifier.NotifyEntry(name, value, (NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                    }
                    else if (oldValue != value)
                    {
                        m_notifier.NotifyEntry(name, value, (NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal));
                    }
                }

                if (m_queueOutgoing == null) return true;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null)
                {
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
                else if (oldValue != value)
                {
                    ++entry.SeqNum;
                    if (entry.Id != 0xffff)
                    {
                        var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                        monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                        monitorToUnlock.Dispose();
                        queueOutgoing(msg, null, null);
                    }
                }
                return true;
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public void SetEntryTypeValue(string name, Value value)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (value == null) return;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    entry = new Entry(name);
                    m_entries.Add(name, entry);
                }
                var oldValue = entry.Value;
                entry.Value = value;
                if (oldValue != null && oldValue == value) return;

                if (m_server && entry.Id == 0xffff)
                {
                    int id = m_idMap.Count;
                    entry.Id = (uint)id;
                    m_idMap.Add(entry);
                }

                if (entry.IsPersistent()) m_persistentDirty = true;

                if (m_notifier.LocalNotifiers())
                {
                    if (oldValue == null)
                    {
                        m_notifier.NotifyEntry(name, value, NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal);
                    }
                    else
                    {
                        m_notifier.NotifyEntry(name, value, NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal);
                    }
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    if (entry.Id != 0xffff)
                    {
                        var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                        monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                        monitorToUnlock.Dispose();
                        queueOutgoing(msg, null, null);
                    }
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public void SetEntryFlags(string name, EntryFlags flags)
        {
            if (string.IsNullOrEmpty(name)) return;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    //Key does not exist. Return
                    return;
                }
                if (entry.Flags == flags) return;

                if ((entry.Flags & EntryFlags.Persistent) != (flags & EntryFlags.Persistent))
                    m_persistentDirty = true;

                entry.Flags = flags;

                m_notifier.NotifyEntry(name, entry.Value, NotifyFlags.NotifyFlagsChanged | NotifyFlags.NotifyLocal);

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                uint id = entry.Id;
                if (id != 0xffff)
                {
                    IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(Message.FlagsUpdate(id, flags), null, null);
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public EntryFlags GetEntryFlags(string name)
        {
            using(m_monitor.Enter())
            {
                if (m_entries.TryGetValue(name, out Entry entry))
                {
                    //Grabbed
                    return entry.Flags;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void DeleteEntry(string name)
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (!m_entries.TryGetValue(name, out Entry entry)) return;
                uint id = entry.Id;
                if (entry.IsPersistent()) m_persistentDirty = true;


                m_entries.Remove(name);

                if (id < m_idMap.Count) m_idMap[(int)id] = null;
                if (entry.Value == null) return;

                m_notifier.NotifyEntry(name, entry.Value, (NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));

                if (id != 0xffff)
                {
                    if (m_queueOutgoing == null) return;
                    var queueOutgoing = m_queueOutgoing;
                    IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(Message.EntryDelete(id), null, null);
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        private void DeleteAllEntriesImpl()
        {
            if (m_entries.Count == 0) return;

            // only delete non-persistent values
            // can't erase without invalidating iterators, so build
            // a new dictionary

            Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
            foreach (var i in m_entries)
            {
                var entry = i.Value;
                if (!entry.IsPersistent())
                {
                    // notify it's getting deleted
                    if (m_notifier.LocalNotifiers())
                    {
                        m_notifier.NotifyEntry(i.Key, i.Value.Value, NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal);
                    }
                    // remove it from idmap
                    if (entry.Id != 0xffff) m_idMap[(int)entry.Id] = null;
                }
                else
                {
                    // Add it to new entries
                    entries.Add(i.Key, i.Value);
                }
            }
            m_entries = entries;
        }

        public void DeleteAllEntries()
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (m_entries.Count == 0) return;

                DeleteAllEntriesImpl();

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                monitorToUnlock.Dispose();
                queueOutgoing(Message.ClearEntries(), null, null);
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            if (prefix == null) prefix = string.Empty;
            using(m_monitor.Enter())
            {
                List<EntryInfo> infos = new List<EntryInfo>();
                foreach (var i in m_entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    Entry entry = i.Value;
                    var value = entry.Value;
                    if (value == null) continue;
                    if (types != 0 && (types & value.Type) == 0) continue;
                    EntryInfo info = new EntryInfo(i.Key, value.Type, entry.Flags, (uint)value.LastChange);
                    infos.Add(info);
                }
                return infos;
            }
        }

        public void NotifyEntries(string prefix, EntryListenerCallback only = null)
        {
            using(m_monitor.Enter())
            {
                foreach (var i in m_entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    m_notifier.NotifyEntry(i.Key, i.Value.Value, NotifyFlags.NotifyImmediate, only);
                }
            }
        }



        public void CreateRpc(string name, IList<byte> def, RpcCallback callback)
        {
            if (string.IsNullOrEmpty(name) || def == null || def.Count == 0 || callback == null) return;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                if (!m_server) return;

                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    entry = new Entry(name);
                    m_entries.Add(name, entry);
                }

                var oldValue = entry.Value;
                var value = Value.MakeRpc(def);
                entry.Value = value;
                entry.RpcCallback = callback;
                m_rpcServer.Start();

                if (oldValue != null && oldValue == value) return;

                if (entry.Id == 0xffff)
                {
                    int id = m_idMap.Count;
                    entry.Id = (uint)id;
                    m_idMap.Add(entry);
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }

            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public void CreatePolledRpc(string name, IList<byte> def)
        {
            if (string.IsNullOrEmpty(name) || def == null || def.Count == 0) return;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                if (!m_server) return;

                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    entry = new Entry(name);
                    m_entries.Add(name, entry);
                }

                var oldValue = entry.Value;
                var value = Value.MakeRpc(def);
                entry.Value = value;
                entry.RpcCallback = null;

                if (oldValue != null && oldValue == value) return;

                if (entry.Id == 0xffff)
                {
                    int id = m_idMap.Count;
                    entry.Id = (uint)id;
                    m_idMap.Add(entry);
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public long CallRpc(string name, IList<byte> param)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                IDisposable monitorToUnlock;
                if (!m_entries.TryGetValue(name, out Entry entry))
                {
                    return 0;
                }
                if (!entry.Value.IsRpc()) return 0;

                ++entry.RpcCallUid;

                if (entry.RpcCallUid > 0xffff) entry.RpcCallUid = 0;
                uint combinedUid = (entry.Id << 16) | entry.RpcCallUid;
                var msg = Message.ExecuteRpc(entry.Id, entry.RpcCallUid, param);
                if (m_server)
                {
                    var rpcCallback = entry.RpcCallback;
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    ConnectionInfo connInfo = new ConnectionInfo("Server", "localhost", 0, Support.Timestamp.Now(), 0x0300);
                    m_rpcServer.ProcessRpc(name, msg, rpcCallback, 0xffff, message =>
                    {
                        using(m_monitor.Enter())
                        {
                            m_rpcResults.Add((message.Id, message.SeqNumUid), message.Val.GetRpc());
                            m_monitor.PulseAll();
                        }
                    }, ref connInfo);
                }
                else
                {
                    var queueOutgoing = m_queueOutgoing;
                    monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                    monitorToUnlock.Dispose();
                    queueOutgoing(msg, null, null);
                }
                return combinedUid;

            }
            finally
            {
                monitor?.Dispose();
            }
        }

        public async Task<IList<byte>> GetRpcResultAsync(long callUid, CancellationToken token)
        {
            IDisposable monitor = null;
            try
            {
                monitor = await m_monitor.EnterAsync(token).ConfigureAwait(false);
                if (!m_blockingRpcCalls.Add(callUid)) return null;
                for (;;)
                {
                    var pair = ((uint)callUid >> 16, (uint)callUid & 0xffff);
                    if (!m_rpcResults.TryGetValue(pair, out byte[] str))
                    {
                        if (m_terminating)
                        {
                            m_blockingRpcCalls.Remove(callUid);
                            return null;
                        }

                        await m_monitor.WaitAsync(token).ConfigureAwait(false);
                        if (token.IsCancellationRequested)
                        {
                            m_blockingRpcCalls.Remove(callUid);
                            return null;
                        }
                        if (m_terminating)
                        {
                            m_blockingRpcCalls.Remove(callUid);
                            return null;
                        }
                        continue;
                    }
                    byte[] result = new byte[str.Length];
                    Array.Copy(str, result, result.Length);
                    m_blockingRpcCalls.Remove(callUid);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                // Operation canceled. Return null.
                return null;
            }
            finally
            {

                monitor?.Dispose();
            }
        }

        public bool GetRpcResult(bool blocking, long callUid, out byte[] result)
        {
            return GetRpcResult(blocking, callUid, Timeout.InfiniteTimeSpan, out result);
        }

        public bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out byte[] result)
        {
            IDisposable monitor = null;
            try
            {
                monitor = m_monitor.Enter();
                if (!m_blockingRpcCalls.Add(callUid))
                {
                    result = null;
                    return false;
                }
                DateTime startTime = DateTime.UtcNow;

                for (;;)
                {
                    var pair = ((uint)callUid >> 16, (uint)callUid & 0xffff);
                    if (!m_rpcResults.TryGetValue(pair, out byte[] str))
                    {
                        if (!blocking || m_terminating)
                        {
                            result = null;
                            m_blockingRpcCalls.Remove(callUid);
                            return false;
                        }
                        CancellationTokenSource source = new CancellationTokenSource();
                        var task = m_monitor.WaitAsync(source.Token);
                        TimeSpan waitTimeout = timeout;
                        if (timeout != Timeout.InfiniteTimeSpan)
                        {
                            DateTime now = DateTime.UtcNow;
                            if (now < startTime + timeout)
                            {
                                // We still have time to wait.
                                waitTimeout = (startTime + timeout) - now;
                            }
                            else
                            {
                                // We're past the wait time. No need to wait anymore
                                waitTimeout = TimeSpan.Zero;
                            }
                        }
                        bool success = task.Wait(waitTimeout);
                        if (!success || m_terminating)
                        {
                            source.Cancel();
                            // Call wait again without timeout to wait for lock to be reaquired
                            // ReSharper disable once MethodSupportsCancellation
                            task.WaitWithoutException();
                            m_blockingRpcCalls.Remove(callUid);
                            result = null;
                            return false;
                        }
                        continue;
                    }
                    byte[] vresult = new byte[str.Length];
                    Array.Copy(str, vresult, vresult.Length);
                    result = vresult;
                    m_blockingRpcCalls.Remove(callUid);
                    return true;
                }
            }
            finally
            {
                monitor?.Dispose();
            }
        }
    }
}
