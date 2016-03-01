using System;

namespace NetworkTables
{
    /// <summary>
    /// An enumeration of all types allowed in the NetworkTables.
    /// </summary>
    [Flags]
    public enum NtType : uint
    {
        /// <summary>
        /// No type assigned
        /// </summary>
        Unassigned = 0,
        /// <summary>
        /// Boolean type
        /// </summary>
        Boolean = 0x01,
        /// <summary>
        /// Double type
        /// </summary>
        Double = 0x02,
        /// <summary>
        /// String type
        /// </summary>
        String = 0x04,
        /// <summary>
        /// Raw type
        /// </summary>
        Raw = 0x08,
        /// <summary>
        /// Boolean Array type
        /// </summary>
        BooleanArray = 0x10,
        /// <summary>
        /// Double Array type
        /// </summary>
        DoubleArray = 0x20,
        /// <summary>
        /// String Array type
        /// </summary>
        StringArray = 0x40,
        /// <summary>
        /// Rpc type
        /// </summary>
        Rpc = 0x80
    }
}
