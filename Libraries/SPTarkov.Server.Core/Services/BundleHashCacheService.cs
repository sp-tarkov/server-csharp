using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BundleHashCacheService(ISptLogger<BundleHashCacheService> logger, JsonUtil jsonUtil, HashUtil hashUtil, FileUtil fileUtil)
{
    protected const string _bundleHashCachePath = "./user/cache/";
    protected const string _cacheName = "bundleHashCache.json";
    protected Dictionary<string, uint> _bundleHashes = [];

    public async Task HydrateCache()
    {
        if (!Directory.Exists(_bundleHashCachePath))
        {
            Directory.CreateDirectory(_bundleHashCachePath);
        }

        var fullCachePath = Path.Join(_bundleHashCachePath, _cacheName);

        // File doesn't exist, assume this is the first time we're trying to load in bundles
        if (!File.Exists(fullCachePath))
        {
            return;
        }

        _bundleHashes = await jsonUtil.DeserializeFromFileAsync<Dictionary<string, uint>>(fullCachePath) ?? [];
    }

    protected uint GetStoredValue(string key)
    {
        if (!_bundleHashes.TryGetValue(key, out var value))
        {
            return 0;
        }

        return value;
    }

    protected async Task StoreValue(string bundlePath, uint hash)
    {
        _bundleHashes.Add(bundlePath, hash);

        var bundleHashesSerialized = jsonUtil.Serialize(_bundleHashes);

        if (bundleHashesSerialized is null)
        {
            return;
        }

        await fileUtil.WriteFileAsync(Path.Join(_bundleHashCachePath, _cacheName), bundleHashesSerialized);

        logger.Debug($"Bundle: {bundlePath} hash stored in: ${_bundleHashCachePath}");
    }

    /// <summary>
    /// Calculate, match the current hash and store the correct hash of the bundle
    /// </summary>
    /// <param name="BundlePath">The path to the bundle</param>
    public async Task<uint> CalculateMatchAndStoreHash(string BundlePath)
    {
        var hash = await CalculateHash(BundlePath);

        if (!MatchWithStoredHash(BundlePath, hash))
        {
            await StoreValue(BundlePath, await CalculateHash(BundlePath));
        }

        return hash;
    }

    protected async Task<uint> CalculateHash(string BundlePath)
    {
        return hashUtil.GenerateCrc32ForData(await fileUtil.ReadFileAsBytesAsync(BundlePath));
    }

    protected bool MatchWithStoredHash(string BundlePath, uint hash)
    {
        return GetStoredValue(BundlePath) == hash;
    }
}
