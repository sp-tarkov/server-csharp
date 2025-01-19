using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Launcher;
using Core.Models.Spt.Config;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Extensions;
using Info = Core.Models.Eft.Profile.Info;

namespace Core.Controllers;

[Injectable]
public class LauncherV2Controller(
    ISptLogger<LauncherV2Controller> _logger,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    SaveServer _saveServer,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    Watermark _watermark
)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    /// <summary>
    /// Returns a simple string of Pong!
    /// </summary>
    /// <returns></returns>
    public string Ping()
    {
        return "Pong!";
    }

    /// <summary>
    /// Returns all available profile types and descriptions for creation.
    /// - This is also localised.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> Types()
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
    /// Checks if login details were correct.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Login(LoginRequestData info)
    {
        var sessionId = GetSessionId(info);

        return sessionId is not null;
    }

    /// <summary>
    /// Register a new profile.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Register(RegisterData info)
    {
        foreach (var session in _saveServer.GetProfiles())
        {
            if (info.Username == _saveServer.GetProfile(session.Key).ProfileInfo!.Username)
            {
                return false;
            }
        }

        CreateAccount(info);
        return true;
    }

    /// <summary>
    /// Make a password change.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool PasswordChange(ChangeRequestData info)
    {
        var sessionId = GetSessionId(info);

        if (sessionId is null)
            return false;
        
        _saveServer.GetProfile(sessionId).ProfileInfo!.Password = info.Password;
        return true;
    }

    /// <summary>
    /// Remove profile from server.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Remove(LoginRequestData info)
    {
        var sessionId = GetSessionId(info);
        
        return sessionId is not null && _saveServer.RemoveProfile(sessionId);
    }

    /// <summary>
    /// Gets the Servers SPT Version.
    /// - "4.0.0"
    /// </summary>
    /// <returns></returns>
    public string SptVersion()
    {
        return _watermark.GetVersionTag();
    }

    /// <summary>
    /// Gets the compatible EFT Version.
    /// - "0.14.9.31124"
    /// </summary>
    /// <returns></returns>
    public string EftVersion()
    {
        return _coreConfig.CompatibleTarkovVersion;
    }

    /// <summary>
    /// Gets the Servers loaded mods.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, PackageJsonData> LoadedMods()
    {
        return new Dictionary<string, PackageJsonData>();
    }

    /// <summary>
    /// Creates the account from provided details.
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

    protected string? GetSessionId(LoginRequestData info)
    {
        foreach (var profile in _saveServer.GetProfiles())
        {
            if (info.Username == profile.Value.ProfileInfo!.Username 
                && info.Password == profile.Value.ProfileInfo.Password)
                return profile.Key;
        }

        return null;
    }
}
