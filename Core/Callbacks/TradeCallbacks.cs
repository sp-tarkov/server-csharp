using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Callbacks;

public class TradeCallbacks
{
    public TradeCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle client/game/profile/items/moving TradingConfirm event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse ProcessTrade(PmcData pmcData, ProcessBaseTradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse ProcessRagfairTrade(PmcData pmcData, ProcessRagfairTradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse SellAllFromSavage(PmcData pmcData, SellScavItemsToFenceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}