namespace RadialMenu.Config;

/// <summary>
/// Top-level configuration data for Star Control.
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Configures the input settings, e.g. which buttons are used to open and navigate the menus.
    /// </summary>
    public InputConfiguration Input { get; set; } = new();

    /// <summary>
    /// Configures the appearance of the Inventory and Mod Wheels/Menus.
    /// </summary>
    public Styles Style { get; set; } = new();

    /// <summary>
    /// Configures which pages and items will appear in each menu and in the quick slots.
    /// </summary>
    public ItemsConfiguration Items { get; set; } = new();

    /// <summary>
    /// Settings for third-party mod integrations.
    /// </summary>
    public ModIntegrationsConfiguration Integrations { get; set; } = new();

    /// <summary>
    /// Debug settings, for development and troubleshooting.
    /// </summary>
    public DebugConfiguration Debug { get; set; } = new();
}
