using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewValley.ItemTypeDefinitions;

namespace RadialMenu.UI;

internal enum QuickSlotItemSource
{
    Inventory,
    GameItems,
    ModItems,
}

internal partial class QuickSlotPickerViewModel
{
    private const int MAX_RESULTS = 40;

    private static readonly Color AssignedColor = new(50, 100, 50);
    private static readonly Color UnassignedColor = new(60, 60, 60);

    public Color AllModPagesTint =>
        ModMenuPageIndex < 0 ? new Color(0xaa, 0xcc, 0xee) : Color.White;
    public Action Close { get; set; } = () => { };

    public Vector2 ContentPanelSize
    {
        get => Pager.ContentPanelSize;
        set => Pager.ContentPanelSize = value;
    }

    public Color CurrentAssignmentColor =>
        Slot.ItemData is not null || Slot.ModAction is not null ? AssignedColor : UnassignedColor;
    public string CurrentAssignmentLabel =>
        Slot.ItemData is not null || Slot.ModAction is not null
            ? I18n.Config_QuickSlot_Assigned_Title()
            : I18n.Config_QuickSlot_Unassigned_Title();
    public bool HasLoadedGame => Game1.hasLoadedGame;
    public bool HasMoreModItems =>
        ModMenuPageIndex < 0 && ModMenuPages.Sum(page => page.Items.Count) > MAX_RESULTS;
    public IEnumerable<QuickSlotPickerItemViewModel> InventoryItems =>
        Game1
            .player.Items.Where(item => item is not null)
            .Select(QuickSlotPickerItemViewModel.ForItem)
            .ToArray();
    public EnumSegmentsViewModel<QuickSlotItemSource> ItemSource { get; } = new();
    public IEnumerable<QuickSlotPickerItemViewModel> ModMenuItems =>
        (
            ModMenuPageIndex < 0
                ? ModMenuPages.SelectMany(page => page.Items).Take(MAX_RESULTS)
                : ModMenuPages[ModMenuPageIndex].Items
        ).Select(QuickSlotPickerItemViewModel.ForModAction);
    public IReadOnlyList<ModMenuPageConfigurationViewModel> ModMenuPages { get; }
    public string MoreModItemsMessage =>
        HasMoreModItems ? I18n.Config_QuickSlot_ModItems_LimitedResults(MAX_RESULTS) : "";
    public string MoreResultsMessage =>
        HasMoreSearchResults ? I18n.Config_QuickSlot_Search_LimitedResults(MAX_RESULTS) : "";
    public PagerViewModel<PageViewModel> Pager { get; } =
        new() { Pages = [new(0), new(1), new(2)] };
    public QuickSlotConfigurationViewModel Slot { get; }

    private readonly IReadOnlyList<ParsedItemData> allItems;

    [Notify(Setter.Private)]
    private bool hasMoreSearchResults;

    [Notify]
    private int modMenuPageIndex = -1; // Use -1 for "all", if they fit

    [Notify(Setter.Private)]
    private IReadOnlyList<QuickSlotPickerItemViewModel> searchResults = [];

    [Notify]
    private string searchText = "";

    private readonly object searchLock = new();

    private CancellationTokenSource searchCancellationTokenSource = new();

    public QuickSlotPickerViewModel(
        QuickSlotConfigurationViewModel slot,
        IReadOnlyList<ParsedItemData> allItems,
        IReadOnlyList<ModMenuPageConfigurationViewModel> modMenuPages
    )
    {
        Slot = slot;
        this.allItems = allItems;
        ModMenuPages = modMenuPages;
        UpdateSearchResults();
        ItemSource.PropertyChanged += ItemSource_PropertyChanged;
    }

    public void AssignItem(QuickSlotPickerItemViewModel item)
    {
        item.UpdateSlot(Slot);
        Close();
    }

    public void ClearAssignment()
    {
        Slot.ItemData = null;
        Slot.ModAction = null;
        Close();
    }

    public bool HandleButtonPress(SButton button)
    {
        if (!Pager.HandleButtonPress(button))
        {
            return false;
        }
        ItemSource.SelectedIndex = Pager.SelectedPageIndex;
        return true;
    }

    public void SelectModMenuPage(int index)
    {
        if (index == ModMenuPageIndex)
        {
            return;
        }
        Game1.playSound("smallSelect");
        ModMenuPageIndex = index;
    }

    private void ItemSource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Pager.SelectedPageIndex = ItemSource.SelectedIndex;
    }

    private void OnModMenuPageIndexChanged(int oldValue, int newValue)
    {
        if (oldValue >= 0)
        {
            ModMenuPages[oldValue].Selected = false;
        }
        if (newValue >= 0)
        {
            ModMenuPages[newValue].Selected = true;
        }
    }

    private void OnSearchTextChanged()
    {
        UpdateSearchResults();
    }

    private void UpdateSearchResults()
    {
        searchCancellationTokenSource.Cancel();
        searchCancellationTokenSource = new();
        var cancellationToken = searchCancellationTokenSource.Token;
        var searchTask = Task.Run(
            () => allItems.Search(SearchText, cancellationToken),
            cancellationToken
        );
        searchTask.ContinueWith(
            t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Log($"Failed searching for items: {t.Exception}", LogLevel.Error);
                    return;
                }
                if (t.IsCanceled)
                {
                    return;
                }
                lock (searchLock)
                {
                    var results = new List<QuickSlotPickerItemViewModel>();
                    // Results can easily be limited using .Take(limit), but we also need to know if there are more,
                    // which no Linq extension can tell us.
                    using var resultEnumerator = t.Result.GetEnumerator();
                    while (results.Count < MAX_RESULTS && resultEnumerator.MoveNext())
                    {
                        results.Add(
                            QuickSlotPickerItemViewModel.ForItemData(resultEnumerator.Current)
                        );
                    }
                    SearchResults = results;
                    HasMoreSearchResults = resultEnumerator.MoveNext();
                }
            },
            cancellationToken
        );
    }
}
