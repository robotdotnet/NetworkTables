using System;

namespace NetworkTables.Support
{
    internal static class Timestamp
    {
        public static long Now()
        {
            return DateTime.UtcNow.ToFileTimeUtc();
        }
    }
}
