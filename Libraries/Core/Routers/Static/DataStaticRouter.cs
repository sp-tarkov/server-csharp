using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class DataStaticRouter : StaticRouter
{
    protected static DataCallbacks _dataCallbacks;

    public DataStaticRouter(
        JsonUtil jsonUtil,
        DataCallbacks dataCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/settings",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetSettings(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/globals",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetGlobals(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/items",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetTemplateItems(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/handbook/templates",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetTemplateHandbook(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/customization",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetTemplateSuits(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/account/customization",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetTemplateCharacter(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/production/recipes",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetHideoutProduction(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/settings",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetHideoutSettings(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/areas",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetHideoutAreas(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/languages",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetLocalesLanguages(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/hideout/qte/list",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _dataCallbacks.GetQteList(url, info as EmptyRequestData, sessionID)
            )
        ]
    )
    {
        _dataCallbacks = dataCallbacks;
    }
}
