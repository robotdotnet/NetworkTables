using System;
using System.Collections.Generic;
using NetworkTables.Exceptions;
using NetworkTables.Support;

namespace NetworkTables
{
    /// <summary>
    /// This class represents all types allowed by NetworkTables
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Creates a value with an Unassigned default type
        /// </summary>
        public Value()
        {
            Type = NtType.Unassigned;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Val.GetHashCode();
        }

        /// <summary>
        /// Gets the type of this Value
        /// </summary>
        public NtType Type { get; }

        internal object Val { get; }

        /// <summary>
        /// Gets the Timestamp when this value was last changed.
        /// </summary>
        public long LastChange { get; } = Timestamp.Now();

        /// <summary>
        /// Gets if the type is boolean
        /// </summary>
        /// <returns></returns>
        public bool IsBoolean() => Type == NtType.Boolean;
        /// <summary>
        /// Gets if the type is double
        /// </summary>
        /// <returns></returns>
        public bool IsDouble() => Type == NtType.Double;
        /// <summary>
        /// Gets if the type is string
        /// </summary>
        /// <returns></returns>
        public bool IsString() => Type == NtType.String;

        /// <summary>
        /// Gets if the type is raw
        /// </summary>
        /// <returns></returns>
        public bool IsRaw() => Type == NtType.Raw;

        /// <summary>
        /// Gets if the type is Rpc
        /// </summary>
        /// <returns></returns>
        public bool IsRpc() => Type == NtType.Rpc;

        /// <summary>
        /// Gets if the type is boolean array
        /// </summary>
        /// <returns></returns>
        public bool IsBooleanArray() => Type == NtType.BooleanArray;

        /// <summary>
        /// Gets if the type is double array
        /// </summary>
        /// <returns></returns>
        public bool IsDoubleArray() => Type == NtType.DoubleArray;

        /// <summary>
        /// Gets if the type is string array
        /// </summary>
        /// <returns></returns>
        public bool IsStringArray() => Type == NtType.StringArray;

        /// <summary>
        /// Gets the boolean value from the type
        /// </summary>
        /// <returns>boolean contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not boolean.</exception>
        public bool GetBoolean()
        {
            if (Type != NtType.Boolean)
            {
                throw new TableKeyDifferentTypeException(NtType.Boolean, Type);
            }
            return (bool)Val;
        }
        /// <summary>
        /// Gets the double value from the type
        /// </summary>
        /// <returns>double contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not double.</exception>
        public double GetDouble()
        {
            if (Type != NtType.Double)
            {
                throw new TableKeyDifferentTypeException(NtType.Double, Type);
            }
            return (double)Val;
        }

        /// <summary>
        /// Gets the string value from the type
        /// </summary>
        /// <returns>string contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not string.</exception>
        public string GetString()
        {
            if (Type != NtType.String)
            {
                throw new TableKeyDifferentTypeException(NtType.String, Type);
            }
            return (string)Val;
        }

        //For reference types (other then strings) return copies;

        /// <summary>
        /// Gets the raw value from the type
        /// </summary>
        /// <returns>raw byte array contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not raw.</exception>
        public byte[] GetRaw()
        {
            if (Type != NtType.Raw)
            {
                throw new TableKeyDifferentTypeException(NtType.Raw, Type);
            }
            byte[] v = (byte[])Val;

            byte[] tmp = new byte[v.Length];
            Array.Copy(v, tmp, v.Length);
            return tmp;
        }

        /// <summary>
        /// Gets the rpc value from the type
        /// </summary>
        /// <returns>rpc byte array contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not rpc.</exception>
        public byte[] GetRpc()
        {
            if (Type != NtType.Rpc)
            {
                throw new TableKeyDifferentTypeException(NtType.Rpc, Type);
            }
            byte[] v = (byte[])Val;

            byte[] tmp = new byte[v.Length];
            Array.Copy(v, tmp, v.Length);
            return tmp;
        }

        /// <summary>
        /// Gets the boolean array value from the type
        /// </summary>
        /// <returns>boolean array contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not boolean array.</exception>
        public bool[] GetBooleanArray()
        {
            if (Type != NtType.BooleanArray)
            {
                throw new TableKeyDifferentTypeException(NtType.BooleanArray, Type);
            }
            bool[] v = (bool[])Val;

            bool[] tmp = new bool[v.Length];
            Array.Copy(v, tmp, v.Length);
            return tmp;
        }

        /// <summary>
        /// Gets the double array value from the type
        /// </summary>
        /// <returns>double array contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not double array.</exception>
        public double[] GetDoubleArray()
        {
            if (Type != NtType.DoubleArray)
            {
                throw new TableKeyDifferentTypeException(NtType.DoubleArray, Type);
            }
            double[] v = (double[])Val;

            double[] tmp = new double[v.Length];
            Array.Copy(v, tmp, v.Length);
            return tmp;
        }

        /// <summary>
        /// Gets the string array value from the type
        /// </summary>
        /// <returns>string array contained in type</returns>
        /// <exception cref="TableKeyDifferentTypeException">Thrown if
        /// type is not string arrya.</exception>
        public string[] GetStringArray()
        {
            if (Type != NtType.StringArray)
            {
                throw new TableKeyDifferentTypeException(NtType.StringArray, Type);
            }
            string[] v = (string[])Val;

            string[] tmp = new string[v.Length];
            Array.Copy(v, tmp, v.Length);
            return tmp;
        }

        /// <summary>
        /// Gets a string representation of the backing value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Val == null) return "Unassigned";
            return Val.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var v = obj as Value;
            if (v == null) return false;
            return this == v;
        }

        /// <summary>
        /// Checks to see if two <see cref="Value">Values</see> are equal
        /// </summary>
        /// <param name="lhs">The left hand <see cref="Value"/></param>
        /// <param name="rhs">The right hand <see cref="Value"/></param>
        /// <returns>True if the <see cref="Value">Values</see> are equal</returns>
        public static bool operator ==(Value lhs, Value rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (((object)lhs == null) || ((object)rhs == null)) return false;
            if (lhs.Type != rhs.Type) return false;
            switch (lhs.Type)
            {
                case NtType.Unassigned:
                    return true;
                case NtType.Boolean:
                    return (bool)lhs.Val == (bool)rhs.Val;
                case NtType.Double:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return (double)lhs.Val == (double)rhs.Val;
                case NtType.String:
                    return (string)lhs.Val == (string)rhs.Val;
                case NtType.Raw:
                case NtType.Rpc:
                    byte[] rawLhs = (byte[])lhs.Val;
                    byte[] rawRhs = (byte[])rhs.Val;
                    if (rawLhs.Length != rawRhs.Length) return false;
                    for (int i = 0; i < rawLhs.Length; i++)
                    {
                        if (rawLhs[i] != rawRhs[i]) return false;
                    }
                    return true;
                case NtType.BooleanArray:
                    bool[] boolLhs = (bool[])lhs.Val;
                    bool[] boolRhs = (bool[])rhs.Val;
                    if (boolLhs.Length != boolRhs.Length) return false;
                    for (int i = 0; i < boolLhs.Length; i++)
                    {
                        if (boolLhs[i] != boolRhs[i]) return false;
                    }
                    return true;
                case NtType.DoubleArray:
                    double[] doubleLhs = (double[])lhs.Val;
                    double[] doubleRhs = (double[])rhs.Val;
                    if (doubleLhs.Length != doubleRhs.Length) return false;
                    for (int i = 0; i < doubleLhs.Length; i++)
                    {
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (doubleLhs[i] != doubleRhs[i]) return false;
                    }
                    return true;
                case NtType.StringArray:
                    string[] stringLhs = (string[])lhs.Val;
                    string[] stringRhs = (string[])rhs.Val;
                    if (stringLhs.Length != stringRhs.Length) return false;
                    for (int i = 0; i < stringLhs.Length; i++)
                    {
                        if (stringLhs[i] != stringRhs[i]) return false;
                    }
                    return true;
                default:
                    return false;

            }
        }

        /// <summary>
        /// Checks to see if two <see cref="Value">Values</see> are not equal
        /// </summary>
        /// <param name="lhs">The left hand <see cref="Value"/></param>
        /// <param name="rhs">The right hand <see cref="Value"/></param>
        /// <returns>True if the <see cref="Value">Values</see> are not equal</returns>
        public static bool operator !=(Value lhs, Value rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Makes a double <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeDouble(double val)
        {
            return new Value(val);
        }

        /// <summary>
        /// Makes a boolean <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeBoolean(bool val)
        {
            return new Value(val);
        }

        /// <summary>
        /// Makes a string <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeString(string val)
        {
            if (val == null) throw new ArgumentNullException(nameof(val));
            return new Value(val);
        }

        /// <summary>
        /// Makes a raw <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeRaw(params byte[] val)
        {
            byte[] tmp = new byte[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        /// <summary>
        /// Makes a raw <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeRaw(List<byte> val)
        {
            return new Value(val.ToArray());
        }

        /// <summary>
        /// Makes an Rpc <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <param name="size">The size of the array to use for the Rpc</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeRpc(byte[] val, int size)
        {
            if (size > val.Length) return null;
            byte[] tmp = new byte[size];
            Array.Copy(val, tmp, size);
            return new Value(tmp, true);
        }

        /// <summary>
        /// Makes an Rpc <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeRpc(params byte[] val)
        {
            byte[] tmp = new byte[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp, true);
        }

        /// <summary>
        /// Makes an Rpc <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeRpc(List<byte> val)
        {
            return new Value(val.ToArray(), true);
        }

        /// <summary>
        /// Makes a boolean array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeBooleanArray(params bool[] val)
        {
            bool[] tmp = new bool[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        /// <summary>
        /// Makes a double array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeDoubleArray(params double[] val)
        {
            double[] tmp = new double[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        /// <summary>
        /// Makes a string array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeStringArray(params string[] val)
        {
            string[] tmp = new string[val.Length];
            Array.Copy(val, tmp, val.Length);
            return new Value(tmp);
        }

        /// <summary>
        /// Makes a boolean array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeBooleanArray(List<bool> val)
        {
            return new Value(val.ToArray());
        }

        /// <summary>
        /// Makes a double array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeDoubleArray(List<double> val)
        {
            return new Value(val.ToArray());
        }

        /// <summary>
        /// Makes a string array <see cref="Value"/>
        /// </summary>
        /// <param name="val">The value to set the <see cref="Value"/> to</param>
        /// <returns>The created <see cref="Value"/></returns>
        public static Value MakeStringArray(List<string> val)
        {
            return new Value(val.ToArray());
        }

        internal static Value MakeEmpty()
        {
            return new Value();
        }

        private Value(string val)
        {
            Type = NtType.String;
            Val = val;
        }

        private Value(byte[] val)
        {
            Type = NtType.Raw;
            Val = val;
        }

        // Extra parameter 
        // ReSharper disable once UnusedParameter.Local
        private Value(byte[] val, bool rpc)
        {
            Type = NtType.Rpc;
            Val = val;
        }

        private Value(bool val)
        {
            Type = NtType.Boolean;
            Val = val;
        }

        private Value(double val)
        {
            Type = NtType.Double;
            Val = val;
        }

        private Value(string[] val)
        {
            Type = NtType.StringArray;
            Val = val;
        }

        private Value(double[] val)
        {
            Type = NtType.DoubleArray;
            Val = val;
        }

        private Value(bool[] val)
        {
            Type = NtType.BooleanArray;
            Val = val;
        }
    }
}
