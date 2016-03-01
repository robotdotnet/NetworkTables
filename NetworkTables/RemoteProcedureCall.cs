using System.Collections.Generic;
using System.IO;
using NetworkTables.Wire;

namespace NetworkTables
{
    public static class RemoteProcedureCall
    {
        public static void CreateRpc(string name, byte[] def, RpcCallback callback)
        {
            Storage.Instance.CreateRpc(name, def, callback);
        }

        public static void CreateRpc(string name, RpcDefinition def, RpcCallback callback)
        {
            Storage.Instance.CreateRpc(name, PackRpcDefinition(def), callback);
        }

        public static void CreatePolledRpc(string name, byte[] def)
        {
            Storage.Instance.CreatePolledRpc(name, def);
        }

        public static void CreatePolledRpc(string name, RpcDefinition def)
        {
            Storage.Instance.CreatePolledRpc(name, PackRpcDefinition(def));
        }

        public static bool PollRpc(bool blocking, ref RpcCallInfo callInfo)
        {
            return RpcServer.Instance.PollRpc(blocking, ref callInfo);
        }

        public static void PostRpcResponse(long rpcId, long callUid, params byte[] result)
        {
            RpcServer.Instance.PostRpcResponse(rpcId, callUid, result);
        }

        public static long CallRpc(string name, params byte[] param)
        {
            return Storage.Instance.CallRpc(name, param);
        }

        public static long CallRpc(string name, params Value[] param)
        {
            return Storage.Instance.CallRpc(name, PackRpcValues(param));
        }


        public static bool GetRpcResult(bool blocking, long callUid, ref byte[] result)
        {
            return Storage.Instance.GetRpcResult(blocking, callUid, ref result);
        }

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

        public static bool UnpackRpcDefinition(byte[] packed, ref RpcDefinition def)
        {
            MemoryStream iStream = new MemoryStream(packed);
            WireDecoder dec = new WireDecoder(iStream, 0x0300);
            byte ref8 = 0;
            ushort ref16 = 0;
            ulong ref32 = 0;
            string str = "";

            if (!dec.Read8(ref ref8)) return false;
            def.Version = ref8;
            if (!dec.ReadString(ref str)) return false;
            def.Name = str;

            int paramsSize = 0;
            if (!dec.Read8(ref ref8)) return false;
            paramsSize = ref8;
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

            int resultsSize = 0;
            if (!dec.Read8(ref ref8)) return false;
            resultsSize = ref8;
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

        public static byte[] PackRpcValues(params Value[] values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

        public static byte[] PackRpcValues(List<Value> values)
        {
            WireEncoder enc = new WireEncoder(0x0300);
            foreach (var value in values)
            {
                enc.WriteValue(value);
            }
            return enc.Buffer;
        }

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
