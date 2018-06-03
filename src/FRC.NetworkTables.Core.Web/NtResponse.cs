using System;

namespace FRC.NetworkTables.Core.Web
{
    public struct NtResponse
    {
        public string Key;
        public object Value;
        public EntryFlags Flags;
        public ulong LastChange;
    }
}
