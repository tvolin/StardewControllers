using StardewModdingAPI.Utilities;

namespace StarControl.Gmcm;

public interface IGenericModConfigKeybindings
{
    public static IGenericModConfigKeybindings? Instance { get; set; }

    public event EventHandler<ModEventArgs>? Saved;

    IReadOnlyDictionary<string, IManifest> AllMods { get; }
    IReadOnlyList<IGenericModConfigKeybindOption> AllOptions { get; }

    IGenericModConfigKeybindOption? Find(
        string modId,
        string fieldId,
        string fieldName = "",
        Keybind? previousBinding = null
    );
}
