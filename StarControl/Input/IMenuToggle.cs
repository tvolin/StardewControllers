namespace StarControl.Input;

/// <summary>
/// Monitors the state of a single menu's toggle, i.e. the keybind combination that controls opening
/// and closing the menu.
/// </summary>
public interface IMenuToggle
{
    /// <summary>
    /// The current state of the toggle, indicating whether the corresponding menu should be open.
    /// </summary>
    MenuToggleState State { get; }

    /// <summary>
    /// Begins a forced suppression of the toggle's bound button, if any forced suppression could be
    /// required, i.e. if the combination of toggle mode and button binding are at risk of leaking
    /// presses into a subsequent UI.
    /// </summary>
    void ForceButtonSuppression();

    /// <summary>
    /// Enters a forced off state (<see cref="MenuToggleState.Suppressed"/>) that is not removed
    /// until the toggle keybind is released.
    /// </summary>
    /// <remarks>
    /// This is normally used after an item activation, and only affects menus set up in hold mode,
    /// since suppression is not required for toggle-mode menus.
    /// </remarks>
    void ForceOff();

    /// <summary>
    /// Gets whether the toggle keybind is considered to be on the right side of the controller.
    /// </summary>
    /// <returns><c>true</c> if the toggle button is unambiguously on the right side of a
    /// conventional controller layout, i.e. using <b>only</b> the A/B/X/Y buttons, the right
    /// thumbstick, right trigger and/or right shoulder/button; <c>false</c> if any part of the
    /// chord is in the center (start/select) or on the left side (d-pad, left stick, left trigger,
    /// left shoulder/button, etc.).</returns>
    bool IsRightSided();

    /// <summary>
    /// Performs logic that needs to run before the game's main update loop, such as global input
    /// suppression.
    /// </summary>
    /// <param name="interactive">Whether or not the user is allowed to interact with the menu, i.e.
    /// the player is being controlled and no other menu is active.</param>
    void PreUpdate(bool interactive);

    /// <summary>
    /// Reads the current input state and updates the <see cref="MenuToggle.State"/> if necessary.
    /// </summary>
    /// <param name="allowOn">Whether the <see cref="MenuToggle.State"/> is allowed to transition to
    /// <see cref="MenuToggleState.On"/> in this frame; if <c>false</c>, for example due to the
    /// player being locked in an animation, then a positive input combination will only transition
    /// to <see cref="MenuToggleState.Wait"/>.</param>
    void Update(bool allowOn);
}
