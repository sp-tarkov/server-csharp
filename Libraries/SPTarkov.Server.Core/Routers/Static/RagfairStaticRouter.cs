using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class RagfairStaticRouter : StaticRouter
{
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
                ) => { return ragfairCallbacks.Search(url, info as SearchRequestData, sessionID); },
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/find",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.Search(url, info as SearchRequestData, sessionID); },
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/itemMarketPrice",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.GetMarketPrice(url, info as GetMarketPriceRequestData, sessionID); },
                typeof(GetMarketPriceRequestData)
            ),
            new RouteAction(
                "/client/ragfair/offerfees",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.StorePlayerOfferTaxAmount(url, info as StorePlayerOfferTaxAmountRequestData, sessionID); },
                typeof(StorePlayerOfferTaxAmountRequestData)
            ),
            new RouteAction(
                "/client/reports/ragfair/send",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.SendReport(url, info as SendRagfairReportRequestData, sessionID); },
                typeof(SendRagfairReportRequestData)
            ),
            new RouteAction(
                "/client/items/prices",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.GetFleaPrices(url, info as EmptyRequestData, sessionID); }),
            new RouteAction(
                "/client/ragfair/offer/findbyid",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => { return ragfairCallbacks.GetFleaOfferById(url, info as GetRagfairOfferByIdRequest, sessionID); },
                typeof(GetRagfairOfferByIdRequest)
            )
        ]
    )
    {
    }
}
