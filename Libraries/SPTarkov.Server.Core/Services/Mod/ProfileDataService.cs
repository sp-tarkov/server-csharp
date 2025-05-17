using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Mod;

[Injectable(InjectionType.Singleton)]
public class ProfileDataService(ISptLogger<ProfileDataService> logger, FileUtil fileUtil, JsonUtil jsonUtil)
{
    protected const string ProfileDataFilepath = "user/profileData/";
    private readonly ConcurrentDictionary<string, object> _profileDataCache = new();

    public bool ProfileDataExists(string profileId, string modKey)
    {
        return fileUtil.FileExists($"{ProfileDataFilepath}{profileId}/{modKey}.json");
    }

    public T? GetProfileData<T>(string profileId, string modKey)
    {
        var profileDataKey = $"{profileId}:{modKey}";
        if (!_profileDataCache.TryGetValue(profileDataKey, out var value))
        {
            if (fileUtil.FileExists($"{ProfileDataFilepath}{profileId}/{modKey}.json"))
            {
                value = jsonUtil.Deserialize<T>(fileUtil.ReadFile($"{ProfileDataFilepath}{profileId}/{modKey}.json"));
                if (value != null)
                {
                    while (!_profileDataCache.TryAdd(profileDataKey, value))
                    {
                    }
                }
            }
            else
            {
                value = null;
            }
        }

        return (T?) value;
    }

    public void SaveProfileData<T>(string profileId, string modKey, T profileData)
    {
        ArgumentNullException.ThrowIfNull(profileData);

        var data = jsonUtil.Serialize(profileData, profileData.GetType());
        if (data == null)
        {
            throw new Exception("The profile data when serialized resulted in a null value");
        }

        while (!_profileDataCache.TryAdd($"{profileId}:{modKey}", data))
        {
        }

        fileUtil.WriteFile($"{ProfileDataFilepath}{profileId}/{modKey}.json", data);
    }
}
