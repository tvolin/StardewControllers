using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RadialMenu.Config;
using RadialMenu.Graphics;

namespace RadialMenu.Menus;

public class RadialMenuPainter(GraphicsDevice graphicsDevice, Styles styles)
{
    private record SelectionState(int ItemCount, int SelectedIndex, int FocusedIndex);

    private const float CIRCLE_MAX_ERROR = 0.1f;
    private const float EQUILATERAL_ANGLE = MathF.PI * 2 / 3;
    private const float MENU_SPRITE_MAX_WIDTH_RATIO = 0.8f;
    private const float TWO_PI = MathF.PI * 2;

    private static readonly float ROOT_3 = MathF.Sqrt(3);

    private static readonly Sprite UnknownSprite = new(
        Game1.mouseCursors, // Question Mark
        new(176, 425, 9, 12)
    );

    public IReadOnlyList<IRadialMenuItem?> Items { get; set; } = [];
    public RenderTarget2D? RenderTarget { get; set; }
    public float Scale { get; set; } = 1f;

    private readonly BasicEffect effect = new(graphicsDevice)
    {
        World = Matrix.Identity,
        View = Matrix.CreateLookAt(Vector3.Forward, Vector3.Zero, Vector3.Down),
        VertexColorEnabled = true,
    };

    private VertexPositionColor[] innerVertices = [];
    private VertexPositionColor[] outerVertices = [];
    private float previousScale = 1f;
    private float selectionBlend = 1.0f;
    private SelectionState selectionState = new(ItemCount: 0, SelectedIndex: 0, FocusedIndex: 0);

    public void Invalidate()
    {
        innerVertices = [];
        outerVertices = [];
    }

    public void Paint(
        SpriteBatch spriteBatch,
        int selectedIndex,
        int focusedIndex,
        float? selectionAngle = null,
        float selectionBlend = 1.0f,
        Rectangle? viewport = null
    )
    {
        if (Scale <= 0)
        {
            return;
        }
        if (Scale != previousScale)
        {
            Invalidate();
            previousScale = Scale;
        }
        var hasNewVertices = GenerateVertices();
        var selectionState = new SelectionState(Items.Count, selectedIndex, focusedIndex);
        if (
            hasNewVertices
            || selectionState != this.selectionState
            || selectionBlend != this.selectionBlend
        )
        {
            this.selectionState = selectionState;
            this.selectionBlend = selectionBlend;
            UpdateVertexColors();
        }
        viewport ??= RenderTarget?.Bounds ?? Viewports.DefaultViewport;
        RenderTargetBinding[]? previousTargets = null;
        if (RenderTarget is not null)
        {
            previousTargets = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTarget(RenderTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                rasterizerState: new() { MultiSampleAntiAlias = false },
                samplerState: SamplerState.PointClamp
            );
        }
        try
        {
            PaintBackgrounds(viewport.Value, selectionAngle);
            PaintItems(spriteBatch, viewport.Value);
            PaintSelectionDetails(spriteBatch, viewport.Value);
        }
        finally
        {
            if (previousTargets is not null)
            {
                spriteBatch.End();
                graphicsDevice.SetRenderTargets(previousTargets);
            }
        }
    }

    private void PaintBackgrounds(Rectangle viewport, float? selectionAngle)
    {
        effect.World = Matrix.CreateTranslation(viewport.X, viewport.Y, 0);
        effect.Projection = Matrix.CreateOrthographic(viewport.Width, viewport.Height, 0, 1);
        // Cursor is just 1 triangle, so we can compute this on every frame.
        var cursorVertices =
            selectionAngle != null
                ? GenerateCursorVertices(
                    (styles.InnerRadius - styles.CursorDistance) * Scale,
                    selectionAngle.Value
                )
                : [];
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                innerVertices,
                0,
                innerVertices.Length / 3
            );
            graphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                outerVertices,
                0,
                outerVertices.Length / 3
            );
            if (cursorVertices.Length > 0)
            {
                graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    cursorVertices,
                    0,
                    cursorVertices.Length / 3
                );
            }
        }
    }

    private void PaintItems(SpriteBatch spriteBatch, Rectangle viewport)
    {
        var centerX = viewport.X + viewport.Width / 2.0f;
        var centerY = viewport.Y + viewport.Height / 2.0f;
        var itemRadius = (styles.InnerRadius + styles.GapWidth + styles.OuterRadius / 2.0f) * Scale;
        var angleBetweenItems = TWO_PI / Items.Count;
        var currentAngle = 0.0f;
        foreach (var item in Items)
        {
            if (item is null)
            {
                currentAngle += angleBetweenItems;
                continue;
            }
            var itemPoint = GetCirclePoint(itemRadius, currentAngle);
            var displaySize = GetScaledSize(item, styles.MenuSpriteHeight * Scale);
            // Aspect ratio is usually almost square, or has extra height (e.g. big craftables).
            // In case of a horizontal aspect ratio, shrink the size so that it still fits.
            var maxWidth = styles.OuterRadius * MENU_SPRITE_MAX_WIDTH_RATIO * Scale;
            if (displaySize.X > maxWidth)
            {
                var itemScale = maxWidth / displaySize.X;
                displaySize = new(
                    (int)MathF.Round(displaySize.X * itemScale),
                    (int)MathF.Round(displaySize.Y * itemScale)
                );
            }
            GetSpriteSize(item, out var isMonogram);
            // Sprites draw from top left rather than center; we have to adjust for it.
            var itemPoint2d = new Vector2(
                centerX + itemPoint.X - displaySize.X / 2.0f,
                centerY + itemPoint.Y - displaySize.Y / 2.0f
            );
            var destinationRect = new Rectangle(itemPoint2d.ToPoint(), displaySize);
            if (isMonogram)
            {
                Monogram.Draw(spriteBatch, destinationRect, item.Title, Color.White);
            }
            else
            {
                // TODO: Shadows may end up in wrong place when using non-default sizes. Fix.
                var shadowTexture = Game1.shadowTexture;
                spriteBatch.Draw(
                    shadowTexture,
                    destinationRect.Location.ToVector2() + new Vector2(32f * Scale, 52f * Scale),
                    shadowTexture.Bounds,
                    new(Color.Gray, 0.5f),
                    0.0f,
                    new(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y),
                    3f * Scale,
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
                spriteBatch.Draw(item.Texture, destinationRect, item.SourceRectangle, baseColor);
                if (item.TintRectangle is { } tintRect && item.TintColor is { } tintColor)
                {
                    spriteBatch.Draw(item.Texture, destinationRect, tintRect, tintColor);
                }
            }
            if (item.Quality is { } quality && quality > 0)
            {
                // From StardewValley:Object.cs
                Rectangle qualitySourceRect =
                    quality < 4 ? new(338 + (quality - 1) * 8, 400, 8, 8) : new(346, 392, 8, 8);
                var qualityIconPos = new Vector2(
                    destinationRect.Left,
                    destinationRect.Bottom - 16 * Scale
                );
                spriteBatch.Draw(
                    Game1.mouseCursors,
                    qualityIconPos,
                    qualitySourceRect,
                    Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: 3.0f * Scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0.1f
                );
            }
            if (item.StackSize is { } stackSize)
            {
                var stackTextScale = 3.0f * Scale;
                var stackTextWidth = Utility.getWidthOfTinyDigitString(stackSize, stackTextScale);
                var stackLabelPos = new Vector2(
                    destinationRect.Right - stackTextWidth,
                    destinationRect.Bottom - 8 * Scale
                );
                Utility.drawTinyDigits(
                    stackSize,
                    spriteBatch,
                    stackLabelPos,
                    stackTextScale,
                    layerDepth: 0.1f,
                    styles.StackSizeColor
                );
            }
            currentAngle += angleBetweenItems;
        }
    }

    private void PaintSelectionDetails(SpriteBatch spriteBatch, Rectangle viewport)
    {
        if (selectionState.FocusedIndex < 0)
        {
            return;
        }
        var item =
            Items.Count > selectionState.FocusedIndex ? Items[selectionState.FocusedIndex] : null;
        if (item is null)
        {
            return;
        }

        var centerX = viewport.X + viewport.Width / 2.0f;
        var centerY = viewport.Y + viewport.Height / 2.0f;
        if (item.Texture is not null)
        {
            var itemDrawSize = GetScaledSize(item, styles.SelectionSpriteHeight * Scale);
            var itemPos = new Vector2(centerX - itemDrawSize.X / 2, centerY - itemDrawSize.Y - 24);
            var itemRect = new Rectangle(itemPos.ToPoint(), itemDrawSize);
            var baseColor = item.TintRectangle is null
                ? (item.TintColor ?? Color.White)
                : Color.White;
            spriteBatch.Draw(item.Texture, itemRect, item.SourceRectangle, baseColor);
            if (item.TintRectangle is Rectangle tintRect && item.TintColor is Color tintColor)
            {
                spriteBatch.Draw(item.Texture, itemRect, tintRect, tintColor);
            }
        }

        var labelFont = Game1.dialogueFont;
        var labelSize = labelFont.MeasureString(item.Title) * Scale;
        var labelPos = new Vector2(centerX - labelSize.X / 2.0f, centerY);
        spriteBatch.DrawString(
            labelFont,
            item.Title,
            labelPos,
            styles.SelectionTitleColor,
            rotation: 0,
            origin: Vector2.Zero,
            scale: Scale,
            effects: SpriteEffects.None,
            layerDepth: 0
        );

        var descriptionFont = Game1.smallFont;
        var descriptionText = item.Description;
        var descriptionY = labelPos.Y + (labelFont.LineSpacing + 16.0f) * Scale;
        var descriptionLines = Game1
            .parseText(descriptionText, descriptionFont, 400)
            .Split(Environment.NewLine);
        foreach (var descriptionLine in descriptionLines)
        {
            var descriptionSize = descriptionFont.MeasureString(descriptionLine) * Scale;
            var descriptionPos = new Vector2(centerX - descriptionSize.X / 2.0f, descriptionY);
            descriptionY += descriptionFont.LineSpacing * Scale;
            spriteBatch.DrawString(
                descriptionFont,
                descriptionLine,
                descriptionPos,
                styles.SelectionDescriptionColor,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }

    private bool GenerateVertices()
    {
        var wasGenerated = false;
        if (innerVertices.Length == 0)
        {
            innerVertices = GenerateCircleVertices(
                styles.InnerRadius * Scale,
                styles.InnerBackgroundColor
            );
            wasGenerated = true;
        }
        if (outerVertices.Length == 0)
        {
            outerVertices = GenerateDonutVertices(
                (styles.InnerRadius + styles.GapWidth) * Scale,
                styles.OuterRadius * Scale,
                styles.OuterBackgroundColor
            );
            wasGenerated = true;
        }
        return wasGenerated;
    }

    private static (float start, float end) GetSegmentRange(
        int focusedIndex,
        int itemCount,
        int segmentCount
    )
    {
        if (focusedIndex < 0)
        {
            return (-1.0f, -0.5f);
        }
        var sliceSize = (float)segmentCount / itemCount;
        var relativePosition = (float)focusedIndex / itemCount;
        var end = (relativePosition * segmentCount + sliceSize / 2) % segmentCount;
        var start = (end - sliceSize + segmentCount) % segmentCount;
        return (start, end);
    }

    private void UpdateVertexColors()
    {
        var (itemCount, selectedIndex, focusedIndex) = selectionState;
        const int outerChordSize = 6;
        var segmentCount = outerVertices.Length / outerChordSize;
        var (selectionHighlightStartSegment, selectionHighlightEndSegment) = GetSegmentRange(
            selectedIndex,
            itemCount,
            segmentCount
        );
        var (focusHighlightStartSegment, focusHighlightEndSegment) = GetSegmentRange(
            focusedIndex,
            itemCount,
            segmentCount
        );
        for (var i = 0; i < segmentCount; i++)
        {
            var isFocusHighlight =
                focusHighlightStartSegment < focusHighlightEndSegment
                    ? (i >= focusHighlightStartSegment && i < focusHighlightEndSegment)
                    : (i >= focusHighlightStartSegment || i < focusHighlightEndSegment);
            var isSelectionHighlight =
                !isFocusHighlight
                && (
                    selectionHighlightStartSegment < selectionHighlightEndSegment
                        ? (i >= selectionHighlightStartSegment && i < selectionHighlightEndSegment)
                        : (i >= selectionHighlightStartSegment || i < selectionHighlightEndSegment)
                );
            var outerIndex = i * outerChordSize;
            var outerColor =
                isFocusHighlight
                    ? Color.Lerp(styles.OuterBackgroundColor, styles.HighlightColor, selectionBlend)
                : isSelectionHighlight ? styles.SelectionColor
                : styles.OuterBackgroundColor;
            for (var j = 0; j < outerChordSize; j++)
            {
                outerVertices[outerIndex + j].Color = outerColor;
            }
        }
    }

    private static VertexPositionColor[] GenerateCircleVertices(float radius, Color color)
    {
        var vertexCount = GetOptimalVertexCount(radius);
        var step = TWO_PI / vertexCount;
        var t = 0.0f;
        var vertices = new VertexPositionColor[vertexCount * 3];
        var vertexIndex = 0;
        var prevPoint = GetCirclePoint(radius, 0);
        // Note: We loop using a fixed number of vertices, instead of a max angle, in case of
        // floating point rounding error.
        for (var i = 0; i < vertexCount; i++)
        {
            t += step;
            var nextPoint = GetCirclePoint(radius, t);
            vertices[vertexIndex++] = new(prevPoint, color);
            vertices[vertexIndex++] = new(nextPoint, color);
            vertices[vertexIndex++] = new(Vector3.Zero, color);
            prevPoint = nextPoint;
        }
        return vertices;
    }

    private static VertexPositionColor[] GenerateDonutVertices(
        float innerRadius,
        float thickness,
        Color color
    )
    {
        var outerRadius = innerRadius + thickness;
        var vertexCount = GetOptimalVertexCount(outerRadius);
        var step = TWO_PI / vertexCount;
        var t = 0.0f;
        var vertices = new VertexPositionColor[vertexCount * 6];
        var vertexIndex = 0;
        var prevInnerPoint = GetCirclePoint(innerRadius, 0);
        var prevOuterPoint = GetCirclePoint(outerRadius, 0);
        // Note: We loop using a fixed number of vertices, instead of a max angle, in case of
        // floating point rounding error.
        for (var i = 0; i < vertexCount; i++)
        {
            t += step;
            var nextInnerPoint = GetCirclePoint(innerRadius, t);
            var nextOuterPoint = GetCirclePoint(outerRadius, t);
            vertices[vertexIndex++] = new(prevOuterPoint, color);
            vertices[vertexIndex++] = new(nextOuterPoint, color);
            vertices[vertexIndex++] = new(nextInnerPoint, color);
            vertices[vertexIndex++] = new(nextInnerPoint, color);
            vertices[vertexIndex++] = new(prevInnerPoint, color);
            vertices[vertexIndex++] = new(prevOuterPoint, color);
            prevInnerPoint = nextInnerPoint;
            prevOuterPoint = nextOuterPoint;
        }
        return vertices;
    }

    private VertexPositionColor[] GenerateCursorVertices(float tipRadius, float angle)
    {
        var center = GetCirclePoint(tipRadius - styles.CursorSize / 2, angle);
        // Compute the points for an origin-centered triangle, then offset.
        var radius = styles.CursorSize / ROOT_3;
        var p1 = center + radius * new Vector3(MathF.Sin(angle), -MathF.Cos(angle), 0);
        var angle2 = angle + EQUILATERAL_ANGLE;
        var p2 = center + radius * new Vector3(MathF.Sin(angle2), -MathF.Cos(angle2), 0);
        var angle3 = angle2 + EQUILATERAL_ANGLE;
        var p3 = center + radius * new Vector3(MathF.Sin(angle3), -MathF.Cos(angle3), 0);
        return
        [
            new(p1, styles.CursorColor),
            new(p2, styles.CursorColor),
            new(p3, styles.CursorColor),
        ];
    }

    private static Vector3 GetCirclePoint(float radius, float angle)
    {
        var x = radius * MathF.Sin(angle);
        var y = radius * -MathF.Cos(angle);
        return new Vector3(x, y, 0);
    }

    private static int GetOptimalVertexCount(float radius)
    {
        var optimalAngle = Math.Acos(1 - CIRCLE_MAX_ERROR / radius);
        return Math.Max((int)Math.Ceiling(TWO_PI / optimalAngle), 8);
    }

    private static Point GetScaledSize(IRadialMenuItem item, float height)
    {
        var sourceSize = GetSpriteSize(item, out _);
        var aspectRatio = (float)sourceSize.X / sourceSize.Y;
        var width = (int)MathF.Round(height * aspectRatio);
        return new(width, (int)MathF.Round(height));
    }

    private static Point GetSpriteSize(IRadialMenuItem item, out bool isMonogram)
    {
        if (item.Texture is null)
        {
            var monogramSize = Monogram.Measure(item.Title)?.ToPoint();
            isMonogram = monogramSize.HasValue;
            return monogramSize ?? UnknownSprite.SourceRect.Size;
        }
        isMonogram = false;
        return item.SourceRectangle?.Size ?? new Point(item.Texture.Width, item.Texture.Height);
    }
}
