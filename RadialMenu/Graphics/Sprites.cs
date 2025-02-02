namespace RadialMenu.Graphics;

/// <summary>
/// Common sprites used in the mod.
/// </summary>
public static class Sprites
{
    /// <summary>
    /// Asset path for the mod's own UI tile sheet.
    /// </summary>
    public const string UI_TEXTURE_PATH = "Mods/focustense.RadialMenu/Sprites/UI";

    /// <summary>
    /// Gets the default error item sprite used for missing items.
    /// </summary>
    public static Sprite Error() => Sprite.ForItemId("Error_Invalid");

    /// <summary>
    /// Sprite used for the mod settings menu item.
    /// </summary>
    public static Sprite? Settings() => Sprite.TryLoad(UI_TEXTURE_PATH, new(80, 0, 16, 16));
}
