using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Utils;

namespace Core.DI;

public abstract class Router
{
    protected List<HandledRoute> handledRoutes = [];

    public virtual string GetTopLevelRoute()
    {
        return "spt";
    }

    protected abstract List<HandledRoute> GetHandledRoutes();

    protected List<HandledRoute> GetInternalHandledRoutes()
    {
        if (handledRoutes.Count == 0)
        {
            handledRoutes = GetHandledRoutes();
        }

        return handledRoutes;
    }

    public bool CanHandle(string url, bool partialMatch = false)
    {
        if (partialMatch)
        {
            return GetInternalHandledRoutes()
                .Where(r => r.dynamic)
                .Any(r => url.Contains(r.route));
        }

        return GetInternalHandledRoutes()
            .Where(r => !r.dynamic)
            .Any(r => r.route == url);
    }
}

public abstract class StaticRouter : Router
{
    private readonly List<RouteAction> _actions;
    private readonly JsonUtil _jsonUtil;

    public StaticRouter(JsonUtil jsonUtil, List<RouteAction> routes)
    {
        _actions = routes;
        _jsonUtil = jsonUtil;
    }

    public object HandleStatic(string url, string? body, string sessionID, string output)
    {
        var action = _actions.Single(route => route.url == url);
        var type = action.bodyType;
        IRequestData? info = null;
        if (type != null && !string.IsNullOrEmpty(body))
        {
            info = (IRequestData?) _jsonUtil.Deserialize(body, type);
        }

        return action.action(url, info, sessionID, output);
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return _actions.Select(route => new HandledRoute(route.url, false)).ToList();
    }
}

public abstract class DynamicRouter : Router
{
    private readonly JsonUtil _jsonUtil;
    private readonly List<RouteAction> actions;

    public DynamicRouter(JsonUtil jsonUtil, List<RouteAction> routes)
    {
        actions = routes;
        _jsonUtil = jsonUtil;
    }

    public object HandleDynamic(string url, string? body, string sessionID, string output)
    {
        var action = actions.First(r => url.Contains(r.url));
        var type = action.bodyType;
        IRequestData? info = null;
        if (type != null && !string.IsNullOrEmpty(body))
        {
            info = (IRequestData?) _jsonUtil.Deserialize(body, type);
        }

        return action.action(url, info, sessionID, output);
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return actions.Select(route => new HandledRoute(route.url, true)).ToList();
    }
}

// The name of this class should be ItemEventRouter, but that name is taken,
// So instead I added the definition
public abstract class ItemEventRouterDefinition : Router
{
    public abstract ItemEventRouterResponse? HandleItemEvent(string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        string sessionID,
        ItemEventRouterResponse output);
}

public abstract class SaveLoadRouter : Router
{
    public abstract SptProfile HandleLoad(SptProfile profile);
}

public record HandledRoute(string route, bool dynamic);

public record RouteAction(
    string url,
    Func<string, IRequestData?, string?, string?, object> action,
    Type? bodyType = null
);
//public action: (url: string, info: any, sessionID: string, output: string) => Promise<any>,
