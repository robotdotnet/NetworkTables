using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRC.NetworkTables.Strings
{
    /// <summary>
    /// Contains a UTF8 string whos memory is owned in managed memory
    /// </summary>
    public class ManagedString
    {
        /// <summary>
        /// The buffer to the string. Do not modify this array. Null terminated
        /// </summary>
        public byte[] Buffer { get; }
        /// <summary>
        /// The length of this string, not including the null terminator
        /// </summary>
        public UIntPtr Length
        {
            get
            {
                return (UIntPtr)(Buffer.Length - 1);
            }
        }
        private string m_string;

        /// <summary>
        /// Constructs a managed UTF8 string from a C# string
        /// </summary>
        /// <param name="str"></param>
        public ManagedString(string str)
        {
            var encoding = Encoding.UTF8;
            var bytes = encoding.GetByteCount(str);
            Buffer = new byte[bytes + 1];
            encoding.GetBytes(str, 0, str.Length, Buffer, 0);
            Buffer[bytes] = 0;
            m_string = str;
        }

        /// <summary>
        /// Gets a Hash Code for this string
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return m_string.GetHashCode();
        }

        /// <summary>
        /// Checks if an object is equal to this string
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if the objects are equal</returns>
        public override bool Equals(object obj)
        {
            return m_string.Equals(obj);
        }

        /// <summary>
        /// Returns the contents of this string
        /// </summary>
        /// <returns>The contents of this string</returns>
        public override string ToString()
        {
            return m_string;
        }
    }
}
