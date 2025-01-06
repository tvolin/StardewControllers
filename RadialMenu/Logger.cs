namespace RadialMenu;

/// <summary>
/// Provides static access to the mod's logging instance.
/// </summary>
internal static class Logger
{
    internal static IMonitor? Monitor;

    /// <inheritdoc cref="IMonitor.Log(string, LogLevel)"/>
    public static void Log(string message, LogLevel level = LogLevel.Trace)
    {
        Monitor?.Log(message, level);
    }

    /// <inheritdoc cref="IMonitor.LogOnce(string, LogLevel)"/>
    public static void LogOnce(string message, LogLevel level = LogLevel.Trace)
    {
        Monitor?.LogOnce(message, level);
    }
}
