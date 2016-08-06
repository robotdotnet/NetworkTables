using System;
using NetworkTables.Tables;

namespace NetworkTables.Exceptions
{
    /// <summary>
    /// An exception thrown when the key has a different type then requested in the <see cref="ITable"/>
    /// </summary>
    public class TableKeyDifferentTypeException : InvalidOperationException
    {
        /// <summary>
        /// Gets the key that was requested
        /// </summary>
        public string RequestedKey { get; }
        /// <summary>
        /// Gets the type that was requested
        /// </summary>
        public NtType RequestedType { get; }
        /// <summary>
        /// Gets the type that actually exists in the table or value
        /// </summary>
        public NtType TypeInTable { get; }

        /// <summary>
        /// Gets if the exception was thrown during a <see cref="Value"/> Get() method.
        /// </summary>
        public bool ThrownByValueGet { get; }

        /// <summary>
        /// Creates a new <see cref="TableKeyDifferentTypeException"/>.
        /// </summary>
        /// <param name="key">The table key that was different.</param>
        /// <param name="requested">The type requested.</param>
        /// <param name="typeInTable">The type actually in the table.</param>
        public TableKeyDifferentTypeException(string key, NtType requested, NtType typeInTable)
            : base($"Key: {key}, Requested Type: {requested}, Type in Table: {typeInTable}")
        {
            RequestedKey = key;
            RequestedType = requested;
            TypeInTable = typeInTable;
            ThrownByValueGet = false;
        }

        /// <summary>
        /// Creates a new <see cref="TableKeyDifferentTypeException"/> during a <see cref="Value"/> error
        /// </summary>
        /// <param name="requested">The type requested.</param>
        /// <param name="typeInTable">The type actually in the value.</param>
        public TableKeyDifferentTypeException(NtType requested, NtType typeInTable)
            : base($"Requested Type {requested} does not match actual Type {typeInTable}.")
        {
            RequestedKey = "";
            RequestedType = requested;
            TypeInTable = typeInTable;
            ThrownByValueGet = true;
        }
    }
}
