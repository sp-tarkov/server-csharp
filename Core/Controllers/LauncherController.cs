using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Mod;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Extensions;
using ILogger = Core.Models.Utils.ILogger;
using Info = Core.Models.Eft.Profile.Info;

namespace Core.Controllers;

[Injectable]
public class LauncherController
{
 protected CoreConfig _coreConfig;

 protected ILogger _logger;
 protected HashUtil _hashUtil;
 protected TimeUtil _timeUtil;
 protected RandomUtil _randomUtil;
 protected SaveServer _saveServer;
 protected HttpServerHelper _httpServerHelper;
 protected ProfileHelper _profileHelper;
 protected DatabaseService _databaseService;
 protected LocalisationService _localisationService;
 

 public LauncherController(
        Models.Utils.ILogger logger,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        RandomUtil randomUtil,
        SaveServer saveServer,
        HttpServerHelper httpServerHelper,
        ProfileHelper profileHelper,
        DatabaseService databaseService,
        LocalisationService localisationService,
        // TODO => PreSptModLoader preSptModLoader,
        ConfigServer configServer
    ) {
     _logger = logger;
     _hashUtil = hashUtil;
     _timeUtil = timeUtil;
     _randomUtil = randomUtil;
     _saveServer = saveServer;
     _httpServerHelper = httpServerHelper;
     _profileHelper = profileHelper;
     _databaseService = databaseService;
     _localisationService = localisationService;
        _coreConfig = configServer.GetConfig<CoreConfig>(ConfigTypes.CORE);
    }

    public ConnectResponse Connect()
    {
        // Get all possible profile types + filter out any that are blacklisted

        var profiles = typeof(ProfileTemplates).GetProperties()
            .Select(p => p.GetJsonName())
            .Where(profileName => !_coreConfig.Features.CreateNewProfileTypesBlacklist.Contains(profileName))
            .ToList();

        return new ConnectResponse(){
            BackendUrl = _httpServerHelper.GetBackendUrl(),
            Name = _coreConfig.ServerName,
            Editions = profiles,
            ProfileDescriptions = GetProfileDescriptions(),
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
        foreach (var templatesProperty in typeof(ProfileTemplates).GetProperties().Where(p => p.CanWrite == true))
        {
            var propertyValue = templatesProperty.GetValue(dbProfiles);
            if (propertyValue == null) {
                _logger.Warning(_localisationService.GetText("launcher-missing_property", templatesProperty));
                continue;
            }

            var casterPropertyValue = propertyValue as ProfileSides;
            result[templatesProperty.GetJsonName()] = _localisationService.GetText(casterPropertyValue.DescriptionLocaleKey);
        }

        return result;
    }

    public Info? Find(string sessionId)
    {
        return _saveServer.GetProfiles().TryGetValue(sessionId, out var profile) ? profile.ProfileInfo : null;
    }

    public string? Login(LoginRequestData info)
    {
        foreach (var sessionID in _saveServer.GetProfiles()) {
            var account = _saveServer.GetProfile(sessionID.Key).ProfileInfo;
            if (info.Username == account.Username) {
                return sessionID.Key;
            }
        }

        return null;
    }

    public string Register(RegisterData info)
    {
        foreach (var sessionID in _saveServer.GetProfiles()) {
            if (info.Username == _saveServer.GetProfile(sessionID.Key).ProfileInfo.Username) {
                return "";
            }
        }

        return CreateAccount(info);
    }

    protected string CreateAccount(RegisterData info)
    {
        var profileId = GenerateProfileId();
        var scavId = GenerateProfileId();
        var newProfileDetails = new Info(){
            ProfileId = profileId,
            ScavengerId = scavId,
            Aid = _hashUtil.GenerateAccountId(),
            Username = info.Username,
            Password = info.Password,
            IsWiped = true,
            Edition = info.Edition,
        };
        _saveServer.CreateProfile(newProfileDetails);

        _saveServer.LoadProfile(profileId);
        _saveServer.SaveProfile(profileId);

        return profileId;
    }

    protected string GenerateProfileId()
    {
        var timestamp = _timeUtil.GetTimeStamp();

        return FormatID(timestamp, timestamp * _randomUtil.GetInt(1, 1000000));
    }

    protected string FormatID(long timeStamp, long counter)
    {
        
        var timeStampStr = Convert.ToString(timeStamp, 16).PadLeft(8, '0');
        var counterStr = Convert.ToString(counter, 16).PadLeft(16, '0');

        return timeStampStr.ToLower() + counterStr.ToLower();
    }

    public string ChangeUsername(ChangeRequestData info)
    {
        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID)) {
            _saveServer.GetProfile(sessionID).ProfileInfo.Username = info.Change;
        }

        return sessionID;
    }

    public string ChangePassword(ChangeRequestData info)
    {
        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID)) {
            _saveServer.GetProfile(sessionID).ProfileInfo.Password = info.Change;
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
        if (!_coreConfig.AllowProfileWipe) {
            return null;
        }

        var sessionID = Login(info);

        if (!string.IsNullOrEmpty(sessionID)) {
            var profileInfo = _saveServer.GetProfile(sessionID).ProfileInfo;
            profileInfo.Edition = info.Edition;
            profileInfo.IsWiped = true;
        }

        return sessionID;
    }

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
        _logger.Error("NOT IMPLEMENTED - _preSptModLoader GetLoadedServerMods()");
        return new Dictionary<string, PackageJsonData>();
        // TODO => return this.preSptModLoader.getImportedModDetails();
    }

    /**
     * Get the mods a profile has ever loaded into game with
     * @param sessionId Player id
     * @returns Array of mod details
     */
    public List<ModDetails> GetServerModsProfileUsed(string sessionId)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);

        _logger.Error("NOT IMPLEMENTED - _preSptModLoader GetServerModsProfileUsed()");
        /* TODO => modding
        if (profile?.spt?.mods) {
            return this.preSptModLoader.GetProfileModsGroupedByModName(profile?.spt?.mods);
        }
        */

        return [];
    }
}
