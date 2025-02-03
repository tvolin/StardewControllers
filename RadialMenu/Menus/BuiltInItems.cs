using RadialMenu.Graphics;
using StardewValley.Menus;

namespace RadialMenu.Menus;

/// <summary>
/// Contains the library of built-in items, which are not part of any default menu but can be added
/// to the Mod Menu via the API (Library) tab.
/// </summary>
public class BuiltInItems
{
    /// <summary>
    /// Menu item for opening the Crafting Menu.
    /// </summary>
    public IRadialMenuItem Crafting { get; }

    /// <summary>
    /// Menu item for opening the Journal (Quest Log).
    /// </summary>
    public IRadialMenuItem Journal { get; }

    /// <summary>
    /// Menu item for showing mail (i.e. Mailbox contents) for the current location.
    /// </summary>
    public IRadialMenuItem Mail { get; }

    /// <summary>
    /// Menu item for displaying the Main Menu, defaulting to Inventory tab.
    /// </summary>
    /// <remarks>
    /// This function already has two controller buttons dedicated to it (start/menu and B) in the
    /// default controllers, so it is not generally used unless both those buttons are remapped.
    /// </remarks>
    public IRadialMenuItem MainMenu { get; }

    /// <summary>
    /// Menu item for displaying the local map.
    /// </summary>
    public IRadialMenuItem Map { get; }

    public BuiltInItems(IManifest mod)
    {
        Crafting = CreateItem(
            $"{mod.UniqueID}.Crafting",
            I18n.ModMenu_Crafting_Name,
            I18n.ModMenu_Crafting_Description,
            Sprites.Hammer,
            _ => OpenMenuTab(GameMenu.craftingTab)
        );
        Journal = CreateItem(
            $"{mod.UniqueID}.Journal",
            I18n.ModMenu_Journal_Name,
            I18n.ModMenu_Journal_Description,
            Sprites.Book,
            _ => Game1.activeClickableMenu = new QuestLog()
        );
        Mail = CreateItem(
            $"{mod.UniqueID}.Mail",
            I18n.ModMenu_Mailbox_Name,
            I18n.ModMenu_Mailbox_Description,
            Sprites.Letter,
            _ => Game1.currentLocation.mailbox()
        );
        MainMenu = CreateItem(
            $"{mod.UniqueID}.MainMenu",
            I18n.ModMenu_MainMenuItem_Name,
            I18n.ModMenu_MainMenuItem_Description,
            Sprites.Menu,
            _ => OpenMenuTab(GameMenu.inventoryTab)
        );
        Map = CreateItem(
            $"{mod.UniqueID}.Map",
            I18n.ModMenu_Map_Name,
            I18n.ModMenu_Map_Description,
            () => Sprite.ForItemId("(F)1366"), // Globe,
            _ => OpenMenuTab(GameMenu.mapTab)
        );
    }

    private static IRadialMenuItem CreateItem(
        string id,
        Func<string> titleSelector,
        Func<string> descriptionSelector,
        Func<Sprite?> spriteSelector,
        Action<Farmer> activate
    )
    {
        var sprite = new Lazy<Sprite?>(spriteSelector);
        return new ModMenuItem(
            id,
            title: titleSelector,
            description: descriptionSelector,
            texture: () => sprite.Value?.Texture,
            sourceRectangle: () => sprite.Value?.SourceRect,
            activate: (who, _, _) =>
            {
                if (!Game1.CanShowPauseMenu())
                {
                    return ItemActivationResult.Ignored;
                }
                activate(who);
                return ItemActivationResult.Custom;
            }
        );
    }

    private static void OpenMenuTab(int tab)
    {
        Game1.PushUIMode();
        Game1.activeClickableMenu = new GameMenu(tab);
        Game1.PopUIMode();
    }
}
