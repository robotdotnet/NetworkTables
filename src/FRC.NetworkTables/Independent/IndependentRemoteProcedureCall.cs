using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTables.Independent
{
    /// <summary>
    /// A RemoteProcedureCall class that can be run without the static NetworkTables implmentation
    /// </summary>
    public class IndependentRemoteProcedureCall
    {
        private readonly IndependentNtCore m_ntCore;

        /// <summary>
        /// Creates a new <see cref="IndependentNetworkTable"/> object from a <see cref="IndependentNtCore"/> object
        /// </summary>
        /// <param name="ntCore">The <see cref="IndependentNtCore"/> object to use</param>
        public IndependentRemoteProcedureCall(IndependentNtCore ntCore)
        {
            m_ntCore = ntCore;
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreateRpc(string, IList{byte}, RpcCallback)"/>
        public void CreateRpc(string name, IList<byte> def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, def, callback);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreateRpc(string, RpcDefinition, RpcCallback)"/>
        public void CreateRpc(string name, RpcDefinition def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, PackRpcDefinition(def), callback);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreatePolledRpc(string, IList{byte})"/>
        public void CreatePolledRpc(string name, IList<byte> def)
        {
            m_ntCore.m_storage.CreatePolledRpc(name, def);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreatePolledRpc(string, RpcDefinition)"/>
        public void CreatePolledRpc(string name, RpcDefinition def)
        {
            m_ntCore.m_storage.CreatePolledRpc(name, PackRpcDefinition(def));
        }

        /// <inheritdoc cref="RemoteProcedureCall.PollRpcAsync"/>
        public async Task<RpcCallInfo?> PollRpcAsync(CancellationToken token)
        {
            return await m_ntCore.m_rpcServer.PollRpcAsync(token);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PollRpc(bool, TimeSpan, out RpcCallInfo)"/>
        public bool PollRpc(bool blocking, TimeSpan timeout, out RpcCallInfo callInfo)
        {
            return m_ntCore.m_rpcServer.PollRpc(blocking, timeout, out callInfo);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PollRpc(bool, out RpcCallInfo)"/>
        public bool PollRpc(bool blocking, out RpcCallInfo callInfo)
        {
            return m_ntCore.m_rpcServer.PollRpc(blocking, out callInfo);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PostRpcResponse(long, long, IList{byte})"/>
        public void PostRpcResponse(long rpcId, long callUid, IList<byte> result)
        {
            m_ntCore.m_rpcServer.PostRpcResponse(rpcId, callUid, result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpc(string, IList{byte})"/>
        public long CallRpc(string name, IList<byte> param)
        {
            return m_ntCore.m_storage.CallRpc(name, param);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpc(string, IList{Value})"/>
        public long CallRpc(string name, IList<Value> param)
        {
            return m_ntCore.m_storage.CallRpc(name, PackRpcValues(param));
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResultAsync"/>
        public async Task<IList<byte>> GetRpcResultAsync(long callUid, CancellationToken token)
        {
            return await m_ntCore.m_storage.GetRpcResultAsync(callUid, token);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpcWithResultAsync(string, CancellationToken, IList{byte})"/>
        public async Task<IList<byte>> CallRpcWithResultAsync(string name, CancellationToken token, IList<byte> param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpcWithResultAsync(string, CancellationToken, IList{byte})"/>
        public async Task<IList<byte>> CallRpcWithResultAsync(string name, CancellationToken token, IList<Value> param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResult(bool, long, TimeSpan, out byte[])"/>
        public bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out byte[] result)
        {
            return m_ntCore.m_storage.GetRpcResult(blocking, callUid, timeout, out result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResult(bool, long, out byte[])"/>
        public bool GetRpcResult(bool blocking, long callUid, out byte[] result)
        {
            return m_ntCore.m_storage.GetRpcResult(blocking, callUid, out result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PackRpcDefinition(RpcDefinition)"/>
        public byte[] PackRpcDefinition(RpcDefinition def)
        {
            return RemoteProcedureCall.PackRpcDefinition(def);
        }

        /// <inheritdoc cref="RemoteProcedureCall.UnpackRpcDefinition(IList{byte}, ref RpcDefinition)"/>
        public bool UnpackRpcDefinition(IList<byte> packed, ref RpcDefinition def)
        {
            return RemoteProcedureCall.UnpackRpcDefinition(packed, ref def);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PackRpcValues(IList{Value})"/>
        public byte[] PackRpcValues(IList<Value> values)
        {
            return RemoteProcedureCall.PackRpcValues(values);
        }

        /// <inheritdoc cref="RemoteProcedureCall.UnpackRpcValues(IList{byte}, IList{NtType})"/>
        public List<Value> UnpackRpcValues(IList<byte> packed, IList<NtType> types)
        {
            return RemoteProcedureCall.UnpackRpcValues(packed, types);
        }
    }
}
