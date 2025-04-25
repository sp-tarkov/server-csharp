using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.DI;

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
