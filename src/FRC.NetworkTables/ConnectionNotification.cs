using FRC.NetworkTables.Interop;

namespace FRC.NetworkTables
{
    public readonly struct ConnectionNotification
    {
        public NT_ConnectionListener Listener { get; }
        public bool Connected { get; }
        public readonly ConnectionInfo Conn;
        public NetworkTableInstance Instance { get; }

        internal unsafe ConnectionNotification(NetworkTableInstance inst, NT_ConnectionNotification* notification)
        {
            this.Listener = notification->listener;
            this.Connected = notification->connected.Get();
            this.Conn = new ConnectionInfo(&notification->conn);
            this.Instance = inst;
        }
    }
}
