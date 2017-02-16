using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetworkTables.Interfaces
{
    internal interface IServerOverridable
    {
        void SetServerOverride(IPAddress address, int port);
        void ClearServerOverride();
    }
}
