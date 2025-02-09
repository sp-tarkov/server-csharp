using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Quests;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class QuestStaticRouter : StaticRouter
{
    public QuestStaticRouter(
        JsonUtil jsonUtil,
        QuestCallbacks questCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/quest/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => questCallbacks.ListQuests(url, info as ListQuestsRequestData, sessionID),
                typeof(ListQuestsRequestData)
            ),
            new RouteAction(
                "/client/repeatalbeQuests/activityPeriods",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => questCallbacks.ActivityPeriods(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
