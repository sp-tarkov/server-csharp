using System.Text.Json.Serialization;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Loaders
{
    public class BundleInfo
    {
        public string ModPath
        {
            get;
        }

        public BundleManifestEntry Bundle
        {
            get;
        }

        public string Crc
        {
            get;
        }

        public List<string> Dependencies
        {
            get;
        }

        public BundleInfo(
            string modPath,
            BundleManifestEntry bundle,
            string bundleHash)
        {
            ModPath = modPath;
            Bundle = bundle;
            Crc = bundleHash;
            Dependencies = bundle?.DependencyKeys ?? [];
        }
    }

    [Injectable(InjectionType.Singleton)]
    public class BundleLoader
    {
        private readonly ISptLogger<BundleLoader> _logger;
        private readonly HashUtil _hashUtil;
        private readonly JsonUtil _jsonUtil;
        private readonly FileUtil _fileUtil;
        private readonly BundleHashCacheService _bundleHashCacheService;
        private readonly InMemoryCacheService _inMemoryCacheService;
        private readonly ICloner _cloner;
        private readonly Dictionary<string, BundleInfo> _bundles;

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

        public List<BundleInfo> GetBundles()
        {
            var result = new List<BundleInfo>();

            foreach (var bundle in _bundles) {
                result.Add(bundle.Value);
            }

            return result;
        }

        public BundleInfo? GetBundle(string bundleKey)
        {
            return _bundles.GetValueOrDefault(bundleKey);
        }

        public void AddBundles(string modPath)
        {
            // TODO: Implement

            var modBundlesJson = _fileUtil.ReadFile(Path.Combine(modPath, "bundles.json"));
            var modBundles = _jsonUtil.Deserialize<BundleManifest>(modBundlesJson);
            var bundleManifestArr = modBundles?.Manifest;

            foreach (var bundleManifest in bundleManifestArr)
            {
                // TODO: complete
                // we currently get D:\HomeRepos\SPT-CS-Server\Server\bin\Debug\net9.0/user/mods/Mod3
                // we want /user/mods/Mod3
                var relativeModPath = modPath.Substring(0, modPath.Length - 1); // /\\/g, "/" - replaces all instances of \\ with /

                // we currently get D:\HomeRepos\SPT-CS-Server\Server\bin\Debug\net9.0/user/mods/Mod3/bundles\assets/content/weapons/usable_items/item_bottle/textures/client_assets.bundle
                // we want /user/mods/Mod3/bundles\assets/content/weapons/usable_items/item_bottle/textures/client_assets.bundle
                var bundleLocalPath = Path.Combine(modPath, "bundles", bundleManifest.Key); // /\\/g, "/" - replaces all instances of \\ with /

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
}

public record BundleManifest
{
    [JsonPropertyName("manifest")]
    public List<BundleManifestEntry> Manifest { get; set; }
}

public record BundleManifestEntry
{
    [JsonPropertyName("key")]
    public string Key {
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
