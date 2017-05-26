using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using static NetworkTables.Logging.Logger;
using NetworkTables.Logging;

namespace NetworkTables
{
    internal partial class Storage
    {
        private class StoragePairComparer : IComparer<(string key, Value value)>
        {
            public int Compare((string key, Value value) x, (string key, Value value) y)
            {
                return string.Compare(x.key, y.key, StringComparison.Ordinal);
            }
        }

        private readonly static StoragePairComparer s_storageComparer = new StoragePairComparer();

        private bool GetPersistentEntries(bool periodic, List<(string key, Value value)> entries)
        {
            using (m_monitor.Enter())
            {
                if (periodic && !m_persistentDirty) return false;
                m_persistentDirty = false;
                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    if (!entry.IsPersistent()) continue;
                    entries.Add((i.Key, entry.Value));
                }
            }
            entries.Sort(s_storageComparer);
            return true;
        }

        private static async Task SavePersistentImpl(StreamWriter stream, IEnumerable<(string key, Value value)> entries)
        {
            await stream.WriteAsync("[NetworkTables Storage 3.0]\n").ConfigureAwait(false);
            foreach (var i in entries)
            {
                var v = i.value;
                if (v == null) continue;
                switch (v.Type)
                {
                    case NtType.Boolean:
                        await stream.WriteAsync("boolean ").ConfigureAwait(false);
                        break;
                    case NtType.Double:
                        await stream.WriteAsync("double ").ConfigureAwait(false);
                        break;
                    case NtType.String:
                        await stream.WriteAsync("string ").ConfigureAwait(false);
                        break;
                    case NtType.Raw:
                        await stream.WriteAsync("raw ").ConfigureAwait(false);
                        break;
                    case NtType.BooleanArray:
                        await stream.WriteAsync("array boolean ").ConfigureAwait(false);
                        break;
                    case NtType.DoubleArray:
                        await stream.WriteAsync("array double ").ConfigureAwait(false);
                        break;
                    case NtType.StringArray:
                        await stream.WriteAsync("array string ").ConfigureAwait(false);
                        break;
                    default:
                        continue;
                }

                await WriteStringAsync(stream, i.key).ConfigureAwait(false);

                await stream.WriteAsync('=').ConfigureAwait(false);

                switch (v.Type)
                {
                    case NtType.Boolean:
                        await stream.WriteAsync(v.GetBoolean() ? "true" : "false").ConfigureAwait(false);
                        break;
                    case NtType.Double:
                        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                        await stream.WriteAsync(v.GetDouble().ToString()).ConfigureAwait(false);
                        break;
                    case NtType.String:
                        await WriteStringAsync(stream, v.GetString()).ConfigureAwait(false);
                        break;
                    case NtType.Raw:
                        await stream.WriteAsync(Convert.ToBase64String(v.GetRaw())).ConfigureAwait(false);
                        break;
                    case NtType.BooleanArray:
                        bool first = true;
                        foreach (var b in v.GetBooleanArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            await stream.WriteAsync(b ? "true" : "false").ConfigureAwait(false);
                        }
                        break;
                    case NtType.DoubleArray:
                        first = true;
                        foreach (var b in v.GetDoubleArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                            await stream.WriteAsync(b.ToString()).ConfigureAwait(false);
                        }
                        break;
                    case NtType.StringArray:
                        first = true;
                        foreach (var b in v.GetStringArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            await WriteStringAsync(stream, b).ConfigureAwait(false);
                        }
                        break;
                }
                //eol
                await stream.WriteAsync('\n').ConfigureAwait(false);
            }
        }

        private static char HexDigit(int x)
        {
            const byte hexChar = (byte)'A';
            return (char)(x < 10 ? (byte)'0' + x : hexChar + x - 10);
        }

        private static async Task WriteStringAsync(StreamWriter stream, string str)
        {
            await stream.WriteAsync('"').ConfigureAwait(false);
            foreach (var c in str)
            {
                switch (c)
                {
                    case '\\':
                        await stream.WriteAsync("\\\\").ConfigureAwait(false);
                        break;
                    case '\t':
                        await stream.WriteAsync("\\t").ConfigureAwait(false);
                        break;
                    case '\n':
                        await stream.WriteAsync("\\n").ConfigureAwait(false);
                        break;
                    case '"':
                        await stream.WriteAsync("\\\"").ConfigureAwait(false);
                        break;
                    case '\0':
                        await stream.WriteAsync("\\x00").ConfigureAwait(false);
                        break;
                    default:
                        if (IsPrintable(c) && c != '=')
                        {
                            await stream.WriteAsync(c).ConfigureAwait(false);
                            break;
                        }

                        await stream.WriteAsync("\\x").ConfigureAwait(false);
                        await stream.WriteAsync(HexDigit((c >> 4) & 0xF)).ConfigureAwait(false);
                        await stream.WriteAsync(HexDigit((c >> 0) & 0xF)).ConfigureAwait(false);
                        break;
                }
            }
            await stream.WriteAsync('"').ConfigureAwait(false);
        }

        private static bool IsPrintable(char c)
        {
            return c > 0x1f && c < 127;
        }

        private static void ReadStringToken(out string first, out string second, string source)
        {
            if (string.IsNullOrEmpty(source) || source[0] != '"')
            {
                first = "";
                second = source;
                return;
            }
            int size = source.Length;
            int pos;
            for (pos = 1; pos < size; ++pos)
            {
                if (source[pos] == '"' && source[pos - 1] != '\\')
                {
                    ++pos;
                    break;
                }
            }

            first = source.Substring(0, pos);
            second = source.Substring(pos);
        }

        private static bool IsXDigit(char c)
        {
            if ('0' <= c && c <= '9') return true;
            if ('a' <= c && c <= 'f') return true;
            if ('A' <= c && c <= 'F') return true;
            return false;
        }

        private static int FromXDigit(char ch)
        {
            if (ch >= 'a' && ch <= 'f')
                return (ch - 'a' + 10);
            else if (ch >= 'A' && ch <= 'F')
                return (ch - 'A' + 10);
            else
                return ch - '0';
        }

        private static void UnescapeString(string source, out string dest)
        {
            if (!(source.Length >= 2 && source[0] == '"' && source[source.Length - 1] == '"'))
            {
                throw new ArgumentOutOfRangeException(nameof(source), "Source not correct");
            }

            StringBuilder builder = new StringBuilder(source.Length - 2);
            int s = 1;
            int end = source.Length - 1;

            for (; s != end; ++s)
            {
                if (source[s] != '\\')
                {
                    builder.Append(source[s]);
                    continue;
                }
                switch (source[++s])
                {
                    case '\\':
                    case '"':
                        builder.Append(source[s]);
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'x':
                        if (!IsXDigit(source[s + 1]))
                        {
                            builder.Append('x');
                            break;
                        }
                        int ch = FromXDigit(source[++s]);
                        if (IsXDigit(source[s + 1]))
                        {
                            ch <<= 4;
                            ch |= FromXDigit(source[++s]);
                        }
                        builder.Append((char)ch);
                        break;
                    default:
                        builder.Append(source[s - 1]);
                        break;
                }
            }
            dest = builder.ToString();
        }

        public void SavePersistent(Stream stream, bool periodic)
        {
            List<(string key, Value value)> entries = new List<(string key, Value value)>();
            if (!GetPersistentEntries(periodic, entries)) return;
            StreamWriter w = new StreamWriter(stream);
            Task task = SavePersistentImpl(w, entries);
            task.WaitAndUnwrapException();
            w.Flush();
        }

        public string SavePersistent(string filename, bool periodic)
        {
            string err = null;
            try
            {
                string fn = filename;
                string tmp = filename;

                tmp += ".tmp";
                string bak = filename;
                bak += ".bak";

                //Get entries before creating files
                List<(string key, Value value)> entries = new List<(string key, Value value)>();
                if (!GetPersistentEntries(periodic, entries)) return null;



                //Start writing to a temp file
                try
                {
                    using (FileStream fStream = File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (StreamWriter writer = new StreamWriter(fStream))
                    {
                        Debug(Logger.Instance, $"saving persistent file '{filename}'");
                        Task task = SavePersistentImpl(writer, entries);
                        task.WaitAndUnwrapException();
                        writer.Flush();
                    }
                }
                catch (IOException)
                {
                    err = "could not open or save file";
                    return err;
                }

                try
                {
                    File.Delete(bak);
                    File.Move(fn, bak);
                }
                catch (IOException)
                {
                    //Unable to delete or copy. Ignoring
                }

                try
                {
                    File.Move(tmp, fn);
                }
                catch (IOException)
                {
                    //Attempt to restore backup
                    try
                    {
                        File.Move(bak, fn);
                    }
                    catch (IOException)
                    {
                        //Do nothing if it fails
                    }
                    err = "could not rename temp file to real file";
                }
                return err;
            }
            finally
            {
                if (err != null && periodic) m_persistentDirty = true;
            }
        }

        public async Task<string> SavePersistentAsync(string filename, bool periodic)
        {
            string fn = filename;
            string tmp = filename;

            tmp += ".tmp";
            string bak = filename;
            bak += ".bak";

            //Get entries before creating files
            List<(string key, Value value)> entries = new List<(string key, Value value)>();
            if (!GetPersistentEntries(periodic, entries)) return null;

            string err = null;

            //Start writing to a temp file
            try
            {
                using (FileStream fStream = File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(fStream))
                {
                    Debug(Logger.Instance, $"saving persistent file '{filename}'");
                    await SavePersistentImpl(writer, entries).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                err = "could not open or save file";
                m_persistentDirty = true;
                return err;
            }

            try
            {
                File.Delete(bak);
                File.Move(fn, bak);
            }
            catch (IOException)
            {
                //Unable to delete or copy. Ignoring
            }

            try
            {
                File.Move(tmp, fn);
            }
            catch (IOException)
            {
                //Attempt to restore backup
                try
                {
                    File.Move(bak, fn);
                }
                catch (IOException)
                {
                    //Do nothing if it fails
                }
                err = "could not rename temp file to real file";
            }

            if (err != null && periodic) m_persistentDirty = true;
            return err;
        }

        private static Value ReadStringArray(string line, int lineNum, List<string> stringArray, Action<int, string> warn)
        {
            stringArray.Clear();
            while (!string.IsNullOrEmpty(line))
            {
                ReadStringToken(out string elemTok, out line, line);
                if (string.IsNullOrEmpty(elemTok))
                {
                    warn?.Invoke(lineNum, "missing string value");
                    return null;
                }
                if (elemTok[elemTok.Length - 1] != '"')
                {
                    warn?.Invoke(lineNum, "unterminated string value");
                    return null;
                }
                UnescapeString(elemTok, out string str);
                stringArray.Add(str);

                line = line.TrimStart(' ', '\t');
                if (string.IsNullOrEmpty(line)) break;
                if (line[0] != ',')
                {
                    warn?.Invoke(lineNum, "expected comma between strings");
                    return null;
                }
                line = line.Substring(1).TrimStart(' ', '\t');
            }

            return Value.MakeStringArray(stringArray.ToArray());
        }

        private static Value ReadDoubleArray(string line, int lineNum, List<double> doubleArray,
            Action<int, string> warn)
        {
            doubleArray.Clear();
            while (!string.IsNullOrEmpty(line))
            {
                string[] spl = line.Split(new[] { ',' }, 2);
                line = spl.Length == 1 ? string.Empty : spl[1];
                string strTok = spl[0].Trim(' ', '\t');
                bool tmpBoolean = double.TryParse(strTok, out double tmpDouble);
                if (!tmpBoolean)
                {
                    warn?.Invoke(lineNum, "invalid double value");
                    return null;
                }
                doubleArray.Add(tmpDouble);
            }
            return Value.MakeDoubleArray(doubleArray.ToArray());
        }

        private static Value ReadBooleanArray(string line, int lineNum, List<bool> boolArray,
            Action<int, string> warn)
        {
            boolArray.Clear();
            while (!string.IsNullOrEmpty(line))
            {
                string[] spl = line.Split(new[] { ',' }, 2);
                line = spl.Length < 2 ? string.Empty : spl[1];
                string strTok = spl[0].Trim(' ', '\t');
                if (strTok == "true")
                    boolArray.Add(true);
                else if (strTok == "false")
                    boolArray.Add(false);
                else
                {
                    warn?.Invoke(lineNum, "unrecognized boolean value, not 'true' or 'false'");
                    return null;
                }
            }
            return Value.MakeBooleanArray(boolArray.ToArray());
        }

        public string LoadPersistent(string filename, Action<int, string> warn)
        {
            try
            {
                using (Stream stream = new FileStream(filename, FileMode.Open))
                {
                    Task<bool> task = LoadPersistentAsync(stream, warn);
                    task.WaitAndUnwrapException();
                    if (!task.Result) return "error reading file";
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                return "could not open file";
            }
        }

        public async Task<string> LoadPersistentAsync(string filename, Action<int, string> warn)
        {
            try
            {
                using (Stream stream = new FileStream(filename, FileMode.Open))
                {
                    if (!await LoadPersistentAsync(stream, warn).ConfigureAwait(false)) return "error reading file";
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                return "could not open file";
            }
        }

        public bool LoadPersistent(Stream stream, Action<int, string> warn)
        {
            Task<bool> task = LoadPersistentAsync(stream, warn);
            task.WaitAndUnwrapException();
            return task.Result;
        }

        public async Task<bool> LoadPersistentAsync(Stream stream, Action<int, string> warn)
        {
            int lineNum = 1;

            List<(string key, Value value)> entries = new List<(string key, Value value)>();

            List<bool> boolArray = new List<bool>();
            List<double> doubleArray = new List<double>();
            List<string> stringArray = new List<string>();

            using (StreamReader reader = new StreamReader(stream))
            {
                string lineStr;
                while ((lineStr = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    string line = lineStr.Trim();
                    if (line != string.Empty && line[0] != ';' && line[0] != '#')
                    {
                        break;
                    }
                }

                if (lineStr != "[NetworkTables Storage 3.0]")
                {
                    warn?.Invoke(lineNum, "header line mismatch, ignoring rest of file");
                    return false;
                }

                while ((lineStr = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    string line = lineStr.Trim();
                    ++lineNum;

                    if (line == string.Empty || line[0] == ';' || line[0] == '#')
                    {
                        continue;
                    }

                    string[] split = line.Split(new[] { ' ' }, 2);
                    var typeTok = split[0];
                    line = split[1];
                    NtType type = NtType.Unassigned;
                    if (typeTok == "boolean") type = NtType.Boolean;
                    else if (typeTok == "double") type = NtType.Double;
                    else if (typeTok == "string") type = NtType.String;
                    else if (typeTok == "raw") type = NtType.Raw;
                    else if (typeTok == "array")
                    {
                        split = line.Split(new[] { ' ' }, 2);
                        var arrayTok = split[0];
                        line = split[1];
                        if (arrayTok == "boolean") type = NtType.BooleanArray;
                        else if (arrayTok == "double") type = NtType.DoubleArray;
                        else if (arrayTok == "string") type = NtType.StringArray;
                    }

                    if (type == NtType.Unassigned)
                    {
                        warn?.Invoke(lineNum, "unrecognized type");
                        continue;
                    }

                    ReadStringToken(out string nameTok, out line, line);
                    if (string.IsNullOrEmpty(nameTok))
                    {
                        warn?.Invoke(lineNum, "unterminated name string");
                        continue;
                    }
                    UnescapeString(nameTok, out string name);

                    line = line.TrimStart('\t');
                    if (string.IsNullOrEmpty(line) || line[0] != '=')
                    {
                        warn?.Invoke(lineNum, "expected = after name");
                        continue;
                    }
                    line = line.Substring(1).TrimStart(' ', '\t');

                    Value value = null;
                    string str;
                    switch (type)
                    {
                        case NtType.Boolean:
                            if (line == "true")
                                value = Value.MakeBoolean(true);
                            else if (line == "false")
                                value = Value.MakeBoolean(false);
                            else
                            {
                                warn?.Invoke(lineNum, "unrecognized boolean value, not 'true' or 'false'");
                                continue;
                            }
                            break;
                        case NtType.Double:
                            str = line;
                            double tmpDouble;
                            var tmpBoolean = double.TryParse(str, out tmpDouble);
                            if (!tmpBoolean)
                            {
                                warn?.Invoke(lineNum, "invalid double value");
                                continue;
                            }
                            value = Value.MakeDouble(tmpDouble);
                            break;
                        case NtType.String:
                            string strTok;
                            ReadStringToken(out strTok, out line, line);
                            if (string.IsNullOrEmpty(strTok))
                            {
                                warn?.Invoke(lineNum, "missing string value");
                                continue;
                            }
                            if (strTok[strTok.Length - 1] != '"')
                            {
                                warn?.Invoke(lineNum, "unterminated string value");
                                continue;
                            }
                            UnescapeString(strTok, out str);
                            value = Value.MakeString(str);
                            break;
                        case NtType.Raw:
                            value = Value.MakeRaw(Convert.FromBase64String(line));
                            break;
                        case NtType.BooleanArray:
                            value = ReadBooleanArray(line, lineNum, boolArray, warn);
                            if (value == null) continue;
                            break;
                        case NtType.DoubleArray:
                            value = ReadDoubleArray(line, lineNum, doubleArray, warn);
                            if (value == null) continue;
                            break;
                        case NtType.StringArray:
                            value = ReadStringArray(line, lineNum, stringArray, warn);
                            if (value == null) continue;
                            break;
                    }
                    if (name.Length != 0 && value != null)
                    {
                        entries.Add((name, value));
                    }

                }

                List<Message> msgs = new List<Message>();

                IDisposable monitor = null;
                try
                {
                    monitor = await m_monitor.EnterAsync().ConfigureAwait(false);
                    foreach (var i in entries)
                    {
                        if (!m_entries.TryGetValue(i.key, out Entry entry))
                        {
                            entry = new Entry(i.key);
                            m_entries.Add(i.key, entry);
                        }
                        var oldValue = entry.Value;
                        entry.Value = i.value;
                        bool wasPersist = entry.IsPersistent();
                        if (!wasPersist) entry.Flags |= EntryFlags.Persistent;

                        if (m_server && entry.Id == 0xffff)
                        {
                            uint id = (uint)m_idMap.Count;
                            entry.Id = id;
                            m_idMap.Add(entry);
                        }

                        if (m_notifier.LocalNotifiers())
                        {
                            if (oldValue != null)
                            {
                                m_notifier.NotifyEntry(i.key, i.value, (NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                            }
                            else if (oldValue != i.value)
                            {
                                NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal;
                                if (!wasPersist) notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                m_notifier.NotifyEntry(i.key, i.value, notifyFlags);
                            }
                        }

                        if (m_queueOutgoing == null) continue;
                        ++entry.SeqNum;

                        if (oldValue == null || oldValue.Type != i.value.Type)
                        {
                            msgs.Add(Message.EntryAssign(i.key, entry.Id, entry.SeqNum.Value, i.value, entry.Flags));
                        }
                        else if (entry.Id != 0xffff)
                        {
                            if (oldValue != i.value)
                            {
                                msgs.Add(Message.EntryUpdate(entry.Id, entry.SeqNum.Value, i.value));
                            }
                            if (!wasPersist)
                                msgs.Add(Message.FlagsUpdate(entry.Id, entry.Flags));
                        }
                    }

                    if (m_queueOutgoing != null)
                    {
                        var queuOutgoing = m_queueOutgoing;
                        IDisposable monitorToUnlock = Interlocked.Exchange(ref monitor, null);
                        monitorToUnlock.Dispose();
                        foreach (var msg in msgs) queuOutgoing(msg, null, null);
                    }
                }
                finally
                {
                    monitor?.Dispose();
                }
            }
            return true;
        }
    }
}
