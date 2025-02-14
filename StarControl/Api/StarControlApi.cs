using StarControl.Config;
using StarControl.Menus;

namespace StarControl.Api;

/// <summary>
/// Implementation of the <see cref="IStarControlApi"/>.
/// </summary>
/// <remarks>
/// Must be public to satisfy SMAPI requirements. Avoid passing concrete references.
/// </remarks>
public class StarControlApi : IStarControlApi
{
    private readonly PageRegistry registry;
    private readonly ModConfig config;
    private readonly IMonitor monitor;

    internal StarControlApi(PageRegistry registry, ModConfig config, IMonitor monitor)
    {
        this.registry = registry;
        this.config = config;
        this.monitor = monitor;
    }

    public void InvalidatePage(IManifest mod, string id)
    {
        var pageKey = GetPageKey(mod, id);
        if (!registry.InvalidatePage(pageKey))
        {
            monitor.Log($"No menu page '{id}' registered for mod '{mod.UniqueID}'.", LogLevel.Warn);
        }
    }

    public void RegisterCustomMenuPage(IManifest mod, string id, IRadialMenuPageFactory factory)
    {
        var pageKey = GetPageKey(mod, id);
        registry.RegisterPage(mod, pageKey, factory);
        if (!config.Integrations.Priorities.Any(p => p.ModId == mod.UniqueID))
        {
            config.Integrations.Priorities.Add(new() { ModId = mod.UniqueID });
        }
        monitor.Log($"Registered menu page '{id}' for mod '{mod.UniqueID}'.", LogLevel.Info);
    }

    public void RegisterItems(IManifest mod, IEnumerable<IRadialMenuItem> items)
    {
        foreach (var item in items)
        {
            registry.RegisterItem(mod, item);
            monitor.Log(
                $"Registered menu item '{item.Title}' for mod '{mod.UniqueID}'.",
                LogLevel.Info
            );
        }
    }

    private static string GetPageKey(IManifest mod, string id)
    {
        return $"{mod.UniqueID}:{id}";
    }
}
