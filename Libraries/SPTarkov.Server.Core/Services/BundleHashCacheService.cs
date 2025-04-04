﻿using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services
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
}
