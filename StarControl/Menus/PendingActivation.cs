namespace StarControl.Menus;

internal record PendingActivation(
    IRadialMenuItem Item,
    ItemActivationType ActivationType = ItemActivationType.Primary,
    bool IsRegularItem = true,
    bool RequireConfirmation = false
);
