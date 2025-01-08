using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;

namespace Core.Callbacks;

public class QuestCallbacks
{
    public QuestCallbacks()
    {
    }

    /// <summary>
    /// Handle RepeatableQuestChange event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle QuestAccept event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle QuestComplete event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle QuestHandover event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/quest/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<List<Quest>> ListQuests(string url, ListQuestsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/repeatalbeQuests/activityPeriods
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<List<PmcDataRepeatableQuest>> ActivityPeriods(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}