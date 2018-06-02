using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public delegate void TableListener(NetworkTable parent, string name, NetworkTable table);
}
