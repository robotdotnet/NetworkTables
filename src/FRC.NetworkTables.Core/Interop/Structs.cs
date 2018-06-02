using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    public readonly struct NtBool
    {
        private readonly int m_value;

        public NtBool(int value)
        {
            this.m_value = value;
        }

        public NtBool(bool value)
        {
            this.m_value = value ? 1 : 0;
        }

        public bool Get()
        {
            return m_value != 0;
        }

        public static implicit operator NtBool(bool value)
        {
            return new NtBool(value);
        }
    }

    public unsafe struct NtString
    {
        public byte* str;
        public UIntPtr len;
    }

    public unsafe struct BoolArr
    {
        public NtBool* arr;
        public UIntPtr len;
    }

    public unsafe struct DoubleArr
    {
        public double* arr;
        public UIntPtr len;
    }

    public unsafe struct StringArr
    {
        public NtString* arr;
        public UIntPtr len;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DataUnion
    {
        [FieldOffset(0)]
        public NtBool v_boolean;
        [FieldOffset(0)]
        public double v_double;
        [FieldOffset(0)]
        public NtString v_string;
        [FieldOffset(0)]
        public NtString v_raw;
        [FieldOffset(0)]
        public BoolArr arr_boolean;
        [FieldOffset(0)]
        public DoubleArr arr_double;
        [FieldOffset(0)]
        public StringArr arr_string;
    }

    public struct NtValue
    {
        public NtType type;
        public ulong last_change;
        public DataUnion data;
    }

    public struct NtEntryInfo
    {
        public NtEntry entry;
        public NtString name;
        public NtType type;
        public uint flags;
        public ulong last_change;
    }

    public struct NtConnectionInfo
    {
        public NtString remote_id;
        public NtString remote_ip;
        public uint remote_port;
        public ulong last_update;
        public uint protocol_version;
    }

    public struct NtRpcParamDef
    {
        public NtString name;
        public NtType type;
    }

    public struct NtRpcResultDef
    {
        public NtString name;
        public NtType type;
    }

    public unsafe struct NtRpcDefinition
    {
        public uint version;
        public NtString name;
        public UIntPtr num_params;
        public NtRpcParamDef* @params;
        public UIntPtr num_results;
        public NtRpcResultDef* results;
    }

    public struct NtRpcAnswer
    {
        public NtEntry entry;
        public NtRpcCall call;
        public NtString name;
        public NtString @params;
        public NtConnectionInfo conn;
    }

    public struct NtEntryNotification
    {
        public NtEntryListener listener;
        public NtEntry entry;
        public NtString name;
        public NtValue value;
        public uint flags;
    }

    public struct NtConnectionNotification
    {
        public NtConnectionListener listener;
        public NtBool connected;
        public NtConnectionInfo conn;
    }

    public unsafe struct NtLogMessage
    {
        public NtLogger logger;
        public uint level;
        public byte* filename;
        public uint line;
        public byte* message;
    }
}
