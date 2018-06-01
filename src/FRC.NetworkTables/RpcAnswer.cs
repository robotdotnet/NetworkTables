using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;

namespace FRC.NetworkTables
{
    public readonly struct RpcAnswer
    {
        public NT_Entry EntryHandle { get; }
        public NT_RpcCall Call { get; }
        public string Name { get; }
        public byte[] Params { get; }
        public readonly ConnectionInfo Conn;
        public NetworkTableEntry Entry => new NetworkTableEntry(m_instance, EntryHandle);
        private readonly NetworkTableInstance m_instance;

        internal unsafe RpcAnswer(NetworkTableInstance inst, NT_RpcAnswer* answer)
        {
            EntryHandle = answer->entry;
            Call = answer->call;
            Name = UTF8String.ReadUTF8String(answer->name);
            Params = new Span<byte>(answer->@params.str, (int)answer->@params.len).ToArray();
            Conn = new ConnectionInfo(&answer->conn);
            m_instance = inst;
        }

        public bool IsValid => Call.Get() != 0;

        public void PostResponse(ReadOnlySpan<byte> result)
        {
            NtCore.PostRpcResponse(EntryHandle, Call, result);
        }
    }
}
