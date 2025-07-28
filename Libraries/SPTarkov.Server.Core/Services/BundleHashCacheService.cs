using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BundleHashCacheService(ISptLogger<BundleHashCacheService> logger, JsonUtil jsonUtil, HashUtil hashUtil, FileUtil fileUtil)
{
    protected const string _bundleHashCachePath = "./user/cache/";
    protected const string _cacheName = "bundleHashCache.json";
    protected readonly Dictionary<string, uint> _bundleHashes = [];

    public uint GetStoredValue(string key)
    {
        if (!_bundleHashes.TryGetValue(key, out var value))
        {
            return 0;
        }

        return value;
    }

    public async Task StoreValue(string bundlePath, uint hash)
    {
        _bundleHashes.Add(bundlePath, hash);

        if (!Directory.Exists(_bundleHashCachePath))
        {
            Directory.CreateDirectory(_bundleHashCachePath);
        }

        await fileUtil.WriteFileAsync(Path.Join(_bundleHashCachePath, _cacheName), jsonUtil.Serialize(_bundleHashes));

        logger.Debug($"Bundle: {bundlePath} hash stored in: ${_bundleHashCachePath}");
    }

    public bool CalculateAndMatchHash(string BundlePath)
    {
        return MatchWithStoredHash(BundlePath, CalculateHash(BundlePath));
    }

    public async Task CalculateAndStoreHash(string BundlePath)
    {
        await StoreValue(BundlePath, CalculateHash(BundlePath));
    }

    public uint CalculateHash(string BundlePath)
    {
        var fileData = fileUtil.ReadFile(BundlePath);
        return hashUtil.GenerateCrc32ForData(fileData);
    }

    public bool MatchWithStoredHash(string BundlePath, uint hash)
    {
        return GetStoredValue(BundlePath) == hash;
    }
}
