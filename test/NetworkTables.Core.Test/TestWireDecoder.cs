using System.Collections.Generic;
using System.IO;
using System.Text;
using NetworkTables.Wire;
using NUnit.Framework;

namespace NetworkTables.Core.Test
{
    [TestFixture]
    class WireDecoderTest
    {
        readonly Value v_boolean = Value.MakeBoolean(true);
        readonly Value v_double = Value.MakeDouble(1.0);
        readonly Value v_string = Value.MakeString("hello");
        readonly Value v_raw = Value.MakeRaw((byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o');
        readonly Value v_rpc = Value.MakeRpc((byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o');
        readonly Value v_boolArray = Value.MakeBooleanArray(false, true, false);
        readonly Value v_boolArrayBig = Value.MakeBooleanArray(new bool[255]);
        readonly Value v_doubleArray = Value.MakeDoubleArray(0.5, 0.25);
        readonly Value v_doubleArrayBig = Value.MakeDoubleArray(new double[255]);

        readonly Value v_stringArray = Value.MakeStringArray("hello", "goodbye");
        readonly Value v_stringArrayBig;


        readonly string s_normal = "hello";

        private readonly string s_long;
        private readonly string s_big;

        private readonly string s_big2;

        public WireDecoderTest()
        {
            List<string> sa = new List<string>();
            for (int i = 0; i < 255; i++)
            {
                sa.Add("h");
            }
            v_stringArrayBig = Value.MakeStringArray(sa.ToArray());

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 127; i++)
            {
                builder.Append('*');
            }
            builder.Append('x');
            s_long = builder.ToString();

            builder.Clear();
            for (int i = 0; i < 65534; i++)
            {
                builder.Append('*');
            }
            builder.Append('x');
            builder.Append('x');
            builder.Append('x');
            s_big = builder.ToString();

            builder.Clear();
            for (int i = 0; i < 65534; i++)
            {
                builder.Append('*');
            }
            builder.Append('x');
            s_big2 = builder.ToString();

        }

        [Test]
        public void Construct()
        {
            MemoryStream stream = new MemoryStream(new byte[0]);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            Assert.That(d.Error, Is.Null);
            Assert.That(d.ProtoRev, Is.EqualTo(0x0300));
        }

        [Test]
        public void SetProtoRev()
        {
            MemoryStream stream = new MemoryStream(new byte[0]);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            d.ProtoRev = 0x0200;
            Assert.That(d.ProtoRev, Is.EqualTo(0x0200));
        }

        [Test]
        public void Read8()
        {
            byte[] rawData = { 0x05, 0x01, 0x00 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            byte val = 0;
            Assert.That(d.Read8(ref val));
            Assert.That(val, Is.EqualTo(5));

            Assert.That(d.Read8(ref val));
            Assert.That(val, Is.EqualTo(1));

            Assert.That(d.Read8(ref val));
            Assert.That(val, Is.EqualTo(0));

            Assert.That(!d.Read8(ref val));

            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void Read16()
        {
            byte[] rawData = { 0x00, 0x05, 0x00, 0x01, 0x45, 0x67, 0x00, 0x00 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            ushort val = 0;
            Assert.That(d.Read16(ref val));
            Assert.That(val, Is.EqualTo(5));

            Assert.That(d.Read16(ref val));
            Assert.That(val, Is.EqualTo(1));

            Assert.That(d.Read16(ref val));
            Assert.That(val, Is.EqualTo(0x4567));

            Assert.That(d.Read16(ref val));
            Assert.That(val, Is.EqualTo(0));

            Assert.That(!d.Read16(ref val));
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void Read32()
        {
            byte[] rawData = { 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0xab, 0xcd, 0x12, 0x34, 0x56, 0x78, 0x00, 0x00, 0x00, 0x00 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            uint val = 0;
            Assert.That(d.Read32(ref val));
            Assert.That(val, Is.EqualTo(5));

            Assert.That(d.Read32(ref val));
            Assert.That(val, Is.EqualTo(1));

            Assert.That(d.Read32(ref val));
            Assert.That(val, Is.EqualTo(0xabcd));

            Assert.That(d.Read32(ref val));
            Assert.That(val, Is.EqualTo(0x12345678));

            Assert.That(d.Read32(ref val));
            Assert.That(val, Is.EqualTo(0));

            Assert.That(!d.Read32(ref val));

            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadDouble()
        {
            byte[] rawData =
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x41, 0x0c, 0x13, 0x80, 0x00, 0x00, 0x00, 0x00,
                0x7f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x7f, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            double val = 0;
            Assert.That(d.ReadDouble(ref val));
            Assert.That(val, Is.EqualTo(0.0));

            Assert.That(d.ReadDouble(ref val));
            Assert.That(val, Is.EqualTo(2.3e5));

            Assert.That(d.ReadDouble(ref val));
            Assert.That(val, Is.EqualTo(double.PositiveInfinity));

            Assert.That(d.ReadDouble(ref val));
            Assert.That(val, Is.EqualTo(2.2250738585072014e-308));

            Assert.That(d.ReadDouble(ref val));
            Assert.That(val, Is.EqualTo(double.MaxValue));

            Assert.That(!d.ReadDouble(ref val));

            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadUleb128()
        {
            byte[] rawData = { 0x00, 0x7f, 0x80, 0x01, 0x80 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            ulong val = 0;
            Assert.That(d.ReadUleb128(out val));
            Assert.That(val, Is.EqualTo(0));

            Assert.That(d.ReadUleb128(out val));
            Assert.That(val, Is.EqualTo(0x7f));

            Assert.That(d.ReadUleb128(out val));
            Assert.That(val, Is.EqualTo(0x80));

            Assert.That(!d.ReadUleb128(out val));

            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadType()
        {
            byte[] rawData = { 0x00, 0x01, 0x02, 0x03, 0x10, 0x11, 0x12, 0x20 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            NtType val = 0;
            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.Boolean));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.Double));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.String));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.Raw));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.BooleanArray));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.DoubleArray));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.StringArray));

            Assert.That(d.ReadType(ref val));
            Assert.That(val, Is.EqualTo(NtType.Rpc));

            Assert.That(!d.ReadType(ref val));

            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadTypeError()
        {
            byte[] rawData = { 0x30 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            NtType val = 0;

            Assert.That(!d.ReadType(ref val));

            Assert.That(d.Error, Is.Not.Null);
        }

        [Test]
        public void Reset()
        {
            byte[] rawData = { 0x30 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            NtType val = 0;

            Assert.That(!d.ReadType(ref val));

            Assert.That(d.Error, Is.Not.Null);
            d.Reset();
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void BooleanValue2()
        {
            byte[] rawData = new byte[] { 0x01, 0x00 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            var val = d.ReadValue(NtType.Boolean);
            Assert.That(val.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(val.Val, Is.EqualTo(v_boolean.Val));

            var vFalse = Value.MakeBoolean(false);
            val = d.ReadValue(NtType.Boolean);
            Assert.That(val.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            Assert.That(d.ReadValue(NtType.Boolean), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void DoubleValue2()
        {
            byte[] rawData = new byte[]
            {
                0x3f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x3f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            var val = d.ReadValue(NtType.Double);
            Assert.That(val.Type, Is.EqualTo(NtType.Double));
            Assert.That(val.Val, Is.EqualTo(v_double.Val));

            val = d.ReadValue(NtType.Double);
            Assert.That(val.Type, Is.EqualTo(NtType.Double));
            Assert.That(val.Val, Is.EqualTo(v_double.Val));

            Assert.That(d.ReadValue(NtType.Double), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void StringValue2()
        {
            byte[] rawData = new byte[]
            {
                0x00, 0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o',
                0x00, 0x03, (byte)'b', (byte)'y', (byte)'e', 0x55,
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            var val = d.ReadValue(NtType.String);
            Assert.That(val.Type, Is.EqualTo(NtType.String));
            Assert.That(val.Val, Is.EqualTo(v_string.Val));

            var vFalse = Value.MakeString("bye");
            val = d.ReadValue(NtType.String);
            Assert.That(val.Type, Is.EqualTo(NtType.String));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.String), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadBooleanArray2()
        {
            byte[] b = { 0x03, 0x00, 0x01, 0x00, 0x02, 0x01, 0x00, 0xff };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(v_boolArray.Val));

            var boolArray2 = Value.MakeBooleanArray(true, false);
            val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(boolArray2.Val));

            Assert.That(d.ReadValue(NtType.BooleanArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadBooleanArrayBig2()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255; i++)
            {
                s.Add(0x00);
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(v_boolArrayBig.Val));

            Assert.That(d.ReadValue(NtType.BooleanArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }



        [Test]
        public void ReadDoubleArray2()
        {
            byte[] b =
            {
                0x02, 0x3f, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x3f, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55
            };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.DoubleArray);
            Assert.That(val.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(val.Val, Is.EqualTo(v_doubleArray.Val));

            byte by = 0;
            Assert.That(d.Read8(ref by));
            Assert.That(by, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.DoubleArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadDoubleArrayBig2()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255 * 8; i++)
            {
                s.Add(0x00);
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.DoubleArray);
            Assert.That(val.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(val.Val, Is.EqualTo(v_doubleArrayBig.Val));

            Assert.That(d.ReadValue(NtType.DoubleArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadStringArray2()
        {
            List<byte> b = new List<byte>()
            {
                0x02,
                0x00,
                0x05
            };
            b.AddRange(Encoding.UTF8.GetBytes("hello"));
            b.Add(0x00);
            b.Add(0x07);
            b.AddRange(Encoding.UTF8.GetBytes("goodbye"));
            b.Add(0x55);
            MemoryStream stream = new MemoryStream(b.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.StringArray);
            Assert.That(val.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(val.Val, Is.EqualTo(v_stringArray.Val));

            byte by = 0;
            Assert.That(d.Read8(ref by));
            Assert.That(by, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.StringArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadStringArrayBig2()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255; i++)
            {
                s.Add(0x00);
                s.Add(0x01);
                s.Add((byte)'h');
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0200);

            var val = d.ReadValue(NtType.StringArray);
            Assert.That(val.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(val.Val, Is.EqualTo(v_stringArrayBig.Val));

            Assert.That(d.ReadValue(NtType.StringArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadValueError2()
        {
            MemoryStream stream = new MemoryStream(new byte[0]);
            WireDecoder d = new WireDecoder(stream, 0x0200);
            Assert.That(d.ReadValue(NtType.Unassigned), Is.Null);
            Assert.That(d.Error, Is.Not.Null);

            d.Reset();
            Assert.That(d.ReadValue(NtType.Raw), Is.Null);
            Assert.That(d.Error, Is.Not.Null);

            Assert.That(d.ReadValue(NtType.Rpc), Is.Null);
            Assert.That(d.Error, Is.Not.Null);
        }

        [Test]
        public void BooleanValue3()
        {
            byte[] rawData = new byte[] { 0x01, 0x00 };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            var val = d.ReadValue(NtType.Boolean);
            Assert.That(val.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(val.Val, Is.EqualTo(v_boolean.Val));

            var vFalse = Value.MakeBoolean(false);
            val = d.ReadValue(NtType.Boolean);
            Assert.That(val.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            Assert.That(d.ReadValue(NtType.Boolean), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void DoubleValue3()
        {
            byte[] rawData = new byte[]
            {
                0x3f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x3f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            var val = d.ReadValue(NtType.Double);
            Assert.That(val.Type, Is.EqualTo(NtType.Double));
            Assert.That(val.Val, Is.EqualTo(v_double.Val));

            val = d.ReadValue(NtType.Double);
            Assert.That(val.Type, Is.EqualTo(NtType.Double));
            Assert.That(val.Val, Is.EqualTo(v_double.Val));

            Assert.That(d.ReadValue(NtType.Double), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void StringValue3()
        {
            byte[] rawData = new byte[]
            {
                0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o',
                0x03, (byte)'b', (byte)'y', (byte)'e', 0x55,
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            var val = d.ReadValue(NtType.String);
            Assert.That(val.Type, Is.EqualTo(NtType.String));
            Assert.That(val.Val, Is.EqualTo(v_string.Val));

            var vFalse = Value.MakeString("bye");
            val = d.ReadValue(NtType.String);
            Assert.That(val.Type, Is.EqualTo(NtType.String));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.String), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void RawValue3()
        {
            byte[] rawData = new byte[]
            {
                0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o',
                0x03, (byte)'b', (byte)'y', (byte)'e', 0x55,
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            var val = d.ReadValue(NtType.Raw);
            Assert.That(val.Type, Is.EqualTo(NtType.Raw));
            Assert.That(val.Val, Is.EqualTo(v_raw.Val));

            var vFalse = Value.MakeRaw((byte)'b', (byte)'y', (byte)'e');
            val = d.ReadValue(NtType.Raw);
            Assert.That(val.Type, Is.EqualTo(NtType.Raw));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.Raw), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void RpcValue3()
        {
            byte[] rawData = new byte[]
            {
                0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o',
                0x03, (byte)'b', (byte)'y', (byte)'e', 0x55,
            };
            MemoryStream stream = new MemoryStream(rawData);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            var val = d.ReadValue(NtType.Rpc);
            Assert.That(val.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(val.Val, Is.EqualTo(v_rpc.Val));

            var vFalse = Value.MakeRaw((byte)'b', (byte)'y', (byte)'e');
            val = d.ReadValue(NtType.Rpc);
            Assert.That(val.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(val.Val, Is.EqualTo(vFalse.Val));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.Rpc), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadBooleanArray3()
        {
            byte[] b = { 0x03, 0x00, 0x01, 0x00, 0x02, 0x01, 0x00, 0xff };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(v_boolArray.Val));

            var boolArray2 = Value.MakeBooleanArray(true, false);
            val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(boolArray2.Val));

            Assert.That(d.ReadValue(NtType.BooleanArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadBooleanArrayBig3()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255; i++)
            {
                s.Add(0x00);
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.BooleanArray);
            Assert.That(val.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(val.Val, Is.EqualTo(v_boolArrayBig.Val));

            Assert.That(d.ReadValue(NtType.BooleanArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }



        [Test]
        public void ReadDoubleArray3()
        {
            byte[] b =
            {
                0x02, 0x3f, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x3f, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55
            };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.DoubleArray);
            Assert.That(val.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(val.Val, Is.EqualTo(v_doubleArray.Val));

            byte by = 0;
            Assert.That(d.Read8(ref by));
            Assert.That(by, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.DoubleArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadDoubleArrayError3()
        {
            byte[] b =
            {
                0x02,
            };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.DoubleArray);
            Assert.That(val, Is.Null);
        }

        [Test]
        public void ReadStringArrayError3()
        {
            byte[] b =
            {
                0x02,
            };
            MemoryStream stream = new MemoryStream(b);
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.StringArray);
            Assert.That(val, Is.Null);
        }

        [Test]
        public void ReadDoubleArrayBig3()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255 * 8; i++)
            {
                s.Add(0x00);
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.DoubleArray);
            Assert.That(val.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(val.Val, Is.EqualTo(v_doubleArrayBig.Val));

            Assert.That(d.ReadValue(NtType.DoubleArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadStringArray3()
        {
            List<byte> b = new List<byte>()
            {
                0x02,
                0x05
            };
            b.AddRange(Encoding.UTF8.GetBytes("hello"));
            b.Add(0x07);
            b.AddRange(Encoding.UTF8.GetBytes("goodbye"));
            b.Add(0x55);
            MemoryStream stream = new MemoryStream(b.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.StringArray);
            Assert.That(val.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(val.Val, Is.EqualTo(v_stringArray.Val));

            byte by = 0;
            Assert.That(d.Read8(ref by));
            Assert.That(by, Is.EqualTo(0x55));

            Assert.That(d.ReadValue(NtType.StringArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadStringArrayBig3()
        {
            List<byte> s = new List<byte>();
            s.Add(0xff);
            for (int i = 0; i < 255; i++)
            {
                s.Add(0x01);
                s.Add((byte)'h');
            }
            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0300);

            var val = d.ReadValue(NtType.StringArray);
            Assert.That(val.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(val.Val, Is.EqualTo(v_stringArrayBig.Val));

            Assert.That(d.ReadValue(NtType.StringArray), Is.Null);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadValueError3()
        {
            MemoryStream stream = new MemoryStream(new byte[0]);
            WireDecoder d = new WireDecoder(stream, 0x0300);
            Assert.That(d.ReadValue(NtType.Unassigned), Is.Null);
            Assert.That(d.Error, Is.Not.Null);
        }

        [Test]
        public void ReadStringError2()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x00, 0x05 });
            WireDecoder d = new WireDecoder(stream, 0x0200);
            string str = "";
            Assert.That(d.ReadString(ref str), Is.False);
        }

        [Test]
        public void ReadString2()
        {
            byte[] sNormalBytes = Encoding.UTF8.GetBytes(s_normal);
            byte[] sLongBytes = Encoding.UTF8.GetBytes(s_long);
            byte[] sBigBytes = Encoding.UTF8.GetBytes(s_big2);
            List<byte> s = new List<byte>();
            s.AddRange(new byte[] { 0x00, 0x05 });
            s.AddRange(sNormalBytes);
            s.AddRange(new byte[] { 0x00, 0x80 });
            s.AddRange(sLongBytes);
            s.AddRange(new byte[] { 0xff, 0xff });
            s.AddRange(sBigBytes);
            s.Add(0x55);

            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0200);

            string outs = null;
            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_normal));

            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_long));

            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_big2));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadString(ref outs), Is.False);
            Assert.That(d.Error, Is.Null);
        }

        [Test]
        public void ReadStringError3()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x05 });
            WireDecoder d = new WireDecoder(stream, 0x0300);
            string str = "";
            Assert.That(d.ReadString(ref str), Is.False);
        }

        [Test]
        public void ReadRawError3()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x05 });
            WireDecoder d = new WireDecoder(stream, 0x0300);
            byte[] str = null;
            Assert.That(d.ReadRaw(ref str), Is.False);
        }

        [Test]
        public void ReadString3()
        {
            byte[] sNormalBytes = Encoding.UTF8.GetBytes(s_normal);
            byte[] sLongBytes = Encoding.UTF8.GetBytes(s_long);
            byte[] sBigBytes = Encoding.UTF8.GetBytes(s_big);
            List<byte> s = new List<byte>();
            s.Add(0x05);
            s.AddRange(sNormalBytes);
            s.AddRange(new byte[] { 0x80, 0x01 });
            s.AddRange(sLongBytes);
            s.AddRange(new byte[] { 0x81, 0x80, 0x04 });
            s.AddRange(sBigBytes);
            s.Add(0x55);

            MemoryStream stream = new MemoryStream(s.ToArray());
            WireDecoder d = new WireDecoder(stream, 0x0300);

            string outs = null;
            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_normal));

            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_long));

            Assert.That(d.ReadString(ref outs));
            Assert.That(outs, Is.EquivalentTo(s_big));

            byte b = 0;
            Assert.That(d.Read8(ref b));
            Assert.That(b, Is.EqualTo(0x55));

            Assert.That(d.ReadString(ref outs), Is.False);
            Assert.That(d.Error, Is.Null);
        }
    }
}
