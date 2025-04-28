using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class TradeCallbacks(TradeController _tradeController)
{
    /// <summary>
    ///     Handle client/game/profile/items/moving TradingConfirm event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ProcessTrade(PmcData pmcData, ProcessBaseTradeRequestData info, string sessionID)
    {
        return _tradeController.ConfirmTrading(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ProcessRagfairTrade(PmcData pmcData, ProcessRagfairTradeRequestData info, string sessionID)
    {
        return _tradeController.ConfirmRagfairTrading(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse SellAllFromSavage(PmcData pmcData, SellScavItemsToFenceRequestData info, string sessionID)
    {
        return _tradeController.SellScavItemsToFence(pmcData, info, sessionID);
    }
}
