namespace RadialMenu.UI;

/// <summary>
/// Wrapper class to hold the <see cref="IViewEngine"/> instance used for the mod.
/// </summary>
/// <remarks>
/// Created as an alternative to full-fledged DI so that the view engine is accessible from within
/// view models themselves, i.e. to open submenus, without having to pass dependencies around
/// multiple levels.
/// </remarks>
internal static class ViewEngine
{
    /// <summary>
    /// The mod's view engine.
    /// </summary>
    public static IViewEngine? Instance { get; set; }

    /// <summary>
    /// Prefix for view names provided to <see cref="OpenChildMenu"/>.
    /// </summary>
    public static string ViewAssetPrefix { get; set; } = "";

    /// <summary>
    /// Creates a menu and opens it as the child of whichever menu is topmost.
    /// </summary>
    /// <param name="viewName">Local name of the view, excluding the path prefix.</param>
    /// <param name="context">View model for the menu.</param>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="Instance"/> has not
    /// been assigned by the mod's entry point.</exception>
    public static void OpenChildMenu(string viewName, object? context)
    {
        if (Instance is null)
        {
            throw new InvalidOperationException(
                $"ViewEngine Instance is not set up; StardewUI is probably not installed."
            );
        }
        var assetName = ViewAssetPrefix + '/' + viewName;
        var menu = Instance.CreateMenuFromAsset(assetName, context);
        var parent = Game1.activeClickableMenu;
        for (; parent?.GetChildMenu() is not null; parent = parent.GetChildMenu()) { }
        if (parent is not null)
        {
            parent.SetChildMenu(menu);
        }
        else
        {
            Game1.activeClickableMenu = menu;
        }
    }
}
