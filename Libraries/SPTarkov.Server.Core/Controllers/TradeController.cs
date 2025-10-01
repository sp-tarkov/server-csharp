using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class TradeController(
    ISptLogger<TradeController> logger,
    DatabaseService databaseService,
    EventOutputHolder eventOutputHolder,
    TradeHelper tradeHelper,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    ItemHelper itemHelper,
    RagfairOfferHelper ragfairOfferHelper,
    RagfairServer ragfairServer,
    HttpResponseUtil httpResponseUtil,
    ServerLocalisationService serverLocalisationService,
    MailSendService mailSendService,
    ConfigServer configServer
)
{
    protected readonly RagfairConfig RagfairConfig = configServer.GetConfig<RagfairConfig>();
    protected readonly TraderConfig TraderConfig = configServer.GetConfig<TraderConfig>();

    /// <summary>
    ///     Handle TradingConfirm event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmTrading(PmcData pmcData, ProcessBaseTradeRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        // Buying
        if (request.Type == "buy_from_trader")
        {
            var foundInRaid = TraderConfig.PurchasesAreFoundInRaid;
            var buyData = (ProcessBuyTradeRequestData)request;
            tradeHelper.BuyItem(pmcData, buyData, sessionID, foundInRaid, output);

            return output;
        }

        // Selling
        if (request.Type == "sell_to_trader")
        {
            var sellData = (ProcessSellTradeRequestData)request;
            tradeHelper.SellItem(pmcData, pmcData, sellData, sessionID, output);

            return output;
        }

        var errorMessage = $"Unhandled trade event: {request.Type}";
        logger.Error(errorMessage);

        return httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.RagfairUnavailable);
    }

    /// <summary>
    ///     Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmRagfairTrading(PmcData pmcData, ProcessRagfairTradeRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        foreach (var offer in request.Offers)
        {
            var fleaOffer = ragfairServer.GetOffer(new MongoId(offer.Id));
            if (fleaOffer is null)
            {
                return httpResponseUtil.AppendErrorToOutput(
                    output,
                    $"Offer with ID: {offer.Id} not found",
                    BackendErrorCodes.OfferNotFound
                );
            }

            if (offer.Count == 0)
            {
                var errorMessage = serverLocalisationService.GetText(
                    "ragfair-unable_to_purchase_0_count_item",
                    itemHelper.GetItem(fleaOffer.Items[0].Template).Value.Name
                );
                return httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.OfferOutOfStock);
            }

            if (fleaOffer.IsTraderOffer())
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
    ///     Buy an item off the flea sold by a trader
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="requestOffer">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    protected void BuyTraderItemFromRagfair(
        MongoId sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest requestOffer,
        ItemEventRouterResponse output
    )
    {
        // Skip buying items when player doesn't have needed loyalty
        if (!pmcData.ProfileMeetsTraderLoyaltyLevelToBuyOffer(fleaOffer))
        {
            var errorMessage =
                $"Unable to buy item: {fleaOffer.Items[0].Template} from trader: {fleaOffer.User.Id} as loyalty level too low, skipping";
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(errorMessage);
            }

            httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.RagfairUnavailable);

            return;
        }

        // Trigger purchase of item from trader
        var buyData = new ProcessBuyTradeRequestData
        {
            Action = "TradingConfirm",
            Type = "buy_from_ragfair_trader",
            TransactionId = fleaOffer.User.Id,
            ItemId = fleaOffer.Root,
            Count = requestOffer.Count,
            SchemeId = 0,
            SchemeItems = requestOffer.Items,
        };
        tradeHelper.BuyItem(pmcData, buyData, sessionId, TraderConfig.PurchasesAreFoundInRaid, output);

        // Remove/lower offer quantity of item purchased from trader flea offer
        ragfairServer.ReduceOfferQuantity(fleaOffer.Id, requestOffer.Count ?? 0);
    }

    /// <summary>
    ///     Buy an item off the flea sold by a PMC
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="requestOffer">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    protected void BuyPmcItemFromRagfair(
        MongoId sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest requestOffer,
        ItemEventRouterResponse output
    )
    {
        var buyData = new ProcessBuyTradeRequestData
        {
            Action = "TradingConfirm",
            Type = "buy_from_ragfair_pmc",
            TransactionId = fleaOffer.User.Id,
            ItemId = fleaOffer.Id, // Store ragfair offerId in buyRequestData.item_id
            Count = requestOffer.Count,
            SchemeId = 0,
            SchemeItems = requestOffer.Items,
        };

        // buyItem() must occur prior to removing the offer stack, otherwise item inside offer doesn't exist for confirmTrading() to use
        tradeHelper.BuyItem(pmcData, buyData, sessionId, RagfairConfig.Dynamic.PurchasesAreFoundInRaid, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // resolve when a profile buy another profile's offer
        var offerOwnerId = fleaOffer.User.Id;
        var offerBuyCount = requestOffer.Count;

        if (fleaOffer.IsPlayerOffer())
        {
            // Complete selling the offer now it has been purchased
            ragfairOfferHelper.CompleteOffer(offerOwnerId, fleaOffer, offerBuyCount ?? 0);

            return;
        }

        // Remove/lower offer quantity of item purchased from PMC flea offer
        ragfairServer.ReduceOfferQuantity(fleaOffer.Id, requestOffer.Count ?? 0);
    }

    /// <summary>
    ///     Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse SellScavItemsToFence(PmcData pmcData, SellScavItemsToFenceRequestData request, MongoId sessionId)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        MailMoneyToPlayer(sessionId, (int)request.TotalValue, Traders.FENCE);

        return output;
    }

    /// <summary>
    ///     Send the specified rouble total to player as mail
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="roublesToSend">amount of roubles to send</param>
    /// <param name="trader">Trader to sell items to</param>
    protected void MailMoneyToPlayer(MongoId sessionId, int roublesToSend, MongoId trader)
    {
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Selling scav items to fence for {roublesToSend} roubles");
        }

        // Create single currency item with all currency on it
        var rootCurrencyReward = new Item
        {
            Id = new MongoId(),
            Template = Money.ROUBLES,
            Upd = new Upd { StackObjectsCount = roublesToSend },
        };

        // Ensure money is properly split to follow its max stack size limit
        var currencyReward = itemHelper.SplitStackIntoSeparateItems(rootCurrencyReward);

        // Send mail from trader
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            trader,
            MessageType.MessageWithItems,
            randomUtil.GetArrayValue(databaseService.GetTrader(trader).Dialogue.TryGetValue("soldItems", out var items) ? items : []),
            currencyReward.SelectMany(x => x).ToList(),
            timeUtil.GetHoursAsSeconds(72)
        );
    }

    /// <summary>
    ///     Looks up an items children and gets total handbook price for them
    /// </summary>
    /// <param name="parentItemId">parent item that has children we want to sum price of</param>
    /// <param name="items">All items (parent + children)</param>
    /// <param name="handbookPrices">Prices of items from handbook</param>
    /// <param name="traderDetails">Trader being sold to, to perform buy category check against</param>
    /// <returns>Rouble price</returns>
    protected int GetPriceOfItemAndChildren(
        MongoId parentItemId,
        IEnumerable<Item> items,
        Dictionary<MongoId, int?> handbookPrices,
        TraderBase traderDetails
    )
    {
        var itemWithChildren = items.GetItemWithChildren(parentItemId);

        var totalPrice = 0;
        foreach (var itemToSell in itemWithChildren)
        {
            var itemDetails = itemHelper.GetItem(itemToSell.Template);
            if (!(itemDetails.Key && itemHelper.IsOfBaseclasses(itemDetails.Value.Id, traderDetails.ItemsBuy.Category)))
            // Skip if tpl isn't item OR item doesn't fulfil match traders buy categories
            {
                continue;
            }

            // Get price of item multiplied by how many are in stack
            totalPrice += (int)((handbookPrices[itemToSell.Template] ?? 0) * (itemToSell.Upd?.StackObjectsCount ?? 1));
        }

        return totalPrice;
    }
}
