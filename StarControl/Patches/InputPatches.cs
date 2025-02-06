using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StarControl.Patches;

internal static class InputPatches
{
    public static Buttons? ToolUseButton { get; set; }

    private static readonly FieldInfo GetGameInputField = AccessTools.Field(
        typeof(Game1),
        nameof(Game1.input)
    );
    private static readonly MethodInfo GetGamePadStateMethod = AccessTools.Method(
        typeof(InputState),
        nameof(InputState.GetGamePadState)
    );
    private static readonly MethodInfo GetRemappedGamePadStateMethod = AccessTools.Method(
        typeof(InputPatches),
        nameof(GetRemappedGamePadState)
    );

    public static IEnumerable<CodeInstruction> GenericGamePadStateTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldsfld, GetGameInputField),
                new CodeMatch(OpCodes.Callvirt, GetGamePadStateMethod)
            )
            .ThrowIfNotMatch(
                "Couldn't find call to Game1.input.GetGamePadState() in the method body"
            )
            .SetAndAdvance(OpCodes.Call, GetRemappedGamePadStateMethod)
            .RemoveInstructions(1)
            .InstructionEnumeration();
    }

    private static GamePadState GetRemappedGamePadState()
    {
        var gamepadState =
            Game1.playerOneIndex >= PlayerIndex.One
                ? GamePad.GetState(Game1.playerOneIndex)
                : new();
        if (ToolUseButton is null)
        {
            return gamepadState;
        }
        var downButtons = gamepadState.Buttons._buttons;
        if (gamepadState.IsButtonDown(ToolUseButton.Value))
        {
            downButtons |= Buttons.X;
        }
        gamepadState.Buttons = new(downButtons);
        return gamepadState;
    }
}
