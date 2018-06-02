namespace FRC.NetworkTables
{
    public delegate void TableEntryListener(NetworkTable table, string key, in NetworkTableEntry entry, in NetworkTableValue value, NotifyFlags flags);
}
