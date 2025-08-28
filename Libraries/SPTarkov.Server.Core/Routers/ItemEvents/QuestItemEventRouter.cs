using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable]
public class QuestItemEventRouter(QuestCallbacks questCallbacks) : ItemEventRouterDefinition
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return
        [
            new(ItemEventActions.QUEST_ACCEPT, false),
            new(ItemEventActions.QUEST_COMPLETE, false),
            new(ItemEventActions.QUEST_HANDOVER, false),
            new(ItemEventActions.REPEATABLE_QUEST_CHANGE, false),
        ];
    }

    protected override ValueTask<ItemEventRouterResponse> HandleItemEventInternal(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case ItemEventActions.QUEST_ACCEPT:
                return new ValueTask<ItemEventRouterResponse>(
                    questCallbacks.AcceptQuest(pmcData, body as AcceptQuestRequestData, sessionID)
                );
            case ItemEventActions.QUEST_COMPLETE:
                return new ValueTask<ItemEventRouterResponse>(
                    questCallbacks.CompleteQuest(pmcData, body as CompleteQuestRequestData, sessionID)
                );
            case ItemEventActions.QUEST_HANDOVER:
                return new ValueTask<ItemEventRouterResponse>(
                    questCallbacks.HandoverQuest(pmcData, body as HandoverQuestRequestData, sessionID)
                );
            case ItemEventActions.REPEATABLE_QUEST_CHANGE:
                return new ValueTask<ItemEventRouterResponse>(
                    questCallbacks.ChangeRepeatableQuest(pmcData, body as RepeatableQuestChangeRequest, sessionID)
                );
            default:
                throw new Exception($"QuestItemEventRouter being used when it cant handle route {url}");
        }
    }
}
