using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class DataDynamicRouter : DynamicRouter
{
    protected static DataCallbacks _dataCallbacks;
    
    public DataDynamicRouter(
        JsonUtil jsonUtil,
        DataCallbacks dataCallbacks
    ) : base(
        jsonUtil,
        [
        new RouteAction(
            "/client/menu/locale/", 
            (
                url, 
                info, 
                sessionID, 
                output
                ) => _dataCallbacks.GetLocalesMenu(url, info as EmptyRequestData, sessionID)),
        new RouteAction(
            "/client/locale/", 
            (
                url, 
                info, 
                sessionID, 
                output
            ) => _dataCallbacks.GetLocalesGlobal(url, info as EmptyRequestData, sessionID)),
        new RouteAction(
            "/client/items/prices/", 
            (
                url, 
                info, 
                sessionID, 
                output
            ) => _dataCallbacks.GetItemPrices(url, info as EmptyRequestData, sessionID)),
        ]
    )
    {
        _dataCallbacks = dataCallbacks;
    }
}
