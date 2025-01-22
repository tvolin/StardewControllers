using Microsoft.Xna.Framework.Graphics;

namespace RadialMenu.Graphics;

/// <summary>
/// Utility for creating circle texture of an arbitrary pixel size.
/// </summary>
public static class ShapeTexture
{
    /// <summary>
    /// Creates a circle texture with a given radius.
    /// </summary>
    /// <remarks>
    /// The final width and height will be 1 less than twice the <paramref name="radius"/>.
    /// </remarks>
    /// <param name="radius">The circle radius.</param>
    /// <param name="filled">Whether to fill the circle, i.e. instead of just the border.</param>
    /// <param name="graphicsDevice">Optional graphics device on which to create the texture; uses
    /// the game's default if not specified.</param>
    public static Texture2D CreateCircle(
        int radius,
        bool filled = false,
        GraphicsDevice? graphicsDevice = null
    )
    {
        graphicsDevice ??= Game1.graphics.GraphicsDevice;
        int length = radius * 2 + 1;
        int r2 = radius * radius;
        int e1 = r2 + radius;
        int e2 = r2 - radius;
        var texture = new Texture2D(graphicsDevice, length, length);
        var data = new Color[length * length];
        int i = 0;
        for (int y = -radius; y <= radius; y++)
        {
            int y2 = y * y;
            for (int x = -radius; x <= radius; x++)
            {
                int p = y2 + x * x;
                if (p < e1 && (filled || p > e2))
                {
                    data[i] = Color.White;
                }
                i++;
            }
        }
        texture.SetData(data);
        return texture;
    }
}
