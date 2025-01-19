using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Match;
using Core.Services;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class MatchCallbacks(
    HttpResponseUtil _httpResponseUtil,
    MatchController _matchController,
    DatabaseService _databaseService
)
{
    /// <summary>
    /// Handle client/match/updatePing
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string UpdatePing(string url, UpdatePingRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/exit
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ExitMatch(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/group/exit_from_menu
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ExitFromMenu(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GroupCurrent(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(new MatchGroupCurrentResponse { Squad = [] });
    }

    /// <summary>
    /// Handle client/match/group/looking/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string StartGroupSearch(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/group/looking/stop
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string StopGroupSearch(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/group/invite/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SendGroupInvite(string url, MatchGroupInviteSendRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody("2427943f23698ay9f2863735");
    }

    /// <summary>
    /// Handle client/match/group/invite/accept
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string AcceptGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(new List<GroupCharacter>() { new GroupCharacter() });
    }

    /// <summary>
    /// Handle client/match/group/invite/decline
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string DeclineGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/invite/cancel
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string CancelGroupInvite(string url, RequestIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/transfer
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string TransferGroup(string url, MatchGroupTransferRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/invite/cancel-all
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string CancelAllGroupInvite(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/putMetrics
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string PutMetrics(string url, PutMetricsRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/analytics/event-disconnect
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string EventDisconnect(string url, PutMetricsRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/available
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ServerAvailable(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.GetEnabled());
    }

    /// <summary>
    /// Handle match/group/start_game
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string JoinMatch(string url, MatchGroupStartGameRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.JoinMatch(info, sessionID));
    }

    /// <summary>
    /// Handle client/getMetricsConfig
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMetrics(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_databaseService.GetMatch().Metrics);
    }

    /// <summary>
    /// Called periodically while in a group
    /// Handle client/match/group/status
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetGroupStatus(string url, MatchGroupStatusRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.GetGroupStatus(info));
    }

    /// <summary>
    /// Handle client/match/group/delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string DeleteGroup(string url, EmptyRequestData info, string sessionID)
    {
        _matchController.DeleteGroup(info);
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/leave
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string LeaveGroup(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/player/remove
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string RemovePlayerFromGroup(string url, MatchGroupPlayerRemoveRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/local/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string StartLocalRaid(string url, StartLocalRaidRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_matchController.StartLocalRaid(sessionID, info));
    }

    /// <summary>
    /// Handle client/match/local/end
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string EndLocalRaid(string url, EndLocalRaidRequestData info, string sessionID)
    {
        _matchController.EndLocalRaid(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/raid/configuration
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetRaidConfiguration(string url, GetRaidConfigurationRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/raid/configuration-by-profile
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetConfigurationByProfile(string url, GetRaidConfigurationRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    /// Handle client/match/group/raid/ready
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string RaidReady(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }

    /// <summary>
    /// Handle client/match/group/raid/not-ready
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string NotRaidReady(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(true);
    }
}
