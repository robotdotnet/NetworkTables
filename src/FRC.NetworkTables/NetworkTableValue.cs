using FRC.NetworkTables.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct NetworkTableValue
    {
        public NtType Type => Value.Type;
        public readonly NT_ManagedValue Value;
        public bool IsValid => Type != NtType.Unassigned;

        internal NetworkTableValue(in NT_ManagedValue value)
        {
            this.Value = value;
        }


    }
}
