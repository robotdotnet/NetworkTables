using FRC.NetworkTables.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct RpcCall : IDisposable
    {
        public RpcCall(NetworkTableEntry entry, NT_RpcCall call)
        {
            Handle = call;
            Entry = entry;
        }

        public NT_RpcCall Handle { get; }
        public NetworkTableEntry Entry { get; }

        public bool IsValid => Handle.Get() != 0;

        public void Dispose()
        {
            if (IsValid)
            {
                CancelResult();
            }
        }

        public byte[] GetResult()
        {
            byte[] result = NtCore.GetRpcResult(Entry.Handle, Handle);
            return result;
        }

        public byte[] GetResult(double timeout)
        {
            byte[] result = NtCore.GetRpcResult(Entry.Handle, Handle, timeout);
            return result;
        }

        public void CancelResult()
        {
            NtCore.CancelRpcResult(Entry.Handle, Handle);
        }
    }
}
