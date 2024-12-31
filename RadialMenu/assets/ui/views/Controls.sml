<lane layout="stretch content" orientation="vertical">
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