using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

// TODO: Limit number of items that can be added per page (stop showing + button).
// TODO: Make minimum window height match max number of item rows, test with different row counts.
// TODO: Implement item reordering.

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
            .FirstOrDefault(item => item.Id == id);
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
