using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RadialMenu.Config;
using RadialMenu.Menus;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal class RadialMenuPreview : IDisposable
{
    public Texture2D? Texture => !renderTarget.IsDisposed ? renderTarget : null;

    private readonly StyleConfigurationViewModel context;
    private readonly RadialMenuPainter painter;
    private readonly PreviewItem[] previewItems =
    [
        new("(T)GoldAxe"),
        new("(T)CopperHoe"),
        new("(T)SteelWateringCan"),
        new("(T)IridiumPickaxe"),
        new("(W)47"),
        new("(O)24", 6),
        new("(O)388", 55),
        new("(O)390", 141),
        new("(W)10"),
    ];
    private readonly RenderTarget2D renderTarget;
    private readonly Styles styles = new();

    public RadialMenuPreview(StyleConfigurationViewModel context, int width, int height)
    {
        this.context = context;
        context.PropertyChanged += Context_PropertyChanged;
        renderTarget = new(Game1.graphics.GraphicsDevice, width, height);
        painter = new(Game1.graphics.GraphicsDevice, styles)
        {
            Items = previewItems,
            RenderTarget = renderTarget,
            Scale = 0.5f,
        };
    }

    public void Dispose()
    {
        painter.RenderTarget = null;
        renderTarget.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Context_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        context.Save(styles);
        painter.Invalidate();
        Draw();
    }

    private void Draw()
    {
        painter.Paint(
            Game1.spriteBatch,
            selectedIndex: 1,
            focusedIndex: 3,
            selectionAngle: MathHelper.ToRadians(120),
            selectionBlend: 1f
        );
    }

    private class PreviewItem(string itemId, int? stackSize = null) : IRadialMenuItem
    {
        public string Description => data.Description;
        public Rectangle? SourceRectangle => data.GetSourceRect();
        public int? StackSize => stackSize;
        public string Title => data.DisplayName;
        public Texture2D Texture => data.GetTexture();

        private readonly ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);

        public ItemActivationResult Activate(
            Farmer who,
            DelayedActions delayedActions,
            bool secondaryAction = false
        )
        {
            return ItemActivationResult.Ignored;
        }
    }
}
