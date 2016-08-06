using NetworkTables.Wire;
using static NetworkTables.Message.MsgType;
using static NetworkTables.Logging.Logger;

namespace NetworkTables
{
    internal class Message
    {
        public enum MsgType : uint
        {
            Unknown = 0xffff,
            KeepAlive = 0x00,
            ClientHello = 0x01,
            ProtoUnsup = 0x02,
            ServerHelloDone = 0x03,
            ServerHello = 0x04,
            ClientHelloDone = 0x05,
            EntryAssign = 0x10,
            EntryUpdate = 0x11,
            FlagsUpdate = 0x12,
            EntryDelete = 0x13,
            ClearEntries = 0x14,
            ExecuteRpc = 0x20,
            RpcResponse = 0x21
        };

        public delegate NtType GetEntryTypeFunc(uint id);

        private string m_str = "";

        public Message()
        {
            Type = Unknown;
            Id = 0;
            Flags = 0;
            SeqNumUid = 0;
        }

        private Message(MsgType type)
        {
            Type = type;
            Id = 0;
            Flags = 0;
            SeqNumUid = 0;
        }

        public MsgType Type { get; }

        public bool Is(MsgType type)
        {
            return Type == type;
        }

        public string Str => m_str;

        public Value Val { get; private set; } = new Value();

        public uint Id { get; private set; }

        public uint Flags { get; private set; }

        public uint SeqNumUid { get; private set; }

        private const uint ClearAllMagic = 0xD06CB27Au;

        public void Write(WireEncoder encoder)
        {
            switch (Type)
            {
                case MsgType.KeepAlive:
                    encoder.Write8((byte)MsgType.KeepAlive);
                    break;
                case MsgType.ClientHello:
                    encoder.Write8((byte)MsgType.ClientHello);
                    encoder.Write16((ushort)encoder.ProtoRev);
                    if (encoder.ProtoRev < 0x0300u) return;
                    encoder.WriteString(m_str);
                    break;
                case MsgType.ProtoUnsup:
                    encoder.Write8((byte)MsgType.ProtoUnsup);
                    encoder.Write16((ushort)encoder.ProtoRev);
                    break;
                case MsgType.ServerHelloDone:
                    encoder.Write8((byte)MsgType.ServerHelloDone);
                    break;
                case MsgType.ServerHello:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.ServerHello);
                    encoder.Write8((byte)Flags);
                    encoder.WriteString(m_str);
                    break;
                case MsgType.ClientHelloDone:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.ClientHelloDone);
                    break;
                case MsgType.EntryAssign:
                    encoder.Write8((byte)MsgType.EntryAssign);
                    encoder.WriteString(m_str);
                    encoder.WriteType(Val.Type);
                    encoder.Write16((ushort)Id);
                    encoder.Write16((ushort)SeqNumUid);
                    if (encoder.ProtoRev >= 0x0300u) encoder.Write8((byte)Flags);
                    encoder.WriteValue(Val);
                    break;
                case MsgType.EntryUpdate:
                    encoder.Write8((byte)MsgType.EntryUpdate);
                    encoder.Write16((ushort)Id);
                    encoder.Write16((ushort)SeqNumUid);
                    if (encoder.ProtoRev >= 0x0300u) encoder.WriteType(Val.Type);
                    encoder.WriteValue(Val);
                    break;
                case MsgType.FlagsUpdate:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.FlagsUpdate);
                    encoder.Write16((ushort)Id);
                    encoder.Write8((byte)Flags);
                    break;
                case MsgType.EntryDelete:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.EntryDelete);
                    encoder.Write16((ushort)Id);
                    break;
                case MsgType.ClearEntries:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.ClearEntries);
                    encoder.Write32(ClearAllMagic);
                    break;
                case MsgType.ExecuteRpc:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.ExecuteRpc);
                    encoder.Write16((ushort)Id);
                    encoder.Write16((ushort)SeqNumUid);
                    encoder.WriteValue(Val);
                    break;
                case MsgType.RpcResponse:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)MsgType.RpcResponse);
                    encoder.Write16((ushort)Id);
                    encoder.Write16((ushort)SeqNumUid);
                    encoder.WriteValue(Val);
                    break;
            }
        }

        public static Message Read(WireDecoder decoder, GetEntryTypeFunc getEntryType)
        {
            byte msgType = 0;
            if (!decoder.Read8(ref msgType)) return null;
            MsgType mtype = (MsgType)msgType;
            var msg = new Message(mtype);
            NtType type = 0;
            byte tmpB = 0;
            ushort tmpUs = 0;
            switch (mtype)
            {
                case MsgType.KeepAlive:
                    break;
                case MsgType.ClientHello:
                    ushort protoRev = 0;
                    if (!decoder.Read16(ref protoRev)) return null;
                    msg.Id = protoRev;
                    if (protoRev >= 0x0300u)
                    {
                        if (!decoder.ReadString(ref msg.m_str)) return null;
                    }
                    break;
                case MsgType.ProtoUnsup:
                    ushort rdproto = 0;
                    if (!decoder.Read16(ref rdproto)) return null;
                    msg.Id = rdproto;
                    break;
                case MsgType.ServerHelloDone:
                    break;
                case MsgType.ServerHello:
                    if (decoder.ProtoRev < 0x0300u)
                    {
                        decoder.Error = "received SERVER_HELLO_DONE in protocol < 3.0";
                        return null;
                    }
                    byte rdflgs = 0;
                    if (!decoder.Read8(ref rdflgs)) return null;
                    msg.Flags = rdflgs;
                    if (!decoder.ReadString(ref msg.m_str)) return null;
                    break;
                case MsgType.ClientHelloDone:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "recieved SERVER_HELLO_DONE in protocol < 3.0";
                        return null;
                    }
                    break;
                case MsgType.EntryAssign:
                    if (!decoder.ReadString(ref msg.m_str)) return null;
                    if (!decoder.ReadType(ref type)) return null;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.SeqNumUid = tmpUs;
                    if (decoder.ProtoRev >= 0x0300)
                    {
                        if (!decoder.Read8(ref tmpB)) return null;
                        msg.Flags = tmpB;
                    }
                    msg.Val = decoder.ReadValue(type);
                    if (msg.Val == null) return null;
                    break;
                case MsgType.EntryUpdate:
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.SeqNumUid = tmpUs;
                    if (decoder.ProtoRev >= 0x0300)
                    {
                        if (!decoder.ReadType(ref type)) return null;
                    }
                    else
                    {
                        type = getEntryType(msg.Id);
                    }
                    Debug4($"update message data tyepe: {type}");
                    msg.Val = decoder.ReadValue(type);
                    if (msg.Val == null) return null;
                    break;
                case MsgType.FlagsUpdate:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received FLAGS_UPDATE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    if (!decoder.Read8(ref tmpB)) return null;
                    msg.Flags = tmpB;
                    break;
                case MsgType.EntryDelete:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received ENTRY_DELETE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    break;
                case MsgType.ClearEntries:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received CLEAR_ENTRIES in protocol < 3.0";
                        return null;
                    }
                    uint magic = 0;
                    if (!decoder.Read32(ref magic)) return null;
                    if (magic != ClearAllMagic)
                    {
                        decoder.Error = "received incorrect CLEAR_ENTRIES magic value, ignoring";
                        return null;
                    }
                    break;
                case MsgType.ExecuteRpc:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received EXECUTE_RPC in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.SeqNumUid = tmpUs;
                    ulong size;
                    if (!decoder.ReadUleb128(out size)) return null;
                    byte[] results;
                    if (!decoder.Read(out results, (int)size)) return null;
                    msg.Val = Value.MakeRpc(results, (int)size);
                    break;
                case MsgType.RpcResponse:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received RPC_RESPONSE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.Id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.SeqNumUid = tmpUs;
                    ulong size2;
                    if (!decoder.ReadUleb128(out size2)) return null;
                    byte[] results2;
                    if (!decoder.Read(out results2, (int)size2)) return null;
                    msg.Val = Value.MakeRpc(results2, (int)size2);
                    break;
                default:
                    decoder.Error = "unrecognized message type";
                    Info($"unrecognized message type: {msgType}");
                    return null;
            }
            return msg;
        }

        public static Message KeepAlive() => new Message(MsgType.KeepAlive);

        public static Message ProtoUnsup() => new Message(MsgType.ProtoUnsup);

        public static Message ServerHelloDone() => new Message(MsgType.ServerHelloDone);

        public static Message ClientHelloDone() => new Message(MsgType.ClientHelloDone);

        public static Message ClearEntries() => new Message(MsgType.ClearEntries);

        public static Message ClientHello(string selfId)
        {
            var msg = new Message(MsgType.ClientHello) {m_str = selfId};
            return msg;
        }

        public static Message ServerHello(uint flags, string selfId)
        {
            var msg = new Message(MsgType.ServerHello)
            {
                m_str = selfId,
                Flags = flags
            };
            return msg;
        }

        public static Message EntryAssign(string name, uint id, uint seqNum, Value value, EntryFlags flags)
        {
            var msg = new Message(MsgType.EntryAssign)
            {
                m_str = name,
                Val = value,
                Id = id,
                Flags = (uint) flags,
                SeqNumUid = seqNum
            };
            return msg;
        }

        public static Message EntryUpdate(uint id, uint seqNum, Value value)
        {
            var msg = new Message(MsgType.EntryUpdate)
            {
                Val = value,
                Id = id,
                SeqNumUid = seqNum
            };
            return msg;
        }

        public static Message FlagsUpdate(uint id, EntryFlags flags)
        {
            var msg = new Message(MsgType.FlagsUpdate)
            {
                Id = id,
                Flags = (uint) flags
            };
            return msg;
        }

        public static Message EntryDelete(uint id)
        {
            var msg = new Message(MsgType.EntryDelete) {Id = id};
            return msg;
        }

        public static Message ExecuteRpc(uint id, uint uid, byte[] param)
        {
            var msg = new Message(MsgType.ExecuteRpc)
            {
                Val = Value.MakeRpc(param, param.Length),
                Id = id,
                SeqNumUid = uid
            };
            return msg;
        }

        public static Message RpcResponse(uint id, uint uid, byte[] results)
        {
            var msg = new Message(MsgType.RpcResponse)
            {
                Val = Value.MakeRpc(results, results.Length),
                Id = id,
                SeqNumUid = uid
            };
            return msg;
        }
    }
}
