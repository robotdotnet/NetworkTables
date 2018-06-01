using FRC.NetworkTables.Interop;
using System;
using System.Runtime.InteropServices;
using FRC.NetworkTables.Strings;

namespace FRC.NetworkTables
{
    public readonly struct NT_ManagedValue
    {
        public NtType Type { get; }
        public ulong LastChange { get; }
        public NT_EntryUnion Data { get; }

        internal unsafe void CreateNativeFromManaged(NT_Value* value)
        {
            value->last_change = LastChange;
            value->type = Type;
            switch (Type)
            {
                case NtType.Boolean:
                    value->data.v_boolean = Data.VBoolean;
                    break;
                case NtType.Double:
                    value->data.v_double = Data.VDouble;
                    break;
                case NtType.String:
                    Utilities.CreateNtString(Data.VString, &value->data.v_string);
                    break;
                case NtType.Rpc:
                case NtType.Raw:
                    value->data.v_raw.str = (byte*)Marshal.AllocHGlobal(Data.VRaw.Length);
                    value->data.v_raw.len = (UIntPtr)Data.VRaw.Length;
                    for (int i = 0; i < Data.VRaw.Length; i++)
                    {
                        value->data.v_raw.str[i] = Data.VRaw[i];
                    }
                    break;
                case NtType.BooleanArray:
                    value->data.arr_boolean.arr = (NT_Bool*)Marshal.AllocHGlobal(Data.VBooleanArray.Length * sizeof(NT_Bool));
                    value->data.arr_boolean.len = (UIntPtr)Data.VBooleanArray.Length;
                    for (int i = 0; i < Data.VBooleanArray.Length; i++)
                    {
                        value->data.arr_boolean.arr[i] = Data.VBooleanArray[i];
                    }
                    break;
                case NtType.DoubleArray:
                    value->data.arr_double.arr = (double*)Marshal.AllocHGlobal(Data.VDoubleArray.Length * sizeof(double));
                    value->data.arr_double.len = (UIntPtr)Data.VDoubleArray.Length;
                    for (int i = 0; i < Data.VDoubleArray.Length; i++)
                    {
                        value->data.arr_double.arr[i] = Data.VDoubleArray[i];
                    }
                    break;
                case NtType.StringArray:
                    value->data.arr_string.arr = (NT_String*)Marshal.AllocHGlobal(Data.VStringArray.Length * sizeof(NT_String));
                    value->data.arr_string.len = (UIntPtr)Data.VStringArray.Length;
                    for (int i = 0; i < Data.VStringArray.Length; i++)
                    {
                        Utilities.CreateNtString(Data.VStringArray[i], &value->data.arr_string.arr[i]);
                    }
                    break;
            }
        }

        public unsafe static void DisposeCreatedNative(NT_Value* v)
        {
            switch (v->type)
            {
                case NtType.String:
                    Utilities.DisposeNtString(&v->data.v_string);
                    break;
                case NtType.Rpc:
                case NtType.Raw:
                    Marshal.FreeHGlobal((IntPtr)v->data.v_raw.str);
                    break;
                case NtType.BooleanArray:
                    Marshal.FreeHGlobal((IntPtr)v->data.arr_boolean.arr);
                    break;
                case NtType.DoubleArray:
                    Marshal.FreeHGlobal((IntPtr)v->data.arr_double.arr);
                    break;
                case NtType.StringArray:
                    int len = (int)v->data.arr_string.len;
                    for (int i = 0; i < len; i++)
                    {
                        Utilities.DisposeNtString(&v->data.arr_string.arr[i]);
                    }
                    Marshal.FreeHGlobal((IntPtr)v->data.arr_string.arr);
                    break;
            }
        }

        internal unsafe NT_ManagedValue(NT_Value* v)
        {
            LastChange = v->last_change;
            Type = v->type;
            Data = new NT_EntryUnion(v);
            
        }

        internal unsafe NT_ManagedValue(bool v, ulong t)
        {
            LastChange = t;
            Type = NtType.Boolean;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(double v, ulong t)
        {
            LastChange = t;
            Type = NtType.Double;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(string v, ulong t)
        {
            LastChange = t;
            Type = NtType.String;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(byte[] v, ulong t)
        {
            LastChange = t;
            Type = NtType.Raw;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(bool[] v, ulong t)
        {
            LastChange = t;
            Type = NtType.BooleanArray;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(double[] v, ulong t)
        {
            LastChange = t;
            Type = NtType.DoubleArray;
            Data = new NT_EntryUnion(v);
        }

        internal unsafe NT_ManagedValue(string[] v, ulong t)
        {
            LastChange = t;
            Type = NtType.StringArray;
            Data = new NT_EntryUnion(v);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct NT_EntryUnion
    {
        [FieldOffset(0)]
        public readonly bool VBoolean;
        [FieldOffset(0)]
        public readonly double VDouble;
        [FieldOffset(0)]
        public readonly string VString;
        [FieldOffset(0)]
        public readonly byte[] VRaw;
        [FieldOffset(0)]
        public readonly bool[] VBooleanArray;
        [FieldOffset(0)]
        public readonly double[] VDoubleArray;
        [FieldOffset(0)]
        public readonly string[] VStringArray;

        internal NT_EntryUnion(bool v)
        {
            VBoolean = v;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;
        }

        internal NT_EntryUnion(double v)
        {
            VBoolean = false;
            VDouble = v;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;
        }

        internal NT_EntryUnion(string v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = v;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;
        }

        internal NT_EntryUnion(byte[] v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = v;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;
        }

        internal NT_EntryUnion(bool[] v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = v;
            VDoubleArray = null;
            VStringArray = null;
        }

        internal NT_EntryUnion(double[] v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = v;
            VStringArray = null;
        }

        internal NT_EntryUnion(string[] v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = v;
        }

        internal unsafe NT_EntryUnion(NT_Value* v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            switch (v->type)
            {
                case NtType.Unassigned:
                    break;
                case NtType.Boolean:
                    VBoolean = v->data.v_boolean.Get();
                    break;
                case NtType.Double:
                    VDouble = v->data.v_double;
                    break;
                case NtType.String:
                    VString = UTF8String.ReadUTF8String(v->data.v_string.str, v->data.v_string.len);
                    break;
                case NtType.Rpc:
                case NtType.Raw:
                    VRaw = new byte[(int)v->data.v_raw.len];
                    for (int i = 0; i < VRaw.Length; i++)
                    {
                        VRaw[i] = v->data.v_raw.str[i];
                    }
                    break;
                case NtType.BooleanArray:
                    VBooleanArray = new bool[(int)v->data.arr_boolean.len];
                    for (int i = 0; i < VBooleanArray.Length; i++)
                    {
                        VBooleanArray[i] = v->data.arr_boolean.arr[i].Get();
                    }
                    break;
                case NtType.DoubleArray:
                    VDoubleArray = new double[(int)v->data.arr_double.len];
                    for (int i = 0; i < VDoubleArray.Length; i++)
                    {
                        VDoubleArray[i] = v->data.arr_double.arr[i];
                    }
                    break;
                case NtType.StringArray:
                    VStringArray = new string[(int)v->data.arr_string.len];
                    for (int i = 0; i < VStringArray.Length; i++)
                    {
                        VStringArray[i] = UTF8String.ReadUTF8String(v->data.arr_string.arr[i].str, v->data.arr_string.arr[i].len);
                    }
                    break;
            }
        }
    }
}
