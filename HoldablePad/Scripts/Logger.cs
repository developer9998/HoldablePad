using BepInEx.Logging;

namespace HoldablePad.Scripts
{
    public static class Logger
    {
        public static ManualLogSource manualLogSource;

        public static void Log(LogLevel logLevel, string logMessage)
            => manualLogSource.Log(logLevel, logMessage);

        public static void Log(string logMessage)
            => manualLogSource.Log(LogLevel.Info, logMessage);

        public static void LogWarning(string logMessage)
            => manualLogSource.Log(LogLevel.Warning, logMessage);

        public static void LogError(string logMessage)
            => manualLogSource.Log(LogLevel.Error, logMessage);
    }
}
