using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NetworkTables.Wire
{
    internal class FastBitConverterBE : IFastBitConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddUShort(List<byte> list, ushort val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddShort(List<byte> list, short val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddUInt(List<byte> list, uint val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddInt(List<byte> list, int val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddULong(List<byte> list, ulong val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 32) & 0xff));
            list.Add((byte)((val >> 40) & 0xff));
            list.Add((byte)((val >> 48) & 0xff));
            list.Add((byte)((val >> 56) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddLong(List<byte> list, long val)
        {
            list.Add((byte)(val & 0xff));
            list.Add((byte)((val >> 8) & 0xff));
            list.Add((byte)((val >> 16) & 0xff));
            list.Add((byte)((val >> 24) & 0xff));
            list.Add((byte)((val >> 32) & 0xff));
            list.Add((byte)((val >> 40) & 0xff));
            list.Add((byte)((val >> 48) & 0xff));
            list.Add((byte)((val >> 56) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecuritySafeCritical]
        public void AddDouble(List<byte> list, double val)
        {
            AddLong(list, BitConverter.DoubleToInt64Bits(val));
        }
    }
}
