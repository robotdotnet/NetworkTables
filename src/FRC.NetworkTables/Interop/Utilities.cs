using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables.Interop
{
    internal static class Utilities
    {
        internal static unsafe void CreateNtString(string vStr, NT_String* nStr)
        {
            fixed (char* str = vStr)
            {
                var encoding = Encoding.UTF8;
                int bytes = encoding.GetByteCount(vStr);
                nStr->str = (byte*)Marshal.AllocHGlobal((bytes) * sizeof(byte));
                nStr->len = (UIntPtr)bytes;
                encoding.GetBytes(str, vStr.Length, nStr->str, bytes);
            }
        }

        internal static unsafe void DisposeNtString(NT_String* str)
        {
            Marshal.FreeHGlobal((IntPtr)str->str);
        }
    }
}
