using System.Reflection;
using RadialMenu.Api;
using RadialMenu.Config;
using RadialMenu.Gmcm;
using RadialMenu.Graphics;
using RadialMenu.Input;
using RadialMenu.Menus;
using RadialMenu.Patches;
using RadialMenu.UI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace RadialMenu;

public class ModEntry : Mod
{
    private const string GMCM_MOD_ID = "spacechase0.GenericModConfigMenu";

    private readonly PerScreen<RadialMenuController> menuController;
    private readonly PageRegistry pageRegistry = new();

    private RadialMenuController MenuController => menuController.Value;

    // Global state
    private RadialMenuApi api = null!;
    private ModConfig config = null!;
    private Gmcm.GenericModConfigMenu? configMenu;
    private IGenericModMenuConfigApi? configMenuApi;
    private GenericModConfigSync? gmcmSync;
    private KeybindActivator keybindActivator = null!;

    public ModEntry()
    {
        menuController = new(CreateMenuController);
    }

    public override void Entry(IModHelper helper)
    {
        Logger.Monitor = Monitor;
        config = Helper.ReadConfig<ModConfig>();
        Logger.Config = config.Debug;
        I18n.Init(helper.Translation);
        var builtInItems = new BuiltInItems(ModManifest);
        pageRegistry.RegisterItem(ModManifest, builtInItems.MainMenu);
        pageRegistry.RegisterItem(ModManifest, builtInItems.Journal);
        pageRegistry.RegisterItem(ModManifest, builtInItems.Map);
        pageRegistry.RegisterItem(ModManifest, builtInItems.Mail);
        pageRegistry.RegisterItem(ModManifest, builtInItems.Crafting);
        api = new(pageRegistry, Monitor);
        keybindActivator = new(helper.Input);
        Sound.Enabled = config.Sound.EnableUiSounds;
        ConfigurationViewModel.Saved += ConfigurationViewModel_Saved;

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        // Ensure menu gets updated at the right time.
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        // For optimal latency: handle input before the Update loop, perform actions/rendering after.
        helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Display.RenderedHud += Display_RenderedHud;

        Patcher.PatchAll(ModManifest);
        GamePatches.SuppressRightStickChatBox = config.Input.SuppressRightStickChatBox;
    }

    public override object? GetApi()
    {
        return api;
    }

    private void ConfigurationViewModel_Saved(object? sender, EventArgs e)
    {
        if (menuController.IsActiveForScreen())
        {
            menuController.Value.Invalidate();
        }
        Sound.Enabled = config.Sound.EnableUiSounds;
        GamePatches.SuppressRightStickChatBox = config.Input.SuppressRightStickChatBox;
    }

    [EventPriority(EventPriority.Low)]
    private void Display_RenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (MenuController.Enabled)
        {
            MenuController.Draw(e.SpriteBatch);
        }
    }

    [EventPriority(EventPriority.Low - 10)]
    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        configMenuApi = Helper.ModRegistry.GetApi<IGenericModMenuConfigApi>(GMCM_MOD_ID);
        LoadGmcmKeybindings();

        var viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
        if (viewEngine is null)
        {
            Monitor.Log(
                "StardewUI Framework is not installed; some aspects of the mod will not be configurable in-game.",
                LogLevel.Warn
            );
            return;
        }
        viewEngine.RegisterCustomData($"Mods/{ModManifest.UniqueID}", "assets/ui/data");
        viewEngine.RegisterSprites($"Mods/{ModManifest.UniqueID}/Sprites", "assets/ui/sprites");
        viewEngine.RegisterViews($"Mods/{ModManifest.UniqueID}/Views", "assets/ui/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
        viewEngine.PreloadAssets();
        viewEngine.PreloadModels(
            typeof(ConfigurationViewModel),
            typeof(QuickSlotPickerViewModel),
            typeof(ConfirmationViewModel)
        );
        ViewEngine.Instance = viewEngine;
        ViewEngine.ViewAssetPrefix = $"Mods/{ModManifest.UniqueID}/Views";

        RegisterConfigMenu();
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        menuController.ResetAllScreens();
    }

    private void GameLoop_UpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        var wasActive = MenuController.IsMenuActive;
        MenuController.Enabled = Context.CanPlayerMove;
        MenuController.Update(Game1.currentGameTime.ElapsedGameTime);
        if (!wasActive && MenuController.IsMenuActive)
        {
            Game1.player.completelyStopAnimatingOrDoingAction();
            Game1.freezeControls = true;
        }
        else if (wasActive && !MenuController.IsMenuActive)
        {
            Game1.freezeControls = false;
        }
    }

    private void Input_ButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (
            Context.IsPlayerFree
            && e.Pressed.Contains(SButton.F10)
            && ViewEngine.Instance is not null
        )
        {
            OpenConfigMenu();
        }
    }

    private void MenuController_ItemActivated(object? sender, ItemActivationEventArgs e)
    {
        // Because we allow "internal" page changes within the radial menu that don't go through
        // Farmer.shiftToolbar (on purpose), the selected index can now be on a non-active page.
        // To avoid confusing the game's UI, check for this condition and switch to the backpack
        // page that actually does contain the index.
        if (
            e.Result != ItemActivationResult.Selected
            || Game1.player.CurrentToolIndex < GameConstants.BACKPACK_PAGE_SIZE
        )
        {
            return;
        }
        var items = Game1.player.Items;
        var currentPage = Game1.player.CurrentToolIndex / GameConstants.BACKPACK_PAGE_SIZE;
        var indexOnPage = Game1.player.CurrentToolIndex % GameConstants.BACKPACK_PAGE_SIZE;
        var newFirstIndex = currentPage * GameConstants.BACKPACK_PAGE_SIZE;
        var itemsBefore = items.GetRange(0, newFirstIndex);
        var itemsAfter = items.GetRange(newFirstIndex, items.Count - newFirstIndex);
        items.Clear();
        items.AddRange(itemsAfter);
        items.AddRange(itemsBefore);
        Game1.player.CurrentToolIndex = indexOnPage;
    }

    private void Player_InventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        // We don't need to invalidate the menu if the only change was a quantity, since that's only
        // read at paint time. Any items added/removed, however, will change the layout/items in the
        // menu.
        if (e.Added.Any() || e.Removed.Any())
        {
            MenuController.Invalidate();
        }
    }

    private void ActivateModMenuItem(ModMenuItemConfiguration item)
    {
        if (!item.Keybind.IsBound)
        {
            Game1.showRedMessage(I18n.Error_MissingBinding());
            return;
        }
        keybindActivator.Activate(item.Keybind);
    }

    private RadialMenuController CreateMenuController()
    {
        var player = Game1.player;
        var menuPainter = new RadialMenuPainter(Game1.graphics.GraphicsDevice, config.Style);
        var inventoryToggle = new MenuToggle(
            Helper.Input,
            config.Input,
            c => c.InventoryMenuButton
        );
        var inventoryMenu = new InventoryMenu(inventoryToggle, player, config.Items);
        var registeredPages = pageRegistry.CreatePageList(player);
        var modMenuToggle = new MenuToggle(Helper.Input, config.Input, c => c.ModMenuButton);
        var settingsSprite = new Lazy<Sprite?>(Sprites.Settings);
        var settingsItem = new ModMenuItem(
            id: "focustense.StarControl.Settings",
            title: I18n.ModMenu_SettingsItem_Name,
            description: I18n.ModMenu_SettingsItem_Description,
            texture: () => settingsSprite.Value?.Texture,
            sourceRectangle: () => settingsSprite.Value?.SourceRect,
            activate: (_, _, _) =>
            {
                if (!ViewEngine.IsInstalled)
                {
                    Game1.showRedMessage(I18n.Error_MissingStardewUI());
                    return ItemActivationResult.Ignored;
                }
                OpenConfigMenu();
                return ItemActivationResult.Custom;
            }
        );
        var modMenu = new ModMenu(
            modMenuToggle,
            config,
            settingsItem,
            ActivateModMenuItem,
            registeredPages,
            pageRegistry.StandaloneItems.Select(x => x.Item)
        );
        var quickSlotRenderer = new QuickSlotRenderer(Game1.graphics.GraphicsDevice, config);
        var quickSlotController = new QuickSlotController(
            Helper.Input,
            config,
            Game1.player,
            modMenu,
            quickSlotRenderer
        );
        var menuController = new RadialMenuController(
            Helper.Input,
            config,
            Game1.player,
            menuPainter,
            quickSlotController,
            inventoryMenu,
            modMenu
        );
        menuController.ItemActivated += MenuController_ItemActivated;
        return menuController;
    }

    private void LoadGmcmKeybindings()
    {
        const string LOADER_METHOD = "Load";
        const string LOADER_TYPE = "RadialMenu.Gmcm.Loader";

        if (configMenuApi is null)
        {
            Logger.Log(
                $"Couldn't read global keybindings; mod {GMCM_MOD_ID} is not installed.",
                LogLevel.Warn
            );
            return;
        }
        Logger.Log(
            LogCategory.GmcmSync,
            "Generic Mod Config Menu is loaded; reading keybindings.",
            LogLevel.Info
        );
        try
        {
            var gmcmExtensionPath = Path.Combine(
                Helper.DirectoryPath,
                "assets",
                "extensions",
                "RadialMenu.Gmcm.dll"
            );
            var gmcmExtensionAssembly = Assembly.LoadFile(gmcmExtensionPath);
            var loaderType = gmcmExtensionAssembly.GetType("RadialMenu.Gmcm.Loader");
            if (loaderType is null)
            {
                Logger.Log(
                    $"Failed to initialize GMCM extension: Type {LOADER_TYPE} was not found.",
                    LogLevel.Error
                );
                return;
            }
            var loadMethod = loaderType.GetMethod(
                LOADER_METHOD,
                BindingFlags.Static | BindingFlags.Public
            );
            if (loadMethod is null)
            {
                Logger.Log(
                    $"Failed to initialize GMCM extension: Method {LOADER_METHOD} was not "
                        + $"found on type {LOADER_TYPE}.",
                    LogLevel.Error
                );
                return;
            }
            Action<Action> openRealConfigMenu = onClose =>
                OpenConfigMenu(asRoot: true, onClose: onClose);
            loadMethod.Invoke(
                null,
                [ModManifest, openRealConfigMenu, Monitor, config.Debug.EnableGmcmDetailedLogging]
            );
        }
        catch (Exception ex)
        {
            Logger.Log(
                "Error loading GMCM extension; keybindings will not be synced.\n" + ex,
                LogLevel.Error
            );
            return;
        }
        if (IGenericModConfigKeybindings.Instance is not { } gmcmBindings)
        {
            Logger.Log(
                "Failed to load keybindings from installed version of GMCM. "
                    + "Check previous log messages for details.",
                LogLevel.Error
            );
            return;
        }
        Logger.Log(LogCategory.GmcmSync, "Finished reading keybindings from GMCM.", LogLevel.Info);
        gmcmSync = new(config, gmcmBindings);
        if (gmcmSync.SyncAll())
        {
            Helper.WriteConfig(config);
        }
        gmcmBindings.Saved += (_, e) =>
        {
            Logger.Log(
                LogCategory.GmcmSync,
                "Detected save from GMCM; re-syncing options.",
                LogLevel.Info
            );
            if (gmcmSync.SyncAll(e.Mod))
            {
                Helper.WriteConfig(config);
            }
        };
    }

    private void OpenConfigMenu(bool asRoot = false, Action? onClose = null)
    {
        ConfigurationMenu.Open(Helper, config, pageRegistry, asRoot: asRoot, onClose: onClose);
    }

    private void RegisterConfigMenu()
    {
        if (configMenuApi is null)
        {
            return;
        }
        configMenu = new(
            configMenuApi,
            ModManifest,
            onClose => OpenConfigMenu(asRoot: true, onClose: onClose)
        );
        configMenu.Setup();
    }
}
