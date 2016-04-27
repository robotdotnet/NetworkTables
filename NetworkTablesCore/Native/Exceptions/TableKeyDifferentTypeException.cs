using System;
using NetworkTables.Tables;

namespace NetworkTables.Native.Exceptions
{
    /// <summary>
    /// An exception thrown when the key has a different type then requested in the <see cref="ITable"/>
    /// </summary>
    public class TableKeyDifferentTypeException : InvalidOperationException
    {
        public string RequestedKey { get; }
        public NtType RequestedType { get; }
        public NtType TypeInTable { get; }

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
