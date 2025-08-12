using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using static SPTarkov.Server.Core.Services.MatchLocationService;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class MatchController(
    MatchLocationService matchLocationService,
    ConfigServer configServer,
    LocationLifecycleService locationLifecycleService,
    ProfileActivityService profileActivityService,
    WeatherHelper weatherHelper
)
{
    protected readonly MatchConfig _matchConfig = configServer.GetConfig<MatchConfig>();
    protected readonly PmcConfig _pmcConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Handle client/match/available
    /// </summary>
    /// <returns>True if server should be available</returns>
    public bool GetEnabled()
    {
        return _matchConfig.Enabled;
    }

    /// <summary>
    ///     Handle client/match/group/delete
    /// </summary>
    /// <param name="request">Delete group request</param>
    public void DeleteGroup(DeleteGroupRequest request)
    {
        matchLocationService.DeleteGroup(request);
    }

    /// <summary>
    ///     Handle match/group/start_game
    /// </summary>
    /// <param name="request">Start game request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ProfileStatusResponse</returns>
    public ProfileStatusResponse JoinMatch(MatchGroupJoinRequest request, MongoId sessionId)
    {
        var output = new ProfileStatusResponse
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
                    AdditionalInfo = null,
                },
            ],
        };

        return output;
    }

    /// <summary>
    ///     Handle client/match/group/status
    /// </summary>
    /// <param name="request">Group status request</param>
    /// <returns>MatchGroupStatusResponse</returns>
    public MatchGroupStatusResponse GetGroupStatus(MatchGroupStatusRequest request)
    {
        return new MatchGroupStatusResponse { Players = [], MaxPveCountExceeded = false };
    }

    /// <summary>
    ///     Handle /client/raid/configuration
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    public void ConfigureOfflineRaid(GetRaidConfigurationRequestData request, MongoId sessionId)
    {
        // set IsNightRaid to use it later for bot inventory generation
        request.IsNightRaid = weatherHelper.IsNightTime(request.TimeVariant, request.Location);

        // Store request data for access during bot generation
        profileActivityService.GetProfileActivityRaidData(sessionId).RaidConfiguration = request;

        // TODO: add code to strip PMC of equipment now they've started the raid

        // Set pmcs to difficulty set in pre-raid screen if override in bot config isnt enabled
        if (!_pmcConfig.UseDifficultyOverride)
        {
            _pmcConfig.Difficulty = ConvertDifficultyDropdownIntoBotDifficulty(request.WavesSettings.BotDifficulty.ToString());
        }
    }

    /// <summary>
    ///     Convert a difficulty value from pre-raid screen to a bot difficulty
    /// </summary>
    /// <param name="botDifficulty">dropdown difficulty value</param>
    /// <returns>Bot difficulty</returns>
    protected string ConvertDifficultyDropdownIntoBotDifficulty(string botDifficulty)
    {
        // Edge case medium - must be altered
        if (string.Equals(botDifficulty, "medium", StringComparison.OrdinalIgnoreCase))
        {
            return "normal";
        }

        return botDifficulty;
    }

    /// <summary>
    ///     Handle client/match/local/start
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request">Start raid request</param>
    /// <returns>StartLocalRaidResponseData</returns>
    public StartLocalRaidResponseData StartLocalRaid(MongoId sessionId, StartLocalRaidRequestData request)
    {
        return locationLifecycleService.StartLocalRaid(sessionId, request);
    }

    /// <summary>
    ///     Handle client/match/local/end
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request">Emd local raid request</param>
    public void EndLocalRaid(MongoId sessionId, EndLocalRaidRequestData request)
    {
        locationLifecycleService.EndLocalRaid(sessionId, request);
    }
}
