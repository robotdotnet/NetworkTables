namespace NetworkTables
{
    /// <summary>
    /// This class contains all info needed for a given connection.
    /// </summary>
    public struct ConnectionInfo
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

        internal ConnectionInfo(string rId, string rIp, int rPort, long lastUpdate, int protocolVersion)
        {
            RemoteId = rId;
            RemoteIp = rIp;
            RemotePort = rPort;
            LastUpdate = lastUpdate;
            ProtocolVersion = protocolVersion;
        }
    }
}
