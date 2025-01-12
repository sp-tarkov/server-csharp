using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class QuestController
{
    private readonly ILogger _logger;
    private readonly TimeUtil _timeUtil;
    private readonly HttpResponseUtil _httpResponseUtil;
    private readonly QuestHelper _questHelper;
    private readonly QuestRewardHelper _questRewardHelper;

    public QuestController(
        ILogger logger,
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
