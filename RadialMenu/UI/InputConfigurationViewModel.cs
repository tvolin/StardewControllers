using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class InputConfigurationViewModel
{
    public EnumSegmentsViewModel<DelayedActions> DelayedActions { get; } = new();
    public EnumSegmentsViewModel<MenuOpenMode> OpenMode { get; } = new();
    public EnumSegmentsViewModel<ThumbStickPreference> ThumbStickPreference { get; } = new();

    [Notify]
    private int activationDelayMs;

    [Notify]
    private SButton inventoryMenuButton;

    [Notify]
    private SButton modMenuButton;

    [Notify]
    private SButton previousPageButton;

    [Notify]
    private SButton nextPageButton;

    [Notify]
    private SButton primaryActionButton;

    [Notify]
    private SButton secondaryActionButton;

    [Notify]
    private bool rememberSelection;

    [Notify]
    private float triggerDeadZone;

    [Notify]
    private float thumbstickDeadZone;

    public void Load(InputConfiguration config)
    {
        InventoryMenuButton = config.InventoryMenuButton;
        ModMenuButton = config.ModMenuButton;
        PreviousPageButton = config.PreviousPageButton;
        NextPageButton = config.NextPageButton;
        PrimaryActionButton = config.PrimaryActionButton;
        SecondaryActionButton = config.SecondaryActionButton;
        ThumbStickPreference.SelectedValue = config.ThumbStickPreference;
        OpenMode.SelectedValue = config.OpenMode;
        DelayedActions.SelectedValue = config.DelayedActions;
        ActivationDelayMs = config.ActivationDelayMs;
        RememberSelection = config.RememberSelection;
        TriggerDeadZone = config.TriggerDeadZone;
        ThumbstickDeadZone = config.ThumbstickDeadZone;
    }

    public void Save(InputConfiguration config)
    {
        config.InventoryMenuButton = InventoryMenuButton;
        config.ModMenuButton = ModMenuButton;
        config.PreviousPageButton = PreviousPageButton;
        config.NextPageButton = NextPageButton;
        config.PrimaryActionButton = PrimaryActionButton;
        config.SecondaryActionButton = SecondaryActionButton;
        config.ThumbStickPreference = ThumbStickPreference.SelectedValue;
        config.OpenMode = OpenMode.SelectedValue;
        config.DelayedActions = DelayedActions.SelectedValue;
        config.ActivationDelayMs = ActivationDelayMs;
        config.RememberSelection = RememberSelection;
        config.TriggerDeadZone = TriggerDeadZone;
        config.ThumbstickDeadZone = ThumbstickDeadZone;
    }
}
