<frame layout="60%[..800] content"
	   background={@Mods/StardewUI/Sprites/MenuBackground}
       border={@Mods/StardewUI/Sprites/MenuBorder}
       border-thickness="36, 36, 40, 36"
       padding="8"
       button-press=|HandleButtonPress($Button)|>
    <frame *float="above; 0, 16"
           layout="stretch content"
           margin="36, 0"
           background={@Mods/StardewUI/Sprites/MenuSlotInset}
           padding="12">
        <panel>
            <image layout="1px"
                   margin="20, 4"
                   sprite={@Mods/StardewUI/Sprites/White}
                   tint="#6ad"
                   transform="translateX: -6; scale: 112, 112" />
            <lane>
                <nav-tab icon={@Mods/focustense.RadialMenu/Sprites/UI:Gamepad} text="Controls" selected="true" />
                <nav-tab icon={@Mods/focustense.RadialMenu/Sprites/UI:Paintbrush} text="Style" selected="false" />
                <nav-tab icon={@Mods/focustense.RadialMenu/Sprites/UI:Backpack} text="Actions" selected="false" />
                <nav-tab icon={@Mods/focustense.RadialMenu/Sprites/UI:Plug} text="Mods" selected="false" />
                <nav-tab icon={@Mods/focustense.RadialMenu/Sprites/UI:BugNet} text="Debug" selected="false" />
            </lane>
        </panel>
    </frame>
    <panel layout="stretch content" margin="0, 0, 0, 4">
        <lane layout="stretch content" orientation="vertical" visibility="Visible">
            <form-heading title="Button Mapping" />
            <form-row title="Item menu">
                <mini-keybind button="LeftTrigger" />
            </form-row>
            <form-row title="Mod menu">
                <mini-keybind button="RightTrigger" />
            </form-row>
            <form-row title="Navigation" />
            <form-row title="Previous page">
                <mini-keybind button="LeftShoulder" />
            </form-row>
            <form-row title="Next page">
                <mini-keybind button="RightShoulder" />
            </form-row>
            <form-row title="Primary action (use)">
                <mini-keybind button="ControllerA" />
            </form-row>
            <form-row title="Secondary action (select)">
                <mini-keybind button="ControllerX" />
            </form-row>
            <form-heading title="Selection" />
            <form-row title="Open mode">
                <label text="test" />
            </form-row>
            <form-row title="Delayed actions" />
            <form-row title="Blink duration">
                <slider track-width="300" min="0" max="1000" interval="50" />
            </form-row>
            <form-row title="Remember last selection">
                <checkbox />
            </form-row>
            <form-heading title="Sensitivity" />
            <form-row title="Trigger dead zone">
                <slider track-width="300" min="0" max="1" interval="0.01" value="0.2" />
            </form-row>
            <form-row title="Thumbstick dead zone">
                <slider track-width="300" min="0" max="1" interval="0.01" value="0.2" />
            </form-row>
        </lane>
        <lane layout="stretch content" orientation="vertical" visibility="Hidden">
            <form-heading title="Layout" />
            <form-row title="Inner (spotlight) radius" />
            <form-row title="Spotlight image size" />
            <form-row title="Outer (menu) radius" />
            <form-row title="Menu item size" />
            <form-row title="Inner-outer gap" />
            <form-row title="Cursor size" />
            <form-row title="Cursor distance" />
            <form-heading title="Colors" />
            <form-row title="Spotlight background" />
            <form-row title="Menu background" />
            <form-row title="Active item background" />
            <form-row title="Focused item background" />
            <form-row title="Cursor" />
            <form-row title="Spotlight title" />
            <form-row title="Spotlight description" />
            <form-row title="Item count" />
        </lane>
        <lane layout="stretch content" orientation="vertical" visibility="Hidden">
            <form-heading title="Inventory Menu" />
            <form-row title="Page size">
                <slider track-width="300" min="4" max="24" interval="1" value="12" />
            </form-row>
            <form-row title="Show blanks">
                <checkbox is-checked="true" />
            </form-row>
            <form-heading title="Mod Menu" />
            <lane margin="16, 4, 0, 4" vertical-content-alignment="middle">
                <label margin="0, 0, 8, 0" text="Pages:" />
                <button layout="48px 40px" margin="2, 0"><image layout="16px" sprite={@Mods/focustense.RadialMenu/Sprites/Cursors:Number1} /></button>
                <button layout="48px 40px" margin="2, 0"><image layout="16px" sprite={@Mods/focustense.RadialMenu/Sprites/Cursors:Number2} /></button>
                <button layout="48px 40px" margin="2, 0"><image layout="16px" sprite={@Mods/focustense.RadialMenu/Sprites/Cursors:Number3} /></button>
                <button layout="48px 40px" margin="2, 0"><image layout="16px" sprite={@Mods/focustense.RadialMenu/Sprites/Cursors:Number4} /></button>
                <button layout="48px 40px" margin="2, 0"><label color="#fff" text="+" shadow-offset="-2, 2" shadow-alpha="0.8" shadow-color="#333" /></button>
            </lane>
            <label margin="16, 4, 0, 4" color="#666" text="Click on a slot below to edit or delete it, or click on the '+' slot to add a new item. To move items, press X or right-click." />
            <grid layout="stretch content"
                  margin="16, 4"
                  item-layout="length: 80+"
                  item-spacing="0, 8"
                  horizontal-item-alignment="middle">
                <mod-menu-slot icon={@Mods/focustense.RadialMenu/Sprites/UI:GamepadAlt} tooltip="Star Control Settings" />
                <mod-menu-slot icon={@Item/(O)534} tooltip="Swap Rings" />
                <mod-menu-slot icon={@Item/(O)911} tooltip="Summon Horse" />
                <mod-menu-slot icon={@Item/(BC)42} tooltip="Event Lookup" />
                <mod-menu-slot icon={@Item/(F)1402} tooltip="Calendar" />
                <mod-menu-slot icon={@Item/(F)BulletinBoard} tooltip="Quest Board" />
                <mod-menu-slot icon={@Item/(O)434} tooltip="Stardew Progress" />
                <mod-menu-slot icon={@Item/(F)1543} tooltip="Data Layers" />
                <mod-menu-slot icon={@Item/(F)2427} tooltip="Garbage In Garbage Can" />
                <mod-menu-slot icon={@Item/(O)112} tooltip="Generic Mod Config Menu" />
                <mod-menu-slot icon={@Item/(BC)130} tooltip="Convenient Inventory - Quick Stack" />
                <mod-menu-slot icon={@Item/(F)1545} tooltip="NPC Location Compass" />
                <mod-menu-slot icon={@Item/(O)128} tooltip="A Fishing Sea" />
                <mod-menu-slot icon={@Mods/focustense.RadialMenu/Sprites/Cursors:BigPlus} tooltip="Add new item" />
            </grid>
            <form-heading title="Quick Actions" />
            <label margin="16, 4, 0, 4" color="#666" text="Set your most frequently used tools and consumables here. To activate, press the associated button while the pie menu is open and nothing is selected." />
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
    </panel>
</frame>

<template name="nav-tab">
    <lane orientation="vertical"
          margin="12, 0"
          padding="8"
          horizontal-content-alignment="middle">
        <image layout="64px"
               sprite={&icon}
               shadow-alpha="0.4"
               shadow-offset="-4, 4" />
        <label margin="0, 8, 0, 0"
               bold={&selected}
               shadow-alpha="0.8"
               shadow-color="#999"
               shadow-layers="VerticalAndDiagonal"
               shadow-offset="-2, 2"
               text={&text} />
    </lane>
</template>

<template name="form-heading">
    <banner margin="0, 8, 0, 8" text={&title} />
</template>

<template name="form-row">
    <lane margin="16, 4, 0, 4" vertical-content-alignment="middle">
        <panel layout="350px content">
            <spacer layout="32px stretch" focusable="true" />
            <label text={&title} tooltip={&description} />
        </panel>
        <outlet />
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
           tooltip={&tooltip}>
        <image layout="64px"
               horizontal-alignment="middle"
               vertical-alignment="middle"
               sprite={&icon}
               tint={&tint} />
    </frame>
</template>