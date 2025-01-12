using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;

namespace Core.Controllers;

[Injectable]
public class QuestController
{
    private readonly QuestHelper _questHelper;

    public QuestController(
        QuestHelper questHelper)
    {
        _questHelper = questHelper;
    }
    // TODO
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
