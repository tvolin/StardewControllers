using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class InputConfigurationViewModel
{
    public EnumSegmentsViewModel<DelayedActions> DelayedActions { get; } = new();
    public Func<float, string> FormatActivationDelay { get; } = v => $"{v:f0} ms";
    public Func<float, string> FormatDeadZone { get; } = v => v.ToString("f2");
    public EnumSegmentsViewModel<ThumbStickPreference> ThumbStickPreference { get; } = new();
    public EnumSegmentsViewModel<MenuToggleMode> ToggleMode { get; } = new();

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
    private bool reopenOnHold;

    [Notify]
    private bool reopenOnHoldDisabled;

    [Notify]
    private bool rememberSelection;

    [Notify]
    private float triggerDeadZone;

    [Notify]
    private float thumbstickDeadZone;

    public InputConfigurationViewModel()
    {
        ToggleMode.ValueChanged += ToggleMode_ValueChanged;
    }

    private void ToggleMode_ValueChanged(object? sender, EventArgs e)
    {
        ReopenOnHoldDisabled = ToggleMode.SelectedValue != MenuToggleMode.Hold;
        if (ReopenOnHoldDisabled)
        {
            ReopenOnHold = false;
        }
    }

    public void Load(InputConfiguration config)
    {
        InventoryMenuButton = config.InventoryMenuButton;
        ModMenuButton = config.ModMenuButton;
        PreviousPageButton = config.PreviousPageButton;
        NextPageButton = config.NextPageButton;
        PrimaryActionButton = config.PrimaryActionButton;
        SecondaryActionButton = config.SecondaryActionButton;
        ThumbStickPreference.SelectedValue = config.ThumbStickPreference;
        ToggleMode.SelectedValue = config.ToggleMode;
        ReopenOnHold = config.ReopenOnHold;
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
        config.ToggleMode = ToggleMode.SelectedValue;
        config.ReopenOnHold = ReopenOnHold;
        config.DelayedActions = DelayedActions.SelectedValue;
        config.ActivationDelayMs = ActivationDelayMs;
        config.RememberSelection = RememberSelection;
        config.TriggerDeadZone = TriggerDeadZone;
        config.ThumbstickDeadZone = ThumbstickDeadZone;
    }
}
