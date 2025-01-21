using StardewModdingAPI.Utilities;

namespace RadialMenu.Config;

/// <summary>
/// Settings for a single item in the Mod Menu.
/// </summary>
public class ModMenuItemConfiguration : IConfigEquatable<ModMenuItemConfiguration>
{
    /// <summary>
    /// Unique (per game installation) identifier for this item.
    /// </summary>
    /// <remarks>
    /// IDs can be any string. The configuration UI generates random IDs when new items are created.
    /// The value is used to link <see cref="ItemsConfiguration.QuickSlots"/> to mod menu items; the
    /// same ID must be used in both places.
    /// </remarks>
    public string Id { get; set; } = "";

    /// <summary>
    /// The item name, or title, displayed on top in large text.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The item description, or subtitle, displayed underneath the title in small text.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The keybind to simulate when the item is activated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is <b>not</b> the "desired" keybind for performing the action. It must be the actual
    /// keybind that the target mod expects. Star Control will make the game think that this key
    /// combination has actually been pressed, so that the mod which really handles that key will
    /// respond accordingly.
    /// </para>
    /// <para>
    /// Most mods allow their keybinds to be changed, so the process for setting up a Mod Menu Item
    /// is to first configure the keybind for the target mod, and then set the <em>same</em> keybind
    /// for the item in Star Control; or, use <see cref="GmcmSync"/> to sync the keybind
    /// automatically if the target mod has a GMCM page.
    /// </para>
    /// </remarks>
    public Keybind Keybind { get; set; } = new(SButton.None);

    /// <summary>
    /// The icon representing this item, to display in the radial menu.
    /// </summary>
    public IconConfig Icon { get; set; } = new();

    /// <summary>
    /// Configures some information to be automatically synchronized from the target mod's options
    /// in Generic Mod Config Menu.
    /// </summary>
    public GmcmAssociation? GmcmSync { get; set; }

    /// <inheritdoc />
    public bool Equals(ModMenuItemConfiguration? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return Id == other.Id
            && Name == other.Name
            && Description == other.Description
            && Keybind.Equals(other.Keybind)
            && Icon.Equals(other.Icon)
            && GmcmSync is null == other.GmcmSync is null
            && GmcmSync?.Equals(other.GmcmSync) != false;
    }
}
