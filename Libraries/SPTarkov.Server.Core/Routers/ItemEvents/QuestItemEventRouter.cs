using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class QuestItemEventRouter(QuestCallbacks questCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<AcceptQuestRequestData>(
            ItemEventActions.QUEST_ACCEPT,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await questCallbacks.AcceptQuest(pmcData, body, sessionID)
        ),
        new ItemRouteAction<CompleteQuestRequestData>(
            ItemEventActions.QUEST_COMPLETE,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await questCallbacks.CompleteQuest(pmcData, body, sessionID)
        ),
        new ItemRouteAction<HandoverQuestRequestData>(
            ItemEventActions.QUEST_HANDOVER,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await questCallbacks.HandoverQuest(pmcData, body, sessionID)
        ),
        new ItemRouteAction<RepeatableQuestChangeRequest>(
            ItemEventActions.REPEATABLE_QUEST_CHANGE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await questCallbacks.ChangeRepeatableQuest(pmcData, body, sessionID)
        ),
    ]) { }
