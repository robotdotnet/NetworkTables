using FRC.NetworkTables.Interop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FRC.NetworkTables
{
    public readonly struct RpcCall : IDisposable
    {
        public RpcCall(NetworkTableEntry entry, Interop.RpcCall call)
        {
            Handle = call;
            Entry = entry;
        }

        public readonly Interop.RpcCall Handle;
        public readonly NetworkTableEntry Entry;

        public bool IsValid => Handle.Get() != 0;

        public void Dispose()
        {
            if (IsValid)
            {
                CancelResult();
            }
        }

        public Span<byte> GetResult()
        {
            Span<byte> store = Span<Byte>.Empty;
            var result = NtCore.GetRpcResult(Entry.Handle, Handle, store);
            return result;
        }

        public Span<byte> GetResult(Span<byte> store)
        {
            var result = NtCore.GetRpcResult(Entry.Handle, Handle, store);
            return result;
        }

        public Span<byte> GetResult(double timeout)
        {
            Span<byte> store = Span<Byte>.Empty;
            var result = NtCore.GetRpcResult(Entry.Handle, Handle, timeout, store);
            return result;
        }

        public Span<byte> GetResult(double timeout, Span<byte> store)
        {
            var result = NtCore.GetRpcResult(Entry.Handle, Handle, timeout, store);
            return result;
        }

        public Task<byte[]> GetResultAsync(CancellationToken token = default)
        {
            Entry handle = Entry.Handle;
            Interop.RpcCall call = Handle;
            token.Register(() => {
                NtCore.CancelRpcResult(handle, call);
            });
            return Task.Run(() =>
            {
                return NtCore.GetRpcResult(handle, call, Span<byte>.Empty).ToArray();
            });
        }

        public void CancelResult()
        {
            NtCore.CancelRpcResult(Entry.Handle, Handle);
        }
    }
}
