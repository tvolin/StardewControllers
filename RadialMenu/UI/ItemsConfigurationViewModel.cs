using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI.Utilities;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal partial class ItemsConfigurationViewModel
{
    public ModMenuPageConfigurationViewModel? SelectedPage =>
        SelectedPageIndex >= 0 && SelectedPageIndex < ModMenuPages.Count
            ? ModMenuPages[SelectedPageIndex]
            : null;

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
                    new("1")
                    {
                        Name = "Swap Rings",
                        IconItemId = "(O)534",
                        Keybind = new(SButton.Z),
                    },
                    new("2")
                    {
                        Name = "Summon Horse",
                        IconItemId = "(O)911",
                        Keybind = new(SButton.H),
                    },
                    new("3")
                    {
                        Name = "Event Lookup",
                        IconItemId = "(BC)42",
                        Keybind = new(SButton.N),
                    },
                    new("4")
                    {
                        Name = "Calendar",
                        IconItemId = "(F)1402",
                        Keybind = new(SButton.B),
                    },
                    new("5")
                    {
                        Name = "Quest Board",
                        IconItemId = "(F)BulletinBoard",
                        Keybind = new(SButton.Q),
                    },
                    new("6")
                    {
                        Name = "Stardew Progress",
                        IconItemId = "(O)434",
                        Keybind = new(SButton.F3),
                    },
                    new("7")
                    {
                        Name = "Data Layers",
                        IconItemId = "(F)1543",
                        Keybind = new(SButton.F2),
                    },
                    new("8")
                    {
                        Name = "Garbage In Garbage Can",
                        IconItemId = "(F)2427",
                        Keybind = new(SButton.G),
                    },
                    new("9")
                    {
                        Name = "Generic Mod Config Menu",
                        IconItemId = "(O)112",
                        Keybind = new(SButton.LeftShift, SButton.F8),
                    },
                    new("10")
                    {
                        Name = "Quick Stack",
                        IconItemId = "(BC)130",
                        Keybind = new(SButton.K),
                    },
                    new("11")
                    {
                        Name = "NPC Location Compass",
                        IconItemId = "(F)1545",
                        Keybind = new(SButton.LeftAlt),
                    },
                    new("12")
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

internal partial class ModMenuItemConfigurationViewModel(string id)
{
    // Extra hidden elements to include on either side of search results.
    private const int VISIBLE_RESULT_BUFFER_SIZE = 2;

    // Maximum number of results that should be visible in a non-transient state.
    private const int VISIBLE_RESULT_COUNT = 5;

    public string Id { get; } = id;
    public Sprite Icon =>
        (IconType == ItemIconType.Item ? iconFromItemId : CustomIcon)
        ?? new(Game1.mouseCursors, new(240, 192, 16, 16)); // Question mark

    [Notify]
    private Sprite? customIcon;

    [Notify]
    private string description = "";

    [Notify]
    private Keybind keybind = new();

    [Notify]
    private Sprite? iconFromItemId;

    [Notify]
    private ItemIconType iconType = ItemIconType.Item;

    [Notify]
    private string? iconItemId = null;

    [Notify]
    private string name = "";

    [Notify]
    private ObservableCollection<ModMenuItemIconSearchResultViewModel> searchResults = [];

    [Notify]
    private string searchText = "";

    [Notify]
    private ItemSyncType syncType = ItemSyncType.None;

    private CancellationTokenSource searchCancellationTokenSource = new();

    private readonly Task<ParsedItemData[]> allItems = Task.Run(
        () =>
            ItemRegistry
                .ItemTypes.SelectMany(t => t.GetAllIds())
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .Select(ItemRegistry.GetDataOrErrorItem)
                .ToArray()
    );
    private readonly object searchLock = new();

    private ParsedItemData[] rawSearchResults = [];
    private int searchResultIndex = -1;

    public void AdvanceSearchResult(int step)
    {
        if (rawSearchResults.Length == 0)
        {
            return;
        }
        Game1.playSound("shiny4");
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

    private void OnIconItemIdChanged()
    {
        if (!string.IsNullOrEmpty(IconItemId))
        {
            var data = ItemRegistry.GetDataOrErrorItem(IconItemId);
            IconFromItemId = new(data.GetTexture(), data.GetSourceRect());
        }
    }

    private void UpdateRawSearchResults()
    {
        searchCancellationTokenSource.Cancel();
        searchCancellationTokenSource = new();
        var cancellationToken = searchCancellationTokenSource.Token;
        var searchTask = Task.Run(() => GetRawSearchResults(cancellationToken), cancellationToken);
        searchTask.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Logger.Log($"Failed searching for items: {t.Exception}", LogLevel.Error);
            }
            lock (searchLock)
            {
                rawSearchResults = t.Result;
            }
            UpdateVisibleSearchResults();
        });
    }

    private void UpdateVisibleSearchResults()
    {
        var previousIconItemId = IconItemId; // Save for thread safety
        lock (searchLock)
        {
            if (rawSearchResults.Length == 0)
            {
                SearchResults.Clear();
                return;
            }
            searchResultIndex = !string.IsNullOrEmpty(previousIconItemId)
                ? Array.FindIndex(rawSearchResults, r => r.QualifiedItemId == previousIconItemId)
                : 0;
            var visibleResultCount = Math.Min(
                rawSearchResults.Length,
                VISIBLE_RESULT_COUNT + VISIBLE_RESULT_BUFFER_SIZE * 2
            );
            var visibleResults = new ModMenuItemIconSearchResultViewModel[visibleResultCount];
            var midIndex = (visibleResultCount - 1) / 2;
            visibleResults[midIndex] = new(rawSearchResults[searchResultIndex]);
            var perSideCount = Math.Min(
                visibleResultCount / 2,
                VISIBLE_RESULT_COUNT / 2 + VISIBLE_RESULT_BUFFER_SIZE
            );
            for (int i = 1; i < perSideCount; i++)
            {
                var visibleIndexAfter = midIndex + 1;
                if (visibleIndexAfter < visibleResults.Length)
                {
                    var sourceIndexAfter = (searchResultIndex + i) % rawSearchResults.Length;
                    visibleResults[visibleIndexAfter] = new(
                        rawSearchResults[sourceIndexAfter],
                        20 + i * 80
                    );
                }
                var visibleIndexBefore = midIndex - 1;
                if (visibleIndexBefore >= 0)
                {
                    var sourceIndexBefore =
                        (searchResultIndex - i + rawSearchResults.Length) % rawSearchResults.Length;
                    visibleResults[visibleIndexBefore] = new(
                        rawSearchResults[sourceIndexBefore],
                        -20 - i * 80
                    );
                }
            }
            SearchResults = new(visibleResults);
        }
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
