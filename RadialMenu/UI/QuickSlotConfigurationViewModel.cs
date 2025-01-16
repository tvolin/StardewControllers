using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal partial class QuickSlotConfigurationViewModel
{
    private static readonly Color UnavailableColor = new(0x44, 0x44, 0x44, 0x44);

    [DependsOn(nameof(ItemData), nameof(ModAction))]
    public Sprite? Icon => GetIcon();

    public Color Tint =>
        ItemData is not null && Game1.player.Items.FindUsableItem(ItemData.QualifiedItemId) is null
            ? UnavailableColor
            : Color.White;

    [DependsOn(nameof(ItemData), nameof(ModAction))]
    public TooltipData Tooltip => GetTooltip();

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
        return ItemData is not null
            ? new(ItemData.GetTexture(), ItemData.GetSourceRect())
            : ModAction?.Icon;
    }

    private TooltipData GetTooltip()
    {
        if (ItemData is not null)
        {
            return new(
                Title: ItemData.DisplayName,
                Text: ItemData.Description,
                Item: ItemRegistry.Create(ItemData.QualifiedItemId)
            );
        }
        if (ModAction is not null)
        {
            return !string.IsNullOrEmpty(ModAction.Description)
                ? new(Title: ModAction.Name, Text: ModAction.Description)
                : new(ModAction.Name);
        }
        return new(I18n.Config_QuickActions_EmptySlot_Title());
    }

    private void ModAction_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ModAction.Icon))
        {
            OnPropertyChanged(new(nameof(Icon)));
        }
        else if (e.PropertyName is nameof(ModAction.Name) or nameof(ModAction.Description))
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
