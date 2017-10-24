using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkTables
{
    [Flags]
    public enum NetMode : byte 
    {
        None = 0x00,
        Server = 0x01,
        Client = 0x02,
        Starting = 0x04,
        Failure = 0x08
    }
}
