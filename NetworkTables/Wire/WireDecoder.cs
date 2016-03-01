using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Wire
{
    internal class WireDecoder
    {
        public static double ReadDouble(byte[] buf, int count)
        {
            return BitConverter.Int64BitsToDouble(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buf, count * 8)));
        }

        private Stream m_stream;
        private byte[] m_buffer;
        private int m_allocated;

        public string Error { get; internal set; }

        public uint ProtoRev { get; set; }

        public WireDecoder(Stream istream, uint protoRev)
        {
            m_allocated = 1024;
            m_buffer = new byte[m_allocated];
            Error = null;
            m_stream = istream;
            ProtoRev = protoRev;
        }

        public void Reset()
        {
            Error = null;
        }

        private void Realloc(int len)
        {
            if (m_allocated >= len) return;
            int newLen = m_allocated * 2;
            while (newLen < len) newLen *= 2;
            byte[] newBuf = new byte[newLen];
            Array.Copy(m_buffer, newBuf, m_buffer.Length);
            m_buffer = newBuf;
            m_allocated = newLen;
        }

        public bool Read(out byte[] buf, int len)
        {
            if (len > m_allocated) Realloc(len);
            buf = m_buffer;
            int rv = m_stream.Read(m_buffer, 0, len);
            return rv != 0;
        }

        public bool ReadType(ref NtType type)
        {
            byte itype = 0;
            if (!Read8(ref itype)) return false;
            switch (itype)
            {
                case 0x00:
                    type = NtType.Boolean;
                    break;
                case 0x01:
                    type = NtType.Double;
                    break;
                case 0x02:
                    type = NtType.String;
                    break;
                case 0x03:
                    type = NtType.Raw;
                    break;
                case 0x10:
                    type = NtType.BooleanArray;
                    break;
                case 0x11:
                    type = NtType.DoubleArray;
                    break;
                case 0x12:
                    type = NtType.StringArray;
                    break;
                case 0x20:
                    type = NtType.Rpc;
                    break;
                default:
                    type = NtType.Unassigned;
                    Error = "unrecognized value type";
                    return false;
            }
            return true;
        }

        public Value ReadValue(NtType type)
        {
            byte size = 0;
            byte[] buf;
            Error = null;
            switch (type)
            {
                case NtType.Boolean:
                    byte vB = 0;
                    return !Read8(ref vB) ? null : Value.MakeBoolean(vB != 0);
                case NtType.Double:
                    double vD = 0;
                    return !ReadDouble(ref vD) ? null : Value.MakeDouble(vD);
                case NtType.Raw:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "Received raw value in protocol < 3.0";
                        return null;
                    }
                    byte[] vRa = null;
                    return !ReadRaw(ref vRa) ? null : Value.MakeRaw(vRa);
                case NtType.Rpc:
                    if (ProtoRev < 0x0300u)
                    {
                        Error = "Received raw value in protocol < 3.0";
                        return null;
                    }
                    byte[] vR = null;
                    return !ReadRaw(ref vR) ? null : Value.MakeRpc(vR, vR.Length);
                case NtType.String:
                    string vS = "";
                    return !ReadString(ref vS) ? null : Value.MakeString(vS);
                case NtType.BooleanArray:
                    if (!Read8(ref size)) return null;

                    if (!Read(out buf, size)) return null;
                    bool[] bBuf = new bool[size];
                    for (int i = 0; i < size; i++)
                    {
                        bBuf[i] = buf[i] != 0;
                    }
                    return Value.MakeBooleanArray(bBuf);
                case NtType.DoubleArray:
                    if (!Read8(ref size)) return null;
                    if (!Read(out buf, size * 8)) return null;
                    double[] dBuf = new double[size];
                    for (int i = 0; i < size; i++)
                    {
                        dBuf[i] = ReadDouble(buf, i);
                    }
                    return Value.MakeDoubleArray(dBuf);
                case NtType.StringArray:
                    if (!Read8(ref size)) return null;
                    string[] sBuf = new string[size];
                    for (int i = 0; i < size; i++)
                    {
                        if (!ReadString(ref sBuf[i])) return null;
                    }
                    return Value.MakeStringArray(sBuf);
                default:
                    Error = "invalid type when trying to read value";
                    Console.WriteLine("invalid type when trying to read value");
                    return null;
            }
        }

        public bool ReadUleb128(out ulong val)
        {
            return Leb128.ReadUleb128(m_stream, out val);
        }

        public bool Read8(ref byte val)
        {
            byte[] buf;
            if (!Read(out buf, 1)) return false;
            val = (byte)(buf[0] & 0xff);
            return true;
        }

        public bool Read16(ref ushort val)
        {
            byte[] buf;
            if (!Read(out buf, 2)) return false;
            val = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(buf, 0));
            return true;
        }

        public bool Read32(ref uint val)
        {
            byte[] buf;
            if (!Read(out buf, 4)) return false;
            val = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(buf, 0));
            return true;
        }

        public bool ReadDouble(ref double val)
        {
            byte[] buf;
            if (!Read(out buf, 8)) return false;
            val = ReadDouble(buf, 0);
            return true;
        }

        public bool ReadString(ref string val)
        {
            int len;
            if (ProtoRev < 0x0300u)
            {
                ushort v = 0;
                if (!Read16(ref v)) return false;
                len = v;
            }
            else
            {
                ulong v = 0;
                if (!ReadUleb128(out v)) return false;
                len = (int)v;
            }
            byte[] buf;
            if (!Read(out buf, len)) return false;
            val = Encoding.UTF8.GetString(buf, 0, len);
            return true;
        }

        public bool ReadRaw(ref byte[] val)
        {
            ulong v;
            if (!ReadUleb128(out v)) return false;
            var len = (int)v;

            byte[] buf;
            if (!Read(out buf, len)) return false;

            val = new byte[len];
            Array.Copy(m_buffer, 0, val, 0, len);
            return true;
        }
    }
}
