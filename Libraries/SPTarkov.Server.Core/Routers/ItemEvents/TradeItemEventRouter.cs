using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class TradeItemEventRouter : ItemEventRouterDefinition
{
    protected TradeCallbacks _tradeCallbacks;

    public TradeItemEventRouter(TradeCallbacks tradeCallbacks)
    {
        _tradeCallbacks = tradeCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.TRADING_CONFIRM, false),
            new(ItemEventActions.RAGFAIR_BUY_OFFER, false),
            new(ItemEventActions.SELL_ALL_FROM_SAVAGE, false),
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case ItemEventActions.TRADING_CONFIRM:
                return _tradeCallbacks.ProcessTrade(
                    pmcData,
                    body as ProcessBaseTradeRequestData,
                    sessionID
                );
            case ItemEventActions.RAGFAIR_BUY_OFFER:
                return _tradeCallbacks.ProcessRagfairTrade(
                    pmcData,
                    body as ProcessRagfairTradeRequestData,
                    sessionID
                );
            case ItemEventActions.SELL_ALL_FROM_SAVAGE:
                return _tradeCallbacks.SellAllFromSavage(
                    pmcData,
                    body as SellScavItemsToFenceRequestData,
                    sessionID
                );
            default:
                throw new Exception(
                    $"TradeItemEventRouter being used when it cant handle route {url}"
                );
        }
    }
}
