using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    /// <summary>
    /// This class contains all info needed for a given connection.
    /// </summary>
    public struct ConnectionInfo
    {
        /// <summary>Gets the Remote Id of the Connection.</summary>
        public string RemoteId { get; }
        /// <summary>Gets the Remote Name of the Connection.</summary>
        public string RemoteName { get; }
        /// <summary>Gets the Remote Port of the Connection.</summary>
        public int RemotePort { get; }
        /// <summary>Gets the last update time of the Connection.</summary>
        public long LastUpdate { get; }
        /// <summary>Gets the Protocol Version of the Connection.</summary>
        public int ProtocolVersion { get; }

        internal ConnectionInfo(string rId, string rName, int rPort, long lastUpdate, int protocolVersion)
        {
            RemoteId = rId;
            RemoteName = rName;
            RemotePort = rPort;
            LastUpdate = lastUpdate;
            ProtocolVersion = protocolVersion;
        }
    }
}
