using System.Collections.Concurrent;
using Spectre.Console;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Spt.Bundles;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Server;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectionType.Singleton)]
public sealed class BundleLoader(ISptLogger<BundleLoader> logger, JsonUtil jsonUtil, BundleHashCacheService bundleHashCacheService)
{
    private readonly ConcurrentDictionary<string, BundleInfo> _bundles = [];

    public async Task LoadBundlesAsync(SptMod mod, CancellationToken cancellationToken = default)
    {
        await bundleHashCacheService.HydrateCacheAsync(cancellationToken);

        var modPath = mod.GetModPath();
        var modBundles = await jsonUtil.DeserializeFromFileAsync<BundleManifest>(
            Path.Join(Directory.GetCurrentDirectory(), modPath, "bundles.json"),
            cancellationToken
        );

        var relativeModPath = modPath.Replace('\\', '/');
        var bundlesPath = Path.Join(relativeModPath, "bundles");

        if (modBundles?.Manifest is null)
        {
            logger.Warning($"Could not load bundle manifest for mod {mod.ModMetadata.Name}, skipping!");
            return;
        }

        var total = modBundles.Manifest.Count;
        var ok = 0;
        var missing = 0;

        await AnsiConsole
            .Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn()
            )
            .StartAsync(async ctx =>
            {
                var progressTask = ctx.AddTask(
                    $"Loading bundles for {mod.ModMetadata.Name}",
                    new ProgressTaskSettings { MaxValue = total }
                );

                await Parallel.ForEachAsync(
                    modBundles.Manifest,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    async (bundleManifest, ct) =>
                    {
                        var bundleLocalPath = Path.Join(bundlesPath, bundleManifest.Key).Replace('\\', '/');

                        if (!File.Exists(bundleLocalPath))
                        {
                            logger.Warning($"Could not find bundle {bundleManifest.Key} for mod {mod.ModMetadata.Name}");
                            Interlocked.Increment(ref missing);
                        }
                        else
                        {
                            var bundleHash = await bundleHashCacheService.CalculateMatchAndStoreHashAsync(bundleLocalPath, ct);
                            AddBundle(
                                bundleManifest.Key,
                                new BundleInfo
                                {
                                    ModPath = relativeModPath,
                                    Bundle = bundleManifest,
                                    Crc = bundleHash,
                                }
                            );
                            Interlocked.Increment(ref ok);
                        }

                        progressTask.Increment(1);
                        progressTask.Description = $"Loading bundles for {mod.ModMetadata.Name} (ok: {ok}, missing: {missing})";
                    }
                );
            });

        await bundleHashCacheService.WriteCacheAsync(cancellationToken);
    }

    /// <summary>
    ///     HandleAsync singleplayer/bundles
    /// </summary>
    /// <returns> List of loaded bundles.</returns>
    public List<BundleInfo> GetBundles()
    {
        var result = new List<BundleInfo>();

        foreach (var bundle in _bundles)
        {
            result.Add(bundle.Value);
        }

        return result;
    }

    public BundleInfo? GetBundle(string bundleKey)
    {
        return _bundles.GetValueOrDefault(bundleKey);
    }

    public void AddBundle(string key, BundleInfo bundle)
    {
        var success = _bundles.TryAdd(key, bundle);
        if (!success)
        {
            logger.Error($"Unable to add bundle: {key}");
        }
    }
}
