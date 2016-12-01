using System;
using System.Collections.Generic;
using System.Linq;
using NetworkTables.Exceptions;
using NetworkTables.Logging;

namespace NetworkTables.Independent
{
    /// <summary>
    /// This class contains all NtCore methods exposed by the underlying library, running in an independent state
    /// </summary>
    /// <remarks>The static <see cref="NtCore"/>, <see cref="NetworkTable"/> and <see cref="RemoteProcedureCall"/>
    /// all run using the same backend library. This means you cannot have both a client and a server running
    /// in the same user program. The 
    /// <see cref="IndependentNtCore"/>, <see cref="IndependentNetworkTable"/> and <see cref="IndependentRemoteProcedureCall"/>
    /// get around this restriction, and allow multiple clients and servers in the same user program. Note that this is
    /// not supported by NetworkTables.Core.</remarks>
    public class IndependentNtCore : IDisposable
    {
        /// <inheritdoc cref="NetworkTable.DefaultPort"/>
        public const int DefaultPort = NetworkTable.DefaultPort;

        internal readonly Storage m_storage;
        internal readonly Notifier m_notifier;
        internal readonly Dispatcher m_dispatcher;
        internal readonly RpcServer m_rpcServer;

        private readonly object m_lockObject = new object();

        /// <summary>
        /// Creates a new NtCore object to run independently of all other NtCore objects
        /// </summary>
        public IndependentNtCore()
        {
            m_notifier = new Notifier();
            m_rpcServer = new RpcServer();
            m_storage = new Storage(m_notifier, m_rpcServer);
            m_dispatcher = new Dispatcher(m_storage, m_notifier);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc cref="NtCore.SetDefaultEntryBoolean"/>
        public bool SetDefaultEntryBoolean(string name, bool value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeBoolean(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryDouble"/>
        public bool SetDefaultEntryDouble(string name, double value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeDouble(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryString"/>
        public bool SetDefaultEntryString(string name, string value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeString(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryBooleanArray"/>
        public bool SetDefaultEntryBooleanArray(string name, IList<bool> value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeBooleanArray(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryDoubleArray"/>
        public bool SetDefaultEntryDoubleArray(string name, IList<double> value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeDoubleArray(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryStringArray"/>
        public bool SetDefaultEntryStringArray(string name, IList<string> value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeStringArray(value));
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryRaw"/>
        public bool SetDefaultEntryRaw(string name, IList<byte> value)
        {
            return m_storage.SetDefaultEntryValue(name, Value.MakeRaw(value));
        }

        /// <inheritdoc cref="NtCore.SetEntryBoolean"/>
        public bool SetEntryBoolean(string name, bool value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeBoolean(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeBoolean(value));
        }

        /// <inheritdoc cref="NtCore.SetEntryDouble"/>
        public bool SetEntryDouble(string name, double value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeDouble(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeDouble(value));

        }

        /// <inheritdoc cref="NtCore.SetEntryString"/>
        public bool SetEntryString(string name, string value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeString(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeString(value));

        }

        /// <inheritdoc cref="NtCore.SetEntryBooleanArray"/>
        public bool SetEntryBooleanArray(string name, IList<bool> value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeBooleanArray(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeBooleanArray(value));

        }

        /// <inheritdoc cref="NtCore.SetEntryDoubleArray"/>
        public bool SetEntryDoubleArray(string name, IList<double> value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeDoubleArray(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeDoubleArray(value));

        }

        /// <inheritdoc cref="NtCore.SetEntryStringArray"/>
        public bool SetEntryStringArray(string name, IList<string> value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeStringArray(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeStringArray(value));

        }

        /// <inheritdoc cref="NtCore.SetEntryRaw"/>
        public bool SetEntryRaw(string name, IList<byte> value, bool force = false)
        {
            if (force)
            {
                m_storage.SetEntryTypeValue(name, Value.MakeRaw(value));
                return true;
            }
            return m_storage.SetEntryValue(name, Value.MakeRaw(value));

        }



        #region ThrowingGetters

        /// <inheritdoc cref="NtCore.GetEntryBoolean(string)"/>
        public bool GetEntryBoolean(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) throw NtCore.GetValueException(name, v, NtType.Boolean);
            return v.GetBoolean();

        }

        /// <inheritdoc cref="NtCore.GetEntryDouble(string)"/>
        public double GetEntryDouble(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsDouble()) throw NtCore.GetValueException(name, v, NtType.Double);
            return v.GetDouble();

        }

        /// <inheritdoc cref="NtCore.GetEntryString(string)"/>
        public string GetEntryString(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsString()) throw NtCore.GetValueException(name, v, NtType.String);
            return v.GetString();

        }

        /// <inheritdoc cref="NtCore.GetEntryBooleanArray(string)"/>
        public bool[] GetEntryBooleanArray(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray()) throw NtCore.GetValueException(name, v, NtType.BooleanArray);
            return v.GetBooleanArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryDoubleArray(string)"/>
        public double[] GetEntryDoubleArray(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray()) throw NtCore.GetValueException(name, v, NtType.DoubleArray);
            return v.GetDoubleArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryStringArray(string)"/>
        public string[] GetEntryStringArray(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsStringArray()) throw NtCore.GetValueException(name, v, NtType.StringArray);
            return v.GetStringArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryRaw(string)"/>
        public byte[] GetEntryRaw(string name)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsRaw()) throw NtCore.GetValueException(name, v, NtType.Raw);
            return v.GetRaw();

        }

        #endregion

        #region DefaultGetters

        /// <inheritdoc cref="NtCore.GetEntryBoolean(string, bool)"/>
        public bool GetEntryBoolean(string name, bool defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsBoolean()) return defaultValue;
            return v.GetBoolean();

        }

        /// <inheritdoc cref="NtCore.GetEntryDouble(string, double)"/>
        public double GetEntryDouble(string name, double defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsDouble()) return defaultValue;
            return v.GetDouble();

        }

        /// <inheritdoc cref="NtCore.GetEntryString(string, string)"/>
        public string GetEntryString(string name, string defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsString()) return defaultValue;
            return v.GetString();

        }

        /// <inheritdoc cref="NtCore.GetEntryBooleanArray(string, bool[])"/>
        public bool[] GetEntryBooleanArray(string name, bool[] defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsBooleanArray())
            {
                return defaultValue;
            }
            return v.GetBooleanArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryDoubleArray(string, double[])"/>
        public double[] GetEntryDoubleArray(string name, double[] defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsDoubleArray())
            {

                return defaultValue;
            }
            return v.GetDoubleArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryStringArray(string, string[])"/>
        public string[] GetEntryStringArray(string name, string[] defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsStringArray())
            {
                return defaultValue;
            }
            return v.GetStringArray();

        }

        /// <inheritdoc cref="NtCore.GetEntryRaw(string, byte[])"/>
        public byte[] GetEntryRaw(string name, byte[] defaultValue)
        {
            var v = m_storage.GetEntryValue(name);
            if (v == null || !v.IsRaw())
            {
                return defaultValue;
            }
            return v.GetRaw();

        }
        #endregion

        /// <inheritdoc cref="NtCore.GetEntryValue"/>
        public Value GetEntryValue(string name)
        {
            return m_storage.GetEntryValue(name);
        }

        /// <inheritdoc cref="NtCore.SetEntryValue"/>
        public bool SetEntryValue(string name, Value value)
        {
            return m_storage.SetEntryValue(name, value);
        }

        /// <inheritdoc cref="NtCore.SetDefaultEntryValue"/>
        public bool SetDefaultEntryValue(string name, Value value)
        {
            return m_storage.SetDefaultEntryValue(name, value);
        }

        /// <inheritdoc cref="NtCore.SetEntryTypeValue"/>
        public void SetEntryTypeValue(string name, Value value)
        {
            m_storage.SetEntryTypeValue(name, value);
        }

        /// <inheritdoc cref="NtCore.SetEntryFlags"/>
        public void SetEntryFlags(string name, EntryFlags flags)
        {
            m_storage.SetEntryFlags(name, flags);
        }

        /// <inheritdoc cref="NtCore.GetEntryFlags"/>
        public EntryFlags GetEntryFlags(string name)
        {
            return m_storage.GetEntryFlags(name);
        }

        /// <inheritdoc cref="NtCore.DeleteEntry"/>
        public void DeleteEntry(string name)
        {
            m_storage.DeleteEntry(name);
        }

        /// <inheritdoc cref="NtCore.DeleteAllEntries"/>
        public void DeleteAllEntries()
        {
            m_storage.DeleteAllEntries();
        }

        /// <inheritdoc cref="NtCore.GetEntryInfo"/>
        public List<EntryInfo> GetEntryInfo(string prefix, NtType types)
        {
            return m_storage.GetEntryInfo(prefix, types);
        }

        /// <inheritdoc cref="NtCore.GetType(string)"/>
        public NtType GetType(string name)
        {
            var v = GetEntryValue(name);
            if (v == null) return NtType.Unassigned;
            return v.Type;
        }

        /// <inheritdoc cref="NtCore.ContainsEntry"/>
        public bool ContainsEntry(string name)
        {
            return GetType(name) != NtType.Unassigned;
        }

        /// <inheritdoc cref="NtCore.Flush"/>
        public void Flush()
        {
            m_dispatcher.Flush();
        }

        /// <inheritdoc cref="NtCore.AddEntryListener"/>
        public int AddEntryListener(string prefix, EntryListenerCallback callback, NotifyFlags flags)
        {
            Notifier notifier = m_notifier;
            int uid = notifier.AddEntryListener(prefix, callback, flags);
            notifier.Start();
            if ((flags & NotifyFlags.NotifyImmediate) != 0)
                m_storage.NotifyEntries(prefix, callback);
            return uid;
        }

        /// <inheritdoc cref="NtCore.RemoveEntryListener"/>
        public void RemoveEntryListener(int uid)
        {
            m_notifier.RemoveEntryListener(uid);
        }

        /// <inheritdoc cref="NtCore.AddConnectionListener"/>
        public int AddConnectionListener(ConnectionListenerCallback callback, bool immediateNotify)
        {
            Notifier notifier = m_notifier;
            int uid = notifier.AddConnectionListener(callback);
            notifier.Start();
            if (immediateNotify) m_dispatcher.NotifyConnections(callback);
            return uid;
        }

        /// <inheritdoc cref="NtCore.RemoveConnectionListener"/>
        public void RemoveConnectionListener(int uid)
        {
            m_notifier.RemoveConnectionListener(uid);
        }

        /// <inheritdoc cref="NtCore.NotifierDestroyed"/>
        public bool NotifierDestroyed()
        {
            return m_notifier.Destroyed();
        }

        /// <inheritdoc cref="NtCore.StartClient(IList{NtIPAddress})"/>
        public void StartClient(IList<NtIPAddress> servers)
        {

            lock (m_lockObject)
            {
                CheckInit();
                Client = true;
                Running = true;
            }

            m_dispatcher.StartClient();
            m_dispatcher.SetServer(servers);
        }

        /// <inheritdoc cref="NtCore.StartServer"/>
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

        /// <inheritdoc cref="NtCore.StopServer"/>
        public void StopServer()
        {
            lock (m_lockObject)
            {
                Running = false;
            }
            m_dispatcher.Stop();
        }

        /// <summary>
        /// Gets or sets the remote name for this table
        /// </summary>
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

        /// <summary>
        /// Gets if this table is a client
        /// </summary>
        public bool Client { get; private set; }
        /// <summary>
        /// Gets if this table is running
        /// </summary>
        public bool Running { get; private set; }

        private void CheckInit()
        {
            lock (m_lockObject)
            {
                if (Running)
                    throw new InvalidOperationException("Operation cannot be completed while NtCore is running");
            }
        }

        /// <inheritdoc cref="NtCore.StartClient(string, int)"/>
        public void StartClient(string serverName, int port)
        {
            lock (m_lockObject)
            {
                CheckInit();
                Client = true;
                Running = true;
                m_dispatcher.StartClient();
                m_dispatcher.SetServer(serverName, port);
            }
        }

        /// <inheritdoc cref="NtCore.StopClient"/>
        public void StopClient()
        {
            lock (m_lockObject)
            {
                Running = false;
                m_dispatcher.Stop();
            }
        }

        /// <inheritdoc cref="NtCore.StopRpcServer"/>
        public void StopRpcServer()
        {
            m_rpcServer.Stop();
        }

        /// <inheritdoc cref="NtCore.StopNotifier"/>
        public void StopNotifier()
        {
            m_notifier.Stop();
        }

        /// <summary>
        /// Gets or sets the update rate for this table (seconds)
        /// </summary>
        public double UpdateRate
        {
            get { return m_dispatcher.UpdateRate; }
            set { m_dispatcher.UpdateRate = value; }
        }

        /// <inheritdoc cref="NtCore.GetConnections"/>
        public List<ConnectionInfo> GetConnections()
        {
            return m_dispatcher.GetConnections();
        }

        /// <inheritdoc cref="NtCore.SavePersistent"/>
        public string SavePersistent(string filename)
        {
            return m_storage.SavePersistent(filename, false);
        }

        /// <inheritdoc cref="NtCore.LoadPersistent(string, Action{int,string})"/>
        public string LoadPersistent(string filename, Action<int, string> warn)
        {
            return m_storage.LoadPersistent(filename, warn);
        }

        /// <inheritdoc cref="NtCore.Now"/>
        public long Now()
        {
            return Support.Timestamp.Now();
        }

        /// <inheritdoc cref="NtCore.SetLogger"/>
        public void SetLogger(LogFunc func, LogLevel minLevel)
        {
            Logger logger = Logger.Instance;
            logger.SetLogger(func);
            logger.MinLevel = minLevel;
        }

        /// <inheritdoc cref="NtCore.LoadPersistent(string)"/>
        public List<string> LoadPersistent(string filename)
        {
            List<string> warns = new List<string>();
            var err = LoadPersistent(filename, (i, s) =>
            {
                warns.Add($"{i}: {s}");
            });
            if (err != null) throw new PersistentException($"Load Persistent Failed: {err}");
            return warns;
        }
    }
}
