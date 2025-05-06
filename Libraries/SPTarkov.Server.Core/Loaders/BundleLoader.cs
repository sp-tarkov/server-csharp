using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Loaders;

/*
{
    "ModPath" : "/user/mods/Mod3",
    "FileName" : "assets/content/weapons/usable_items/item_bottle/textures/client_assets.bundle",
    "Bundle" : {
        "key" : "assets/content/weapons/usable_items/item_bottle/textures/client_assets.bundle",
        "dependencyKeys" : [ ]
    },
    "Crc" : 1030040371,
    "Dependencies" : [ ]
} */
public class BundleInfo
{
    public BundleInfo()
    {
    }

    public BundleInfo(
        string modPath,
        BundleManifestEntry bundle,
        uint bundleHash)
    {
        ModPath = modPath;
        FileName = bundle.Key;
        Bundle = bundle;
        Crc = bundleHash;
        Dependencies = bundle?.DependencyKeys ?? [];
    }

    public string? ModPath
    {
        get;
        set;
    }

    public string FileName
    {
        get;
        set;
    }

    public BundleManifestEntry Bundle
    {
        get;
        set;
    }

    public uint Crc
    {
        get;
        set;
    }

    public List<string> Dependencies
    {
        get;
        set;
    }
}

[Injectable(InjectionType.Singleton)]
public class BundleLoader
{
    private readonly BundleHashCacheService _bundleHashCacheService;
    private readonly Dictionary<string, BundleInfo> _bundles = new();
    private readonly ICloner _cloner;
    private readonly FileUtil _fileUtil;
    private readonly HashUtil _hashUtil;
    private readonly InMemoryCacheService _inMemoryCacheService;
    private readonly JsonUtil _jsonUtil;
    private readonly ISptLogger<BundleLoader> _logger;

    public BundleLoader(
        ISptLogger<BundleLoader> logger,
        HashUtil hashUtil,
        JsonUtil jsonUtil,
        FileUtil fileUtil,
        BundleHashCacheService bundleHashCacheService,
        InMemoryCacheService inMemoryCacheService,
        ICloner cloner)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        _bundleHashCacheService = bundleHashCacheService;
        _inMemoryCacheService = inMemoryCacheService;
        _cloner = cloner;
    }

    /// <summary>
    ///     Handle singleplayer/bundles
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
        return _cloner.Clone(_bundles.GetValueOrDefault(bundleKey));
    }

    public void AddBundles(string modPath)
    {
        // modPath should be relative to the server exe - ./user/mods/Mod3
        // TODO: make sure the mod is passing a path that is relative from the server exe

        var modBundlesJson = _fileUtil.ReadFile(Path.Join(Directory.GetCurrentDirectory(), modPath, "bundles.json"));
        var modBundles = _jsonUtil.Deserialize<BundleManifest>(modBundlesJson);
        var bundleManifestArr = modBundles?.Manifest;

        foreach (var bundleManifest in bundleManifestArr)
        {
            var relativeModPath = modPath.Replace('\\', '/');

            var bundleLocalPath = Path.Join(relativeModPath, "bundles", bundleManifest.Key).Replace('\\', '/');

            if (!_bundleHashCacheService.CalculateAndMatchHash(bundleLocalPath))
            {
                _bundleHashCacheService.CalculateAndStoreHash(bundleLocalPath);
            }

            var bundleHash = _bundleHashCacheService.GetStoredValue(bundleLocalPath);

            AddBundle(bundleManifest.Key, new BundleInfo(relativeModPath, bundleManifest, bundleHash));
        }
    }

    public void AddBundle(string key, BundleInfo bundle)
    {
        var success = _bundles.TryAdd(key, bundle);
        if (!success)
        {
            _logger.Error($"Unable to add bundle: {key}");
        }
    }
}

public record BundleManifest
{
    [JsonPropertyName("manifest")]
    public List<BundleManifestEntry> Manifest
    {
        get;
        set;
    }
}

public record BundleManifestEntry
{
    [JsonPropertyName("key")]
    public string Key
    {
        get;
        set;
    }

    [JsonPropertyName("dependencyKeys")]
    public List<string>? DependencyKeys
    {
        get;
        set;
    }
}
