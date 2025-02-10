using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services
{
    [Injectable(InjectionType.Singleton)]
    public class BundleHashCacheService
    {
        private readonly ISptLogger<BundleHashCacheService> _logger;
        private readonly JsonUtil _jsonUtil;
        private readonly HashUtil _hashUtil;
        private readonly FileUtil _fileUtil;
        protected readonly Dictionary<string, string> _bundleHashes = new Dictionary<string, string>();
        protected const string _bundleHashCachePath = "./user/cache/bundleHashCache.json";

        public BundleHashCacheService(
            ISptLogger<BundleHashCacheService> logger,
            JsonUtil jsonUtil,
            HashUtil hashUtil,
            FileUtil fileUtil)
        {
            _logger = logger;
            _jsonUtil = jsonUtil;
            _hashUtil = hashUtil;
            _fileUtil = fileUtil;
        }
        public string GetStoredValue(string key)
        {
            if (!_bundleHashes.TryGetValue(key, out var value))
            {
                return string.Empty;
            }

            return value;
        }

        public void StoreValue(string bundlePath, string hash)
        {
            _bundleHashes.Add(bundlePath, hash);

            _fileUtil.WriteFile(_bundleHashCachePath, _jsonUtil.Serialize(_bundleHashes));

            _logger.Debug($"Bundle: {bundlePath} hash stored in: ${_bundleHashCachePath}");
        }

        public bool CalculateAndMatchHash(string bundlePath)
        {
            return MatchWithStoredHash(bundlePath, CalculateHash(bundlePath));
        }

        public void CalculateAndStoreHash(string bundlePath)
        {
            StoreValue(bundlePath, CalculateHash(bundlePath));
        }

        public string CalculateHash(string bundlePath)
        {
            var fileData = _fileUtil.ReadFile(bundlePath);
            return _hashUtil.GenerateMd5ForData(fileData);
        }

        public bool MatchWithStoredHash(string bundlePath, string hash)
        {
            return GetStoredValue(bundlePath) == hash;
        }
    }
}
