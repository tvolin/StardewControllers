using Microsoft.Xna.Framework.Graphics;
using RadialMenu.Config;
using StardewValley.Tools;

namespace RadialMenu.Menus;

internal class QuickSlotController(
    IInputHelper inputHelper,
    ModConfig config,
    Farmer player,
    ModMenu modMenu,
    QuickSlotRenderer renderer
)
{
    private readonly Dictionary<SButton, IRadialMenuItem> slotItems = [];

    private bool isDirty = true;

    public void Draw(SpriteBatch spriteBatch, Rectangle viewport, float opacity = 1)
    {
        if (isDirty)
        {
            RefreshSlots();
            renderer.SlotItems = slotItems;
            isDirty = false;
        }
        renderer.Opacity = opacity;
        renderer.Draw(spriteBatch, viewport);
    }

    public void Invalidate()
    {
        renderer.Invalidate();
        isDirty = true;
    }

    public void ShowDelayedActivation(SButton button)
    {
        renderer.FlashDelay(button);
    }

    public PendingActivation? TryGetNextActivation(out SButton pressedButton)
    {
        foreach (var button in config.Items.QuickSlots.Keys)
        {
            if (inputHelper.GetState(button) != SButtonState.Pressed)
            {
                continue;
            }
            if (!slotItems.TryGetValue(button, out var item))
            {
                Game1.playSound("cancel");
                renderer.FlashError(button);
                continue;
            }
            var itemConfig = config.Items.QuickSlots[button];
            pressedButton = button;
            return new(
                item,
                SecondaryAction: itemConfig.UseSecondaryAction,
                IsRegularItem: itemConfig.IdType == ItemIdType.GameItem,
                RequireConfirmation: itemConfig.RequireConfirmation
            );
        }
        pressedButton = SButton.None;
        return null;
    }

    public void Update(TimeSpan elapsed)
    {
        renderer.Update(elapsed);
    }

    private void RefreshSlots()
    {
        slotItems.Clear();
        foreach (var (button, slot) in config.Items.QuickSlots)
        {
            if (string.IsNullOrEmpty(slot.Id))
            {
                continue;
            }
            var slottedItem = slot.IdType switch
            {
                ItemIdType.GameItem => TryGetInventoryItem(slot.Id) is { } item
                    ? new InventoryMenuItem(item)
                    : null,
                ItemIdType.ModItem => modMenu.GetItem(slot.Id),
                _ => null,
            };
            if (slottedItem is not null)
            {
                slotItems.Add(button, slottedItem);
            }
        }
        isDirty = false;
    }

    private Item? TryGetInventoryItem(string id)
    {
        if (ItemRegistry.GetData(id) is not { } data)
        {
            return null;
        }
        // Melee weapons don't have upgrades or base items, but if we didn't find an exact match, it
        // is often helpful to find any other melee weapon that's available.
        if (data.ItemType.Identifier == "(W)")
        {
            // We'll match scythes to scythes, and non-scythes to non-scythes.
            // Most likely, the player wants Iridium Scythe if the slot says Scythe. The upgraded
            // version is simply better, like a more typical tool.
            //
            // With real weapons it's fuzzier because the highest-level weapon isn't necessarily
            // appropriate for the situation. If there's one quick slot for Galaxy Sword and another
            // for Galaxy Hammer, then activating those slots should activate their *specific*
            // weapons respectively if both are in the inventory.
            //
            // So we match on the inferred "type" (scythe vs. weapon) and then for non-scythe
            // weapons specifically (and only those), give preference to exact matches before
            // sorting by level.
            var isScythe = data.InternalName.Contains("Scythe");
            var bestWeapon = player
                .Items.OfType<MeleeWeapon>()
                .Where(weapon => weapon.Name.Contains("Scythe") == isScythe)
                .OrderByDescending(weapon => !isScythe && weapon.QualifiedItemId == id)
                .ThenByDescending(weapon => weapon.getItemLevel())
                .FirstOrDefault();
            if (bestWeapon is not null)
            {
                return bestWeapon;
            }
        }
        var baseItem = data.GetBaseItem();
        return player
            .Items.Where(item => item is not null)
            .Where(item =>
                item.QualifiedItemId == id
                || ItemRegistry
                    .GetDataOrErrorItem(item.QualifiedItemId)
                    .GetBaseItem()
                    .QualifiedItemId == baseItem.QualifiedItemId
            )
            .OrderByDescending(item => item is Tool tool ? tool.UpgradeLevel : 0)
            .FirstOrDefault();
    }
}
