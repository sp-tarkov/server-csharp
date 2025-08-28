using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.DI;

public interface IOnBeforeEventRequestData;

public interface IOnAfterEventRequestData;

public record StaticDynamicOnBeforeEventRequestData(string Url, IRequestData RequestData, MongoId SessionId, string Output)
    : IOnBeforeEventRequestData;

public record StaticDynamicOnAfterEventRequestData(string Url, IRequestData RequestData, MongoId SessionId, string Output, object Result)
    : IOnAfterEventRequestData;

public abstract class Router
{
    public event EventHandler<IOnBeforeEventRequestData>? OnBeforeAction;
    public event EventHandler<IOnAfterEventRequestData>? OnAfterAction;

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

    protected void TriggerOnBeforeAction(IOnBeforeEventRequestData requestData)
    {
        OnBeforeAction?.Invoke(this, requestData);
    }

    protected void TriggerOnAfterAction(IOnAfterEventRequestData requestData)
    {
        OnAfterAction?.Invoke(this, requestData);
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

        info ??= new EmptyRequestData();
        TriggerOnBeforeAction(new StaticDynamicOnBeforeEventRequestData(url, info, sessionId, output));
        var result = await action.action(url, info, sessionId, output);
        TriggerOnAfterAction(new StaticDynamicOnAfterEventRequestData(url, info, sessionId, output, result));
        return result;
    }

    protected override IEnumerable<HandledRoute> GetHandledRoutes()
    {
        return routes.Select(route => new HandledRoute(route.url, false));
    }
}

public abstract class DynamicRouter(JsonUtil jsonUtil, IEnumerable<RouteAction> routes) : Router
{
    public async ValueTask<object> HandleDynamic(string url, string? body, MongoId sessionId, string output)
    {
        var action = routes.First(r => url.Contains(r.url));
        var type = action.bodyType;
        IRequestData? info = null;
        if (type != null && !string.IsNullOrEmpty(body))
        {
            info = (IRequestData?)jsonUtil.Deserialize(body, type);
        }

        info ??= new EmptyRequestData();
        TriggerOnBeforeAction(new StaticDynamicOnBeforeEventRequestData(url, info, sessionId, output));
        var result = await action.action(url, info, sessionId, output);
        TriggerOnAfterAction(new StaticDynamicOnAfterEventRequestData(url, info, sessionId, output, result));
        return result;
    }

    protected override IEnumerable<HandledRoute> GetHandledRoutes()
    {
        return routes.Select(route => new HandledRoute(route.url, true));
    }
}

public record ItemRouterOnBeforeEventRequestData(
    string Url,
    PmcData PmcData,
    BaseInteractionRequestData Body,
    MongoId SessionId,
    ItemEventRouterResponse Output
) : IOnBeforeEventRequestData;

public record ItemRouterOnAfterEventRequestData(
    string Url,
    PmcData PmcData,
    BaseInteractionRequestData Body,
    MongoId SessionId,
    ItemEventRouterResponse Output,
    ValueTask<ItemEventRouterResponse> Result
) : IOnAfterEventRequestData;

public record OnAfterEventRequestData<T, R>(string Url, T RequestData, MongoId SessionId, R Output, object Result)
    : IOnAfterEventRequestData;

// The name of this class should be ItemEventRouter, but that name is taken,
// So instead I added the definition
public abstract class ItemEventRouterDefinition : Router
{
    public ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        TriggerOnBeforeAction(new ItemRouterOnBeforeEventRequestData(url, pmcData, body, sessionID, output));
        var result = HandleItemEventInternal(url, pmcData, body, sessionID, output);
        TriggerOnAfterAction(new ItemRouterOnAfterEventRequestData(url, pmcData, body, sessionID, output, result));
        return result;
    }

    protected abstract ValueTask<ItemEventRouterResponse> HandleItemEventInternal(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output
    );
}

public record SaveLoadOnBeforeEventRequestData(SptProfile Profile) : IOnBeforeEventRequestData;

public record SaveLoadRouterOnAfterEventRequestData(SptProfile Profile) : IOnAfterEventRequestData;

public abstract class SaveLoadRouter : Router
{
    public SptProfile HandleLoad(SptProfile profile)
    {
        TriggerOnBeforeAction(new SaveLoadOnBeforeEventRequestData(profile));
        var result = HandleLoadInternal(profile);
        TriggerOnAfterAction(new SaveLoadRouterOnAfterEventRequestData(profile));
        return result;
    }

    protected abstract SptProfile HandleLoadInternal(SptProfile profile);
}

public record HandledRoute(string route, bool dynamic);

public record RouteAction(string url, Func<string, IRequestData, MongoId, string?, ValueTask<object>> action, Type? bodyType = null);

public record RouteAction<TRequest>(string url, Func<string, TRequest, MongoId, string?, ValueTask<string>> typedAction)
    : RouteAction(url, async (url, info, sessionId, output) => await typedAction(url, (TRequest)info, sessionId, output), typeof(TRequest))
    where TRequest : class;
