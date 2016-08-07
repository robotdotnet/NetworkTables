using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
using NetworkTables.Tables;

namespace NetworkTables.Independent
{
    public class IndependentNetworkTable : ITable, IRemote
    {
        private readonly IndependentNtCore m_ntCore;
        public const char PathSeperatorChar = NetworkTable.PathSeperatorChar;
        private readonly string m_path;

        public IndependentNetworkTable(IndependentNtCore ntCore, string path)
        {
            m_ntCore = ntCore;
            m_path = path;
        }

        public override string ToString()
        {
            return $"NetworkTable: {m_path}";
        }

        /// <summary>
        /// Checkts the table and tells if it contains the specified key.
        /// </summary>
        /// <param name="key">The key to be checked.</param>
        /// <returns>True if the table contains the key, otherwise false.</returns>
        public bool ContainsKey(string key)
        {
            return m_ntCore.ContainsKey(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Checks the table and tells if if contains the specified sub-table.
        /// </summary>
        /// <param name="key">The sub-table to check for</param>
        /// <returns>True if the table contains the sub-table, otherwise false</returns>
        public bool ContainsSubTable(string key)
        {
            return m_ntCore.GetEntryInfo(m_path + PathSeperatorChar + key + PathSeperatorChar, 0).Count != 0;
        }

        /// <summary>
        /// Gets a set of all the keys contained in the table with the specified type.
        /// </summary>
        /// <param name="types">Bitmask of types to check for; 0 is treated as a "don't care".</param>
        /// <returns>A set of all keys currently in the table.</returns>
        public HashSet<string> GetKeys(NtType types)
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = m_path.Length + 1;
            foreach (EntryInfo entry in m_ntCore.GetEntryInfo(m_path + PathSeperatorChar, types))
            {
                string relativeKey = entry.Name.Substring(prefixLen);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                    continue;
                keys.Add(relativeKey);
            }
            return keys;
        }

        /// <summary>
        /// Gets a set of all the keys contained in the table.
        /// </summary>
        /// <returns>A set of all keys currently in the table.</returns>
        public HashSet<string> GetKeys()
        {
            return GetKeys(0);
        }

        /// <summary>
        /// Gets a set of all the sub-tables contained in the table.
        /// </summary>
        /// <returns>A set of all subtables currently contained in the table.</returns>
        public HashSet<string> GetSubTables()
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = m_path.Length + 1;
            foreach (EntryInfo entry in m_ntCore.GetEntryInfo(m_path + PathSeperatorChar, 0))
            {
                string relativeKey = entry.Name.Substring(prefixLen);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    continue;
                keys.Add(relativeKey.Substring(0, endSubTable));
            }
            return keys;
        }

        /// <summary>
        /// Returns the <see cref="ITable"/> at the specified key. If there is no 
        /// table at the specified key, it will create a new table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The <see cref="ITable"/> to be returned.</returns>
        public ITable GetSubTable(string key)
        {
            return new IndependentNetworkTable(m_ntCore, m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Makes a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        public void SetPersistent(string key)
        {
            SetFlags(key, EntryFlags.Persistent);
        }

        /// <summary>
        /// Stop making a key's value persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        public void ClearPersistent(string key)
        {
            ClearFlags(key, EntryFlags.Persistent);
        }

        /// <summary>
        /// Returns whether a value is persistent through program restarts.
        /// </summary>
        /// <param name="key">The key name (cannot be null).</param>
        /// <returns>True if the value is persistent.</returns>
        public bool IsPersistent(string key)
        {
            return GetFlags(key).HasFlag(EntryFlags.Persistent);
        }

        /// <summary>
        /// Sets flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to set. (Bitmask)</param>
        public void SetFlags(string key, EntryFlags flags)
        {
            m_ntCore.SetEntryFlags(m_path + PathSeperatorChar + key, GetFlags(key) | flags);
        }

        /// <summary>
        /// Clears flags on the specified key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="flags">The flags to clear. (Bitmask)</param>
        public void ClearFlags(string key, EntryFlags flags)
        {
            m_ntCore.SetEntryFlags(m_path + PathSeperatorChar + key, GetFlags(key) & ~flags);
        }

        /// <summary>
        /// Returns the flags for the specified key.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <returns>The flags attached to the key.</returns>
        public EntryFlags GetFlags(string key)
        {
            return m_ntCore.GetEntryFlags(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Deletes the specifed key in this table.
        /// </summary>
        /// <param name="key">The key name.</param>
        public void Delete(string key)
        {
            m_ntCore.DeleteEntry(m_path + PathSeperatorChar + key);
        }

        /// <summary>
        /// Flushes all updated values immediately to the network.
        /// </summary>
        /// <remarks>
        /// Note that this is rate-limited to protect the network from flooding.
        /// This is primarily useful for synchronizing network updates with user code.
        /// </remarks>
        public void Flush()
        {
            m_ntCore.Flush();
        }

        /// <summary>
        /// Saves persistent keys to a file. The server does this automatically.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <exception cref="PersistentException">Thrown if there is an error
        /// saving the file.</exception>
        public void SavePersistent(string filename)
        {
            m_ntCore.SavePersistent(filename);
        }

        /// <summary>
        /// Loads persistent keys from a file. The server does this automatically.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <returns>A List of warnings (errors result in an exception instead.)</returns>
        /// <exception cref="PersistentException">Thrown if there is an error
        /// loading the file.</exception>
        public string[] LoadPersistent(string filename)
        {
            return m_ntCore.LoadPersistent(filename);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public Value GetValue(string key)
        {
            string localPath = m_path + PathSeperatorChar + key;
            var v = NtCore.GetEntryValue(localPath);
            if (v == null) throw new TableKeyNotDefinedException(localPath);
            return v;
        }

        ///<inheritdoc/>
        public Value GetValue(string key, Value defaultValue)
        {
            string localPath = m_path + PathSeperatorChar + key;
            var v = m_ntCore.GetEntryValue(localPath);
            if (v == null) return defaultValue;
            return v;
        }

        ///<inheritdoc/>
        public bool PutValue(string key, Value value)
        {
            key = m_path + PathSeperatorChar + key;
            return m_ntCore.SetEntryValue(key, value);
        }

        ///<inheritdoc/>
        public bool PutNumber(string key, double value)
        {

            return m_ntCore.SetEntryDouble(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        public double GetNumber(string key, double defaultValue)
        {

            return m_ntCore.GetEntryDouble(m_path + PathSeperatorChar + key, defaultValue);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public double GetNumber(string key)
        {

            return m_ntCore.GetEntryDouble(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public bool PutString(string key, string value)
        {

            return m_ntCore.SetEntryString(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        public string GetString(string key, string defaultValue)
        {

            return m_ntCore.GetEntryString(m_path + PathSeperatorChar + key, defaultValue);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public string GetString(string key)
        {

            return m_ntCore.GetEntryString(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public bool PutBoolean(string key, bool value)
        {

            return m_ntCore.SetEntryBoolean(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        public bool GetBoolean(string key, bool defaultValue)
        {

            return m_ntCore.GetEntryBoolean(m_path + PathSeperatorChar + key, defaultValue);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public bool GetBoolean(string key)
        {

            return m_ntCore.GetEntryBoolean(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public bool PutStringArray(string key, string[] value)
        {

            return m_ntCore.SetEntryStringArray(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public string[] GetStringArray(string key)
        {

            return m_ntCore.GetEntryStringArray(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public string[] GetStringArray(string key, string[] defaultValue)
        {

            return m_ntCore.GetEntryStringArray(m_path + PathSeperatorChar + key, defaultValue);

        }

        ///<inheritdoc/>
        public bool PutNumberArray(string key, double[] value)
        {

            return m_ntCore.SetEntryDoubleArray(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public double[] GetNumberArray(string key)
        {

            return m_ntCore.GetEntryDoubleArray(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public double[] GetNumberArray(string key, double[] defaultValue)
        {

            return m_ntCore.GetEntryDoubleArray(m_path + PathSeperatorChar + key, defaultValue);
        }

        ///<inheritdoc/>
        public bool PutBooleanArray(string key, bool[] value)
        {

            return m_ntCore.SetEntryBooleanArray(m_path + PathSeperatorChar + key, value);
        }

        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public bool[] GetBooleanArray(string key)
        {

            return m_ntCore.GetEntryBooleanArray(m_path + PathSeperatorChar + key);
        }

        ///<inheritdoc/>
        public bool PutRaw(string key, byte[] value)
        {

            return m_ntCore.SetEntryRaw(m_path + PathSeperatorChar + key, value);
        }
        ///<inheritdoc/>
        [Obsolete("Please use the Default Value Get... Methods instead.")]
        public byte[] GetRaw(string key)
        {

            return m_ntCore.GetEntryRaw(m_path + PathSeperatorChar + key);
        }
        ///<inheritdoc/>
        public byte[] GetRaw(string key, byte[] defaultValue)
        {

            return m_ntCore.GetEntryRaw(m_path + PathSeperatorChar + key, defaultValue);
        }

        ///<inheritdoc/>
        public bool[] GetBooleanArray(string key, bool[] defaultValue)
        {

            return m_ntCore.GetEntryBooleanArray(m_path + PathSeperatorChar + key, defaultValue);
        }

        private readonly Dictionary<ITableListener, List<int>> m_listenerMap = new Dictionary<ITableListener, List<int>>();

        private readonly Dictionary<Action<ITable, string, Value, NotifyFlags>, List<int>> m_actionListenerMap = new Dictionary<Action<ITable, string, Value, NotifyFlags>, List<int>>();

        ///<inheritdoc/>
        public void AddTableListenerEx(ITableListener listener, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }

            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                {
                    return;
                }
                listener.ValueChanged(this, relativeKey, value, flags_);
            };

            int id = m_ntCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListenerEx(string key, ITableListener listener, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }
            string fullKey = m_path + PathSeperatorChar + key;
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, funcKey, value, flags_) =>
            {
                if (!funcKey.Equals(fullKey))
                    return;
                listener.ValueChanged(this, key, value, flags_);
            };

            int id = m_ntCore.AddEntryListener(fullKey, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(ITableListener listener, bool localNotify)
        {
            List<int> adapters;
            if (!m_listenerMap.TryGetValue(listener, out adapters))
            {
                adapters = new List<int>();
                m_listenerMap.Add(listener, adapters);
            }
            HashSet<string> notifiedTables = new HashSet<string>();
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    return;
                string subTableKey = relativeKey.Substring(0, endSubTable);
                if (notifiedTables.Contains(subTableKey))
                    return;
                notifiedTables.Add(subTableKey);
                listener.ValueChanged(this, subTableKey, null, flags_);
            };
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (localNotify)
                flags |= NotifyFlags.NotifyLocal;
            int id = m_ntCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListener(ITableListener listener, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(listener, flags);
        }

        ///<inheritdoc/>
        public void AddTableListener(string key, ITableListener listener, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(key, listener, flags);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(ITableListener listener)
        {
            AddSubTableListener(listener, false);
        }

        ///<inheritdoc/>
        public void RemoveTableListener(ITableListener listener)
        {
            List<int> adapters;
            if (m_listenerMap.TryGetValue(listener, out adapters))
            {
                foreach (int t in adapters)
                {
                    m_ntCore.RemoveEntryListener(t);
                }
                adapters.Clear();
            }
        }


        ///<inheritdoc/>
        public void AddTableListenerEx(Action<ITable, string, Value, NotifyFlags> listenerDelegate, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }

            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                if (relativeKey.IndexOf(PathSeperatorChar) != -1)
                {
                    return;
                }
                listenerDelegate(this, relativeKey, value, flags_);
            };

            int id = m_ntCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListenerEx(string key, Action<ITable, string, Value, NotifyFlags> listenerDelegate, NotifyFlags flags)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }
            string fullKey = m_path + PathSeperatorChar + key;
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, funcKey, value, flags_) =>
            {
                if (!funcKey.Equals(fullKey))
                    return;
                listenerDelegate(this, key, value, flags_);
            };

            int id = m_ntCore.AddEntryListener(fullKey, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool localNotify)
        {
            List<int> adapters;
            if (!m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                adapters = new List<int>();
                m_actionListenerMap.Add(listenerDelegate, adapters);
            }
            HashSet<string> notifiedTables = new HashSet<string>();
            // ReSharper disable once InconsistentNaming
            EntryListenerCallback func = (uid, key, value, flags_) =>
            {
                string relativeKey = key.Substring(m_path.Length + 1);
                int endSubTable = relativeKey.IndexOf(PathSeperatorChar);
                if (endSubTable == -1)
                    return;
                string subTableKey = relativeKey.Substring(0, endSubTable);
                if (notifiedTables.Contains(subTableKey))
                    return;
                notifiedTables.Add(subTableKey);
                listenerDelegate(this, subTableKey, null, flags_);
            };
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (localNotify)
                flags |= NotifyFlags.NotifyLocal;
            int id = m_ntCore.AddEntryListener(m_path + PathSeperatorChar, func, flags);

            adapters.Add(id);
        }

        ///<inheritdoc/>
        public void AddTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(listenerDelegate, flags);
        }

        ///<inheritdoc/>
        public void AddTableListener(string key, Action<ITable, string, Value, NotifyFlags> listenerDelegate, bool immediateNotify = false)
        {
            NotifyFlags flags = NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            if (immediateNotify)
                flags |= NotifyFlags.NotifyImmediate;
            AddTableListenerEx(key, listenerDelegate, flags);
        }

        ///<inheritdoc/>
        public void AddSubTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate)
        {
            AddSubTableListener(listenerDelegate, false);
        }

        ///<inheritdoc/>
        public void RemoveTableListener(Action<ITable, string, Value, NotifyFlags> listenerDelegate)
        {
            List<int> adapters;
            if (m_actionListenerMap.TryGetValue(listenerDelegate, out adapters))
            {
                foreach (int t in adapters)
                {
                    m_ntCore.RemoveEntryListener(t);
                }
                adapters.Clear();
            }
        }

        private readonly Dictionary<IRemoteConnectionListener, int> m_connectionListenerMap =
            new Dictionary<IRemoteConnectionListener, int>();

        private readonly Dictionary<Action<IRemote, ConnectionInfo, bool>, int> m_actionConnectionListenerMap
            = new Dictionary<Action<IRemote, ConnectionInfo, bool>, int>();

        ///<inheritdoc/>
        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {

            if (m_connectionListenerMap.ContainsKey(listener))
            {
                throw new ArgumentException("Cannot add the same listener twice", nameof(listener));
            }

            ConnectionListenerCallback func = (uid, connected, conn) =>
            {
                if (connected) listener.Connected(this, conn);
                else listener.Disconnected(this, conn);
            };
            int id = m_ntCore.AddConnectionListener(func, immediateNotify);
            m_connectionListenerMap.Add(listener, id);

        }

        ///<inheritdoc/>
        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            int val;
            if (m_connectionListenerMap.TryGetValue(listener, out val))
            {
                m_ntCore.RemoveConnectionListener(val);
            }
        }

        /// <inheritdoc/>
        public void AddConnectionListener(Action<IRemote, ConnectionInfo, bool> listener, bool immediateNotify)
        {
            if (m_actionConnectionListenerMap.ContainsKey(listener))
            {
                throw new ArgumentException("Cannot add the same listener twice", nameof(listener));
            }

            ConnectionListenerCallback func = (uid, connected, conn) =>
            {
                listener(this, conn, connected);
            };
            int id = m_ntCore.AddConnectionListener(func, immediateNotify);
            m_actionConnectionListenerMap.Add(listener, id);
        }

        /// <inheritdoc/>
        public void RemoveConnectionListener(Action<IRemote, ConnectionInfo, bool> listener)
        {
            int val;
            if (m_actionConnectionListenerMap.TryGetValue(listener, out val))
            {
                m_ntCore.RemoveConnectionListener(val);
            }
        }

        /// <summary>
        /// Gets if the NetworkTables is connected to a client or server.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                var conns = m_ntCore.GetConnections();
                return conns.Count > 0;
            }
        }

        public bool IsServer => !m_ntCore.Client;
    }
}
