namespace RadialMenu.Menus;

/// <summary>
/// Types of actions that can be performed when activating an item from the Inventory menu.
/// </summary>
public enum InventoryAction
{
    /// <summary>
    /// Always select the item, regardless of whether it can be consumed or used. Used for gifting
    /// items, placing them into machines, etc.
    /// </summary>
    Select,

    /// <summary>
    /// Try to consume or use the item, if it has that ability; otherwise, <see cref="Select"/> it.
    /// </summary>
    Use,
}
