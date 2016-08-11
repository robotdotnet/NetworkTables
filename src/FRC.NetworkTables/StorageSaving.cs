using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using static NetworkTables.Logging.Logger;

namespace NetworkTables
{
    internal partial class Storage
    {
        private bool GetPersistentEntries(bool periodic, List<StoragePair> entries)
        {
            lock (m_mutex)
            {
                if (periodic && !m_persistentDirty) return false;
                m_persistentDirty = false;
                foreach (var i in m_entries)
                {
                    Entry entry = i.Value;
                    if (!entry.IsPersistent()) continue;
                    entries.Add(new StoragePair(i.Key, entry.Value));
                }
            }
            entries.Sort();
            return true;
        }

        private static void SavePersistentImpl(StreamWriter stream, IEnumerable<StoragePair> entries)
        {
            stream.Write("[NetworkTables Storage 3.0]\n");
            foreach (var i in entries)
            {
                var v = i.Second;
                if (v == null) continue;
                switch (v.Type)
                {
                    case NtType.Boolean:
                        stream.Write("boolean ");
                        break;
                    case NtType.Double:
                        stream.Write("double ");
                        break;
                    case NtType.String:
                        stream.Write("string ");
                        break;
                    case NtType.Raw:
                        stream.Write("raw ");
                        break;
                    case NtType.BooleanArray:
                        stream.Write("array boolean ");
                        break;
                    case NtType.DoubleArray:
                        stream.Write("array double ");
                        break;
                    case NtType.StringArray:
                        stream.Write("array string ");
                        break;
                    default:
                        continue;
                }

                WriteString(stream, i.First);

                stream.Write('=');

                switch (v.Type)
                {
                    case NtType.Boolean:
                        stream.Write(v.GetBoolean() ? "true" : "false");
                        break;
                    case NtType.Double:
                        stream.Write(v.GetDouble());
                        break;
                    case NtType.String:
                        WriteString(stream, v.GetString());
                        break;
                    case NtType.Raw:
                        stream.Write(Convert.ToBase64String(v.GetRaw()));
                        break;
                    case NtType.BooleanArray:
                        bool first = true;
                        foreach (var b in v.GetBooleanArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            stream.Write(b ? "true" : "false");
                        }
                        break;
                    case NtType.DoubleArray:
                        first = true;
                        foreach (var b in v.GetDoubleArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            stream.Write(b);
                        }
                        break;
                    case NtType.StringArray:
                        first = true;
                        foreach (var b in v.GetStringArray())
                        {
                            if (!first) stream.Write(",");
                            first = false;
                            WriteString(stream, b);
                        }
                        break;
                }
                //eol
                stream.Write('\n');
            }
        }

        private static char HexDigit(int x)
        {
            const byte hexChar = (byte)'A';
            return (char)(x < 10 ? (byte)'0' + x : hexChar + x - 10);
        }

        private static void WriteString(TextWriter os, string str)
        {
            os.Write('"');
            foreach (var c in str)
            {
                switch (c)
                {
                    case '\\':
                        os.Write("\\\\");
                        break;
                    case '\t':
                        os.Write("\\t");
                        break;
                    case '\n':
                        os.Write("\\n");
                        break;
                    case '"':
                        os.Write("\\\"");
                        break;
                    case '\0':
                        os.Write("\\x00");
                        break;
                    default:
                        if (IsPrintable(c))
                        {
                            os.Write(c);
                            break;
                        }

                        os.Write("\\x");
                        os.Write(HexDigit((c >> 4) & 0xF));
                        os.Write(HexDigit((c >> 0) & 0xF));
                        break;
                }
            }
            os.Write('"');
        }

        private static bool IsPrintable(char c)
        {
            return c > 0x1f && c < 127;
        }

        public string SavePersistent(string filename, bool periodic)
        {
            string fn = filename;
            string tmp = filename;

            tmp += ".tmp";
            string bak = filename;
            bak += ".bak";

            //Get entries before creating files
            List<StoragePair> entries = new List<StoragePair>();
            if (!GetPersistentEntries(periodic, entries)) return null;

            string err = null;

            //Start writing to a temp file
            try
            {
                using (FileStream fStream = File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(fStream))
                {
                    Debug($"saving persistent file '{filename}'");
                    SavePersistentImpl(writer, entries);
                    writer.Flush();
                }
            }
            catch (IOException)
            {
                err = "could not open or save file";
                goto done;
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

            done:

            if (err != null && periodic) m_persistentDirty = true;
            return err;
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

        public string LoadPersistent(string filename, Action<int, string> warn)
        {
            try
            {
                using (Stream stream = new FileStream(filename, FileMode.Open))
                {
                    if (!LoadPersistent(stream, warn)) return "error reading file";
                    return null;
                }
            }
            catch (FileNotFoundException)
            {
                return "could not open file";
            }
        }

        public void SavePersistent(Stream stream, bool periodic)
        {
            List<StoragePair> entries = new List<StoragePair>();
            if (!GetPersistentEntries(periodic, entries)) return;
            StreamWriter w = new StreamWriter(stream);
            SavePersistentImpl(w, entries);
            w.Flush();
        }

        public bool LoadPersistent(Stream stream, Action<int, string> warn)
        {
            int lineNum = 1;

            List<StoragePair> entries = new List<StoragePair>();

            List<bool> boolArray = new List<bool>();
            List<double> doubleArray = new List<double>();
            List<string> stringArray = new List<string>();
            string strTok = null;

            using (StreamReader reader = new StreamReader(stream))
            {
                string lineStr;
                while ((lineStr = reader.ReadLine()) != null)
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

                while ((lineStr = reader.ReadLine()) != null)
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

                    string nameTok;
                    ReadStringToken(out nameTok, out line, line);
                    if (string.IsNullOrEmpty(nameTok))
                    {
                        warn?.Invoke(lineNum, "unterminated name string");
                        continue;
                    }
                    string name;
                    UnescapeString(nameTok, out name);

                    line = line.TrimStart('\t');
                    if (string.IsNullOrEmpty(line) || line[0] != '=')
                    {
                        warn?.Invoke(lineNum, "expected = after name");
                        continue;
                    }
                    line = line.Substring(1).TrimStart(' ', '\t');

                    Value value = null;
                    string str;
                    bool tmpBoolean;
                    double tmpDouble;
                    string[] spl;
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
                                goto nextLine;
                            }
                            break;
                        case NtType.Double:
                            str = line;
                            tmpBoolean = double.TryParse(str, out tmpDouble);
                            if (!tmpBoolean)
                            {
                                warn?.Invoke(lineNum, "invalid double value");
                                goto nextLine;
                            }
                            value = Value.MakeDouble(tmpDouble);
                            break;
                        case NtType.String:
                            ReadStringToken(out strTok, out line, line);
                            if (string.IsNullOrEmpty(strTok))
                            {
                                warn?.Invoke(lineNum, "missing string value");
                                goto nextLine;
                            }
                            if (strTok[strTok.Length - 1] != '"')
                            {
                                warn?.Invoke(lineNum, "unterminated string value");
                                goto nextLine;
                            }
                            UnescapeString(strTok, out str);
                            value = Value.MakeString(str);
                            break;
                        case NtType.Raw:
                            value = Value.MakeRaw(Convert.FromBase64String(line));
                            break;
                        case NtType.BooleanArray:
                            boolArray.Clear();
                            while (!string.IsNullOrEmpty(line))
                            {
                                spl = line.Split(new[] { ',' }, 2);
                                line = spl.Length < 2 ? string.Empty : spl[1];
                                strTok = spl[0].Trim(' ', '\t');
                                if (strTok == "true")
                                    boolArray.Add(true);
                                else if (strTok == "false")
                                    boolArray.Add(false);
                                else
                                {
                                    warn?.Invoke(lineNum, "unrecognized boolean value, not 'true' or 'false'");
                                    goto nextLine;
                                }
                            }
                            value = Value.MakeBooleanArray(boolArray.ToArray());
                            break;
                        case NtType.DoubleArray:
                            doubleArray.Clear();
                            while (!string.IsNullOrEmpty(line))
                            {
                                spl = line.Split(new[] { ',' }, 2);
                                line = spl.Length == 1 ? string.Empty : spl[1];
                                strTok = spl[0].Trim(' ', '\t');
                                tmpBoolean = double.TryParse(strTok, out tmpDouble);
                                if (!tmpBoolean)
                                {
                                    warn?.Invoke(lineNum, "invalid double value");
                                    goto nextLine;
                                }
                                doubleArray.Add(tmpDouble);
                            }
                            value = Value.MakeDoubleArray(doubleArray.ToArray());
                            break;
                        case NtType.StringArray:
                            stringArray.Clear();
                            while (!string.IsNullOrEmpty(line))
                            {
                                string elemTok;
                                ReadStringToken(out elemTok, out line, line);
                                if (string.IsNullOrEmpty(elemTok))
                                {
                                    warn?.Invoke(lineNum, "missing string value");
                                    goto nextLine;
                                }
                                if (strTok[strTok.Length - 1] != '"')
                                {
                                    warn?.Invoke(lineNum, "unterminated string value");
                                    goto nextLine;
                                }
                                UnescapeString(elemTok, out str);
                                stringArray.Add(str);

                                line = line.TrimStart(' ', '\t');
                                if (string.IsNullOrEmpty(line)) break;
                                if (line[0] != ',')
                                {
                                    warn?.Invoke(lineNum, "expected comma between strings");
                                    goto nextLine;
                                }
                                line = line.Substring(1).TrimStart(' ', '\t');
                            }

                            value = Value.MakeStringArray(stringArray.ToArray());
                            break;
                    }
                    if (name.Length != 0 && value != null)
                    {
                        entries.Add(new StoragePair(name, value));
                    }
                    nextLine:
                    ;
                }

                List<Message> msgs = new List<Message>();

                bool lockTaken = false;
                try
                {
                    Monitor.Enter(m_mutex, ref lockTaken);
                    foreach (var i in entries)
                    {
                        Entry entry;
                        if (!m_entries.TryGetValue(i.First, out entry))
                        {
                            entry = new Entry(i.First);
                            m_entries.Add(i.First, entry);
                        }
                        var oldValue = entry.Value;
                        entry.Value = i.Second;
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
                                m_notifier.NotifyEntry(i.First, i.Second, (NotifyFlags.NotifyNew | NotifyFlags.NotifyLocal));
                            }
                            else if (oldValue != i.Second)
                            {
                                NotifyFlags notifyFlags = NotifyFlags.NotifyUpdate | NotifyFlags.NotifyLocal;
                                if (!wasPersist) notifyFlags |= NotifyFlags.NotifyFlagsChanged;
                                m_notifier.NotifyEntry(i.First, i.Second, notifyFlags);
                            }
                        }

                        if (m_queueOutgoing == null) continue;
                        ++entry.SeqNum;

                        if (oldValue == null || oldValue.Type != i.Second.Type)
                        {
                            msgs.Add(Message.EntryAssign(i.First, entry.Id, entry.SeqNum.Value, i.Second, entry.Flags));
                        }
                        else if (entry.Id != 0xffff)
                        {
                            if (oldValue != i.Second)
                            {
                                msgs.Add(Message.EntryUpdate(entry.Id, entry.SeqNum.Value, i.Second));
                            }
                            if (!wasPersist)
                                msgs.Add(Message.FlagsUpdate(entry.Id, entry.Flags));
                        }
                    }

                    if (m_queueOutgoing != null)
                    {
                        var queuOutgoing = m_queueOutgoing;
                        Monitor.Exit(m_mutex);
                        lockTaken = false;
                        foreach (var msg in msgs) queuOutgoing(msg, null, null);
                    }
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(m_mutex);
                }
            }
            return true;
        }
    }
}
