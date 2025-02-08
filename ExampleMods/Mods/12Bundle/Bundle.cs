using Core.Loaders;
using Core.Models.External;
using SptCommon.Annotations;

namespace ExampleMods.Mods._12Bundle;

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
        _bundleLoader.AddBundles(modFolder);
    }
}
