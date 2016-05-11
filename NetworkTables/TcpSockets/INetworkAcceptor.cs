using System;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor: IDisposable
    {
        int Start();
        void Shutdown();
        IClient Accept();
    }
}
