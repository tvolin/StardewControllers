using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;

namespace RadialMenu.Config;

/// <summary>
/// Configuration data for the mod.
/// </summary>
public partial class ModConfig
{
    [Notify]
    private InputConfiguration input = new();

    [Notify]
    private MenuContentConfiguration content = new();

    [Notify]
    private Styles style = new();
}

public enum MenuOpenMode
{
    Hold,
    Toggle,
}

public partial class MenuContentConfiguration
{
    [Notify]
    private int inventoryPageSize = 12;

    [Notify]
    private bool showBlanksInInventory = false;

    [Notify]
    private ObservableCollection<MenuPageConfiguration> modPages = [];
}

public partial class MenuPageConfiguration
{
    [Notify]
    private ObservableCollection<CustomMenuItemConfiguration> items = [];

    [Notify]
    private Dictionary<SButton, QuickActionConfiguration> quickActions = [];
}

public partial class QuickActionConfiguration
{
    [Notify]
    private string itemId = "";

    [Notify]
    private string actionId = "";
}

public partial class ModIntegrationsConfiguration
{
    [Notify]
    private Dictionary<string, ModIntegrationConfiguration> mods = [];
}

public partial class ModIntegrationConfiguration
{
    [Notify]
    private bool enabled;
}

public partial class DebugConfiguration
{
    [Notify]
    private bool logGmcmKeyBindingsOnStartup;

    [Notify]
    private bool verboseLogging;
}
