using StardewModdingAPI.Utilities;

namespace StarControl.Gmcm;

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
