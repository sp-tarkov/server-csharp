using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class TradeItemEventRouter(TradeCallbacks tradeCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<ProcessBaseTradeRequestData>(
            ItemEventActions.TRADING_CONFIRM,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await tradeCallbacks.ProcessTrade(pmcData, body, sessionID)
        ),
        new ItemRouteAction<ProcessRagfairTradeRequestData>(
            ItemEventActions.RAGFAIR_BUY_OFFER,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await tradeCallbacks.ProcessRagfairTrade(pmcData, body, sessionID)
        ),
        new ItemRouteAction<SellScavItemsToFenceRequestData>(
            ItemEventActions.SELL_ALL_FROM_SAVAGE,
            async (url, pmcData, body, sessionID, output, cancellationToken) =>
                await tradeCallbacks.SellAllFromSavage(pmcData, body, sessionID)
        ),
    ]) { }
