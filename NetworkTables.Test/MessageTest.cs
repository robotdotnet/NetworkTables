using NetworkTables.Wire;
using NUnit.Framework;
using System.IO;
using static NetworkTables.Message.MsgType;

namespace NetworkTables.Test
{
    [TestFixture]
    public class MessageTest
    {
        [Test]
        public void CreateParameterless()
        {
            Message msg = new Message();
            Assert.That(msg.Type, Is.EqualTo(Unknown));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateKeepAlive()
        {
            Message msg = Message.KeepAlive();
            Assert.That(msg.Type, Is.EqualTo(KeepAlive));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateProtoUnsup()
        {
            Message msg = Message.ProtoUnsup();
            Assert.That(msg.Type, Is.EqualTo(ProtoUnsup));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateServerHelloDone()
        {
            Message msg = Message.ServerHelloDone();
            Assert.That(msg.Type, Is.EqualTo(ServerHelloDone));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateClientHelloDone()
        {
            Message msg = Message.ClientHelloDone();
            Assert.That(msg.Type, Is.EqualTo(ClientHelloDone));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateClearEntries()
        {
            Message msg = Message.ClearEntries();
            Assert.That(msg.Type, Is.EqualTo(ClearEntries));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateClientHello()
        {
            string name = "Testing";
            Message msg = Message.ClientHello(name);
            Assert.That(msg.Type, Is.EqualTo(ClientHello));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(name));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateServerHello()
        {
            uint flags = 1;
            string name = "Testing";
            Message msg = Message.ServerHello(flags, name);
            Assert.That(msg.Type, Is.EqualTo(ServerHello));
            Assert.That(msg.Id, Is.EqualTo(0));
            Assert.That(msg.Flags, Is.EqualTo(flags));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(name));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateEntryAssign()
        {
            uint id = 1;
            uint seq = 1;
            EntryFlags flags = EntryFlags.Persistent;
            string name = "Testing";
            Value val = Value.MakeDouble(5);
            Message msg = Message.EntryAssign(name, id, seq, val, flags);
            Assert.That(msg.Type, Is.EqualTo(EntryAssign));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo((uint)flags));
            Assert.That(msg.SeqNumUid, Is.EqualTo(seq));
            Assert.That(msg.Str, Is.EqualTo(name));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.IsDouble());
            Assert.That(msg.Val.GetDouble(), Is.EqualTo(5));
        }

        [Test]
        public void CreateEntryUpdate()
        {
            uint id = 1;
            uint seq = 1;
            Value val = Value.MakeDouble(5);
            Message msg = Message.EntryUpdate(id, seq, val);
            Assert.That(msg.Type, Is.EqualTo(EntryUpdate));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(seq));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.IsDouble());
            Assert.That(msg.Val.GetDouble(), Is.EqualTo(5));
        }

        [Test]
        public void CreateFlagsUpdate()
        {
            EntryFlags flags = EntryFlags.Persistent;
            uint id = 1;
            Message msg = Message.FlagsUpdate(id, flags);
            Assert.That(msg.Type, Is.EqualTo(FlagsUpdate));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo((uint)flags));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateEntryDelete()
        {
            uint id = 1;
            Message msg = Message.EntryDelete(id);
            Assert.That(msg.Type, Is.EqualTo(EntryDelete));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(0));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.Type, Is.EqualTo(NtType.Unassigned));
        }

        [Test]
        public void CreateExecuteRpc()
        {
            uint id = 1;
            uint uid = 1;
            byte[] val = new byte[] { 5, 5, 8, 9, 0, 5 };
            Message msg = Message.ExecuteRpc(id, uid, val);
            Assert.That(msg.Type, Is.EqualTo(ExecuteRpc));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(uid));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.IsRpc());
            byte[] ret = msg.Val.GetRpc();
            Assert.That(ret, Is.EquivalentTo(val));
        }

        [Test]
        public void CreateRpcResponse()
        {
            uint id = 1;
            uint uid = 1;
            byte[] val = new byte[] { 5, 5, 8, 9, 0, 5 };
            Message msg = Message.RpcResponse(id, uid, val);
            Assert.That(msg.Type, Is.EqualTo(RpcResponse));
            Assert.That(msg.Id, Is.EqualTo(id));
            Assert.That(msg.Flags, Is.EqualTo(0));
            Assert.That(msg.SeqNumUid, Is.EqualTo(uid));
            Assert.That(msg.Str, Is.EqualTo(""));
            Assert.That(msg.Val, Is.Not.Null);
            Assert.That(msg.Val.IsRpc());
            byte[] ret = msg.Val.GetRpc();
            Assert.That(ret, Is.EquivalentTo(val));
        }

        [Test]
        public void WriteKeepAlive3()
        {
            Message msg = Message.KeepAlive();
            WireEncoder enc = new WireEncoder(0x0300);
            msg.Write(enc);

            byte[] buf = enc.Buffer;
            Assert.That(buf, Has.Length.EqualTo(1));
            Assert.That(buf[0], Is.EqualTo((byte)KeepAlive));
        }

        [Test]
        public void WriteKeepAlive2()
        {
            Message msg = Message.KeepAlive();
            WireEncoder enc = new WireEncoder(0x0200);
            msg.Write(enc);

            byte[] buf = enc.Buffer;
            Assert.That(buf, Has.Length.EqualTo(1));
            Assert.That(buf[0], Is.EqualTo((byte)KeepAlive));
        }

        [Test]
        public void TestClientHello3()
        {
            string name = "Testing";
            Message msg = Message.ClientHello(name);
            WireEncoder enc = new WireEncoder(0x0300);
            msg.Write(enc);
            byte[] buf = enc.Buffer;
            WireDecoder dec = new WireDecoder(new MemoryStream(buf), 0x0300);
            byte u8 = 0;
            ushort u16 = 0;
            string str = null;
            Assert.That(dec.Read8(ref u8), Is.True);
            Assert.That(u8, Is.EqualTo((byte)ClientHello));
            Assert.That(dec.Read16(ref u16), Is.True);
            Assert.That(u16, Is.EqualTo((ushort)0x0300));
            Assert.That(dec.ReadString(ref str), Is.True);
            Assert.That(str, Is.EqualTo(name));
        }

        [Test]
        public void TestClientHello2()
        {
            string name = "Testing";
            Message msg = Message.ClientHello(name);
            WireEncoder enc = new WireEncoder(0x0200);
            msg.Write(enc);
            byte[] buf = enc.Buffer;
            WireDecoder dec = new WireDecoder(new MemoryStream(buf), 0x0200);
            byte u8 = 0;
            ushort u16 = 0;
            string str = null;
            Assert.That(dec.Read8(ref u8), Is.True);
            Assert.That(u8, Is.EqualTo((byte)ClientHello));
            Assert.That(dec.Read16(ref u16), Is.True);
            Assert.That(u16, Is.EqualTo((ushort)0x0200));
            Assert.That(dec.ReadString(ref str), Is.False);
        }
    }
}
