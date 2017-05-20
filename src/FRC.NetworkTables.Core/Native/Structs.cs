using System;
using System.Runtime.InteropServices;
using System.Text;
using FRC;

namespace NetworkTables.Core.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NtStringRead
    {
        private readonly IntPtr str;
        private readonly UIntPtr len;

        internal NtStringRead(IntPtr str, UIntPtr len)
        {
            this.str = str;
            this.len = len;
        }

        public override string ToString()
        {
            byte[] arr = new byte[len.ToUInt64()];
            Marshal.Copy(str, arr, 0, (int)len.ToUInt64());
            return Encoding.UTF8.GetString(arr);
        }

        public byte[] ToRpcArray()
        {
            byte[] arr = new byte[len.ToUInt64()];
            Marshal.Copy(str, arr, 0, (int)len.ToUInt64());
            return arr;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NtEntryInfo
    {
        public NtStringRead name;
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public NtType type;
        public uint flags;
        public ulong last_change;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
    }


    //Looks like this will always be created for us by the library, so we do not have to write it.
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct NtConnectionInfo
    {
#pragma warning disable 649
        public readonly NtStringRead RemoteId;
        public readonly NtStringRead RemoteIp;
        public readonly uint RemotePort;
        public readonly ulong LastUpdate;
        public readonly uint ProtocolVersion;
#pragma warning restore 649

        internal NtConnectionInfo(NtStringRead remoteId, NtStringRead remoteIp, uint port, ulong update, uint proto)
        {
            RemoteId = remoteId;
            RemoteIp = remoteIp;
            RemotePort = port;
            LastUpdate = update;
            ProtocolVersion = proto;
        }

        public ConnectionInfo ToManaged()
        {
            return new ConnectionInfo(RemoteId.ToString(), RemoteIp.ToString(), (int)RemotePort, (long)LastUpdate, (int)ProtocolVersion);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NtRpcCallInfo
    {
#pragma warning disable 649
        public readonly uint RpcId;
        public readonly uint CallUid;
        public readonly NtStringRead Name;
        public readonly NtStringRead Param;
#pragma warning restore 649

        internal NtRpcCallInfo(NtStringRead name, NtStringRead param, uint rpcId, uint callUid)
        {
            Name = name;
            Param = param;
            RpcId = rpcId;
            CallUid = callUid;
        }

        public RpcCallInfo ToManaged()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return new RpcCallInfo(RpcId, CallUid, Name.ToString(), Param.ToRpcArray());
        }
    }


}
