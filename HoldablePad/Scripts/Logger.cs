using BepInEx.Logging;
using System;
using System.IO;

namespace HoldablePad.Scripts
{
    public static class Logger
    {
        public static ManualLogSource manualLogSource;
        private static string LogPath => BepInEx.Paths.BepInExRootPath + "/HPLog.txt";
        private static bool LoggedMessage;

        public static void Log(LogLevel logLevel, string logMessage)
        {
            manualLogSource.Log(logLevel, logMessage);
            Write(logMessage);
        }

        public static void Log(string logMessage)
        {
            manualLogSource.Log(LogLevel.Info, logMessage);
            Write(logMessage);
        }

        public static void LogWarning(string logMessage)
        {
            manualLogSource.Log(LogLevel.Warning, "* " + logMessage);
            Write(logMessage);
        }

        public static void LogError(string logMessage)
        {
            manualLogSource.Log(LogLevel.Error, "** " + logMessage);
            Write(logMessage);
        }

        public static void Write(string logMessage)
        {
            if (!File.Exists(LogPath)) File.Create(LogPath).Close();

            if (!LoggedMessage)
            {
                LoggedMessage = true;
                using (StreamWriter streamWriter = File.AppendText(LogPath))
                    streamWriter.WriteLine("> HoldablePad logs for " + DateTime.Now.ToString("g") + "\n");
            }

            using (StreamWriter streamWriter = File.AppendText(LogPath))
                streamWriter.WriteLine(logMessage);
        }
    }
}
