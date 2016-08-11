using System.Collections.Generic;
using NetworkTables.Wire;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    public class TestLeb128
    {
        [Test]
        public void TestLeb128Write()
        {
            ExpectLeb128Eq(0, 0, 0x00);
            ExpectLeb128Eq(1, 0, 0x01);
            ExpectLeb128Eq(63, 0, 0x3f);
            ExpectLeb128Eq(64, 0, 0x40);
            ExpectLeb128Eq(0x7f, 0, 0x7f);
            ExpectLeb128Eq(0x80, 0, 0x80, 0x01);
            ExpectLeb128Eq(0x81, 0, 0x81, 0x01);
            ExpectLeb128Eq(0x90, 0, 0x90, 0x01);
            ExpectLeb128Eq(0xff, 0, 0xff, 0x01);
            ExpectLeb128Eq(0x100, 0, 0x80, 0x02);
            ExpectLeb128Eq(0x101, 0, 0x81, 0x02);
        }

        public void ExpectLeb128Eq(ulong value, ulong pad, params byte[] expected)
        {
            List<byte> buf = new List<byte>(32);
            int size = Leb128.WriteUleb128(buf, value);
            Assert.That(size, Is.EqualTo(buf.Count));
            Assert.That(expected, Is.EquivalentTo(buf));
        }

        [Test]
        public void TestLeb128Read()
        {
            ExpectReadLeb128Eq(0, 0x00);
            ExpectReadLeb128Eq(1, 0x01);
            ExpectReadLeb128Eq(63, 0x3f);
            ExpectReadLeb128Eq(64, 0x40);
            ExpectReadLeb128Eq(0x7f, 0x7f);
            ExpectReadLeb128Eq(0x80, 0x80, 0x01);
            ExpectReadLeb128Eq(0x81, 0x81, 0x01);
            ExpectReadLeb128Eq(0x90, 0x90, 0x01);
            ExpectReadLeb128Eq(0xff, 0xff, 0x01);
            ExpectReadLeb128Eq(0x100, 0x80, 0x02);
            ExpectReadLeb128Eq(0x101, 0x81, 0x02);
            ExpectReadLeb128Eq(8320, 0x80, 0xc1, 0x80, 0x80, 0x10);
        }

        public void ExpectReadLeb128Eq(ulong expected, params byte[] value)
        {
            do
            {
                int count = 0;
                ulong val = 0;
                int size = Leb128.ReadUleb128(value, ref count, out val);
                Assert.That(size, Is.EqualTo(value.Length));
                Assert.That(val, Is.EqualTo(expected));
            } while (false);
        }

        [Test]
        public void TestLeb128Size()
        {
            Assert.That(Leb128.SizeUleb128(0), Is.EqualTo(1));

            Assert.That(Leb128.SizeUleb128(0x1UL), Is.EqualTo(1));
            Assert.That(Leb128.SizeUleb128(0x40UL), Is.EqualTo(1));
            Assert.That(Leb128.SizeUleb128(0x7fUL), Is.EqualTo(1));

            Assert.That(Leb128.SizeUleb128(0x80UL), Is.EqualTo(2));
            Assert.That(Leb128.SizeUleb128(0x2000UL), Is.EqualTo(2));
            Assert.That(Leb128.SizeUleb128(0x3fffUL), Is.EqualTo(2));

            Assert.That(Leb128.SizeUleb128(0x4000UL), Is.EqualTo(3));
            Assert.That(Leb128.SizeUleb128(0x100000UL), Is.EqualTo(3));
            Assert.That(Leb128.SizeUleb128(0x1fffffUL), Is.EqualTo(3));

            Assert.That(Leb128.SizeUleb128(0x200000UL), Is.EqualTo(4));
            Assert.That(Leb128.SizeUleb128(0x8000000UL), Is.EqualTo(4));
            Assert.That(Leb128.SizeUleb128(0xfffffffUL), Is.EqualTo(4));

            Assert.That(Leb128.SizeUleb128(0x10000000UL), Is.EqualTo(5));
            Assert.That(Leb128.SizeUleb128(0x40000000UL), Is.EqualTo(5));
            Assert.That(Leb128.SizeUleb128(0x7fffffffUL), Is.EqualTo(5));

            Assert.That(Leb128.SizeUleb128(uint.MaxValue), Is.EqualTo(5));
        }
    }
}
