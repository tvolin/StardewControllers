using PropertyChanged.SourceGenerator;
using StarControl.Gmcm;
using StardewModdingAPI.Utilities;

namespace StarControl.UI;

internal partial class GmcmSyncSettingsViewModel(IGenericModConfigKeybindings bindings)
{
    public IReadOnlyList<IManifest> AvailableMods { get; } = bindings.AllMods.Values.ToList();
    public Func<IManifest, string> FormatModName { get; } = manifest => manifest.Name;

    public GmcmKeybindOptionViewModel? SelectedOption
    {
        get => selectedOption;
        set
        {
            if (value == selectedOption)
            {
                return;
            }
            if (selectedOption is not null)
            {
                selectedOption.Selected = false;
            }
            selectedOption = value;
            if (value is not null)
            {
                SelectedMod = value.Mod;
                selectedOption =
                    AvailableOptions.FirstOrDefault(opt => opt.IsSameOptionAs(value)) ?? value;
                selectedOption.Selected = true;
            }
            OnPropertyChanged(new(nameof(SelectedOption)));
        }
    }

    [Notify]
    private IReadOnlyList<GmcmKeybindOptionViewModel> availableOptions = [];

    [Notify]
    private bool enableDescriptionSync = true;

    [Notify]
    private bool enableTitleSync = true;

    [Notify]
    private IManifest? selectedMod;

    private GmcmKeybindOptionViewModel? selectedOption;

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
        if (SelectedMod is null || SelectedOption?.Mod != SelectedMod)
        {
            SelectedOption = AvailableOptions.Count > 0 ? AvailableOptions[0] : null;
        }
    }
}

internal partial class GmcmKeybindOptionViewModel(IGenericModConfigKeybindOption option)
{
    public Keybind CurrentKeybind => option.GetCurrentBinding();

    public string DisplayName => option.UniqueFieldName;

    public string FieldId => option.FieldId;

    public bool IsMissingKeybind => !CurrentKeybind.IsBound;

    public string MissingKeybindDescription =>
        IsMissingKeybind
            ? I18n.Config_ModMenuItem_Gmcm_MissingKeybind_Description(
                option.ModManifest.Name,
                option.UniqueFieldName
            )
            : "";

    public IManifest Mod => option.ModManifest;

    public string SimpleName => option.FieldName;

    public string UniqueFieldName => option.UniqueFieldName;

    [Notify]
    private bool selected;

    private readonly IGenericModConfigKeybindOption option = option;

    public bool IsSameOptionAs(GmcmKeybindOptionViewModel other)
    {
        return other.option == option;
    }
}
