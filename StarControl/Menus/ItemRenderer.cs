using Microsoft.Xna.Framework.Graphics;
using StarControl.Config;
using StarControl.Graphics;

namespace StarControl.Menus;

internal static class ItemRenderer
{
    public static void Draw(
        SpriteBatch b,
        IRadialMenuItem item,
        Rectangle destinationRect,
        Styles styles,
        bool monogram = false,
        float scale = 1,
        float opacity = 1
    )
    {
        if (monogram)
        {
            Monogram.Draw(b, destinationRect, item.Title, Color.White * opacity);
        }
        else
        {
            // TODO: Shadows may end up in wrong place when using non-default sizes. Fix.
            var shadowTexture = Game1.shadowTexture;
            b.Draw(
                shadowTexture,
                destinationRect.Location.ToVector2() + new Vector2(32f * scale, 52f * scale),
                shadowTexture.Bounds,
                new Color(Color.Gray, 0.5f) * opacity,
                0.0f,
                new(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y),
                3f * scale,
                SpriteEffects.None,
                -0.0001f
            );
            // Tinting may be an overlay sprite, or a tint of the original sprite. In here, we
            // determine that by the nullness of both the rectangle and color. If a tint color
            // is specified, but no separate rectangle, then it means we need to tint the base
            // sprite since no overlay will be drawn; otherwise, draw the base normally.
            var baseColor = item.TintRectangle is null
                ? (item.TintColor ?? Color.White)
                : Color.White;
            b.Draw(item.Texture, destinationRect, item.SourceRectangle, baseColor * opacity);
            if (item.TintRectangle is { } tintRect && item.TintColor is { } tintColor)
            {
                b.Draw(item.Texture, destinationRect, tintRect, tintColor * opacity);
            }
        }
        if (item.Quality is { } quality && quality > 0)
        {
            // From StardewValley:Object.cs
            Rectangle qualitySourceRect =
                quality < 4 ? new(338 + (quality - 1) * 8, 400, 8, 8) : new(346, 392, 8, 8);
            var qualityIconPos = new Vector2(
                destinationRect.Left,
                destinationRect.Bottom - 16 * scale
            );
            b.Draw(
                Game1.mouseCursors,
                qualityIconPos,
                qualitySourceRect,
                Color.White * opacity,
                rotation: 0,
                origin: Vector2.Zero,
                scale: 3.0f * scale,
                effects: SpriteEffects.None,
                layerDepth: 0.1f
            );
        }
        if (item.StackSize is { } stackSize)
        {
            var stackTextScale = 3.0f * scale;
            var stackTextWidth = Utility.getWidthOfTinyDigitString(stackSize, stackTextScale);
            var stackLabelPos = new Vector2(
                destinationRect.Right - stackTextWidth,
                destinationRect.Bottom - 8 * scale
            );
            Utility.drawTinyDigits(
                stackSize,
                b,
                stackLabelPos,
                stackTextScale,
                layerDepth: 0.1f,
                styles.StackSizeColor * opacity
            );
        }
    }
}
