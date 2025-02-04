using HarmonyLib;

namespace StarControl.Patches;

internal static class Patcher
{
    public static void PatchAll(IManifest mod)
    {
        var harmony = new Harmony(mod.UniqueID);
        TryPatch(
            harmony,
            typeof(Game1),
            "UpdateChatBox",
            transpiler: new(typeof(GamePatches), nameof(GamePatches.UpdateChatBox_Transpiler))
        );
    }

    private static void TryPatch(
        Harmony harmony,
        Type targetType,
        string targetMethodName,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null
    )
    {
        try
        {
            var method = AccessTools.Method(targetType, targetMethodName);
            if (method is null)
            {
                Logger.Log(
                    $"Harmony patching failed: method {MethodName()} does not exist.",
                    LogLevel.Error
                );
            }
            harmony.Patch(method, prefix, postfix, transpiler, finalizer);
            Logger.Log($"Patched {MethodName()}.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to patch {MethodName()}: {ex}", LogLevel.Error);
        }
        return;

        string MethodName() => targetType.FullName + targetMethodName;
    }
}
