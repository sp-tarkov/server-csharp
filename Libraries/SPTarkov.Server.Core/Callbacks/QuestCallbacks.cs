using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class QuestCallbacks(
    HttpResponseUtil httpResponseUtil,
    QuestController questController,
    RepeatableQuestController repeatableQuestController
)
{
    /// <summary>
    ///     Handle RepeatableQuestChange event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest info, MongoId sessionID)
    {
        return repeatableQuestController.ChangeRepeatableQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestAccept event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData info, MongoId sessionID)
    {
        if (info.Type == "repeatable")
        {
            return repeatableQuestController.AcceptRepeatableQuest(pmcData, info, sessionID);
        }

        return questController.AcceptQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestComplete event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData info, MongoId sessionID)
    {
        return questController.CompleteQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle QuestHandover event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData info, MongoId sessionID)
    {
        return questController.HandoverQuest(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle client/quest/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> ListQuests(string url, ListQuestsRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(questController.GetClientQuests(sessionID)));
    }

    /// <summary>
    ///     Handle client/repeatalbeQuests/activityPeriods
    /// </summary>
    /// <param name="url"></param>
    /// <param name="_"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> ActivityPeriods(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(repeatableQuestController.GetClientRepeatableQuests(sessionID)));
    }
}
