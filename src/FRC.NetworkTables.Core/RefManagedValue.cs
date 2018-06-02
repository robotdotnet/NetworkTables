using FRC.NetworkTables.Interop;
using System;
using System.Runtime.InteropServices;
using FRC.NetworkTables.Strings;

namespace FRC.NetworkTables
{
    internal readonly ref struct RefManagedValue
    {
        public readonly NtType Type;
        public readonly ulong LastChange;
        public readonly NT_RefEntryUnion Data;

        internal unsafe void CreateNativeFromManaged(NtValue* value)
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
                    var rSpan = Data.VRaw;
                    for (int i = 0; i < rSpan.Length; i++)
                    {
                        value->data.v_raw.str[i] = rSpan[i];
                    }
                    break;
                case NtType.BooleanArray:
                    value->data.arr_boolean.arr = (NtBool*)Marshal.AllocHGlobal(Data.VBooleanArray.Length * sizeof(NtBool));
                    value->data.arr_boolean.len = (UIntPtr)Data.VBooleanArray.Length;
                    var bSpan = Data.VBooleanArray;
                    for (int i = 0; i < bSpan.Length; i++)
                    {
                        value->data.arr_boolean.arr[i] = bSpan[i];
                    }
                    break;
                case NtType.DoubleArray:
                    value->data.arr_double.arr = (double*)Marshal.AllocHGlobal(Data.VDoubleArray.Length * sizeof(double));
                    value->data.arr_double.len = (UIntPtr)Data.VDoubleArray.Length;
                    var dSpan = Data.VDoubleArray;
                    for (int i = 0; i < dSpan.Length; i++)
                    {
                        value->data.arr_double.arr[i] = dSpan[i];
                    }
                    break;
                case NtType.StringArray:
                    value->data.arr_string.arr = (NtString*)Marshal.AllocHGlobal(Data.VStringArray.Length * sizeof(NtString));
                    value->data.arr_string.len = (UIntPtr)Data.VStringArray.Length;
                    var sSpan = Data.VStringArray;
                    for (int i = 0; i < sSpan.Length; i++)
                    {
                        Utilities.CreateNtString(sSpan[i], &value->data.arr_string.arr[i]);
                    }
                    break;
            }
        }

        public unsafe static void DisposeCreatedNative(NtValue* v)
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

        internal unsafe RefManagedValue(NtValue* v)
        {
            LastChange = v->last_change;
            Type = v->type;
            Data = new NT_RefEntryUnion(v);

        }

        internal unsafe RefManagedValue(bool v, ulong t)
        {
            LastChange = t;
            Type = NtType.Boolean;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(double v, ulong t)
        {
            LastChange = t;
            Type = NtType.Double;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<char> v, ulong t)
        {
            LastChange = t;
            Type = NtType.String;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<byte> v, ulong t)
        {
            LastChange = t;
            Type = NtType.Raw;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<byte> v, ulong t, bool r)
        {
            LastChange = t;
            Type = r ? NtType.Rpc : NtType.Raw;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<bool> v, ulong t)
        {
            LastChange = t;
            Type = NtType.BooleanArray;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<double> v, ulong t)
        {
            LastChange = t;
            Type = NtType.DoubleArray;
            Data = new NT_RefEntryUnion(v);
        }

        internal unsafe RefManagedValue(ReadOnlySpan<string> v, ulong t)
        {
            LastChange = t;
            Type = NtType.StringArray;
            Data = new NT_RefEntryUnion(v);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly ref struct NT_RefEntryUnion
    {
        [FieldOffset(0)]
        public readonly bool VBoolean;
        [FieldOffset(0)]
        public readonly double VDouble;
        [FieldOffset(0)]
        public readonly ReadOnlySpan<char> VString;
        [FieldOffset(0)]
        public readonly ReadOnlySpan<byte> VRaw;
        [FieldOffset(0)]
        public readonly ReadOnlySpan<bool> VBooleanArray;
        [FieldOffset(0)]
        public readonly ReadOnlySpan<double> VDoubleArray;
        [FieldOffset(0)]
        public readonly ReadOnlySpan<string> VStringArray;

        internal NT_RefEntryUnion(bool v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VBoolean = v;
        }

        internal NT_RefEntryUnion(double v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VDouble = v;
        }

        internal NT_RefEntryUnion(ReadOnlySpan<char> v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VString = v;
        }

        internal NT_RefEntryUnion(ReadOnlySpan<byte> v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VRaw = v;
        }

        internal NT_RefEntryUnion(ReadOnlySpan<bool> v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VBooleanArray = v;
        }

        internal NT_RefEntryUnion(ReadOnlySpan<double> v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VDoubleArray = v;
        }

        internal NT_RefEntryUnion(ReadOnlySpan<string> v)
        {
            VBoolean = false;
            VDouble = 0;
            VString = null;
            VRaw = null;
            VBooleanArray = null;
            VDoubleArray = null;
            VStringArray = null;

            VStringArray = v;
        }

        internal unsafe NT_RefEntryUnion(NtValue* v)
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
                    VString = UTF8String.ReadUTF8String(v->data.v_string.str, v->data.v_string.len).AsSpan();
                    break;
                case NtType.Rpc:
                case NtType.Raw:
                    var raw = new byte[(int)v->data.v_raw.len];
                    for (int i = 0; i < raw.Length; i++)
                    {
                        raw[i] = v->data.v_raw.str[i];
                    }
                    VRaw = raw;
                    break;
                case NtType.BooleanArray:
                    var barr = new bool[(int)v->data.arr_boolean.len];
                    for (int i = 0; i < barr.Length; i++)
                    {
                        barr[i] = v->data.arr_boolean.arr[i].Get();
                    }
                    VBooleanArray = barr;
                    break;
                case NtType.DoubleArray:
                    var darr = new double[(int)v->data.arr_double.len];
                    for (int i = 0; i < darr.Length; i++)
                    {
                        darr[i] = v->data.arr_double.arr[i];
                    }
                    VDoubleArray = darr;
                    break;
                case NtType.StringArray:
                    var sarr = new string[(int)v->data.arr_string.len];
                    for (int i = 0; i < sarr.Length; i++)
                    {
                        sarr[i] = UTF8String.ReadUTF8String(v->data.arr_string.arr[i].str, v->data.arr_string.arr[i].len);
                    }
                    VStringArray = sarr;
                    break;
            }
        }
    }
}
