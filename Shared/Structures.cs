namespace NetworkTables
{
    /// <summary>
    /// Creates an Immutable pair as a struct
    /// </summary>
    /// <typeparam name="T">The first type of the pair</typeparam>
    /// <typeparam name="U">The second type of the pair</typeparam>
    public struct ImmutablePair<T,U>
    {
        public T First { get; }
        public U Second { get; }

        public ImmutablePair(T f, U s)
        {
            First = f;
            Second = s;
        }
    }

    public struct MutablePair<T, U>
    {
        public T First { get; private set; }
        public U Second { get; private set; }

        public void SetFirst(T first)
        {
            First = first;
        }

        public void SetSecond(U second)
        {
            Second = second;
        }
    }
}