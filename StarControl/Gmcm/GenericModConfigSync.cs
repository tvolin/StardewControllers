using StarControl.Config;

namespace StarControl.Gmcm;

internal class GenericModConfigSync(ModConfig config, IGenericModConfigKeybindings bindings)
{
    public bool SyncAll(IManifest? mod = null)
    {
        var anyModified = false;
        foreach (var page in config.Items.ModMenuPages)
        {
            foreach (var item in page)
            {
                if (mod is not null && item.GmcmSync?.ModId != mod.UniqueID)
                {
                    continue;
                }
                anyModified |= Sync(item, config.Debug.EnableGmcmSyncLogging);
            }
        }
        return anyModified;
    }

    private bool Sync(ModMenuItemConfiguration item, bool enableLogging = false)
    {
        if (item.GmcmSync is not { } gmcm)
        {
            return false;
        }
        var keybindOption = bindings.Find(gmcm.ModId, gmcm.FieldId, gmcm.FieldName, item.Keybind);
        if (keybindOption is null)
        {
            Logger.Log(
                $"Couldn't sync key binding information for item named '{item.Name}'. "
                    + $"No keybinding field in {gmcm.ModId} for field name '{gmcm.FieldName}' or "
                    + $"field ID {gmcm.FieldId}.",
                LogLevel.Warn
            );
            return false;
        }
        if (gmcm.EnableNameSync)
        {
            item.Name = keybindOption.ModManifest.Name;
        }
        if (gmcm.EnableDescriptionSync)
        {
            // Some mod names can be quite long, the most obvious being "Generic Mod Config Menu"
            // itself. Since the title uses large font and there is limited space, it's usually a
            // better idea to combine both the field name and tooltip into the description, instead
            // of making the field name part of the title as it might be shown in the GMCM select
            // box.
            item.Description = !string.IsNullOrWhiteSpace(keybindOption.Tooltip)
                ? $"{keybindOption.FieldName} - {keybindOption.Tooltip}"
                : keybindOption.FieldName;
        }
        gmcm.FieldId = keybindOption.FieldId;
        gmcm.FieldName = keybindOption.UniqueFieldName;
        item.Keybind = keybindOption.GetCurrentBinding();
        if (enableLogging)
        {
            Logger.Log(
                LogCategory.GmcmSync,
                $"Synced GMCM keybinding for item '{item.Name}'.",
                LogLevel.Info
            );
        }
        return true;
    }
}
