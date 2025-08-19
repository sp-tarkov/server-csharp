using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class QuestStaticRouter(JsonUtil jsonUtil, QuestCallbacks questCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ListQuestsRequestData>(
                "/client/quest/list",
                async (url, info, sessionID, output) => await questCallbacks.ListQuests(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/repeatalbeQuests/activityPeriods",
                async (url, info, sessionID, output) => await questCallbacks.ActivityPeriods(url, info, sessionID)
            ),
        ]
    ) { }
