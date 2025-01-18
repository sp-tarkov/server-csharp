using Core.Annotations;
using Core.Models.Eft.Match;

namespace Core.Controllers;

[Injectable]
public class MatchController(
    
)
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GetEnabled()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/group/delete
    /// </summary>
    /// <param name="info"></param>
    public void DeleteGroup(object info) // TODO: info is `any` in the node server
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle match/group/start_game
    /// </summary>
    /// <param name="info"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ProfileStatusResponse JoinMatch(
        MatchGroupStartGameRequest info,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/group/status
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public MatchGroupStatusResponse GetGroupStatus(
        MatchGroupStatusRequest info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /client/raid/configuration
    /// </summary>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    public void ConfigureOfflineRaid(
        GetRaidConfigurationRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a difficulty value from pre-raid screen to a bot difficulty
    /// </summary>
    /// <param name="botDifficulty">dropdown difficulty value</param>
    /// <returns>bot difficulty</returns>
    private string ConvertDifficultyDropdownIntoBotDifficulty(
        string botDifficulty)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/local/start
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public StartLocalRaidResponseData StartLocalRaid(
        string sessionId,
        StartLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/local/end
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    public void EndLocalRaid(
        string sessionId,
        EndLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }
}
