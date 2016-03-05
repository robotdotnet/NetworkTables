using System;
using NetworkTables.Streams;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor: IDisposable
    {
        int Start();
        void Shutdown();
        NtNetworkStream Accept();
    }
}
