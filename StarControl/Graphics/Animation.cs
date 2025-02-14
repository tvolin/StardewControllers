namespace StarControl.Graphics;

internal static class Animation
{
    public const int ERROR_FLASH_DURATION_MS = 250;

    private const int DELAY_FLASH_DURATION_MS = 80;

    public static float GetDelayFlashPosition(float elapsedMs)
    {
        return MathF.Abs(elapsedMs / DELAY_FLASH_DURATION_MS % 2 - 1);
    }

    public static float GetErrorFlashPosition(float elapsedMs)
    {
        return 1 - elapsedMs / ERROR_FLASH_DURATION_MS;
    }
}
