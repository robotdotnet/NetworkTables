using System;
using System.IO;
using System.Net;

namespace NetworkTables.TcpSockets
{
    interface IClient : IDisposable
    {
        Stream GetStream();
        EndPoint RemoteEndPoint { get; }
        bool NoDelay { set; }
    }
}
