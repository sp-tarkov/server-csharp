using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Utils;
using Core.Utils;


namespace Core.Controllers;

[Injectable]
public class QuestController(
    ISptLogger<QuestController> _logger,
    TimeUtil _timeUtil,
    HttpResponseUtil _httpResponseUtil,
    QuestHelper _questHelper,
    QuestRewardHelper _questRewardHelper
)
{
    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AcceptRepeatableQuest(PmcData pmcData, AcceptQuestRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public List<Quest> GetClientQuest(string sessionId)
    {
        return _questHelper.GetClientQuests(sessionId);
    }
}
