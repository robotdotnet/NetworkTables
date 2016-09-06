using System;
using NetworkTables.Tables;

namespace NetworkTables
{
    /// <summary>
    /// This class is used to pass back an IRemote parameter to the global connection listener methods
    /// </summary>
    internal class StaticRemote : IRemote
    {
        public bool IsConnected
        {
            get
            {
                var conns = NtCore.GetConnections();
                return conns.Count > 0;
            }
        }

        public bool IsServer => !NetworkTable.Client;

        public void AddConnectionListener(Action<IRemote, ConnectionInfo, bool> listener, bool immediateNotify)
        {
            NetworkTable.AddGlobalConnectionListener(listener, immediateNotify);
        }

        public void AddConnectionListener(IRemoteConnectionListener listener, bool immediateNotify)
        {
            NetworkTable.AddGlobalConnectionListener(listener, immediateNotify);
        }

        public void RemoveConnectionListener(Action<IRemote, ConnectionInfo, bool> listener)
        {
            NetworkTable.RemoveGlobalConnectionListener(listener);
        }

        public void RemoveConnectionListener(IRemoteConnectionListener listener)
        {
            NetworkTable.RemoveGlobalConnectionListener(listener);
        }
    }
}