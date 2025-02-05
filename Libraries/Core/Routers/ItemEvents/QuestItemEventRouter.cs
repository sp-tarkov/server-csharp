using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;


namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class QuestItemEventRouter : ItemEventRouterDefinition
{
    protected QuestCallbacks _questCallbacks;

    public QuestItemEventRouter
    (
        QuestCallbacks questCallbacks
    )
    {
        _questCallbacks = questCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.QUEST_ACCEPT, false),
            new(ItemEventActions.QUEST_COMPLETE, false),
            new(ItemEventActions.QUEST_HANDOVER, false),
            new(ItemEventActions.REPEATABLE_QUEST_CHANGE, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.QUEST_ACCEPT:
                return _questCallbacks.AcceptQuest(pmcData, body as AcceptQuestRequestData, sessionID);
            case ItemEventActions.QUEST_COMPLETE:
                return _questCallbacks.CompleteQuest(pmcData, body as CompleteQuestRequestData, sessionID);
            case ItemEventActions.QUEST_HANDOVER:
                return _questCallbacks.HandoverQuest(pmcData, body as HandoverQuestRequestData, sessionID);
            case ItemEventActions.REPEATABLE_QUEST_CHANGE:
                return _questCallbacks.ChangeRepeatableQuest(pmcData, body as RepeatableQuestChangeRequest, sessionID);
            default:
                throw new Exception($"QuestItemEventRouter being used when it cant handle route {url}");
        }
    }
}
