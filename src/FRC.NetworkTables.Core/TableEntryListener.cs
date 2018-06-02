namespace FRC.NetworkTables
{
    public delegate void TableEntryListener(NetworkTable table, string key, in NetworkTableEntry entry, in NetworkTableRefValue value, NotifyFlags flags);
}
