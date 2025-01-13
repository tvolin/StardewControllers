<frame layout="660px 700px"
       padding="24, 24, 24, 0"
       background={@Mods/StardewUI/Sprites/ControlBorder}>
    <lane layout="stretch content" orientation="vertical">
        <form-row title={#Config.QuickSlot.ItemSource.Title} description={#Config.QuickSlot.ItemSource.Description}>
            <enum-segments *context={:ItemSource} />
        </form-row>
        <image layout="stretch content" margin="0, 8" fit="stretch" sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} tint="#8888" />
        <panel *context={:Pager}
               layout="stretch content"
               margin="0, 8, 0, 4"
               padding="8, 0"
               clip-size="stretch"
               outer-size={>ContentPanelSize}>
            <page *repeat={Pages} *switch={Index}>
                <lane *case="0" layout="stretch content" orientation="vertical">
                    <label *if={:^^HasLoadedGame}
                           margin="0, -4, 0, 16"
                           color="#666"
                           text={#Config.QuickSlot.Inventory.Description} />
                    <item-grid *if={:^^HasLoadedGame} items={:^^InventoryItems} />
                    <label *!if={:^^HasLoadedGame} text={#Config.QuickSlot.Inventory.Unavailable} />
                </lane>
                <lane *case="1" layout="stretch content" orientation="vertical">
                    <panel layout="stretch content"
                           margin="0, 0, 0, 16"
                           vertical-content-alignment="middle">
                        <textinput layout="stretch 52px"
                                   border-thickness="56, 12, 12, 12"
                                   placeholder={#Config.ModMenuItem.Search.Placeholder}
                                   text={<>^^SearchText} />
                        <image layout="32px"
                               margin="18, 2, 0, 0"
                               sprite={@Mods/focustense.RadialMenu/Sprites/UI:MagnifyingGlass}
                               shadow-alpha="0.25"
                               shadow-offset="-2, 2" />
                    </panel>
                    <item-grid items={^^SearchResults} />
                    <label *if={^^HasMoreSearchResults}
                           margin="0, 8, 0, 0"
                           color="#666"
                           text={^^MoreResultsMessage} />
                </lane>
            </page>
        </panel>
    </lane>
</frame>

<template name="form-row">
    <lane margin="4" vertical-content-alignment="middle">
        <frame layout="150px content">
            <label text={&title} tooltip={&description} />
        </frame>
        <frame tooltip={&description}>
            <outlet />
        </frame>
    </lane>
</template>

<template name="enum-segments">
    <frame background={@Mods/StardewUI/Sprites/MenuSlotTransparent} padding="4" tooltip="">
        <segments highlight={@Mods/StardewUI/Sprites/White}
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

<template name="page">
    <frame layout="stretch content"
           pointer-events-enabled={Selected}
           transform={Transform}
           +transition:transform="200ms EaseOutCubic"
           visibility={Visible}>
        <outlet />
    </frame>
</template>

<template name="item-grid">
    <grid layout="stretch content"
          item-layout="length: 72+"
          item-spacing="4, 4"
          grid-alignment="middle"
          horizontal-item-alignment="middle"
          vertical-item-alignment="middle">
        <item *repeat={&items} />
    </grid>
</template>

<template name="item">
    <frame layout="stretch content"
           padding="4"
           background={@Mods/StardewUI/Sprites/White}
           background-tint="#0000"
           border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
           border-thickness="4"
           focusable="true"
           tooltip={:Tooltip}
           +hover:background-tint="#39d"
           +transition:background-tint="100ms">
        <panel>
            <image layout="64px"
                   horizontal-alignment="middle"
                   vertical-alignment="middle"
                   sprite={:Sprite}
                   tint={:SpriteColor}
                   shadow-alpha="0.25"
                   shadow-offset="-2, 2" />
            <image *if={:HasTintSprite}
                   horizontal-alignment="middle"
                   vertical-alignment="middle"
                   sprite={:TintSprite}
                   tint={:TintSpriteColor} />
        </panel>
    </frame>
</template>