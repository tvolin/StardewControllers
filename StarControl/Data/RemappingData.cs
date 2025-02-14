namespace StarControl.Data;

/// <summary>
/// Data for the player's current button remapping settings.
/// </summary>
public class RemappingData
{
    /// <summary>
    /// Whether to display the HUD showing current button assignments.
    /// </summary>
    public bool HudVisible { get; set; }

    /// <summary>
    /// The currently mapped slots/buttons.
    /// </summary>
    public Dictionary<SButton, RemappingSlot> Slots { get; set; } = [];
}
