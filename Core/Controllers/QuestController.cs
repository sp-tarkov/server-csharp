using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Utils;
using Core.Utils;


namespace Core.Controllers;

[Injectable]
public class QuestController
{
    protected ISptLogger<QuestController> _logger;
    protected TimeUtil _timeUtil;
    protected HttpResponseUtil _httpResponseUtil;
    protected QuestHelper _questHelper;
    protected QuestRewardHelper _questRewardHelper;

    public QuestController(
        ISptLogger<QuestController> logger,
        TimeUtil timeUtil,
        HttpResponseUtil httpResponseUtil,
        QuestHelper questHelper,
        QuestRewardHelper questRewardHelper)
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _httpResponseUtil = httpResponseUtil;
        _questHelper = questHelper;
        _questRewardHelper = questRewardHelper;
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
