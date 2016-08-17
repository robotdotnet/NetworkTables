using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables.Wire;
#if CORE
using NetworkTables.Core.Native;
using System.Runtime.InteropServices;
#endif

namespace NetworkTables
{
    /// <summary>
    /// NetworkTables client to server remote procedure calls
    /// </summary>
    public static class RemoteProcedureCall
    {
#if CORE
        // Never queried because it is only used to save for the GC
        // ReSharper disable once CollectionNeverQueried.Global
        internal static readonly List<Interop.NT_RPCCallback> s_rpcCallbacks = new List<Interop.NT_RPCCallback>();
#endif

        /// <summary>
        /// Creates an procedure that can be called by a remote client
        /// </summary>
        /// <remarks>
        /// Note that this can only be called by a server.
        /// </remarks>
        /// <param name="name">The name for this Rpc</param>
        /// <param name="def">The definition for this Rpc</param>
        /// <param name="callback">The callback to use the the procedure is called from a remote</param>
        public static void CreateRpc(string name, byte[] def, RpcCallback callback)
        {
#if CORE
            Interop.NT_RPCCallback modCallback =
                (IntPtr data, IntPtr ptr, UIntPtr len, IntPtr intPtr, UIntPtr paramsLen, out UIntPtr resultsLen) =>
                {
                    string retName = CoreMethods.ReadUTF8String(ptr, len);
                    byte[] param = CoreMethods.GetRawDataFromPtr(intPtr, paramsLen);
                    byte[] cb = callback(retName, param);
                    resultsLen = (UIntPtr)cb.Length;
                    IntPtr retPtr = Interop.NT_AllocateCharArray(resultsLen);
                    Marshal.Copy(cb, 0, retPtr, cb.Length);
                    return retPtr;
                };
            UIntPtr nameLen;
            IntPtr nameB = CoreMethods.CreateCachedUTF8String(name, out nameLen);
            Interop.NT_CreateRpc(nameB, nameLen, def, (UIntPtr)def.Length, IntPtr.Zero, modCallback);
            s_rpcCallbacks.Add(modCallback);
#else
            Storage.Instance.CreateRpc(name, def, callback);
#endif
        }

        /// <summary>
        /// Creates an procedure that can be called by a remote client
        /// </summary>
        /// <remarks>
        /// Note that this can only be called by a server.
        /// </remarks>
        /// <param name="name">The name for this Rpc</param>
        /// <param name="def">The definition for this Rpc</param>
        /// <param name="callback">The callback to use the the procedure is called from a remote</param>
        public static void CreateRpc(string name, RpcDefinition def, RpcCallback callback)
        {
#if CORE
            CreateRpc(name, PackRpcDefinition(def), callback);
#else
            Storage.Instance.CreateRpc(name, PackRpcDefinition(def), callback);
#endif
        }

        /// <summary>
        /// Creates an procedure called by a remote client that can be polled by the server
        /// </summary>
        /// <remarks>
        /// Note that this can only be called by a server.
        /// </remarks>
        /// <param name="name">The name for this Rpc</param>
        /// <param name="def">The definition for this Rpc</param>
        public static void CreatePolledRpc(string name, byte[] def)
        {
#if CORE
            UIntPtr nameLen;
            IntPtr nameB = CoreMethods.CreateCachedUTF8String(name, out nameLen);
            Interop.NT_CreatePolledRpc(nameB, nameLen, def, (UIntPtr)def.Length);
#else
            Storage.Instance.CreatePolledRpc(name, def);
#endif
        }

        /// <summary>
        /// Creates an procedure called by a remote client that can be polled by the server
        /// </summary>
        /// <remarks>
        /// Note that this can only be called by a server.
        /// </remarks>
        /// <param name="name">The name for this Rpc</param>
        /// <param name="def">The definition for this Rpc</param>
        public static void CreatePolledRpc(string name, RpcDefinition def)
        {
#if CORE
            CreatePolledRpc(name, PackRpcDefinition(def));
#else
            Storage.Instance.CreatePolledRpc(name, PackRpcDefinition(def));
#endif
        }

        /// <summary>
        /// Polls for an Rpc request from a client
        /// </summary>
        /// <param name="blocking">True to block waiting for a request</param>
        /// <param name="timeout">Timeout to wait for if blocking</param>
        /// <param name="callInfo">The info for the call request</param>
        /// <returns>True if an Rpc call has been requested, otherwise false</returns>
        public static bool PollRpc(bool blocking, TimeSpan timeout, out RpcCallInfo callInfo)
        {
#if CORE
            Func<RpcCallInfo?> func = () =>
            {
                RpcCallInfo info;
                bool success = PollRpc(true, out info);
                if (success) return info;
                return null;
            };

            var task = Task.Run(func);
            var completed = task.Wait(timeout);
            if (completed)
            {
                if (task.Result.HasValue)
                {
                    callInfo = task.Result.Value;
                    return true;
                }
            }
            callInfo = new RpcCallInfo();
            return false;
#else
            return RpcServer.Instance.PollRpc(blocking, timeout, out callInfo);
#endif
        }

        /// <summary>
        /// Polls for an Rpc request from a client asynchronously
        /// </summary>
        /// <param name="token">Token to cancel the polling on</param>
        /// <returns>The info for the call request, or null if canceled.</returns>
        public static async Task<RpcCallInfo?> PollRpcAsync(CancellationToken token)
        {
#if CORE
            try
            {
                Func<RpcCallInfo?> func = () =>
                {
                    RpcCallInfo info;
                    bool success = PollRpc(true, out info);
                    if (success) return info;
                    return null;
                };

                var result = await Task.Run(func, token);
                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
#else
            return await RpcServer.Instance.PollRpcAsync(token);
#endif
        }

        /// <summary>
        /// Polls for an Rpc request from a client
        /// </summary>
        /// <param name="blocking">True to block waiting for a request</param>
        /// <param name="callInfo">The info for the call request</param>
        /// <returns>True if an Rpc call has been requested, otherwise false</returns>
        public static bool PollRpc(bool blocking, out RpcCallInfo callInfo)
        {
#if CORE
            NtRpcCallInfo nativeInfo;
            int retVal = Interop.NT_PollRpc(blocking ? 1 : 0, out nativeInfo);
            if (retVal == 0) 
            {
                callInfo = new RpcCallInfo();
                return false;
            }
            callInfo = nativeInfo.ToManaged();
            return retVal != 0;
#else
            return RpcServer.Instance.PollRpc(blocking, out callInfo);
#endif
        }

        /// <summary>
        /// Posts an Rpc response to a polled Rpc request
        /// </summary>
        /// <param name="rpcId">The id of the rpc to respond to</param>
        /// <param name="callUid">The id of the request to respond to</param>
        /// <param name="result">The result to send as a response</param>
        public static void PostRpcResponse(long rpcId, long callUid, params byte[] result)
        {
#if CORE
            Interop.NT_PostRpcResponse((uint)rpcId, (uint)callUid, result, (UIntPtr)result.Length);
#else
            RpcServer.Instance.PostRpcResponse(rpcId, callUid, result);
#endif
        }

        /// <summary>
        /// Calls a procedure on a remote server
        /// </summary>
        /// <param name="name">The name of the Rpc</param>
        /// <param name="param">The data to send for the request</param>
        /// <returns>The Rpc call id</returns>
        public static long CallRpc(string name, params byte[] param)
        {
#if CORE
            UIntPtr size;
            IntPtr nameB = CoreMethods.CreateCachedUTF8String(name, out size);
            return Interop.NT_CallRpc(nameB, size, param, (UIntPtr)param.Length);
#else
            return Storage.Instance.CallRpc(name, param);
#endif
        }

        /// <summary>
        /// Calls a procedure on a remote server
        /// </summary>
        /// <param name="name">The name of the Rpc</param>
        /// <param name="param">The data to send for the request</param>
        /// <returns>The Rpc call id</returns>
        public static long CallRpc(string name, params Value[] param)
        {
#if CORE
            return CallRpc(name, PackRpcValues(param));
#else
            return Storage.Instance.CallRpc(name, PackRpcValues(param));
#endif
        }

        /// <summary>
        /// Calls a procedure on a remote server, and awaits a response asynchronously
        /// </summary>
        /// <param name="name">The name of the Rpc</param>
        /// <param name="token">The token to cancel the response request</param>
        /// <param name="param">The data to send for the request</param>
        /// <returns>The results received from the server for the request</returns>
        public static async Task<byte[]> CallRpcWithResultAsync(string name, CancellationToken token, params byte[] param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token);
        }

        /// <summary>
        /// Calls a procedure on a remote server, and awaits a response asynchronously
        /// </summary>
        /// <param name="name">The name of the Rpc</param>
        /// <param name="token">The token to cancel the response request</param>
        /// <param name="param">The data to send for the request</param>
        /// <returns>The results received from the server for the request</returns>
        public static async Task<byte[]> CallRpcWithResultAsync(string name, CancellationToken token, params Value[] param)
        {
            long id = CallRpc(name, param);
            return await GetRpcResultAsync(id, token);
        }

        /// <summary>
        /// Gets the result of an Rpc request asynchronously
        /// </summary>
        /// <param name="callUid">The Rpc call id</param>
        /// <param name="token">Token to cancel the request on</param>
        /// <returns>Array of results sent back from the server from the request</returns>
        public static async Task<byte[]> GetRpcResultAsync(long callUid, CancellationToken token)
        {
#if CORE
            try
            {
                var result = await Task.Run(() =>
                {
                    byte[] info;
                    bool success = GetRpcResult(true, callUid, out info);
                    if (success) return info;
                    return null;
                }, token);
                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
#else
            return await Storage.Instance.GetRpcResultAsync(callUid, token);
#endif
        }

        /// <summary>
        /// Gets the result of an Rpc request
        /// </summary>
        /// <param name="blocking">True if the call should block until the request is received</param>
        /// <param name="callUid">The Rpc call id</param>
        /// <param name="timeout">Timeout to wait for if blocking</param>
        /// <param name="result">Array of results sent back from the server from the request</param>
        /// <returns>True if a result was received, otherwise false</returns>
        public static bool GetRpcResult(bool blocking, long callUid, TimeSpan timeout, out byte[] result)
        {
#if CORE
            var task = Task.Run(() =>
            {
                byte[] info;
                bool success = GetRpcResult(true, callUid, out info);
                if (success) return info;
                return null;
            });
            var completed = task.Wait(timeout);
            if (completed)
            {
                result = task.Result;
                return true;
            }
            result = null;
            return false;
#else
            return Storage.Instance.GetRpcResult(blocking, callUid, timeout, out result);
#endif
        }

        /// <summary>
        /// Gets the result of an Rpc request
        /// </summary>
        /// <param name="blocking">True if the call should block until the request is received</param>
        /// <param name="callUid">The Rpc call id</param>
        /// <param name="result">Array of results sent back from the server from the request</param>
        /// <returns>True if a result was received, otherwise false</returns>
        public static bool GetRpcResult(bool blocking, long callUid, out byte[] result)
        {
#if CORE
            UIntPtr size = UIntPtr.Zero;
            IntPtr retVal = Interop.NT_GetRpcResult(blocking ? 1 : 0, (uint)callUid, ref size);
            if (retVal == IntPtr.Zero)
            {
                result = null;
                return false;
            }
            result = CoreMethods.GetRawDataFromPtr(retVal, size);
            return true;
#else
            return Storage.Instance.GetRpcResult(blocking, callUid, out result);
#endif
        }

        /// <summary>
        /// Pack an Rpc defintion in to a byte array
        /// </summary>
        /// <param name="def">The definition to pack</param>
        /// <returns>The packed data</returns>
        public static byte[] PackRpcDefinition(RpcDefinition def)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            enc.Write8((byte)def.Version);
            enc.WriteString(def.Name);

            int paramsSize = def.Params.Count;
            if (paramsSize > 0xff) paramsSize = 0xff;
            enc.Write8((byte)paramsSize);
            for (int i = 0; i < paramsSize; i++)
            {
                enc.WriteType(def.Params[i].DefValue.Type);
                enc.WriteString(def.Params[i].Name);
                enc.WriteValue(def.Params[i].DefValue);
            }

            int resultsSize = def.Results.Count;
            if (resultsSize > 0xff) resultsSize = 0xff;
            enc.Write8((byte)resultsSize);
            for (int i = 0; i < resultsSize; i++)
            {
                enc.WriteType(def.Results[i].Type);
                enc.WriteString(def.Results[i].Name);
            }
            return enc.Buffer;
        }

        /// <summary>
        /// Unpack an Rpc definition from a byte array
        /// </summary>
        /// <param name="packed">The data array</param>
        /// <param name="def">The definition to unpack to</param>
        /// <returns>True if the data was unpacked successfully</returns>
        public static bool UnpackRpcDefinition(byte[] packed, ref RpcDefinition def)
        {
            MemoryStream iStream = new MemoryStream(packed);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            byte ref8 = 0;
            string str = "";

            if (!dec.Read8(ref ref8)) return false;
            def.SetVersion(ref8);
            if (!dec.ReadString(ref str)) return false;
            def.SetName(str);

            if (!dec.Read8(ref ref8)) return false;
            int paramsSize = ref8;
            def.Params.Clear();
            NtType type = 0;
            for (int i = 0; i < paramsSize; i++)
            {

                if (!dec.ReadType(ref type)) return false;
                if (!dec.ReadString(ref str)) return false;
                var val = dec.ReadValue(type);
                if (val == null) return false;
                def.Params.Add(new RpcParamDef(str, val));
            }

            if (!dec.Read8(ref ref8)) return false;
            int resultsSize = ref8;
            def.Results.Clear();
            for (int i = 0; i < resultsSize; i++)
            {
                type = 0;
                if (!dec.ReadType(ref type)) return false;
                if (!dec.ReadString(ref str)) return false;
                def.Results.Add(new RpcResultsDef(str, type));
            }

            return true;
        }

        /// <summary>
        /// Pack a list of values
        /// </summary>
        /// <param name="values">The values to pack</param>
        /// <returns>The packed values</returns>
        public static byte[] PackRpcValues(params Value[] values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        /// <summary>
        /// Pack a list of values
        /// </summary>
        /// <param name="values">The values to pack</param>
        /// <returns>The packed values</returns>
        public static byte[] PackRpcValues(List<Value> values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        /// <summary>
        /// Unpack a list of values
        /// </summary>
        /// <param name="packed">The packed data</param>
        /// <param name="types">The types the packed data should be</param>
        /// <returns>A list of the unpacked values</returns>
        public static List<Value> UnpackRpcValues(byte[] packed, params NtType[] types)
        {
            MemoryStream iStream = new MemoryStream(packed);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            List<Value> vec = new List<Value>();
            foreach (var ntType in types)
            {
                var item = dec.ReadValue(ntType);
                if (item == null) return new List<Value>();
                vec.Add(item);
            }
            return vec;
        }

        /// <summary>
        /// Unpack a list of values
        /// </summary>
        /// <param name="packed">The packed data</param>
        /// <param name="types">The types the packed data should be</param>
        /// <returns>A list of the unpacked values</returns>
        public static List<Value> UnpackRpcValues(byte[] packed, List<NtType> types)
        {
            MemoryStream iStream = new MemoryStream(packed);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            List<Value> vec = new List<Value>();
            foreach (var ntType in types)
            {
                var item = dec.ReadValue(ntType);
                if (item == null) return new List<Value>();
                vec.Add(item);
            }
            return vec;
        }
    }
}
