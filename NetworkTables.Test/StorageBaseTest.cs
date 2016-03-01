using System.Collections.Generic;
using static NetworkTables.Storage;

namespace NetworkTables.Test
{
    public class StorageBaseTest
    {
        internal Entry tmpEntry;

        internal Storage storage;

        internal struct OutgoingData
        {
            public Message msg;
            public NetworkConnection only;
            public NetworkConnection except;

            public OutgoingData(Message m, NetworkConnection o, NetworkConnection e)
            {
                msg = m;
                only = o;
                except = e;
            }
        }
        internal bool m_server = false;

        internal List<OutgoingData> outgoing = new List<OutgoingData>();

        internal Dictionary<string, Storage.Entry> Entries => storage.Entries;
        internal List<Storage.Entry> IdMap => storage.IdMap;

        internal Storage.Entry GetEntry(string name)
        {
            Storage.Entry i = null;
            if (Entries.TryGetValue(name, out i))
            {
                return i;
            }
            return tmpEntry;
        }

        internal void HookOutgoing()
        {
            storage.SetOutgoing(QueueOutgoing, m_server);
        }

        internal void QueueOutgoing(Message msg, NetworkConnection only, NetworkConnection except)
        {
            outgoing.Add(new OutgoingData(msg, only, except));
        }

    }
}
