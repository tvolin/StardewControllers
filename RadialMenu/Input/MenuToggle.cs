using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RadialMenu.Config;

namespace RadialMenu.Input;

/// <summary>
/// Menu toggle implementation based on a hybrid of SMAPI and direct XNA/MonoGame APIs.
/// </summary>
/// <remarks>
/// Ensures correct <see cref="State"/> transitions and works around certain limitations of SMAPI
/// input suppressions during standard gameplay (farmer-world interaction).
/// </remarks>
/// <param name="inputHelper">SMAPI input helper, used to check button states.</param>
/// <param name="inputConfig">The current input settings.</param>
public class MenuToggle(
    IInputHelper inputHelper,
    InputConfiguration inputConfig,
    Func<InputConfiguration, SButton> buttonSelector
) : IMenuToggle
{
    /// <inheritdoc />
    public MenuToggleState State { get; private set; }

    private SButton Button => buttonSelector(inputConfig);
    private MenuToggleMode Mode => inputConfig.ToggleMode;
    private float TriggerDeadZone => inputConfig.TriggerDeadZone;

    private GamePadState gamePadState;
    private bool wasDown;

    /// <inheritdoc />
    public void ForceOff()
    {
        State = Mode == MenuToggleMode.Hold ? MenuToggleState.Suppressed : MenuToggleState.Off;
        Logger.Log(LogCategory.Input, $"Toggle for {Button} was forced off; new state is {State}.");
    }

    /// <inheritdoc />
    public bool IsRightSided()
    {
        return Button
            is SButton.ControllerA
                or SButton.ControllerB
                or SButton.ControllerX
                or SButton.ControllerY
                or SButton.RightStick
                or SButton.RightTrigger
                or SButton.RightShoulder;
    }

    /// <inheritdoc />
    public void PreUpdate()
    {
        if (RequiresSmapiBypass(Button))
        {
            Logger.LogOnce(
                LogCategory.Input,
                $"Button {Button} used for menu toggle requires SMAPI bypass due to timing/state "
                    + "discrepancies and will be suppressed before every update tick."
            );
            inputHelper.Suppress(Button);
        }
    }

    /// <inheritdoc />
    public void Update(bool allowOn)
    {
        gamePadState = GetRawGamePadState();
        var isDown = IsButtonDown(Button);
        if (isDown != wasDown)
        {
            Logger.Log(
                LogCategory.Input,
                $"Down state for {Button} changed from {wasDown} -> {isDown}."
            );
        }
        var nextState = State switch
        {
            MenuToggleState.Off when !wasDown && isDown => allowOn
                ? MenuToggleState.On
                : MenuToggleState.Wait,
            MenuToggleState.Wait when isDown && allowOn => MenuToggleState.On,
            MenuToggleState.Wait when !isDown && Mode == MenuToggleMode.Hold => MenuToggleState.Off,
            MenuToggleState.On when !isDown && Mode == MenuToggleMode.Hold => MenuToggleState.Off,
            MenuToggleState.On when !wasDown && isDown && Mode == MenuToggleMode.Toggle =>
                MenuToggleState.Off,
            MenuToggleState.Suppressed when !isDown => MenuToggleState.Off,
            _ => State,
        };
        if (nextState != State)
        {
            Logger.LogOnce(
                LogCategory.Input,
                $"Toggle state is changing from {State} -> {nextState}. (Current mode: {Mode})"
            );
        }
        State = nextState;
        wasDown = isDown;
        if (isDown && !RequiresSmapiBypass(Button))
        {
            Logger.Log(
                LogCategory.Input,
                $"Suppressing button {Button} which triggered state change."
            );
            inputHelper.Suppress(Button);
        }
    }

    private static GamePadState GetRawGamePadState()
    {
        return Game1.playerOneIndex >= PlayerIndex.One
            ? GamePad.GetState(Game1.playerOneIndex)
            : new();
    }

    private bool IsButtonDown(SButton button)
    {
        return button switch
        {
            SButton.LeftTrigger => gamePadState.Triggers.Left > TriggerDeadZone,
            SButton.RightTrigger => gamePadState.Triggers.Right > TriggerDeadZone,
            _ => inputHelper.IsDown(button),
        };
    }

    // The usual approach to handling a keybind is watch for it, and then after it is detected,
    // suppress it with SMAPI's input helper to block its vanilla function. This doesn't work with
    // some buttons due to differences between SMAPI's dead zone and/or timing, and Stardew's. When
    // one of these buttons is used for menu activation, we instead have to constantly suppress the
    // vanilla input, and bypass SMAPI's input helper to get the raw value of the axis.
    private static bool RequiresSmapiBypass(SButton button)
    {
        return button is SButton.LeftTrigger or SButton.RightTrigger;
    }
}
