using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace SPTarkov.Server.Core.DI.Routing;

public abstract class ItemEventRouter(IEnumerable<ItemRouteAction> routes) : Router
{
    public async ValueTask<ItemEventRouterResponse> HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        MongoId sessionID,
        ItemEventRouterResponse output,
        CancellationToken cancellationToken = default
    )
    {
        var action = routes.Single(route => route.Url == url);

        var result = await action.Action(url, pmcData, body, sessionID, output, cancellationToken);

        return result;
    }

    public Type? GetBodyType(string url)
    {
        var action = routes.SingleOrDefault(route => route.Url == url);

        if (action is null)
        {
            return null;
        }

        return action.BodyType;
    }

    protected override IEnumerable<HandledRoute> GetHandledRoutes()
    {
        return routes.Select(route => new HandledRoute(route.Url, false));
    }
}

public abstract record ItemRouteAction(
    string Url,
    Func<
        string,
        PmcData,
        BaseInteractionRequestData,
        MongoId,
        ItemEventRouterResponse,
        CancellationToken,
        ValueTask<ItemEventRouterResponse>
    > Action,
    Type BodyType
);

public sealed record ItemRouteAction<TRequest>(
    string Url,
    Func<string, PmcData, TRequest, MongoId, ItemEventRouterResponse, CancellationToken, ValueTask<ItemEventRouterResponse>> TypedAction
)
    : ItemRouteAction(
        Url,
        (url, pmcData, body, sessionID, output, cancellationToken) =>
        {
            if (body is not TRequest typedBody)
            {
                throw new InvalidOperationException(
                    $"Item event route '{url}' expected body type '{typeof(TRequest).Name}' but received '{body?.GetType().Name ?? "null"}'."
                );
            }

            return TypedAction(url, pmcData, typedBody, sessionID, output, cancellationToken);
        },
        typeof(TRequest)
    )
    where TRequest : BaseInteractionRequestData;
