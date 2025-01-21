using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RadialMenu.Config;
using RadialMenu.Input;

namespace RadialMenu.Menus;

public class RadialMenuController(
    IInputHelper inputHelper,
    ModConfig config,
    Farmer player,
    Painter painter,
    params IRadialMenu[] menus
)
{
    public bool IsMenuActive => activeMenu is not null;

    private IRadialMenu? activeMenu;
    private float? cursorAngle;
    private (IRadialMenuItem item, bool isSecondaryAction)? delayedItem;
    private TimeSpan elapsedActivationDelay;
    private int focusedIndex;
    private IRadialMenuItem? focusedItem;

    public void Draw(SpriteBatch b)
    {
        if (activeMenu?.GetSelectedPage() is not { } page)
        {
            return;
        }
        painter.Items = page.Items;
        painter.Paint(b, page.SelectedItemIndex, focusedIndex, cursorAngle, GetSelectionBlend());
    }

    public void Update(TimeSpan elapsed)
    {
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
            if (menu.Toggle.State == MenuToggleState.On)
            {
                activeMenu = menu;
            }
        }
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

    private void ActivateItem(IRadialMenuItem item, bool secondaryAction)
    {
        var result = item.Activate(player, config.Input.DelayedActions, secondaryAction);
        switch (result)
        {
            case MenuItemActivationResult.Ignored:
                return;
            case MenuItemActivationResult.Delayed:
                delayedItem = (item, secondaryAction);
                break;
            default:
                FinishActivation();
                break;
        }
    }

    private void FinishActivation()
    {
        // It is probably not necessary to clear any state other than delayedItem and activeMenu,
        // but we're staying on the safe side to avoid bugs.
        delayedItem = null;
        elapsedActivationDelay = TimeSpan.Zero;
        focusedIndex = -1;
        focusedItem = null;
        activeMenu = null;
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
        return MathF.Abs(elapsed / 80 % 2 - 1);
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
        if (delayedItem is not ({ } item, var secondaryAction))
        {
            return false;
        }
        elapsedActivationDelay += elapsed;
        if (elapsedActivationDelay.TotalMilliseconds >= config.Input.ActivationDelayMs)
        {
            item.Activate(player, DelayedActions.None, secondaryAction);
            delayedItem = null;
            elapsedActivationDelay = TimeSpan.Zero;
        }
        // We still return true here, even if the delay hasn't expired, because a delayed activation
        // should prevent any other menu state from changing.
        return true;
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
            activeMenu = null;
            return;
        }

        if (SuppressIfPressed(config.Input.PreviousPageButton))
        {
            activeMenu.PreviousPage();
        }
        else if (SuppressIfPressed(config.Input.NextPageButton))
        {
            activeMenu.NextPage();
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
            focusedIndex = (int)MathF.Round(cursorAngle.Value / itemAngle) % page.Items.Count;
            focusedItem = page.Items[focusedIndex];
        }
        else
        {
            focusedIndex = -1;
            focusedItem = null;
        }
    }
}
