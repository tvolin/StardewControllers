using System.Reflection;

namespace StarControl.Gmcm;

public static class Loader
{
    public static void Load(
        IManifest modManifest,
        Action<Action> openRealConfigMenu,
        IMonitor monitor,
        bool enableDetailedLogging
    )
    {
        HarmonyPatches.ModManifest = modManifest;
        HarmonyPatches.OpenRealConfigMenu = openRealConfigMenu;
        try
        {
            var data = KeybindData.Load();
            monitor.Log("Finished reading keybindings from GMCM.", LogLevel.Info);
            HarmonyPatches.Data = data;
            if (enableDetailedLogging)
            {
                foreach (var option in data.AllOptions)
                {
                    monitor.Log(
                        "Found keybind option: "
                            + $"[{option.ModManifest.UniqueID}] - {option.UniqueFieldName}",
                        LogLevel.Info
                    );
                }
            }
            IGenericModConfigKeybindings.Instance = data;
        }
        catch (Exception ex) when (ex is InvalidOperationException or TargetInvocationException)
        {
            monitor.Log(
                "Couldn't read global keybindings; the current version of GMCM is not compatible.\n"
                    + ex,
                LogLevel.Error
            );
        }

        HarmonyPatches.Initialize(monitor);
    }
}
