<lane *context={^Style} layout="stretch content" orientation="vertical">
    <form-heading title={#Config.Layout.Heading} />
    <form-row title={#Config.Layout.InnerRadius.Title} description={#Config.Layout.InnerRadius.Description}>
        <slider track-width="300"
                min="200"
                max="400"
                interval="10"
                value={<>InnerRadius}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.InnerImageSize.Title} description={#Config.Layout.InnerImageSize.Description}>
        <slider track-width="300"
                min="32"
                max="256"
                interval="16"
                value={<>SelectionSpriteHeight}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.OuterRadius.Title} description={#Config.Layout.OuterRadius.Description}>
        <slider track-width="300"
                min="80"
                max="240"
                interval="10"
                value={<>OuterRadius}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.OuterImageSize.Title} description={#Config.Layout.OuterImageSize.Description}>
        <slider track-width="300"
                min="32"
                max="160"
                interval="16"
                value={<>MenuSpriteHeight}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.OuterDistance.Title} description={#Config.Layout.OuterDistance.Description}>
        <slider track-width="300"
                min="0"
                max="20"
                interval="1"
                value={<>GapWidth}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.CursorSize.Title} description={#Config.Layout.CursorSize.Description}>
        <slider track-width="300"
                min="8"
                max="64"
                interval="4"
                value={<>CursorSize}
                value-format={:FormatPixels} />
    </form-row>
    <form-row title={#Config.Layout.CursorDistance.Title} description={#Config.Layout.CursorDistance.Description}>
        <slider track-width="300"
                min="0"
                max="20"
                interval="1"
                value={<>CursorDistance}
                value-format={:FormatPixels} />
    </form-row>

    <form-heading title={#Config.Palette.Heading} />
    <form-row title={#Config.Palette.InnerBackground.Title} description={#Config.Palette.InnerBackground.Description}>
        <color-picker layout="300px content" color={<>InnerBackgroundColor} />
    </form-row>
    <form-row title={#Config.Palette.OuterBackground.Title} description={#Config.Palette.OuterBackground.Description}>
        <color-picker layout="300px content" color={<>OuterBackgroundColor} />
    </form-row>
    <form-row title={#Config.Palette.ActiveBackground.Title} description={#Config.Palette.ActiveBackground.Description}>
        <color-picker layout="300px content" color={<>SelectionColor} />
    </form-row>
    <form-row title={#Config.Palette.FocusedBackground.Title} description={#Config.Palette.FocusedBackground.Description}>
        <color-picker layout="300px content" color={<>HighlightColor} />
    </form-row>
    <form-row title={#Config.Palette.Cursor.Title} description={#Config.Palette.Cursor.Description}>
        <color-picker layout="300px content" color={<>CursorColor} />
    </form-row>
    <form-row title={#Config.Palette.ItemName.Title} description={#Config.Palette.ItemName.Description}>
        <color-picker layout="300px content" color={<>SelectionTitleColor} />
    </form-row>
    <form-row title={#Config.Palette.ItemDetails.Title} description={#Config.Palette.ItemDetails.Description}>
        <color-picker layout="300px content" color={<>SelectionDescriptionColor} />
    </form-row>
    <form-row title={#Config.Palette.ItemCount.Title} description={#Config.Palette.ItemCount.Description}>
        <color-picker layout="300px content" color={<>StackSizeColor} />
    </form-row>
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