using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkTables
{
    internal class ListStream : Stream
    {
        private readonly IList<byte> m_buffer;

        private int m_position;

        public ListStream(IList<byte> list)
        {
            m_buffer = list ?? throw new ArgumentNullException(nameof(list), "List cannot be null");
            m_position = 0;
        }

        public override void Flush()
        {
            // Do nothing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer), "The buffer cannot be null");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "The offset cannot be less then 0");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "The count cannot be less then 0");
            if (buffer.Length - offset < count) throw new ArgumentException("Cannot read past the end of the buffer");
            // Implementation from .NET Core
            int num = m_buffer.Count - m_position;
            if (num > count) num = count;
            if (num <= 0) return 0;

            int byteCount = num;
            while (--byteCount >= 0)
                buffer[offset + byteCount] = m_buffer[m_position + byteCount];

            m_position += num;

            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => m_buffer.Count;

        public override long Position
        {
            get { return m_position; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be less then 0");
                if (value > m_buffer.Count)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"Value cannot be greater then {m_buffer.Count.ToString()}");
                m_position = (int)value;
            }
        }
    }
}
