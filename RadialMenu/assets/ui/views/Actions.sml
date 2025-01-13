<lane *context={:~ConfigurationViewModel.Items} layout="stretch content" orientation="vertical" clip-size="stretch">
    <form-heading title={#Config.Inventory.Heading} />
    <form-row title={#Config.Inventory.PageSize.Title} description={#Config.Inventory.PageSize.Description}>
        <slider track-width="300" min="4" max="24" interval="1" value={<>InventoryPageSize} />
    </form-row>
    <form-row title={#Config.Inventory.ShowBlanks.Title} description={#Config.Inventory.ShowBlanks.Description}>
        <checkbox is-checked={<>ShowInventoryBlanks} />
    </form-row>
    <form-heading title={#Config.ModMenu.Heading} />
    <lane *context={:Pager} margin="16, 4, 0, 4" vertical-content-alignment="middle">
        <label margin="0, 0, 8, 0" text={#Config.ModMenu.Pages.Title} tooltip={#Config.ModMenu.Pages.Description} />
        <button *repeat={Pages}
                layout="48px"
                margin="2, 0"
                default-background-tint={ButtonTint}
                hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                tooltip={#Config.ModMenu.Pages.Description}
                left-click=|^SelectPage(Index)|>
            <digits number={:DisplayIndex} scale="3" />
        </button>
        <button layout="48px"
                margin="2, 0"
                hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                tooltip={#Config.ModMenu.AddPage.Description}
                left-click=|^AddPage()|>
            <label color="#fff" text="+" shadow-offset="-2, 2" shadow-alpha="0.8" shadow-color="#333" />
        </button>
    </lane>
    <label margin="16, 4, 0, 4" color="#666" text={#Config.ModMenu.Items.Help} />
    <panel *context={:Pager} margin="16, 4">
        <grid *repeat={Pages}
              layout="stretch content"
              item-layout="length: 80+"
              item-spacing="0, 8"
              horizontal-item-alignment="middle"
              pointer-events-enabled={Selected}
              transform={Transform}
              +transition:transform="100ms EaseOutCubic"
              visibility={Visible}>
            <mod-menu-slot icon={@Mods/focustense.RadialMenu/Sprites/UI:GamepadAlt} tooltip="Star Control Settings" />
            <mod-menu-slot *repeat={Items} id={:Id} icon={Icon} tooltip={Name} />
            <mod-menu-slot icon={@Mods/focustense.RadialMenu/Sprites/Cursors:BigPlus} tooltip="Add new item" />
        </grid>
    </panel>
    <form-heading title={#Config.QuickActions.Heading} />
    <label margin="16, 4, 0, 4" color="#666" text={#Config.QuickActions.Items.Help} />
    <lane layout="stretch content" margin="12, 4, 12, 4">
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadLeft} icon={@Item/(O)287} tooltip="Bomb" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadUp} icon={@Item/(T)Pickaxe} tooltip="Pickaxe" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadRight} icon={@Item/(W)4} tooltip="Galaxy Sword" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadDown} icon={@Item/(BC)71} tint="#4444" tooltip="Staircase" />
        <spacer layout="stretch 0px" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadX} icon={@Item/(O)424} tooltip="Cheese" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadY} icon={@Item/(O)253} tooltip="Triple Shot Espresso" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadB} tooltip="Unassigned" />
        <quick-slot prompt={@Mods/focustense.RadialMenu/Sprites/UI:GamepadA} icon={@Item/(O)534} tooltip="Swap Rings" />
    </lane>
</lane>

<template name="form-heading">
    <banner margin="0, 8, 0, 8" text={&title} />
</template>

<template name="form-row">
    <lane margin="16, 4, 0, 4" vertical-content-alignment="middle">
        <frame layout="350px content">
            <label text={&title} tooltip={&description} />
        </frame>
        <frame tooltip={&description}>
            <outlet />
        </frame>
    </lane>
</template>

<template name="quick-slot">
    <lane margin="4, 0" orientation="vertical" horizontal-content-alignment="middle">
        <mod-menu-slot icon={&icon} tint={&tint} tooltip={&tooltip} />
        <image layout="32px" margin="0, -8, 0, 0" sprite={&prompt} />
    </lane>
</template>

<template name="mod-menu-slot">
    <frame padding="8"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
           focusable="true"
           tooltip={&tooltip}
           left-click=|~ItemsConfigurationViewModel.EditModMenuItem(&id)|>
        <image layout="64px"
               horizontal-alignment="middle"
               vertical-alignment="middle"
               sprite={&icon}
               tint={&tint} />
    </frame>
</template>