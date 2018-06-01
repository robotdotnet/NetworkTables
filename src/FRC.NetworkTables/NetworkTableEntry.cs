using FRC.NetworkTables.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct NetworkTableEntry
    {
        public const int kPersistent = 0x01;

        public NetworkTableEntry(NetworkTableInstance inst, NT_Entry handle)
        {
            Instance = inst;
            Handle = handle;
        }

        public bool IsValid => Handle.Get() != 0;

        public readonly NT_Entry Handle;

        public readonly NetworkTableInstance Instance;

        public bool Exists() => GetEntryType() != NtType.Unassigned;

        public string GetEntryName() => NtCore.GetEntryName(Handle);

        public NtType GetEntryType() => NtCore.GetEntryType(Handle);

        public EntryFlags GetEntryFlags() => NtCore.GetEntryFlags(Handle);

        public ulong GetLastChange() => NtCore.GetEntryLastChange(Handle);

        public EntryInfo? GetEntryInfo()
        {
            return NtCore.GetEntryInfoHandle(Instance, Handle);
        }

        public NetworkTableValue GetValue()
        {
            return new NetworkTableValue(NtCore.GetEntryValue(Handle));
        }

        public bool GetBoolean(bool defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.Boolean)
            {
                return entry.Data.VBoolean;
            }
            return defaultValue;
        }

        public double GetDouble(double defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.Double)
            {
                return entry.Data.VDouble;
            }
            return defaultValue;
        }

        public string GetString(string defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.String)
            {
                return entry.Data.VString;
            }
            return defaultValue;
        }

        public byte[] GetRaw(byte[] defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.Raw)
            {
                return entry.Data.VRaw;
            }
            return defaultValue;
        }

        public bool[] GetBooleanArray(bool[] defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.BooleanArray)
            {
                return entry.Data.VBooleanArray;
            }
            return defaultValue;
        }

        public double[] GetDoubleArray(double[] defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.DoubleArray)
            {
                return entry.Data.VDoubleArray;
            }
            return defaultValue;
        }

        public string[] GetStringArray(string[] defaultValue)
        {
            var entry = NtCore.GetEntryValue(Handle);
            if (entry.Type == NtType.StringArray)
            {
                return entry.Data.VStringArray;
            }
            return defaultValue;
        }

        private double? GetAsDouble<T>(T value)
        {
            switch (value)
            {
                case double v:
                    return v;
                case int v:
                    return v;
                case short v:
                    return v;
                case byte v:
                    return v;
                case long v:
                    return v;
                case float v:
                    return v;
                case ushort v:
                    return v;
                case uint v:
                    return v;
                case ulong v:
                    return v;
                case sbyte v:
                    return v;
                case decimal v:
                    return (double)v;
                default:
                    return null;
            }
        }

        public bool SetDefaultValue<T>(T defaultValue)
        {
            switch(defaultValue)
            {
                case NetworkTableValue v:
                    return NtCore.SetDefaultEntryValue(Handle, v.Value);
                case bool v:
                    return SetDefaultBoolean(v);
                case string v:
                    return SetDefaultString(v);
                case byte[] v:
                    return SetDefaultRaw(v);
                case bool[] v:
                    return SetDefaultBooleanArray(v);
                case double[] v:
                    return SetDefaultDoubleArray(v);
                case string[] v:
                    return SetDefaultStringArray(v);
                default:
                    var d = GetAsDouble(defaultValue);
                    if (d != null)
                    {
                        return SetDefaultDouble(d.Value);
                    }
                    throw new InvalidOperationException($"Value of type {typeof(T).Name} cannot be put into a table");

            }
        }

        public bool SetDefaultBoolean(bool defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultDouble(double defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultString(string defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultRaw(byte[] defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultBooleanArray(bool[] defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultDoubleArray(double[] defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetDefaultStringArray(string[] defaultValue)
        {
            return NtCore.SetDefaultEntryValue(Handle, new NT_ManagedValue(defaultValue, 0));
        }

        public bool SetValue<T>(T value)
        {
            switch (value)
            {
                case NetworkTableValue v:
                    return NtCore.SetDefaultEntryValue(Handle, v.Value);
                case bool v:
                    return SetBoolean(v);
                case string v:
                    return SetString(v);
                case byte[] v:
                    return SetRaw(v);
                case bool[] v:
                    return SetBooleanArray(v);
                case double[] v:
                    return SetDoubleArray(v);
                case string[] v:
                    return SetStringArray(v);
                default:
                    var d = GetAsDouble(value);
                    if (d != null)
                    {
                        return SetDouble(d.Value);
                    }
                    throw new InvalidOperationException($"Value of type {typeof(T).Name} cannot be put into a table");

            }
        }

        public bool SetBoolean(bool value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetDouble(double value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetString(string value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetRaw(byte[] value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetBooleanArray(bool[] value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetDoubleArray(double[] value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public bool SetStringArray(string[] value)
        {
            return NtCore.SetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetValue<T>(T value)
        {
            switch (value)
            {
                case NetworkTableValue v:
                    NtCore.SetDefaultEntryValue(Handle, v.Value);
                    break;
                case bool v:
                    ForceSetBoolean(v);
                    break;
                case string v:
                    ForceSetString(v);
                    break;
                case byte[] v:
                    ForceSetRaw(v);
                    break;
                case bool[] v:
                    ForceSetBooleanArray(v);
                    break;
                case double[] v:
                    ForceSetDoubleArray(v);
                    break;
                case string[] v:
                    ForceSetStringArray(v);
                    break;
                default:
                    var d = GetAsDouble(value);
                    if (d != null)
                    {
                        ForceSetDouble(d.Value);
                        break;
                    }
                    throw new InvalidOperationException($"Value of type {typeof(T).Name} cannot be put into a table");

            }
        }

        public void ForceSetBoolean(bool value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetDouble(double value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetString(string value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetRaw(byte[] value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetBooleanArray(bool[] value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetDoubleArray(double[] value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void ForceSetStringArray(string[] value)
        {
            NtCore.ForceSetEntryValue(Handle, new NT_ManagedValue(value, 0));
        }

        public void SetFlags(EntryFlags flags)
        {
            NtCore.SetEntryFlags(Handle, GetEntryFlags() | flags);
        }

        public void ClearFlags(EntryFlags flags)
        {
            NtCore.SetEntryFlags(Handle, GetEntryFlags() & ~flags);
        }

        public void SetPersistent()
        {
            SetFlags(EntryFlags.Persistent);
        }

        public void ClearPersistent()
        {
            ClearFlags(EntryFlags.Persistent);
        }

        public bool IsPersistent()
        {
            return (GetEntryFlags() & EntryFlags.Persistent) != 0;
        }

        public void Delete()
        {
            NtCore.DeleteEntry(Handle);
        }

        internal void CreateRpc(InAction<RpcAnswer> callback)
        {
            Instance.CreateRpc(this, callback);
        }

        internal RpcCall CallRpc(Span<byte> @params)
        {
            return new RpcCall(this, NtCore.CallRpc(Handle, @params));
        }

        public NT_EntryListener AddListener(InAction<EntryNotification> listener, NotifyFlags flags)
        {
            return Instance.AddEntryListener(this, listener, flags);
        }

        public void RemoveListener(NT_EntryListener listener)
        {
            Instance.RemoveEntryListener(listener);
        }

        //TODO : Equals
    }
}
