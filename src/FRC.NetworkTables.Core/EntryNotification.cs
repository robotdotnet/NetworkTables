using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct EntryNotification
    {
        public readonly EntryListener Listener;
        public readonly Entry EntryHandle;
        public readonly string Name;
        public readonly NetworkTableValue Value;
        public readonly NotifyFlags Flags;
        public NetworkTableEntry Entry => new NetworkTableEntry(m_instance, EntryHandle);
        private readonly NetworkTableInstance m_instance;

        internal unsafe EntryNotification(NetworkTableInstance inst, NtEntryNotification* entry)
        {
            Listener = entry->listener;
            EntryHandle = entry->entry;
            Name = UTF8String.ReadUTF8String(entry->name.str, entry->name.len);
            Value = new NetworkTableValue(new ManagedValue(&entry->value));
            Flags = (NotifyFlags)entry->flags;
            m_instance = inst;
        }
    }
}
