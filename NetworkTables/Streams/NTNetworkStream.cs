using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.Streams
{
    internal class NtNetworkStream : NetworkStream
    {
        public string PeerIP { get; }

        public int PeerPort { get; }

        // Allow turning off Nagle algorithm
        public bool NoDelay
        {
            get { return Socket.NoDelay; }
            set { Socket.NoDelay = value; }
        }

        public NtNetworkStream(Socket socket) : base(socket, true)
        {
            IPEndPoint ipEp = socket.RemoteEndPoint as IPEndPoint;
            if (ipEp != null)
            {
                PeerIP = ipEp.Address.ToString();
                PeerPort = ipEp.Port;
            }
            else
            {
                PeerIP = "";
                PeerPort = 0;
            }
        }

        public int Send(byte[] buffer, int pos, int len)
        {
            if (Socket == null || !CanWrite)
            {
                return 0;
            }

            int errorCode = 0;
            while (true)
            {
                try
                {
                    Write(buffer, pos, len);
                    break;
                }
                catch (IOException ex)
                {
                    SocketException sx = ex.InnerException as SocketException;
                    if (sx == null)
                    {
                        //Not socket exception is real error. Rethrow
                        throw;
                    }
                    if (sx.NativeErrorCode != 10035)//WSAEWOULDBLOCK 
                    {
                        errorCode = sx.NativeErrorCode;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }

            if (errorCode != 0)
            {
                string error = $"Send() failed: WSA error={errorCode}\n";
                Debug4(error);
                return 0;
            }
            return len;
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (!CanRead) return 0;
            try
            {
                int pos = offset;

                while (pos < size + offset)
                {
                    int count = base.Read(buffer, pos, size - pos);
                    if (count == 0) return 0;
                    pos += count;
                }

                return size;
            }
            catch (IOException ex)
            {
                SocketException sx = ex.InnerException as SocketException;
                if (sx == null)
                {
                    //Not socket exception is real error. Rethrow
                    throw;
                }
                //Return 0 on socket exception
                return 0;
            }
            
        }
    }
}
