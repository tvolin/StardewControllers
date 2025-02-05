using StarControl.Data;

namespace StarControl.Menus;

internal class RemappingController
{
    public bool HudVisible { get; set; }

    public Dictionary<SButton, RemappingSlot> Slots
    {
        get => slots;
        set => slots = value;
    }

    private Dictionary<SButton, RemappingSlot> slots = [];
}
