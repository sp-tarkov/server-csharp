using System.Runtime;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Services;

public class SptServerBackgroundService(IReadOnlyList<SptMod> loadedMods, BundleLoader bundleLoader, App app) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (ProgramStatics.MODS())
        {
            foreach (var mod in loadedMods)
            {
                if (mod.ModMetadata?.IsBundleMod == true)
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
