using RadialMenu.Config;

namespace RadialMenu.UI;

internal static class ConfigurationMenu
{
    public static void Open(IModHelper helper, ModConfig config)
    {
        var context = new ConfigurationViewModel(helper, config);
        context.Controller = ViewEngine.OpenChildMenu("Configuration", context);
        context.Controller.CanClose = () => context.IsNavigationEnabled;
        context.Controller.CloseAction = () =>
        {
            if (!context.Dismissed && context.HasUnsavedChanges())
            {
                context.ShowCloseConfirmation();
            }
            else
            {
                Game1.playSound("bigDeSelect");
                Game1.exitActiveMenu();
            }
        };
        // CloseSound is normally played before CloseAction has a chance to run; in order to
        // suppress the sound only when displaying the confirmation above, we need to suppress
        // it at all times and play it ad-hoc when "really" closing.
        context.Controller.CloseSound = "";
    }
}
