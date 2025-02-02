namespace RadialMenu.Config;

/// <summary>
/// Configuration for a Quick Slot that binds actions to a single button press while one of the
/// radial menus is open.
/// </summary>
public class QuickSlotConfiguration : IConfigEquatable<QuickSlotConfiguration>
{
    /// <summary>
    /// The type of ID that the <see cref="Id"/> refers to.
    /// </summary>
    public ItemIdType IdType { get; set; }

    /// <summary>
    /// Identifies which item to select or use. Can be the ID of a regular game item or of a Mod
    /// Menu action, depending on the <see cref="IdType"/>.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Whether to display a confirmation dialog before activating the item in this slot.
    /// </summary>
    public bool RequireConfirmation { get; set; }

    /// <summary>
    /// Whether to perform the item's secondary action (generally "Select") instead of its primary
    /// action (generally "Consume/Use"), if a secondary action exists. If the item only has one
    /// possible action, this setting is ignored.
    /// </summary>
    public bool UseSecondaryAction { get; set; }

    /// <summary>
    /// Whether the quick slot should trigger, and replace any vanilla behavior, when its button is
    /// pressed <em>without</em> the controller menu open.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enabling this effectively means that the button is "remapped" to the quick slot's function,
    /// which means fewer button presses to activate, but may interfere with built-in game controls
    /// or other mods; for example, the d-pad buttons are normally used in-world for character
    /// movement, but for players who never use the d-pad for that, and instead rely solely on the
    /// left analog stick, it might be more useful to have the d-pad slots always active.
    /// </para>
    /// <para>
    /// This setting is per-slot because every player is likely to have a different idea of which
    /// slots should be "always on".
    /// </para>
    /// <para>
    /// Applies to the slot itself, not the item in the slot. If the item cannot be used - for
    /// example, if a tool is not in the inventory - then the default/vanilla behavior for the
    /// slot's button will still be suppressed.
    /// </para>
    /// </remarks>
    public bool ActiveOutsideMenu { get; set; }

    /// <inheritdoc />
    public bool Equals(QuickSlotConfiguration? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return IdType == other.IdType
            && Id == other.Id
            && RequireConfirmation == other.RequireConfirmation
            && UseSecondaryAction == other.UseSecondaryAction
            && ActiveOutsideMenu == other.ActiveOutsideMenu;
    }
}
