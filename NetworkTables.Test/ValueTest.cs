using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.Exceptions;
using NUnit.Framework;

namespace NetworkTables.Test
{
    [TestFixture]
    public class ValueTest
    {
        [Test]
        public void TestToStringUnassigned()
        {
            Value v = new Value();
            Assert.That(v.ToString(), Is.EqualTo("Unassigned"));
        }

        [Test]
        public void TestToString()
        {
            Value v = Value.MakeBoolean(true);
            Assert.That(v.ToString(), Is.EqualTo("True"));
        }

        [Test]
        public void EqualsDifferentObject()
        {
            Assert.That(new Value(), Is.Not.EqualTo("randomstring"));
        }

        [Test]
        public void EqualsNull()
        {
            Value v = new Value();
            object o = null;
            Assert.That(v.Equals(o), Is.False);
            Assert.That(new Value(), Is.Not.EqualTo(null));
        }

        [Test]
        public void ConstructEmpty()
        {
            Value v = new Value();
            Assert.That(v.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void Boolean()
        {
            var v = Value.MakeBoolean(false);
            Assert.That(v.IsBoolean(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(v.GetBoolean(), Is.False);

            v = Value.MakeBoolean(true);
            Assert.That(v.Type, Is.EqualTo(NtType.Boolean));
            Assert.That(v.GetBoolean(), Is.True);
        }

        [Test]
        public void Double()
        {
            var v = Value.MakeDouble(0.5);
            Assert.That(v.IsDouble(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Double));
            Assert.That(v.GetDouble(), Is.EqualTo(0.5));

            v = Value.MakeDouble(0.25);
            Assert.That(v.Type, Is.EqualTo(NtType.Double));
            Assert.That(v.GetDouble(), Is.EqualTo(0.25));
        }

        [Test]
        public void String()
        {
            var v = Value.MakeString("hello");
            Assert.That(v.IsString(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.String));
            Assert.That(v.GetString(), Is.EqualTo("hello"));

            v = Value.MakeString("goodbye");
            Assert.That(v.Type, Is.EqualTo(NtType.String));
            Assert.That(v.GetString(), Is.EqualTo("goodbye"));
        }

        [Test]
        public void Raw()
        {
            byte[] raw = new byte[] { 5, 19, 28 };

            var v = Value.MakeRaw(raw);
            Assert.That(v.IsRaw(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(ReferenceEquals(v.GetRaw(), raw), Is.False);
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = (byte)'a';
            Assert.That(v.GetRaw(), Is.Not.EquivalentTo(raw));
            raw[1] = 19;

            var vR = v.GetRaw();
            vR[1] = (byte)'b';

            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new byte[] { 0, 28, 53 };
            v = Value.MakeRaw(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(ReferenceEquals(v.GetRaw(), raw), Is.False);
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = Encoding.UTF8.GetBytes("goodbye");

            v = Value.MakeRaw(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(ReferenceEquals(v.GetRaw(), raw), Is.False);
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));
        }

        [Test]
        public void Rpc()
        {
            byte[] raw = new byte[] { 5, 19, 28 };

            var v = Value.MakeRpc(raw);
            Assert.That(v.IsRpc(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(ReferenceEquals(v.GetRpc(), raw), Is.False);
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = (byte)'a';
            Assert.That(v.GetRpc(), Is.Not.EquivalentTo(raw));
            raw[1] = 19;

            var vR = v.GetRpc();
            vR[1] = (byte)'b';

            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new byte[] { 0, 28, 53 };
            v = Value.MakeRpc(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(ReferenceEquals(v.GetRpc(), raw), Is.False);
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = Encoding.UTF8.GetBytes("goodbye");

            v = Value.MakeRpc(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(ReferenceEquals(v.GetRpc(), raw), Is.False);
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));
        }

        [Test]
        public void BoolArray()
        {
            bool[] raw = new bool[] { true, false, true };

            var v = Value.MakeBooleanArray(raw);
            Assert.That(v.IsBooleanArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(ReferenceEquals(v.GetBooleanArray(), raw), Is.False);
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = true;
            Assert.That(v.GetBooleanArray(), Is.Not.EquivalentTo(raw));
            raw[1] = false;

            var vR = v.GetBooleanArray();
            vR[1] = true;

            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new bool[] { false, true, false };
            v = Value.MakeBooleanArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(ReferenceEquals(v.GetBooleanArray(), raw), Is.False);
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new bool[] { false, true };

            v = Value.MakeBooleanArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(ReferenceEquals(v.GetBooleanArray(), raw), Is.False);
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));
        }

        [Test]
        public void DoubleArray()
        {
            double[] raw = new double[] { 0.5, 0.25, 0.5 };

            var v = Value.MakeDoubleArray(raw);
            Assert.That(v.IsDoubleArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(ReferenceEquals(v.GetDoubleArray(), raw), Is.False);
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = 0.65;
            Assert.That(v.GetDoubleArray(), Is.Not.EquivalentTo(raw));
            raw[1] = 0.25;

            var vR = v.GetDoubleArray();
            vR[1] = 0.85;

            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new double[] { 0.25, 0.5, 0.25 };
            v = Value.MakeDoubleArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(ReferenceEquals(v.GetDoubleArray(), raw), Is.False);
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new double[] { 0.5, 0.25 };

            v = Value.MakeDoubleArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(ReferenceEquals(v.GetDoubleArray(), raw), Is.False);
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));
        }

        [Test]
        public void StringArray()
        {
            string[] raw = new string[] { "hello", "goodbye", "string" };

            var v = Value.MakeStringArray(raw);
            Assert.That(v.IsStringArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(ReferenceEquals(v.GetStringArray(), raw), Is.False);
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = "falsehood";
            Assert.That(v.GetStringArray(), Is.Not.EquivalentTo(raw));
            raw[1] = "goodbye";

            var vR = v.GetStringArray();
            vR[1] = "foobar";

            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new string[] { "s1", "str2", "string3" };
            v = Value.MakeStringArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(ReferenceEquals(v.GetStringArray(), raw), Is.False);
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new string[] { "short", "er" };

            v = Value.MakeStringArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(ReferenceEquals(v.GetStringArray(), raw), Is.False);
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));
        }


        [Test]
        public void RawList()
        {
            List<byte> raw = new List<byte> { 5, 19, 28 };

            var v = Value.MakeRaw(raw);
            Assert.That(v.IsRaw(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = (byte)'a';
            Assert.That(v.GetRaw(), Is.Not.EquivalentTo(raw));
            raw[1] = 19;

            var vR = v.GetRaw();
            vR[1] = (byte)'b';

            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new List<byte> { 0, 28, 53 };
            v = Value.MakeRaw(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new List<byte>(Encoding.UTF8.GetBytes("goodbye"));

            v = Value.MakeRaw(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Raw));
            Assert.That(v.GetRaw(), Is.EquivalentTo(raw));
        }

        [Test]
        public void RpcList()
        {
            List<byte> raw = new List<byte> { 5, 19, 28 };

            var v = Value.MakeRpc(raw);
            Assert.That(v.IsRpc(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = (byte)'a';
            Assert.That(v.GetRpc(), Is.Not.EquivalentTo(raw));
            raw[1] = 19;

            var vR = v.GetRpc();
            vR[1] = (byte)'b';

            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new List<byte> { 0, 28, 53 };
            v = Value.MakeRpc(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new List<byte>(Encoding.UTF8.GetBytes("goodbye"));

            v = Value.MakeRpc(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.Rpc));
            Assert.That(v.GetRpc(), Is.EquivalentTo(raw));
        }

        [Test]
        public void BoolArrayList()
        {
            List<bool> raw = new List<bool> { true, false, true };

            var v = Value.MakeBooleanArray(raw);
            Assert.That(v.IsBooleanArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = true;
            Assert.That(v.GetBooleanArray(), Is.Not.EquivalentTo(raw));
            raw[1] = false;

            var vR = v.GetBooleanArray();
            vR[1] = true;

            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new List<bool> { false, true, false };
            v = Value.MakeBooleanArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new List<bool> { false, true };

            v = Value.MakeBooleanArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.BooleanArray));
            Assert.That(ReferenceEquals(v.GetBooleanArray(), raw), Is.False);
            Assert.That(v.GetBooleanArray(), Is.EquivalentTo(raw));
        }

        [Test]
        public void DoubleArrayList()
        {
            List<double> raw = new List<double> { 0.5, 0.25, 0.5 };

            var v = Value.MakeDoubleArray(raw);
            Assert.That(v.IsDoubleArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = 0.65;
            Assert.That(v.GetDoubleArray(), Is.Not.EquivalentTo(raw));
            raw[1] = 0.25;

            var vR = v.GetDoubleArray();
            vR[1] = 0.85;

            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new List<double> { 0.25, 0.5, 0.25 };
            v = Value.MakeDoubleArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new List<double> { 0.5, 0.25 };

            v = Value.MakeDoubleArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.DoubleArray));
            Assert.That(v.GetDoubleArray(), Is.EquivalentTo(raw));
        }

        [Test]
        public void StringArrayList()
        {
            List<string> raw = new List<string> { "hello", "goodbye", "string" };

            var v = Value.MakeStringArray(raw);
            Assert.That(v.IsStringArray(), Is.True);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));

            //Modify raw, and make sure copies of the array are created
            raw[1] = "falsehood";
            Assert.That(v.GetStringArray(), Is.Not.EquivalentTo(raw));
            raw[1] = "goodbye";

            var vR = v.GetStringArray();
            vR[1] = "foobar";

            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));

            //Assign with same size
            raw = new List<string> { "s1", "str2", "string3" };
            v = Value.MakeStringArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));


            //Assign with different size
            raw = new List<string> { "short", "er" };

            v = Value.MakeStringArray(raw);
            Assert.That(v.Type, Is.EqualTo(NtType.StringArray));
            Assert.That(v.GetStringArray(), Is.EquivalentTo(raw));
        }

        [Test]
        public void ValueAssertions()
        {
            Value v = new Value();

            TableKeyDifferentTypeException ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetBoolean();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type Boolean does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Boolean));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetBooleanArray();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type BooleanArray does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.BooleanArray));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetDouble();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type Double does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Double));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetDoubleArray();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type DoubleArray does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.DoubleArray));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetRaw();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type Raw does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Raw));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetRpc();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type Rpc does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.Rpc));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetString();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type String does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.String));

            ex = Assert.Throws<TableKeyDifferentTypeException>(() =>
            {
                v.GetStringArray();
            });
            Assert.That(ex.Message, Is.EqualTo("Requested Type StringArray does not match actual Type Unassigned."));
            Assert.That(ex.ThrownByValueGet, Is.True);
            Assert.That(ex.TypeInTable, Is.EqualTo(NtType.Unassigned));
            Assert.That(ex.RequestedType, Is.EqualTo(NtType.StringArray));
        }

        [Test]
        public void MakeRawInvalidSize()
        {
            byte[] b = new byte[2];
            Assert.That(Value.MakeRpc(b, 10), Is.Null);
        }

        [Test]
        public void ValueUnassignedComparison()
        {
            Value v1 = new Value(), v2 = new Value();
            Assert.That(v1, Is.EqualTo(v2));
        }

        [Test]
        public void ValueMixedComparison()
        {
            Value v1 = new Value(), v2 = Value.MakeBoolean(true);
            Assert.That(v1, Is.Not.EqualTo(v2));
            Value v3 = Value.MakeDouble(0.5);
            Assert.That(v2, Is.Not.EqualTo(v3));
        }
    }
}
