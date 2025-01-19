using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Callbacks;

[Injectable]
public class TradeCallbacks(TradeController _tradeController)
{
    /// <summary>
    /// Handle client/game/profile/items/moving TradingConfirm event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ProcessTrade(PmcData pmcData, ProcessBaseTradeRequestData info, string sessionID)
    {
        return _tradeController.ConfirmTrading(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ProcessRagfairTrade(PmcData pmcData, ProcessRagfairTradeRequestData info, string sessionID)
    {
        return _tradeController.ConfirmRagfairTrading(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SellAllFromSavage(PmcData pmcData, SellScavItemsToFenceRequestData info, string sessionID)
    {
        return _tradeController.SellScavItemsToFence(pmcData, info, sessionID);
    }
}
