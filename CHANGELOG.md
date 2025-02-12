# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0-beta] - 2025-02-12

### Added

- Documentation page is up at https://focustense.github.io/StardewControllers/.

### Changed

- Assigned but disabled/unavailable slots are shown in the Instant Actions HUD.
- Instant Actions menu will show error sprites for invalid items, such as items once registered via the API but no longer present, so that they can be identified and removed from those slots.
- Placeable objects, including e.g. bombs, are automatically placed when used as an Instant Action.
- Assigned Instant Action slots flash when the action is unavailable, such as due to missing inventory, or a placeable object being forbidden at that location.
- Always suppress input for assigned instant slots, even if the item/action isn't available, to prevent default behavior from coming back unexpectedly even though the HUD shows a bound action.

### Fixed

- Keys bound to mouse buttons (e.g. when using GMCM Sync in Item Settings) render correctly.

## [1.0.0-alpha2] - 2025-02-06

### Removed

- Hardcoded keyboard shortcuts (F10) for config menu and instant actions. These were dev shortcuts and left in alpha1 accidentally.

### Fixed

- Icons in pie menus draw with the correct rendering settings (no longer blurry).
- Mod Menu can now be displayed when the only item is the Settings item, i.e. when the mod is newly installed with a fresh configuration.

## [1.0.0-alpha1] - 2025-02-06

### Added

- [Quick Actions](https://focustense.github.io/StardewControllers/controller-hud/#quick-actions): single-button bindings while overlay is open.
- [Instant Actions](https://focustense.github.io/StardewControllers/instant-actions/): single-button bindings while interacting with the game world, capable of direct tool use and item placement.
- New configuration UI built in [Stardew UI](https://github.com/focustense/StardewUI) with many new configuration options:
    - Every button used by the mod is rebindable and removable.
    - Pie menus (radial menus) can be activated in "Toggle Mode" (press to open, press again to close) instead of "Hold Mode" (open while button is held).
    - Pie menus can render empty slices for empty inventory slots, to ensure that slices are always the same size and always in the same position.
    - Alternate activation styles—stick press and trigger release—can both be used, either in addition to or instead of button presses, and can be assigned to either the primary or secondary action.
    - Menus can be configured to reopen after activation if the trigger button is still held. (Only applicable to Hold Mode)
    - Style editor now has a proportionate, accurate preview and uses the Stardew UI [color picker](https://focustense.github.io/StardewUI/library/standard-views/#color-picker) for colors.
    - Mod Menu items can be organized into pages and moved or deleted, and the Icon selector includes a search field.
    - All sound cues are fully configurable, and sound can be globally muted.
    - Third-party pages—those added via the [API](https://focustense.github.io/StardewControllers/api/)—can be disabled or reordered.
- Integrated (configurable) chatbox suppression to enable use of the right stick for mod features, without blocking emojis and other functions.
- Light/fast animations for menus and slots.
    - Menus do not wait for animations to complete before handling inputs, so this does not affect latency.
- New [Item Registration API](https://focustense.github.io/StardewControllers/api/#item-registration) for mod authors allows registering actions without forcing them into the user's menu or requiring an intermediate (keyboard) keybind to activate.
- Combined, standalone API is now published in the [PublicApi directory](https://github.com/focustense/StardewControllers/tree/master/PublicApi) for easy copy-pasting into other mods.
- Verbose logging options for mod authors.

### Changed

- Empty pages are automatically hidden from pie menus.
- Mod settings are accessible directly from the Mod Menu (can be disabled/hidden).
- Keybind sync for [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (GMCM) moved to subassembly, which makes GMCM now an optional rather than required dependency, and allows graceful recovery when GMCM is not installed or is an incompatible version.
- GMCM keybind sync now updates every time the target mod's configuration is changed via GMCM (i.e. in real time), not only when the game is restarted.
- Changes to the [API](https://focustense.github.io/StardewControllers/api/):
    - `IRadialMenuPage.Items` can contain nulls; these will render as empty slices.
    - `IRadialMenuItem` now has an `Id` property; all API clients **should** implement this with a unique and persistent ID if possible, although it is only mandatory for [Item Registration](https://focustense.github.io/StardewControllers/api/#item-registration), not [Page Registration](https://focustense.github.io/StardewControllers/api/#page-registration).
    - `IRadialMenuItem` has an optional `Enabled` property which, if set to `false`, will dim the item in menus and prevent players from activating it.
    - `IRadialMenuItem` has an optional `GetActivationSound` method that can be used to override, or disable, the default confirmation sound that the player has set up per item.
    - **(Breaking Change)** `MenuItemActivationResult` is renamed to `ItemActivationResult`
    - **(Breaking Change)** `MenuItemAction`, which was inventory-centric, is removed and changed to `ItemActivationType`, which is more general and supports Instant Actions.
    - `ItemActivationResult` includes new `ToolUseStarted`. API clients generally should **not** use this result as it starts a complicated event loop to repeatedly fire successive activations and trick the game into thinking that different buttons are pressed.
- **(Breaking Change)** New mod name and ID.
    - Mod ID ("unique ID") has been intentionally changed so that API clients can remain backward-compatible with older versions, if they choose to, by invoking both the old and new APIs.

### Removed

- GMCM-based configuration menu and all code and assets related to it. The GMCM page is now a stub, containing only a button that opens the mod's own configuration UI, and is only shown as a fallback when direct (Harmony) integration with the GMCM index page fails.

### Fixed

- Correct tool is selected when activating an item that is in the same slot, but on a different page.

## [0.2.2] - 2024-11-06

### Fixed

- No code changes, but rebuilt binary with Stardew Valley 1.6.10 DLL to eliminate `InvalidProgramException` after update.

## [0.2.1] - 2024-08-28

### Fixed

- Inventory radial menu now stays in sync with actual inventory when an item from the second (or later) row is moved up to the first row and becomes the selected tool.
- Custom menu no longer fails to render when focused on an item without a valid texture (i.e. provided by an API user).

## [0.2.0] - 2024-08-15

### Added

- Public API for use by other mods, with demo project; see [documentation in the readme](README.md#api).
- Option to remember the last-opened menu page.
- Per-item setting to force activation delay on custom menu items.

### Changed

- Menu pages are persistent and synced on specific triggers, such as `InventoryChanged`, instead of being recreated every time the menu is opened; this change supports the new API.
- Switch from `TranslationHelper` to `ModTranslationClassBuilder`; should have no user-facing effects.

### Fixed

- Items that are picked up while a menu is already open (i.e. pulled in by "magnetism") will show up in the inventory menu immediately, without having to close/reopen the menu.
- Minor fixes to error logging.

## [0.1.6] - 2024-06-23

### Added

- Switch pages (using L/R shoulder buttons) while menu is open.
  - By design, this works differently from vanilla shifting during gameplay; the switch only affects the menu itself, and only persists while the menu is open, so cancelling out of the menu or auto-using an item on a different page does not affect current tool selection.
  - In simple terms, you can be busy chopping/mining, eat a food item on backpack page 2 or 3 using the menu, and still have the axe/pickaxe selected afterward.
- Highlight current tool selection in the menu, using a different background color for that wedge.
  - Color is customizable in GMCM; default is a darker shade of the normal menu background. Players who find this distracting can set it to the menu background color.
  - This was added because the radial menu freezes gameplay controls which, as a side effects, also hides the vanilla toolbar, so previously the only way to remind oneself of which item was already selected was to dismiss the radial menu in order to see the toolbar again.
- This changelog. Covers all past releases.

## [0.1.5] - 2024-06-21

### Fixed

- Menu now works in local co-op without freezing, thrashing or locking out controls.

## [0.1.4] - 2024-06-20

### Fixed

- Item titles are now correct for flavored items, e.g. "Starfruit Wine" instead of just "Wine".
- Focused item descriptions will no longer include format args like `{0}` as literal text; they will display the item's full and correctly-formatted text. As a side effect, this also includes occasional suffix text like weapon stats.
- Menu/focused items will have the correct color tint.
- Smoked Fish will show the correct fish (previously it was always a carp).

## [0.1.3-alpha] - 2024-06-18

### Changed

- Primary and secondary actions are both configurable, meaning the "default" (now primary) action for a consumable item can instead be to select it, and the secondary action to auto-use.
- Out-of-the-box control scheme remains the scheme (A to auto-use, X to force-select) but players can now edit these to form one of the more commonly-requested alternate schemes, using trigger release for select and A or X button for auto-use.
- Changes names of the configuration settings related to the above features, so those settings will be reset on first-time launch and may have to be updated again for players not using the defaults.

## [0.1.2-alpha] - 2024-06-17

### Added

- Forced tool selection (skip quick actions like eat, warp) using a secondary button, default "X". This restores the ability to hold a consumable item for putting into machines, gifting, etc.
- Secondary button configuration in GMCM settings.

## [0.1.1-alpha] - 2024-06-17

### Changed

- Only play "select" sound when activation is delayed. Many items have their own activation sounds and/or animations, so having the menu also play a sound could lead to confusing and annoying feedback.

## [0.1.0-alpha] - 2024-06-16

### Added

- Initial release.
- Inventory menu via left trigger (default).
- Custom shortcuts menu via right trigger (default).
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) pages for control scheme, appearance and custom shortcuts.

[Unreleased]: https://github.com/focustense/StardewRadialMenu/compare/v1.0.0-beta...HEAD
[1.0.0-beta]: https://github.com/focustense/StardewRadialMenu/compare/v1.0.0-alpha2...v1.0.0-beta
[1.0.0-alpha2]: https://github.com/focustense/StardewRadialMenu/compare/v1.0.0-alpha1...v1.0.0-alpha2
[1.0.0-alpha1]: https://github.com/focustense/StardewRadialMenu/compare/v0.2.2...v1.0.0-alpha1
[0.2.2]: https://github.com/focustense/StardewRadialMenu/compare/v0.2.1...v0.2.2
[0.2.1]: https://github.com/focustense/StardewRadialMenu/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.6...v0.2.0
[0.1.6]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.5...v0.1.6
[0.1.5]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.4...v0.1.5
[0.1.4]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.3-alpha...v0.1.4
[0.1.3-alpha]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.2-alpha...v0.1.3-alpha
[0.1.2-alpha]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.1-alpha...v0.1.2-alpha
[0.1.1-alpha]: https://github.com/focustense/StardewRadialMenu/compare/v0.1.0-alpha...v0.1.1-alpha
[0.1.0-alpha]: https://github.com/focustense/StardewRadialMenu/tree/v0.1.0-alpha