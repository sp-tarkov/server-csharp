﻿using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class QuestStaticRouter : StaticRouter
{
    public QuestStaticRouter(JsonUtil jsonUtil, QuestCallbacks questCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/quest/list",
                    (url, info, sessionID, output) =>
                    {
                        return questCallbacks.ListQuests(
                            url,
                            info as ListQuestsRequestData,
                            sessionID
                        );
                    },
                    typeof(ListQuestsRequestData)
                ),
                new RouteAction(
                    "/client/repeatalbeQuests/activityPeriods",
                    (url, info, sessionID, output) =>
                    {
                        return questCallbacks.ActivityPeriods(
                            url,
                            info as EmptyRequestData,
                            sessionID
                        );
                    }
                ),
            ]
        ) { }
}
