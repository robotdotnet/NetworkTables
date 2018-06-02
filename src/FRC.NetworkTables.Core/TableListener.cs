using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public delegate void TableListener(NetworkTable parent, ReadOnlySpan<char> name, NetworkTable table);
}
