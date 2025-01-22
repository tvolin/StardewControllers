namespace RadialMenu.Menus;

/// <summary>
/// Details of an event raised for item activation.
/// </summary>
/// <param name="item">The item that was activated.</param>
/// <param name="result">The activation result, i.e. type of activation that occurred.</param>
public class ItemActivationEventArgs(IRadialMenuItem item, ItemActivationResult result) : EventArgs
{
    /// <summary>
    /// The item that was activated.
    /// </summary>
    public IRadialMenuItem Item => item;

    /// <summary>
    /// The activation result, i.e. type of activation that occurred.
    /// </summary>
    public ItemActivationResult Result => result;
}
