using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Ragfair;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class RagfairStaticRouter : StaticRouter
{
    protected static RagfairCallbacks _ragfairCallbacks;

    public RagfairStaticRouter(
        JsonUtil jsonUtil,
        RagfairCallbacks ragfairCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/ragfair/search",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.Search(url, info as SearchRequestData, sessionID),
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/find",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.Search(url, info as SearchRequestData, sessionID),
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/itemMarketPrice",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.GetMarketPrice(url, info as GetMarketPriceRequestData, sessionID),
                typeof(GetMarketPriceRequestData)
            ),
            new RouteAction(
                "/client/ragfair/offerfees",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.StorePlayerOfferTaxAmount(url, info as StorePlayerOfferTaxAmountRequestData, sessionID),
                typeof(StorePlayerOfferTaxAmountRequestData)
            ),
            new RouteAction(
                "/client/reports/ragfair/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.SendReport(url, info as SendRagfairReportRequestData, sessionID),
                typeof(SendRagfairReportRequestData)
            ),
            new RouteAction(
                "/client/items/prices",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.GetFleaPrices(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/ragfair/offer/findbyid",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _ragfairCallbacks.GetFleaOfferById(url, info as GetRagfairOfferByIdRequest, sessionID),
                typeof(GetRagfairOfferByIdRequest)
            )
        ]
    )
    {
        _ragfairCallbacks = ragfairCallbacks;
    }
}
