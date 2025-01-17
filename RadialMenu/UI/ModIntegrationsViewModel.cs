using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;

namespace RadialMenu.UI;

internal partial class ModIntegrationsViewModel
{
    // Helps avoid some instability due to the frame delays between binding update (observable collection change),
    // subsequent layout and the next drag event. Without this, the drag handler can oscillate temporarily at the moment
    // the cursor goes out of bounds, especially noticeable when changing direction.
    // Since people generally can't respond or even perceive faster than about 50 ms, the delay should not be a problem.
    private const int DRAG_THROTTLE_FRAMES = 3;

    [Notify]
    private ObservableCollection<ModPriorityViewModel> priorities = [];

    private int dragFrameCount;

    public void BeginDrag(ModPriorityViewModel mod)
    {
        if (mod.Dragging)
        {
            return;
        }
        Game1.playSound("dwop");
        dragFrameCount = 0;
        mod.Dragging = true;
    }

    public void EndDrag(ModPriorityViewModel mod)
    {
        mod.Dragging = false;
    }

    public void HandleDrag(ModPriorityViewModel mod, Vector2 position)
    {
        if (dragFrameCount < DRAG_THROTTLE_FRAMES)
        {
            return;
        }
        if (position.Y < 0)
        {
            MoveUp(mod, fromDrag: true);
            dragFrameCount = 0;
        }
        else if (position.Y > mod.LayoutSize.Y)
        {
            MoveDown(mod, fromDrag: true);
            dragFrameCount = 0;
        }
    }

    public bool MoveDown(ModPriorityViewModel mod, bool fromDrag = false)
    {
        int priority = Priorities.IndexOf(mod);
        if (priority < 0 || priority >= Priorities.Count - 1)
        {
            return false;
        }
        Game1.playSound(fromDrag ? "stoneStep" : "drumkit5");
        Priorities.Move(priority, priority + 1);
        return true;
    }

    public bool MoveUp(ModPriorityViewModel mod, bool fromDrag = false)
    {
        int priority = Priorities.IndexOf(mod);
        if (priority <= 0)
        {
            return false;
        }
        Game1.playSound(fromDrag ? "stoneStep" : "drumkit3");
        Priorities.Move(priority, priority - 1);
        return true;
    }

    public void Update()
    {
        if (dragFrameCount < DRAG_THROTTLE_FRAMES)
        {
            dragFrameCount++;
        }
    }
}

internal partial class ModPriorityViewModel(string id)
{
    public string Id { get; } = id;

    [Notify]
    private string description = "";

    [Notify]
    private bool dragging;

    [Notify]
    private bool enabled = true;

    [Notify]
    private Vector2 layoutSize;

    [Notify]
    private string name = "";

    public static ModPriorityViewModel Self(string id, ItemsConfigurationViewModel items)
    {
        var result = new ModPriorityViewModel(id)
        {
            Name = I18n.ModTitle(),
            Description = GetDescription(items.Pager.Pages.Count),
        };
        items.Pager.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PagerViewModel<PageViewModel>.Pages))
            {
                result.Description = GetDescription(items.Pager.Pages.Count);
            }
        };
        items.Pager.Pages.CollectionChanged += (_, _) =>
            result.Description = GetDescription(items.Pager.Pages.Count);
        return result;

        string GetDescription(int pageCount)
        {
            return I18n.Config_ModIntegrations_CustomItems(items.Pager.Pages.Count);
        }
    }
}
