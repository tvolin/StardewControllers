using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class ConfigurationViewModel
{
    public Func<float, string> FormatActivationDelay { get; } = v => $"{v:f0} ms";
    public Func<float, string> FormatDeadZone { get; } = v => v.ToString("f2");

    public InputConfigurationViewModel Input { get; }

    public List<NavPageViewModel> Pages { get; }

    public Transform TabSelectionTransform => GetTabSelectionTransform(SelectedPageIndex);

    [Notify]
    private ModConfig config;

    [Notify]
    private Vector2 contentPanelSize;

    [Notify]
    private int selectedPageIndex;

    public ConfigurationViewModel(ModConfig config, string modId)
    {
        this.config = config;
        Input = new();
        Input.Load(config.Input);
        Pages =
        [
            new(NavPage.Controls, I18n.Config_Tab_Controls_Title(), $"Mods/{modId}/Views/Controls"),
            new(NavPage.Style, I18n.Config_Tab_Style_Title(), $"Mods/{modId}/Views/Style"),
            new(NavPage.Actions, I18n.Config_Tab_Actions_Title(), $"Mods/{modId}/Views/Actions"),
            new(NavPage.Mods, I18n.Config_Tab_Mods_Title(), ""),
            new(NavPage.Debug, I18n.Config_Tab_Debug_Title(), ""),
        ];
        Pages[0].Selected = true;
        foreach (var page in Pages)
        {
            page.PropertyChanged += Page_PropertyChanged;
        }
        UpdatePageTransforms();
    }

    public void HandleButtonPress(SButton button)
    {
        switch (button)
        {
            case SButton.LeftTrigger:
                SetPageIndex((SelectedPageIndex + Pages.Count - 1) % Pages.Count);
                break;
            case SButton.RightTrigger:
                SetPageIndex((SelectedPageIndex + 1) % Pages.Count);
                break;
        }
    }

    public void SetPage(NavPage id)
    {
        var index = Pages.FindIndex(x => x.Id == id);
        SetPageIndex(index);
    }

    private Transform GetTabSelectionTransform(int pageIndex)
    {
        float translateX = 0;
        for (int i = 0; i < pageIndex; i++)
        {
            translateX += Pages[i].TabSize.X;
        }
        var selectedPage = Pages[pageIndex];
        var scale = new Vector2(selectedPage.TabSize.X, selectedPage.TabSize.Y);
        var translation = new Vector2(translateX, 0);
        return new(translation, scale);
    }

    private void OnContentPanelSizeChanged()
    {
        UpdatePageTransforms();
    }

    private void OnSelectedPageIndexChanged()
    {
        UpdatePageTransforms();
    }

    private void Page_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NavPageViewModel.TabSize))
        {
            OnPropertyChanged(new(nameof(TabSelectionTransform)));
        }
    }

    private void SetPageIndex(int index)
    {
        if (index < 0 || index == SelectedPageIndex)
        {
            return;
        }
        Game1.playSound("smallSelect");
        Pages[SelectedPageIndex].Selected = false;
        SelectedPageIndex = index;
        Pages[index].Selected = true;
    }

    private void UpdatePageTransforms()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];
            if (ContentPanelSize != Vector2.Zero)
            {
                page.PageTransform = new(new(ContentPanelSize.X * (i - SelectedPageIndex), 0));
                // Setting visible only when the page - or a subsequent page - is actually selected
                // prevents awkward transition animations from playing when the menu is first shown.
                if (i <= SelectedPageIndex)
                {
                    page.Visible = true;
                }
            }
        }
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
{
    public NavPage Id { get; } = id;
    public string PageAssetName { get; } = pageAssetName;
    public string Title { get; } = title;

    [Notify]
    private Transform pageTransform = new(Vector2.Zero);

    [Notify]
    private bool selected;

    [Notify]
    private Vector2 tabSize;

    [Notify]
    private bool visible = false;
}

internal record Transform(Vector2 Translation, Vector2 Scale)
{
    public Transform(Vector2 translation)
        : this(translation, Vector2.One) { }
}
