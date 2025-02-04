namespace StarControl.Config;

/// <summary>
/// Debug/developer settings for the mod.
/// </summary>
public class DebugConfiguration : IConfigEquatable<DebugConfiguration>
{
    /// <summary>
    /// Enables in-depth logging for GMCM integration, including a dump of all registered keybind
    /// settings for all mods that can help with manual (.json) configuration.
    /// </summary>
    public bool EnableGmcmDetailedLogging { get; set; }

    /// <summary>
    /// Whether to log GMCM synchronization activity, i.e. when the keybind for a Mod Menu Item is
    /// updated to match a change in the GMCM settings for the target mod.
    /// </summary>
    public bool EnableGmcmSyncLogging { get; set; }

    /// <summary>
    /// Whether to log events related to low-level input handling for interacting with menus or
    /// quick slots.
    /// </summary>
    public bool EnableInputLogging { get; set; }

    /// <summary>
    /// Whether to log events related to item activation, i.e. actions taken and outcomes received.
    /// </summary>
    public bool EnableItemActivationLogging { get; set; }

    /// <summary>
    /// Whether to log interactions with the menu, including opening/closing and selecting items.
    /// </summary>
    public bool EnableMenuInteractionLogging { get; set; }

    /// <summary>
    /// Whether to log events related to quick slot usage.
    /// </summary>
    public bool EnableQuickSlotLogging { get; set; }

    /// <inheritdoc />
    public bool Equals(DebugConfiguration? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return EnableGmcmDetailedLogging == other.EnableGmcmDetailedLogging
            && EnableGmcmSyncLogging == other.EnableGmcmSyncLogging
            && EnableInputLogging == other.EnableInputLogging
            && EnableItemActivationLogging == other.EnableItemActivationLogging
            && EnableMenuInteractionLogging == other.EnableMenuInteractionLogging
            && EnableQuickSlotLogging == other.EnableQuickSlotLogging;
    }
}
