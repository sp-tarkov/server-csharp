using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Modding;

[Injectable(InjectionType.Singleton)]
public sealed class ProfileDataService(FileUtil fileUtil, JsonUtil jsonUtil)
{
    private const string ProfileDataFilepath = "user/profileData/";
    private readonly ConcurrentDictionary<string, object> _profileDataCache = new();

    /// <summary>
    /// Check if a specific mod file exists for a profile
    /// </summary>
    /// <param name="profileId">Profile to look up</param>
    /// <param name="modKey">Name of json file to look up</param>
    public bool ProfileDataExists(string profileId, string modKey)
    {
        return fileUtil.FileExists(Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json"));
    }

    /// <summary>
    /// Gets all custom mod data assigned to a profile
    /// </summary>
    /// <param name="profileId">The profile to look up</param>
    /// <returns>A dictionary containing all of the profile data</returns>
    public async Task<Dictionary<string, object>> GetAllProfileDataAsync(MongoId profileId, CancellationToken cancellationToken = default)
    {
        var profileDataPath = Path.Combine(ProfileDataFilepath, profileId.ToString());
        var profileData = new Dictionary<string, object>();

        if (!fileUtil.DirectoryExists(profileDataPath))
        {
            return profileData;
        }

        var profileFiles = fileUtil.GetFiles(profileDataPath);

        foreach (var filePath in profileFiles)
        {
            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var modKey = Path.GetFileNameWithoutExtension(filePath);

            if (string.IsNullOrWhiteSpace(modKey))
            {
                continue;
            }

            var cacheKey = GetCacheKey(profileId, modKey);

            if (_profileDataCache.TryGetValue(cacheKey, out var cachedValue))
            {
                profileData[modKey] = cachedValue;
                continue;
            }

            var deserializedData = await jsonUtil.DeserializeFromFileAsync<object>(filePath, cancellationToken);

            if (deserializedData == null)
            {
                continue;
            }

            profileData[modKey] = deserializedData;
        }

        return profileData;
    }

    public async Task<T?> GetProfileDataAsync<T>(MongoId profileId, string modKey, CancellationToken cancellationToken = default)
    {
        var profileDataKey = GetCacheKey(profileId, modKey);
        if (!_profileDataCache.TryGetValue(profileDataKey, out var value))
        {
            if (ProfileDataExists(profileId, modKey))
            {
                value = await jsonUtil.DeserializeFromFileAsync<T>(
                    Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json"),
                    cancellationToken
                );

                if (value != null)
                {
                    _profileDataCache[profileDataKey] = value;
                }
            }
            else
            {
                value = null;
            }
        }

        return (T?)value;
    }

    public async Task SaveProfileDataAsync<T>(
        MongoId profileId,
        string modKey,
        T profileData,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(profileData);

        var data =
            jsonUtil.Serialize(profileData, profileData.GetType(), true)
            ?? throw new Exception("The profile data when serialized resulted in a null value");

        _profileDataCache[GetCacheKey(profileId, modKey)] = profileData;

        await fileUtil.WriteFileAsync(Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json"), data, cancellationToken);
    }

    /// <summary>
    /// Clear all data for a profile
    /// </summary>
    /// <param name="profileId">Id of profile to delete files for</param>
    public void ClearProfileData(MongoId profileId)
    {
        if (!fileUtil.DirectoryExists(Path.Combine(ProfileDataFilepath, profileId)))
        {
            return;
        }

        var profileFiles = fileUtil.GetFiles(Path.Combine(ProfileDataFilepath, profileId));
        foreach (var filepPath in profileFiles)
        {
            fileUtil.DeleteFile(filepPath);
        }

        var keysInCacheToRemove = _profileDataCache.Keys.Where(key => key.StartsWith($"{profileId}:")).ToList(); // ToList so we can iterate over results without modifying collection

        foreach (var key in keysInCacheToRemove)
        {
            _profileDataCache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Get the cache key in specific format
    /// </summary>
    private string GetCacheKey(MongoId profileId, string modKey)
    {
        return $"{profileId}:{modKey}";
    }
}
