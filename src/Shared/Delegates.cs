namespace NetworkTables
{
    /// <summary>
    /// This delegate is used when creating callbacks to be used for EntryListeners
    /// </summary>
    /// <param name="uid">The uid of the callback</param>
    /// <param name="name">The name of the entry being called back</param>
    /// <param name="value">The value of the entry being called back</param>
    /// <param name="flags">The flags of the entry bing called back</param>
    public delegate void EntryListenerCallback(int uid, string name, Value value, NotifyFlags flags);

    /// <summary>
    /// This delegate is used when creating callbacks to be used for Connection Listeners
    /// </summary>
    /// <param name="uid">The uid of the callback</param>
    /// <param name="connected">True if this is an initial connection, false if the connection disconnected</param>
    /// <param name="conn">The information for the connection</param>
    public delegate void ConnectionListenerCallback(int uid, bool connected, ConnectionInfo conn);

    /// <summary>
    /// This delegate is use to specify the log function called back from the library
    /// </summary>
    /// <param name="level">The level of the current log</param>
    /// <param name="file">The file the log was called from</param>
    /// <param name="line">The line the log was called from</param>
    /// <param name="msg">The message of the log</param>
    public delegate void LogFunc(LogLevel level, string file, int line, string msg);

    /// <summary>
    /// This delegate is used for Remote Procedure Call callbacks
    /// </summary>
    /// <param name="name">The name of the callback</param>
    /// <param name="param">The binary data of the callback</param>
    /// <returns>The raw rpc data to send in response</returns>
    public delegate byte[] RpcCallback(string name, byte[] param);
}
