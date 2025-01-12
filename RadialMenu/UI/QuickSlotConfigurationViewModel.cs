using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

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
        return ItemData is not null
            ? new(ItemData.GetTexture(), ItemData.GetSourceRect())
            : ModAction?.Icon;
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
