namespace StarControl.UI;

/// <summary>
/// The result of a confirmation dialog.
/// </summary>
public enum ConfirmationResult
{
    /// <summary>
    /// The action was accepted, e.g. user confirmed saving changes.
    /// </summary>
    Yes = 1,

    /// <summary>
    /// The action was rejected, e.g. user confirmed discarding changes.
    /// </summary>
    No = 2,

    /// <summary>
    /// The action was cancelled and no further action should be taken, other than closing the
    /// confirmation dialog itself.
    /// </summary>
    Cancel = 0,
}
