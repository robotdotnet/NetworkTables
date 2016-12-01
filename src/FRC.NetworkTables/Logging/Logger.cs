using System;
using System.IO;
using System.Runtime.CompilerServices;
// ReSharper disable ExplicitCallerInfoArgument

namespace NetworkTables.Logging
{
    internal class Logger
    {
        private static Logger s_instance;

        public static Logger Instance => s_instance ?? (s_instance = new Logger());

        internal Logger()
        {
            m_func = DefLogFunc;
        }

        private LogFunc m_func;

        public LogLevel MinLevel { get; set; } = 0;

        public void SetLogger(LogFunc func)
        {
            m_func = func;
        }

        public void SetDefaultLogger()
        {
            m_func = DefLogFunc;
        }

        public void Log(LogLevel level, string file, int line, string msg)
        {
            if (m_func == null || level < MinLevel) return;
            m_func(level, file, line, msg);
        }

        public bool HasLogger()
        {
            return m_func != null;
        }

        private static void DefLogFunc(LogLevel level, string file, int line, string msg)
        {
            if (level == LogLevel.LogInfo)
            {
                Console.Error.WriteLine($"NT: {msg}");
            }

            string levelmsg;
            if (level >= LogLevel.LogCritical)
                levelmsg = "CRITICAL";
            else if (level >= LogLevel.LogError)
                levelmsg = "ERROR";
            else if (level >= LogLevel.LogWarning)
                levelmsg = "WARNING";
            else
                return;
            string fname = Path.GetFileName(file);
            Console.Error.WriteLine($"NT: {levelmsg}: {msg} ({fname}:{line.ToString()})");
        }


        // ReSharper disable once UnusedParameter.Global
        public static void Log(Logger logger, LogLevel level, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (logger == null) return;
            do
            {
                if (logger.MinLevel <= level && logger.HasLogger())
                {
                    logger.Log(level, filePath, lineNumber, msg);
                }
            }
            while (false);
        }

        public static void Error(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogError, msg, memberName, filePath, lineNumber);
        }

        public static void Warning(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogWarning, msg, memberName, filePath, lineNumber);
        }

        public static void Info(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogInfo, msg, memberName, filePath, lineNumber);
        }

        public static void Debug(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogDebug, msg, memberName, filePath, lineNumber);
        }

        public static void Debug1(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogDebug1, msg, memberName, filePath, lineNumber);
        }

        public static void Debug2(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogDebug2, msg, memberName, filePath, lineNumber);
        }

        public static void Debug3(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogDebug3, msg, memberName, filePath, lineNumber);
        }

        public static void Debug4(Logger logger, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(logger, LogLevel.LogDebug4, msg, memberName, filePath, lineNumber);
        }
    }

}
