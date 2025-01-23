using RadialMenu.Config;
using RadialMenu.Input;

namespace RadialMenu.Menus;

/// <summary>
/// Radial menu displaying the shortcuts set up in the
/// <see cref="Config.ItemsConfiguration.ModMenuPages"/>, as well as any mod-added pages.
/// </summary>
internal class ModMenu(
    IMenuToggle toggle,
    ModConfig config,
    ModMenuItem settingsItem,
    Action<ModMenuItemConfiguration> shortcutActivator,
    IInvalidatableList<IRadialMenuPage> additionalPages
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
            .OfType<MenuPage<ModMenuItem>>()
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
        isDirty = true;
        additionalPages.Invalidate();
    }

    public void ResetSelectedPage()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            if (!Pages[i].IsEmpty())
            {
                SelectedPageIndex = i;
                return;
            }
        }
        SelectedPageIndex = 0;
    }

    private IReadOnlyList<IRadialMenuPage> GetCombinedPages()
    {
        var pages = new List<IRadialMenuPage>();
        int pageIndex = 0;
        foreach (var pageConfig in config.Items.ModMenuPages)
        {
            pages.Add(
                MenuPage.FromModItemConfiguration(
                    pageConfig,
                    shortcutActivator,
                    pageIndex == config.Items.SettingsItemPageIndex ? InsertSettingsItem : null
                )
            );
            pageIndex++;
        }
        pages.AddRange(additionalPages);
        return pages;

        void InsertSettingsItem(List<ModMenuItem> items)
        {
            var index = Math.Clamp(config.Items.SettingsItemPositionIndex, 0, items.Count - 1);
            items.Insert(index, settingsItem);
        }
    }
}
