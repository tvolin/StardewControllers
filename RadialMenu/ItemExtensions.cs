using StardewValley.ItemTypeDefinitions;

namespace RadialMenu;

internal static class ItemExtensions
{
    public static Item? FindUsableItem(this IEnumerable<Item?> items, string qualifiedItemId)
    {
        return items.FirstOrDefault(item =>
            item is not null
            && (
                item.QualifiedItemId == qualifiedItemId
                || GetBaseItem(
                    ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId)
                ).QualifiedItemId == qualifiedItemId
            )
        );
    }

    public static ParsedItemData GetBaseItem(this ParsedItemData item)
    {
        if (!Game1.toolData.TryGetValue(item.ItemId, out var toolData))
        {
            return item;
        }
        var result = item;
        while (
            !string.IsNullOrEmpty(toolData.ConventionalUpgradeFrom)
            && ItemRegistry.GetData(toolData.ConventionalUpgradeFrom) is { } baseToolItem
            && Game1.toolData.TryGetValue(baseToolItem.ItemId, out var baseToolData)
        )
        {
            result = baseToolItem;
            toolData = baseToolData;
        }
        return result;
    }
}
