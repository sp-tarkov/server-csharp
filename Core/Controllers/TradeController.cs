using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Eft.Trade;
using Core.Models.Enums;

namespace Core.Controllers;

[Injectable]
public class TradeController
{
    /// <summary>
    /// Handle TradingConfirm event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmTrading(
        PmcData pmcData,
        ProcessBaseTradeRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmRagfairTrading(
        PmcData pmcData,
        ProcessRagfairTradeRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Buy an item off the flea sold by a trader
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="offerRequest">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    private void BuyTraderItemFromRagfair(
        string sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest offerRequest,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Buy an item off the flea sold by a PMC
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="offerRequest">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    private void BuyPmcItemFromRagfair(
        string sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest offerRequest,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided offerid and ownerid from a player made offer
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    /// <param name="offerOwnerId">Owner id</param>
    /// <returns>true if offer was made by a player</returns>
    private bool IsPlayerOffer(
        string offerId,
        string offerOwnerId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does Player have necessary trader loyalty to purchase flea offer
    /// </summary>
    /// <param name="fleaOffer">Flea offer being bought</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>True if player can buy offer</returns>
    private bool PlayerLacksTraderLoyaltyLevelToBuyOffer(
        RagfairOffer fleaOffer,
        PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SellScavItemsToFence(
        PmcData pmcData,
        SellScavItemsToFenceRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send the specified rouble total to player as mail
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="roublesToSend">amount of roubles to send</param>
    /// <param name="trader">Trader to sell items to</param>
    private void MailMoneyToPlayer(
        string sessionId,
        int roublesToSend,
        string trader)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Looks up an items children and gets total handbook price for them
    /// </summary>
    /// <param name="parentItemId">parent item that has children we want to sum price of</param>
    /// <param name="items">All items (parent + children)</param>
    /// <param name="handbookPrices">Prices of items from handbook</param>
    /// <param name="traderDetails">Trader being sold to, to perform buy category check against</param>
    /// <returns>Rouble price</returns>
    private int GetPriceOfItemAndChildren(
        string parentItemId,
        List<Item> items,
        Dictionary<string, int> handbookPrices,
        TraderBase traderDetails)
    {
        throw new NotImplementedException();
    }
}
