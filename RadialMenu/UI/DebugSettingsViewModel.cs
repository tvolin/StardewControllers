using PropertyChanged.SourceGenerator;

namespace RadialMenu.UI;

internal partial class DebugSettingsViewModel
{
    [Notify]
    private bool enableAllLogging;

    [Notify]
    private bool enableGmcmDetailedLogging;

    [Notify]
    private bool enableGmcmSyncLogging;

    [Notify]
    private bool enableItemActivationLogging;

    [Notify]
    private bool enableMenuInteractionLogging;

    [Notify]
    private bool enableQuickSlotLogging;

    private bool isLoading;

    private void OnEnableAllLoggingChanged()
    {
        if (isLoading)
        {
            return;
        }
        EnableGmcmDetailedLogging = EnableAllLogging;
        EnableGmcmSyncLogging = EnableAllLogging;
        EnableItemActivationLogging = EnableAllLogging;
        EnableMenuInteractionLogging = EnableAllLogging;
        EnableQuickSlotLogging = EnableAllLogging;
    }
}
