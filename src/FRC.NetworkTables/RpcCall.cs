using FRC.NetworkTables.Interop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FRC.NetworkTables
{
    public readonly struct RpcCall : IDisposable
    {
        public RpcCall(NetworkTableEntry entry, NT_RpcCall call)
        {
            Handle = call;
            Entry = entry;
        }

        public readonly NT_RpcCall Handle;
        public readonly NetworkTableEntry Entry;

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

        public Task<byte[]> GetResultAsync(CancellationToken token = default)
        {
            NT_Entry handle = Entry.Handle;
            NT_RpcCall call = Handle;
            token.Register(() => {
                NtCore.CancelRpcResult(handle, call);
            });
            return Task.Run(() =>
            {
                return NtCore.GetRpcResult(handle, call);
            });
        }

        public void CancelResult()
        {
            NtCore.CancelRpcResult(Entry.Handle, Handle);
        }
    }
}
