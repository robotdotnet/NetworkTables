using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkTables.Internal
{
    public class NetworkConnection : IDisposable
    {
        public int ProtoRev { get; private set; }

        public enum State { Created, Init, Handshake, Synchronized, Active, Dead };

        public delegate bool HandshakeFunc(NetworkConnection conn, Func<Message> getMsg, Action<List<Message>> sendMsgs);

        public delegate void ProcessIncomingFunc(Message msg, NetworkConnection conn);
    }
}
