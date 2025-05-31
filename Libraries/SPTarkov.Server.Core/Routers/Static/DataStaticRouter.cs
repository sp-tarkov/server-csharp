using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class DataStaticRouter : StaticRouter
{
    public DataStaticRouter(
        JsonUtil jsonUtil,
        DataCallbacks dataCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/settings",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetSettings(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/globals",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetGlobals(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/items",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetTemplateItems(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/handbook/templates",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetTemplateHandbook(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/customization",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetTemplateSuits(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/account/customization",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetTemplateCharacter(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/production/recipes",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetHideoutProduction(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/settings",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetHideoutSettings(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/areas",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetHideoutAreas(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/languages",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetLocalesLanguages(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/qte/list",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await dataCallbacks.GetQteList(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
    }
}
