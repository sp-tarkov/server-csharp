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
        protected readonly Dictionary<string, uint> _bundleHashes = new Dictionary<string, uint>();
        protected const string _bundleHashCachePath = "./user/cache/";
        protected const string _cacheName = "bundleHashCache.json";
        private readonly string _currentDirectory = Directory.GetCurrentDirectory();

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

            _fileUtil.WriteFile(Path.Join(_bundleHashCachePath, _cacheName), _jsonUtil.Serialize(_bundleHashes));

            _logger.Debug($"Bundle: {bundlePath} hash stored in: ${_bundleHashCachePath}");
        }

        public bool CalculateAndMatchHash(string relativeBundlePath)
        {
            var absolutePath = Path.Join(_currentDirectory, relativeBundlePath);
            return MatchWithStoredHash(relativeBundlePath, CalculateHash(absolutePath));
        }

        public void CalculateAndStoreHash(string relativeBundlePath)
        {
            var absolutePath = Path.Join(_currentDirectory, relativeBundlePath);
            StoreValue(relativeBundlePath, CalculateHash(absolutePath));
        }

        public uint CalculateHash(string absoluteBundlePath)
        {
            var fileData = _fileUtil.ReadFile(absoluteBundlePath);
            return _hashUtil.GenerateCrc32ForData(fileData);
        }

        public bool MatchWithStoredHash(string relativeBundlePath, uint hash)
        {
            return GetStoredValue(relativeBundlePath) == hash;
        }
    }
}
