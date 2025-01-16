using System.Text;

namespace RadialMenu.UI;

internal static class IdGenerator
{
    private static readonly string AllCharacters =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string NewId(int length)
    {
        var chars = AllCharacters.AsSpan();
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sb.Append(chars[Random.Shared.Next(62)]);
        }
        return sb.ToString();
    }
}
