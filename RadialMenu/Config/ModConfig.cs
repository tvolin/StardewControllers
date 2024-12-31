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

public partial class InputConfiguration
{
    [Notify]
    private SButton inventoryMenuButton = SButton.LeftTrigger;

    [Notify]
    private SButton modMenuButton = SButton.RightTrigger;

    [Notify]
    private SButton previousPageButton = SButton.LeftShoulder;

    [Notify]
    private SButton nextPageButton = SButton.RightShoulder;

    [Notify]
    private SButton primaryActionButton = SButton.A;

    [Notify]
    private SButton secondaryActionButton = SButton.X;

    [Notify]
    private ThumbStickPreference thumbStickPreference = ThumbStickPreference.AlwaysLeft;

    [Notify]
    private MenuOpenMode openMode = MenuOpenMode.Toggle;

    [Notify]
    private DelayedActions delayedActions = DelayedActions.ToolSwitch;

    [Notify]
    private int activationDelayMs = 250;

    [Notify]
    private bool rememberSelection;

    [Notify]
    private float triggerDeadZone = 0.2f;

    [Notify]
    private float thumbstickDeadZone = 0.2f;
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
