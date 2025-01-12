using Core.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.InRaid;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class InRaidController
{
    protected ILogger _logger;
    protected SaveServer _saveServer;
    protected ProfileHelper _profileHelper;
    protected LocalisationService _localisationService;
    protected ApplicationContext _applicationContext;
    protected ConfigServer _configServer;

    protected InRaidConfig _inRaidConfig;
    protected BotConfig _botConfig;

    public InRaidController
    (
        ILogger logger,
        SaveServer saveServer,
        ProfileHelper profileHelper,
        LocalisationService localisationService,
        ApplicationContext applicationContext,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _saveServer = saveServer;
        _profileHelper = profileHelper;
        _localisationService = localisationService;
        _applicationContext = applicationContext;
        _configServer = configServer;
        _inRaidConfig = configServer.GetConfig<InRaidConfig>(ConfigTypes.IN_RAID);
        _botConfig = configServer.GetConfig<BotConfig>(ConfigTypes.BOT);
    }

    /// <summary>
    /// Save locationId to active profiles in-raid object AND app context
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="info">Register player request</param>
    public void AddPlayer(
        string sessionId,
        RegisterPlayerRequestData info)
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
    public void SavePostRaidProfileForScav(
        ScavSaveRequestData offRaidProfileData,
        string sessionId)
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
    public float GetTraitorScavHostileChance(
        string url,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public List<string> GetBossConvertSettings(
        string url,
        string sessionId)
    {
        return _botConfig.AssaultToBossConversion.BossesToConvertToWeights.Keys.ToList();
    }
}
