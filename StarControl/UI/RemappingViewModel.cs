using PropertyChanged.SourceGenerator;
using StarControl.Data;
using StarControl.Graphics;
using StarControl.Menus;

namespace StarControl.UI;

internal partial class RemappingViewModel(
    IInputHelper inputHelper,
    Farmer who,
    IEnumerable<IRadialMenuItem> modItems,
    Action<Dictionary<SButton, RemappingSlot>> onSave
)
{
    public bool IsItemHovered => HoveredItem is not null;
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

    private readonly Dictionary<SButton, RemappableItemViewModel> assignedItems = [];
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

    public void UnassignSlot(RemappingSlotViewModel slot)
    {
        if (slot.Item is null)
        {
            return;
        }
        Game1.playSound("trashcan");
        slot.Item.AssignedButton = SButton.None;
        slot.Item = null;
        Save();
    }

    public void Update()
    {
        CanReassign =
            inputHelper.IsDown(SButton.LeftTrigger) || inputHelper.IsDown(SButton.RightTrigger);
    }
}

internal partial class RemappingSlotViewModel(SButton button)
{
    public SButton Button { get; } = button;

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

    [Notify]
    private SButton assignedButton;

    [Notify]
    private bool hovered;

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
