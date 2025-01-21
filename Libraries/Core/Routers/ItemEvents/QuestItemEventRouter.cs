using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;


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
        return new()
        {
            new HandledRoute("QuestAccept", false),
            new HandledRoute("QuestComplete", false),
            new HandledRoute("QuestHandover", false),
            new HandledRoute("RepeatableQuestChange", false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url) {
            case "QuestAccept":
                return Task.FromResult(_questCallbacks.AcceptQuest(pmcData, body as AcceptQuestRequestData, sessionID));
            case "QuestComplete":
                return Task.FromResult(_questCallbacks.CompleteQuest(pmcData, body as CompleteQuestRequestData, sessionID));
            case "QuestHandover":
                return Task.FromResult(_questCallbacks.HandoverQuest(pmcData, body as HandoverQuestRequestData, sessionID));
            case "RepeatableQuestChange":
                return Task.FromResult(_questCallbacks.ChangeRepeatableQuest(pmcData, body as RepeatableQuestChangeRequest, sessionID));
            default:
                throw new Exception($"QuestItemEventRouter being used when it cant handle route {url}");
        }
    }
}
