using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
#if !CORE
using NetworkTables.Logging;
#endif
#if CORE
using NetworkTables.Core.Native;
#endif

namespace NetworkTables
{
    public static class NtCore
    {
        public static bool SetEntryBoolean(string name, bool value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryBoolean(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeBoolean(value));
#endif
        }

        public static bool SetEntryDouble(string name, double value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryDouble(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeDouble(value));
#endif
        }

        public static bool SetEntryString(string name, string value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryString(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeString(value));
#endif
        }

        public static bool SetEntryBooleanArray(string name, bool[] value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryBooleanArray(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeBooleanArray(value));
#endif
        }

        public static bool SetEntryDoubleArray(string name, double[] value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryDoubleArray(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeDoubleArray(value));
#endif
        }

        public static bool SetEntryStringArray(string name, string[] value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryStringArray(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeStringArray(value));
#endif
        }

        public static bool SetEntryRaw(string name, byte[] value, bool force = false)
        {
#if CORE
            return CoreMethods.SetEntryRaw(name, value, force);
#else
            return Storage.Instance.SetEntryValue(name, Value.MakeRaw(value));
#endif
        }



        #region ThrowingGetters

#if !CORE
        internal static void ThrowException(string name, Value v, NtType requestedType)
        {
            if (v == null || v.Type == NtType.Unassigned)
            {
                throw new TableKeyNotDefinedException(name);
            }
            else
            {
                throw new TableKeyDifferentTypeException(name, requestedType, v.Type);
            }
        }
#endif

        public static bool GetEntryBoolean(string name)
        {
#if CORE
            return CoreMethods.GetEntryBoolean(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) ThrowException(name, v, NtType.Boolean);
            return v.GetBoolean();
#endif
        }

        public static double GetEntryDouble(string name)
        {
#if CORE
            return CoreMethods.GetEntryDouble(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDouble()) ThrowException(name, v, NtType.Double);
            return v.GetDouble();
#endif
        }

        public static string GetEntryString(string name)
        {
#if CORE
            return CoreMethods.GetEntryString(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsString()) ThrowException(name, v, NtType.String);
            return v.GetString();
#endif
        }

        public static bool[] GetEntryBooleanArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryBooleanArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray()) ThrowException(name, v, NtType.BooleanArray);
            return v.GetBooleanArray();
#endif
        }

        public static double[] GetEntryDoubleArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryDoubleArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray()) ThrowException(name, v, NtType.DoubleArray);
            return v.GetDoubleArray();
#endif
        }

        public static string[] GetEntryStringArray(string name)
        {
#if CORE
            return CoreMethods.GetEntryStringArray(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsStringArray()) ThrowException(name, v, NtType.StringArray);
            return v.GetStringArray();
#endif
        }

        public static byte[] GetEntryRaw(string name)
        {
#if CORE
            return CoreMethods.GetEntryRaw(name);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsRaw()) ThrowException(name, v, NtType.Raw);
            return v.GetRaw();
#endif
        }

        #endregion

        #region DefaultGetters
        public static bool GetEntryBoolean(string name, bool defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryBoolean(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) return defaultValue;
            return v.GetBoolean();
#endif
        }

        public static double GetEntryDouble(string name, double defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryDouble(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDouble()) return defaultValue;
            return v.GetDouble();
#endif
        }

        public static string GetEntryString(string name, string defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryString(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsString()) return defaultValue;
            return v.GetString();
#endif
        }

        public static bool[] GetEntryBooleanArray(string name, bool[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryBooleanArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray()) return defaultValue;
            return v.GetBooleanArray();
#endif
        }

        public static double[] GetEntryDoubleArray(string name, double[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryDoubleArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray()) return defaultValue;
            return v.GetDoubleArray();
#endif
        }

        public static string[] GetEntryStringArray(string name, string[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryStringArray(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsStringArray()) return defaultValue;
            return v.GetStringArray();
#endif
        }

        public static byte[] GetEntryRaw(string name, byte[] defaultValue)
        {
#if CORE
            return CoreMethods.GetEntryRaw(name, defaultValue);
#else
            var v = Storage.Instance.GetEntryValue(name);
            if (v == null || !v.IsRaw()) return defaultValue;
            return v.GetRaw();
#endif
        }
#endregion

        public static Value GetEntryValue(string name)
        {
#if CORE
            return CoreMethods.GetEntryValue(name);
#else
            return Storage.Instance.GetEntryValue(name);
#endif
        }

        public static bool SetEntryValue(string name, Value value)
        {
#if CORE
            return CoreMethods.SetEntryValue(name, value);
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
            CoreMethods.SetEntryValue(name, value, true);
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
            CoreMethods.StartServer(persistFilename, listenAddress, port);
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

        public static void StartClient(IList<NtIPAddress> servers)
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
            RemoteProcedureCall.s_rpcCallbacks.Clear();
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
