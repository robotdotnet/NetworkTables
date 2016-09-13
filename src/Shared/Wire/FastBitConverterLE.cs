using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetworkTables.Wire
{
    internal class FastBitConverterLE : IFastBitConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUShort(List<byte> list, ushort val)
        {
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddShort(List<byte> list, short val)
        {
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUInt(List<byte> list, uint val)
        {
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInt(List<byte> list, int val)
        {
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddULong(List<byte> list, ulong val)
        {
            list.Add((byte)((val >> 56) & 0xff));
            list.Add((byte)((val >> 48) & 0xff));
            list.Add((byte)((val >> 40) & 0xff));
            list.Add((byte)((val >> 32) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLong(List<byte> list, long val)
        {
            list.Add((byte)((val >> 56) & 0xff));
            list.Add((byte)((val >> 48) & 0xff));
            list.Add((byte)((val >> 40) & 0xff));
            list.Add((byte)((val >> 32) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)(val & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDouble(List<byte> list, double val)
        {
            AddLong(list, BitConverter.DoubleToInt64Bits(val));
        }
    }
}
