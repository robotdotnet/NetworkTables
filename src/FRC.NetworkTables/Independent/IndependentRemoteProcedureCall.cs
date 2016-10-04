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

        /// <inheritdoc cref="RemoteProcedureCall.CreateRpc(string, IReadOnlyList{byte}, RpcCallback)"/>
        public void CreateRpc(string name, IReadOnlyList<byte> def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, def, callback);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreateRpc(string, RpcDefinition, RpcCallback)"/>
        public void CreateRpc(string name, RpcDefinition def, RpcCallback callback)
        {
            m_ntCore.m_storage.CreateRpc(name, PackRpcDefinition(def), callback);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CreatePolledRpc(string, IReadOnlyList{byte})"/>
        public void CreatePolledRpc(string name, IReadOnlyList<byte> def)
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

        /// <inheritdoc cref="RemoteProcedureCall.PostRpcResponse(long, long, IReadOnlyList{byte})"/>
        public void PostRpcResponse(long rpcId, long callUid, IReadOnlyList<byte> result)
        {
            m_ntCore.m_rpcServer.PostRpcResponse(rpcId, callUid, result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpc(string, IReadOnlyList{byte})"/>
        public long CallRpc(string name, IReadOnlyList<byte> param)
        {
            return m_ntCore.m_storage.CallRpc(name, param);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpc(string, IReadOnlyList{Value})"/>
        public long CallRpc(string name, IReadOnlyList<Value> param)
        {
            return m_ntCore.m_storage.CallRpc(name, PackRpcValues(param));
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResultAsync"/>
        public async Task<IReadOnlyList<byte>> GetRpcResultAsync(long callUid, CancellationToken token)
        {
            return await m_ntCore.m_storage.GetRpcResultAsync(callUid, token);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpcWithResultAsync(string, CancellationToken, IReadOnlyList{byte})"/>
        public async Task<IReadOnlyList<byte>> CallRpcWithResultAsync(string name, CancellationToken token, IReadOnlyList<byte> param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="RemoteProcedureCall.CallRpcWithResultAsync(string, CancellationToken, IReadOnlyList{byte})"/>
        public async Task<IReadOnlyList<byte>> CallRpcWithResultAsync(string name, CancellationToken token, IReadOnlyList<Value> param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResult(bool, long, TimeSpan, out IReadOnlyList{byte})"/>
        public bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out IReadOnlyList<byte> result)
        {
            return m_ntCore.m_storage.GetRpcResult(blocking, callUid, timeout, out result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.GetRpcResult(bool, long, out IReadOnlyList{byte})"/>
        public bool GetRpcResult(bool blocking, long callUid, out IReadOnlyList<byte> result)
        {
            return m_ntCore.m_storage.GetRpcResult(blocking, callUid, out result);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PackRpcDefinition(RpcDefinition)"/>
        public IReadOnlyList<byte> PackRpcDefinition(RpcDefinition def)
        {
            return RemoteProcedureCall.PackRpcDefinition(def);
        }

        /// <inheritdoc cref="RemoteProcedureCall.UnpackRpcDefinition(IReadOnlyList{byte}, ref RpcDefinition)"/>
        public bool UnpackRpcDefinition(IReadOnlyList<byte> packed, ref RpcDefinition def)
        {
            return RemoteProcedureCall.UnpackRpcDefinition(packed, ref def);
        }

        /// <inheritdoc cref="RemoteProcedureCall.PackRpcValues(IReadOnlyList{Value})"/>
        public IReadOnlyList<byte> PackRpcValues(IReadOnlyList<Value> values)
        {
            return RemoteProcedureCall.PackRpcValues(values);
        }

        /// <inheritdoc cref="RemoteProcedureCall.UnpackRpcValues(IReadOnlyList{byte}, IReadOnlyList{NtType})"/>
        public IReadOnlyList<Value> UnpackRpcValues(IReadOnlyList<byte> packed, IReadOnlyList<NtType> types)
        {
            return RemoteProcedureCall.UnpackRpcValues(packed, types);
        }
    }
}
