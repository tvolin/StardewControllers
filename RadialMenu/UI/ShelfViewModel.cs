using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;

namespace RadialMenu.UI;

internal partial class ShelfViewModel<T>
{
    public static readonly ShelfViewModel<T> Empty = new([], 0, 0, 0, 0, 0);

    private static readonly Transform DefaultTransform = new(Vector2.Zero);

    public T? SelectedItem => items.Count > 0 ? items[SelectedIndex] : default;

    [Notify]
    private Vector2 layoutSize;

    [Notify]
    private int selectedIndex;

    [Notify]
    private ObservableCollection<ShelfItemViewModel<T>> visibleItems;

    // Item duplication places copies of one or more items at both the beginning and end of the
    // visible list, allowing for the appearance of smooth unidirectional scrolling even when
    // wrapping to the opposite side.
    //
    // Duplication is imperceptible when the number of items is at least the target visible count;
    // that is, the user will never actually see two copies at once, except possibly in the context
    // of an animation in which one is going off-screen while the other is coming on-screen.
    //
    // It is more complicated when there are too few items. For example, repeating the same single
    // item five times in a row would look confusing and broken. On the other hand, if we have
    // exactly four items to fill a five-item shelf, then even though one visibly-duplicated item
    // could result, it is still better to duplicate it, because otherwise we can't do simulated
    // wraparound AND one of the items is potentially out of view when scrolled to the very first or
    // very last item.
    //
    // The heuristic to use here is half the shelf size plus one, which is the exact threshold above
    // which it's not possible to see all items at maximum scroll. Any count at or below this, and
    // all items are always visible no matter which one occupies the center slot, therefore it's
    // better to avoid duplication and simply scroll in the opposite direction when wrapping.
    //
    // Note that allowing duplication in practice is synonymous with the ability to wrap without
    // reversing the scroll direction. Whether or not duplication actually occurs, having it enabled
    // ensures that the entire visible count + buffer size is filled and therefore looping around is
    // always assumed to be seamless.
    private readonly bool allowDuplication;

    private readonly float centerMargin;
    private readonly float itemDistance;
    private readonly IReadOnlyList<T> items;

    private int visibleCenterIndex;

    public ShelfViewModel(
        IReadOnlyList<T> items,
        int visibleSize,
        int bufferSize,
        float centerMargin,
        float itemDistance,
        int initialSelectedIndex
    )
    {
        this.items = items;
        this.centerMargin = centerMargin;
        this.itemDistance = itemDistance;
        allowDuplication = items.Count > visibleSize / 2 + 1;
        var visibleCount = allowDuplication ? visibleSize + bufferSize * 2 : items.Count;
        selectedIndex = initialSelectedIndex;
        var visibleItems = new ShelfItemViewModel<T>[visibleCount];
        visibleCenterIndex = allowDuplication ? (visibleCount - 1) / 2 : selectedIndex;
        var itemStartIndex = selectedIndex - visibleCenterIndex;
        for (int i = 0; i < visibleItems.Length; i++)
        {
            var item = GetItemAtIndex(itemStartIndex + i);
            visibleItems[i] = new(item) { Transform = GetItemTransform(i - visibleCenterIndex) };
        }
        this.visibleItems = new(visibleItems);
    }

    public bool HandleButton(SButton button)
    {
        switch (button)
        {
            case SButton.DPadLeft:
            case SButton.LeftThumbstickLeft:
                MovePrevious();
                return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
                MoveNext();
                return true;
            default:
                return false;
        }
    }

    public void MoveNext(bool withSound = true)
    {
        if (items.Count <= 1)
        {
            return;
        }
        if (withSound)
        {
            Game1.playSound("shiny4");
        }
        selectedIndex = (selectedIndex + 1) % items.Count;
        if (allowDuplication)
        {
            VisibleItems.RemoveAt(0);
            var trailingItem = GetItemAtIndex(
                selectedIndex + visibleItems.Count - visibleCenterIndex
            );
            VisibleItems.Add(new(trailingItem));
        }
        else
        {
            visibleCenterIndex = (visibleCenterIndex + 1) % visibleItems.Count;
        }
        UpdateTransforms();
    }

    public void MovePrevious(bool withSound = true)
    {
        if (items.Count <= 1)
        {
            return;
        }
        if (withSound)
        {
            Game1.playSound("shiny4");
        }
        selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
        if (allowDuplication)
        {
            VisibleItems.RemoveAt(VisibleItems.Count - 1);
            var leadingItem = GetItemAtIndex(selectedIndex - visibleCenterIndex);
            VisibleItems.Insert(0, new(leadingItem));
        }
        else
        {
            visibleCenterIndex = (visibleCenterIndex - 1 + visibleItems.Count) % visibleItems.Count;
        }
        UpdateTransforms();
    }

    public void ScrollToPoint(Vector2 position)
    {
        var x = position.X - layoutSize.X / 2;
        var firstItemDistance = itemDistance / 2 + centerMargin;
        if (x >= -firstItemDistance && x <= firstItemDistance)
        {
            return;
        }
        var absDistance = (int)((MathF.Abs(x) - firstItemDistance) / itemDistance) + 1;
        for (int i = 0; i < absDistance; i++)
        {
            if (x > 0)
            {
                MoveNext(withSound: false);
            }
            else
            {
                MovePrevious(withSound: false);
            }
        }
    }

    private T GetItemAtIndex(int index)
    {
        var validIndex = (index + items.Count) % items.Count;
        return items[validIndex];
    }

    private Transform GetItemTransform(int offsetFromCenter)
    {
        if (offsetFromCenter == 0)
        {
            return DefaultTransform;
        }
        float translateX = MathF.CopySign(
            centerMargin + Math.Abs(offsetFromCenter) * itemDistance,
            offsetFromCenter
        );
        return new(new(translateX, 0));
    }

    private void UpdateTransforms()
    {
        for (int i = 0; i < visibleItems.Count; i++)
        {
            visibleItems[i].Transform = GetItemTransform(i - visibleCenterIndex);
        }
    }
}

internal partial class ShelfItemViewModel<T>(T data)
{
    public T Data { get; } = data;

    [Notify]
    private Transform transform = new(Vector2.Zero);
}
