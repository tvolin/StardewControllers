<frame layout="60%[..800] content"
	   background={@Mods/StardewUI/Sprites/MenuBackground}
       border={@Mods/StardewUI/Sprites/MenuBorder}
       border-thickness="36, 36, 40, 36"
       padding="8"
       border-size={>ContentPanelSize}
       button-press=|HandleButtonPress($Button)|>
    <frame *float="above; 0, 16"
           *context={:Pager}
           layout="stretch content"
           margin="36, 0"
           background={@Mods/StardewUI/Sprites/MenuSlotInset}
           padding="12">
        <segments highlight={@Mods/StardewUI/Sprites/White}
                  highlight-tint="#39d"
                  highlight-transition="200ms EaseOutExpo"
                  selected-index={<>SelectedPageIndex}>
            <nav-tab *repeat={Pages}
                     margin="12, 8"
                     bold={Selected}
                     text={:Name}
                     tooltip={:Description} />
        </segments>
    </frame>
    <panel *context={:Pager}
           layout="stretch content"
           margin="0, 0, 0, 4"
           clip-size="stretch">
        <frame *repeat={Pages}
               layout="stretch content"
               pointer-events-enabled={Selected}
               transform={Transform}
               +transition:transform="200ms EaseOutCubic"
               visibility={Visible}>
            <include name={:PageAssetName} />
        </frame>
    </panel>
</frame>

<template name="nav-tab">
    <lane *switch={:Id}
          orientation="vertical"
          margin="12, 0"
          padding="8"
          horizontal-content-alignment="middle"
          focusable="true"
          click=|^SelectPage(Index)|>
        <!--
            Not the cleanest way to handle images per tab, but the alternative is hard-coding
            the source rectangles in code, because API code can't reference sprite assets.
        -->
        <nav-image *case="Controls" icon={@Mods/focustense.RadialMenu/Sprites/UI:Gamepad} />
        <nav-image *case="Style" icon={@Mods/focustense.RadialMenu/Sprites/UI:Paintbrush} />
        <nav-image *case="Actions" icon={@Mods/focustense.RadialMenu/Sprites/UI:Backpack} />
        <nav-image *case="Mods" icon={@Mods/focustense.RadialMenu/Sprites/UI:Plug} />
        <nav-image *case="Debug" icon={@Mods/focustense.RadialMenu/Sprites/UI:BugNet} />
        <label margin="0, 8, 0, 0"
               bold={Selected}
               shadow-alpha="0.8"
               shadow-color="#999"
               shadow-layers="VerticalAndDiagonal"
               shadow-offset="-2, 2"
               text={:Title} />
    </lane>
</template>

<template name="nav-image">
    <image layout="64px"
           sprite={&icon}
           shadow-alpha="0.4"
           shadow-offset="-4, 4" />
</template>