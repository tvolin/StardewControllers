using PropertyChanged.SourceGenerator;
using RadialMenu.Gmcm;
using StardewModdingAPI.Utilities;

namespace RadialMenu.UI;

internal partial class GmcmSyncSettingsViewModel(GenericModConfigKeybindings bindings)
{
    public IReadOnlyList<IManifest> AvailableMods { get; } = bindings.AllMods.Values.ToList();
    public Func<IManifest, string> FormatModName { get; } = manifest => manifest.Name;

    [Notify]
    private IReadOnlyList<GmcmKeybindOptionViewModel> availableOptions = [];

    [Notify]
    private bool enableDescriptionSync = true;

    [Notify]
    private bool enableTitleSync = true;

    [Notify]
    private GmcmKeybindOptionViewModel? selectedOption;

    [Notify]
    private IManifest? selectedMod = null;

    public void SelectOption(GmcmKeybindOptionViewModel option)
    {
        if (option == SelectedOption)
        {
            return;
        }
        Game1.playSound("drumkit6");
        SelectedOption = option;
    }

    private void OnSelectedModChanged()
    {
        AvailableOptions = SelectedMod is not null
            ? bindings
                .AllOptions.Where(opt => opt.ModManifest == SelectedMod)
                .Select(opt => new GmcmKeybindOptionViewModel(opt))
                .ToList()
            : [];
        SelectedOption = AvailableOptions.Count > 0 ? AvailableOptions[0] : null;
    }

    private void OnSelectedOptionChanged(
        GmcmKeybindOptionViewModel? oldValue,
        GmcmKeybindOptionViewModel? newValue
    )
    {
        if (oldValue is not null)
        {
            oldValue.Selected = false;
        }
        if (newValue is not null)
        {
            newValue.Selected = true;
        }
    }
}

internal partial class GmcmKeybindOptionViewModel(GenericModConfigKeybindOption option)
{
    public Keybind CurrentKeybind => option.GetCurrentBinding();

    public string DisplayName => option.UniqueFieldName;

    public bool IsMissingKeybind => !CurrentKeybind.IsBound;

    public string MissingKeybindDescription =>
        IsMissingKeybind
            ? I18n.Config_ModMenuItem_Gmcm_MissingKeybind_Description(
                option.ModManifest.Name,
                option.UniqueFieldName
            )
            : "";

    public string SimpleName => option.GetFieldName();

    [Notify]
    private bool selected;
}
