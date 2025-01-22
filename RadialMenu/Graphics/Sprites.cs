namespace RadialMenu.Graphics;

/// <summary>
/// Common sprites used in the mod.
/// </summary>
public static class Sprites
{
    /// <summary>
    /// Sprite used for the mod settings menu item.
    /// </summary>
    public static Sprite? Settings() =>
        Sprite.TryLoad("Mods/focustense.RadialMenu/Sprites/UI", new(80, 0, 16, 16));
}
