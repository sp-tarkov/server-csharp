using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Config;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;
using SptCommon.Extensions;
using Info = Core.Models.Eft.Profile.Info;

namespace Core.Controllers;

[Injectable]
public class LauncherController(
    ISptLogger<LauncherController> _logger,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    SaveServer _saveServer,
    HttpServerHelper _httpServerHelper,
    ProfileHelper _profileHelper,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ApplicationContext _applicationContext
)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ConnectResponse Connect()
    {
        // Get all possible profile types + filter out any that are blacklisted

        var profiles = typeof(ProfileTemplates).GetProperties()
            .Where(p => p.CanWrite)
            .Select(p => p.GetJsonName())
            .Where(profileName => !_coreConfig.Features.CreateNewProfileTypesBlacklist.Contains(profileName))
            .ToList();

        return new ConnectResponse
        {
            BackendUrl = _httpServerHelper.GetBackendUrl(),
            Name = _coreConfig.ServerName,
            Editions = profiles,
            ProfileDescriptions = GetProfileDescriptions()
        };
    }

    /**
     * Get descriptive text for each of the profile edtions a player can choose, keyed by profile.json profile type e.g. "Edge Of Darkness"
     * @returns Dictionary of profile types with related descriptive text
     */
    protected Dictionary<string, string> GetProfileDescriptions()
    {
        var result = new Dictionary<string, string>();
        var dbProfiles = _databaseService.GetProfiles();
        foreach (var templatesProperty in typeof(ProfileTemplates).GetProperties().Where(p => p.CanWrite))
        {
            var propertyValue = templatesProperty.GetValue(dbProfiles);
            if (propertyValue == null)
            {
                _logger.Warning(_localisationService.GetText("launcher-missing_property", templatesProperty));
                continue;
            }

            var casterPropertyValue = propertyValue as ProfileSides;
            result[templatesProperty.GetJsonName()] = _localisationService.GetText(casterPropertyValue?.DescriptionLocaleKey!);
        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Info? Find(string? sessionId)
    {
        return sessionId is not null && _saveServer.GetProfiles().TryGetValue(sessionId, out var profile) ? profile.ProfileInfo : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string? Login(LoginRequestData? info)
    {
        foreach (var kvp in _saveServer.GetProfiles())
        {
            var account = _saveServer.GetProfile(kvp.Key).ProfileInfo;
            if (info?.Username == account?.Username)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string Register(RegisterData info)
    {
        foreach (var kvp in _saveServer.GetProfiles())
        {
            if (info.Username == _saveServer.GetProfile(kvp.Key).ProfileInfo?.Username)
            {
                return "";
            }
        }

        return CreateAccount(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    protected string CreateAccount(RegisterData info)
    {
        var profileId = GenerateProfileId();
        var scavId = GenerateProfileId();
        var newProfileDetails = new Info
        {
            ProfileId = profileId,
            ScavengerId = scavId,
            Aid = _hashUtil.GenerateAccountId(),
            Username = info.Username,
            Password = info.Password,
            IsWiped = true,
            Edition = info.Edition
        };
        _saveServer.CreateProfile(newProfileDetails);

        _saveServer.LoadProfile(profileId);
        _saveServer.SaveProfile(profileId);

        return profileId;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected string GenerateProfileId()
    {
        var timestamp = _timeUtil.GetTimeStamp();

        return FormatID(timestamp, timestamp * _randomUtil.GetInt(1, 1000000));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="counter"></param>
    /// <returns></returns>
    protected string FormatID(long timeStamp, long counter)
    {
        var timeStampStr = Convert.ToString(timeStamp, 16).PadLeft(8, '0');
        var counterStr = Convert.ToString(counter, 16).PadLeft(16, '0');

        return timeStampStr.ToLower() + counterStr.ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string? ChangeUsername(ChangeRequestData info)
    {
        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID))
        {
            _saveServer.GetProfile(sessionID).ProfileInfo!.Username = info.Change;
        }

        return sessionID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public string? ChangePassword(ChangeRequestData info)
    {
        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID))
        {
            _saveServer.GetProfile(sessionID).ProfileInfo!.Password = info.Change;
        }

        return sessionID;
    }

    /**
     * Handle launcher requesting profile be wiped
     * @param info IRegisterData
     * @returns Session id
     */
    public string? Wipe(RegisterData info)
    {
        if (!_coreConfig.AllowProfileWipe)
        {
            return null;
        }

        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID))
        {
            var profileInfo = _saveServer.GetProfile(sessionID).ProfileInfo;
            profileInfo!.Edition = info.Edition;
            profileInfo.IsWiped = true;
        }

        return sessionID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetCompatibleTarkovVersion()
    {
        return _coreConfig.CompatibleTarkovVersion;
    }

    /**
     * Get the mods the server has currently loaded
     * @returns Dictionary of mod name and mod details
     */
    public Dictionary<string, PackageJsonData> GetLoadedServerMods()
    {
        var mods = _applicationContext?.GetLatestValue(ContextVariableType.LOADED_MOD_ASSEMBLIES).GetValue<List<SptMod>>();
        var result = new Dictionary<string, PackageJsonData>();

        foreach (var sptMod in mods)
        {
            result.Add(sptMod.PackageJson.Name, sptMod.PackageJson);
        }

        return result;
    }

    /**
     * Get the mods a profile has ever loaded into game with
     * @param sessionId Player id
     * @returns Array of mod details
     */
    public List<ModDetails> GetServerModsProfileUsed(string sessionId)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);

        if (profile?.SptData?.Mods is not null)
        {
            return GetProfileModsGroupedByModName(profile?.SptData?.Mods);
        }

        return [];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profileMods"></param>
    /// <returns></returns>
    public List<ModDetails> GetProfileModsGroupedByModName(List<ModDetails> profileMods)
    {
        // Group all mods used by profile by name
        var modsGroupedByName = new Dictionary<string, List<ModDetails>>();
        foreach (var mod in profileMods)
        {
            if (!modsGroupedByName.ContainsKey(mod.Name))
            {
                modsGroupedByName[mod.Name] = [];
            }

            modsGroupedByName[mod.Name].Add(mod);
        }

        // Find the highest versioned mod and add to results array
        var result = new List<ModDetails>();
        foreach (var modName in modsGroupedByName)
        {
            var modDatas = modsGroupedByName[modName.Key];
            var modVersions = modDatas.Select(x => x.Version);
            // var highestVersion = MaxSatisfying(modVersions, "*"); ?? TODO: Node used SemVer here

            var chosenVersion = modDatas.FirstOrDefault(x => x.Name == modName.Key); // && x.Version == highestVersion
            if (chosenVersion is null)
            {
                continue;
            }

            result.Add(chosenVersion);
        }

        return result;
    }
}
