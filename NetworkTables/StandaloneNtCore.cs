﻿using System;
using System.Collections.Generic;
using System.Linq;
using NetworkTables.Exceptions;
using NetworkTables.Logging;
using NetworkTables.Tables;

namespace NetworkTables
{
    public class StandaloneNtCore : IStandaloneTable, IDisposable
    {
        internal Storage m_storage;
        internal Notifier m_notifier;
        internal Dispatcher m_dispatcher;
        internal RpcServer m_rpcServer;

        private readonly object m_lockObject = new object();

        

        public StandaloneNtCore()
        {
            m_notifier = new Notifier();
            m_rpcServer = new RpcServer();
            m_storage = new Storage(m_notifier, m_rpcServer);
            m_dispatcher = new Dispatcher(m_storage, m_notifier);
        }

        public void Dispose()
        {
            StopClient();
            StopServer();
            StopNotifier();
            StopRpcServer();
            m_dispatcher.Dispose();
            m_storage.Dispose();
            m_rpcServer.Dispose();
            m_notifier.Dispose();
        }

        public Value GetEntryValue(string name)
        {
            return m_storage.GetEntryValue(name);
        }

        public bool SetEntryValue(string name, Value value)
        {
            return m_storage.SetEntryValue(name, value);
        }


        public bool SetDefaultEntryValue(string name, Value value)
        {
            return m_storage.SetDefaultEntryValue(name, value);
        }

        public void SetEntryTypeValue(string name, Value value)
        {
            m_storage.SetEntryTypeValue(name, value);
        }

        public void SetEntryFlags(string name, EntryFlags flags)
        {
            m_storage.SetEntryFlags(name, flags);
        }

        public EntryFlags GetEntryFlags(string name)
        {
            return m_storage.GetEntryFlags(name);
        }

        public void DeleteEntry(string name)
        {
            m_storage.DeleteEntry(name);
        }

        public void DeleteAllEntries()
        {
            m_storage.DeleteAllEntries();
        }

        public List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            return m_storage.GetEntryInfo(prefix, types);
        }

        public NtType GetType(string key)
        {
            var v = GetEntryValue(key);
            if (v == null) return NtType.Unassigned;
            return GetEntryValue(key).Type;
        }

        public bool ContainsKey(string key)
        {
            return GetType(key) != NtType.Unassigned;
        }

        public void Flush()
        {
            m_dispatcher.Flush();
        }

        public int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            Notifier notifier = m_notifier;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                m_storage.NotifyEntries(prefix, callback);
            return uid;
        }

        public void RemoveEntryListener(int uid)
        {
            m_notifier.RemoveEntryListener(uid);
        }

        public int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            Notifier notifier = m_notifier;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) m_dispatcher.NotifyConnections(callback);
            return uid;
        }

        public void RemoveConnectionListener(int uid)
        {
            m_notifier.RemoveConnectionListener(uid);
        }

        public bool NotifierDestroyed()
        {
            return m_notifier.Destroyed();
        }

        public void StartClient(string[] ipAddresses, int port)
        {

            lock (m_lockObject)
            {
                CheckInit();
                Client = true;
                Running = true;
            }
            List<ImmutablePair<string, int>> servers = new List<ImmutablePair<string, int>>(ipAddresses.Length);
            servers.AddRange(ipAddresses.Select(ipAddress => new ImmutablePair<string, int>(ipAddress, port)));

            m_dispatcher.StartClient(servers);
        }

        public void StartServer(string persistFilename, string listenAddress, int port)
        {
            lock (m_lockObject)
            {
                CheckInit();
                Client = false;
                Running = true;
            }
            m_dispatcher.StartServer(persistFilename, listenAddress, port);
        }

        public void StopServer()
        {
            lock (m_lockObject)
            {
                Running = false;
            }
            m_dispatcher.Stop();
        }

        private string m_remoteName = "Standalone Network Table";

        public string RemoteName
        {
            get
            {
                return m_dispatcher.Identity;
            }
            set
            {
                CheckInit();
                m_dispatcher.Identity = value;
            }
        }

        public bool Client { get; private set; }
        public bool Running { get; private set; }

        private void CheckInit()
        {
            lock (m_lockObject)
            {
                if (Running)
                    throw new InvalidOperationException("Operation cannot be completed while NtCore is running");
            }
        }

        public void StartClient(string serverName, int port)
        {
            lock (m_lockObject)
            {
                CheckInit();
                Client = true;
                Running = true;
            }
            m_dispatcher.StartClient(serverName, port);
        }

        public void StopClient()
        {
            lock (m_lockObject)
            {
                Running = false;
            }
            m_dispatcher.Stop();
        }

        public void StopRpcServer()
        {
            m_rpcServer.Stop();
        }

        public void StopNotifier()
        {
            m_notifier.Stop();
        }

        public double UpdateRate
        {
            get { return m_dispatcher.UpdateRate; }
            set { m_dispatcher.UpdateRate = value; }
        }

        public List<ConnectionInfo> GetConnections()
        {
            return m_dispatcher.GetConnections();
        }

        public string SavePersistent(string filename)
        {
            return m_storage.SavePersistent(filename, false);
        }

        public string LoadPersistent(string filename, Action<int, string> warn)
        {
            return m_storage.LoadPersistent(filename, warn);
        }

        public long Now()
        {
            return Support.Timestamp.Now();
        }

        public void SetLogger(LogFunc func, LogLevel minLevel)
        {
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.MinLevel = minLevel;
        }

        public string[] LoadPersistent(string filename)
        {
            List<string> warns = new List<string>();
            var err = LoadPersistent(filename, (i, s) =>
            {
                warns.Add($"{i}: {s}");
            });
            if (err != null) throw new PersistentException($"Load Persistent Failed: {err}");
            return warns.ToArray();
        }
    }
}
