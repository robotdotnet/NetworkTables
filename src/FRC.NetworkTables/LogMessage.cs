using FRC.NetworkTables.Interop;
using FRC.NetworkTables.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables
{
    public readonly struct LogMessage
    {
        public NT_Logger Logger { get; }
        public LogLevel Level { get; }
        public string Filename { get; }
        public int Line { get; }
        public string Message { get; }
        internal NetworkTableInstance Instance { get; }

        internal unsafe LogMessage(NetworkTableInstance inst, NT_LogMessage* log)
        {
            Instance = inst;
            Logger = log->logger;
            Level = (LogLevel)log->level;
            Filename = UTF8String.ReadUTF8String(log->filename);
            Line = (int)log->line;
            Message = UTF8String.ReadUTF8String(log->message);
        }
    }
}
