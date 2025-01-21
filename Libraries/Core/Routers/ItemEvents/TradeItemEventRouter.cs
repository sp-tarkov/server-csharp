using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class TradeItemEventRouter : ItemEventRouterDefinition
{
    protected TradeCallbacks _tradeCallbacks;

    public TradeItemEventRouter
    (
        TradeCallbacks tradeCallbacks
    )
    {
        _tradeCallbacks = tradeCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new()
        {
            new HandledRoute("TradingConfirm", false),
            new HandledRoute("RagFairBuyOffer", false),
            new HandledRoute("SellAllFromSavage", false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url) {
            case "TradingConfirm":
                return Task.FromResult(_tradeCallbacks.ProcessTrade(pmcData, body as ProcessBaseTradeRequestData, sessionID));
            case "RagFairBuyOffer":
                return Task.FromResult(_tradeCallbacks.ProcessRagfairTrade(pmcData, body as ProcessRagfairTradeRequestData, sessionID));
            case "SellAllFromSavage":
                return Task.FromResult(_tradeCallbacks.SellAllFromSavage(pmcData, body as SellScavItemsToFenceRequestData, sessionID));
            default:
                throw new Exception($"TradeItemEventRouter being used when it cant handle route {url}");
        }
    }
}
