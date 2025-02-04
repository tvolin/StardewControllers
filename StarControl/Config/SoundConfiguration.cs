namespace StarControl.Config;

public class SoundConfiguration : IConfigEquatable<SoundConfiguration>
{
    /// <summary>
    /// Whether to enable UI sounds globally.
    /// </summary>
    /// <remarks>
    /// Setting to <c>false</c> will suppress <b>all</b> sounds from the mod, except those
    /// specifically tied to a particular item such as a staircase.
    /// </remarks>
    public bool EnableUiSounds { get; set; } = true;

    /// <summary>
    /// Name of the sound cue to play when opening a controller menu.
    /// </summary>
    public string MenuOpenSound { get; set; } = "shwip";

    /// <summary>
    /// Name of the sound cue to play when closing a controller menu.
    /// </summary>
    public string MenuCloseSound { get; set; } = "";

    /// <summary>
    /// Name of the sound cue to play when navigating to the next page of a radial menu.
    /// </summary>
    public string NextPageSound { get; set; } = "shwip";

    /// <summary>
    /// Name of the sound cue to play when navigating to the previous page of a radial menu.
    /// </summary>
    public string PreviousPageSound { get; set; } = "shwip";

    /// <summary>
    /// Name of the sound cue to play when focusing a new item in a radial menu.
    /// </summary>
    public string ItemFocusSound { get; set; } = "shiny4";

    /// <summary>
    /// Name of the sound cue to play when activating a menu item or quick slot normally, i.e.
    /// without any delay.
    /// </summary>
    public string ItemActivationSound { get; set; } = "tinyWhip";

    /// <summary>
    /// Name of the sound cue to play when selecting an item that will be activated after a delay
    /// and menu flash.
    /// </summary>
    public string ItemDelaySound { get; set; } = "select";

    /// <summary>
    /// Name of the sound cue to play when an item can't be activated.
    /// </summary>
    public string ItemErrorSound { get; set; } = "cancel";

    /// <inheritdoc />
    public bool Equals(SoundConfiguration? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return EnableUiSounds == other.EnableUiSounds
            && MenuOpenSound == other.MenuOpenSound
            && MenuCloseSound == other.MenuCloseSound
            && NextPageSound == other.NextPageSound
            && PreviousPageSound == other.PreviousPageSound
            && ItemFocusSound == other.ItemFocusSound
            && ItemActivationSound == other.ItemActivationSound
            && ItemDelaySound == other.ItemDelaySound
            && ItemErrorSound == other.ItemErrorSound;
    }
}
