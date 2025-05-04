using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BundleHashCacheService
{
    protected const string _bundleHashCachePath = "./user/cache/";
    protected const string _cacheName = "bundleHashCache.json";
    protected readonly Dictionary<string, uint> _bundleHashes = new();
    private readonly FileUtil _fileUtil;
    private readonly HashUtil _hashUtil;
    private readonly JsonUtil _jsonUtil;
    private readonly ISptLogger<BundleHashCacheService> _logger;

    public BundleHashCacheService(
        ISptLogger<BundleHashCacheService> logger,
        JsonUtil jsonUtil,
        HashUtil hashUtil,
        FileUtil fileUtil
    )
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _hashUtil = hashUtil;
        _fileUtil = fileUtil;
    }

    public uint GetStoredValue(string key)
    {
        if (!_bundleHashes.TryGetValue(key, out var value))
        {
            return 0;
        }

        return value;
    }

    public void StoreValue(string bundlePath, uint hash)
    {
        _bundleHashes.Add(bundlePath, hash);

        if (!Directory.Exists(_bundleHashCachePath))
        {
            Directory.CreateDirectory(_bundleHashCachePath);
        }

        _fileUtil.WriteFile(
            Path.Join(_bundleHashCachePath, _cacheName),
            _jsonUtil.Serialize(_bundleHashes)
        );

        _logger.Debug($"Bundle: {bundlePath} hash stored in: ${_bundleHashCachePath}");
    }

    public bool CalculateAndMatchHash(string BundlePath)
    {
        return MatchWithStoredHash(BundlePath, CalculateHash(BundlePath));
    }

    public void CalculateAndStoreHash(string BundlePath)
    {
        StoreValue(BundlePath, CalculateHash(BundlePath));
    }

    public uint CalculateHash(string BundlePath)
    {
        var fileData = _fileUtil.ReadFile(BundlePath);
        return _hashUtil.GenerateCrc32ForData(fileData);
    }

    public bool MatchWithStoredHash(string BundlePath, uint hash)
    {
        return GetStoredValue(BundlePath) == hash;
    }
}
