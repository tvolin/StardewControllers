using PropertyChanged.SourceGenerator;

namespace RadialMenu.UI;

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
