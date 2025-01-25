using System.Text;
using StardewModdingAPI.Utilities;

namespace RadialMenu.Gmcm;

internal class KeybindOption(
    IManifest modManifest,
    string fieldId,
    Func<string> getPageTitle,
    Func<string> getSectionTitle,
    Func<string> getFieldName,
    Func<string> getTooltip,
    Func<Keybind> getCurrentBinding
) : IGenericModConfigKeybindOption
{
    public string FieldId => fieldId;

    public string FieldName => getFieldName();

    public IManifest ModManifest => modManifest;

    public string Tooltip => getTooltip();

    public string UniqueFieldName
    {
        get
        {
            var sb = new StringBuilder();
            var pageTitle = getPageTitle();
            if (!string.IsNullOrWhiteSpace(pageTitle))
            {
                sb.Append(pageTitle).Append(" > ");
            }
            var sectionTitle = getSectionTitle();
            if (!string.IsNullOrWhiteSpace(sectionTitle))
            {
                sb.Append(sectionTitle).Append(" > ");
            }
            return sb.Append(getFieldName()).ToString();
        }
    }

    public Keybind GetCurrentBinding()
    {
        return getCurrentBinding();
    }

    public bool MatchesBinding(Keybind? otherBinding)
    {
        return getCurrentBinding().Equals(otherBinding);
    }
}
