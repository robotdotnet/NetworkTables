using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables.Native;

namespace NetworkTables
{
    public static class NtCore
    {
        public static void StartClient(string ipAddress, int port)
        {
            CoreMethods.StartClient(ipAddress, (uint)port);
        }

        public static void StartServer(string persistFilename, string listenAddress, int port)
        {
            CoreMethods.StartServer(persistFilename, listenAddress, (uint)port);
        }

        public static void StopClient()
        {
            CoreMethods.StopClient();
        }

        public static void StopServer()
        {
            CoreMethods.StopServer();
        }

        public static void SetNetworkIdentity(string name)
        {
            CoreMethods.SetNetworkIdentity(name);
        }
    }
}
