using System;
using System.Net.Sockets;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor: IDisposable
    {
        int Start();
        void Shutdown();
        TcpClient Accept();
    }
}
