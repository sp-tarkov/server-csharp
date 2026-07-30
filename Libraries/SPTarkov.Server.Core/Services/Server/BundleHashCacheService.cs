using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Server;

[Injectable(InjectionType.Singleton)]
public sealed class BundleHashCacheService(JsonUtil jsonUtil, HashUtil hashUtil, FileUtil fileUtil)
{
    private const string BundleHashCachePath = "./user/cache/";
    private const string CacheName = "bundleHashCache.json";

    private ConcurrentDictionary<string, uint> _bundleHashes = [];
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async Task HydrateCacheAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(BundleHashCachePath))
        {
            Directory.CreateDirectory(BundleHashCachePath);
        }

        var fullCachePath = Path.Join(BundleHashCachePath, CacheName);

        // File doesn't exist, assume this is the first time we're trying to load in bundles
        if (!File.Exists(fullCachePath))
        {
            return;
        }

        _bundleHashes = await jsonUtil.DeserializeFromFileAsync<ConcurrentDictionary<string, uint>>(fullCachePath, cancellationToken) ?? [];
    }

    public async Task WriteCacheAsync(CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken);

        try
        {
            var bundleHashesSerialized = jsonUtil.Serialize(_bundleHashes);

            if (bundleHashesSerialized is null)
            {
                return;
            }

            await fileUtil.WriteFileAsync(Path.Join(BundleHashCachePath, CacheName), bundleHashesSerialized, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private uint GetStoredValue(string key)
    {
        if (!_bundleHashes.TryGetValue(key, out var value))
        {
            return 0;
        }

        return value;
    }

    private void StoreValue(string bundlePath, uint hash)
    {
        _bundleHashes.TryAdd(bundlePath, hash);
    }

    /// <summary>
    /// Calculate, match the current hash and store the correct hash of the bundle
    /// </summary>
    /// <param name="BundlePath">The path to the bundle</param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that can be used to cancel the hashing operation.
    /// </param>
    public async Task<uint> CalculateMatchAndStoreHashAsync(string BundlePath, CancellationToken cancellationToken = default)
    {
        var hash = await CalculateHashAsync(BundlePath, cancellationToken);

        if (!MatchWithStoredHash(BundlePath, hash))
        {
            StoreValue(BundlePath, hash);
        }

        return hash;
    }

    public async Task<uint> CalculateHashAsync(string BundlePath, CancellationToken cancellationToken = default)
    {
        return await hashUtil.GenerateCrc32ForFileAsync(BundlePath, cancellationToken);
    }

    private bool MatchWithStoredHash(string BundlePath, uint hash)
    {
        return GetStoredValue(BundlePath) == hash;
    }
}
