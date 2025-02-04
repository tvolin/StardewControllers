using StarControl.Config;
using StarControl.Input;

namespace StarControl.Menus;

/// <summary>
/// Radial menu displaying the shortcuts set up in the
/// <see cref="Config.ItemsConfiguration.ModMenuPages"/>, as well as any mod-added pages.
/// </summary>
internal class ModMenu(
    IMenuToggle toggle,
    ModConfig config,
    ModMenuItem settingsItem,
    Action<ModMenuItemConfiguration> shortcutActivator,
    IInvalidatableList<IRadialMenuPage> additionalPages,
    IEnumerable<IRadialMenuItem> standaloneItems
) : IRadialMenu
{
    public IReadOnlyList<IRadialMenuPage> Pages
    {
        get
        {
            if (isDirty)
            {
                combinedPages = GetCombinedPages();
                isDirty = false;
            }
            return combinedPages;
        }
    }

    public int SelectedPageIndex { get; set; }

    public IMenuToggle Toggle { get; } = toggle;

    private IReadOnlyList<IRadialMenuPage> combinedPages = [];
    private bool isDirty = true;

    /// <summary>
    /// Retries the item (on any page) given its ID.
    /// </summary>
    /// <param name="id">The item ID.</param>
    /// <returns>The item matching the specified <paramref name="id"/>, or <c>null</c> if not
    /// found.</returns>
    public IRadialMenuItem? GetItem(string id)
    {
        return Pages
            .OfType<MenuPage<IRadialMenuItem>>()
            .SelectMany(page => page.InternalItems)
            .FirstOrDefault(item => item?.Id == id);
    }

    /// <summary>
    /// Recreates the items on the shortcut page (first page of this menu) and marks all other (mod) pages invalid,
    /// causing them to be recreated when next accessed.
    /// </summary>
    /// <remarks>
    /// Use when shortcuts have changed or may have changed, e.g. after the configuration was edited or upstream mod
    /// keybindings were changed.
    /// </remarks>
    public void Invalidate()
    {
        Logger.Log(LogCategory.Menus, "Mod menu invalidated.", LogLevel.Info);
        isDirty = true;
        additionalPages.Invalidate();
    }

    public void ResetSelectedPage()
    {
        Logger.Log(LogCategory.Menus, "Resetting page selection for mod menu.");
        for (int i = 0; i < Pages.Count; i++)
        {
            if (Pages[i].IsEmpty())
            {
                continue;
            }
            Logger.Log(LogCategory.Menus, $"Defaulting selection to non-empty page {i}");
            SelectedPageIndex = i;
            return;
        }
        Logger.Log(
            LogCategory.Menus,
            "Couldn't find non-empty page; defaulting selection to first page."
        );
        SelectedPageIndex = 0;
    }

    private IReadOnlyList<IRadialMenuPage> GetCombinedPages()
    {
        var pages = new List<IRadialMenuPage>();
        int pageIndex = 0;
        foreach (var pageConfig in config.Items.ModMenuPages)
        {
            Logger.Log(LogCategory.Menus, $"Creating page {pageIndex} of mod menu...");
            pages.Add(
                MenuPage.FromModItemConfigurations(
                    pageConfig,
                    standaloneItems,
                    shortcutActivator,
                    pageIndex == config.Items.SettingsItemPageIndex ? InsertSettingsItem : null
                )
            );
            pageIndex++;
        }
        pages.AddRange(additionalPages);
        Logger.Log(
            LogCategory.Menus,
            $"Added {config.Items.ModMenuPages.Count} user pages and {additionalPages.Count} "
                + "external pages to mod menu."
        );
        return pages;

        void InsertSettingsItem(List<IRadialMenuItem> items)
        {
            var index = Math.Clamp(config.Items.SettingsItemPositionIndex, 0, items.Count - 1);
            items.Insert(index, settingsItem);
            Logger.Log(
                LogCategory.Menus,
                $"Inserted built-in Mod Settings item at position {index}."
            );
        }
    }
}
