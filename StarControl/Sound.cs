namespace StarControl;

internal static class Sound
{
    public static bool Enabled { get; set; } = true;

    public static void Play(string cueName)
    {
        if (Enabled && !string.IsNullOrEmpty(cueName))
        {
            Game1.playSound(cueName);
        }
    }
}
