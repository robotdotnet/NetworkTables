namespace NetworkTables {
    /// <summary>
    /// A NetworkTables IP Address
    /// </summary>
    /// <remarks>Used for Round Robin connections</remarks>
    public class NtIPAddress
    {
        /// <summary>
        /// The IP Address
        /// </summary>
        public string IpAddress { get; }
        /// <summary>
        /// The Port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Creates a new NetworkTable IP Address
        /// </summary>
        /// <param name="ip">The IP Address</param>
        /// <param name="port">The Port</param>
        public NtIPAddress(string ip, int port)
        {
            IpAddress = ip;
            Port = port;
        }
    }
}