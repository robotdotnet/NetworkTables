namespace NetworkTables
{
    /// <summary>
    /// This class contains all info for a given entry.
    /// </summary>
    public struct EntryInfo
    {
        /// <summary>Gets the Name of the entry.</summary>
        public string Name { get; }
        /// <summary>Gets the Type of the entry.</summary>
        public NtType Type { get; }
        /// <summary>Gets the Flags attached to the entry.</summary>
        public EntryFlags Flags { get; }
        /// <summary>Gets the last change time of the entry.</summary>
        public long LastChange { get; }

        internal EntryInfo(string name, NtType type, EntryFlags flags, long lastChange)
        {
            Name = name;
            Type = type;
            Flags = flags;
            LastChange = lastChange;
        }
    }
}
