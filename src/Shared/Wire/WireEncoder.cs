using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NetworkTables.Wire
{
    /// <summary>
    /// Utility class that can write values to be sent over a byte array
    /// </summary>
    public class WireEncoder
    {
        private readonly List<byte> m_buffer = new List<byte>(1024);

        /// <summary>
        /// Gets the current buffer of data
        /// </summary>
        public byte[] Buffer => m_buffer.ToArray();

        /// <summary>
        /// Gets the error currently set by the encoder
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Writes a double to the buffer
        /// </summary>
        /// <param name="val">The value to write</param>
        public void WriteDouble(double val)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.DoubleToInt64Bits(val)));

            m_buffer.AddRange(bytes);
        }

        /// <summary>
        /// Gets the size 
        /// </summary>
        /// <value></value>
        public int Count => m_buffer.Count;

        /// <summary>
        /// Gets or sets the protocol revision of NetworkTables
        /// </summary>
        public int ProtoRev { get; set; }

        /// <summary>
        /// Creates a new <see cref="WireEncoder"/>
        /// </summary>
        /// <param name="protoRev">The protocol vision for the encoder</param>
        public WireEncoder(int protoRev)
        {
            ProtoRev = protoRev;
        }

        /// <summary>
        /// Resets the encoder
        /// </summary>
        public void Reset()
        {
            m_buffer.Clear();
            Error = null;
        }

        /// <summary>
        /// Writes a byte to the encoder
        /// </summary>
        /// <param name="val">The byte to write</param>
        public void Write8(byte val)
        {
            m_buffer.Add(val);
        }

        /// <summary>
        /// Writes a ushort to the encoder
        /// </summary>
        /// <param name="val">The ushort to write to the encoder</param>
        public void Write16(ushort val)
        {
            var tmp = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)val));
            m_buffer.AddRange(tmp);
        }

        /// <summary>
        /// Writes a uint to the encoder
        /// </summary>
        /// <param name="val">The uint to write to the encoder</param>
        public void Write32(uint val)
        {
            m_buffer.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)val)));
        }

        /// <summary>
        /// Write a Uleb128 length to the encoder
        /// </summary>
        /// <param name="val">The Uleb128 length to write</param>
        public void WriteUleb128(ulong val)
        {
            Leb128.WriteUleb128(m_buffer, val);
        }

        /// <summary>
        /// Writes a <see cref="NtType"/> to the encoder
        /// </summary>
        /// <param name="type">The <see cref="NtType"/> to write</param>
        public void WriteType(NtType type)
        {
            switch (type)
            {
                case NtType.Boolean:
                    m_buffer.Add(0x00);
                    break;
                case NtType.Double:
                    m_buffer.Add(0x01);
                    break;
                case NtType.String:
                    m_buffer.Add(0x02);
                    break;
                case NtType.Raw:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "raw type not supported in protocol < 3.0";
                        return;
                    }
                    m_buffer.Add(0x03);
                    break;
                case NtType.BooleanArray:
                    m_buffer.Add(0x10);
                    break;
                case NtType.DoubleArray:
                    m_buffer.Add(0x11);
                    break;
                case NtType.StringArray:
                    m_buffer.Add(0x12);
                    break;
                case NtType.Rpc:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "RPC type not supported in protocol < 3.0";
                        return;
                    }
                    m_buffer.Add(0x20);
                    break;
                default:
                    Error = "unrecognized Type";
                    return;
            }
            Error = null;
        }

        /// <summary>
        /// Gets the size of a value if it was to be written to the encoder
        /// </summary>
        /// <param name="value">The <see cref="Value"/> to check the size of</param>
        /// <returns>The size the <see cref="Value"/> would take on the encoder</returns>
        public int GetValueSize(Value value)
        {
            if (value == null) return 0;
            int size;
            switch (value.Type)
            {
                case NtType.Boolean:
                    return 1;
                case NtType.Double:
                    return 8;
                case NtType.Raw:
                    if (ProtoRev < 0x0300u) return 0;
                    return GetRawSize((byte[])value.Val);
                case NtType.Rpc:
                    if (ProtoRev < 0x0300u) return 0;
                    return GetRawSize((byte[])value.Val);
                case NtType.String:
                    return GetStringSize((string)value.Val);
                case NtType.BooleanArray:
                    size = ((bool[])value.Val).Length;
                    if (size > 0xff) size = 0xff;
                    return 1 + size;
                case NtType.DoubleArray:
                    size = ((double[])value.Val).Length;
                    if (size > 0xff) size = 0xff;
                    return 1 + size * 8;
                case NtType.StringArray:
                    string[] v = (string[])value.Val;
                    size = v.Length;
                    if (size > 0xff) size = 0xff;
                    int len = 1;
                    for (int i = 0; i < size; i++)
                    {
                        len += GetStringSize(v[i]);
                    }
                    return len;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Writes a <see cref="Value"/> to the encoder
        /// </summary>
        /// <param name="value">The <see cref="Value"/> to write</param>
        public void WriteValue(Value value)
        {
            if (value == null)
            {
                Error = "Value cannot be null";
                return;
            }
            switch (value.Type)
            {
                case NtType.Boolean:
                    Write8((bool)value.Val ? (byte)1 : (byte)0);
                    break;
                case NtType.Double:
                    WriteDouble((double)value.Val);
                    break;
                case NtType.Raw:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "Raw values not supported in protocol < 3.0";
                        return;
                    }
                    WriteRaw((byte[])value.Val);
                    break;
                case NtType.String:
                    WriteString((string)value.Val);
                    break;
                case NtType.Rpc:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "RPC values not supported in protocol < 3.0";
                        return;
                    }
                    WriteRaw((byte[])value.Val);
                    break;
                case NtType.BooleanArray:
                    var vB = (bool[])value.Val;
                    int sizeB = vB.Length;
                    if (sizeB > 0xff) sizeB = 0xff;
                    Write8((byte)sizeB);
                    for (int i = 0; i < sizeB; i++)
                    {
                        Write8(vB[i] ? (byte)1 : (byte)0);
                    }
                    break;
                case NtType.DoubleArray:
                    var vD = (double[])value.Val;
                    int sizeD = vD.Length;
                    if (sizeD > 0xff) sizeD = 0xff;
                    Write8((byte)sizeD);
                    for (int i = 0; i < sizeD; i++)
                    {
                        WriteDouble(vD[i]);
                    }
                    break;
                case NtType.StringArray:
                    var vS = (string[])value.Val;
                    int sizeS = vS.Length;
                    if (sizeS > 0xff) sizeS = 0xff;
                    Write8((byte)sizeS);
                    for (int i = 0; i < sizeS; i++)
                    {
                        WriteString(vS[i]);
                    }
                    break;
                default:
                    Error = "unrecognized type when writing value";
                    return;
            }
            Error = null;
        }

        /// <summary>
        /// Gets the size of a raw array if it was to be written to the encoder
        /// </summary>
        /// <param name="raw">The raw array to check the size of</param>
        /// <returns>The size the raw array would take on the encoder</returns>
        public int GetRawSize(byte[] raw)
        {
            return Leb128.SizeUleb128((ulong)raw.Length) + raw.Length;
        }

        /// <summary>
        /// Writes a raw array to the encoder
        /// </summary>
        /// <param name="raw">The raw array to write</param>
        public void WriteRaw(byte[] raw)
        {
            WriteUleb128((ulong)raw.Length);
            m_buffer.AddRange(raw);
        }

        /// <summary>
        /// Gets the size of a string if it was to be written to the encoder
        /// </summary>
        /// <param name="str">The string to check the size of</param>
        /// <returns>The size the string would take on the encoder</returns>
        public int GetStringSize(string str)
        {
            if (ProtoRev < 0x0300u)
            {
                int len = str.Length;
                if (len > 0xffff) len = 0xffff;
                return 2 + len;
            }
            return Leb128.SizeUleb128((ulong)str.Length) + str.Length;
        }

        /// <summary>
        /// Write a string to the encoder
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteString(string str)
        {
            int len = str.Length;
            if (ProtoRev < 0x0300u)
            {
                if (len > 0xffff) len = 0xffff;

                Write16((ushort)len);
                byte[] b = Encoding.UTF8.GetBytes(str);
                byte[] bytes;
                if (b.Length > len)
                {
                    bytes = new byte[len];
                    Array.Copy(b, bytes, len);
                }
                else
                {
                    bytes = b;
                }
                m_buffer.AddRange(bytes);
            }
            else
            {
                WriteUleb128((ulong)len);
                m_buffer.AddRange(Encoding.UTF8.GetBytes(str));
            }


        }
    }
}
