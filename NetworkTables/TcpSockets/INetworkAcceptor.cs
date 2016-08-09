using System;
using System.Net.Sockets;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor
    {
        int Start();
        void Shutdown();
        TcpClient Accept();
    }
}
