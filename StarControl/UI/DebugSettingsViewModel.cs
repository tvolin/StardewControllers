using PropertyChanged.SourceGenerator;
using StarControl.Config;

namespace StarControl.UI;

internal partial class DebugSettingsViewModel
{
    [Notify]
    private bool enableAllLogging;

    [Notify]
    private bool enableGmcmDetailedLogging;

    [Notify]
    private bool enableGmcmSyncLogging;

    [Notify]
    private bool enableInputLogging;

    [Notify]
    private bool enableItemActivationLogging;

    [Notify]
    private bool enableMenuInteractionLogging;

    [Notify]
    private bool enableQuickSlotLogging;

    private bool isLoading;

    public void Load(DebugConfiguration config)
    {
        isLoading = true;
        try
        {
            EnableGmcmDetailedLogging = config.EnableGmcmDetailedLogging;
            EnableGmcmSyncLogging = config.EnableGmcmSyncLogging;
            EnableInputLogging = config.EnableInputLogging;
            EnableItemActivationLogging = config.EnableItemActivationLogging;
            EnableMenuInteractionLogging = config.EnableMenuInteractionLogging;
            EnableQuickSlotLogging = config.EnableQuickSlotLogging;
            EnableAllLogging =
                EnableGmcmDetailedLogging
                && EnableGmcmSyncLogging
                && EnableItemActivationLogging
                && EnableMenuInteractionLogging
                && EnableQuickSlotLogging;
        }
        finally
        {
            isLoading = false;
        }
    }

    public void Save(DebugConfiguration config)
    {
        config.EnableGmcmDetailedLogging = EnableGmcmDetailedLogging;
        config.EnableGmcmSyncLogging = EnableGmcmSyncLogging;
        config.EnableInputLogging = EnableInputLogging;
        config.EnableItemActivationLogging = EnableItemActivationLogging;
        config.EnableMenuInteractionLogging = EnableMenuInteractionLogging;
        config.EnableQuickSlotLogging = EnableQuickSlotLogging;
    }

    private void OnEnableAllLoggingChanged()
    {
        if (isLoading)
        {
            return;
        }
        EnableGmcmDetailedLogging = EnableAllLogging;
        EnableGmcmSyncLogging = EnableAllLogging;
        EnableInputLogging = EnableAllLogging;
        EnableItemActivationLogging = EnableAllLogging;
        EnableMenuInteractionLogging = EnableAllLogging;
        EnableQuickSlotLogging = EnableAllLogging;
    }
}
