using StardewModdingAPI.Utilities;

namespace RadialMenu.Gmcm;

public interface IGenericModConfigKeybindings
{
    public static IGenericModConfigKeybindings? Instance { get; set; }

    IReadOnlyDictionary<string, IManifest> AllMods { get; }
    IReadOnlyList<IGenericModConfigKeybindOption> AllOptions { get; }

    IGenericModConfigKeybindOption? Find(
        string modId,
        string fieldId,
        string fieldName = "",
        Keybind? previousBinding = null
    );
}
