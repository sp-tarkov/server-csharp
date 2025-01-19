using SptCommon.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.InRaid;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;


namespace Core.Controllers;

[Injectable]
public class InRaidController(
    ISptLogger<InRaidController> _logger,
    SaveServer _saveServer,
    ProfileHelper _profileHelper,
    LocalisationService _localisationService,
    ApplicationContext _applicationContext,
    ConfigServer _configServer
)
{
    protected InRaidConfig _inRaidConfig = _configServer.GetConfig<InRaidConfig>();
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

    /// <summary>
    /// Save locationId to active profiles in-raid object AND app context
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="info">Register player request</param>
    public void AddPlayer(string sessionId, RegisterPlayerRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle raid/profile/scavsave
    /// Save profile state to disk
    /// Handles pmc/pscav
    /// </summary>
    /// <param name="offRaidProfileData"></param>
    /// <param name="sessionId"></param>
    public void SavePostRaidProfileForScav(ScavSaveRequestData offRaidProfileData, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the inraid config from configs/inraid.json
    /// </summary>
    public InRaidConfig GetInRaidConfig()
    {
        return _inRaidConfig;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public double GetTraitorScavHostileChance(string url, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public List<string> GetBossConvertSettings(string url, string sessionId)
    {
        return _botConfig.AssaultToBossConversion.BossesToConvertToWeights.Keys.ToList();
    }
}
