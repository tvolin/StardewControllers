namespace RadialMenu.Input;

/// <summary>
/// The state of a <see cref="MenuToggle"/> for a single menu.
/// </summary>
public enum MenuToggleState
{
    /// <summary>
    /// The toggle button is off, and the menu should not be shown.
    /// </summary>
    Off,

    /// <summary>
    /// The toggle button is on, or in the case of <see cref="Config.MenuToggleMode.Toggle"/>, was
    /// pressed at least once, but the input has not been accepted yet, possibly due to the player
    /// still performing an action or due to a different menu being open.
    /// </summary>
    Wait,

    /// <summary>
    /// The toggle button is on and the input has been accepted. In the
    /// <see cref="Config.MenuToggleMode.Toggle"/> mode, this means another press will transition
    /// back to <see cref="Off"/>.
    /// </summary>
    On,
}
