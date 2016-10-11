using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkTables
{
    internal static class MessageTypeExtensions
    {
        internal static string GetString(this Message.MsgType msgType)
        {
            switch (msgType)
            {
                case Message.MsgType.KeepAlive:
                    return "KeepAlive";
                case Message.MsgType.ClientHello:
                    return "ClientHello";
                case Message.MsgType.ProtoUnsup:
                    return "ProtoUnsup";
                case Message.MsgType.ServerHelloDone:
                    return "ServerHelloDone";
                case Message.MsgType.ServerHello:
                    return "ServerHello";
                case Message.MsgType.ClientHelloDone:
                    return "ClientHelloDone";
                case Message.MsgType.EntryAssign:
                    return "EntryAssign";
                case Message.MsgType.EntryUpdate:
                    return "EntryUpdate";
                case Message.MsgType.FlagsUpdate:
                    return "FlagsUpdate";
                case Message.MsgType.EntryDelete:
                    return "EntryDelete";
                case Message.MsgType.ClearEntries:
                    return "ClearEntries";
                case Message.MsgType.ExecuteRpc:
                    return "ExecuteRpc";
                case Message.MsgType.RpcResponse:
                    return "RpcResponse";
                default:
                    return "Unknown";
            }
        }

        internal static string GetString(this NtType ntType)
        {
            switch (ntType)
            {
                case NtType.Boolean:
                    return "Boolean";
                case NtType.Double:
                    return "Double";
                case NtType.String:
                    return "String";
                case NtType.Raw:
                    return "Raw";
                case NtType.BooleanArray:
                    return "BooleanArray";
                case NtType.DoubleArray:
                    return "DoubleArray";
                case NtType.StringArray:
                    return "StringArray";
                case NtType.Rpc:
                    return "Rpc";
                default:
                    return "Unknown";
            }
        }
    }
}
