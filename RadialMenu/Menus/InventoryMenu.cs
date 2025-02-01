using RadialMenu.Config;
using RadialMenu.Input;
using StardewValley;

namespace RadialMenu.Menus;

/// <summary>
/// An inventory radial menu for a single player.
/// </summary>
/// <param name="toggle">The toggle controller for this menu.</param>
/// <param name="who">The player whose inventory will be displayed.</param>
/// <param name="itemsConfig">Configuration for the menu items.</param>
internal class InventoryMenu(IMenuToggle toggle, Farmer who, ItemsConfiguration itemsConfig)
    : IRadialMenu
{
    public IReadOnlyList<IRadialMenuPage> Pages
    {
        get
        {
            RefreshIfDirty();
            return pages;
        }
    }

    public int SelectedPageIndex { get; set; }

    public IMenuToggle Toggle => toggle;

    private readonly List<IRadialMenuPage> pages = [];

    private bool isDirty = true;

    /// <inheritdoc />
    public void Invalidate()
    {
        Logger.Log(LogCategory.Menus, "Inventory menu invalidated.", LogLevel.Info);
        isDirty = true;
    }

    public void ResetSelectedPage()
    {
        Logger.Log(LogCategory.Menus, "Resetting page selection for inventory menu.");
        RefreshIfDirty();
        // In case there's any inconsistency between the menu and the player's inventory, we'll first try to match on
        // the item itself. However, there might be no current item if the player just consumed the last of a stack, or
        // put it into a chest, etc., so if this happens, default to the first item on the current "page", which is
        // better than doing nothing.
        var currentItem = who.CurrentItem ?? who.Items.FirstOrDefault(i => i is not null);
        Logger.Log(LogCategory.Menus, $"Current item is {currentItem?.Name ?? "(nothing)"}");
        if (currentItem is not null)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (
                    pages[i]
                        .Items.Any(item =>
                            item is InventoryMenuItem menuItem && menuItem.Item == currentItem
                        )
                )
                {
                    Logger.Log(LogCategory.Menus, $"Found current item on page {i}");
                    SelectedPageIndex = i;
                    return;
                }
            }
        }
        Logger.Log(
            LogCategory.Menus,
            "Couldn't find current item on any page. "
                + "This is expected when there is no current item; otherwise it may be a bug."
        );
        // We shouldn't normally reach this, but if we did, then it means we can't make a useful decision. We'd like to
        // display something other than an empty page, so the first choice is to just stay on the current page (if it's
        // non-empty) or, failing that, go to the first non-empty page (if we can).
        if ((this as IRadialMenu).GetSelectedPage()?.IsEmpty() == true)
        {
            Logger.Log(
                LogCategory.Menus,
                "Selected page is empty; will try to find a non-empty page."
            );
            SelectedPageIndex = Math.Max(pages.FindIndex(page => page.Items.AnyNotNull()), 0);
            Logger.Log(LogCategory.Menus, $"Set page index to {SelectedPageIndex}.");
        }
    }

    private void RefreshIfDirty()
    {
        if (!isDirty)
        {
            return;
        }
        Logger.Log(LogCategory.Menus, "Starting refresh of inventory menu.");
        pages.Clear();
        var pageSize = itemsConfig.InventoryPageSize;
        for (int i = 0; i < who.Items.Count; i += pageSize)
        {
            var actualCount = Math.Min(pageSize, who.Items.Count - i);
            var page = MenuPage.FromFarmerInventory(
                who,
                i,
                actualCount,
                itemsConfig.ShowInventoryBlanks
            );
            pages.Add(page);
            Logger.Log(
                LogCategory.Menus,
                $"Added page {pages.Count} with {page.Items.Count} items."
            );
        }
        isDirty = false;
    }
}
