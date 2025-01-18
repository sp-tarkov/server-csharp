using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Helpers;

[Injectable]
public class TradeHelper()
{
    
    /// <summary>
    /// Buy item from flea or trader
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="buyRequestData">data from client</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="foundInRaid">Should item be found in raid</param>
    /// <param name="output">Item event router response</param>
    public void buyItem(
        PmcData pmcData,
        ProcessBuyTradeRequestData buyRequestData,
        string sessionID,
        bool foundInRaid,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sell item to trader
    /// </summary>
    /// <param name="profileWithItemsToSell">Profile to remove items from</param>
    /// <param name="profileToReceiveMoney">Profile to accept the money for selling item</param>
    /// <param name="sellRequest">Request data</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="output">Item event router response</param>
    public void sellItem(
        PmcData profileWithItemsToSell,
        PmcData profileToReceiveMoney,
        ProcessSellTradeRequestData sellRequest,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    protected void incrementCirculateSoldToTraderCounter(
        PmcData profileWithItemsToSell,
        PmcData profileToReceiveMoney,
        ProcessSellTradeRequestData sellRequest
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Traders allow a limited number of purchases per refresh cycle (default 60 mins)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile making the purchase</param>
    /// <param name="traderId">Trader assort is purchased from</param>
    /// <param name="assortBeingPurchased">the item from trader being bought</param>
    /// <param name="assortId">Id of assort being purchased</param>
    /// <param name="count">How many of the item are being bought</param>
    protected void checkPurchaseIsWithinTraderItemLimit(
        string sessionId,
        PmcData pmcData,
        string traderId,
        Item assortBeingPurchased,
        string assortId,
        int count
    )
    {
        throw new NotImplementedException();
    }
}
