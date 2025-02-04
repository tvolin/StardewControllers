using StarControl.Menus;

namespace StarControl.Api;

/// <summary>
/// Public API for mod integrations.
/// </summary>
public interface IRadialMenuApi
{
    /// <summary>
    /// Forces a previously-registered page to be recreated the next time the menu is about to be shown.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While a custom <see cref="IRadialMenuPage"/> can always decide when to update its contents, sometimes this is
    /// not convenient; for example, if the contents of a menu page are generated from some other data (like player
    /// inventory) and cached, or if activation of an item in one page could indirectly affect the contents of a
    /// different page. Explicit invalidation allows each page to cache its data for performance but still remain up to
    /// date when conditions change.
    /// </para>
    /// <para>
    /// The <paramref name="mod"/> and <paramref name="id"/> must already have been registered through
    /// <see cref="RegisterCustomMenuPage"/>, otherwise the request will be ignored.
    /// </para>
    /// </remarks>
    /// <param name="mod">Manifest for the mod that registered the menu.</param>
    /// <param name="id">Unique (per mod) ID for the page to invalidate.</param>
    void InvalidatePage(IManifest mod, string id);

    /// <summary>
    /// Registers a new page to be made available in the Mod Menu (default: right trigger).
    /// </summary>
    /// <param name="mod">Manifest for the mod that will own the menu.</param>
    /// <param name="id">Unique (per mod) ID for the page. Registering a page with a previously-used ID will overwrite
    /// the previous page.</param>
    /// <param name="factory">Factory for creating the page.</param>
    void RegisterCustomMenuPage(IManifest mod, string id, IRadialMenuPageFactory factory);

    /// <summary>
    /// Registers items to be available in the user's item library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Items registered this way are not immediately associated with a menu page, and do not appear in an enforced order;
    /// instead, they can be assigned to Mod Menu pages and Quick Slots in Star Control's settings menu.
    /// </para>
    /// <para>
    /// Mods should use this registration method when they wish to simply provide access to their features without
    /// enforcing a particular order or hierarchy on the player.
    /// </para>
    /// </remarks>
    /// <param name="mod">Manifest for the mod providing the item.</param>
    /// <param name="items">The items to register.</param>
    void RegisterItems(IManifest mod, IEnumerable<IRadialMenuItem> items);
}

/// <summary>
/// Factory for creating an <see cref="IRadialMenuPage"/> which adds mod-specific menu content.
/// </summary>
public interface IRadialMenuPageFactory
{
    /// <summary>
    /// Creates the page for a given player.
    /// </summary>
    /// <remarks>
    /// This method may be invoked multiple times in case of invalidation, either implicitly due to a config change in
    /// RadialMenu or explicitly by <see cref="IRadialMenuApi.InvalidateCustomMenuPage"/>. If page creation is an
    /// expensive process then callers are allowed to return a cached result, but if doing so, must ensure that the
    /// result is unique per player/screen.
    /// </remarks>
    /// <param name="who">The player for whom the page will be displayed. Callers should use this whenever possible
    /// instead of <see cref="Game1.player"/> in case of co-op play.</param>
    IRadialMenuPage CreatePage(Farmer who);
}
