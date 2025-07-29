using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.DI;

public abstract class Router
{
    protected IEnumerable<HandledRoute> handledRoutes = [];

    public virtual string GetTopLevelRoute()
    {
        return "spt";
    }

    protected abstract IEnumerable<HandledRoute> GetHandledRoutes();

    protected IEnumerable<HandledRoute> GetInternalHandledRoutes()
    {
        if (!handledRoutes.Any())
        {
            handledRoutes = GetHandledRoutes();
        }

        return handledRoutes;
    }

    public bool CanHandle(string url, bool partialMatch = false)
    {
        if (partialMatch)
        {
            return GetInternalHandledRoutes().Where(r => r.dynamic).Any(r => url.Contains(r.route));
        }

        return GetInternalHandledRoutes().Where(r => !r.dynamic).Any(r => r.route == url);
    }
}

public abstract class StaticRouter(JsonUtil jsonUtil, IEnumerable<RouteAction> routes) : Router
{
    public async ValueTask<object> HandleStatic(string url, string? body, MongoId sessionId, string output)
    {
        var action = routes.Single(route => route.url == url);
        var type = action.bodyType;
        IRequestData? info = null;
        if (type != null && !string.IsNullOrEmpty(body))
        {
            info = (IRequestData?)jsonUtil.Deserialize(body, type);
        }

        return await action.action(url, info, sessionId, output);
    }

    protected override IEnumerable<HandledRoute> GetHandledRoutes()
    {
        return routes.Select(route => new HandledRoute(route.url, false));
    }
}

public abstract class DynamicRouter(JsonUtil jsonUtil, IEnumerable<RouteAction> routes) : Router
{
    public async ValueTask<object> HandleDynamic(string url, string? body, MongoId sessionID, string output)
    {
        var action = routes.First(r => url.Contains(r.url));
        var type = action.bodyType;
        IRequestData? info = null;
        if (type != null && !string.IsNullOrEmpty(body))
        {
            info = (IRequestData?)jsonUtil.Deserialize(body, type);
        }

        return await action.action(url, info, sessionID, output);
    }

    protected override IEnumerable<HandledRoute> GetHandledRoutes()
    {
        return routes.Select(route => new HandledRoute(route.url, true));
    }
}

// The name of this class should be ItemEventRouter, but that name is taken,
// So instead I added the definition
public abstract class ItemEventRouterDefinition : Router
{
    public abstract ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    );
}

public abstract class SaveLoadRouter : Router
{
    public abstract SptProfile HandleLoad(SptProfile profile);
}

public record HandledRoute(string route, bool dynamic);

public record RouteAction(string url, Func<string, IRequestData?, MongoId, string?, ValueTask<object>> action, Type? bodyType = null);
//public action: (url: string, info: any, sessionID: string, output: string) => Promise<any>,
