using System;
using System.Collections.Generic;

namespace NetworkTables
{
    /// <summary>
    /// Definitions for an Rpc parameter
    /// </summary>
    public struct RpcParamDef
    {
        /// <summary>
        /// Gets the name for this parameter
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the value for this parameter
        /// </summary>
        public Value DefValue { get; }

        /// <summary>
        /// Creates a new Rpc parameter definition
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="def">The value of the parameter</param>
        public RpcParamDef(string name, Value def)
        {
            Name = name;
            DefValue = def;
        }
    }

    /// <summary>
    /// Defitions for an Rpc result
    /// </summary>
    public struct RpcResultsDef
    {
        /// <summary>
        /// Gets the name for this result
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type this result should be 
        /// </summary>
        public NtType Type { get; }

        /// <summary>
        /// Create a new Rpc result definition
        /// </summary>
        /// <param name="name">The name of the result</param>
        /// <param name="type">The type of the result</param>
        public RpcResultsDef(string name, NtType type)
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// Definitions for an Rpc call
    /// </summary>
    public struct RpcDefinition
    {
        /// <summary>
        /// The version of this Rpc definition
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// The name of this Rpc definition
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The parameters to send over this Rpc definition
        /// </summary>
        public List<RpcParamDef> Params { get; }

        /// <summary>
        /// The results to receive from this Rpc definition
        /// </summary>
        public List<RpcResultsDef> Results { get; }

        /// <summary>
        /// Creates a new Rpc defintion without parameters or results
        /// </summary>
        /// <param name="version">The version of this definition</param>
        /// <param name="name">The name of this definition</param>
        public RpcDefinition(int version, string name)
        {
            Version = version;
            Name = name;
            Params = new List<RpcParamDef>();
            Results = new List<RpcResultsDef>();
        }

        internal void SetName(string name)
        {
            Name = name;
        }

        internal void SetVersion(int version)
        {
            Version = version;
        }

        /// <summary>
        /// Creates a new Rpc definition with parameters and results
        /// </summary>
        /// <param name="version">The version of this definition</param>
        /// <param name="name">The name of this defition</param>
        /// <param name="param">The parameter definitions for this Rpc definition</param>
        /// <param name="res">The result definitions for this Rpc definition</param>
        public RpcDefinition(int version, string name, List<RpcParamDef> param, List<RpcResultsDef> res)
        {
            Version = version;
            Name = name;
            Params = param;
            Results = res;
        }
    }

    /// <summary>
    /// Call info related to a polled Rpc call
    /// </summary>
    public struct RpcCallInfo
    {
        /// <summary>
        /// The Id of the Rpc being polled
        /// </summary>
        public long RpcId { get; }
        /// <summary>
        /// The Id of the Rpc call that set this polled value
        /// </summary>
        public long CallUid { get; }
        /// <summary>
        /// The name of this call info
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The data from this rpc call
        /// </summary>
        public byte[] Params { get; }

        /// <summary>
        /// Creates a new polled RpcCallInfo def
        /// </summary>
        /// <param name="rpcId">The Rpc that is getting polled</param>
        /// <param name="callUid">The call Id that set this polled value</param>
        /// <param name="name">The name of this polled call info</param>
        /// <param name="param">The data associated with this polled info</param>
        public RpcCallInfo(long rpcId, long callUid, string name, byte[] param)
        {
            RpcId = rpcId;
            CallUid = callUid;
            Name = name;
            Params = new byte[param.Length];
            Array.Copy(param, Params, param.Length);
        }
    }
}
