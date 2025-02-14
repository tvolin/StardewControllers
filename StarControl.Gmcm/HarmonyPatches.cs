using GenericModConfigMenu.Framework;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Mod = GenericModConfigMenu.Mod;

namespace StarControl.Gmcm;

internal static class HarmonyPatches
{
    internal static KeybindData? Data { get; set; }
    internal static IManifest? ModManifest { get; set; }
    internal static Action<Action>? OpenRealConfigMenu { get; set; }

    public static void Initialize(IMonitor monitor)
    {
        var harmony = new Harmony("focustense.StarControl.Gmcm");
        TryPatch(
            harmony,
            typeof(SpecificModConfigMenu),
            nameof(SpecificModConfigMenu.SaveConfig),
            postfix: new(typeof(HarmonyPatches), nameof(SpecificModConfigMenu_SaveConfig_Postfix)),
            monitor: monitor
        );
        if (OpenRealConfigMenu is not null)
        {
            var openModMenuPrefix = new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(OpenModMenu_Prefix)
            );
            TryPatch(
                harmony,
                typeof(Mod),
                nameof(Mod.OpenModMenu),
                prefix: openModMenuPrefix,
                monitor: monitor
            );
            TryPatch(
                harmony,
                typeof(Mod),
                nameof(Mod.OpenModMenuNew),
                prefix: openModMenuPrefix,
                monitor: monitor
            );
        }
        else
        {
            monitor.Log(
                "Unable to set up direct config menu integration because the Star Control "
                    + "configuration callback has not been set up.",
                LogLevel.Error
            );
        }
    }

    private static bool OpenModMenu_Prefix(Mod __instance, IManifest mod, int? listScrollRow)
    {
        if (mod != ModManifest)
        {
            return true;
        }
        if (Game1.activeClickableMenu is TitleMenu titleMenu)
        {
            // GMCM hacks this to be false, so we hack it back to be true. Otherwise the fake cursor
            // movement will fight with the real gamepad controls supported by Stardew UI.
            titleMenu.titleInPosition = true;
        }
        OpenRealConfigMenu!.Invoke(() => __instance.OpenListMenu(listScrollRow));
        return false;
    }

    private static void SpecificModConfigMenu_SaveConfig_Postfix(SpecificModConfigMenu __instance)
    {
        Data?.NotifySaved(__instance.Manifest);
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
            monitor?.Log($"Patched {MethodName()}.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            monitor?.Log($"Failed to patch {MethodName()}: {ex}", LogLevel.Error);
        }
        return;

        string MethodName() => targetType.FullName + targetMethodName;
    }
}
