using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class ConfigurationViewModel
{
    public IMenuController? Controller { get; set; }
    public InputConfigurationViewModel Input { get; } = new();
    public bool IsNavigationDisabled => !IsNavigationEnabled;
    public ItemsConfigurationViewModel Items { get; } = new();
    public ModIntegrationsViewModel Mods { get; } = new();
    public PagerViewModel<NavPageViewModel> Pager { get; } = new();
    public StyleConfigurationViewModel Style { get; } = new();

    [Notify]
    private ModConfig config;

    [Notify]
    private Vector2 contentPanelSize;

    [Notify]
    private bool isNavigationEnabled = true;

    public ConfigurationViewModel(ModConfig config, string modId)
    {
        this.config = config;
        Input.Load(config.Input);
        Style.Load(config.Style);
        Mods.Priorities =
        [
            ModPriorityViewModel.Self(modId, Items),
            new("furyx639.ToolbarIcons")
            {
                Name = "Iconic Framework",
                Description = "Adds shortcut icons to vanilla and mod functions.",
            },
            new("foo.Test1") { Name = "Test Mod 1", Description = "Test Description 1" },
            new("foo.Test2") { Name = "Test Mod 2", Description = "Test Description 2" },
            new("foo.Test3") { Name = "Test Mod 3", Description = "Test Description 3" },
        ];
        Pager.Pages =
        [
            new(NavPage.Controls, I18n.Config_Tab_Controls_Title(), $"Mods/{modId}/Views/Controls"),
            new(NavPage.Style, I18n.Config_Tab_Style_Title(), $"Mods/{modId}/Views/Style"),
            new(NavPage.Actions, I18n.Config_Tab_Actions_Title(), $"Mods/{modId}/Views/Actions"),
            new(NavPage.Mods, I18n.Config_Tab_Mods_Title(), $"Mods/{modId}/Views/ModIntegrations"),
            new(NavPage.Debug, I18n.Config_Tab_Debug_Title(), ""),
        ];
        Items.PropertyChanged += Items_PropertyChanged;
    }

    public bool CancelBlockingAction()
    {
        if (Items.IsReordering)
        {
            return Items.EndReordering();
        }
        return false;
    }

    public bool HandleButtonPress(SButton button)
    {
        if (!IsNavigationEnabled && IsCancelButton(button))
        {
            return CancelBlockingAction();
        }
        return Pager.HandleButtonPress(button);
    }

    private static bool IsCancelButton(SButton button)
    {
        return button is SButton.ControllerB or SButton.ControllerBack
            || (
                button.TryGetStardewInput(out var inputButton)
                && Game1.options.menuButton.Contains(inputButton)
            );
    }

    private void Items_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ItemsConfigurationViewModel.GrabbedItem))
        {
            IsNavigationEnabled = Items.GrabbedItem is null;
            if (Items.GrabbedItem is { } item)
            {
                Controller?.SetCursorAttachment(
                    item.Icon.Texture,
                    item.Icon.SourceRect,
                    new(64 * item.Icon.SourceRect.Width / item.Icon.SourceRect.Height, 64)
                );
            }
            else
            {
                Controller?.ClearCursorAttachment();
            }
        }
    }

    private void OnContentPanelSizeChanged()
    {
        // Explicitly assigning it here, as opposed to passing in a selector to the constructor, guarantees that the
        // property change event will fire for the dependent models as well.
        Pager.ContentPanelSize = contentPanelSize;
        Items.Pager.ContentPanelSize = ContentPanelSize;
    }
}

internal enum NavPage
{
    Controls,
    Style,
    Actions,
    Mods,
    Debug,
}

internal partial class NavPageViewModel(NavPage id, string title, string pageAssetName)
    : PageViewModel((int)id)
{
    public NavPage Id { get; } = id;
    public string PageAssetName { get; } = pageAssetName;
    public string Title { get; } = title;
}
