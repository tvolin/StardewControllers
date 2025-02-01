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
        Logger.Log(LogCategory.QuickSlots, "Quick slots invalidated.", LogLevel.Info);
        renderer.Invalidate();
        isDirty = true;
    }

    public void ShowDelayedActivation(SButton button)
    {
        Logger.Log(LogCategory.QuickSlots, $"Starting blink ({button}) for delayed activation.");
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
            Logger.Log(LogCategory.QuickSlots, $"Detected button press for {button}.");
            if (!slotItems.TryGetValue(button, out var item))
            {
                Logger.Log(LogCategory.QuickSlots, $"No item in the slot for {button}.");
                Sound.Play(config.Sound.ItemErrorSound);
                renderer.FlashError(button);
                continue;
            }
            var itemConfig = config.Items.QuickSlots[button];
            Logger.Log(
                LogCategory.QuickSlots,
                $"Found item in slot for {button}: ID = {itemConfig.IdType}:{itemConfig.Id}, "
                    + $"secondary action = {itemConfig.UseSecondaryAction}, "
                    + $"require confirmation = {itemConfig.RequireConfirmation}"
            );
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
        Logger.Log(LogCategory.QuickSlots, "Starting refresh of Quick Slots.");
        slotItems.Clear();
        foreach (var (button, slot) in config.Items.QuickSlots)
        {
            if (string.IsNullOrEmpty(slot.Id))
            {
                continue;
            }
            Logger.Log(
                LogCategory.QuickSlots,
                $"Item data for quick slot {button}: ID = {slot.IdType}:{slot.Id}, "
                    + $"secondary action = {slot.UseSecondaryAction}, "
                    + $"require confirmation = {slot.RequireConfirmation}"
            );
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
                Logger.Log(
                    LogCategory.QuickSlots,
                    $"Created quick slot item '{slottedItem.Title}' for {button} slot.",
                    LogLevel.Info
                );
                slotItems.Add(button, slottedItem);
            }
            else
            {
                Logger.Log(
                    LogCategory.QuickSlots,
                    $"No item or invalid item data for quick slot button {button}."
                );
            }
        }
        isDirty = false;
    }

    private Item? TryGetInventoryItem(string id)
    {
        Logger.Log(LogCategory.QuickSlots, $"Searching for inventory item equivalent to '{id}'...");
        if (ItemRegistry.GetData(id) is not { } data)
        {
            Logger.Log(
                LogCategory.QuickSlots,
                $"'{id}' does not have valid item data; aborting search."
            );
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
            Logger.Log(
                LogCategory.QuickSlots,
                $"Item '{id}' appears to be a weapon with (scythe = {isScythe})."
            );
            var bestWeapon = player
                .Items.OfType<MeleeWeapon>()
                .Where(weapon => weapon.Name.Contains("Scythe") == isScythe)
                .OrderByDescending(weapon => !isScythe && weapon.QualifiedItemId == id)
                .ThenByDescending(weapon => weapon.getItemLevel())
                .FirstOrDefault();
            if (bestWeapon is not null)
            {
                Logger.Log(
                    LogCategory.QuickSlots,
                    "Best weapon match in inventory is "
                        + $"{bestWeapon.Name} with ID {bestWeapon.QualifiedItemId}."
                );
                return bestWeapon;
            }
        }
        var baseItem = data.GetBaseItem();
        Logger.Log(
            LogCategory.QuickSlots,
            "Searching for regular item using base item "
                + $"{baseItem.InternalName} with ID {baseItem.QualifiedItemId}."
        );
        var match = player
            .Items.Where(item => item is not null)
            .Where(item =>
                item.QualifiedItemId == id
                || ItemRegistry
                    .GetDataOrErrorItem(item.QualifiedItemId)
                    .GetBaseItem()
                    .QualifiedItemId == baseItem.QualifiedItemId
            )
            .OrderByDescending(item => item is Tool tool ? tool.UpgradeLevel : 0)
            .ThenByDescending(item => item.Quality)
            .FirstOrDefault();
        Logger.Log(
            LogCategory.QuickSlots,
            $"Best match by quality/upgrade level is "
                + $"{match?.Name ?? "(nothing)"} with ID {match?.QualifiedItemId ?? "N/A"}."
        );
        return match;
    }
}
