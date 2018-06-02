using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly ref struct EntryNotification
    {
        public readonly NtEntryListener Listener;
        public readonly NtEntry EntryHandle;
        public readonly string Name;
        public readonly NetworkTableRefValue Value;
        public readonly NotifyFlags Flags;
        public NetworkTableEntry Entry => new NetworkTableEntry(m_instance, EntryHandle);
        private readonly NetworkTableInstance m_instance;

        internal unsafe EntryNotification(NetworkTableInstance inst, in NtEntryNotification entry)
        {
            Listener = entry.listener;
            EntryHandle = entry.entry;
            Name = UTF8String.ReadUTF8String(entry.name.str, entry.name.len);
            Value = new NetworkTableRefValue(new RefManagedValue(entry.value));
            Flags = (NotifyFlags)entry.flags;
            m_instance = inst;
        }
    }
}
