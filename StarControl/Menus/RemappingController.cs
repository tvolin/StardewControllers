using Microsoft.Xna.Framework.Graphics;
using StarControl.Data;
using StarControl.Graphics;

namespace StarControl.Menus;

internal class RemappingController(QuickSlotResolver resolver, QuickSlotRenderer renderer)
{
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

    private readonly Dictionary<SButton, IRadialMenuItem> resolvedItems = [];

    private float fadeTimeMs;
    private Dictionary<SButton, RemappingSlot> slots = [];

    public void Draw(SpriteBatch b)
    {
        if (renderer.SpriteOpacity > 0)
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
