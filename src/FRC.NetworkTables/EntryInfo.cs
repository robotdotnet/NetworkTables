using FRC;
using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;

namespace FRC.NetworkTables
{
    /// <summary>
    /// This class contains all info for a given entry.
    /// </summary>
    public readonly struct EntryInfo
    {
        public NT_Entry EntryHandle { get; }
        /// <summary>Gets the Name of the entry.</summary>
        public string Name { get; }
        /// <summary>Gets the Type of the entry.</summary>
        public NtType Type { get; }
        /// <summary>Gets the Flags attached to the entry.</summary>
        public EntryFlags Flags { get; }
        /// <summary>Gets the last change time of the entry.</summary>
        public long LastChange { get; }
        /// <summary>Gets the entry object for this entry.</summary>
        public NetworkTableEntry Entry => new NetworkTableEntry(m_instance, EntryHandle);
        private readonly NetworkTableInstance m_instance;

        internal unsafe EntryInfo(NetworkTableInstance instance, NT_EntryInfo* entryInfo)
        {
            EntryHandle = entryInfo->entry;
            Type = (NtType)entryInfo->type;
            Flags = (EntryFlags)entryInfo->flags;
            LastChange = (long)entryInfo->last_change;
            Name = UTF8String.ReadUTF8String(entryInfo->name.str, entryInfo->name.len);
            m_instance = instance;
        }
    }
}
