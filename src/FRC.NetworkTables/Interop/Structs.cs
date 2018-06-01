using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    public readonly struct NT_Bool
    {
        private readonly int m_value;

        public NT_Bool(int value)
        {
            this.m_value = value;
        }

        public NT_Bool(bool value)
        {
            this.m_value = value ? 1 : 0;
        }

        public bool Get()
        {
            return m_value != 0;
        }

        public static implicit operator NT_Bool(bool value)
        {
            return new NT_Bool(value);
        }
    }

    public unsafe struct NT_String
    {
        public byte* str;
        public UIntPtr len;
    }

    public unsafe struct BoolArr
    {
        public NT_Bool* arr;
        public UIntPtr len;
    }

    public unsafe struct DoubleArr
    {
        public double* arr;
        public UIntPtr len;
    }

    public unsafe struct StringArr
    {
        public NT_String* arr;
        public UIntPtr len;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DataUnion
    {
        [FieldOffset(0)]
        public NT_Bool v_boolean;
        [FieldOffset(0)]
        public double v_double;
        [FieldOffset(0)]
        public NT_String v_string;
        [FieldOffset(0)]
        public NT_String v_raw;
        [FieldOffset(0)]
        public BoolArr arr_boolean;
        [FieldOffset(0)]
        public DoubleArr arr_double;
        [FieldOffset(0)]
        public StringArr arr_string;
    }

    public struct NT_Value
    {
        public NtType type;
        public ulong last_change;
        public DataUnion data;
    }

    public struct NT_EntryInfo
    {
        public NT_Entry entry;
        public NT_String name;
        public NtType type;
        public uint flags;
        public ulong last_change;
    }

    public struct NT_ConnectionInfo
    {
        public NT_String remote_id;
        public NT_String remote_ip;
        public uint remote_port;
        public ulong last_update;
        public uint protocol_version;
    }

    public struct NT_RpcParamDef
    {
        public NT_String name;
        public NtType type;
    }

    public struct NT_RpcResultDef
    {
        public NT_String name;
        public NtType type;
    }

    public unsafe struct NT_RpcDefinition
    {
        public uint version;
        public NT_String name;
        public UIntPtr num_params;
        public NT_RpcParamDef* @params;
        public UIntPtr num_results;
        public NT_RpcResultDef* results;
    }

    public struct NT_RpcAnswer
    {
        public NT_Entry entry;
        public NT_RpcCall call;
        public NT_String name;
        public NT_String @params;
        public NT_ConnectionInfo conn;
    }

    public struct NT_EntryNotification
    {
        public NT_EntryListener listener;
        public NT_Entry entry;
        public NT_String name;
        public NT_Value value;
        public uint flags;
    }

    public struct NT_ConnectionNotification
    {
        public NT_ConnectionListener listener;
        public NT_Bool connected;
        public NT_ConnectionInfo conn;
    }

    public unsafe struct NT_LogMessage
    {
        public NT_Logger logger;
        public uint level;
        public byte* filename;
        public uint line;
        public byte* message;
    }
}
