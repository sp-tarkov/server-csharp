using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Eft.Trade;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;


namespace Core.Controllers;

[Injectable]
public class TradeController(
    ISptLogger<TradeController> _logger,
    DatabaseService _databaseService,
    EventOutputHolder _eventOutputHolder,
    TradeHelper _tradeHelper,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    ProfileHelper _profileHelper,
    RagfairOfferHelper _ragfairOfferHelper,
    TraderHelper _traderHelper,
    RagfairServer _ragfairServer,
    HttpResponseUtil _httpResponseUtil,
    LocalisationService _localisationService,
    RagfairPriceService _ragfairPriceService,
    MailSendService _mailSendService,
    ConfigServer _configServer
)
{
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    protected TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();

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
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        // Buying
        if (request.Type == "buy_from_trader")
        {
            var foundInRaid = _traderConfig.PurchasesAreFoundInRaid;
            ProcessBuyTradeRequestData buyData = (ProcessBuyTradeRequestData)request;
            _tradeHelper.buyItem(pmcData, buyData, sessionID, foundInRaid, output);

            return output;
        }

        // Selling
        if (request.Type == "sell_to_trader")
        {
            ProcessSellTradeRequestData sellData = (ProcessSellTradeRequestData)request;
            _tradeHelper.sellItem(pmcData, pmcData, sellData, sessionID, output);

            return output;
        }

        var errorMessage = $"Unhandled trade event: {request.Type}";
        _logger.Error(errorMessage);

        return _httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.RagfairUnavailable);
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
        string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        foreach (var offer in request.Offers)
        {
            var fleaOffer = _ragfairServer.GetOffer(offer.Id);
            if (fleaOffer is null)
            {
                return _httpResponseUtil.AppendErrorToOutput(
                    output,
                    $"Offer with ID {offer.Id} not found",
                    BackendErrorCodes.OfferNotFound
                );
            }

            if (offer.Count == 0)
            {
                var errorMessage = _localisationService.GetText(
                    "ragfair-unable_to_purchase_0_count_item",
                    _itemHelper.GetItem(fleaOffer.Items[0].Template).Value.Name
                );
                return _httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.OfferOutOfStock);
            }

            if (_ragfairOfferHelper.OfferIsFromTrader(fleaOffer))
            {
                BuyTraderItemFromRagfair(sessionID, pmcData, fleaOffer, offer, output);
            }
            else
            {
                BuyPmcItemFromRagfair(sessionID, pmcData, fleaOffer, offer, output);
            }

            // Exit loop early if problem found
            if (output.Warnings?.Count > 0)
            {
                return output;
            }
        }

        return output;
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
        OfferRequest requestOffer,
        ItemEventRouterResponse output)
    {
        // Skip buying items when player doesn't have needed loyalty
        if (PlayerLacksTraderLoyaltyLevelToBuyOffer(fleaOffer, pmcData))
        {
            var errorMessage = $"Unable to buy item: {fleaOffer.Items[0].Template} from trader: {fleaOffer.User.Id} as loyalty level too low, skipping";
            _logger.Debug(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.RagfairUnavailable);

            return;
        }

        ProcessBuyTradeRequestData buyData = new ProcessBuyTradeRequestData
        {
            Action = "TradingConfirm",
            Type = "buy_from_ragfair",
            TransactionId = fleaOffer.User.Id,
            ItemId = fleaOffer.Root,
            Count = requestOffer.Count,
            SchemeId = 0,
            SchemeItems = requestOffer.Items,
        };

        _tradeHelper.buyItem(pmcData, buyData, sessionId, _traderConfig.PurchasesAreFoundInRaid, output);
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
        OfferRequest requestOffer,
        ItemEventRouterResponse output)
    {
        ProcessBuyTradeRequestData buyData = new ProcessBuyTradeRequestData
        {
            Action = "TradingConfirm",
            Type = "buy_from_ragfair",
            TransactionId = "ragfair",
            ItemId = fleaOffer.Id, // Store ragfair offerId in buyRequestData.item_id
            Count = requestOffer.Count,
            SchemeId = 0,
            SchemeItems = requestOffer.Items,
        };

        // buyItem() must occur prior to removing the offer stack, otherwise item inside offer doesn't exist for confirmTrading() to use
        _tradeHelper.buyItem(pmcData, buyData, sessionId, _ragfairConfig.Dynamic.PurchasesAreFoundInRaid, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // resolve when a profile buy another profile's offer
        var offerOwnerId = fleaOffer.User?.Id;
        var offerBuyCount = requestOffer.Count;

        var isPlayerOffer = IsPlayerOffer(fleaOffer.Id, fleaOffer.User?.Id);
        if (isPlayerOffer)
        {
            // Complete selling the offer now its been purchased
            _ragfairOfferHelper.CompleteOffer(offerOwnerId, fleaOffer, offerBuyCount ?? 0);

            return;
        }

        // Remove/lower stack count of item purchased from PMC flea offer
        _ragfairServer.RemoveOfferStack(fleaOffer.Id, requestOffer.Count ?? 0);
    }

    /// <summary>
    /// Is the provided offerid and ownerid from a player made offer
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    /// <param name="offerOwnerId">Owner id</param>
    /// <returns>true if offer was made by a player</returns>
    private bool IsPlayerOffer(
        string offerId,
        string? offerOwnerId)
    {
        // No ownerid, not player offer
        if (offerOwnerId is null)
        {
            return false;
        }

        var offerCreatorProfile = _profileHelper.GetPmcProfile(offerOwnerId);
        if (offerCreatorProfile is null || offerCreatorProfile.RagfairInfo.Offers?.Count == 0)
        {
            // No profile or no offers
            return false;
        }

        // Does offer id exist in profile
        return offerCreatorProfile.RagfairInfo.Offers.Any(offer => offer.Id == offerId);
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
        return fleaOffer.LoyaltyLevel > pmcData.TradersInfo[fleaOffer.User.Id].LoyaltyLevel;
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
        var output = _eventOutputHolder.GetOutput(sessionId);

        MailMoneyToPlayer(sessionId, (int)request.TotalValue, Traders.FENCE);

        return output;
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
        _logger.Debug($"Selling scav items to fence for {roublesToSend} roubles");

        // Create single currency item with all currency on it
        Item rootCurrencyReward = new Item
        {
            Id = _hashUtil.Generate(),
            Template = Money.ROUBLES,
            Upd = new Upd { StackObjectsCount = roublesToSend },
        };

        // Ensure money is properly split to follow its max stack size limit
        var curencyReward = _itemHelper.SplitStackIntoSeparateItems(rootCurrencyReward);

        // Send mail from trader
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            _traderHelper.GetTraderById(trader).ToString(),
            MessageType.MESSAGE_WITH_ITEMS,
            _randomUtil.GetArrayValue((_databaseService.GetTrader(trader).Dialogue.TryGetValue("SoldItems", out var items)) ? items : new List<string>()),
            curencyReward.SelectMany(x => x).ToList(),
            _timeUtil.GetHoursAsSeconds(72)
        );
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
        Dictionary<string, int?> handbookPrices,
        TraderBase traderDetails)
    {
        var itemWithChildren = _itemHelper.FindAndReturnChildrenAsItems(items, parentItemId);

        var totalPrice = 0;
        foreach (var itemToSell in itemWithChildren) {
            var itemDetails = _itemHelper.GetItem(itemToSell.Template);
            if (!(itemDetails.Key && _itemHelper.IsOfBaseclasses(itemDetails.Value.Id, traderDetails.ItemsBuy.Category))) 
            {
                // Skip if tpl isn't item OR item doesn't fulfil match traders buy categories
                continue;
            }

            // Get price of item multiplied by how many are in stack
            totalPrice += (int)((handbookPrices[itemToSell.Template] ?? 0) * (itemToSell.Upd?.StackObjectsCount ?? 1));
        }

        return totalPrice;
    }
}
