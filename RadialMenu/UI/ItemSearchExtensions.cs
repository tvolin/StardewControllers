using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal static class ItemSearchExtensions
{
    public static IEnumerable<ParsedItemData> Search(
        this IEnumerable<ParsedItemData> allItems,
        string searchText,
        CancellationToken? cancellationToken = null
    )
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return allItems;
        }
        if (ItemRegistry.IsQualifiedItemId(searchText))
        {
            var exactItem = ItemRegistry.GetData(searchText);
            return exactItem is not null ? [exactItem] : [];
        }
        if (int.TryParse(searchText, out var objectId))
        {
            var exactItem = ItemRegistry.GetData("(O)" + objectId);
            return exactItem is not null ? [exactItem] : [];
        }
        var matches = allItems.Where(item =>
        {
            cancellationToken?.ThrowIfCancellationRequested();
            return item.DisplayName.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
        });
        return matches;
    }
}
