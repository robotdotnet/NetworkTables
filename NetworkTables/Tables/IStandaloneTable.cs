using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Tables
{
    public interface IStandaloneTable : IDisposable
    {
        string RemoteName { get; set; }
        bool Client { get; }
        bool Running { get; }

        void StartClient(string ipAddress, int port);
        void StartClient(string[] ipAddresses, int port);
        void StartServer(string listenAddress, string persistentFileName, int port);

        void StopClient();
        void StopServer();
        void StopRpcServer();
        void StopNotifier();

        void Flush();
        void DeleteAllEntries();
    }
}
