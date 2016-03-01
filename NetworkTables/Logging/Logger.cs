using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace NetworkTables.Logging
{
    internal class Logger
    {
        private static Logger s_instance;

        public static Logger Instance => s_instance ?? (s_instance = new Logger());

        private Logger()
        {
            m_func = DefLogFunc;
        }

        private LogFunc m_func;
        public void SetLogger(LogFunc func)
        {
            m_func = func;
        }

        public void SetDefaultLogger()
        {
            m_func = DefLogFunc;
        }

        public void SetMinLevel(LogLevel level)
        {
            m_minLevel = level;
        }

        public LogLevel MinLevel()
        {
            return m_minLevel;
        }

        public void Log(LogLevel level, string file, int line, string msg)
        {
            if (m_func == null || level < m_minLevel) return;
            m_func(level, file, line, msg);
        }

        private LogLevel m_minLevel = 0;

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

            string levelmsg = "";
            if (level >= LogLevel.LogCritical)
                levelmsg = "CRITICAL";
            else if (level >= LogLevel.LogError)
                levelmsg = "ERROR";
            else if (level >= LogLevel.LogWarning)
                levelmsg = "WARNING";
            else
                return;
            string fname = Path.GetFileName(file);
            Console.Error.WriteLine($"NT: {levelmsg}: {msg} ({fname}:{line})");
        }


        public static void Log(LogLevel level, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            do
            {
                Logger logger = Logger.Instance;
                if (logger.MinLevel() <= level && logger.HasLogger())
                {
                    logger.Log(level, filePath, lineNumber, msg);
                }
            }
            while (false);
        }

        public static void Error(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogError, msg, memberName, filePath, lineNumber);
        }

        public static void Warning(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogWarning, msg, memberName, filePath, lineNumber);
        }

        public static void Info(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogInfo, msg, memberName, filePath, lineNumber);
        }

        public static void Debug(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogDebug, msg, memberName, filePath, lineNumber);
        }

        public static void Debug1(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogDebug1, msg, memberName, filePath, lineNumber);
        }

        public static void Debug2(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogDebug2, msg, memberName, filePath, lineNumber);
        }

        public static void Debug3(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogDebug3, msg, memberName, filePath, lineNumber);
        }

        public static void Debug4(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log(LogLevel.LogDebug4, msg, memberName, filePath, lineNumber);
        }
    }

}
