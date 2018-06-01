using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct ServerPortPair
    {
        public string Server { get; }
        public int Port { get; }
        public ServerPortPair(string server, int port)
        {
            Server = server;
            Port = port;
        }
    }
}
