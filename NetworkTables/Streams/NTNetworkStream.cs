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

        public string PeerIP { get; }

        public int PeerPort { get; }

        public void SetNoDelay()
        {
            Socket.NoDelay = true;
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
                return base.Read(buffer, offset, size);
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

        /*
        public int Receive(byte[] buffer, int pos, int len, int timeout = 0)
        {
            if (Socket == null)
            {
                return 0;
            }

            int rv;

            if (timeout <= 0)
            {
                try
                {
                    rv = Read(buffer, pos, len);
                }
                catch (SocketException)
                {
                    rv = -1;
                }
            }
            else if (WaitForReadEvent(timeout))
            {
                try
                {
                    rv = Read(buffer, pos, len);
                }
                catch (SocketException)
                {
                    rv = -1;
                }
            }
            else
            {
                return 0;
            }

            if (rv < 0)
            {
                return 0;
            }
            return rv;
        }

        private bool WaitForReadEvent(int timeout)
        {
            ArrayList list = new ArrayList { Socket };
            Socket.Select(list, null, null, timeout * 1000000);
            if (list.Count == 0)
            {
                return false;
            }
            return true;
        */
    }
}
