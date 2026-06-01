using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class QuestItemEventRouter(QuestCallbacks questCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<AcceptQuestRequestData>(
            ItemEventActions.QUEST_ACCEPT,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return questCallbacks.AcceptQuest(pmcData, body, sessionID);
            }
        ),
        new ItemRouteAction<CompleteQuestRequestData>(
            ItemEventActions.QUEST_COMPLETE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return questCallbacks.CompleteQuest(pmcData, body, sessionID);
            }
        ),
        new ItemRouteAction<HandoverQuestRequestData>(
            ItemEventActions.QUEST_HANDOVER,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return questCallbacks.HandoverQuest(pmcData, body, sessionID);
            }
        ),
        new ItemRouteAction<RepeatableQuestChangeRequest>(
            ItemEventActions.REPEATABLE_QUEST_CHANGE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return questCallbacks.ChangeRepeatableQuest(pmcData, body, sessionID);
            }
        ),
    ]) { }
