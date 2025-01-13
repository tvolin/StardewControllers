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
    private const int MAX_SEARCH_RESULTS = 40;

    public bool HasLoadedGame => Game1.hasLoadedGame;
    public IEnumerable<QuickSlotPickerItemViewModel> InventoryItems =>
        Game1
            .player.Items.Where(item => item is not null)
            .Select(QuickSlotPickerItemViewModel.ForItem)
            .ToArray();

    public Vector2 ContentPanelSize
    {
        get => Pager.ContentPanelSize;
        set => Pager.ContentPanelSize = value;
    }
    public EnumSegmentsViewModel<QuickSlotItemSource> ItemSource { get; } = new();
    public string MoreResultsMessage =>
        HasMoreSearchResults ? I18n.Config_QuickSlot_Search_LimitedResults(MAX_SEARCH_RESULTS) : "";
    public PagerViewModel<PageViewModel> Pager { get; } =
        new() { Pages = [new(0), new(1), new(2)] };
    public QuickSlotConfigurationViewModel Slot { get; }

    private readonly IReadOnlyList<ParsedItemData> allItems;

    [Notify]
    private bool hasMoreSearchResults;

    [Notify]
    private IEnumerable<ModMenuItemConfigurationViewModel> modMenuItems = [];

    [Notify]
    private IReadOnlyList<QuickSlotPickerItemViewModel> searchResults = [];

    [Notify]
    private string searchText = "";

    private readonly object searchLock = new();

    private CancellationTokenSource searchCancellationTokenSource = new();

    public QuickSlotPickerViewModel(
        QuickSlotConfigurationViewModel slot,
        IReadOnlyList<ParsedItemData> allItems
    )
    {
        Slot = slot;
        this.allItems = allItems;
        UpdateSearchResults();
        ItemSource.PropertyChanged += ItemSource_PropertyChanged;
    }

    private void ItemSource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Pager.SelectedPageIndex = ItemSource.SelectedIndex;
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
                    while (results.Count < MAX_SEARCH_RESULTS && resultEnumerator.MoveNext())
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
