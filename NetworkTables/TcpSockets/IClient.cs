using System.IO;
using System.Net;

namespace NetworkTables.TcpSockets
{
    interface IClient
    {
        Stream GetStream();
        EndPoint RemoteEndPoint { get; }
        bool NoDelay { set; }
    }
}
