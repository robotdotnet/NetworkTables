using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables.Strings
{
    /// <summary>
    /// Holds a Cached UTF8 string to pass to native code. There is no way to initialize or dispose of this
    /// string from user code.
    /// </summary>
    public unsafe struct CachedNativeString
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

        internal CachedNativeString(string vStr)
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
        /// Gets the string
        /// </summary>
        /// <returns>The contained string</returns>
        public override string ToString()
        {
            return m_string;
        }
    }
}
