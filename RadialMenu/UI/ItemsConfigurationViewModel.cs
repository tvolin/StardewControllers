using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

// TODO: Make minimum window height match max number of item rows, test with different row counts.

internal partial class ItemsConfigurationViewModel
{
    private const int MAX_PAGE_SIZE = 16;

    public string AddButtonTooltip =>
        IsPageFull
            ? I18n.Config_ModMenu_MaxItems_Description()
            : I18n.Config_ModMenu_AddItem_Description();
    public bool CanAddItem => !IsReordering;
    public bool CanRemoveItem => IsReordering;
    public bool IsPageFull => SelectedPageSize >= MAX_PAGE_SIZE;
    public bool IsReordering => GrabbedItem is not null;
    public PagerViewModel<ModMenuPageConfigurationViewModel> Pager { get; } = new();

    private readonly Task<ParsedItemData[]> allItemsTask = Task.Run(
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

    [Notify]
    private ModMenuItemConfigurationViewModel? grabbedItem;

    [Notify]
    private int inventoryPageSize;

    [Notify]
    private bool isTrashCanHovered;

    [Notify]
    private int selectedPageSize;

    [Notify]
    private bool showInventoryBlanks;

    [Notify]
    private QuickSlotGroupConfigurationViewModel quickSlots = new();

    private int grabbedItemIndex; // Within the page
    private int grabbedItemPageIndex;

    public ItemsConfigurationViewModel()
    {
        // Temporary - dummy config.
        InventoryPageSize = 12;
        Pager.PropertyChanging += Pager_PropertyChanging;
        Pager.PropertyChanged += Pager_PropertyChanged;
        allItemsTask.ContinueWith(t =>
        {
            var allItems = t.Result;
            var settingsItem = new ModMenuItemConfigurationViewModel("0", allItems)
            {
                Name = I18n.Config_ModMenu_SettingsItem_Name(),
                Description = I18n.Config_ModMenu_SettingsItem_Description(),
                CustomIcon = new(
                    Game1.content.Load<Texture2D>("Mods/focustense.RadialMenu/Sprites/UI"),
                    new(80, 0, 16, 16)
                ),
                Editable = false,
                IconType = { SelectedValue = ItemIconType.Custom },
            };
            Pager.Pages =
            [
                new(0)
                {
                    Items =
                    [
                        settingsItem,
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
                South = new() { ModAction = Pager.Pages[0].Items[0] },
            };
        });
    }

    public bool AddNewItem()
    {
        if (!CanAddItem || IsPageFull)
        {
            return false;
        }
        var newItem = new ModMenuItemConfigurationViewModel(
            IdGenerator.NewId(6),
            allItemsTask.Result
        );
        Pager.SelectedPage!.Items.Add(newItem);
        EditModMenuItem(newItem);
        return true;
    }

    public bool AddPage()
    {
        Game1.playSound("smallSelect");
        var nextIndex = Pager.Pages.Count;
        Pager.Pages.Add(new(nextIndex));
        Pager.SelectedPageIndex = nextIndex;
        return true;
    }

    public bool BeginReordering(ModMenuItemConfigurationViewModel item)
    {
        var itemIndex = Pager.SelectedPage?.Items.IndexOf(item) ?? -1;
        if (itemIndex < 0)
        {
            return false;
        }
        Game1.playSound("dwop");
        GrabbedItem = item;
        grabbedItemPageIndex = Pager.SelectedPageIndex;
        grabbedItemIndex = itemIndex;
        item.IsReordering = true;
        return true;
    }

    public bool EditModMenuItem(ModMenuItemConfigurationViewModel item)
    {
        if (item.Editable)
        {
            ViewEngine.OpenChildMenu("ModMenuItem", item);
        }
        else
        {
            Game1.playSound("drumkit6");
            item.Enabled = !item.Enabled;
        }
        return true;
    }

    public bool EditQuickSlot(QuickSlotConfigurationViewModel slot)
    {
        var context = new QuickSlotPickerViewModel(
            slot,
            allItemsTask.Result,
            Pager.Pages.Select(page => page.Clone()).ToList()
        );
        ViewEngine.OpenChildMenu("QuickSlotPicker", context);
        return true;
    }

    public bool EndReordering(ModMenuItemConfigurationViewModel? target = null)
    {
        if (GrabbedItem is null)
        {
            return false;
        }
        if (target is not null)
        {
            var targetIndex = Pager.SelectedPage?.Items.IndexOf(target) ?? -1;
            if (targetIndex < 0)
            {
                return false;
            }
            Game1.playSound("stoneStep");
            Pager.SelectedPage!.Items[targetIndex] = GrabbedItem;
            Pager.Pages[grabbedItemPageIndex].Items[grabbedItemIndex] = target;
        }
        GrabbedItem.IsReordering = false;
        GrabbedItem = null;
        return true;
    }

    public void HoverTrashCan()
    {
        Game1.playSound("trashcanlid");
        IsTrashCanHovered = true;
    }

    public void LeaveTrashCan()
    {
        IsTrashCanHovered = false;
    }

    public bool RemoveGrabbedItem()
    {
        if (GrabbedItem is null || !GrabbedItem.Editable)
        {
            return false;
        }
        Game1.playSound("trashcan");
        Pager.Pages[grabbedItemPageIndex].Items.Remove(GrabbedItem);
        GrabbedItem = null;
        return true;
    }

    private void Pager_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(Pager.SelectedPage) && Pager.SelectedPage is { } page)
        {
            page.Items.CollectionChanged += Pager_SelectedPage_ItemsChanged;
        }
    }

    private void Pager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Pager.SelectedPage))
        {
            SelectedPageSize = Pager.SelectedPage?.Items.Count ?? 0;
            if (Pager.SelectedPage is { } page)
            {
                page.Items.CollectionChanged += Pager_SelectedPage_ItemsChanged;
            }
        }
    }

    private void Pager_SelectedPage_ItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedPageSize = Pager.SelectedPage?.Items.Count ?? 0;
    }
}

internal partial class ModMenuPageConfigurationViewModel(int index) : PageViewModel(index)
{
    public Color ButtonTint => Selected ? new Color(0xaa, 0xcc, 0xee) : Color.White;

    [Notify]
    private ObservableCollection<ModMenuItemConfigurationViewModel> items = [];

    // View model also holds the selection state, so in order to copy this view model from the main
    // configuration menu to submenus like the Quick Slot editor, we clone them so that they have an
    // independent selection state (and visibility, transforms, etc).
    public ModMenuPageConfigurationViewModel Clone()
    {
        return new(Index) { Items = Items };
    }

    // Hopefully temporary workaround for the source generator not generating overrides.
    // https://github.com/canton7/PropertyChanged.SourceGenerator/issues/47
    [SuppressMessage(
        "PropertyChanged.SourceGenerator.Generation",
        "INPC021:Do not define your own overrides of the method to raise PropertyChanged events"
    )]
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(Selected))
        {
            OnPropertyChanged(new(nameof(ButtonTint)));
        }
    }
}
