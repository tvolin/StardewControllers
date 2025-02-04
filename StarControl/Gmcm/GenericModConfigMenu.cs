using Microsoft.Xna.Framework.Graphics;
using StarControl.UI;

namespace StarControl.Gmcm;

internal class GenericModConfigMenu(
    IGenericModMenuConfigApi gmcm,
    IManifest mod,
    Action<Action> openRealSettings
)
{
    private const int BUTTON_BORDER_SIZE = 2;

    private const int BUTTON_PADDING = 16;
    private const int BUTTON_SCALE = 4;

    private static readonly Rectangle ButtonDarkSourceRect = new(256, 256, 10, 10);
    private static readonly Rectangle ButtonLightSourceRect = new(267, 256, 10, 10);

    private readonly ClickDetector clickDetector = new();

    // Initialized (or reinitialized) on menu open
    private string buttonText = "";
    private Vector2 buttonTextSize;

    public void Open()
    {
        gmcm.OpenModMenu(mod);
    }

    public void Setup()
    {
        gmcm.Register(mod, reset: () => { }, save: () => { });

        // TODO: Add logo/image thing
        if (!ViewEngine.IsInstalled)
        {
            gmcm.AddParagraph(mod, I18n.Gmcm_Info_MissingStardewUI);
            return;
        }
        // This is actually fallback for when Harmony-patching fails and we aren't able to take over
        // the mod settings page completely. Therefore, minimalistic.
        gmcm.AddParagraph(mod, I18n.Gmcm_Info_Default);
        gmcm.AddComplexOption(
            mod,
            name: () => "",
            height: () => Game1.smallFont.LineSpacing + 2 * BUTTON_PADDING,
            beforeMenuOpened: () =>
            {
                buttonText = I18n.Gmcm_Button_OpenConfiguration_Title();
                buttonTextSize = Game1.smallFont.MeasureString(buttonText);
            },
            draw: DrawButton
        );
    }

    private void DrawButton(SpriteBatch b, Vector2 position)
    {
        var buttonSize = new Vector2(
            buttonTextSize.X + BUTTON_PADDING * 2,
            buttonTextSize.Y + BUTTON_PADDING * 2
        );
        var buttonPosition = (position - buttonSize / 2).ToPoint();
        var mousePosition = Game1.getMousePosition(ui_scale: true);
        var hovering =
            mousePosition.X >= buttonPosition.X
            && mousePosition.Y >= buttonPosition.Y
            && mousePosition.X < buttonPosition.X + buttonSize.X
            && mousePosition.Y < buttonPosition.Y + buttonSize.Y;
        if (hovering && clickDetector.HasLeftClick())
        {
            openRealSettings(Open);
            return;
        }
        var sourceRect = hovering ? ButtonLightSourceRect : ButtonDarkSourceRect;

        // Clumsy ad-hoc nine-slice; StardewUI can do this easily, but we are going through the API
        // and don't have direct access to its scaler.
        var (sourceX1, sourceY1) = (sourceRect.X, sourceRect.Y);
        var (sourceX2, sourceY2) = (sourceX1 + BUTTON_BORDER_SIZE, sourceY1 + BUTTON_BORDER_SIZE);
        var (sourceX3, sourceY3) = (
            sourceX1 + sourceRect.Width - BUTTON_BORDER_SIZE,
            sourceY1 + sourceRect.Height - BUTTON_BORDER_SIZE
        );
        var sourceMidWidth = sourceX3 - sourceX2;

        var (buttonWidth, buttonHeight) = buttonSize.ToPoint();
        var destBorderLength = BUTTON_BORDER_SIZE * BUTTON_SCALE;
        var (destX1, destY1) = buttonPosition;
        var (destX2, destY2) = (destX1 + destBorderLength, destY1 + destBorderLength);
        var (destX3, destY3) = (
            destX1 + buttonWidth - destBorderLength,
            destY1 + buttonHeight - destBorderLength
        );
        var destMidWidth = destX3 - destX2;

        DrawRow(sourceY1, destY1, BUTTON_BORDER_SIZE, destBorderLength);
        DrawRow(sourceY2, destY2, sourceY3 - sourceY2, destY3 - destY2);
        DrawRow(sourceY3, destY3, BUTTON_BORDER_SIZE, destBorderLength);

        var textPosition = position - buttonTextSize / 2;
        Utility.drawTextWithShadow(b, buttonText, Game1.smallFont, textPosition, Game1.textColor);

        return;

        void DrawRow(int sourceY, int destY, int sourceHeight, int destHeight)
        {
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(destX1, destY, destBorderLength, destHeight),
                new Rectangle(sourceX1, sourceY, BUTTON_BORDER_SIZE, sourceHeight),
                Color.White
            );
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(destX2, destY, destMidWidth, destHeight),
                new Rectangle(sourceX2, sourceY, sourceMidWidth, sourceHeight),
                Color.White
            );
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(destX3, destY, destBorderLength, destHeight),
                new Rectangle(sourceX3, sourceY, BUTTON_BORDER_SIZE, sourceHeight),
                Color.White
            );
        }
    }
}
