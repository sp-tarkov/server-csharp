using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Callbacks;

public class TradeCallbacks
{
    public TradeCallbacks()
    {
        
    }
    
    public ItemEventRouterResponse ProcessTrade(PmcData pmcData, ProcessBaseTradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ProcessRagfairTrade(PmcData pmcData, ProcessRagfairTradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse SellAllFromSavage(PmcData pmcData, SellScavItemsToFenceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}