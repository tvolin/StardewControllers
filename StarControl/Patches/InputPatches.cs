using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StarControl.Patches;

internal static class InputPatches
{
    public static Buttons? ToolUseButton { get; set; }

    private static readonly FieldInfo GameInputField = AccessTools.Field(
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
    private static readonly MethodInfo GetRemappedOldPadStateMethod = AccessTools.Method(
        typeof(InputPatches),
        nameof(GetRemappedOldPadState)
    );
    private static readonly FieldInfo OldPadStateField = AccessTools.Field(
        typeof(Game1),
        nameof(Game1.oldPadState)
    );

    public static IEnumerable<CodeInstruction> GenericGamePadStateTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldsfld, GameInputField),
                new CodeMatch(OpCodes.Callvirt, GetGamePadStateMethod)
            )
            .Repeat(
                matcher =>
                    matcher
                        .SetAndAdvance(OpCodes.Call, GetRemappedGamePadStateMethod)
                        .RemoveInstructions(1),
                _ =>
                    throw new InvalidOperationException(
                        "Couldn't find call to Game1.input.GetGamePadState() in the method body"
                    )
            )
            .InstructionEnumeration();
    }

    public static IEnumerable<CodeInstruction> GenericOldPadStateTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        var stateLocal = gen.DeclareLocal(typeof(GamePadState));
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(new CodeMatch(OpCodes.Ldsflda, OldPadStateField))
            .Repeat(
                matcher =>
                    matcher
                        .SetAndAdvance(OpCodes.Call, GetRemappedOldPadStateMethod)
                        .Insert(
                            new CodeInstruction(OpCodes.Stloc_S, stateLocal.LocalIndex),
                            new CodeInstruction(OpCodes.Ldloca_S, stateLocal.LocalIndex)
                        ),
                _ =>
                    throw new InvalidOperationException(
                        "Couldn't find call to Game1.oldPadState in the method body"
                    )
            )
            .InstructionEnumeration();
    }

    private static GamePadState GetRemappedGamePadState()
    {
        var gamepadState =
            Game1.playerOneIndex >= PlayerIndex.One
                ? GamePad.GetState(Game1.playerOneIndex)
                : new();
        RemapGamePadState(ref gamepadState);
        return gamepadState;
    }

    private static GamePadState GetRemappedOldPadState()
    {
        var gamepadState = Game1.oldPadState;
        RemapGamePadState(ref gamepadState);
        return gamepadState;
    }

    private static void RemapGamePadState(ref GamePadState gamepadState)
    {
        if (ToolUseButton is null)
        {
            return;
        }
        var downButtons = gamepadState.Buttons._buttons;
        if (gamepadState.IsButtonDown(ToolUseButton.Value))
        {
            downButtons |= Buttons.X;
        }
        gamepadState.Buttons = new(downButtons);
    }
}
