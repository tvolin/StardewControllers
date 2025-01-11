using Microsoft.Xna.Framework.Graphics;

namespace RadialMenu.UI;

internal record Sprite(Texture2D Texture, Rectangle SourceRect)
{
    public static bool TryParseRectangle(string value, out Rectangle rect)
    {
        rect = default;
        var coords = value.Split(',');
        if (coords.Length != 4)
        {
            return false;
        }
        if (
            int.TryParse(coords[0], out int x)
            && int.TryParse(coords[1], out int y)
            && int.TryParse(coords[2], out int width)
            && int.TryParse(coords[3], out int height)
        )
        {
            rect = new(x, y, width, height);
            return true;
        }
        return false;
    }
}
