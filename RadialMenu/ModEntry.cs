using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RadialMenu.Config;
using RadialMenu.Gmcm;
using RadialMenu.Graphics;
using RadialMenu.Input;
using RadialMenu.Menus;
using RadialMenu.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace RadialMenu;

public class ModEntry : Mod
{
    private const string GMCM_MOD_ID = "spacechase0.GenericModConfigMenu";
    private const string GMCM_OPTIONS_MOD_ID = "jltaylor-us.GMCMOptions";

    private readonly PerScreen<RadialMenuController> menuController;
    private readonly PageRegistry pageRegistry = new();

    private RadialMenuController MenuController => menuController.Value;

    // Global state
    private Api api = null!;
    private ModConfig config = null!;
    private LegacyModConfig legacyConfig = null!;
    private ConfigMenu? configMenu;
    private IGenericModMenuConfigApi? configMenuApi;
    private IGMCMOptionsAPI? gmcmOptionsApi;
    private GenericModConfigKeybindings? gmcmKeybindings;
    private GenericModConfigSync? gmcmSync;
    private TextureHelper textureHelper = null!;
    private KeybindActivator keybindActivator = null!;

    public ModEntry()
    {
        menuController = new(CreateMenuController);
    }

    public override void Entry(IModHelper helper)
    {
        Logger.Monitor = Monitor;
        config = Helper.ReadConfig<ModConfig>();
        legacyConfig = new();
        I18n.Init(helper.Translation);
        api = new(pageRegistry, Monitor);
        textureHelper = new(Helper.GameContent, Monitor);
        keybindActivator = new(helper.Input);
        ConfigurationViewModel.Saved += ConfigurationViewModel_Saved;

        helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        // Ensure menu gets updated at the right time.
        helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        // For optimal latency: handle input before the Update loop, perform actions/rendering after.
        helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
        helper.Events.Display.RenderedHud += Display_RenderedHud;
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
        gmcmOptionsApi = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>(GMCM_OPTIONS_MOD_ID);
        LoadGmcmKeybindings();
        RegisterConfigMenu();

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
        ViewEngine.Instance = viewEngine;
        ViewEngine.ViewAssetPrefix = $"Mods/{ModManifest.UniqueID}/Views";
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        menuController.ResetAllScreens();
    }

    private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
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

    private void GameLoop_UpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (Context.CanPlayerMove)
        {
            MenuController.PreUpdate();
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
            var context = new ConfigurationViewModel(Helper, config);
            context.Controller = ViewEngine.OpenChildMenu("Configuration", context);
            context.Controller.CanClose = () => context.IsNavigationEnabled;
            context.Controller.CloseAction = () =>
            {
                if (!context.Dismissed && context.HasUnsavedChanges())
                {
                    context.ShowCloseConfirmation();
                }
                else
                {
                    Game1.playSound("bigDeSelect");
                    Game1.exitActiveMenu();
                }
            };
            // CloseSound is normally played before CloseAction has a chance to run; in order to
            // suppress the sound only when displaying the confirmation above, we need to suppress
            // it at all times and play it ad-hoc when "really" closing.
            context.Controller.CloseSound = "";
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

    private RadialMenuController CreateMenuController()
    {
        var player = Game1.player;
        var painter = CreatePainter();
        var inventoryToggle = new MenuToggle(
            Helper.Input,
            config.Input,
            c => c.InventoryMenuButton
        );
        var inventoryMenu = new InventoryMenu(inventoryToggle, player, config.Items);
        var registeredPages = pageRegistry.CreatePageList(player);
        var modMenuToggle = new MenuToggle(Helper.Input, config.Input, c => c.ModMenuButton);
        var modMenu = new ModMenu(
            modMenuToggle,
            () => legacyConfig.CustomMenuItems,
            ActivateModMenuItem,
            textureHelper,
            registeredPages
        );
        var menuController = new RadialMenuController(
            Helper.Input,
            config,
            Game1.player,
            painter,
            inventoryMenu,
            modMenu
        );
        menuController.ItemActivated += MenuController_ItemActivated;
        return menuController;
    }

    private Painter CreatePainter()
    {
        return new(Game1.graphics.GraphicsDevice, () => legacyConfig.Styles);
    }

    private void LoadGmcmKeybindings()
    {
        if (configMenuApi is null)
        {
            Monitor.Log(
                $"Couldn't read global keybindings; mod {GMCM_MOD_ID} is not installed.",
                LogLevel.Warn
            );
            return;
        }
        Monitor.Log("Generic Mod Config Menu is loaded; reading keybindings.", LogLevel.Info);
        try
        {
            GenericModConfigKeybindings.Instance = gmcmKeybindings =
                GenericModConfigKeybindings.Load();
            Monitor.Log("Finished reading keybindings from GMCM.", LogLevel.Info);
            if (legacyConfig.DumpAvailableKeyBindingsOnStartup)
            {
                foreach (var option in gmcmKeybindings.AllOptions)
                {
                    Monitor.Log(
                        "Found keybind option: "
                            + $"[{option.ModManifest.UniqueID}] - {option.UniqueFieldName}",
                        LogLevel.Info
                    );
                }
            }
            gmcmSync = new(() => legacyConfig, gmcmKeybindings, Monitor);
            gmcmSync.SyncAll();
            // Helper.WriteConfig(legacyConfig);
        }
        catch (Exception ex)
            when (ex is InvalidOperationException || ex is TargetInvocationException)
        {
            Monitor.Log(
                $"Couldn't read global keybindings; the current version of {GMCM_MOD_ID} is "
                    + $"not compatible.\n{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}",
                LogLevel.Error
            );
        }
    }

    private void RegisterConfigMenu()
    {
        if (configMenuApi is null)
        {
            return;
        }
        configMenuApi.Register(
            mod: ModManifest,
            reset: ResetConfiguration,
            save: () => Helper.WriteConfig(legacyConfig)
        );
        configMenu = new(
            configMenuApi,
            gmcmOptionsApi,
            gmcmKeybindings,
            gmcmSync,
            ModManifest,
            Helper.ModContent,
            textureHelper,
            Helper.Events.GameLoop,
            () => legacyConfig
        );
        configMenu.Setup();
    }

    private void ResetConfiguration()
    {
        legacyConfig = new();
    }

    private void ActivateModMenuItem(CustomMenuItemConfiguration item)
    {
        if (!item.Keybind.IsBound)
        {
            Game1.showRedMessage(Helper.Translation.Get("error.missingbinding"));
            return;
        }
        keybindActivator.Activate(item.Keybind);
    }
}
