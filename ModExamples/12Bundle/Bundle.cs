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
        // must be a relative path.
        // for example "./user/mods/Mod3"
        // . being the server.exe or root directory
        // follow all the way to your mods folder name
        // you will be only changing from "./user/mods/" onwards
        _bundleLoader.AddBundles("./user/mods/Mod3");
    }
}
