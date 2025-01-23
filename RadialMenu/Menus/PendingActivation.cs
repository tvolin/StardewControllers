namespace RadialMenu.Menus;

internal record PendingActivation(
    IRadialMenuItem Item,
    bool SecondaryAction = false,
    bool RequireConfirmation = false
);
