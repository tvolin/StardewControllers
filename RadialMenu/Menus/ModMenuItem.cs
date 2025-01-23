using Microsoft.Xna.Framework.Graphics;

namespace RadialMenu.Menus;

/// <summary>
/// An immutable menu item with user-defined properties.
/// </summary>
/// <param name="id">Unique ID for the item, specified by configuration.</param>
/// <param name="title">The <see cref="IRadialMenuItem.Title"/>.</param>
/// <param name="activate">A delegate for the <see cref="IRadialMenuItem.Activate"/> method.</param>
/// <param name="description">The <see cref="IRadialMenuItem.Description"/>.</param>
/// <param name="stackSize">The <see cref="IRadialMenuItem.StackSize"/>.</param>
/// <param name="quality">The <see cref="IRadialMenuItem.Quality"/>.</param>
/// <param name="texture">The <see cref="IRadialMenuItem.Texture"/>.</param>
/// <param name="sourceRectangle">The <see cref="IRadialMenuItem.SourceRectangle"/>.</param>
/// <param name="tintRectangle">The <see cref="IRadialMenuItem.TintRectangle"/>.</param>
/// <param name="tintColor">The <see cref="IRadialMenuItem.TintColor"/>.</param>
internal class ModMenuItem(
    string id,
    string title,
    Func<Farmer, DelayedActions, bool, ItemActivationResult> activate,
    string? description = null,
    int? stackSize = null,
    int? quality = null,
    Texture2D? texture = null,
    Rectangle? sourceRectangle = null,
    Rectangle? tintRectangle = null,
    Color? tintColor = null
) : IRadialMenuItem
{
    public string Id { get; } = id;
    public string Title { get; } = title;

    public string Description { get; } = description ?? "";

    public int? StackSize { get; } = stackSize;

    public int? Quality { get; } = quality;

    public Texture2D? Texture { get; } = texture;

    public Rectangle? SourceRectangle { get; } = sourceRectangle;

    public Rectangle? TintRectangle { get; } = tintRectangle;

    public Color? TintColor { get; } = tintColor;

    public ItemActivationResult Activate(
        Farmer who,
        DelayedActions delayedActions,
        bool secondaryAction
    )
    {
        return activate(who, delayedActions, secondaryAction);
    }
}
