using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class DataStaticRouter(JsonUtil jsonUtil, DataCallbacks dataCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/settings",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetSettings(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/globals",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetGlobals(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/items",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetTemplateItems(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/handbook/templates",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetTemplateHandbook(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/customization",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetTemplateSuits(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/account/customization",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetTemplateCharacter(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/hideout/production/recipes",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetHideoutProduction(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/hideout/settings",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetHideoutSettings(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/hideout/areas",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetHideoutAreas(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/languages",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetLocalesLanguages(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/hideout/qte/list",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetQteList(url, info, sessionID)
            ),
            new RouteAction<GetClientDialogueRequestData>(
                "/client/dialogue",
                async (url, info, sessionID, output, cancellationToken) => await dataCallbacks.GetDialogue(url, info, sessionID)
            ),
        ]
    ) { }
