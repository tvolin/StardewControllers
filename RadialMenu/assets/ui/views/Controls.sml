<lane *context={:~ConfigurationViewModel.Input} layout="stretch content" orientation="vertical">
    <form-heading title={#Config.Keybind.Heading} />
    <form-row-container>
        <inline-keybind title={#Config.Keybind.Inventory.Title}
                        description={#Config.Keybind.Inventory.Description}
                        button={<>InventoryMenuButton} />
        <inline-keybind title={#Config.Keybind.Mods.Title}
                        description={#Config.Keybind.Mods.Description}
                        button={<>ModMenuButton} />
    </form-row-container>
    <form-row-container>
        <inline-keybind title={#Config.Keybind.PreviousPage.Title}
                        description={#Config.Keybind.PreviousPage.Description}
                        button={<>PreviousPageButton} />
        <inline-keybind title={#Config.Keybind.NextPage.Title}
                        description={#Config.Keybind.NextPage.Description}
                        button={<>NextPageButton} />
    </form-row-container>
    <form-row-container>
        <inline-keybind title={#Config.Keybind.PrimaryAction.Title}
                        description={#Config.Keybind.PrimaryAction.Description}
                        button={<>PrimaryActionButton} />
        <inline-keybind title={#Config.Keybind.SecondaryAction.Title}
                        description={#Config.Keybind.SecondaryAction.Description}
                        button={<>SecondaryActionButton} />
    </form-row-container>
    <form-heading title={#Config.Selection.Heading} />
    <form-row title={#Config.Selection.ToggleMode.Title} description={#Config.Selection.ToggleMode.Description}>
        <enum-segments *context={:ToggleMode} />
    </form-row>
    <form-row title={#Config.Keybind.Navigation.Title} description={#Config.Keybind.Navigation.Description}>
        <enum-segments *context={:ThumbStickPreference} />
    </form-row>
    <form-row title={#Config.Selection.DelayedActions.Title} description={#Config.Selection.DelayedActions.Description}>
        <enum-segments *context={:DelayedActions} />
    </form-row>
    <form-row title={#Config.Selection.ActivationDelay.Title} description={#Config.Selection.ActivationDelay.Description}>
        <slider track-width="300"
                min="0"
                max="1000"
                interval="50"
                value={<>ActivationDelayMs}
                value-format={:FormatActivationDelay} />
    </form-row>
    <form-row title={#Config.Selection.Remember.Title} description={#Config.Selection.Remember.Description}>
        <checkbox is-checked={<>RememberSelection} />
    </form-row>
    <form-heading title={#Config.Sensitivity.Heading} />
    <form-row title={#Config.Sensitivity.TriggerDeadZone.Title} description={#Config.Sensitivity.TriggerDeadZone.Description}>
        <slider track-width="300"
                min="0"
                max="1"
                interval="0.01"
                value={<>TriggerDeadZone}
                value-format={:FormatDeadZone} />
    </form-row>
    <form-row title={#Config.Sensitivity.ThumbstickDeadZone.Title} description={#Config.Sensitivity.ThumbstickDeadZone.Description}>
        <slider track-width="300"
                min="0"
                max="1"
                interval="0.01"
                value={<>ThumbstickDeadZone}
                value-format={:FormatDeadZone} />
    </form-row>
</lane>

<template name="form-heading">
    <banner margin="0, 8, 0, 4" text={&title} />
</template>

<template name="form-row">
    <form-row-container>
        <frame layout="350px content">
            <label text={&title} tooltip={&description} />
        </frame>
        <frame tooltip={&description}>
            <outlet />
        </frame>
    </form-row-container>
</template>

<template name="form-row-container">
    <lane margin="16, 4, 0, 4" vertical-content-alignment="middle">
        <outlet />
    </lane>
</template>

<template name="enum-segments">
    <frame background={@Mods/StardewUI/Sprites/MenuSlotTransparent} padding="4" tooltip="">
        <segments balanced="true"
                  highlight={@Mods/StardewUI/Sprites/White}
                  highlight-tint="#39d"
                  highlight-transition="150ms EaseOutQuart"
                  separator={@Mods/StardewUI/Sprites/White}
                  separator-tint="#c99"
                  separator-width="2"
                  selected-index={<>SelectedIndex}>
            <label *repeat={Segments}
                   margin="12, 8"
                   bold={Selected}
                   text={:Name}
                   tooltip={:Description} />
        </segments>
    </frame>
</template>

<template name="inline-keybind">
    <lane layout="350px content"
          vertical-content-alignment="middle"
          tooltip={&description}>
        <mini-keybind button={&button} />
        <label margin="16, 0, 0, 0" text={&title} />
    </lane>
</template>

<template name="mini-keybind">
    <keybind-editor button-height="32"
                    editable-type="SingleButton"
                    focusable="true"
                    empty-text="(Unassigned)"
                    sprite-map={@Mods/focustense.RadialMenu/SpriteMaps/Kenney}
                    keybind-list={&button} />
</template>