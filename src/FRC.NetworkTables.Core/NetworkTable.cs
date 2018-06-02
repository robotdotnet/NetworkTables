using FRC.NetworkTables.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public class NetworkTable
    {
        public static readonly char PathSeparator = '/';
        private readonly String m_pathWithSep;

        public static string BasenameKey(string key)
        {
            int slash = key.LastIndexOf(PathSeparator);
            if (slash == -1)
            {
                return key;
            }
            return key.Substring(slash + 1);
        }

        internal NetworkTable(NetworkTableInstance inst, string path)
        {
            Path = path;
            m_pathWithSep = path + PathSeparator;
            Instance = inst;
            m_entryFactory = (s) =>
            {
                return inst.GetEntry(m_pathWithSep + s);
            };
        }

        public NetworkTableInstance Instance { get; }

        public override string ToString()
        {
            return $"NetworkTable: {Path}";
        }

        private readonly ConcurrentDictionary<string, NetworkTableEntry> m_entries = new ConcurrentDictionary<string, NetworkTableEntry>();

        private readonly Func<string, NetworkTableEntry> m_entryFactory;

        public NetworkTableEntry GetEntry(string key)
        {
            return m_entries.GetOrAdd(key, m_entryFactory);
        }

        public NtEntryListener AddEntryListener(TableEntryListener listener, NotifyFlags flags)
        {
            int prefixLen = Path.Length + 1;
            return Instance.AddEntryListener(m_pathWithSep, (in EntryNotification evnt) =>
            {
                ReadOnlySpan<char> relativeKey = evnt.Name.AsSpan().Slice(prefixLen);
                if (relativeKey.IndexOf(PathSeparator) != -1)
                {
                    return;
                }
                listener(this, relativeKey.ToString(), evnt.Entry, evnt.Value, evnt.Flags);
            }, flags);
        }

        public NtEntryListener AddEntryListener(string key, TableEntryListener listener, NotifyFlags flags)
        {
            var entry = GetEntry(key);
            return Instance.AddEntryListener(entry, (in EntryNotification evnt) =>
            {
                listener(this, key, evnt.Entry, evnt.Value, evnt.Flags);
            }, flags);
        }

        public void RemoveEntryListener(NtEntryListener listener)
        {
            Instance.RemoveEntryListener(listener);
        }

        public NtEntryListener AddSubTableListener(TableListener listener, bool localNotify)
        {
            NotifyFlags flags = NotifyFlags.New | NotifyFlags.Immediate;
            if (localNotify)
            {
                flags |= NotifyFlags.Local;
            }

            int prefixLen = Path.Length + 1;
            HashSet<string> notifiedTable = new HashSet<string>();

            return Instance.AddEntryListener(m_pathWithSep, (in EntryNotification evnt) =>
            {
                ReadOnlySpan<char> relativeKey = evnt.Name.AsSpan().Slice(prefixLen);
                int endSubTable = relativeKey.IndexOf(PathSeparator);
                if (endSubTable == -1)
                {
                    return;
                }
                relativeKey.Slice(0, endSubTable).ToString();
                string subTableKey = relativeKey.Slice(0, endSubTable).ToString();
                if (notifiedTable.Contains(subTableKey))
                {
                    return;
                }
                notifiedTable.Add(subTableKey);
                listener(this, subTableKey, this.GetSubTable(subTableKey));
            }, flags);
        }

        public void RemoveTableListener(NtEntryListener listener)
        {
            Instance.RemoveEntryListener(listener);
        }

        public NetworkTable GetSubTable(string key)
        {
            return new NetworkTable(Instance, m_pathWithSep + key);
        }

        public bool ContainsKey(string key)
        {
            return !(string.IsNullOrWhiteSpace(key)) && GetEntry(key).Exists();
        }

        public bool ContainsSubTable(string key)
        {
            return NtCore.GetEntryCount(Instance.Handle, m_pathWithSep + key + PathSeparator, 0) != 0;
        }
        

        public HashSet<string> GetKeys(NtType types)
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = -Path.Length + 1;
            foreach(var info in Instance.GetEntryInfo(m_pathWithSep, types))
            {
                var relativeKey = info.Name.AsSpan().Slice(prefixLen);
                if (relativeKey.IndexOf(PathSeparator) != -1)
                {
                    continue;
                }
                string rKey = relativeKey.ToString();
                keys.Add(rKey);
                m_entries.GetOrAdd(rKey, new NetworkTableEntry(Instance, info.EntryHandle));
            }
            return keys;
        }

        public HashSet<string> GetKeys()
        {
            return GetKeys(0);
        }

        public HashSet<string> GetSubTables()
        {
            HashSet<string> keys = new HashSet<string>();
            int prefixLen = Path.Length + 1;
            foreach (EntryInfo info in Instance.GetEntryInfo(m_pathWithSep, 0))
            {
                var relativeKey = info.Name.AsSpan().Slice(prefixLen);
                int endSubTable = relativeKey.IndexOf(PathSeparator);
                if (endSubTable == 1)
                {
                    continue;
                }
                keys.Add(relativeKey.Slice(0, endSubTable).ToString());
            }
            return keys;
        }

        public void Delete(string key)
        {
            GetEntry(key).Delete();
        }

        internal bool PutValue(string key, in NetworkTableValue value)
        {
            return GetEntry(key).SetValue(value);
        }

        internal bool SetDefaultValue(string key, in NetworkTableValue value)
        {
            return GetEntry(key).SetDefaultValue(value);
        }

        internal NetworkTableValue GetValue(string key)
        {
            return GetEntry(key).GetValue();
        }

        public string Path { get; }

        public void SaveEntries(string filename)
        {
            Instance.SaveEntries(filename, m_pathWithSep);
        }

        public List<string> LoadEntries(string filename)
        {
            return Instance.LoadEntries(filename, m_pathWithSep);
        }

        // TODO: Equals
    }
}
