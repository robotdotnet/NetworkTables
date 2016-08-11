using NetworkTables.Core.Native;

// ReSharper disable once CheckNamespace
namespace NetworkTables.Support
{
    internal static class Timestamp
    {
        public static long Now()
        {
            // Gets a consistent timestamp that we can use for checking if data is new.
            return CoreMethods.Now();
        }
    }
}
