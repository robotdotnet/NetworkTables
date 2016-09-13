using System;

namespace NetworkTables.Wire
{
    internal static class FastBitConverterCache
    {
        private static readonly Lazy<FastBitConverterBE> s_fastBE = new Lazy<FastBitConverterBE>(true);
        private static readonly Lazy<FastBitConverterLE> s_fastLE = new Lazy<FastBitConverterLE>(true);

        internal static IFastBitConverter GetFastBitConverter()
        {
            if (BitConverter.IsLittleEndian)
            {
                return s_fastLE.Value;
            }
            else
            {
                return s_fastBE.Value;
            }
        }
    }

}