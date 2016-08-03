using System;
using System.Threading.Tasks;

namespace NetworkTables.TcpSockets
{
    internal interface INetworkAcceptor: IDisposable
    {
        int Start();
        void Shutdown();
        Task<NtTcpClient> Accept();
    }
}
