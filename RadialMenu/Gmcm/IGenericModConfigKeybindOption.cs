using StardewModdingAPI.Utilities;

namespace RadialMenu.Gmcm;

public interface IGenericModConfigKeybindOption
{
    string FieldId { get; }
    string FieldName { get; }
    IManifest ModManifest { get; }
    string Tooltip { get; }
    string UniqueFieldName { get; }

    Keybind GetCurrentBinding();

    bool MatchesBinding(Keybind? otherBinding);
}
