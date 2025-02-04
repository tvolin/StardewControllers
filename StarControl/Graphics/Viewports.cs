namespace StarControl.Graphics;

internal static class Viewports
{
    public static Rectangle DefaultViewport
    {
        get
        {
            var uiViewport = Game1.uiViewport;
            return new(0, 0, uiViewport.Width, uiViewport.Height);
        }
    }
}
