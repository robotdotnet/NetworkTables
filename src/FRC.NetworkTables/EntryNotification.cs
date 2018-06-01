using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct EntryNotification
    {
        public NT_EntryListener Listener { get; }
        public NT_Entry EntryHandle { get; }
        public string Name { get; }
        public readonly NetworkTableValue Value;
        public NotifyFlags Flags { get; }
        public NetworkTableEntry Entry => new NetworkTableEntry(m_instance, EntryHandle);
        private readonly NetworkTableInstance m_instance;

        internal unsafe EntryNotification(NetworkTableInstance inst, NT_EntryNotification* entry)
        {
            Listener = entry->listener;
            EntryHandle = entry->entry;
            Name = UTF8String.ReadUTF8String(entry->name.str, entry->name.len);
            Value = new NetworkTableValue(new NT_ManagedValue(&entry->value));
            Flags = (NotifyFlags)entry->flags;
            m_instance = inst;
        }
    }
}
