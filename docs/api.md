# API

Star Control provides a [mod API](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs) for other mods to register pages and actions in the [Mod Menu](controller-hud.md#pie-menus) and subsequently make them available as [Quick Actions](controller-hud.md#quick-actions) or [Instant Actions](instant-actions.md).

The latest released public API definition can always be found at [PublicApi/IStarControlApi.cs](https://github.com/focustense/StardewControllers/blob/master/PublicApi/IStarControlApi.cs), and at a high level there are two basic methods:

- `RegisterItems` creates one or more actions (items) that are added to the player's [Item Library](configuration.md#library-items), giving them as much flexibility as they want in terms of how and where to place and activate each one.
- `RegisterCustomMenuPage` creates an entire page of items (per player) with an order that is strictly managed by the API client. Users cannot change or reorder items on the page owned by your mod, though they can disable or reorder the entire page among other pages in the [Mod Settings](configuration.md#mods).

## Item Registration

The `RegisterItems` API is best for mods that simply want to provide a more convenient mechanism for users than the low-tech [key-press simulation](configuration.md#custom-items) used by custom items, or for mods that do **not** use [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) for their configuration which would make it more difficult for players to keep the keybind in sync.

[A Fishing Sea](https://www.nexusmods.com/stardewvalley/mods/27665) does this because it has its own configuration screen based on [Stardew UI](https://www.nexusmods.com/stardewvalley/mods/28870), including a configurable keybind.

!!! example

    ```cs
    public void RegisterStarControlIntegration()
    {
        var starControl = Helper.ModRegistry.GetApi<IStarControlApi>("focustense.StarControl");
        starControl?.RegisterItems(ModManifest, [new ToggleOverlaysItem()]);
    }
    
    class ToggleOverlaysItem : IRadialMenuItem
    {
        public string Id { get; } = "focustense.FishinC.ToggleOverlays";
        public string Title => I18n.Settings_ModTitle();
        public string Description => I18n.StarControl_ToggleOverlays_Description();
        public Texture2D? Texture => icon.GetTexture();
        public Rectangle? SourceRectangle => icon.GetSourceRect();
        
        private readonly ParsedItemData icon =
            ItemRegistry.GetDataOrErrorItem("(O)147"); // Herring

        public ItemActivationResult Activate(
            Farmer who,
            DelayedActions delayedActions,
            ItemActivationType activationType = ItemActivationType.Primary
        )
        {
            if (delayedActions != DelayedActions.None)
            {
                return ItemActivationResult.Delayed;
            }
            ToggleOverlays();
            return ItemActivationResult.Custom;
        }
        
        private void ToggleOverlays() { ... }
    }
    ```

Implementations of `IRadialMenuItem` can be generic, like Star Control's own [`ModMenuItem`](https://github.com/focustense/StardewControllers/blob/master/StarControl/Menus/ModMenuItem.cs), and many mods might be able to copy this class verbatim; or they can be specific implementations per action, as in the example above, which is often more convenient to implement for mods that will only ever register one or two actions.

!!! warning "Important"

    **Always include a unique `Id` in your registration.**

    The `Id` property is optional to implement because the `IRadialMenuItem` interface is shared with [page registration](#page-registration), which does not need explicit IDs because it manages the items directly. When using the item registration API, failing to specify a unique ID will cause players to be unable to add it to their menus, quick slots, etc.

## Page Registration

A page in a [pie menu](controller-hud.md#pie-menus) is the entire set of items that can be shown at once. Players can switch pages with :prompts-left-button:{.medium}/:prompts-right-button:{.medium} or whichever alternate buttons are configured.

Items are a "loose" integration while pages are a "tight" integration. By registering pages, mods can assert more control over the user experience, but may have to do more work to actively maintain the pages.

Typical use cases for the page-based APIs include:

- Progression systems, e.g. actions that only become available as certain levels or milestones are reached;
- Contextual actions that only show up with a specific tool selected, or in specific locations, etc.
- Mods with their own distinct concept of [active/selected items](controller-hud.md#components), which is implemented at the page rather than item level.

A detailed example of this implementation can be found in Star Control's [API test mod](https://github.com/focustense/StardewControllers/tree/master/StarControl.ApiTestMod). The following is an excerpt:

!!! example

    ```cs
    public void RegisterStarControlMenus()
    {
        var starControl = Helper.ModRegistry.GetApi<IStarControlApi>("focustense.StarControl");
        if (starControl is null)
        {
            return;
        }
        var charactersTexture = helper.ModContent.Load<Texture2D>("assets/characters.png");
        starControl?.RegisterCustomMenuPage(
            ModManifest,
            "characters",
            new MenuPageFactory(() => new CharacterPage(charactersTexture)));
    }
    
    class MenuPageFactory(Func<IRadialMenuPage> selector) : IRadialMenuPageFactory
    {
        public IRadialMenuPage CreatePage(Farmer _who)
        {
            return selector();
        }
    }

    // Example of a page with enhanced features: item selection and animated sprites.
    internal class CharacterPage : IRadialMenuPage
    {
        // Does not have to be immutable; the items in the list can be changed at any time.
        // If modifying the list, be careful to use a SelectedItemIndex implementation that
        // maintains consistency.
        public IReadOnlyList<IRadialMenuItem> Items { get; }

        // In a real-world implementation, the selection action would probably manipulate
        // some actual mod data and this property would be a reference to (or indexOf) the
        // actual selection.
        public int SelectedItemIndex { get; private set; }

        private readonly Texture2D atlasTexture;

        public CharacterPage(Texture2D atlasTexture)
        {
            this.atlasTexture = atlasTexture;
            Items = [
                CreateItem(0, 2, I18n.Character_Soldier_Title, I18n.Character_Soldier_Description),
                CreateItem(1, 0, I18n.Character_BlackMage_Title, I18n.Character_BlackMage_Description),
                CreateItem(2, 7, I18n.Character_WhiteMage_Title, I18n.Character_WhiteMage_Description),
                CreateItem(3, 1, I18n.Character_Fighter_Title, I18n.Character_Fighter_Description),
                CreateItem(4, 5, I18n.Character_Thief_Title, I18n.Character_Thief_Description),
                CreateItem(5, 4, I18n.Character_Spellsword_Title, I18n.Character_Spellsword_Description),
                CreateItem(6, 3, I18n.Character_Royal_Title, I18n.Character_Royal_Description),
                CreateItem(7, 6, I18n.Character_Priest_Title, I18n.Character_Priest_Description),
            ];
        }

        private CharacterMenuItem CreateItem(
            int menuIndex,
            int spriteIndex,
            Func<string> name,
            Func<string> description
        )
        {
            var x = (spriteIndex % 2) * 64;
            var y = spriteIndex / 2 * 32;
            var sourceRect = new Rectangle(x, y, 32, 32);
            return new(
                name,
                description,
                atlasTexture,
                sourceRect,
                () => SelectedItemIndex = menuIndex);
        }
    }

    internal class CharacterMenuItem(
        Func<string> name,
        Func<string> description,
        Texture2D texture,
        Rectangle baseSourceRect,
        Action onSelect)
        : IRadialMenuItem
    {
        private static readonly TimeSpan AnimationInterval = TimeSpan.FromMilliseconds(250);

        public string Title => name();
        public string Description => description();
        public Texture2D Texture => texture;
        public Rectangle? SourceRectangle => GetAnimatedSourceRect();

        public ItemActivationResult Activate(
            Farmer who,
            DelayedActions delayedActions,
            ItemActivationType activationType
        )
        {
            if (delayedActions != DelayedActions.None)
            {
                return ItemActivationResult.Delayed;
            }
            onSelect();
            return ItemActivationResult.Selected;
        }

        private Rectangle GetAnimatedSourceRect()
        {
            var frameIndex = (int)(Game1.currentGameTime.TotalGameTime / AnimationInterval) % 2;
            var rect = baseSourceRect;
            if (frameIndex > 0)
            {
                rect.Offset(frameIndex * baseSourceRect.Width, 0);
            }
            return rect;
        }
    }
    ```
    
This example implements both selection states and animation, which is about as complex as any page implementation is likely to get.

### Invalidation

The final, rarely-used API is `InvalidatePage`, which takes the mod manifest (same as other APIs) and the ID of the registered page, which in the earlier example would be "characters".

Calling `InvalidatePage` forces Star Control to recreate that page, and the items on it. This can be useful if the calling mod is aware of some change that Star Control itself cannot easily detect, such as with the aforementioned progression or context-sensitive menus.

!!! warning

    The presence of the `InvalidatePage` method does not mean mods have **exclusive** control over when to update their pages. Menus can also be invalidated due to configuration changes, loading a saved game, and possibly other triggers. Any mod that uses the `RegisterCustomMenuPage` method and provides an `IRadialMenuPageFactory` implementation must be prepared for the page factory to run **at any time**.

    Invalidation also does not force `CreatePage` to be called immediately. It may be called at any time starting from the moment of invalidation up to the moment at which the specific page is shown to the player.