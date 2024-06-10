﻿namespace RadialMenu.Config;

public class Configuration
{
    private static readonly Configuration Default = new();

    /// <summary>
    /// Dead zone for the left/right trigger buttons for activating/deactivating the menu.
    /// </summary>
    /// <remarks>
    /// Triggers are generally used as regular buttons in Stardew Valley, but are technically
    /// analog inputs. Due to a variety of technical issues, this mod needs to ignore the
    /// simpler on/off behavior and read the analog input directly. Increase the dead zone if
    /// necessary to prevent accidental presses, or reduce it for hair-trigger response.
    /// </remarks>
    public float TriggerDeadZone { get; set; } = 0.2f;

    /// <summary>
    /// Customizes which thumbstick is used to select items from the radial menus.
    /// </summary>
    public ThumbStickPreference ThumbStickPreference { get; set; }

    /// <summary>
    /// Dead-zone for the thumbstick when selecting from a radial menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only applies to menu selection; changing this setting will not affect the dead zone used
    /// for any other mods or controls in the vanilla game.
    /// </para>
    /// <para>
    /// Many, if not most controllers suffer from drift issues in the analog sticks. Setting
    /// this value too low could cause items to get selected even when the thumbstick has not
    /// been moved.
    /// </para>
    /// </remarks>
    public float ThumbStickDeadZone { get; set; } = 0.2f;
    /// <summary>
    /// How to activate the selected item; refer to <see cref="ItemActivationMethod"/>.
    /// </summary>
    public ItemActivationMethod Activation { get; set; }

    /// <summary>
    /// Maximum number of items to display in the inventory radial menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is meant to approximate the same "backpack page" that appears in the game's
    /// toolbar, and uses the same default value. Empty slots will not show up in the menu, but
    /// are still counted against this limit, to provide a similar-to-vanilla experience where
    /// only the first "page" of items can be quick-selected.
    /// </para>
    /// <para>
    /// For example, given the default value of 12, if the fully-upgraded backpack contains 30
    /// items, but the first row has 8 empty slots, then only 4 items will show up in the radial
    /// menu.
    /// </para>
    /// </remarks>
    public int MaxInventoryItems { get; set; } = 12;

    /// <summary>
    /// List of all items configured for the custom radial menu.
    /// </summary>
    public List<CustomMenuItemConfiguration> CustomMenuItems { get; set; } = [];

    /// <summary>
    /// Configures the overall appearance of the menu.
    /// </summary>
    public Styles Styles { get; set; } = new();

    /// <summary>
    /// Debug option that prints the list of all registered GMCM key bindings when starting the
    /// game.
    /// </summary>
    /// <remarks>
    /// Can be helpful for manually editing the <c>config.json</c>, since the field IDs/names
    /// are not documented anywhere except in the source code of the GMCM-enabled mods.
    /// </remarks>
    public bool DumpAvailableKeyBindingsOnStartup { get; set; }
}
