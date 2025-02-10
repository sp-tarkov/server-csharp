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
        _bundleLoader.AddBundles("/user/mods/Mod3");
    }
}
