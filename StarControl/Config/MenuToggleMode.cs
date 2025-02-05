namespace StarControl.Config;

/// <summary>
/// Options for menu opening and persistence; specifies how the menus react to their primary hotkeys.
/// </summary>
/// <remarks>
/// Menus are always closed when an item is activated; this setting determines what happens before
/// said activation.
/// </remarks>
public enum MenuToggleMode
{
    /// <summary>
    /// The menu is held open for as long as the hotkey is held down.
    /// </summary>
    /// <remarks>
    /// This is the default and original Star Control behavior.
    /// </remarks>
    Hold,

    /// <summary>
    /// The menu stays open until the hotkey is pressed a second time.
    /// </summary>
    Toggle,
}
