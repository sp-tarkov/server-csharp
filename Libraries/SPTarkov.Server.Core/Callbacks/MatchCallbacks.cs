using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using static SPTarkov.Server.Core.Services.MatchLocationService;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class MatchCallbacks(
    HttpResponseUtil _httpResponseUtil,
    MatchController _matchController,
    DatabaseService _databaseService
)
{
    /// <summary>
    ///     Handle client/match/updatePing
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string UpdatePing(string url, UpdatePingRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/exit
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string ExitMatch(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/group/exit_from_menu
    /// </summary>
    /// <returns></returns>
    public string ExitFromMenu(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/group/current
    /// </summary>
    /// <returns></returns>
    public string GroupCurrent(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(new MatchGroupCurrentResponse { Squad = [] });
    }

    /// <summary>
    ///     Handle client/match/group/looking/start
    /// </summary>
    /// <returns></returns>
    public string StartGroupSearch(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/group/looking/stop
    /// </summary>
    /// <returns></returns>
    public string StopGroupSearch(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/group/invite/send
    /// </summary>
    /// <returns></returns>
    public string SendGroupInvite(string url, MatchGroupInviteSendRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody("2427943f23698ay9f2863735");
    }

    /// <summary>
    ///     Handle client/match/group/invite/accept
    /// </summary>
    /// <returns></returns>
    public string AcceptGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(new List<GroupCharacter> { new() });
    }

    /// <summary>
    ///     Handle client/match/group/invite/decline
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string DeclineGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/invite/cancel
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string CancelGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/transfer
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string TransferGroup(string url, MatchGroupTransferRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/invite/cancel-all
    /// </summary>
    /// <returns></returns>
    public string CancelAllGroupInvite(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/putMetrics
    /// </summary>
    /// <returns></returns>
    public string PutMetrics(string url, PutMetricsRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/analytics/event-disconnect
    /// </summary>
    /// <returns></returns>
    public string EventDisconnect(string url, PutMetricsRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/available
    /// </summary>
    /// <returns></returns>
    public string ServerAvailable(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.GetEnabled());
    }

    /// <summary>
    ///     Handle match/group/start_game
    /// </summary>
    /// <returns></returns>
    public string JoinMatch(string url, MatchGroupStartGameRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.JoinMatch(info, sessionID));
    }

    /// <summary>
    ///     Handle client/getMetricsConfig
    /// </summary>
    /// <returns></returns>
    public string GetMetrics(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetMatch().Metrics);
    }

    /// <summary>
    ///     Called periodically while in a group
    ///     Handle client/match/group/status
    /// </summary>
    /// <returns></returns>
    public string GetGroupStatus(string url, MatchGroupStatusRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.GetGroupStatus(info));
    }

    /// <summary>
    ///     Handle client/match/group/delete
    /// </summary>
    /// <returns></returns>
    public string DeleteGroup(string url, DeleteGroupRequest info, string sessionID)
    {
        _matchController.DeleteGroup(info);
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/leave
    /// </summary>
    /// <returns></returns>
    public string LeaveGroup(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/player/remove
    /// </summary>
    /// <returns></returns>
    public string RemovePlayerFromGroup(
        string url,
        MatchGroupPlayerRemoveRequest info,
        string sessionID
    )
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/local/start
    /// </summary>
    /// <returns></returns>
    public string StartLocalRaid(string url, StartLocalRaidRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.StartLocalRaid(sessionID, info));
    }

    /// <summary>
    ///     Handle client/match/local/end
    /// </summary>
    /// <returns></returns>
    public string EndLocalRaid(string url, EndLocalRaidRequestData info, string sessionID)
    {
        _matchController.EndLocalRaid(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/raid/configuration
    /// </summary>
    /// <returns></returns>
    public string GetRaidConfiguration(
        string url,
        GetRaidConfigurationRequestData info,
        string sessionID
    )
    {
        _matchController.ConfigureOfflineRaid(info, sessionID);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/raid/configuration-by-profile
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetConfigurationByProfile(
        string url,
        GetRaidConfigurationRequestData info,
        string sessionID
    )
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/match/group/raid/ready
    /// </summary>
    /// <returns></returns>
    public string RaidReady(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    ///     Handle client/match/group/raid/not-ready
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string NotRaidReady(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }
}
