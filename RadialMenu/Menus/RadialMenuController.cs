using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RadialMenu.Config;
using RadialMenu.Graphics;
using RadialMenu.Input;
using StardewValley.Menus;

namespace RadialMenu.Menus;

internal class RadialMenuController(
    IInputHelper inputHelper,
    ModConfig config,
    Farmer player,
    RadialMenuPainter radialMenuPainter,
    QuickSlotController quickSlotController,
    params IRadialMenu[] menus
)
{
    public event EventHandler<ItemActivationEventArgs>? ItemActivated;

    public bool Enabled
    {
        get => enabled;
        set
        {
            if (value == enabled)
            {
                return;
            }
            enabled = value;
            if (!value)
            {
                Reset();
            }
        }
    }
    public bool IsMenuActive => activeMenu is not null;

    private const int MENU_ANIMATION_DURATION_MS = 120;

    private IRadialMenu? activeMenu;
    private float? cursorAngle;
    private PendingActivation? delayedItem;
    private TimeSpan elapsedActivationDelay;
    private bool enabled;
    private int focusedIndex;
    private IRadialMenuItem? focusedItem;
    private float menuOpenTimeMs;
    private float menuScale;
    private float quickSlotOpacity;

    public void Draw(SpriteBatch b, Rectangle? viewport = null)
    {
        if (activeMenu?.GetSelectedPage() is not { } page)
        {
            return;
        }
        viewport ??= Viewports.DefaultViewport;
        radialMenuPainter.Items = page.Items;
        radialMenuPainter.Scale = menuScale;
        radialMenuPainter.Paint(
            b,
            page.SelectedItemIndex,
            focusedIndex,
            cursorAngle,
            GetSelectionBlend(),
            viewport
        );
        quickSlotController.Draw(b, viewport.Value, quickSlotOpacity);
    }

    public void Invalidate()
    {
        foreach (var menu in menus)
        {
            menu.Invalidate();
        }
        quickSlotController.Invalidate();
    }

    public void PreUpdate()
    {
        if (!Enabled)
        {
            return;
        }
        foreach (var menu in menus)
        {
            menu.Toggle.PreUpdate();
        }
    }

    public void Update(TimeSpan elapsed)
    {
        if (!Enabled)
        {
            return;
        }

        AnimateMenuOpen(elapsed);

        // Used only for animation, doesn't interfere with logic right now.
        quickSlotController.Update(elapsed);

        if (TryActivateDelayedItem(elapsed))
        {
            return;
        }

        var previousActiveMenu = activeMenu;
        TryInteractWithActiveMenu();
        foreach (var menu in menus)
        {
            if (menu == previousActiveMenu)
            {
                continue;
            }
            menu.Toggle.Update(allowOn: activeMenu is null);
            if (menu.Toggle.State != MenuToggleState.On)
            {
                continue;
            }
            Game1.playSound("shwip");
            if (!config.Input.RememberSelection)
            {
                menu.ResetSelectedPage();
            }
            else if (menu.GetSelectedPage()?.IsEmpty() == true)
            {
                // Will automatically try to find a non-empty page.
                menu.PreviousPage();
            }
            activeMenu = menu;
            if (previousActiveMenu is null && activeMenu is not null)
            {
                AnimateMenuOpen(elapsed); // Skip "zero" frame
            }
        }

        TryActivateQuickSlot();
    }

    private void ActivateFocusedItem()
    {
        if (focusedItem is null)
        {
            return;
        }
        if (SuppressIfPressed(config.Input.PrimaryActionButton))
        {
            ActivateItem(focusedItem, secondaryAction: false);
        }
        else if (SuppressIfPressed(config.Input.SecondaryActionButton))
        {
            ActivateItem(focusedItem, secondaryAction: true);
        }
    }

    private ItemActivationResult ActivateItem(
        IRadialMenuItem item,
        bool secondaryAction,
        bool allowDelay = true,
        bool forceSuppression = false
    )
    {
        var result = item.Activate(
            player,
            allowDelay ? config.Input.DelayedActions : DelayedActions.None,
            secondaryAction
        );
        switch (result)
        {
            case ItemActivationResult.Ignored:
                return result;
            case ItemActivationResult.Delayed:
                Game1.playSound("select");
                delayedItem = new(item, SecondaryAction: secondaryAction);
                break;
            default:
                ItemActivated?.Invoke(this, new(item, result));
                if (forceSuppression)
                {
                    activeMenu?.Toggle.ForceOff();
                }
                Reset();
                break;
        }
        return result;
    }

    private void AnimateMenuOpen(TimeSpan elapsed)
    {
        if (activeMenu is null || menuOpenTimeMs >= MENU_ANIMATION_DURATION_MS)
        {
            return;
        }
        menuOpenTimeMs += (float)elapsed.TotalMilliseconds;
        var progress = MathHelper.Clamp(menuOpenTimeMs / MENU_ANIMATION_DURATION_MS, 0, 1);
        menuScale = progress < 1 ? 1 - MathF.Pow(1 - progress, 3) : 1;
        quickSlotOpacity = progress < 1 ? MathF.Sin(progress * MathF.PI / 2f) : 1;
    }

    private float GetSelectionBlend()
    {
        if (delayedItem is null)
        {
            return 1.0f;
        }
        var elapsed = (float)(
            config.Input.ActivationDelayMs - elapsedActivationDelay.TotalMilliseconds
        );
        return Animation.GetDelayFlashPosition(elapsed);
    }

    private void Reset()
    {
        delayedItem = null;
        elapsedActivationDelay = TimeSpan.Zero;
        focusedIndex = -1;
        focusedItem = null;
        cursorAngle = null;
        if (!config.Input.ReopenOnHold)
        {
            activeMenu?.Toggle.ForceOff();
        }
        activeMenu = null;
        menuOpenTimeMs = 0;
        menuScale = 0;
        quickSlotOpacity = 0;
    }

    private bool SuppressIfPressed(SButton button)
    {
        if (inputHelper.GetState(button) != SButtonState.Pressed)
        {
            return false;
        }
        inputHelper.Suppress(button);
        return true;
    }

    private bool TryActivateDelayedItem(TimeSpan elapsed)
    {
        if (delayedItem is not { } activation)
        {
            return false;
        }
        elapsedActivationDelay += elapsed;
        if (elapsedActivationDelay.TotalMilliseconds >= config.Input.ActivationDelayMs)
        {
            var result = activation.Item.Activate(
                player,
                DelayedActions.None,
                activation.SecondaryAction
            );
            ItemActivated?.Invoke(this, new(activation.Item, result));
            Reset();
        }
        // We still return true here, even if the delay hasn't expired, because a delayed activation
        // should prevent any other menu state from changing.
        return true;
    }

    private void TryActivateQuickSlot()
    {
        if (activeMenu is null || delayedItem is not null || cursorAngle is not null)
        {
            return;
        }
        var nextActivation = quickSlotController.TryGetNextActivation(out var pressedButton);
        if (nextActivation is not null)
        {
            inputHelper.Suppress(pressedButton);
            if (nextActivation.RequireConfirmation)
            {
                var message = nextActivation.IsRegularItem
                    ? I18n.QuickSlotConfirmation_Item(nextActivation.Item.Title)
                    : I18n.QuickSlotConfirmation_Mod(nextActivation.Item.Title);
                Game1.activeClickableMenu = new ConfirmationDialog(
                    message,
                    _ =>
                    {
                        Game1.activeClickableMenu = null;
                        ActivateItem(
                            nextActivation.Item,
                            nextActivation.SecondaryAction,
                            allowDelay: false,
                            // Forcing suppression here isn't done for any technical reason, it just seems more
                            // principle-of-least-surprise compliant not to have the menu immediately reopen or
                            // appear to stay open after e.g. switching a tool.
                            forceSuppression: true
                        );
                    }
                );
            }
            else
            {
                var result = ActivateItem(nextActivation.Item, nextActivation.SecondaryAction);
                if (result == ItemActivationResult.Delayed)
                {
                    quickSlotController.ShowDelayedActivation(pressedButton);
                }
            }
        }
    }

    private void TryInteractWithActiveMenu()
    {
        if (activeMenu is null)
        {
            return;
        }

        activeMenu.Toggle.Update(allowOn: false);
        if (activeMenu.Toggle.State != MenuToggleState.On)
        {
            Reset();
            return;
        }

        if (SuppressIfPressed(config.Input.PreviousPageButton))
        {
            if (activeMenu.PreviousPage())
            {
                Game1.playSound("shwip");
            }
        }
        else if (SuppressIfPressed(config.Input.NextPageButton))
        {
            if (activeMenu.NextPage())
            {
                Game1.playSound("shwip");
            }
        }

        UpdateFocus(activeMenu);
        ActivateFocusedItem();
    }

    private void UpdateFocus(IRadialMenu menu)
    {
        var thumbsticks = Game1.input.GetGamePadState().ThumbSticks;
        var position = config.Input.ThumbStickPreference switch
        {
            ThumbStickPreference.AlwaysLeft => thumbsticks.Left,
            ThumbStickPreference.AlwaysRight => thumbsticks.Right,
            _ => menu.Toggle.IsRightSided() ? thumbsticks.Right : thumbsticks.Left,
        };
        float? angle =
            position.Length() > config.Input.ThumbstickDeadZone
                ? MathF.Atan2(position.X, position.Y)
                : null;
        cursorAngle = (angle + MathHelper.TwoPi) % MathHelper.TwoPi;
        if (cursorAngle is not null && menu.GetSelectedPage() is { Items.Count: > 0 } page)
        {
            var itemAngle = MathHelper.TwoPi / page.Items.Count;
            var nextFocusedIndex =
                (int)MathF.Round(cursorAngle.Value / itemAngle) % page.Items.Count;
            if (nextFocusedIndex != focusedIndex)
            {
                Game1.playSound("shiny4");
                focusedIndex = nextFocusedIndex;
                focusedItem = page.Items[focusedIndex];
            }
        }
        else
        {
            focusedIndex = -1;
            focusedItem = null;
        }
    }
}
