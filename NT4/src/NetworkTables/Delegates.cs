using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkTables
{
    /// <summary>
    /// This delegate is use to specify the log function called back from the library
    /// </summary>
    /// <param name="level">The level of the current log</param>
    /// <param name="file">The file the log was called from</param>
    /// <param name="line">The line the log was called from</param>
    /// <param name="msg">The message of the log</param>
    public delegate void LogFunc(LogLevel level, string file, int line, string msg);
}
