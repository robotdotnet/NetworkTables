using System;

namespace NetworkTables
{
    /// <summary>
    /// The flags avalible for TableListeners
    /// </summary>
    [Flags]
    public enum NotifyFlags
    {
        /// <summary>
        ///  Notify nobody
        /// </summary>
        NotifyNone = 0x00,
        /// <summary>
        /// Initial listener addition
        /// </summary>
        NotifyImmediate = 0x01,
        /// <summary>
        /// Changed locally
        /// </summary>
        NotifyLocal = 0x02,
        /// <summary>
        /// Newly created entry
        /// </summary>
        NotifyNew = 0x04,
        /// <summary>
        /// Deleted entry
        /// </summary>
        NotifyDelete = 0x08,
        /// <summary>
        /// Value changed for entry
        /// </summary>
        NotifyUpdate = 0x10,
        /// <summary>
        /// Flags changed for entry
        /// </summary>
        NotifyFlagsChanged = 0x20
    };

    /// <summary>
    /// The flags avalible for Entries
    /// </summary>
    [Flags]
    public enum EntryFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Set entry to be persistent
        /// </summary>
        Persistent = 0x01
    }

    /// <summary>
    /// The log level to use for the NT logger
    /// </summary>
    public enum LogLevel
    {
        ///
        LogCritical = 50,
        ///
        LogError = 40,
        ///
        LogWarning = 30,
        ///
        LogInfo = 20,
        ///
        LogDebug = 10,
        ///
        LogDebug1 = 9,
        ///
        LogDebug2 = 8,
        ///
        LogDebug3 = 7,
        ///
        LogDebug4 = 6
    }

}
