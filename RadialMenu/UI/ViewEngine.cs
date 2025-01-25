using StardewValley.Menus;

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
    /// Whether the StardewUI mod appears to be installed and correctly initialized.
    /// </summary>
    public static bool IsInstalled => Instance is not null;

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
    /// <returns>The menu controller for further customization.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="Instance"/> has not
    /// been assigned by the mod's entry point.</exception>
    public static IMenuController OpenChildMenu(string viewName, object? context)
    {
        var controller = CreateMenuController(viewName, context);
        var parent = Game1.activeClickableMenu;
        for (; parent?.GetChildMenu() is not null; parent = parent.GetChildMenu()) { }
        if (parent is not null)
        {
            controller.CloseOnOutsideClick = true;
            parent.SetChildMenu(controller.Menu);
        }
        else
        {
            Game1.activeClickableMenu = controller.Menu;
        }
        return controller;
    }

    /// <summary>
    /// Creates a menu and opens it as the top-level menu, replacing any other open menu.
    /// </summary>
    /// <param name="viewName">Local name of the view, excluding the path prefix.</param>
    /// <param name="context">View model for the menu.</param>
    /// <returns>The menu controller for further customization.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="Instance"/> has not
    /// been assigned by the mod's entry point.</exception>
    public static IMenuController OpenRootMenu(string viewName, object? context)
    {
        var controller = CreateMenuController(viewName, context);
        if (Game1.activeClickableMenu is TitleMenu)
        {
            TitleMenu.subMenu = controller.Menu;
        }
        else
        {
            Game1.activeClickableMenu = controller.Menu;
        }
        return controller;
    }

    private static IMenuController CreateMenuController(string viewName, object? context)
    {
        if (Instance is null)
        {
            throw new InvalidOperationException(
                $"ViewEngine Instance is not set up; StardewUI is probably not installed."
            );
        }
        var assetName = ViewAssetPrefix + '/' + viewName;
        return Instance.CreateMenuControllerFromAsset(assetName, context);
    }
}
