﻿<frame layout="600px content"
       padding="24"
       background={@Mods/StardewUI/Sprites/ControlBorder}>
    <frame *float="above; 0, -8" layout="stretch content" horizontal-content-alignment="middle">
        <frame background={@Mods/StardewUI/Sprites/ControlBorder} padding="24" shadow-alpha="0.25" shadow-offset="-8, 8">
            <lane layout="350px content" orientation="vertical" horizontal-content-alignment="middle">
                <image layout="64px" horizontal-alignment="middle" vertical-alignment="middle" sprite={Icon} />
                <label margin="0, 8, 0, 0"
                       font="dialogue"
                       horizontal-alignment="middle"
                       shadow-alpha="0.3"
                       shadow-color="#3339"
                       shadow-offset="-4, 4"
                       text={Name} />
                <label margin="0, 8, 0, 0"
                       horizontal-alignment="middle"
                       text={Description} />
            </lane>
        </frame>
    </frame>
    <lane layout="stretch content" orientation="vertical">
        <form-row title={#Config.ModMenuItem.SyncType.Title} description={#Config.ModMenuItem.SyncType.Description}>
            <enum-segments *context={:SyncType} />
        </form-row>
        <form-row title={#Config.ModMenuItem.Name.Title} description={#Config.ModMenuItem.Name.Description}>
            <textinput layout="stretch 52px" margin="-6, 0, 0, 0" max-length="30" text={<>Name} />
        </form-row>
        <form-row title={#Config.ModMenuItem.Details.Title} description={#Config.ModMenuItem.Details.Description}>
            <textinput layout="stretch 52px" margin="-6, 0, 0, 0" text={<>Description} />
        </form-row>
        <form-row title={#Config.ModMenuItem.Keybind.Title} description={#Config.ModMenuItem.Keybind.Description}>
            <keybind-editor button-height="48"
                            editable-type="SingleKeybind"
                            focusable="true"
                            empty-text={#Config.ModMenuItem.Keybind.EmptyText}
                            sprite-map={@Mods/focustense.RadialMenu/SpriteMaps/Kenney}
                            keybind-list={Keybind} />
        </form-row>
        <form-row title={#Config.ModMenuItem.IconType.Title} description={#Config.ModMenuItem.IconType.Description}>
            <lane vertical-content-alignment="middle">
                <enum-segments *context={:IconType} />
                <spacer layout="stretch 0px" />
                <button margin="16, 0, 0, 0"
                        hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                        tooltip={#Config.ModMenuItem.RandomizeIcon.Description}
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
                <textinput layout="stretch 52px"
                           margin="-6, 0, 0, 0"
                           border-thickness="56, 12, 12, 12"
                           placeholder={#Config.ModMenuItem.Search.Placeholder}
                           text={<>SearchText} />
                <image layout="32px"
                       margin="12, 2, 0, 0"
                       sprite={@Mods/focustense.RadialMenu/Sprites/UI:MagnifyingGlass}
                       shadow-alpha="0.25"
                       shadow-offset="-2, 2" />
            </panel>
        </frame>
        <lane *context={SearchResults}
              layout="stretch content"
              margin="0, 8, 0, 0"
              vertical-content-alignment="middle">
            <image layout="64px"
                   sprite={@Mods/StardewUI/Sprites/LargeLeftArrow}
                   tooltip={#Config.ModMenuItem.Search.Previous}
                   z-index="1"
                   transform-origin="0.5, 0.5"
                   +hover:transform="scale: 1.2"
                   +transition:transform="150ms EaseOutCubic"
                   click=|MovePrevious()| />
            <panel layout="stretch content"
                   horizontal-content-alignment="middle"
                   vertical-content-alignment="middle"
                   clip-origin="middle middle"
                   clip-size="95% stretch"
                   outer-size={>LayoutSize}
                   button-press=|HandleButton($Button)|
                   button-repeat=|HandleButton($Button)|
                   left-click=|^^SetIconFromSearchResults($Position)|>
                <frame layout="80px"
                       margin="2, 0, 0, 0"
                       padding="8"
                       background={@Mods/StardewUI/Sprites/MenuSlotInset}
                       background-tint="#def"
                       horizontal-content-alignment="end"
                       vertical-content-alignment="end" />
                <image *repeat={VisibleItems}
                       layout="64px"
                       horizontal-alignment="middle"
                       vertical-alignment="middle"
                       sprite={:Data}
                       transform={Transform}
                       +transition:transform="120ms EaseOutCubic" />
                <spacer layout="24px" margin="48, 48, 0, 0" z-index="2" focusable="true" />
            </panel>
            <image layout="64px"
                   sprite={@Mods/StardewUI/Sprites/LargeRightArrow}
                   tooltip={#Config.ModMenuItem.Search.Next}
                   z-index="1"
                   transform-origin="0.5, 0.5"
                   +hover:transform="scale: 1.2"
                   +transition:transform="150ms EaseOutCubic"
                   click=|MoveNext()| />
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