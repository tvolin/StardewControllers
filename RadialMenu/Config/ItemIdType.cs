namespace RadialMenu.Config;

/// <summary>
/// For a field accepting multiple types of IDs, such as <see cref="QuickSlotConfiguration.Id"/>,
/// specifies how to interpret the ID.
/// </summary>
public enum ItemIdType
{
    /// <summary>
    /// The ID is the <see cref="Item.QualifiedItemId"/> of an in-game item.
    /// </summary>
    GameItem,

    /// <summary>
    /// The ID is the <see cref="ModMenuItemConfiguration.Id"/> of an action in the Mod Menu.
    /// </summary>
    ModItem,
}
