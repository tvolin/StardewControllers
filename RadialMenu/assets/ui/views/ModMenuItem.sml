<frame layout="600px content"
       padding="24"
       background={@Mods/StardewUI/Sprites/ControlBorder}>
    <frame *float="above; 0, -8" layout="stretch content" horizontal-content-alignment="middle">
        <frame background={@Mods/StardewUI/Sprites/ControlBorder} padding="24" shadow-alpha="0.25" shadow-offset="-8, 8">
            <lane layout="350px content" orientation="vertical" horizontal-content-alignment="middle">
                <image layout="64px" horizontal-alignment="middle" vertical-alignment="middle" sprite={@Item/(O)434} />
                <label margin="0, 8, 0, 0"
                       font="dialogue"
                       horizontal-alignment="middle"
                       shadow-alpha="0.3"
                       shadow-color="#3339"
                       shadow-offset="-4, 4"
                       text="Perfection Tracker" />
                <label margin="0, 8, 0, 0"
                       horizontal-alignment="middle"
                       text="Blah blah description words more words testing" />
            </lane>
        </frame>
    </frame>
    <lane layout="stretch content" orientation="vertical">
        <form-row title="Sync">
            <frame padding="4" background={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                <segments balanced="true"
                          highlight={@Mods/StardewUI/Sprites/White}
                          highlight-tint="#39d"
                          highlight-transition="150ms EaseOutQuart"
                          separator={@Mods/StardewUI/Sprites/White}
                          separator-tint="#c99"
                          separator-width="2"
                          selected-index="0">
                    <label padding="12, 8" text="None" />
                    <label padding="12, 8" text="GMCM" />
                </segments>
            </frame>
        </form-row>
        <form-row title="Title">
            <textinput layout="stretch 52px" margin="-6, 0, 0, 0" max-length="30" />
        </form-row>
        <form-row title="Description">
            <textinput layout="stretch 52px" margin="-6, 0, 0, 0" />
        </form-row>
        <form-row title="Keys">
            <keybind-editor button-height="48"
                            editable-type="SingleKeybind"
                            focusable="true"
                            empty-text="(Unassigned)"
                            sprite-map={@Mods/focustense.RadialMenu/SpriteMaps/Kenney}
                            keybind-list="LeftShift + G" />
        </form-row>
        <form-row title="Icon">
            <lane vertical-content-alignment="middle">
                <frame padding="4" background={@Mods/StardewUI/Sprites/MenuSlotTransparent}>
                    <segments balanced="true"
                              highlight={@Mods/StardewUI/Sprites/White}
                              highlight-tint="#39d"
                              highlight-transition="150ms EaseOutQuart"
                              separator={@Mods/StardewUI/Sprites/White}
                              separator-tint="#c99"
                              separator-width="2"
                              selected-index="0">
                        <label padding="12, 8" text="Item" />
                        <label padding="12, 8" text="Custom" />
                    </segments>
                </frame>
                <spacer layout="stretch 0px" />
                <button margin="16, 0, 0, 0"
                        hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                        tooltip="Randomize"
                        transform-origin="0.5, 0.5"
                        +hover:transform="scale: 1.1"
                        +transition:transform="150ms EaseOutQuint">
                    <image layout="30px" sprite={@Mods/focustense.RadialMenu/Sprites/Cursors:LuckBuff} />
                </button>
            </lane>
        </form-row>
        <image layout="stretch content" margin="0, 8" fit="stretch" sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} tint="#8888" />
        <frame layout="stretch content"
               margin="100, 8, 100, 0"
               horizontal-content-alignment="middle">
            <panel layout="stretch content"
                   vertical-content-alignment="middle">
                <textinput layout="stretch 52px" margin="-6, 0, 0, 0" border-thickness="56, 12, 12, 12" placeholder="Item ID or search text" />
                <image layout="32px"
                       margin="12, 2, 0, 0"
                       sprite={@Mods/focustense.RadialMenu/Sprites/UI:MagnifyingGlass}
                       shadow-alpha="0.25"
                       shadow-offset="-2, 2" />
            </panel>
        </frame>
        <lane layout="stretch content" margin="0, 8, 0, 0" vertical-content-alignment="middle">
            <image layout="64px" z-index="1" sprite={@Mods/StardewUI/Sprites/LargeLeftArrow} tooltip="Previous icon" transform-origin="0.5, 0.5" +hover:transform="scale: 1.2" +transition:transform="150ms EaseOutCubic" />
            <panel layout="stretch content" horizontal-content-alignment="middle" vertical-content-alignment="middle">
                <frame layout="80px" padding="8" background={@Mods/StardewUI/Sprites/MenuSlotInset} background-tint="#def" />
                <image layout="64px" sprite={@Item/(O)232} />
                <image layout="64px" sprite={@Item/(O)233} transform="translateX: -100" />
                <image layout="64px" sprite={@Item/(O)218} transform="translateX: -180" />
                <image layout="64px" sprite={@Item/(O)222} transform="translateX: 100" />
                <image layout="64px" sprite={@Item/(O)225} transform="translateX: 180" />
            </panel>
            <image layout="64px" z-index="1" sprite={@Mods/StardewUI/Sprites/LargeRightArrow} tooltip="Next icon" transform-origin="0.5, 0.5" +hover:transform="scale: 1.2" +transition:transform="150ms EaseOutCubic" />
        </lane>
    </lane>
</frame>

<template name="form-row">
    <lane margin="4" vertical-content-alignment="middle">
        <frame layout="200px content">
            <label text={&title} tooltip={&description} />
        </frame>
        <frame tooltip={&description}>
            <outlet />
        </frame>
    </lane>
</template>