using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Utils;

namespace Core.DI;

public abstract class Router
{
    protected List<HandledRoute> handledRoutes = [];

    public string GetTopLevelRoute()
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
                .Where((r) => r.dynamic)
                .Any((r) => url.Contains(r.route));
        }

        return GetInternalHandledRoutes()
            .Where((r) => !r.dynamic)
            .Any((r) => r.route == url);
    }

    public abstract Type? GetBodyDeserializationType();
}

public abstract class StaticRouter : Router
{
    private List<RouteAction<IRequestData>> actions;

    public StaticRouter(List<RouteAction<IRequestData>> routes) : base()
    {
        actions = routes;
    }

    public object HandleStatic(string url, IRequestData? info, string sessionID, string output)
    {
        return actions.Single(route => route.url == url).action(url, info, sessionID, output);
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return actions.Select((route) => new HandledRoute(route.url, false)).ToList();
    }
}

public abstract class DynamicRouter : Router
{
    private List<RouteAction<IRequestData>> actions;

    public DynamicRouter(List<RouteAction<IRequestData>> routes) : base()
    {
        actions = routes;
    }

    public object HandleDynamic(string url, IRequestData? info, string sessionID, string output)
    {
        return actions.First(r => url.Contains(r.url)).action(url, info, sessionID, output);
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return actions.Select((route) => new HandledRoute(route.url, true)).ToList();
    }
}

// The name of this class should be ItemEventRouter, but that name is taken,
// So instead I added the definition
public abstract class ItemEventRouterDefinition : Router
{
    public abstract object HandleItemEvent(
        string url,
        PmcData pmcData,
        object body,
        string sessionID,
        ItemEventRouterResponse output
    );
}

public abstract class SaveLoadRouter : Router
{
    public abstract SptProfile HandleLoad(SptProfile profile);
}

public record HandledRoute(string route, bool dynamic);

public record RouteAction<T>(string url, Func<string, T?, string?, string?, object> action) where T : IRequestData;
//public action: (url: string, info: any, sessionID: string, output: string) => Promise<any>,
