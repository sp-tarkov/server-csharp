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
    
    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AcceptQuest(PmcData pmcData, AcceptQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CompleteQuest(PmcData pmcData, CompleteQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HandoverQuest(PmcData pmcData, HandoverQuestRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<Quest>> ListQuests(string url, ListQuestsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<PmcDataRepeatableQuest>> ActivityPeriods(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}