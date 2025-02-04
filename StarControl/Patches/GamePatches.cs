using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;

namespace StarControl.Patches;

internal static class GamePatches
{
    public static bool SuppressRightStickChatBox { get; set; }

    private static readonly MethodInfo IsButtonDownMethod = AccessTools.Method(
        typeof(GamePadState),
        nameof(GamePadState.IsButtonDown)
    );
    private static readonly MethodInfo IsRightStickDownOrSuppressedMethod = AccessTools.Method(
        typeof(GamePatches),
        nameof(IsRightStickDownOrSuppressed)
    );

    public static IEnumerable<CodeInstruction> UpdateChatBox_Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator gen,
        MethodBase original
    )
    {
        return new CodeMatcher(instructions, gen)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Ldc_I4, 128),
                new CodeMatch(OpCodes.Call, IsButtonDownMethod)
            )
            .ThrowIfNotMatch("Couldn't find right-stick button check in the method body")
            .SetOpcodeAndAdvance(OpCodes.Ldloc_S)
            .RemoveInstructions(2)
            .Insert(new CodeInstruction(OpCodes.Call, IsRightStickDownOrSuppressedMethod))
            .InstructionEnumeration();
    }

    private static bool IsRightStickDownOrSuppressed(GamePadState gamePadState)
    {
        return SuppressRightStickChatBox || gamePadState.IsButtonDown(Buttons.RightStick);
    }
}
