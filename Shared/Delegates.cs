using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public delegate void EntryListenerCallback(int uid, string name, Value value, NotifyFlags flags);

    public delegate void ConnectionListenerCallback(int uid, bool connected, ConnectionInfo conn);

    public delegate void LogFunc(LogLevel level, string file, int line, string msg);

    public delegate byte[] RpcCallback(string name, byte[] param);
}
