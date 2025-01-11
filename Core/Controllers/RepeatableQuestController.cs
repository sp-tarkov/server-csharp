using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;

namespace Core.Controllers;

public class RepeatableQuestController
{
    // TODO
    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public List<PmcDataRepeatableQuest> GetClientRepeatableQuests(string sessionId)
    {
        throw new NotImplementedException();
    }
}
