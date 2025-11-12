using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable(InjectionType.Singleton)]
public class ProfileDataService(ISptLogger<ProfileDataService> logger, FileUtil fileUtil, JsonUtil jsonUtil)
{
    protected const string ProfileDataFilepath = "user/profileData/";
    private readonly ConcurrentDictionary<string, object> _profileDataCache = new();

    /// <summary>
    /// Check if a specfici mod file exists for a profile
    /// </summary>
    /// <param name="profileId">Profile to look up</param>
    /// <param name="modKey">Name of json file to look up</param>
    public bool ProfileDataExists(string profileId, string modKey)
    {
        return fileUtil.FileExists(Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json"));
    }

    public T? GetProfileData<T>(string profileId, string modKey)
    {
        var profileDataKey = GetCacheKey(profileId, modKey);
        if (!_profileDataCache.TryGetValue(profileDataKey, out var value))
        {
            if (ProfileDataExists(profileId, modKey))
            {
                value = jsonUtil.Deserialize<T>(fileUtil.ReadFile(Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json")));
                if (value != null)
                {
                    _profileDataCache[GetCacheKey(profileId, modKey)] = value;
                }
            }
            else
            {
                value = null;
            }
        }

        return (T?)value;
    }

    public void SaveProfileData<T>(string profileId, string modKey, T profileData)
    {
        ArgumentNullException.ThrowIfNull(profileData);

        var data =
            jsonUtil.Serialize(profileData, profileData.GetType(), true)
            ?? throw new Exception("The profile data when serialized resulted in a null value");

        _profileDataCache[GetCacheKey(profileId, modKey)] = profileData;

        fileUtil.WriteFile(Path.Combine(ProfileDataFilepath, profileId, $"{modKey}.json"), data);
    }

    /// <summary>
    /// Clear all data for a profile
    /// </summary>
    /// <param name="profileId">Id of profile to delete files for</param>
    public void ClearProfileData(string profileId)
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
    protected string GetCacheKey(string profileId, string modKey)
    {
        return $"{profileId}:{modKey}";
    }
}
