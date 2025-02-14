namespace StarControl.Config;

/// <summary>
/// Configures a mod integration, i.e. for a mod that has registered one or more menu pages via the
/// Star Control API.
/// </summary>
public class ModPriorityConfiguration : IConfigEquatable<ModPriorityConfiguration>
{
    /// <summary>
    /// The unique ID of the mod, i.e. its <see cref="IManifest.UniqueID"/>.
    /// </summary>
    public string ModId { get; set; } = "";

    /// <summary>
    /// Whether to display this mod's registered pages in the Mod Menu.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc />
    public bool Equals(ModPriorityConfiguration? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return ModId == other.ModId && Enabled == other.Enabled;
    }
}
