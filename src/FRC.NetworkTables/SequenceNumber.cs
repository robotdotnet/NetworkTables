namespace NetworkTables
{
    internal struct SequenceNumber
    {
        private bool Equals(SequenceNumber other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((SequenceNumber)obj);
        }

        public override int GetHashCode()
        {
            //No good hash code here, so just have to be careful
            return 0;
        }

        public SequenceNumber(uint value)
        {
            Value = value;
        }

        public SequenceNumber(SequenceNumber old)
        {
            Value = old.Value;
        }

        public uint Value
        {
            get; private set;
        }

        public static SequenceNumber operator ++(SequenceNumber input)
        {
            ++input.Value;
            if (input.Value > 0xffff) input.Value = 0;
            return input;
        }

        public static bool operator <(SequenceNumber lhs, SequenceNumber rhs)
        {
            if (lhs.Value < rhs.Value)
                return (rhs.Value - lhs.Value) < (1u << 15);
            else if (lhs.Value > rhs.Value)
                return (lhs.Value - rhs.Value) > (1u << 15);
            else
                return false;
        }

        public static bool operator >(SequenceNumber lhs, SequenceNumber rhs)
        {
            if (lhs.Value < rhs.Value)
                return (rhs.Value - lhs.Value) > (1u << 15);
            else if (lhs.Value > rhs.Value)
                return (lhs.Value - rhs.Value) < (1u << 15);
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
            return lhs.Value == rhs.Value;
        }

        public static bool operator !=(SequenceNumber lhs, SequenceNumber rhs)
        {
            return lhs.Value != rhs.Value;
        }

    }
}
