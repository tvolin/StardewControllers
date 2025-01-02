using PropertyChanged.SourceGenerator;
using RadialMenu.Config;

namespace RadialMenu.UI;

internal partial class StyleConfigurationViewModel
{
    public Func<float, string> FormatPixels { get; } = v => v.ToString("f0") + " px";

    /// <inheritdoc cref="Styles.InnerBackgroundColor" />
    [Notify]
    private Color innerBackgroundColor;

    /// <inheritdoc cref="Styles.InnerRadius" />
    [Notify]
    private float innerRadius;

    /// <inheritdoc cref="Styles.OuterBackgroundColor" />
    [Notify]
    private Color outerBackgroundColor;

    /// <inheritdoc cref="Styles.OuterRadius" />
    [Notify]
    private float outerRadius;

    /// <inheritdoc cref="Styles.SelectionColor" />
    [Notify]
    private Color selectionColor;

    /// <inheritdoc cref="Styles.HighlightColor" />
    [Notify]
    private Color highlightColor;

    /// <inheritdoc cref="Styles.GapWidth" />
    [Notify]
    private float gapWidth;

    /// <inheritdoc cref="Styles.MenuSpriteHeight" />
    [Notify]
    private int menuSpriteHeight;

    /// <inheritdoc cref="Styles.StackSizeColor" />
    [Notify]
    private Color stackSizeColor;

    /// <inheritdoc cref="Styles.CursorDistance" />
    [Notify]
    private float cursorDistance;

    /// <inheritdoc cref="Styles.CursorSize" />
    [Notify]
    private float cursorSize;

    /// <inheritdoc cref="Styles.CursorColor" />
    [Notify]
    private Color cursorColor;

    /// <inheritdoc cref="Styles.SelectionSpriteHeight" />
    [Notify]
    private int selectionSpriteHeight;

    /// <inheritdoc cref="Styles.SelectionTitleColor" />
    [Notify]
    private Color selectionTitleColor;

    /// <inheritdoc cref="Styles.SelectionDescriptionColor" />
    [Notify]
    private Color selectionDescriptionColor;

    public void Load(Styles config)
    {
        InnerBackgroundColor = config.InnerBackgroundColor;
        InnerRadius = config.InnerRadius;
        OuterBackgroundColor = config.OuterBackgroundColor;
        OuterRadius = config.OuterRadius;
        SelectionColor = config.SelectionColor;
        HighlightColor = config.HighlightColor;
        GapWidth = config.GapWidth;
        MenuSpriteHeight = config.MenuSpriteHeight;
        StackSizeColor = config.StackSizeColor;
        CursorDistance = config.CursorDistance;
        CursorSize = config.CursorSize;
        CursorColor = config.CursorColor;
        SelectionSpriteHeight = config.SelectionSpriteHeight;
        SelectionTitleColor = config.SelectionTitleColor;
        SelectionDescriptionColor = config.SelectionDescriptionColor;
    }

    public void Save(Styles config)
    {
        config.InnerBackgroundColor = new(InnerBackgroundColor);
        config.InnerRadius = InnerRadius;
        config.OuterBackgroundColor = new(OuterBackgroundColor);
        config.OuterRadius = OuterRadius;
        config.SelectionColor = new(SelectionColor);
        config.HighlightColor = new(HighlightColor);
        config.GapWidth = GapWidth;
        config.MenuSpriteHeight = MenuSpriteHeight;
        config.StackSizeColor = new(StackSizeColor);
        config.CursorDistance = CursorDistance;
        config.CursorSize = CursorSize;
        config.CursorColor = new(CursorColor);
        config.SelectionSpriteHeight = SelectionSpriteHeight;
        config.SelectionTitleColor = new(SelectionTitleColor);
        config.SelectionDescriptionColor = new(SelectionDescriptionColor);
    }
}
