using Newtonsoft.Json;

namespace StarControl.Config;

/// <summary>
/// Configuration data for a custom icon, i.e. as used in a <see cref="ModMenuItemConfiguration"/>.
/// </summary>
/// <remarks>
/// Includes its own JSON converter for unambiguous serialization and deserialization.
/// </remarks>
[JsonConverter(typeof(IconConfigJsonConverter))]
public class IconConfig : IConfigEquatable<IconConfig>
{
    /// <summary>
    /// Qualified ID of the in-game item whose icon should be used.
    /// </summary>
    /// <remarks>
    /// If this is specified, then <see cref="TextureAssetPath"/> and <see cref="SourceRect"/> will
    /// be ignored and should both be empty.
    /// </remarks>
    public string ItemId { get; set; } = "";

    /// <summary>
    /// Path to the texture asset where a custom icon is located.
    /// </summary>
    /// <remarks>
    /// Example: <c>LooseSprites/Cursors</c> for a sprite in the Cursors tilesheet.
    /// </remarks>
    public string TextureAssetPath { get; set; } = "";

    /// <summary>
    /// Region of the source texture identified by <see cref="TextureAssetPath"/> where the specific
    /// sprite is located; i.e. which tile to use on the tilesheet.
    /// </summary>
    public Rectangle SourceRect { get; set; }

    private const string ITEM_PREFIX = "Icon:";
    private const string TEXTURE_PREFIX = "Texture:";

    /// <summary>
    /// Parses an <see cref="IconConfig"/> from its string representation.
    /// </summary>
    /// <param name="value">The serialized icon data.</param>
    /// <returns>The parsed icon data.</returns>
    /// <exception cref="FormatException">Thrown when the <paramref name="value"/> cannot be parsed
    /// due to having an unrecognized prefix or invalid format.</exception>
    public static IconConfig Parse(string value)
    {
        if (value.StartsWith(ITEM_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            return new() { ItemId = value[ITEM_PREFIX.Length..] };
        }
        if (value.StartsWith(TEXTURE_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            return ParseTextureRect(value[TEXTURE_PREFIX.Length..]);
        }
        throw new FormatException(
            $"Icon value '{value}' does not start with a valid prefix, either '{ITEM_PREFIX}' or "
                + $"'{TEXTURE_PREFIX}'."
        );
    }

    private static IconConfig ParseTextureRect(string value)
    {
        var parts = value.Split(':');
        if (parts.Length != 2)
        {
            throw new FormatException(
                $"Invalid texture/region value '{value}'. Custom icons must include a texture path "
                    + "and source rectangle separated by a colon (':')."
            );
        }
        var assetPath = parts[0].Trim();
        var formattedRect = parts[1].Trim();
        var coords = formattedRect.Split(',');
        if (coords.Length != 4)
        {
            throw new FormatException(
                $"Invalid region '{formattedRect}' in custom icon '{value}'. Rectangles must be in "
                    + $"the format 'x,y,width,height' but {coords.Length} values were provided instead "
                    + "of 4."
            );
        }
        if (
            !int.TryParse(coords[0], out int x)
            || !int.TryParse(coords[1], out int y)
            || !int.TryParse(coords[2], out int width)
            || !int.TryParse(coords[3], out int height)
        )
        {
            throw new FormatException(
                $"Invalid region '{formattedRect}' in custom icon '{value}'. One or more rectangle "
                    + "dimensions could not be parsed (all must be integer values)."
            );
        }
        return new()
        {
            TextureAssetPath = assetPath,
            SourceRect = new Rectangle(x, y, width, height),
        };
    }

    /// <inheritdoc />
    public bool Equals(IconConfig? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return ItemId == other.ItemId
            && TextureAssetPath == other.TextureAssetPath
            && SourceRect.Equals(other.SourceRect);
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(ItemId))
        {
            return ITEM_PREFIX + ItemId;
        }
        var rect = SourceRect;
        return !string.IsNullOrWhiteSpace(TextureAssetPath)
            ? $"{TEXTURE_PREFIX}{TextureAssetPath}:{rect.X},{rect.Y},{rect.Width},{rect.Height}"
            : "";
    }
}

internal class IconConfigJsonConverter : JsonConverter<IconConfig>
{
    public override IconConfig? ReadJson(
        JsonReader reader,
        Type objectType,
        IconConfig? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        var hexString = (reader.Value as string);
        return !string.IsNullOrEmpty(hexString) ? IconConfig.Parse(hexString) : null;
    }

    public override void WriteJson(JsonWriter writer, IconConfig? value, JsonSerializer serializer)
    {
        if (value is not null)
        {
            writer.WriteValue(value.ToString());
        }
        else
        {
            writer.WriteNull();
        }
    }
}
