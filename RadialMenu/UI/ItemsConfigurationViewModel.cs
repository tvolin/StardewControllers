using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using RadialMenu.Gmcm;
using StardewModdingAPI.Utilities;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal partial class ItemsConfigurationViewModel
{
    public ModMenuPageConfigurationViewModel? SelectedPage =>
        SelectedPageIndex >= 0 && SelectedPageIndex < ModMenuPages.Count
            ? ModMenuPages[SelectedPageIndex]
            : null;

    private readonly Task<ParsedItemData[]> allItems = Task.Run(
        () =>
            ItemRegistry
                .ItemTypes.SelectMany(t =>
                    t.GetAllIds()
                        .OrderBy(id =>
                            int.TryParse(id, out var numericId) ? numericId : int.MaxValue
                        )
                        .ThenBy(id => id)
                        .Select(id => t.Identifier + id)
                )
                .Select(ItemRegistry.GetDataOrErrorItem)
                .ToArray()
    );

    public ItemsConfigurationViewModel()
    {
        // Temporary - dummy config.
        InventoryPageSize = 12;
        ModMenuPages =
        [
            new(1)
            {
                Items =
                [
                    new("1", allItems)
                    {
                        Name = "Swap Rings",
                        IconItemId = "(O)534",
                        Keybind = new(SButton.Z),
                    },
                    new("2", allItems)
                    {
                        Name = "Summon Horse",
                        IconItemId = "(O)911",
                        Keybind = new(SButton.H),
                    },
                    new("3", allItems)
                    {
                        Name = "Event Lookup",
                        IconItemId = "(BC)42",
                        Keybind = new(SButton.N),
                    },
                    new("4", allItems)
                    {
                        Name = "Calendar",
                        IconItemId = "(F)1402",
                        Keybind = new(SButton.B),
                    },
                    new("5", allItems)
                    {
                        Name = "Quest Board",
                        IconItemId = "(F)BulletinBoard",
                        Keybind = new(SButton.Q),
                    },
                    new("6", allItems)
                    {
                        Name = "Stardew Progress",
                        IconItemId = "(O)434",
                        Keybind = new(SButton.F3),
                    },
                    new("7", allItems)
                    {
                        Name = "Data Layers",
                        IconItemId = "(F)1543",
                        Keybind = new(SButton.F2),
                    },
                    new("8", allItems)
                    {
                        Name = "Garbage In Garbage Can",
                        IconItemId = "(F)2427",
                        Keybind = new(SButton.G),
                    },
                    new("9", allItems)
                    {
                        Name = "Generic Mod Config Menu",
                        IconItemId = "(O)112",
                        Keybind = new(SButton.LeftShift, SButton.F8),
                    },
                    new("10", allItems)
                    {
                        Name = "Quick Stack",
                        IconItemId = "(BC)130",
                        Keybind = new(SButton.K),
                    },
                    new("11", allItems)
                    {
                        Name = "NPC Location Compass",
                        IconItemId = "(F)1545",
                        Keybind = new(SButton.LeftAlt),
                    },
                    new("12", allItems)
                    {
                        Name = "Toggle Fishing Overlays",
                        IconItemId = "(O)128",
                        Keybind = new(SButton.LeftShift, SButton.F),
                    },
                ],
            },
        ];
        QuickSlots = new()
        {
            DPadLeft = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(O)287") },
            DPadUp = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(T)Pickaxe") },
            DPadRight = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(W)4") },
            DPadDown = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(BC)71") },
            West = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(O)424") },
            North = new() { ItemData = ItemRegistry.GetDataOrErrorItem("(O)253") },
            South = new() { ModAction = ModMenuPages[0].Items[0] },
        };
    }

    [Notify]
    private Vector2 contentPanelSize;

    [Notify]
    private int inventoryPageSize;

    [Notify]
    private bool showInventoryBlanks;

    [Notify]
    private ObservableCollection<ModMenuPageConfigurationViewModel> modMenuPages = [];

    [Notify]
    private QuickSlotGroupConfigurationViewModel quickSlots = new();

    [Notify]
    private int selectedPageIndex = -1;

    public void AddPage()
    {
        Game1.playSound("smallSelect");
        var nextIndex = ModMenuPages.Count;
        ModMenuPages.Add(new(nextIndex + 1));
        SelectedPageIndex = nextIndex;
    }

    public void EditModMenuItem(string id)
    {
        // The most extreme configuration might have a few dozen items, not enough to justify
        // keeping a separate dictionary in sync.
        var item = ModMenuPages
            .SelectMany(page => page.Items)
            .Where(item => item.Id == id)
            .FirstOrDefault();
        if (item is null)
        {
            Logger.Log($"Item not found on any page: {id}", LogLevel.Warn);
            return;
        }
        ViewEngine.OpenChildMenu("ModMenuItem", item);
    }

    public void SelectModMenuPage(int pageIndex)
    {
        pageIndex--; // Index comes from the view model's Index which is 1-based.
        if (pageIndex == SelectedPageIndex)
        {
            return;
        }
        Game1.playSound("smallSelect");
        SelectedPageIndex = pageIndex;
    }

    private void OnContentPanelSizeChanged()
    {
        UpdatePageTransforms();
    }

    private void OnModMenuPagesChanged()
    {
        SelectedPageIndex = 0;
    }

    private void OnSelectedPageIndexChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0 && oldValue < ModMenuPages.Count)
        {
            ModMenuPages[oldValue].Selected = false;
        }
        if (newValue >= 0 && newValue < ModMenuPages.Count)
        {
            ModMenuPages[newValue].Selected = true;
        }
        UpdatePageTransforms();
    }

    private void UpdatePageTransforms()
    {
        for (int i = 0; i < ModMenuPages.Count; i++)
        {
            var page = ModMenuPages[i];
            if (ContentPanelSize != Vector2.Zero)
            {
                page.Transform = new(new(ContentPanelSize.X * (i - SelectedPageIndex), 0));
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

internal partial class ModMenuPageConfigurationViewModel(int index)
{
    public Color ButtonTint => Selected ? new Color(0xaa, 0xcc, 0xee) : Color.White;

    public int Index { get; } = index;

    [Notify]
    private ObservableCollection<ModMenuItemConfigurationViewModel> items = [];

    [Notify]
    private bool selected;

    [Notify]
    private Transform transform = new(Vector2.Zero);

    [Notify]
    private bool visible = false;
}

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
    public string Id { get; }
    public Sprite Icon =>
        (IconType.SelectedValue == ItemIconType.Item ? IconFromItemId : CustomIcon)
        ?? new(Game1.mouseCursors, new(240, 192, 16, 16)); // Question mark
    public EnumSegmentsViewModel<ItemIconType> IconType { get; } = new();
    public bool IsGmcmSyncVisible => SyncType.SelectedValue == ItemSyncType.Gmcm;
    public EnumSegmentsViewModel<ItemSyncType> SyncType { get; } = new();

    [Notify]
    private Sprite? customIcon;

    [Notify]
    private string description = "";

    [Notify]
    private GmcmSyncSettingsViewModel? gmcmSync;

    [Notify]
    private Keybind keybind = new();

    [Notify]
    private Sprite? iconFromItemId;

    [Notify]
    private string? iconItemId = null;

    [Notify]
    private string name = "";

    [Notify]
    private ShelfViewModel<ParsedItemData> searchResults = ShelfViewModel<ParsedItemData>.Empty;

    [Notify]
    private string searchText = "";

    private readonly Task<ParsedItemData[]> allItems;
    private readonly object searchLock = new();

    private CancellationTokenSource searchCancellationTokenSource = new();

    public ModMenuItemConfigurationViewModel(string id, Task<ParsedItemData[]> allItems)
    {
        Id = id;
        SyncType.ValueChanged += SyncType_ValueChanged;
        IconType.ValueChanged += IconType_ValueChanged;
        this.allItems = allItems;
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

    private async Task<ParsedItemData[]> GetRawSearchResults(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return await allItems;
        }
        else if (ItemRegistry.IsQualifiedItemId(SearchText))
        {
            var exactItem = ItemRegistry.GetData(SearchText);
            return exactItem is not null ? [exactItem] : [];
        }
        else if (int.TryParse(SearchText, out var objectId))
        {
            var exactItem = ItemRegistry.GetData("(O)" + SearchText);
            return exactItem is not null ? [exactItem] : [];
        }
        else
        {
            var allItems = await this.allItems;
            var matches = allItems
                .Where(item =>
                    !cancellationToken.IsCancellationRequested
                    && item.DisplayName.Contains(
                        SearchText,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                )
                .ToArray();
            return !cancellationToken.IsCancellationRequested ? matches : [];
        }
    }

    private void IconType_ValueChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(new(nameof(Icon)));
    }

    private void OnIconItemIdChanged()
    {
        if (!string.IsNullOrEmpty(IconItemId))
        {
            var data = ItemRegistry.GetDataOrErrorItem(IconItemId);
            IconFromItemId = new(data.GetTexture(), data.GetSourceRect());
        }
    }

    private void OnSearchTextChanged()
    {
        UpdateRawSearchResults();
    }

    private void SyncType_ValueChanged(object? sender, EventArgs e)
    {
        if (
            SyncType.SelectedValue == ItemSyncType.Gmcm
            && GenericModConfigKeybindings.Instance is not null
        )
        {
            GmcmSync ??= new(GenericModConfigKeybindings.Instance);
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
        var searchTask = Task.Run(() => GetRawSearchResults(cancellationToken), cancellationToken);
        searchTask.ContinueWith(
            t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Log($"Failed searching for items: {t.Exception}", LogLevel.Error);
                }
                lock (searchLock)
                {
                    var previousIconItemId = IconItemId; // Save for thread safety
                    var selectedIndex = !string.IsNullOrEmpty(previousIconItemId)
                        ? Math.Max(
                            Array.FindIndex(t.Result, r => r.QualifiedItemId == previousIconItemId),
                            0
                        )
                        : 0;
                    SearchResults = new(
                        t.Result,
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

internal partial class ModMenuItemIconSearchResultViewModel(
    ParsedItemData item,
    float translateX = 0
)
{
    public ParsedItemData Item { get; } = item;

    [Notify]
    private Transform transform = new(new(translateX, 0));
}

internal partial class QuickSlotGroupConfigurationViewModel
{
    [Notify]
    private QuickSlotConfigurationViewModel dPadDown = new();

    [Notify]
    private QuickSlotConfigurationViewModel dPadLeft = new();

    [Notify]
    private QuickSlotConfigurationViewModel dPadRight = new();

    [Notify]
    private QuickSlotConfigurationViewModel dPadUp = new();

    [Notify]
    private QuickSlotConfigurationViewModel east = new();

    [Notify]
    private QuickSlotConfigurationViewModel north = new();

    [Notify]
    private QuickSlotConfigurationViewModel south = new();

    [Notify]
    private QuickSlotConfigurationViewModel west = new();
}

internal partial class QuickSlotConfigurationViewModel
{
    [DependsOn(nameof(ItemData), nameof(ModAction))]
    public Sprite? Icon => GetIcon();

    [DependsOn(nameof(ItemData), nameof(ModAction))]
    public object Tooltip => GetTooltip();

    [Notify]
    private ParsedItemData? itemData;

    [Notify]
    private ModMenuItemConfigurationViewModel? modAction;

    [Notify]
    private bool requireConfirmation;

    [Notify]
    private bool useSecondaryAction;

    private Sprite? GetIcon()
    {
        if (ItemData is not null)
        {
            return new(ItemData.GetTexture(), ItemData.GetSourceRect());
        }
        if (ModAction is not null)
        {
            return ModAction.Icon;
        }
        return null;
    }

    private object GetTooltip()
    {
        if (ItemData is not null)
        {
            return ItemData;
        }
        if (ModAction is not null)
        {
            return Tuple.Create(ModAction.Name, ModAction.Description);
        }
        return I18n.Config_QuickActions_EmptySlot_Title();
    }

    private void ModAction_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ModAction.Icon))
        {
            OnPropertyChanged(new(nameof(Icon)));
        }
        else if (
            e.PropertyName == nameof(ModAction.Name)
            || e.PropertyName == nameof(ModAction.Description)
        )
        {
            OnPropertyChanged(new(nameof(Tooltip)));
        }
    }

    private void OnItemDataChanged()
    {
        if (ItemData is not null)
        {
            ModAction = null;
        }
    }

    private void OnModActionChanged(
        ModMenuItemConfigurationViewModel? oldValue,
        ModMenuItemConfigurationViewModel? newValue
    )
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= ModAction_PropertyChanged;
        }
        if (newValue is not null)
        {
            ItemData = null;
            newValue.PropertyChanged += ModAction_PropertyChanged;
        }
    }
}

internal record Sprite(Texture2D Texture, Rectangle SourceRect);
