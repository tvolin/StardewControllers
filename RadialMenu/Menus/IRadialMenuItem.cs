using Microsoft.Xna.Framework.Graphics;

namespace RadialMenu.Menus;

/// <summary>
/// Describes a single item on an <see cref="IRadialMenuPage"/>.
/// </summary>
public interface IRadialMenuItem
{
    /// <summary>
    /// A unique ID for this item.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Providing a non-empty ID allows the item to be assigned to one of the player's Quick Slots.
    /// If this property is implemented, the ID <b>must</b> be stable across multiple game launches,
    /// as its value will be saved to the user's configuration.
    /// </para>
    /// <para>
    /// A typically good choice for an ID is the providing mod's unique ID, followed by the name of
    /// the feature; e.g. <c>focustense.StarControl.Settings</c> to open Star Control's settings.
    /// </para>
    /// </remarks>
    string Id => "";

    /// <summary>
    /// The item title, displayed in large text at the top of the info area when focused.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Additional description text displayed underneath the <see cref="Title"/>.
    /// </summary>
    /// <remarks>
    /// Can be used to display item stats, effect info or simply flavor text.
    /// </remarks>
    string Description { get; }

    /// <summary>
    /// The amount available.
    /// </summary>
    /// <remarks>
    /// For inventory, this is the actual <see cref="Item.Stack"/>. For other types of menu items,
    /// it can be used to indicate any "number of uses available". Any non-<c>null</c> value will
    /// render as digits at the bottom-right of the item icon/sprite in the menu.
    /// </remarks>
    int? StackSize => null;

    /// <summary>
    /// The item's quality, from 0 (base) to 3 (iridium).
    /// </summary>
    /// <remarks>
    /// For non-<c>null</c> values, the corresponding star will be drawn to the bottom-left of the
    /// item's icon determined by its <see cref="Texture"/> and <see cref="SourceRectangle"/>.
    /// </remarks>
    int? Quality => null;

    /// <summary>
    /// The texture (sprite sheet) containing the item's icon to display in the menu.
    /// </summary>
    /// <remarks>
    /// If not specified, the icon area will instead display monogram text based on the
    /// <see cref="Title"/>.
    /// </remarks>
    Texture2D? Texture => null;

    /// <summary>
    /// The area within the <see cref="Texture"/> containing this specific item's icon/sprite that
    /// should be displayed in the menu.
    /// </summary>
    /// <remarks>
    /// If not specified, the entire <see cref="Texture"/> will be used.
    /// </remarks>
    Rectangle? SourceRectangle => null;

    /// <summary>
    /// Optional separate area within the <see cref="Texture"/> providing an overlay sprite to
    /// render with <see cref="TintColor"/>.
    /// </summary>
    /// <remarks>
    /// Some "colored items" define both a base sprite and a sparser, mostly-transparent tint or
    /// overlay sprite so that the tint can be applied to only specific regions. If this is set,
    /// then any <see cref="TintColor"/> will apply only to the overlay and <em>not</em> the base
    /// sprite contained in <see cref="SourceRectangle"/>.
    /// </remarks>
    Rectangle? TintRectangle => null;

    /// <summary>
    /// Tint color, if the item icon/sprite should be drawn in a specific color.
    /// </summary>
    /// <remarks>
    /// If <see cref="TintRectangle"/> is specified, this applies to the tintable region; otherwise,
    /// it applies directly to the base sprite in <see cref="SourceRectangle"/>.
    /// </remarks>
    Color? TintColor => null;

    /// <summary>
    /// Attempts to activate the menu item, i.e. perform its associated action.
    /// </summary>
    /// <param name="who">The player who activated the item; generally,
    /// <see cref="Game1.player"/>.</param>
    /// <param name="delayedActions">The types of actions which should result in a
    /// <see cref="ItemActivationResult.Delayed"/> outcome and the actual action being
    /// skipped.</param>
    /// <param name="secondaryAction">Whether to perform the item's secondary action instead of the
    /// primary action (typically "Select" instead of "Use"), if one is available. If there is no
    /// secondary action, this parameter can be ignored.</param>
    /// <returns>A result that describes what action, if any, was performed.</returns>
    ItemActivationResult Activate(
        Farmer who,
        DelayedActions delayedActions,
        bool secondaryAction = false
    );

    /// <summary>
    /// Chooses the sound to play when activating an item.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementing this is optional; sound cues are typically determined by Star Control's mod
    /// settings. Authors only need to override it if they intend to provide a custom, per-item
    /// sound, or disable the default sound even if the user has sounds enabled.
    /// </para>
    /// <para>
    /// Custom sounds will not play if the user has all sound muted in Star Control's settings.
    /// </para>
    /// </remarks>
    /// <param name="who">The player who activated the item; generally,
    /// <see cref="Game1.player"/>.</param>
    /// <param name="secondaryAction">Whether it is the item's secondary action instead of the
    /// primary action (typically "Select" instead of "Use") that is about to be performed.</param>
    /// <param name="defaultSound">The default sound that would play if not overridden; implementers
    /// should return this value if not changing the default.</param>
    /// <returns>The name of the sound cue to play on activation, or <c>null</c> or an empty string
    /// to play no sound.</returns>
    string? GetActivationSound(Farmer who, bool secondaryAction, string defaultSound) =>
        defaultSound;
}
