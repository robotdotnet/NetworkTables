using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.Native
{
    internal class RawMemoryStream : IInputStream
    {
        private byte[] m_data;
        private int m_cur;
        private int m_left;

        public RawMemoryStream(byte[] mem, int len)
        {
            m_data = mem;
            m_cur = 0;
            m_left = len;
        }

        public virtual bool Read(byte[] data, int len)
        {
            if (len > m_left) return false;
            Array.Copy(m_data, m_cur, data, 0, len);
            m_cur += len;
            m_left -= len;
            return true;
        }

        public virtual void Close()
        {

        }
    }
}
