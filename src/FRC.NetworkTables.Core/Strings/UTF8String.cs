using FRC.NetworkTables.Interop;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables.Strings
{
    /// <summary>
    /// Utility class for working with UTF8 string getting passed to native code.
    /// </summary>
    public static class UTF8String
    {

        /// <summary>
        /// Creates a UTF8 null termincated native string that can be disposed.
        /// </summary>
        /// <param name="str">The string to create as UTF8</param>
        /// <returns>The native UTF8 string</returns>
        public static DisposableNativeString CreateUTF8DisposableString(string str)
        {
            return new DisposableNativeString(str);
        }

        /// <summary>
        /// Reads a UTF8 string from a native pointer.
        /// </summary>
        /// <param name="str">The pointer to read from</param>
        /// <param name="size">The length of the string</param>
        /// <returns>The managed string</returns>
        public static string ReadUTF8String(IntPtr str, UIntPtr size)
        {
            unsafe
            {
                return Encoding.UTF8.GetString((byte*)str, (int)size);
            }
        }

        /// <summary>
        /// Reads a UTF8 string from a native pointer.
        /// </summary>
        /// <param name="str">The pointer to read from</param>
        /// <param name="size">The length of the string</param>
        /// <returns>The managed string</returns>
        public static unsafe string ReadUTF8String(NtString str)
        {
            return Encoding.UTF8.GetString(str.str, (int)str.len);
        }

        /// <summary>
        /// Reads a UTF8 string from a native pointer.
        /// </summary>
        /// <param name="str">The pointer to read from</param>
        /// <param name="size">The length of the string</param>
        /// <returns>The managed string</returns>
        public static unsafe string ReadUTF8String(byte* str, UIntPtr size)
        {
            return Encoding.UTF8.GetString(str, (int)size);
        }

        /// <summary>
        /// Reads a UTF8 string from a null termincated native pointer
        /// </summary>
        /// <param name="str">The pointer to read from (must be null terminated)</param>
        /// <returns>The managed string</returns>
        public static string ReadUTF8String(IntPtr str)
        {
            unsafe
            {
                byte* data = (byte*)str;
                int count = 0;
                while (true)
                {
                    if (data[count] == 0)
                    {
                        break;
                    }
                    count++;
                }

                return ReadUTF8String(str, (UIntPtr)count);
            }
        }

        /// <summary>
        /// Reads a UTF8 string from a null termincated native pointer
        /// </summary>
        /// <param name="str">The pointer to read from (must be null terminated)</param>
        /// <returns>The managed string</returns>
        public static unsafe string ReadUTF8String(byte* str)
        {
            byte* data = str;
            int count = 0;
            while (true)
            {
                if (data[count] == 0)
                {
                    break;
                }
                count++;
            }

            return ReadUTF8String(str, (UIntPtr)count);
        }
    }
}
