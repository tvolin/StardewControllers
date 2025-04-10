﻿using Microsoft.Xna.Framework;

namespace StarControl.Config;

/// <summary>
/// Configures the visual appearance of the menu.
/// </summary>
/// <remarks>
/// All dimensions are in pixels unless otherwise specified.
/// </remarks>
public class Styles : IConfigEquatable<Styles>
{
    /// <summary>
    /// Background color of the inner area where a preview of the current selection is displayed.
    /// </summary>
    public HexColor InnerBackgroundColor { get; set; } = new(new Color(0.05f, 0.05f, 0.05f, 0.9f));

    /// <summary>
    /// Radius of the inner area where a preview of the current selection is displayed.
    /// </summary>
    public float InnerRadius { get; set; } = 300;

    /// <summary>
    /// Background color of the outer area where menu items appear.
    /// </summary>
    public HexColor OuterBackgroundColor { get; set; } = new(new Color(0.91f, 0.73f, 0.49f, 0.9f));

    /// <summary>
    /// Radius of the outer area where menu items appear.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This affects the "thickness" of the selection ring. The radius is not measured from the
    /// center of the screen, like it is for <see cref="InnerRadius"/>, but instead from the inner
    /// border to the outer border.
    /// </para>
    /// <para>
    /// For best results, in order to accommodate unusual image aspect ratios and be able to display
    /// additional content like stack size and quality, the value should be roughly 65-75% larger
    /// than the configured <see cref="SelectionSpriteHeight"/>.
    /// </para>
    /// </remarks>
    public float OuterRadius { get; set; } = 110;

    /// <summary>
    /// Contextual background color for the part of the outer menu that matches the player's current
    /// tool selection.
    /// This is not an overlay color, the selection color completely replaces the
    /// <see cref="OuterBackgroundColor"/> for that slice.
    /// </summary>
    public HexColor SelectionColor { get; set; } = new(new Color(0.85f, 0.55f, 0.15f, 0.9f));

    /// <summary>
    /// Contextual background color for the part of the outer menu that is highlighted (focused).
    /// This is not an overlay color, the highlight completely replaces the
    /// <see cref="OuterBackgroundColor"/> for that slice.
    /// </summary>
    public HexColor HighlightColor { get; set; } = new(Color.RoyalBlue);

    /// <summary>
    /// Empty space between the inner circle and outer ring.
    /// </summary>
    /// <remarks>
    /// The gap is not required for correct display; it is an aesthetic choice and can be removed
    /// entirely (set to <c>0</c>) if desired.
    /// </remarks>
    public float GapWidth { get; set; } = 8;

    /// <summary>
    /// Suggested height of a sprite (icon) within the menu ring.
    /// </summary>
    /// <remarks>
    /// Most sprites will typically be drawn with exactly this height; however, the height may be
    /// reduced in order to fit some sprites with very wide aspect ratios.
    /// </remarks>
    public int MenuSpriteHeight { get; set; } = 64;

    /// <summary>
    /// Text color for the stack size displayed for stackable items. Generally applicable to the
    /// Inventory menu only.
    /// </summary>
    /// <remarks>
    /// As with other stack-size labels in Stardew, the text is drawn with an outline/shadow; this
    /// color represents the inner color and should therefore typically be a very light color, if
    /// not the default of <see cref="Color.White"/>.
    /// </remarks>
    public HexColor StackSizeColor { get; set; } = new(Color.White);

    /// <summary>
    /// Distance between the tip of the menu cursor (pointer showing the precise angle of the
    /// thumbstick) and the outer ring.
    /// </summary>
    public float CursorDistance { get; set; } = 8;

    /// <summary>
    /// Size of the cursor indicating the thumbstick angle.
    /// </summary>
    /// <remarks>
    /// This is the height of the cursor when it is facing straight up. If the cursor is drawn as a
    /// simple triangle, it is the distance from base to tip.
    /// </remarks>
    public float CursorSize { get; set; } = 32;

    /// <summary>
    /// Fill color for the cursor indicating the thumbstick angle.
    /// </summary>
    public HexColor CursorColor { get; set; } = new(Color.LightGray);

    /// <summary>
    /// Height of the zoomed-in sprite displayed in the selection preview (inner circle). Typically
    /// 2x the <see cref="MenuSpriteHeight"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="MenuSpriteHeight"/>, this is a guaranteed height and not a suggestion. The
    /// preview area is assumed to have plenty of space to accommodate larger sprites, and therefore
    /// there are no explicit wideness checks.
    /// </remarks>
    public int SelectionSpriteHeight { get; set; } = 128;

    /// <summary>
    /// Color of the large title text displayed in the selection preview.
    /// </summary>
    public HexColor SelectionTitleColor { get; set; } = new(Color.White);

    /// <summary>
    /// Color of the smaller description text displayed underneath the title in the selection
    /// preview.
    /// </summary>
    public HexColor SelectionDescriptionColor { get; set; } = new(Color.LightGray);

    /// <inheritdoc />
    public bool Equals(Styles? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return InnerBackgroundColor.Equals(other.InnerBackgroundColor)
            && InnerRadius.Equals(other.InnerRadius)
            && OuterBackgroundColor.Equals(other.OuterBackgroundColor)
            && OuterRadius.Equals(other.OuterRadius)
            && SelectionColor.Equals(other.SelectionColor)
            && HighlightColor.Equals(other.HighlightColor)
            && GapWidth.Equals(other.GapWidth)
            && MenuSpriteHeight == other.MenuSpriteHeight
            && StackSizeColor.Equals(other.StackSizeColor)
            && CursorDistance.Equals(other.CursorDistance)
            && CursorSize.Equals(other.CursorSize)
            && CursorColor.Equals(other.CursorColor)
            && SelectionSpriteHeight == other.SelectionSpriteHeight
            && SelectionTitleColor.Equals(other.SelectionTitleColor)
            && SelectionDescriptionColor.Equals(other.SelectionDescriptionColor);
    }
}
