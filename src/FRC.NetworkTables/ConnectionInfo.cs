using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;

namespace FRC.NetworkTables
{
    /// <summary>
    /// This class contains all info needed for a given connection.
    /// </summary>
    public readonly struct ConnectionInfo
    {
        /// <summary>Gets the Remote Id of the Connection.</summary>
        public string RemoteId { get; }
        /// <summary>Gets the Remote IP Address of the Connection.</summary>
        public string RemoteIp { get; }
        /// <summary>Gets the Remote Port of the Connection.</summary>
        public int RemotePort { get; }
        /// <summary>Gets the last update time of the Connection.</summary>
        public long LastUpdate { get; }
        /// <summary>Gets the Protocol Version of the Connection.</summary>
        public int ProtocolVersion { get; }

        internal unsafe ConnectionInfo(NT_ConnectionInfo* info)
        {
            RemoteId = UTF8String.ReadUTF8String(info->remote_id);
            RemoteIp = UTF8String.ReadUTF8String(info->remote_ip);
            RemotePort = (int)info->remote_port;
            LastUpdate = (long)info->last_update;
            ProtocolVersion = (int)info->protocol_version;
        }
    }
}