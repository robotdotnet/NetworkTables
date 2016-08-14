using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Extensions;
using NetworkTables.Logging;
using Nito.AsyncEx;
using static NetworkTables.Logging.Logger;
using static NetworkTables.Message.MsgType;

namespace NetworkTables
{
    internal partial class Storage : IDisposable
    {
        // Global storage mutex
        private readonly object m_mutex = new object();

        private readonly Notifier m_notifier;

        // Rpc support fields
        private readonly Dictionary<ImmutablePair<uint, uint>, byte[]> m_rpcResults = new Dictionary<ImmutablePair<uint, uint>, byte[]>();
        private readonly AutoResetEvent m_rpcResultsCond = new AutoResetEvent(false);
        private readonly AsyncAutoResetEvent m_rpcResultsCondAsync = new AsyncAutoResetEvent(false);
        private readonly RpcServer m_rpcServer;

        // True if the persistance state has changed
        private bool m_persistentDirty;

        private QueueOutgoingFunc m_queueOutgoing;

        private bool m_server = true;
        private bool m_terminating;


        public delegate void QueueOutgoingFunc(Message msg, NetworkConnection only, NetworkConnection except);

        private static Storage s_instance;

        internal Storage() : this(Notifier.Instance, RpcServer.Instance)
        {
        }

        internal Storage(Notifier notifier, RpcServer rpcServer)
        {
            m_notifier = notifier;
            m_rpcServer = rpcServer;
        }

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

        // Dict of all our entries organized by key
        internal Dictionary<string, Entry> Entries { get; private set; } = new Dictionary<string, Entry>();
        // Map of all our Ids
        internal List<Entry> IdMap { get; } = new List<Entry>();


        public void Dispose()
        {
            Logger.Instance.SetDefaultLogger();
            m_terminating = true;
            m_rpcResultsCond.Set();
            m_rpcResultsCondAsync.Set();
        }

        public void SetOutgoing(QueueOutgoingFunc queueOutgoing, bool server)
        {
            lock (m_mutex)
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
            lock (m_mutex)
            {
                if (id >= IdMap.Count) return NtType.Unassigned;
                Entry entry = IdMap[(int)id];
                if (entry?.Value == null) return NtType.Unassigned;
                return entry.Value.Type;
            }
        }

        public void ProcessIncoming(Message msg, NetworkConnection conn, WeakReference<NetworkConnection> connWeak)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
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
                                    if (Entries.ContainsKey(name)) return;


                                    id = (uint)IdMap.Count;
                                    entry = new Entry(name)
                                    {
                                        Value = msg.Val,
                                        Flags = (EntryFlags) msg.Flags,
                                        Id = id
                                    };
                                    Entries[name] = entry;
                                    IdMap.Add(entry);

                                    if (entry.IsPersistent()) m_persistentDirty = true;

                                    m_notifier.NotifyEntry(name, entry.Value, NotifyFlags.NotifyNew);

                                    if (m_queueOutgoing != null)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outMsg = Message.EntryAssign(name, id, entry.SeqNum.Value, msg.Val, (EntryFlags)msg.Flags);
                                        Monitor.Exit(m_mutex);
                                        lockEntered = false;
                                        queueOutgoing(outMsg, null, null);
                                    }

                                    return;
                                }
                                if (id >= IdMap.Count || IdMap[(int)id] == null)
                                {
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    Debug("server: received assignment to unknown entry");
                                    return;
                                }
                                entry = IdMap[(int)id];
                            }
                            else
                            {
                                if (id == 0xffff)
                                {
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    Debug("client: received entry assignment request?");
                                    return;
                                }
                                if (id >= IdMap.Count) ResizeIdMap(id + 1);
                                entry = IdMap[(int)id];
                                if (entry == null)
                                {
                                    Entry newEntry;
                                    if (!Entries.ContainsKey(name))
                                    {
                                        //Entry didn't exist at all.
                                        newEntry = new Entry(name)
                                        {
                                            Value = msg.Val,
                                            Flags = (EntryFlags) msg.Flags,
                                            Id = id
                                        };
                                        IdMap[(int)id] = newEntry;
                                        Entries[name] = newEntry;

                                        m_notifier.NotifyEntry(name, newEntry.Value, NotifyFlags.NotifyNew);
                                        return;
                                    }
                                    else
                                    {
                                        newEntry = Entries[name];
                                    }
                                    mayNeedUpdate = true;
                                    entry = newEntry;
                                    entry.Id = id;
                                    IdMap[(int)id] = entry;

                                    if ((EntryFlags)msg.Flags != entry.Flags)
                                    {
                                        var queueOutgoing = m_queueOutgoing;
                                        var outmsg = Message.FlagsUpdate(id, entry.Flags);
                                        Monitor.Exit(m_mutex);
                                        lockEntered = false;
                                        queueOutgoing(outmsg, null, null);
                                        Monitor.Enter(m_mutex, ref lockEntered);
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
                                    Monitor.Exit(m_mutex);
                                    lockEntered = false;
                                    queueOutgoing(outmsg, null, null);
                                }
                                return;
                            }
                            //Sanity check. Name should match id
                            if (msg.Str != entry.Name)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("entry assignment for same id with different name?");
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
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(outmsg, null, conn);
                            }
                            break;
                        }
                    case EntryUpdate:
                        id = msg.Id;
                        if (id >= IdMap.Count || IdMap[(int)id] == null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            Debug("received update to unknown entyr");
                            return;
                        }

                        entry = IdMap[(int)id];

                        seqNum = new SequenceNumber(msg.SeqNumUid);

                        if (seqNum <= entry.SeqNum) return;

                        entry.Value = msg.Val;
                        entry.SeqNum = seqNum;

                        if (entry.IsPersistent()) m_persistentDirty = true;
                        m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyUpdate);

                        if (m_server && m_queueOutgoing != null)
                        {
                            var queueOutgoing = m_queueOutgoing;
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            queueOutgoing(msg, null, conn);
                        }
                        break;
                    case FlagsUpdate:
                        {
                            id = msg.Id;
                            if (id >= IdMap.Count || IdMap[(int)id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("reeived flags update to unknown entry");
                                return;
                            }

                            entry = IdMap[(int)id];

                            if (entry.Flags == (EntryFlags)msg.Flags) return;

                            if ((entry.Flags & EntryFlags.Persistent) != ((EntryFlags)msg.Flags & EntryFlags.Persistent))
                                m_persistentDirty = true;

                            entry.Flags = (EntryFlags)msg.Flags;

                            m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyFlagsChanged);

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case EntryDelete:
                        {
                            id = msg.Id;
                            if (id >= IdMap.Count || IdMap[(int)id] == null)
                            {
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                Debug("received delete to unknown entry");
                                return;
                            }


                            entry = IdMap[(int)id];

                            if (entry.IsPersistent()) m_persistentDirty = true;

                            IdMap[(int)id] = null;

                            if (Entries.TryGetValue(entry.Name, out entry))
                            {
                                Entries.Remove(entry.Name);

                                m_notifier.NotifyEntry(entry.Name, entry.Value, NotifyFlags.NotifyDelete);
                            }

                            if (m_server && m_queueOutgoing != null)
                            {
                                var queueOutgoing = m_queueOutgoing;
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
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
                                Monitor.Exit(m_mutex);
                                lockEntered = false;
                                queueOutgoing(msg, null, conn);
                            }
                            break;
                        }
                    case ExecuteRpc:
                        if (!m_server) return;
                        id = msg.Id;
                        if (id >= IdMap.Count || IdMap[(int)id] == null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            Debug("received RPC call to unknown entry");
                            return;
                        }
                        entry = IdMap[(int)id];
                        if (!entry.Value.IsRpc())
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            Debug("received RPC call to non-RPC entry");
                            return;
                        }
                        m_rpcServer.ProcessRpc(entry.Name, msg, entry.RpcCallback, conn.Uid, message =>
                        {
                            NetworkConnection c;
                            connWeak.TryGetTarget(out c);
                            if (c != null && !c.Disposed)
                                c.QueueOutgoing(message);
                        });
                        break;
                    case RpcResponse:
                        if (m_server) return;
                        if (!msg.Val.IsRpc()) return; //Not an RPC message
                        m_rpcResults.Add(new ImmutablePair<uint, uint>(msg.Id, msg.SeqNumUid), msg.Val.GetRpc());
                        m_rpcResultsCond.Set();
                        m_rpcResultsCondAsync.Set();
                        break;
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void GetInitialAssignments(NetworkConnection conn, List<Message> msgs)
        {
            lock (m_mutex)
            {
                conn.SetState(NetworkConnection.State.Synchronized);
                foreach (var i in Entries)
                {
                    Entry entry = i.Value;
                    msgs.Add(Message.EntryAssign(i.Key, entry.Id, entry.SeqNum.Value, entry.Value, entry.Flags));
                }
            }
        }

        private void ResizeIdMap(uint newSize)
        {
            int currentSize = IdMap.Count;

            if (newSize > currentSize)
            {
                if (newSize > IdMap.Capacity)
                    IdMap.Capacity = (int)newSize;
                IdMap.AddRange(Enumerable.Repeat<Entry>(null, (int)newSize - currentSize));
            }
        }

        public void ApplyInitialAssignments(NetworkConnection conn, Message[] msgs, bool newServer, List<Message> outMsgs)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (m_server) return;

                conn.SetState(NetworkConnection.State.Synchronized);

                List<Message> updateMsgs = new List<Message>();

                foreach (var i in Entries) i.Value.Id = 0xffff;

                IdMap.Clear();

                foreach (var msg in msgs)
                {
                    if (!msg.Is(EntryAssign))
                    {
                        Debug("client: received non-entry assignment request?");
                        continue;
                    }

                    uint id = msg.Id;

                    if (id == 0xffff)
                    {
                        Debug("client: received entry assignment request?");
                        continue;
                    }

                    SequenceNumber seqNum = new SequenceNumber(msg.SeqNumUid);
                    string name = msg.Str;


                    Entry entry;
                    if (!Entries.TryGetValue(name, out entry))
                    {
                        entry = new Entry(name)
                        {
                            Value = msg.Val,
                            Flags = (EntryFlags) msg.Flags,
                            SeqNum = seqNum
                        };
                        m_notifier.NotifyEntry(name, entry.Value, NotifyFlags.NotifyNew);
                        Entries.Add(name, entry);
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
                    if (id >= IdMap.Count) ResizeIdMap(id + 1);
                    IdMap[(int)id] = entry;
                }

                foreach (var i in Entries)
                {
                    Entry entry = i.Value;
                    if (entry.Id != 0xffff) continue;
                    outMsgs.Add(Message.EntryAssign(entry.Name, entry.Id, entry.SeqNum.Value, entry.Value, entry.Flags));
                }

                var queueOutgoing = m_queueOutgoing;
                Monitor.Exit(m_mutex);
                lockEntered = false;
                foreach (var msg in updateMsgs) queueOutgoing(msg, null, null);
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public Value GetEntryValue(string name)
        {
            lock (m_mutex)
            {
                Entry entry;
                if (Entries.TryGetValue(name, out entry))
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
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry newEntry;
                if (Entries.TryGetValue(name, out newEntry)) // entry already exists
                {
                    var oldValue = newEntry.Value;
                    if (oldValue != null && oldValue.Type == value.Type) return true;
                    else return false;
                }

                // if we've gotten here, entry does not exist, and we can write it.
                newEntry = new Entry(name);
                Entries.Add(name, newEntry);

                var entry = newEntry;

                entry.Value = value;

                // if we're the server, assign an id if it doesn't have one
                if (m_server && entry.Id == 0xffff)
                {
                    int id = IdMap.Count;
                    entry.Id = (uint)id;
                    IdMap.Add(entry);
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

                Monitor.Exit(m_mutex);
                lockEntered = false;
                queueOutgoing(msg, null, null);
                return true;
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public bool SetEntryValue(string name, Value value)
        {
            if (string.IsNullOrEmpty(name)) return true;
            if (value == null) return true;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
                {
                    entry = new Entry(name);
                    Entries.Add(name, entry);
                }
                var oldValue = entry.Value;
                if (oldValue != null && oldValue.Type != value.Type)
                {
                    return false; //Type mismatch error;
                }
                entry.Value = value;

                if (m_server && entry.Id == 0xffff)
                {
                    uint id = (uint)IdMap.Count;
                    entry.Id = id;
                    IdMap.Add(entry);
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
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else if (oldValue != value)
                {
                    ++entry.SeqNum;
                    if (entry.Id != 0xffff)
                    {
                        var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        queueOutgoing(msg, null, null);
                    }
                }
                return true;
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void SetEntryTypeValue(string name, Value value)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (value == null) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
                {
                    entry = new Entry(name);
                    Entries.Add(name, entry);
                }
                var oldValue = entry.Value;
                entry.Value = value;
                if (oldValue != null && oldValue == value) return;

                if (m_server && entry.Id == 0xffff)
                {
                    int id = IdMap.Count;
                    entry.Id = (uint)id;
                    IdMap.Add(entry);
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
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    if (entry.Id != 0xffff)
                    {
                        var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        queueOutgoing(msg, null, null);
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void SetEntryFlags(string name, EntryFlags flags)
        {
            if (string.IsNullOrEmpty(name)) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
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
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(Message.FlagsUpdate(id, flags), null, null);
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public EntryFlags GetEntryFlags(string name)
        {
            lock (m_mutex)
            {
                Entry entry;
                if (Entries.TryGetValue(name, out entry))
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
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!Entries.TryGetValue(name, out entry)) return;
                uint id = entry.Id;
                if (entry.IsPersistent()) m_persistentDirty = true;


                Entries.Remove(name);

                if (id < IdMap.Count) IdMap[(int)id] = null;
                if (entry.Value == null) return;

                m_notifier.NotifyEntry(name, entry.Value, (NotifyFlags.NotifyDelete | NotifyFlags.NotifyLocal));

                if (id != 0xffff)
                {
                    if (m_queueOutgoing == null) return;
                    var queueOutgoing = m_queueOutgoing;
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(Message.EntryDelete(id), null, null);
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        private void DeleteAllEntriesImpl()
        {
            if (Entries.Count == 0) return;

            // only delete non-persistent values
            // can't erase without invalidating iterators, so build
            // a new dictionary

            Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
            foreach (var i in Entries)
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
                    if (entry.Id != 0xffff) IdMap[(int)entry.Id] = null;
                }
                else
                {
                    // Add it to new entries
                    entries.Add(i.Key, i.Value);
                }
            }
            Entries = entries;
        }

        public void DeleteAllEntries()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (Entries.Count == 0) return;

                DeleteAllEntriesImpl();

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                Monitor.Exit(m_mutex);
                lockEntered = false;
                queueOutgoing(Message.ClearEntries(), null, null);
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            if (prefix == null) prefix = string.Empty;
            lock (m_mutex)
            {
                List<EntryInfo> infos = new List<EntryInfo>();
                foreach (var i in Entries)
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
            lock (m_mutex)
            {
                foreach (var i in Entries)
                {
                    if (!i.Key.StartsWith(prefix)) continue;
                    m_notifier.NotifyEntry(i.Key, i.Value.Value, NotifyFlags.NotifyImmediate, only);
                }
            }
        }


        public void CreateRpc(string name, byte[] def, RpcCallback callback)
        {
            if (string.IsNullOrEmpty(name) || def == null || def.Length == 0 || callback == null) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (!m_server) return;

                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
                {
                    entry = new Entry(name);
                    Entries.Add(name, entry);
                }

                var oldValue = entry.Value;
                var value = Value.MakeRpc(def);
                entry.Value = value;
                entry.RpcCallback = callback;
                m_rpcServer.Start();

                if (oldValue != null && oldValue == value) return;

                if (entry.Id == 0xffff)
                {
                    int id = IdMap.Count;
                    entry.Id = (uint)id;
                    IdMap.Add(entry);
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }

            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public void CreatePolledRpc(string name, byte[] def)
        {
            if (string.IsNullOrEmpty(name) || def == null || def.Length == 0) return;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                if (!m_server) return;

                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
                {
                    entry = new Entry(name);
                    Entries.Add(name, entry);
                }

                var oldValue = entry.Value;
                var value = Value.MakeRpc(def);
                entry.Value = value;
                entry.RpcCallback = null;

                if (oldValue != null && oldValue == value) return;

                if (entry.Id == 0xffff)
                {
                    int id = IdMap.Count;
                    entry.Id = (uint)id;
                    IdMap.Add(entry);
                }

                if (m_queueOutgoing == null) return;
                var queueOutgoing = m_queueOutgoing;
                if (oldValue == null || oldValue.Type != value.Type)
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryAssign(name, entry.Id, entry.SeqNum.Value, value, entry.Flags);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                else
                {
                    ++entry.SeqNum;
                    var msg = Message.EntryUpdate(entry.Id, entry.SeqNum.Value, value);
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public long CallRpc(string name, byte[] param)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                Entry entry;
                if (!Entries.TryGetValue(name, out entry))
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
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    m_rpcServer.ProcessRpc(name, msg, rpcCallback, 0xffff, message =>
                    {
                        lock (m_mutex)
                        {
                            m_rpcResults.Add(new ImmutablePair<uint, uint>(msg.Id, msg.SeqNumUid), msg.Val.GetRpc());
                            m_rpcResultsCond.Set();
                            m_rpcResultsCondAsync.Set();
                        }
                    });
                }
                else
                {
                    var queueOutgoing = m_queueOutgoing;
                    Monitor.Exit(m_mutex);
                    lockEntered = false;
                    queueOutgoing(msg, null, null);
                }
                return combinedUid;

            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public async Task<byte[]> GetRpcResultAsync(long callUid, CancellationToken token)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                for (;;)
                {
                    var pair = new ImmutablePair<uint, uint>((uint)callUid >> 16, (uint)callUid & 0xffff);
                    byte[] str;
                    if (!m_rpcResults.TryGetValue(pair, out str))
                    {
                        if (m_terminating) return null;
                        Monitor.Exit(m_mutex);
                        lockEntered = false;
                        await m_rpcResultsCondAsync.WaitAsync(token);
                        Monitor.Enter(m_mutex, ref lockEntered);
                        if (token.IsCancellationRequested) return null;
                        if (m_terminating) return null;
                        continue;
                    }
                    byte[] result = new byte[str.Length];
                    Array.Copy(str, result, result.Length);
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
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        public bool GetRpcResult(bool blocking, long callUid, out byte[] result)
        {
            return GetRpcResult(blocking, callUid, Timeout.InfiniteTimeSpan, out result);
        }

        public bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out byte[] result)
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                for (;;)
                {
                    var pair = new ImmutablePair<uint, uint>((uint)callUid >> 16, (uint)callUid & 0xffff);
                    byte[] str;
                    if (!m_rpcResults.TryGetValue(pair, out str))
                    {
                        if (!blocking || m_terminating)
                        {
                            result = null;
                            return false;
                        }
                        bool notTimedOut = m_rpcResultsCond.WaitTimeout(m_mutex, ref lockEntered, timeout);
                        if (!notTimedOut || m_terminating)
                        {
                            result = null;
                            return false;
                        }
                        continue;
                    }
                    result = new byte[str.Length];
                    Array.Copy(str, result, result.Length);
                    return true;
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        internal struct StoragePair : IComparable<StoragePair>
        {
            public string First { get; }
            public Value Second { get; }

            public StoragePair(string first, Value second)
            {
                First = first;
                Second = second;
            }

            public int CompareTo(StoragePair other)
            {
                return string.Compare(First, other.First, StringComparison.Ordinal);
            }
        }

        internal class Entry
        {
            public readonly string Name;
            public EntryFlags Flags;
            public uint Id;

            public RpcCallback RpcCallback;
            public uint RpcCallUid;

            public SequenceNumber SeqNum;
            public Value Value;

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
        }
    }
}
