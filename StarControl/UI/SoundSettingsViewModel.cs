using PropertyChanged.SourceGenerator;
using StarControl.Config;

namespace StarControl.UI;

internal partial class SoundSettingsViewModel
{
    [Notify]
    private bool enableUiSounds;

    public SoundCueViewModel MenuOpenSound { get; } = new();
    public SoundCueViewModel MenuCloseSound { get; } = new();
    public SoundCueViewModel NextPageSound { get; } = new();
    public SoundCueViewModel PreviousPageSound { get; } = new();
    public SoundCueViewModel ItemFocusSound { get; } = new();
    public SoundCueViewModel ItemActivationSound { get; } = new();
    public SoundCueViewModel ItemDelaySound { get; } = new();
    public SoundCueViewModel ItemErrorSound { get; } = new();

    public void ButtonHover()
    {
        Game1.playSound("Cowboy_Footstep");
    }

    public void Load(SoundConfiguration config)
    {
        EnableUiSounds = config.EnableUiSounds;
        MenuOpenSound.CueName = config.MenuOpenSound;
        MenuCloseSound.CueName = config.MenuCloseSound;
        NextPageSound.CueName = config.NextPageSound;
        PreviousPageSound.CueName = config.PreviousPageSound;
        ItemFocusSound.CueName = config.ItemFocusSound;
        ItemActivationSound.CueName = config.ItemActivationSound;
        ItemDelaySound.CueName = config.ItemDelaySound;
        ItemErrorSound.CueName = config.ItemErrorSound;
    }

    public void Save(SoundConfiguration config)
    {
        config.EnableUiSounds = EnableUiSounds;
        config.MenuOpenSound = MenuOpenSound.CueName;
        config.MenuCloseSound = MenuCloseSound.CueName;
        config.NextPageSound = NextPageSound.CueName;
        config.PreviousPageSound = PreviousPageSound.CueName;
        config.ItemFocusSound = ItemFocusSound.CueName;
        config.ItemActivationSound = ItemActivationSound.CueName;
        config.ItemDelaySound = ItemDelaySound.CueName;
        config.ItemErrorSound = ItemErrorSound.CueName;
    }
}

internal partial class SoundCueViewModel
{
    private static readonly string[] KnownSounds =
    [
        "axchop",
        "axe",
        "backpackIN",
        "bigDeSelect",
        "bigSelect",
        "bob",
        "boulderCrack",
        "breakingGlass",
        "breathin",
        "breathout",
        "button1",
        "cameraNoise",
        "cancel",
        "cat",
        "clank",
        "clubhit",
        "clubswipe",
        "cluck",
        "coin",
        "Cowboy_Footstep",
        "cowboy_gopher",
        "cowboy_gunload",
        "Cowboy_gunshot",
        "Cowboy_monsterDie",
        "cowboy_monsterhit",
        "cowboy_powerup",
        "Cowboy_Secret",
        "crafting",
        "crit",
        "crystal",
        "cut",
        "daggerswipe",
        "detector",
        "dialogueCharacter",
        "dialogueCharacterClose",
        "dirtyHit",
        "discoverMineral",
        "dog_bark",
        "dog_pant",
        "drumkit0",
        "drumkit1",
        "drumkit2",
        "drumkit3",
        "drumkit4",
        "drumkit5",
        "drumkit6",
        "Duck",
        "dwoop",
        "dwop",
        "fallDown",
        "fishEscape",
        "fishSlap",
        "give_gift",
        "grassyStep",
        "hammer",
        "harvest",
        "healSound",
        "hoeHit",
        "jingle1",
        "junimoMeep1",
        "leafrustle",
        "miniharp_note",
        "mouseClick",
        "newArtifact",
        "newRecipe",
        "newRecord",
        "objectiveComplete",
        "openBox",
        "parrot",
        "parry",
        "Pickup_Coin15",
        "pickUpItem",
        "pig",
        "powerup",
        "reward",
        "sandyStep",
        "scissors",
        "select",
        "sell",
        "shiny4",
        "shwip",
        "slimeHit",
        "slosh",
        "smallSelect",
        "snowyStep",
        "stoneStep",
        "swordswipe",
        "throw",
        "throwDownITem",
        "thudStep",
        "tinyWhip",
        "toolSwap",
        "toyPiano",
        "warrior",
        "woodWhack",
        "woodyHit",
        "woodyStep",
        "yoba",
    ];

    [Notify]
    private string cueName = "";

    public void NextSound()
    {
        if (string.IsNullOrEmpty(CueName))
        {
            CueName = KnownSounds[0];
            PlaySound();
            return;
        }
        var index = Array.IndexOf(KnownSounds, CueName);
        if (index == KnownSounds.Length - 1)
        {
            CueName = "";
            return;
        }
        CueName = KnownSounds[(index + 1) % KnownSounds.Length];
        PlaySound();
    }

    public void PlaySound()
    {
        if (!string.IsNullOrEmpty(CueName))
        {
            Game1.playSound(CueName);
        }
    }

    public void PreviousSound()
    {
        if (string.IsNullOrEmpty(CueName))
        {
            CueName = KnownSounds[^1];
            PlaySound();
            return;
        }
        var index = Array.IndexOf(KnownSounds, CueName);
        if (index <= 0)
        {
            CueName = "";
            return;
        }
        CueName = KnownSounds[index - 1];
        PlaySound();
    }
}
