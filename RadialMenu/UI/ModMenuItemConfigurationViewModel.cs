using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using RadialMenu.Config;
using RadialMenu.Gmcm;
using RadialMenu.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal enum ItemIconType
{
    Item,
    Custom,
}

internal enum ItemSyncType
{
    None,
    Gmcm,
}

internal partial class ModMenuItemConfigurationViewModel
{
    public bool CanEditDescription =>
        SyncType.SelectedValue != ItemSyncType.Gmcm || GmcmSync?.EnableDescriptionSync != true;
    public bool CanEditKeybind => SyncType.SelectedValue == ItemSyncType.None;
    public bool CanEditName =>
        SyncType.SelectedValue != ItemSyncType.Gmcm || GmcmSync?.EnableTitleSync != true;
    public bool Editable { get; set; } = true;
    public string Id { get; private set; }
    public Sprite Icon =>
        (IconType.SelectedValue == ItemIconType.Item ? IconFromItemId : CustomIcon)
        ?? new(Game1.mouseCursors, new(240, 192, 16, 16)); // Question mark
    public EnumSegmentsViewModel<ItemIconType> IconType { get; } = new();
    public bool IsCustomIcon => IconType.SelectedValue == ItemIconType.Custom;
    public bool IsGmcmSyncVisible => SyncType.SelectedValue == ItemSyncType.Gmcm;
    public bool IsStandardIcon => IconType.SelectedValue == ItemIconType.Item;
    public EnumSegmentsViewModel<ItemSyncType> SyncType { get; } = new();
    public TooltipData Tooltip =>
        !string.IsNullOrEmpty(Description) ? new TooltipData(Description, Name) : new(Name);

    [Notify]
    private Sprite? customIcon;

    [Notify]
    private string description = "";

    [Notify]
    private bool enableActivationDelay;

    [Notify]
    private bool enabled = true;

    [Notify]
    private GmcmSyncSettingsViewModel? gmcmSync;

    [Notify]
    private Keybind keybind = new();

    [Notify]
    private string iconAssetPath = "";

    [Notify]
    private Sprite? iconFromItemId;

    [Notify]
    private string? iconItemId;

    [Notify]
    private Rectangle iconSourceRect = Rectangle.Empty;

    [Notify]
    private string iconSourceRectText = "";

    [Notify]
    private bool isReordering;

    [Notify]
    private string name = "";

    [Notify]
    private ShelfViewModel<ParsedItemData> searchResults = ShelfViewModel<ParsedItemData>.Empty;

    [Notify]
    private string searchText = "";

    private readonly ParsedItemData[] allItems;
    private readonly object searchLock = new();

    private CancellationTokenSource searchCancellationTokenSource = new();

    public ModMenuItemConfigurationViewModel(string id, ParsedItemData[] allItems)
    {
        Id = id;
        SyncType.ValueChanged += SyncType_ValueChanged;
        IconType.ValueChanged += IconType_ValueChanged;
        this.allItems = allItems;
        UpdateRawSearchResults();
    }

    public void Load(ModMenuItemConfiguration config)
    {
        Id = config.Id;
        Name = config.Name;
        Description = config.Description;
        Keybind = config.Keybind;
        EnableActivationDelay = config.EnableActivationDelay;
        IconAssetPath = config.Icon.TextureAssetPath;
        IconSourceRect = config.Icon.SourceRect;
        IconItemId = config.Icon.ItemId;
        IconType.SelectedValue = !string.IsNullOrWhiteSpace(config.Icon.ItemId)
            ? ItemIconType.Item
            : ItemIconType.Custom;
        if (
            config.GmcmSync is { } gmcm
            && IGenericModConfigKeybindings.Instance is { } gmcmBindings
        )
        {
            SyncType.SelectedValue = ItemSyncType.Gmcm;
            GmcmSync = new(gmcmBindings)
            {
                SelectedOption = gmcmBindings.Find(
                    gmcm.ModId,
                    gmcm.FieldId,
                    gmcm.FieldName,
                    config.Keybind
                )
                    is { } option
                    ? new(option)
                    : null,
                EnableTitleSync = gmcm.EnableNameSync,
                EnableDescriptionSync = gmcm.EnableDescriptionSync,
            };
            GmcmSync.PropertyChanged += GmcmSync_PropertyChanged;
        }
        else
        {
            GmcmSync = null;
        }
    }

    public void Save(ModMenuItemConfiguration config)
    {
        config.Id = Id;
        config.Name = Name;
        config.Description = description;
        config.Keybind = Keybind;
        config.EnableActivationDelay = enableActivationDelay;
        config.Icon = IconType.SelectedValue switch
        {
            ItemIconType.Item => new() { ItemId = IconItemId ?? "" },
            ItemIconType.Custom => new()
            {
                TextureAssetPath = IconAssetPath,
                SourceRect = IconSourceRect,
            },
            _ => new(),
        };
        config.GmcmSync =
            SyncType.SelectedValue == ItemSyncType.Gmcm && GmcmSync is { } gmcm
                ? new()
                {
                    ModId = gmcm.SelectedMod?.UniqueID ?? "",
                    FieldId = gmcm.SelectedOption?.FieldId ?? "",
                    FieldName = gmcm.SelectedOption?.UniqueFieldName ?? "",
                    EnableNameSync = gmcm.EnableTitleSync,
                    EnableDescriptionSync = gmcm.EnableDescriptionSync,
                }
                : null;
    }

    public void OnRandomizeButtonHover()
    {
        Game1.playSound("Cowboy_Footstep");
    }

    public void PickRandomIcon()
    {
        Game1.playSound("drumkit6");
        IconType.SelectedValue = ItemIconType.Item;
        int index = Random.Shared.Next(allItems.Length);
        IconItemId = allItems[index].QualifiedItemId;
        UpdateRawSearchResults();
    }

    public void SetIconFromSearchResults(Vector2 position)
    {
        SearchResults.ScrollToPoint(position);
        if (SearchResults.SelectedItem?.QualifiedItemId is { } id && id != IconItemId)
        {
            Game1.playSound("smallSelect");
            IconItemId = id;
        }
    }

    private void GmcmSync_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName
                is nameof(GmcmSyncSettingsViewModel.SelectedMod)
                    or nameof(GmcmSyncSettingsViewModel.EnableTitleSync)
            && GmcmSync?.SelectedMod is not null
            && GmcmSync.EnableTitleSync
        )
        {
            Name = GmcmSync.SelectedMod.Name;
        }
        else if (
            e.PropertyName
                is nameof(GmcmSyncSettingsViewModel.SelectedOption)
                    or nameof(GmcmSyncSettingsViewModel.EnableDescriptionSync)
            && GmcmSync?.SelectedOption is not null
            && GmcmSync.EnableDescriptionSync
        )
        {
            Description = GmcmSync.SelectedOption.SimpleName;
        }

        if (
            e.PropertyName == nameof(GmcmSyncSettingsViewModel.SelectedOption)
            && GmcmSync?.SelectedOption is { } option
        )
        {
            Keybind = option.CurrentKeybind;
        }

        if (e.PropertyName == nameof(GmcmSyncSettingsViewModel.EnableTitleSync))
        {
            OnPropertyChanged(new(nameof(CanEditName)));
        }
        if (e.PropertyName == nameof(GmcmSyncSettingsViewModel.EnableDescriptionSync))
        {
            OnPropertyChanged(new(nameof(CanEditDescription)));
        }
    }

    private void IconType_ValueChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(new(nameof(Icon)));
        OnPropertyChanged(new(nameof(IsCustomIcon)));
        OnPropertyChanged(new(nameof(IsStandardIcon)));
    }

    private void OnIconAssetPathChanged()
    {
        CustomIcon = Sprite.TryLoad(IconAssetPath, IconSourceRect);
    }

    private void OnIconItemIdChanged()
    {
        if (!string.IsNullOrEmpty(IconItemId))
        {
            var data = ItemRegistry.GetDataOrErrorItem(IconItemId);
            IconFromItemId = new(data.GetTexture(), data.GetSourceRect());
            if (SearchResults.SelectedItem?.QualifiedItemId != IconItemId)
            {
                UpdateRawSearchResults();
            }
        }
    }

    private void OnIconSourceRectChanged()
    {
        if (CustomIcon is not null)
        {
            CustomIcon = new(CustomIcon.Texture, IconSourceRect);
        }
    }

    private void OnIconSourceRectTextChanged()
    {
        IconSourceRect = Sprite.TryParseRectangle(IconSourceRectText, out var rect)
            ? rect
            : Rectangle.Empty;
    }

    private void OnSearchTextChanged()
    {
        UpdateRawSearchResults();
    }

    private void SyncType_ValueChanged(object? sender, EventArgs e)
    {
        if (
            SyncType.SelectedValue == ItemSyncType.Gmcm
            && IGenericModConfigKeybindings.Instance is { } gmcmKeybindings
        )
        {
            GmcmSync ??= new(gmcmKeybindings);
            GmcmSync.PropertyChanged += GmcmSync_PropertyChanged;
        }
        OnPropertyChanged(new(nameof(IsGmcmSyncVisible)));
        OnPropertyChanged(new(nameof(CanEditName)));
        OnPropertyChanged(new(nameof(CanEditDescription)));
        OnPropertyChanged(new(nameof(CanEditKeybind)));
    }

    private void UpdateRawSearchResults()
    {
        searchCancellationTokenSource.Cancel();
        searchCancellationTokenSource = new();
        var cancellationToken = searchCancellationTokenSource.Token;
        var searchTask = Task.Run(
            () => allItems.Search(SearchText, cancellationToken),
            cancellationToken
        );
        searchTask.ContinueWith(
            t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Log($"Failed searching for items: {t.Exception}", LogLevel.Error);
                    return;
                }
                if (t.IsCanceled)
                {
                    return;
                }
                lock (searchLock)
                {
                    var previousIconItemId = IconItemId; // Save for thread safety
                    var foundItems = t.Result.ToArray();
                    var selectedIndex = !string.IsNullOrEmpty(previousIconItemId)
                        ? Math.Max(
                            Array.FindIndex(
                                foundItems,
                                r => r.QualifiedItemId == previousIconItemId
                            ),
                            0
                        )
                        : 0;
                    SearchResults = new(
                        foundItems,
                        visibleSize: 5,
                        bufferSize: 2,
                        centerMargin: 20,
                        itemDistance: 80,
                        initialSelectedIndex: selectedIndex
                    );
                }
            },
            cancellationToken
        );
    }
}
