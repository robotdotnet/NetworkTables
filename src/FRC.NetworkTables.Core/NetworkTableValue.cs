using FRC.NetworkTables.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct NetworkTableValue
    {
        public NtType Type => Value.Type;
        public readonly NT_ManagedValue Value;
        public bool IsValid => Type != NtType.Unassigned;

        internal NetworkTableValue(in NT_ManagedValue value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets if the type is boolean
        /// </summary>
        /// <returns>True if the type is boolean</returns>
        public bool IsBoolean() => Type == NtType.Boolean;
        /// <summary>
        /// Gets if the type is double
        /// </summary>
        /// <returns>True if the type is double</returns>
        public bool IsDouble() => Type == NtType.Double;
        /// <summary>
        /// Gets if the type is string
        /// </summary>
        /// <returns>True if the type is string</returns>
        public bool IsString() => Type == NtType.String;

        /// <summary>
        /// Gets if the type is raw
        /// </summary>
        /// <returns>True if the type is raw</returns>
        public bool IsRaw() => Type == NtType.Raw;

        /// <summary>
        /// Gets if the type is Rpc
        /// </summary>
        /// <returns>True if the type is Rpc</returns>
        public bool IsRpc() => Type == NtType.Rpc;

        /// <summary>
        /// Gets if the type is boolean array
        /// </summary>
        /// <returns>True if the type is a boolean array</returns>
        public bool IsBooleanArray() => Type == NtType.BooleanArray;

        /// <summary>
        /// Gets if the type is double array
        /// </summary>
        /// <returns>True if the type is a double array</returns>
        public bool IsDoubleArray() => Type == NtType.DoubleArray;

        /// <summary>
        /// Gets if the type is string array
        /// </summary>
        /// <returns>True if the type is a string array</returns>
        public bool IsStringArray() => Type == NtType.StringArray;

        /// <summary>
        /// Gets the boolean value from the type
        /// </summary>
        /// <returns>boolean contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not boolean.</exception>
        public bool GetBoolean()
        {
            if (Type != NtType.Boolean)
            {
                throw new InvalidCastException($"cannot convert {Type} to boolean");
            }
            return Value.Data.VBoolean;
        }
        /// <summary>
        /// Gets the double value from the type
        /// </summary>
        /// <returns>double contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not double.</exception>
        public double GetDouble()
        {
            if (Type != NtType.Double)
            {
                throw new InvalidCastException($"cannot convert {Type} to double");
            }
            return Value.Data.VDouble;
        }

        /// <summary>
        /// Gets the string value from the type
        /// </summary>
        /// <returns>string contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not string.</exception>
        public string GetString()
        {
            if (Type != NtType.String)
            {
                throw new InvalidCastException($"cannot convert {Type} to string");
            }
            return Value.Data.VString;
        }

        //For reference types (other then strings) return copies;

        /// <summary>
        /// Gets the raw value from the type
        /// </summary>
        /// <returns>raw byte array contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not raw.</exception>
        public byte[] GetRaw()
        {
            if (Type != NtType.Raw)
            {
                throw new InvalidCastException($"cannot convert {Type} to raw");
            }
            return Value.Data.VRaw;
        }

        /// <summary>
        /// Gets the rpc value from the type
        /// </summary>
        /// <returns>rpc byte array contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not rpc.</exception>
        public byte[] GetRpc()
        {
            if (Type != NtType.Rpc)
            {
                throw new InvalidCastException($"cannot convert {Type} to Rpc");
            }
            return Value.Data.VRaw;
        }

        /// <summary>
        /// Gets the boolean array value from the type
        /// </summary>
        /// <returns>boolean array contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not boolean array.</exception>
        public bool[] GetBooleanArray()
        {
            if (Type != NtType.BooleanArray)
            {
                throw new InvalidCastException($"cannot convert {Type} to boolean array");
            }
            return Value.Data.VBooleanArray;
        }

        /// <summary>
        /// Gets the double array value from the type
        /// </summary>
        /// <returns>double array contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not double array.</exception>
        public double[] GetDoubleArray()
        {
            if (Type != NtType.DoubleArray)
            {
                throw new InvalidCastException($"cannot convert {Type} to double array");
            }
            return Value.Data.VDoubleArray;
        }

        /// <summary>
        /// Gets the string array value from the type
        /// </summary>
        /// <returns>string array contained in type</returns>
        /// <exception cref="InvalidCastException">Thrown if
        /// type is not string arrya.</exception>
        public string[] GetStringArray()
        {
            if (Type != NtType.StringArray)
            {
                throw new InvalidCastException($"cannot convert {Type} to string array");
            }
            return Value.Data.VStringArray;
        }

        public static NetworkTableValue MakeBoolean(bool value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeDouble(double value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeString(string value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeRaw(byte[] value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeRpc(byte[] value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now(), true));
        }

        public static NetworkTableValue MakeBooleanArray(bool[] value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeDoubleArray(double[] value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }

        public static NetworkTableValue MakeStringArray(string[] value)
        {
            return new NetworkTableValue(new NT_ManagedValue(value, NtCore.Now()));
        }
    }
}