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

        public IBundleManifestEntry Bundle
        {
            get;
        }

        public long Crc
        {
            get;
        }

        public List<string> Dependencies
        {
            get;
        }

        public BundleInfo(
            string modPath,
            IBundleManifestEntry bundle,
            long bundleHash)
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

        public BundleInfo GetBundle(string bundleKey)
        {
            return _cloner.Clone(_bundles[bundleKey]);
        }

        public void AddBundles(string modPath)
        {
            //TODO: Implement

            var modBundlesJson = _fileUtil.ReadFile(Path.Combine(modPath, "bundles.json"));
            var modBundles = _jsonUtil.Deserialize<IBundleManifest>(modBundlesJson);
            var bundleManifestArr = modBundles?.Manifest;

            foreach (var bundleManifest in bundleManifestArr)
            {
                //     modpath.slice(0, -1).replace(/\\/ g, "/");
                //    var bundleLocalPath = $"{modpath}bundles/${bundleManifest.key}".replace(/\\/g, "/");

                //    if (!_bundleHashCacheService.CalculateAndMatchHash(bundleLocalPath))
                //    {
                //        _bundleHashCacheService.CalculateAndStoreHash(bundleLocalPath);
                //    }

                //    var bundleHash = _bundleHashCacheService.GetStoredValue(bundleLocalPath);

                //    AddBundle(bundleManifest.key, new BundleInfo(relativeModPath, bundleManifest, bundleHash));
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

public interface IBundleManifest
{
    [JsonPropertyName("manifest")]
    public List<IBundleManifestEntry> Manifest { get; set; }
}

public interface IBundleManifestEntry
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
