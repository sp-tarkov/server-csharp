using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.Search(url, info as SearchRequestData, sessionID),
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/find",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.Search(url, info as SearchRequestData, sessionID),
                typeof(SearchRequestData)
            ),
            new RouteAction(
                "/client/ragfair/itemMarketPrice",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.GetMarketPrice(url, info as GetMarketPriceRequestData, sessionID),
                typeof(GetMarketPriceRequestData)
            ),
            new RouteAction(
                "/client/ragfair/offerfees",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.StorePlayerOfferTaxAmount(url, info as StorePlayerOfferTaxAmountRequestData, sessionID),
                typeof(StorePlayerOfferTaxAmountRequestData)
            ),
            new RouteAction(
                "/client/reports/ragfair/send",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.SendReport(url, info as SendRagfairReportRequestData, sessionID),
                typeof(SendRagfairReportRequestData)
            ),
            new RouteAction(
                "/client/items/prices",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.GetFleaPrices(url, info as EmptyRequestData, sessionID)
            ),
            new RouteAction(
                "/client/ragfair/offer/findbyid",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await ragfairCallbacks.GetFleaOfferById(url, info as GetRagfairOfferByIdRequest, sessionID),
                typeof(GetRagfairOfferByIdRequest)
            )
        ]
    )
    {
    }
}
