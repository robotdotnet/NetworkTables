using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NetworkTables.Wire;
using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class BitConverterTests
    {
        private IFastBitConverter m_bitConverter;

        private bool m_useBE;

        public BitConverterTests(bool useBE)
        {
            m_useBE = useBE;
            if (useBE)
            {
                m_bitConverter = new FastBitConverterBE();
            }
            else
            {
                m_bitConverter = new FastBitConverterLE();
            }
        }

        [Test]
        public void AddUShort()
        {
            ushort valNotConverted = 254;
            ushort val = valNotConverted;
            if (m_useBE)
            {
                val = (ushort) IPAddress.HostToNetworkOrder((short) val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddUShort(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddShort()
        {
            short valNotConverted = 254;
            short val = valNotConverted;
            if (m_useBE)
            {
                val = (short)IPAddress.HostToNetworkOrder((short)val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddShort(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddInt()
        {
            int valNotConverted = 254;
            int val = valNotConverted;
            if (m_useBE)
            {
                val = (int)IPAddress.HostToNetworkOrder((int)val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddInt(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddUInt()
        {
            uint valNotConverted = 254;
            uint val = valNotConverted;
            if (m_useBE)
            {
                val = (uint)IPAddress.HostToNetworkOrder((int)val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddUInt(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddULong()
        {
            ulong valNotConverted = 254;
            ulong val = valNotConverted;
            if (m_useBE)
            {
                val = (ulong)IPAddress.HostToNetworkOrder((long)val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddULong(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddLong()
        {
            long valNotConverted = 254;
            long val = valNotConverted;
            if (m_useBE)
            {
                val = (long)IPAddress.HostToNetworkOrder((long)val);
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(2);
            m_bitConverter.AddLong(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }

        [Test]
        public void AddDouble()
        {
            double valNotConverted = 254.254;
            double val = valNotConverted;
            if (m_useBE)
            {
                val = BitConverter.Int64BitsToDouble(IPAddress.HostToNetworkOrder(BitConverter.DoubleToInt64Bits(val)));
            }

            byte[] frameworkArray = BitConverter.GetBytes(val);

            List<byte> customArray = new List<byte>(8);
            m_bitConverter.AddDouble(customArray, valNotConverted);

            Assert.That(customArray, Is.EquivalentTo(frameworkArray));
        }
    }
}
