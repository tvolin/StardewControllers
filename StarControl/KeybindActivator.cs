using System.Reflection;
using StardewModdingAPI.Utilities;

namespace StarControl;

internal class KeybindActivator
{
    private readonly IInputHelper inputHelper;
    private readonly FieldInfo currentInputStateField;
    private readonly MethodInfo overrideButtonMethod;
    private readonly List<SButton> pendingButtons = [];

    public KeybindActivator(IInputHelper inputHelper)
    {
        this.inputHelper = inputHelper;
        // We can't use ReflectionHelper here because SMAPI blocks reflection on itself.
        currentInputStateField = inputHelper
            .GetType()
            .GetField(
                "CurrentInputState",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            )!;
        // We don't really need the input state object at construction time, but since we don't
        // have direct access to its type, we have to use the object to get it.
        var currentInputState = GetCurrentInputState();
        overrideButtonMethod = currentInputState
            .GetType()
            .GetMethod(
                "OverrideButton",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                [typeof(SButton), typeof(bool)]
            )!;
    }

    public void Prepare(Keybind keybind)
    {
        if (!keybind.IsBound)
        {
            return;
        }
        foreach (var button in keybind.Buttons)
        {
            pendingButtons.Add(button);
        }
    }

    public void Replay()
    {
        var inputState = GetCurrentInputState();
        // Overrides are transient, because the input state itself is transient and recreated on
        // every frame. Therefore we don't need to remove the override; rather, if we wanted to
        // "hold" the button down, we'd need to keep doing this on every subsequent frame.
        foreach (var button in pendingButtons)
        {
            overrideButtonMethod.Invoke(inputState, [button, true]);
        }
        pendingButtons.Clear();
    }

    private object GetCurrentInputState()
    {
        return ((Delegate)(currentInputStateField.GetValue(inputHelper)!)).DynamicInvoke()!;
    }
}
