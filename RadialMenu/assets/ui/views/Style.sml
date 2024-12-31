<lane layout="stretch content" orientation="vertical">
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