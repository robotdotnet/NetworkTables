using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
using NetworkTables.Logging;

namespace NetworkTables
{
    public static partial class NtCore
    {
        public static Value GetEntryValue(string name)
        {
            return Storage.Instance.GetEntryValue(name);
        }

        public static bool SetEntryValue(string name, Value value)
        {
            return Storage.Instance.SetEntryValue(name, value);
        }

        public static void SetEntryTypeValue(string name, Value value)
        {
            Storage.Instance.SetEntryTypeValue(name, value);
        }

        public static void SetEntryFlags(string name, EntryFlags flags)
        {
            Storage.Instance.SetEntryFlags(name, flags);
        }

        public static EntryFlags GetEntryFlags(string name)
        {
            return Storage.Instance.GetEntryFlags(name);
        }

        public static void DeleteEntry(string name)
        {
            Storage.Instance.DeleteEntry(name);
        }

        public static void DeleteAllEntries()
        {
            Storage.Instance.DeleteAllEntries();
        }

        public static List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            return Storage.Instance.GetEntryInfo(prefix, types);
        }

        public static NtType GetType(string key)
        {
            var v = GetEntryValue(key);
            if (v == null) return NtType.Unassigned;
            return GetEntryValue(key).Type;
        }

        public static bool ContainsKey(string key)
        {
            return GetType(key) != NtType.Unassigned;
        }

        public static void Flush()
        {
            Dispatcher.Instance.Flush();
        }

        public static int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                Storage.Instance.NotifyEntries(prefix, callback);
            return uid;
        }

        public static void RemoveEntryListener(int uid)
        {
            Notifier.Instance.RemoveEntryListener(uid);
        }

        public static int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) Dispatcher.Instance.NotifyConnections(callback);
            return uid;
        }

        public static void RemoveConnectionListener(int uid)
        {
            Notifier.Instance.RemoveConnectionListener(uid);
        }

        public static bool NotifierDestroyed()
        {
            return Notifier.Instance.Destroyed();
        }

        public static void SetNetworkIdentity(string name)
        {
            Dispatcher.Instance.Identity = name;
        }

        public static void StartServer(string persistFilename, string listenAddress, int port)
        {
            Dispatcher.Instance.StartServer(persistFilename, listenAddress, port);
        }

        public static void StopServer()
        {
            Dispatcher.Instance.Stop();
        }

        public static void StartClient(string serverName, int port)
        {
            Dispatcher.Instance.StartClient(serverName, port);
        }

        public static void StopClient()
        {
            Dispatcher.Instance.Stop();
        }

        public static void StopRpcServer()
        {
            RpcServer.Instance.Stop();
        }

        public static void StopNotifier()
        {
            Notifier.Instance.Stop();
        }

        public static void SetUpdateRate(double interval)
        {
            Dispatcher.Instance.UpdateRate = interval;
        }

        public static List<ConnectionInfo> GetConnections()
        {
            return Dispatcher.Instance.GetConnections();
        }

        public static string SavePersistent(string filename)
        {
            return Storage.Instance.SavePersistent(filename, false);
        }

        public static string LoadPersistent(string filename, Action<int, string> warn)
        {
            return Storage.Instance.LoadPersistent(filename, warn);
        }

        public static long Now()
        {
            return Support.Timestamp.Now();
        }

        public static void SetLogger(LogFunc func, LogLevel minLevel)
        {
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.SetMinLevel(minLevel);
        }

        public static string[] LoadPersistent(string filename)
        {
            List<string> warns = new List<string>();
            var err = LoadPersistent(filename, (i, s) =>
            {
                warns.Add($"{i}: {s}");
            });
            if (err != null) throw new PersistentException("Load Persistent Failed");
            return warns.ToArray();
        }
    }
}
