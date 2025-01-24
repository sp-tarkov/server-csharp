using Core.Context;
using SptCommon.Annotations;
using Core.Models.Eft.Match;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;
using static Core.Services.MatchLocationService;

namespace Core.Controllers;

[Injectable]
public class MatchController(
    ISptLogger<MatchController> _logger,
    SaveServer _saveServer,
    MatchLocationService _matchLocationService,
    ConfigServer _configServer,
    ApplicationContext _applicationContext,
    LocationLifecycleService _locationLifecycleService,
    ICloner _cloner
)
{
    protected MatchConfig _matchConfig = _configServer.GetConfig<MatchConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GetEnabled()
    {
        return _matchConfig.Enabled;
    }

    /// <summary>
    /// Handle client/match/group/delete
    /// </summary>
    /// <param name="info"></param>
    public void DeleteGroup(DeleteGroupRequest info) // TODO: info is `any` in the node server
    {
        _matchLocationService.DeleteGroup(info);
    }

    /// <summary>
    /// Handle match/group/start_game
    /// </summary>
    /// <param name="info"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ProfileStatusResponse JoinMatch(MatchGroupStartGameRequest info, string sessionId)
    {
        ProfileStatusResponse output = new ProfileStatusResponse
        {
            MaxPveCountExceeded = false,
            // get list of players joining into the match
            Profiles =
            [
                new SessionStatus
                {
                    ProfileId = "TODO",
                    ProfileToken = "TODO",
                    Status = "MatchWait",
                    Sid = "",
                    Ip = "",
                    Port = 0,
                    Version = "live",
                    Location = "TODO get location",
                    RaidMode = "Online",
                    Mode = "deathmatch",
                    ShortId = null,
                    AdditionalInfo = null
                }
            ]
        };

        return output;
    }

    /// <summary>
    /// Handle client/match/group/status
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public MatchGroupStatusResponse GetGroupStatus(MatchGroupStatusRequest info)
    {
        return new MatchGroupStatusResponse()
        {
            Players = [],
            MaxPveCountExceeded = false
        };
    }

    /// <summary>
    /// Handle /client/raid/configuration
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    public void ConfigureOfflineRaid(GetRaidConfigurationRequestData request, string sessionId)
    {
        // Store request data for access during bot generation
        _applicationContext.AddValue(ContextVariableType.RAID_CONFIGURATION, request);

        // TODO: add code to strip PMC of equipment now they've started the raid

        // Set pmcs to difficulty set in pre-raid screen if override in bot config isnt enabled
        if (!_pmcConfig.UseDifficultyOverride)
        {
            _pmcConfig.Difficulty = ConvertDifficultyDropdownIntoBotDifficulty(
                request.WavesSettings.BotDifficulty.ToString()
            );
        }
    }

    /// <summary>
    /// Convert a difficulty value from pre-raid screen to a bot difficulty
    /// </summary>
    /// <param name="botDifficulty">dropdown difficulty value</param>
    /// <returns>bot difficulty</returns>
    private string ConvertDifficultyDropdownIntoBotDifficulty(string botDifficulty)
    {
        // Edge case medium - must be altered
        if (botDifficulty.ToLower() == "medium")
        {
            return "normal";
        }

        return botDifficulty;
    }

    /// <summary>
    /// Handle client/match/local/start
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public StartLocalRaidResponseData StartLocalRaid(string sessionId, StartLocalRaidRequestData request)
    {
        return _locationLifecycleService.StartLocalRaid(sessionId, request);
    }

    /// <summary>
    /// Handle client/match/local/end
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void EndLocalRaid(string sessionId, EndLocalRaidRequestData request)
    {
        _locationLifecycleService.EndLocalRaid(sessionId, request);
    }
}
