namespace RadialMenu.Gmcm;

public class ModEventArgs(IManifest mod) : EventArgs
{
    public IManifest Mod => mod;
}
