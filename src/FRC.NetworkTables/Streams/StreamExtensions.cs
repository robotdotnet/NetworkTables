using NetworkTables.Logging;
using System.IO;
using System.Net.Sockets;
using static NetworkTables.Logging.Logger;

namespace NetworkTables.Streams
{
    internal static class StreamExtensions
    {
        public static int Send(this Stream stream, byte[] buffer, int pos, int len)
        {
            if (!stream.CanWrite)
            {
                return 0;
            }

            int errorCode = 0;
            while (true)
            {
                try
                {
                    stream.Write(buffer, pos, len);
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
                    if (sx.SocketErrorCode != SocketError.WouldBlock)//WSAEWOULDBLOCK 
                    {
                        errorCode = (int)sx.SocketErrorCode;
                        break;
                    }
                }
            }

            if (errorCode != 0)
            {
                string error = $"Send() failed: WSA error={errorCode.ToString()}\n";
                Debug4(Logger.Instance, error);
                return 0;
            }
            return len;
        }

        public static bool ReceiveByte(this Stream stream, out byte data)
        {
            data = 0;
            if (!stream.CanRead) return false;

            try
            {
                int ret = stream.ReadByte();
                if (ret < 0) return false;
                data = (byte)ret;
                return true;
            }
            catch (IOException ex)
            {
                SocketException sx = ex.InnerException as SocketException;
                if (sx == null)
                {
                    //Not socket exception is real error. Rethrow
                    throw;
                }
                //Return false on socket exception
                return false;
            }
        }

        public static int Receive(this Stream stream, byte[] buffer, int offset, int size)
        {
            if (!stream.CanRead) return 0;
            try
            {
                int pos = offset;

                while (pos < size + offset)
                {
                    int count = stream.Read(buffer, pos, size - pos);
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
