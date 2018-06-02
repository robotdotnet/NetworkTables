namespace FRC.NetworkTables
{
    public delegate void RpcAnswerDelegate(in RpcAnswer answer);
    public delegate void EntryNotificationDelegate(in EntryNotification notification);
    public delegate void ConnectionNotificationDelegate(in ConnectionNotification notification);
    public delegate void LogMessageDelegate(in LogMessage log);
}
