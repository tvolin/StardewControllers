using System.Reflection;
using GenericModConfigMenu.Framework;
using HarmonyLib;

namespace RadialMenu.Gmcm;

internal static class HarmonyPatches
{
    public static KeybindData? Data { get; set; }

    public static void Initialize(IMonitor monitor)
    {
        var harmony = new Harmony("focustense.RadialMenu.Gmcm");
        TryPatch(
            harmony,
            typeof(SpecificModConfigMenu),
            nameof(SpecificModConfigMenu.SaveConfig),
            postfix: new(typeof(HarmonyPatches), nameof(SpecificModConfigMenu_SaveConfig_Postfix)),
            monitor: monitor
        );
    }

    private static void TryPatch(
        Harmony harmony,
        Type targetType,
        string targetMethodName,
        HarmonyMethod? prefix = null,
        HarmonyMethod? postfix = null,
        HarmonyMethod? transpiler = null,
        HarmonyMethod? finalizer = null,
        IMonitor? monitor = null
    )
    {
        try
        {
            var method = AccessTools.Method(targetType, targetMethodName);
            if (method is null)
            {
                monitor?.Log(
                    $"Harmony patching failed: method {MethodName()} does not exist.",
                    LogLevel.Error
                );
            }
            harmony.Patch(method, prefix, postfix, transpiler, finalizer);
        }
        catch (Exception ex)
        {
            monitor?.Log($"Failed to patch method {MethodName()}: {ex}");
        }
        return;

        string MethodName() => targetType.FullName + targetMethodName;
    }

    private static void SpecificModConfigMenu_SaveConfig_Postfix(SpecificModConfigMenu __instance)
    {
        Data?.NotifySaved(__instance.Manifest);
    }
}
