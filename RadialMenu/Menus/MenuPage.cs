using RadialMenu.Config;
using RadialMenu.Graphics;
using StardewValley;

namespace RadialMenu.Menus;

/// <summary>
/// Helpers for creating standard menu pages.
/// </summary>
internal static class MenuPage
{
    /// <summary>
    /// Creates an <see cref="IRadialMenuPage"/> based off a player's inventory and paging parameters.
    /// </summary>
    /// <param name="who">The player whose inventory should be displayed.</param>
    /// <param name="startIndex">First index of the player's <see cref="Farmer.Items"/> to display.</param>
    /// <param name="count">Number of items to include on this page.</param>
    /// <param name="includeEmpty">Whether to include empty slots as null items.</param>
    public static IRadialMenuPage FromFarmerInventory(
        Farmer who,
        int startIndex,
        int count,
        bool includeEmpty
    )
    {
        var items = Enumerable
            .Range(startIndex, count)
            .Select(i => who.Items[i])
            .Where(item => includeEmpty || item is not null)
            .Select(item => item is not null ? new InventoryMenuItem(item) : null)
            .ToList();
        bool isSelected(InventoryMenuItem? menuItem) =>
            menuItem?.Item is { } item && item == who.Items[who.CurrentToolIndex];
        return new MenuPage<InventoryMenuItem>(items, isSelected);
    }

    /// <summary>
    /// Creates an <see cref="IRadialMenuPage"/> from the configured list of custom (keybind) shortcuts.
    /// </summary>
    /// <param name="itemConfigs">List of custom item configurations specifying names, keybinds, etc.</param>
    /// <param name="activator">Callback for when a custom item is activated.</param>
    /// <param name="transform">Optional callback to transform the item list before creating the page,
    /// e.g. to rearrange or insert items.</param>
    public static IRadialMenuPage FromModItemConfiguration(
        IEnumerable<ModMenuItemConfiguration> itemConfigs,
        Action<ModMenuItemConfiguration> activator,
        Action<List<ModMenuItem>>? transform = null
    )
    {
        var items = itemConfigs.Select(config => CreateModMenuItem(config, activator)).ToList();
        transform?.Invoke(items);
        return new MenuPage<ModMenuItem>(items, _ => false);
    }

    private static ModMenuItem CreateModMenuItem(
        ModMenuItemConfiguration config,
        Action<ModMenuItemConfiguration> activator
    )
    {
        var sprite = !string.IsNullOrEmpty(config.Icon.ItemId)
            ? Sprite.ForItemId(config.Icon.ItemId)
            : Sprite.TryLoad(config.Icon.TextureAssetPath, config.Icon.SourceRect);
        return new(
            id: config.Id,
            title: config.Name,
            description: config.Description,
            texture: sprite?.Texture,
            sourceRectangle: sprite?.SourceRect,
            activate: (who, delayedActions, _) =>
            {
                if (
                    delayedActions == DelayedActions.All
                    || (delayedActions != DelayedActions.None && config.EnableActivationDelay)
                )
                {
                    return ItemActivationResult.Delayed;
                }
                activator.Invoke(config);
                return ItemActivationResult.Custom;
            }
        );
    }
}

/// <summary>
/// Generic implementation of an <see cref="IRadialMenuPage"/>.
/// </summary>
/// <param name="items">The items on this page.</param>
/// <param name="isSelected">Predicate function to check whether a given item is selected.</param>
internal class MenuPage<T>(IReadOnlyList<T?> items, Predicate<T?> isSelected) : IRadialMenuPage
    where T : class, IRadialMenuItem
{
    public IReadOnlyList<IRadialMenuItem?> Items => items;

    public int SelectedItemIndex => GetSelectedIndex();

    internal IReadOnlyList<T?> InternalItems => items;

    private int GetSelectedIndex()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (isSelected(items[i]))
            {
                return i;
            }
        }
        return -1;
    }
}
