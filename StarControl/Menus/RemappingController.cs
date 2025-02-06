﻿using Microsoft.Xna.Framework.Graphics;
using StarControl.Data;
using StarControl.Graphics;
using StarControl.Patches;
using StardewModdingAPI;

namespace StarControl.Menus;

internal class RemappingController(
    IInputHelper inputHelper,
    Farmer who,
    QuickSlotResolver resolver,
    QuickSlotRenderer renderer
)
{
    public event EventHandler<ItemActivationEventArgs>? ItemActivated;

    private const float BASE_OPACITY = 0.6f;

    // Should generally be kept in sync with RadialMenuController.QUICK_SLOT_ANIMATION_DURATION_MS.
    // Otherwise there may be "pops" where the combined displays are darker or lighter than they
    // should be.
    private const int FADE_DURATION_MS = 250;

    public bool HudVisible { get; set; }

    public Dictionary<SButton, RemappingSlot> Slots
    {
        get => slots;
        set
        {
            slots = value;
            ResolveSlots();
        }
    }

    private readonly HashSet<SButton> downButtons = [];
    private readonly Dictionary<SButton, IRadialMenuItem> resolvedItems = [];

    private float fadeTimeMs;
    private Dictionary<SButton, RemappingSlot> slots = [];

    public void Draw(SpriteBatch b)
    {
        if (Game1.IsHudDrawn && renderer.SpriteOpacity > 0)
        {
            renderer.Draw(b, Viewports.DefaultViewport);
        }
    }

    public void Refresh()
    {
        ResolveSlots();
    }

    public void Update(TimeSpan elapsed, bool isMenuActive)
    {
        Fade(elapsed, isMenuActive);

        if (isMenuActive)
        {
            return;
        }

        foreach (var (button, item) in resolvedItems)
        {
            var controllerButton = button.TryGetController(out var cb) ? cb : default;
            var buttonState = inputHelper.GetState(button);
            if (downButtons.Contains(button))
            {
                if (
                    !inputHelper.IsSuppressed(button)
                    && buttonState is SButtonState.Released or SButtonState.None
                )
                {
                    // We have to release the simulated tool button as soon as the remapped button
                    // is released, because some tools won't allow release until *after* they detect
                    // that the tool button is released.
                    if (InputPatches.ToolUseButton == controllerButton)
                    {
                        InputPatches.ToolUseButton = null;
                    }
                    if (item.EndActivation())
                    {
                        downButtons.Remove(button);
                    }
                }
                else
                {
                    item.ContinueActivation();
                }
                continue;
            }
            if (buttonState == SButtonState.Pressed)
            {
                if (Context.IsPlayerFree && Game1.activeClickableMenu is null)
                {
                    inputHelper.Suppress(button);
                }
                if (!Context.CanPlayerMove && !(Context.IsPlayerFree && item.IsActivating()))
                {
                    continue;
                }
                // We have to apply the patch _before_ trying to activate the item, because the
                // activation may depend on that button's state.
                InputPatches.ToolUseButton = controllerButton;
                var wasToolUseStarted = false;
                try
                {
                    var result = item.Activate(
                        who,
                        DelayedActions.None,
                        ItemActivationType.Instant
                    );
                    if (result != ItemActivationResult.Ignored)
                    {
                        if (result == ItemActivationResult.ToolUseStarted)
                        {
                            downButtons.Add(button);
                            wasToolUseStarted = true;
                        }
                        ItemActivated?.Invoke(this, new(item, result));
                        break;
                    }
                }
                finally
                {
                    if (!wasToolUseStarted)
                    {
                        InputPatches.ToolUseButton = null;
                    }
                }
            }
        }
    }

    private void Fade(TimeSpan elapsed, bool isMenuActive)
    {
        var shouldBeVisible = HudVisible && !isMenuActive;
        if (
            shouldBeVisible && fadeTimeMs >= FADE_DURATION_MS
            || !shouldBeVisible && fadeTimeMs <= 0
        )
        {
            return;
        }
        var delta = shouldBeVisible ? elapsed.TotalMilliseconds : -elapsed.TotalMilliseconds;
        fadeTimeMs = Math.Clamp(fadeTimeMs + (float)delta, 0, FADE_DURATION_MS);
        var fadeProgress = fadeTimeMs / FADE_DURATION_MS;
        var fadeOpacity = (fadeProgress < 1 ? MathF.Sin(fadeProgress * MathF.PI / 2f) : 1);
        renderer.BackgroundOpacity = BASE_OPACITY * fadeOpacity;
        renderer.SpriteOpacity = fadeOpacity;
    }

    private void ResolveSlots()
    {
        Logger.Log(LogCategory.QuickSlots, "Starting refresh of Remapping Slots.");
        renderer.Slots = Slots.ToDictionary(x => x.Key, x => x.Value as IItemLookup);
        resolvedItems.Clear();
        foreach (var (button, slot) in Slots)
        {
            if (string.IsNullOrEmpty(slot.Id))
            {
                continue;
            }
            Logger.Log(
                LogCategory.QuickSlots,
                $"Item data for remapping slot {button}: ID = {slot.IdType}:{slot.Id}"
            );
            var slottedItem = resolver.ResolveItem(slot.Id, slot.IdType);
            if (slottedItem is not null)
            {
                Logger.Log(
                    LogCategory.QuickSlots,
                    $"Created remapping slot item '{slottedItem.Title}' for {button} slot.",
                    LogLevel.Info
                );
                resolvedItems.Add(button, slottedItem);
            }
            else
            {
                Logger.Log(
                    LogCategory.QuickSlots,
                    $"No item or invalid item data for remapping slot button {button}."
                );
            }
        }
        renderer.SlotItems = resolvedItems;
        renderer.Invalidate();
    }
}
