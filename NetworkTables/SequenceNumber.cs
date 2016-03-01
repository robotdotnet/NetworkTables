using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    internal class SequenceNumber
    {
        protected bool Equals(SequenceNumber other)
        {
            return m_value == other.m_value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SequenceNumber) obj);
        }

        public override int GetHashCode()
        {
            //No good hash code here, so just have to be careful
            return 0;
        }

        uint m_value;
        public SequenceNumber()
        {
            m_value = 0;
        }

        public SequenceNumber(uint value)
        {
            m_value = value;
        }

        public SequenceNumber(SequenceNumber old)
        {
            m_value = old.m_value;
        }

        public uint Value() => m_value;

        public static SequenceNumber operator ++(SequenceNumber input)
        {
            ++input.m_value;
            if (input.m_value > 0xffff) input.m_value = 0;
            return input;
        }

        public static bool operator <(SequenceNumber lhs, SequenceNumber rhs)
        {
            if (lhs.m_value < rhs.m_value)
                return (rhs.m_value - lhs.m_value) < (1u << 15);
            else if (lhs.m_value > rhs.m_value)
                return (lhs.m_value - rhs.m_value) > (1u << 15);
            else
                return false;
        }

        public static bool operator >(SequenceNumber lhs, SequenceNumber rhs)
        {
            if (lhs.m_value < rhs.m_value)
                return (rhs.m_value - lhs.m_value) > (1u << 15);
            else if (lhs.m_value > rhs.m_value)
                return (lhs.m_value - rhs.m_value) < (1u << 15);
            else
                return false;
        }

        public static bool operator <=(SequenceNumber lhs, SequenceNumber rhs)
        {
            return lhs == rhs || lhs < rhs;
        }

        public static bool operator >=(SequenceNumber lhs, SequenceNumber rhs)
        {
            return lhs == rhs || lhs > rhs;
        }

        public static bool operator ==(SequenceNumber lhs, SequenceNumber rhs)
        {
            return lhs.m_value == rhs.m_value;
        }

        public static bool operator !=(SequenceNumber lhs, SequenceNumber rhs)
        {
            return lhs.m_value != rhs.m_value;
        }

    }
}
