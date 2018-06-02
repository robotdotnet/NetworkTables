using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FRC.NetworkTables.Strings
{
    /// <summary>
    /// Holds a UTF8 string to pass to native code. Make sure to properly dispose of this to avoid a memory leak
    /// </summary>
    public unsafe struct DisposableNativeString : IDisposable
    {
        /// <summary>
        /// Pointer to this string, null terminated. Do not modify this pointer.
        /// </summary>
        public byte* Buffer;
        /// <summary>
        /// The Length of this string without the null terminator;
        /// </summary>
        public UIntPtr Length;

        private string m_string;

        /// <summary>
        /// Creates a new UTF8 string from a managed string
        /// </summary>
        /// <param name="vStr">The managed string</param>
        public DisposableNativeString(string vStr)
        {
            m_string = vStr;
            unsafe
            {
                fixed (char* str = vStr)
                {
                    var encoding = Encoding.UTF8;
                    int bytes = encoding.GetByteCount(str, vStr.Length);
                    Buffer = (byte*)Marshal.AllocHGlobal((bytes + 1) * sizeof(byte));
                    Length = (UIntPtr)bytes;
                    encoding.GetBytes(str, vStr.Length, Buffer, bytes);
                    Buffer[bytes] = 0;
                }
            }
        }

        /// <summary>
        /// Disposes of the native string
        /// </summary>
        public unsafe void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)Buffer);
            m_string = null;
        }

        /// <summary>
        /// Gets the string
        /// </summary>
        /// <returns>The contained string</returns>
        public override string ToString()
        {
            return m_string;
        }
    }
}
