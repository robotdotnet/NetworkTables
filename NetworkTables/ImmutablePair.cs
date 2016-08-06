using System;
using System.Collections.Generic;

namespace NetworkTables
{
    internal struct ImmutablePair<T, T2> : IEquatable<ImmutablePair<T, T2>>
    {
        public bool Equals(ImmutablePair<T, T2> other)
        {
            return EqualityComparer<T>.Default.Equals(First, other.First) && EqualityComparer<T2>.Default.Equals(Second, other.Second);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ImmutablePair<T, T2> && Equals((ImmutablePair<T, T2>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(First) * 397) ^ EqualityComparer<T2>.Default.GetHashCode(Second);
            }
        }

        public static bool operator ==(ImmutablePair<T, T2> left, ImmutablePair<T, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ImmutablePair<T, T2> left, ImmutablePair<T, T2> right)
        {
            return !left.Equals(right);
        }

        public T First { get; }
        public T2 Second { get; }

        public ImmutablePair(T f, T2 s)
        {
            First = f;
            Second = s;
        }
    }
}
