using System.Runtime;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Services;

[Injectable(InjectionType.Singleton)]
public class SptServerStartupService(IReadOnlyList<SptMod> loadedMods, BundleLoader bundleLoader, App app)
{
    public async Task Startup()
    {
        if (ProgramStatics.MODS())
        {
            foreach (var mod in loadedMods)
            {
                if (mod.ModMetadata.IsBundleMod == true)
                {
                    await bundleLoader.LoadBundlesAsync(mod);
                }
            }
        }

        await app.InitializeAsync();

        // Run garbage collection now the server is ready to start
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
    }
}
