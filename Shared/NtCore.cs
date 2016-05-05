using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
#if !CORE
using NetworkTables.Logging;
#endif
#if CORE
using NetworkTables.Native;
#endif

namespace NetworkTables
{
    public static class NtCore
    {
        public static Value GetEntryValue(string name)
        {
#if CORE
            throw new NotImplementedException("Not implemented in NetworkTablesCore yet");
#else
            return Storage.Instance.GetEntryValue(name);
#endif
        }

        public static bool SetEntryValue(string name, Value value)
        {
#if CORE
            throw new NotImplementedException("Not implemented in NetworkTablesCore yet");
#else
            return Storage.Instance.SetEntryValue(name, value);
#endif
        }


        public static bool SetDefaultEntryValue(string name, Value value)
        {
#if CORE
            throw new NotImplementedException("Not implemented in NetworkTablesCore yet");
#else
            return Storage.Instance.SetDefaultEntryValue(name, value);
#endif
        }

        public static void SetEntryTypeValue(string name, Value value)
        {
#if CORE
            throw new NotImplementedException("Not implemented in NetworkTablesCore yet");
#else
            Storage.Instance.SetEntryTypeValue(name, value);
#endif
        }

        public static void SetEntryFlags(string name, EntryFlags flags)
        {
#if CORE
            CoreMethods.SetEntryFlags(name, flags);
#else
            Storage.Instance.SetEntryFlags(name, flags);
#endif
        }

        public static EntryFlags GetEntryFlags(string name)
        {
#if CORE
            return CoreMethods.GetEntryFlags(name);
#else
            return Storage.Instance.GetEntryFlags(name);
#endif
        }

        public static void DeleteEntry(string name)
        {
#if CORE
            CoreMethods.DeleteEntry(name);
#else
            Storage.Instance.DeleteEntry(name);
#endif
        }

        public static void DeleteAllEntries()
        {
#if CORE
            CoreMethods.DeleteAllEntries();
#else
            Storage.Instance.DeleteAllEntries();
#endif
        }

        public static List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
#if CORE
            return CoreMethods.GetEntryInfo(prefix, types);
#else
            return Storage.Instance.GetEntryInfo(prefix, types);
#endif
        }

        public static NtType GetType(string key)
        {
#if CORE
            return CoreMethods.GetType(key);
#else
            var v = GetEntryValue(key);
            if (v == null) return NtType.Unassigned;
            return GetEntryValue(key).Type;
#endif
        }

        public static bool ContainsKey(string key)
        {
#if CORE
            return CoreMethods.ContainsKey(key);
#else
            return GetType(key) != NtType.Unassigned;
#endif
        }

        public static void Flush()
        {
#if CORE
            CoreMethods.Flush();
#else
            Dispatcher.Instance.Flush();
#endif
        }

        public static int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
#if CORE
            return CoreMethods.AddEntryListener(prefix, callback, flags);
#else
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                Storage.Instance.NotifyEntries(prefix, callback);
            return uid;
#endif
        }

        public static void RemoveEntryListener(int uid)
        {
#if CORE
            CoreMethods.RemoveEntryListener(uid);
#else
            Notifier.Instance.RemoveEntryListener(uid);
#endif
        }

        public static int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
#if CORE
            return CoreMethods.AddConnectionListener(callback, immediateNotify);
#else
            Notifier notifier = Notifier.Instance;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) Dispatcher.Instance.NotifyConnections(callback);
            return uid;
#endif
        }

        public static void RemoveConnectionListener(int uid)
        {
#if CORE
            CoreMethods.RemoveConnectionListener(uid);
#else
            Notifier.Instance.RemoveConnectionListener(uid);
#endif
        }

        public static bool NotifierDestroyed()
        {
#if CORE
            return false; //Not implemented in C interface, TODO
#else
            return Notifier.Instance.Destroyed();
#endif
        }

        public static void SetNetworkIdentity(string name)
        {
#if CORE
            CoreMethods.SetNetworkIdentity(name);
#else
            Dispatcher.Instance.Identity = name;
#endif
        }

        public static void StartServer(string persistFilename, string listenAddress, int port)
        {
#if CORE
            CoreMethods.StartServer(persistFilename, listenAddress, (uint)port);
#else
            Dispatcher.Instance.StartServer(persistFilename, listenAddress, port);
#endif
        }

        public static void StopServer()
        {
#if CORE
            CoreMethods.StopServer();
#else
            Dispatcher.Instance.Stop();
#endif
        }

        public static void StartClient(string serverName, int port)
        {
#if CORE
            CoreMethods.StartClient(serverName, (uint)port);
#else
            Dispatcher.Instance.StartClient(serverName, port);
#endif
        }

        public static void StartClient(IList<ImmutablePair<string, int>> servers)
        {
#if CORE
            CoreMethods.StartClient(servers);
#else
            Dispatcher.Instance.StartClient(servers);
#endif
        }

        public static void StopClient()
        {
#if CORE
            CoreMethods.StopClient();
#else
            Dispatcher.Instance.Stop();
#endif
        }

        public static void StopRpcServer()
        {
#if CORE
            CoreMethods.StopRpcServer();
#else
            RpcServer.Instance.Stop();
#endif
        }

        public static void StopNotifier()
        {
#if CORE
            CoreMethods.StopNotifier();
#else
            Notifier.Instance.Stop();
#endif
        }

        public static void SetUpdateRate(double interval)
        {
#if CORE
            CoreMethods.SetUpdateRate(interval);
#else
            Dispatcher.Instance.UpdateRate = interval;
#endif
        }

        public static List<ConnectionInfo> GetConnections()
        {
#if CORE
            return CoreMethods.GetConnections();
#else
            return Dispatcher.Instance.GetConnections();
#endif
        }

        public static string SavePersistent(string filename)
        {
#if CORE
            return CoreMethods.SavePersistent(filename);
#else
            return Storage.Instance.SavePersistent(filename, false);
#endif
        }

        public static string LoadPersistent(string filename, Action<int, string> warn)
        {
#if CORE
            return CoreMethods.LoadPersistent(filename, warn);
#else
            return Storage.Instance.LoadPersistent(filename, warn);
#endif
        }

        public static long Now()
        {
            return Support.Timestamp.Now();
        }

        public static void SetLogger(LogFunc func, LogLevel minLevel)
        {
#if CORE
            CoreMethods.SetLogger(func, minLevel);
#else
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.MinLevel = minLevel;
#endif
        }

        public static string[] LoadPersistent(string filename)
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
