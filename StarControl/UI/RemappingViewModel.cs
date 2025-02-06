using PropertyChanged.SourceGenerator;
using StarControl.Data;
using StarControl.Graphics;
using StarControl.Menus;

namespace StarControl.UI;

internal partial class RemappingViewModel(
    IInputHelper inputHelper,
    Farmer who,
    IEnumerable<IRadialMenuItem> modItems,
    SButton menuToggleButton,
    Action<Dictionary<SButton, RemappingSlot>> onSave
)
{
    public IMenuController? Controller { get; set; }
    public bool IsItemHovered => HoveredItem is not null;
    public bool IsSlotHovered => HoveredSlot is not null;
    public bool IsSlotHoveredAndAssigned => HoveredSlot?.Item is not null;
    public IReadOnlyList<RemappableItemGroupViewModel> ItemGroups { get; } =
        [
            new()
            {
                Name = I18n.Enum_QuickSlotItemSource_Inventory_Name(),
                Items = who
                    .Items.Where(item => item is not null)
                    .Select(RemappableItemViewModel.FromInventoryItem)
                    .ToList(),
            },
            new()
            {
                Name = I18n.Enum_QuickSlotItemSource_ModItems_Name(),
                Items = modItems.Select(RemappableItemViewModel.FromMenuItem).ToList(),
            },
        ];
    public IEnumerable<RemappingSlotViewModel> Slots => slotsByButton.Values;

    [Notify]
    private bool canReassign;

    [Notify]
    private RemappableItemViewModel? hoveredItem;

    [Notify]
    private RemappingSlotViewModel? hoveredSlot;

    private readonly Dictionary<SButton, RemappingSlotViewModel> slotsByButton = new()
    {
        { SButton.DPadLeft, new(SButton.DPadLeft) },
        { SButton.DPadUp, new(SButton.DPadUp) },
        { SButton.DPadRight, new(SButton.DPadRight) },
        { SButton.DPadDown, new(SButton.DPadDown) },
        { SButton.ControllerX, new(SButton.ControllerX) },
        { SButton.ControllerY, new(SButton.ControllerY) },
        { SButton.ControllerB, new(SButton.ControllerB) },
        { SButton.LeftShoulder, new(SButton.LeftShoulder) },
        { SButton.RightShoulder, new(SButton.RightShoulder) },
    };

    public bool AssignToSlot(SButton button, RemappableItemViewModel item)
    {
        if (!CanReassign || !slotsByButton.TryGetValue(button, out var slot))
        {
            return false;
        }
        Game1.playSound("drumkit6");
        if (slot.Item is not null)
        {
            slot.Item.AssignedButton = SButton.None;
        }
        item.AssignedButton = button;
        slot.Item = item;
        Save();
        return true;
    }

    public void Load(Dictionary<SButton, RemappingSlot> data)
    {
        foreach (var slot in slotsByButton.Values)
        {
            if (slot.Item is { } previousItem)
            {
                previousItem.AssignedButton = SButton.None;
            }
            slot.Item = null;
        }
        foreach (var (button, slotData) in data)
        {
            if (
                string.IsNullOrEmpty(slotData.Id)
                || !slotsByButton.TryGetValue(button, out var slot)
            )
            {
                continue;
            }
            var item = slotData.IdType switch
            {
                ItemIdType.GameItem => ItemGroups[0]
                    .Items.FirstOrDefault(item => item.Id == slotData.Id)
                    ?? RemappableItemViewModel.FromInventoryItem(ItemRegistry.Create(slotData.Id)),
                ItemIdType.ModItem => ItemGroups[1]
                    .Items.FirstOrDefault(item => item.Id == slotData.Id),
                _ => null,
            };
            if (item is not null)
            {
                item.AssignedButton = button;
                slot.Item = item;
            }
        }
    }

    public void Save()
    {
        var data = new Dictionary<SButton, RemappingSlot>();
        foreach (var (button, slot) in slotsByButton)
        {
            if (slot.Item is { } item && !string.IsNullOrEmpty(item.Id))
            {
                data[button] = new() { Id = slot.Item.Id, IdType = slot.Item.IdType };
            }
        }
        onSave(data);
    }

    public void SetItemHovered(RemappableItemViewModel? item)
    {
        if (HoveredItem == item)
        {
            return;
        }
        if (HoveredItem is not null)
        {
            HoveredItem.Hovered = false;
        }
        HoveredItem = item;
        if (item is not null)
        {
            item.Hovered = true;
        }
    }

    public void SetSlotHovered(RemappingSlotViewModel? slot)
    {
        HoveredSlot = slot;
    }

    public void UnassignSlot(RemappingSlotViewModel slot)
    {
        if (slot.Item is null)
        {
            return;
        }
        Game1.playSound("trashcan");
        slot.Item.AssignedButton = SButton.None;
        slot.Item = null;
        OnPropertyChanged(new(nameof(IsSlotHoveredAndAssigned)));
        Save();
    }

    public void Update()
    {
        CanReassign =
            inputHelper.IsDown(SButton.LeftTrigger) || inputHelper.IsDown(SButton.RightTrigger);
        // IClickableMenu.receiveGamePadButton bizarrely does not receive some buttons such as the
        // left/right stick. We have to check them for through the helper.
        if (
            !CanReassign
            && menuToggleButton
                is not SButton.DPadUp
                    or SButton.DPadDown
                    or SButton.DPadLeft
                    or SButton.DPadRight
            && inputHelper.GetState(menuToggleButton) == SButtonState.Pressed
        )
        {
            Controller?.Close();
        }
    }
}

internal partial class RemappingSlotViewModel(SButton button)
{
    public SButton Button { get; } = button;
    public int? Count => Item?.Count ?? 1;
    public bool IsCountVisible => Count > 1;
    public int Quality => Item?.Quality ?? 0;

    public Sprite? Sprite => Item?.Sprite;

    public TooltipData? Tooltip => Item?.Tooltip;

    [Notify]
    private RemappableItemViewModel? item;
}

internal partial class RemappableItemGroupViewModel
{
    [Notify]
    private IReadOnlyList<RemappableItemViewModel> items = [];

    [Notify]
    private string name = "";
}

internal partial class RemappableItemViewModel
{
    public string Id { get; init; } = "";

    public ItemIdType IdType { get; init; }
    public bool IsCountVisible => Count > 1;

    [Notify]
    private SButton assignedButton;

    [Notify]
    private int count = 1;

    [Notify]
    private bool hovered;

    [Notify]
    private int quality;

    [Notify]
    private Sprite? sprite;

    [Notify]
    private TooltipData? tooltip;

    public static RemappableItemViewModel FromInventoryItem(Item item)
    {
        var itemData = ItemRegistry.GetData(item.QualifiedItemId);
        return new()
        {
            Id = item.QualifiedItemId,
            IdType = ItemIdType.GameItem,
            Sprite = new(itemData.GetTexture(), itemData.GetSourceRect()),
            Quality = item.Quality,
            Count = item.Stack,
            Tooltip = new(item.getDescription(), item.DisplayName, item),
        };
    }

    public static RemappableItemViewModel FromMenuItem(IRadialMenuItem item)
    {
        return new()
        {
            Id = item.Id,
            IdType = ItemIdType.ModItem,
            Sprite = item.Texture is not null
                ? new(item.Texture, item.SourceRectangle ?? item.Texture.Bounds)
                : Sprites.Error(),
            Tooltip = !string.IsNullOrEmpty(item.Description)
                ? new(item.Description, item.Title)
                : new(item.Title),
        };
    }
}
