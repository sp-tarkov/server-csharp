using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.DI;

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
