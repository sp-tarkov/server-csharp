using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services.Cache;

[Injectable]
public class ModHashCacheService(
    ISptLogger<ModHashCacheService> _logger,
    JsonUtil _jsonUtil,
    HashUtil _hashUtil,
    FileUtil _fileUtil
)
{
    protected readonly Dictionary<string, string> _modHashes = new();
    protected readonly string _modCachePath = "./user/cache/modCache.json";

    public string? GetStoredValue(string key)
    {
        _modHashes.TryGetValue(key, out var value);

        return value;
    }

    public void StoreValue(string key, string value)
    {
        _modHashes.TryAdd(key, value);

        _fileUtil.WriteFile(_modCachePath, _jsonUtil.Serialize(_modHashes));

        if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Mod {key} hash stored in: {_modCachePath}");
    }

    public bool MatchWithStoredHash(string modName, string hash)
    {
        return GetStoredValue(modName) == hash;
    }

    public bool CalculateAndCompareHash(string modName, string modContent)
    {
        var generatedHash = _hashUtil.GenerateSha1ForData(modContent);

        return MatchWithStoredHash(modName, generatedHash);
    }

    public void CalculateAndStoreHash(string modName, string modContent)
    {
        var generatedHash = _hashUtil.GenerateSha1ForData(modContent);

        StoreValue(modName, generatedHash);
    }
}
