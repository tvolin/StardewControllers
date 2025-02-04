namespace RadialMenu.Menus;

/// <summary>
/// Specifies which action associated with a given menu item should be performed.
/// </summary>
/// <remarks>
/// The meanings of each type are entirely specific to the implementation of the
/// <see cref="IRadialMenuItem"/> and the context in which it is performed. The types simply reflect
/// how the player selected the item - that is, which button was used to activate it.
/// </remarks>
public enum ItemActivationType
{
    /// <summary>
    /// The item's primary action.
    /// </summary>
    /// <remarks>
    /// Primary means that the item was activated from a radial menu or quick slot, via the user
    /// pressing the configured <see cref="Config.InputConfiguration.PrimaryActionButton"/>.
    /// Typical primary actions including eating a food item, warping with a totem, or selecting a
    /// tool such as the Axe or Pickaxe.
    /// </remarks>
    Primary,

    /// <summary>
    /// The item's secondary action.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Secondary means that the item was activated from a radial menu or quick slot, via the user
    /// pressing the configured <see cref="Config.InputConfiguration.SecondaryActionButton"/>.
    /// </para>
    /// <para>
    /// Secondary actions may be the same as the <see cref="Primary"/> action, or may be an
    /// alternate use of the same item, such as selecting (not using) a consumable item for the
    /// purpose of gifting or placing into a machine.
    /// </para>
    /// </remarks>
    Secondary,

    /// <summary>
    /// The item's instant (one-button) action, if it has one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instant actions are used in button remapping and can also be thought of as "tool use". For
    /// example, an actual <see cref="Tool"/> item would perform the tool's actual <em>function</em>
    /// as if the player had pressed the tool-use button (axe chop, sword swing, etc.).Other
    /// consumable items, such as food, might behave similarly to the <see cref="Primary"/> action,
    /// except that if their primary action involves any delay or confirmation aside from their
    /// regular animation, then that delay/confirmation should be skipped.
    /// </para>
    /// <para>
    /// Instant actions are only performed for items that are set up in the player's instant slot
    /// (button remap), and pressed while the player is free and interacting with the world, i.e.
    /// when <b>no</b> menu is open including the radial menu.
    /// </para>
    /// </remarks>
    Instant,
}
