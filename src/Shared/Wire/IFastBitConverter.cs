using System;
using System.Collections.Generic;

namespace NetworkTables.Wire
{
    internal interface IFastBitConverter
    {
        void AddUShort(List<byte> list, ushort val);
        void AddShort(List<byte> list, short val);
        void AddUInt(List<byte> list, uint val);
        void AddInt(List<byte> list, int val);
        void AddULong(List<byte> list, ulong val);
        void AddLong(List<byte> list, long val);
        void AddDouble(List<byte> list, double val);
    }
}
