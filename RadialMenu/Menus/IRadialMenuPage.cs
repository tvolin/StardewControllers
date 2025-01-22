namespace RadialMenu.Menus;

/// <summary>
/// A single page in one of the radial menus.
/// </summary>
/// <remarks>
/// Pages can be navigated using left/right shoulder buttons while a menu is open. Only the items on
/// the currently-active page are visible at any given time.
/// </remarks>
public interface IRadialMenuPage
{
    /// <summary>
    /// The items on this page.
    /// </summary>
    /// <remarks>
    /// Any <c>null</c> entries in the list will render as a blank wedge in the menu, e.g.
    /// representing an empty inventory slot.
    /// </remarks>
    IReadOnlyList<IRadialMenuItem?> Items { get; }

    /// <summary>
    /// Index of the selected <see cref="IRadialMenuItem"/> in the <see cref="Items"/> list, or
    /// <c>-1</c> if no selection.
    /// </summary>
    int SelectedItemIndex { get; }

    /// <summary>
    /// Checks whether the page is empty, i.e. has no non-null items.
    /// </summary>
    /// <returns><c>true</c> if the page is empty, <c>false</c> if it has valid items.</returns>
    bool IsEmpty()
    {
        return !Items.AnyNotNull();
    }
}
