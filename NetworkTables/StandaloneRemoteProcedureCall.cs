using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class StandaloneRemoteProcedureCall
    {
        private StandaloneNtCore m_ntCore;

        public StandaloneRemoteProcedureCall(StandaloneNtCore ntCore)
        {
            m_ntCore = ntCore;
        }

        public void CreateRpc(string name, byte[] def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, def, callback);
        }

        public void CreateRpc(string name, RpcDefinition def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, PackRpcDefinition(def), callback);
        }

        public void CreatePolledRpc(string name, byte[] def)
        {
            m_ntCore.m_storage.CreatePolledRpc(name, def);
        }

        public void CreatePolledRpc(string name, RpcDefinition def)
        {
            m_ntCore.m_storage.CreatePolledRpc(name, PackRpcDefinition(def));
        }

        public bool PollRpc(bool blocking, ref RpcCallInfo callInfo)
        {
            return m_ntCore.m_rpcServer.PollRpc(blocking, ref callInfo);
        }

        public void PostRpcResponse(long rpcId, long callUid, params byte[] result)
        {
            m_ntCore.m_rpcServer.PostRpcResponse(rpcId, callUid, result);
        }

        public long CallRpc(string name, params byte[] param)
        {
            return m_ntCore.m_storage.CallRpc(name, param);
        }

        public long CallRpc(string name, params Value[] param)
        {
            return m_ntCore.m_storage.CallRpc(name, PackRpcValues(param));
        }


        public bool GetRpcResult(bool blocking, long callUid, ref byte[] result)
        {
            return m_ntCore.m_storage.GetRpcResult(blocking, callUid, ref result);
        }

        public byte[] PackRpcDefinition(RpcDefinition def)
        {
            return RemoteProcedureCall.PackRpcDefinition(def);
        }

        public bool UnpackRpcDefinition(byte[] packed, ref RpcDefinition def)
        {
            return RemoteProcedureCall.UnpackRpcDefinition(packed, ref def);
        }

        public byte[] PackRpcValues(params Value[] values)
        {
            return RemoteProcedureCall.PackRpcValues(values);
        }

        public byte[] PackRpcValues(List<Value> values)
        {
            return RemoteProcedureCall.PackRpcValues(values);
        }

        public List<Value> UnpackRpcValues(byte[] packed, params NtType[] types)
        {
            return RemoteProcedureCall.UnpackRpcValues(packed, types);
        }

        public List<Value> UnpackRpcValues(byte[] packed, List<NtType> types)
        {
            return RemoteProcedureCall.UnpackRpcValues(packed, types);
        }
    }
}
