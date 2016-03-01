using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables.Extensions;

namespace NetworkTables
{
    internal class Notifier : IDisposable
    {
        private static Notifier s_instance;
        private static bool s_destroyed;
        private volatile bool m_localNotifiers;

        private Thread m_thread;

        private struct EntryListener
        {
            public EntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
            {
                Prefix = prefix;
                Callback = callback;
                Flags = flags;
            }

            public string Prefix { get; }
            public EntryListenerCallback Callback { get; internal set; }
            public NotifyFlags Flags { get; }
        }

        private readonly List<EntryListener> m_entryListeners = new List<EntryListener>();
        private readonly List<ConnectionListenerCallback> m_connListeners = new List<ConnectionListenerCallback>();

        private struct EntryNotification
        {
            public EntryNotification(string name, Value value, NotifyFlags flags,
                EntryListenerCallback only)
            {
                Name = name;
                Value = value;
                Flags = flags;
                Only = only;
            }

            public string Name { get; }
            public Value Value { get; }
            public NotifyFlags Flags { get; }
            public EntryListenerCallback Only { get; }
        }

        private readonly Queue<EntryNotification> m_entryNotifications = new Queue<EntryNotification>();

        private struct ConnectionNotification
        {
            public ConnectionNotification(bool connected, ConnectionInfo connInfo, ConnectionListenerCallback only)
            {
                Connected = connected;
                ConnInfo = connInfo;
                Only = only;
            }
            public bool Connected { get; }
            public ConnectionInfo ConnInfo { get; }
            public ConnectionListenerCallback Only { get; }
        }

        private readonly Queue<ConnectionNotification> m_connNotifications = new Queue<ConnectionNotification>();

        public static Notifier Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Notifier d = new Notifier();
                    Interlocked.CompareExchange(ref s_instance, d, null);
                }
                return s_instance;
            }
        }

        private Notifier()
        {
            m_active = false;
            m_localNotifiers = false;
            s_destroyed = false;
        }

        public void Dispose()
        {
            s_destroyed = true;
            Stop();
        }

        private void ThreadMain()
        {
            bool lockEntered = false;
            try
            {
                Monitor.Enter(m_mutex, ref lockEntered);
                while (m_active)
                {
                    while (m_entryNotifications.Count == 0 && m_connNotifications.Count == 0)
                    {
                        m_cond.Wait(m_mutex, ref lockEntered);
                        if (!m_active) return;
                    }

                    while (m_entryNotifications.Count != 0)
                    {
                        if (!m_active) return;
                        var item = m_entryNotifications.Dequeue();
                        if (item.Value == null) continue;
                        string name = item.Name;

                        if (item.Only != null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            item.Only(0, name, item.Value, item.Flags);
                            Monitor.Enter(m_mutex, ref lockEntered);
                            continue;
                        }

                        for (int i = 0; i < m_entryListeners.Count; ++i)
                        {
                            if (m_entryListeners[i].Callback == null) continue;

                            NotifyFlags listenFlags = m_entryListeners[i].Flags;
                            NotifyFlags flags = item.Flags;
                            const NotifyFlags assignBoth = (NotifyFlags.NotifyUpdate | NotifyFlags.NotifyFlagsChanged);

                            if ((flags & assignBoth) == assignBoth)
                            {
                                if ((listenFlags & assignBoth) == 0) continue;
                                listenFlags &= ~assignBoth;
                                flags &= ~assignBoth;
                            }
                            if ((flags & ~listenFlags) != 0) continue;

                            if (!name.StartsWith(m_entryListeners[i].Prefix)) continue;

                            var callback = m_entryListeners[i].Callback;

                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            callback((i + 1), name, item.Value, item.Flags);
                            Monitor.Enter(m_mutex, ref lockEntered);
                        }

                    }

                    while (m_connNotifications.Count != 0)
                    {
                        if (!m_active) return;
                        var item = m_connNotifications.Dequeue();

                        if (item.Only != null)
                        {
                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            item.Only(0, item.Connected, item.ConnInfo);
                            Monitor.Enter(m_mutex, ref lockEntered);
                            continue;
                        }

                        for (int i = 0; i < m_connListeners.Count; ++i)
                        {
                            if (m_connListeners[i] == null) continue;
                            var callback = m_connListeners[i];

                            Monitor.Exit(m_mutex);
                            lockEntered = false;
                            callback((i + 1), item.Connected, item.ConnInfo);
                            Monitor.Enter(m_mutex, ref lockEntered);
                        }
                    }
                }
            }
            finally
            {
                if (lockEntered) Monitor.Exit(m_mutex);
            }
        }

        private volatile bool m_active;
        private readonly object m_mutex = new object();
        private readonly AutoResetEvent m_cond = new AutoResetEvent(false);

        public void Start()
        {
            lock (m_mutex)
            {
                if (m_active) return;
                m_active = true;
            }
            m_thread = new Thread(ThreadMain);
            m_thread.IsBackground = true;
            m_thread.Name = "Notifier Thread";
            m_thread.Start();
        }

        public void Stop()
        {
            m_active = false;
            //Notify condition so thread terminates.
            m_cond.Set();
            TimeSpan timeout = TimeSpan.FromSeconds(1);
            if (m_thread == null) return;
            bool joined = m_thread.Join(timeout);
            if (!joined)
            {
                m_thread?.Abort();
            }
        }

        public bool LocalNotifiers()
        {
            return m_localNotifiers;
        }

        public static bool Destroyed()
        {
            return s_destroyed;
        }

        public int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            lock (m_mutex)
            {
                int uid = m_entryListeners.Count;
                m_entryListeners.Add(new EntryListener(prefix, callback, flags));
                if ((flags & NotifyFlags.NotifyLocal) != 0) m_localNotifiers = true;
                return uid + 1;
            }
        }

        public void RemoveEntryListener(int entryListenerUid)
        {
            --entryListenerUid;
            lock (m_mutex)
            {
                if (entryListenerUid < m_entryListeners.Count)
                {
                    var listener = m_entryListeners[entryListenerUid];
                    listener.Callback = null;
                }
            }
        }

        public void NotifyEntry(string name, Value value, NotifyFlags flags, EntryListenerCallback only = null)
        {
            if (!m_active) return;
            if ((flags & NotifyFlags.NotifyLocal) != 0 && !m_localNotifiers) return;
            lock (m_mutex)
            {
                m_entryNotifications.Enqueue(new EntryNotification(name, value, flags, only));
            }
            m_cond.Set();
        }

        public int AddConnectionListener(ConnectionListenerCallback callback)
        {
            lock (m_mutex)
            {
                int uid = m_connListeners.Count;
                m_connListeners.Add(callback);
                return uid + 1;
            }
        }

        public void RemoveConnectionListener(int connListenerUid)
        {
            --connListenerUid;
            lock (m_mutex)
            {
                if (connListenerUid < m_connListeners.Count)
                {
                    m_connListeners[connListenerUid] = null;
                }
            }
        }

        public void NotifyConnection(bool connected, ConnectionInfo connInfo, ConnectionListenerCallback only = null)
        {
            if (!m_active) return;
            lock (m_mutex)
            {
                m_connNotifications.Enqueue(new ConnectionNotification(connected, connInfo, only));
            }
            m_cond.Set();
        }
    }
}
