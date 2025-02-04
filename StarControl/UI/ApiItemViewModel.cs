using PropertyChanged.SourceGenerator;
using StarControl.Graphics;
using StarControl.Menus;

namespace StarControl.UI;

internal partial class ApiItemViewModel(string id)
{
    public string Id { get; } = id;

    [Notify]
    private string description = "";

    [Notify]
    private bool selected;

    [Notify]
    private Sprite? sprite;

    [Notify]
    private string title = "";

    [Notify]
    private TooltipData? tooltip;

    public static ApiItemViewModel FromItem(IManifest sourceMod, IRadialMenuItem item)
    {
        return new(item.Id)
        {
            Title = item.Title,
            Description = item.Description,
            Sprite = item.Texture is not null
                ? new(item.Texture, item.SourceRectangle ?? item.Texture.Bounds)
                : Sprites.Error(),
            Tooltip = new(
                I18n.Config_ModMenuItem_Api_ItemDescription(
                    sourceMod.Name,
                    sourceMod.UniqueID,
                    item.Description
                ),
                item.Title
            ),
        };
    }

    public ApiItemViewModel Clone()
    {
        return new(Id)
        {
            Description = Description,
            Sprite = Sprite,
            Title = Title,
            Tooltip = Tooltip,
            Selected = Selected,
        };
    }
}
