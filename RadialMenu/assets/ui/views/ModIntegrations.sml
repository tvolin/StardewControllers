<lane *context={:~ConfigurationViewModel.Mods} layout="stretch content" orientation="vertical" clip-size="stretch">
    <form-heading title={#Config.ModPriorities.Heading} />
    <label margin="0, 0, 0, 16" color="#666" text={#Config.ModPriorities.Help} />
    <frame *repeat={Priorities}
           layout="stretch content"           
           padding="8, 0"
           vertical-content-alignment="middle"
           background={@Mods/StardewUI/Sprites/White}
           background-tint="#0000"
           border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
           border-thickness="4"
           outer-size={>LayoutSize}
           +state:dragging={Dragging}
           +state:dragging:background-tint="#39d"
           +transition:background-tint="100ms EaseOutSine">
        <lane vertical-content-alignment="middle">
            <checkbox margin="0, 4" label-text={Name} tooltip={Description} is-checked={<>Enabled} />
            <spacer layout="stretch"
                    pointer-style="Hand"
                    draggable="true"
                    drag-start=|^BeginDrag(this)|
                    drag-end=|^EndDrag(this)|
                    drag=|^HandleDrag(this, $Position)|/>
        </lane>
    </frame>
</lane>

<template name="form-heading">
    <banner margin="0, 8, 0, 8" text={&title} />
</template>