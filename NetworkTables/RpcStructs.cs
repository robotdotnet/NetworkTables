using System.Collections.Generic;

namespace NetworkTables
{
    public struct RpcParamDef
    {
        public string Name { get; }
        public Value DefValue { get; }

        public RpcParamDef(string name, Value def)
        {
            Name = name;
            DefValue = def;
        }
    }

    public struct RpcResultsDef
    {
        public string Name { get; }
        public NtType Type { get; }


        public RpcResultsDef(string name, NtType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class RpcDefinition
    {
        public int Version { get; internal set; }
        public string Name { get; internal set; }

        public List<RpcParamDef> Params { get; set; }
        public List<RpcResultsDef> Results { get; set; }

        public RpcDefinition(int version, string name)
        {
            Version = version;
            Name = name;
            Params = new List<RpcParamDef>();
            Results = new List<RpcResultsDef>();
        }

        public RpcDefinition(int version, string name, List<RpcParamDef> param, List<RpcResultsDef> res)
        {
            Version = version;
            Name = name;
            Params = param;
            Results = res;
        }
    }

    public struct RpcCallInfo
    {
        public long RpcId { get; internal set; }
        public long CallUid { get; internal set; }
        public string Name { get; internal set; }
        public string Params { get; internal set; }

        public RpcCallInfo(long rpcId, long callUid, string name, string param)
        {
            RpcId = rpcId;
            CallUid = callUid;
            Name = name;
            Params = param;
        }
    }
}
