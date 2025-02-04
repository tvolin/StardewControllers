namespace StarControl.Gmcm;

public class ModEventArgs(IManifest mod) : EventArgs
{
    public IManifest Mod => mod;
}
