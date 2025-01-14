using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;

namespace RadialMenu.UI;

internal partial class PagerViewModel<T>
    where T : PageViewModel
{
    public T? SelectedPage =>
        SelectedPageIndex >= 0 && SelectedPageIndex < Pages.Count ? Pages[SelectedPageIndex] : null;

    [Notify]
    private Vector2 contentPanelSize;

    [Notify]
    private ObservableCollection<T> pages = [];

    [Notify]
    private int selectedPageIndex = -1;

    public bool HandleButtonPress(SButton button)
    {
        switch (button)
        {
            case SButton.LeftTrigger:
                SelectedPageIndex = (SelectedPageIndex + Pages.Count - 1) % Pages.Count;
                return true;
            case SButton.RightTrigger:
                SelectedPageIndex = (SelectedPageIndex + 1) % Pages.Count;
                return true;
            default:
                return false;
        }
    }

    public void SelectPage(int index)
    {
        if (index < 0 || index == SelectedPageIndex)
        {
            return;
        }
        Game1.playSound("smallSelect");
        SelectedPageIndex = index;
    }

    private void OnContentPanelSizeChanged()
    {
        UpdatePageTransforms();
    }

    private void OnPagesChanged()
    {
        SelectedPageIndex = Pages.Count > 0 ? 0 : -1;
    }

    private void OnSelectedPageIndexChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0 && oldValue < Pages.Count)
        {
            Pages[oldValue].Selected = false;
        }
        if (newValue >= 0 && newValue < Pages.Count)
        {
            Pages[newValue].Selected = true;
        }
        UpdatePageTransforms();
    }

    private void UpdatePageTransforms()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];
            if (ContentPanelSize != Vector2.Zero)
            {
                page.Transform = new(new(ContentPanelSize.X * (i - SelectedPageIndex), 0));
                // Setting visible only when the page - or a subsequent page - is actually selected
                // prevents awkward transition animations from playing when the menu is first shown.
                if (i <= SelectedPageIndex)
                {
                    page.Visible = true;
                }
            }
        }
    }
}

internal partial class PageViewModel(int index)
{
    public int DisplayIndex => Index + 1; // 1-based index if displayed in a numeric page list
    public int Index => index;

    [Notify]
    private bool selected;

    [Notify]
    private Transform transform = new(Vector2.Zero);

    [Notify]
    private bool visible;
}

internal record Transform(Vector2 Translation, Vector2 Scale)
{
    public Transform(Vector2 translation)
        : this(translation, Vector2.One) { }
}
