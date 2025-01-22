using RadialMenu.Graphics;

namespace RadialMenu.UI;

internal class ConfirmationViewModel
{
    /// <summary>
    /// Delegate to close the associated dialog. Must be set by the creator, and generally assigned
    /// to the corresponding <see cref="IMenuController"/> method.
    /// </summary>
    public Action? Close { get; set; }

    public ConfirmationResult Result { get; private set; } = ConfirmationResult.Cancel;

    public string CancelDescription { get; init; } = "";
    public string CancelTitle { get; set; } = "";
    public string DialogDescription { get; init; } = "";
    public string DialogTitle { get; init; } = "";
    public bool HasSprite => Sprite is not null;
    public string RevertDescription { get; init; } = "";
    public string RevertTitle { get; init; } = "";
    public string SaveDescription { get; init; } = "";
    public string SaveTitle { get; init; } = "";
    public Sprite? Sprite { get; init; }

    public void Confirm(ConfirmationResult result)
    {
        Result = result;
        Close?.Invoke();
    }
}
