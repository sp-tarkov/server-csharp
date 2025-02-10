using System.Reflection;
using Core.Loaders;
using Core.Models.External;
using SptCommon.Annotations;

namespace _12Bundle;

[Injectable]
public class Bundle : IPostDBLoadMod
{
    private readonly BundleLoader _bundleLoader;


    public Bundle(
        BundleLoader bundleLoader)
    {
        _bundleLoader = bundleLoader;
    }

    public void PostDBLoad()
    {
        var modFolder = Directory.GetCurrentDirectory();
        var test = Assembly.GetExecutingAssembly().Location;
        _bundleLoader.AddBundles(Path.Join(modFolder, "/user/mods/Mod3/"));
    }
}
