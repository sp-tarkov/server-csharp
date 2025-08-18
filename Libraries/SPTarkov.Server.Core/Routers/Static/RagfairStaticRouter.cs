using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class RagfairStaticRouter(JsonUtil jsonUtil, RagfairCallbacks ragfairCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<SearchRequestData>(
                "/client/ragfair/search",
                async (url, info, sessionID, output) => await ragfairCallbacks.Search(url, info, sessionID)
            ),
            new RouteAction<SearchRequestData>(
                "/client/ragfair/find",
                async (url, info, sessionID, output) => await ragfairCallbacks.Search(url, info, sessionID)
            ),
            new RouteAction<GetMarketPriceRequestData>(
                "/client/ragfair/itemMarketPrice",
                async (url, info, sessionID, output) => await ragfairCallbacks.GetMarketPrice(url, info, sessionID)
            ),
            new RouteAction<StorePlayerOfferTaxAmountRequestData>(
                "/client/ragfair/offerfees",
                async (url, info, sessionID, output) => await ragfairCallbacks.StorePlayerOfferTaxAmount(url, info, sessionID)
            ),
            new RouteAction<SendRagfairReportRequestData>(
                "/client/reports/ragfair/send",
                async (url, info, sessionID, output) => await ragfairCallbacks.SendReport(url, info, sessionID)
            ),
            new RouteAction<EmptyRequestData>(
                "/client/items/prices",
                async (url, info, sessionID, output) => await ragfairCallbacks.GetFleaPrices(url, info, sessionID)
            ),
            new RouteAction<GetRagfairOfferByIdRequest>(
                "/client/ragfair/offer/findbyid",
                async (url, info, sessionID, output) => await ragfairCallbacks.GetFleaOfferById(url, info, sessionID)
            ),
        ]
    ) { }
