using StarControl.Config;

namespace StarControl;

/// <summary>
/// Categories for logging detailed messages. These mirror <see cref="DebugConfiguration"/> options
/// and are used to filter the corresponding <see cref="Logger"/> methods.
/// </summary>
internal enum LogCategory
{
    None,
    Input,
    Menus,
    QuickSlots,
    Activation,
    GmcmDetailed,
    GmcmSync,
}

/// <summary>
/// Provides static access to the mod's logging instance.
/// </summary>
internal static class Logger
{
    internal static DebugConfiguration Config { get; set; } = new();
    internal static IMonitor? Monitor { get; set; }

    /// <summary>
    /// Log a categorized message for the player or developer, if that category is enabled.
    /// </summary>
    /// <param name="category">The log category.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log severity level.</param>
    public static void Log(LogCategory category, string message, LogLevel level = LogLevel.Debug)
    {
        if (IsCategoryEnabled(category))
        {
            Monitor?.Log(message, level);
        }
    }

    /// <summary>
    /// Log a categorized message for the player or developer, if that category is enabled, but only
    /// if it hasn't already been logged since the last game launch.
    /// </summary>
    /// <param name="category">The log category.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log severity level.</param>
    public static void LogOnce(
        LogCategory category,
        string message,
        LogLevel level = LogLevel.Debug
    )
    {
        if (IsCategoryEnabled(category))
        {
            Monitor?.LogOnce(message, level);
        }
    }

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

    private static bool IsCategoryEnabled(LogCategory category)
    {
        return category switch
        {
            LogCategory.Activation => Config.EnableItemActivationLogging,
            LogCategory.GmcmDetailed => Config.EnableGmcmDetailedLogging,
            LogCategory.GmcmSync => Config.EnableGmcmSyncLogging,
            LogCategory.Input => Config.EnableInputLogging,
            LogCategory.Menus => Config.EnableMenuInteractionLogging,
            LogCategory.QuickSlots => Config.EnableQuickSlotLogging,
            _ => false,
        };
    }
}
