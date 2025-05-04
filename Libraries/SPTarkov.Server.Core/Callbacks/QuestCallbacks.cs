using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class QuestCallbacks(
    HttpResponseUtil _httpResponseUtil,
    QuestController _questController,
    RepeatableQuestController _repeatableQuestController
)
{
    /// <summary>
    ///     Handle RepeatableQuestChange event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeRepeatableQuest(
        PmcData pmcData,
        RepeatableQuestChangeRequest info,
        string sessionID
    )
    {
        return _repeatableQuestController.ChangeRepeatableQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestAccept event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse AcceptQuest(
        PmcData pmcData,
        AcceptQuestRequestData info,
        string sessionID
    )
    {
        if (info.Type == "repeatable")
        {
            return _questController.AcceptRepeatableQuest(pmcData, info, sessionID);
        }

        return _questController.AcceptQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestComplete event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse CompleteQuest(
        PmcData pmcData,
        CompleteQuestRequestData info,
        string sessionID
    )
    {
        return _questController.CompleteQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestHandover event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HandoverQuest(
        PmcData pmcData,
        HandoverQuestRequestData info,
        string sessionID
    )
    {
        return _questController.HandoverQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle client/quest/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string ListQuests(string url, ListQuestsRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_questController.GetClientQuests(sessionID));
    }

    /// <summary>
    ///     Handle client/repeatalbeQuests/activityPeriods
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string ActivityPeriods(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(
            _repeatableQuestController.GetClientRepeatableQuests(sessionID)
        );
    }
}
