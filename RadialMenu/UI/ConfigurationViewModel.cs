using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class ConfigurationViewModel
{
    public InputConfigurationViewModel Input { get; } = new();
    public ItemsConfigurationViewModel Items { get; } = new();
    public PagerViewModel<NavPageViewModel> Pager { get; } = new();
    public StyleConfigurationViewModel Style { get; } = new();

    [Notify]
    private ModConfig config;

    [Notify]
    private Vector2 contentPanelSize;

    public ConfigurationViewModel(ModConfig config, string modId)
    {
        this.config = config;
        Input.Load(config.Input);
        Style.Load(config.Style);
        Pager.Pages =
        [
            new(NavPage.Controls, I18n.Config_Tab_Controls_Title(), $"Mods/{modId}/Views/Controls"),
            new(NavPage.Style, I18n.Config_Tab_Style_Title(), $"Mods/{modId}/Views/Style"),
            new(NavPage.Actions, I18n.Config_Tab_Actions_Title(), $"Mods/{modId}/Views/Actions"),
            new(NavPage.Mods, I18n.Config_Tab_Mods_Title(), ""),
            new(NavPage.Debug, I18n.Config_Tab_Debug_Title(), ""),
        ];
    }

    public bool HandleButtonPress(SButton button)
    {
        return Pager.HandleButtonPress(button);
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
