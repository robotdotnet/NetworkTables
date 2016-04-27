using System;
using NetworkTables.Tables;

namespace NetworkTables.Native.Exceptions
{
    /// <summary>
    /// An exception thrown when the lookup of a key-value fails in an <see cref="ITable"/>.
    /// </summary>
    public class TableKeyNotDefinedException : InvalidOperationException
    {
        /// <summary>
        /// Creates a new <see cref="TableKeyNotDefinedException"/>.
        /// </summary>
        /// <param name="key">The key that was not defined in the table.</param>
        public TableKeyNotDefinedException(string key) : base($"Unknown Table Key: {key}")
        {
            
        }
    }
}
