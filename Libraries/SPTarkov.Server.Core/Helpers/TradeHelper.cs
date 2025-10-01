using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class TradeHelper(
    ISptLogger<TradeHelper> logger,
    TraderHelper traderHelper,
    ItemHelper itemHelper,
    QuestHelper questHelper,
    PaymentService paymentService,
    FenceService fenceService,
    ServerLocalisationService serverLocalisationService,
    HttpResponseUtil httpResponseUtil,
    InventoryHelper inventoryHelper,
    RagfairServer ragfairServer,
    TraderAssortHelper traderAssortHelper,
    TraderPurchasePersisterService traderPurchasePersisterService,
    ICloner cloner
)
{
    protected static readonly Lock buyLock = new();

    /// <summary>
    ///     Buy item from flea or trader
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="buyRequestData">data from client</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="foundInRaid">Should item be found in raid</param>
    /// <param name="output">Item event router response</param>
    public void BuyItem(
        PmcData pmcData,
        ProcessBuyTradeRequestData buyRequestData,
        MongoId sessionID,
        bool foundInRaid,
        ItemEventRouterResponse output
    )
    {
        lock (buyLock)
        {
            List<Item> offerItems = [];
            Action<int>? buyCallback;

            if (string.Equals(buyRequestData.Type, "buy_from_ragfair_pmc", StringComparison.OrdinalIgnoreCase))
            {
                // Called when player purchases PMC offer from ragfair
                buyCallback = buyCount =>
                {
                    var allOffers = ragfairServer.GetOffers();

                    // We store ragfair offerId in buyRequestData.item_id
                    var offerWithItem = allOffers.FirstOrDefault(x => x.Id == buyRequestData.ItemId);
                    var itemPurchased = offerWithItem.Items.FirstOrDefault();

                    // Ensure purchase does not exceed trader item limit
                    if (itemPurchased.HasBuyRestrictions())
                    {
                        CheckPurchaseIsWithinTraderItemLimit(
                            sessionID,
                            pmcData,
                            buyRequestData.TransactionId,
                            itemPurchased,
                            buyRequestData.ItemId,
                            buyCount
                        );

                        // Decrement trader item count
                        var itemPurchaseDetails = new PurchaseDetails
                        {
                            Items = [new PurchaseItems { ItemId = buyRequestData.ItemId, Count = buyCount }],
                            TraderId = buyRequestData.TransactionId,
                        };
                        traderHelper.AddTraderPurchasesToPlayerProfile(sessionID, itemPurchaseDetails, itemPurchased);
                    }
                };

                // buyCallback = BuyCallback1;
                // Get raw offer from ragfair, clone to prevent altering offer itself
                var allOffers = ragfairServer.GetOffers();
                var offerWithItemCloned = cloner.Clone(allOffers.FirstOrDefault(x => x.Id == buyRequestData.ItemId));
                offerItems = offerWithItemCloned.Items;
            }
            else if (buyRequestData.TransactionId == Traders.FENCE)
            {
                buyCallback = buyCount =>
                {
                    // Update assort/flea item values
                    var traderAssorts = traderHelper.GetTraderAssortsByTraderId(buyRequestData.TransactionId).Items;
                    var itemPurchased = traderAssorts.FirstOrDefault(assort => assort.Id == buyRequestData.ItemId);

                    // Decrement trader item count
                    itemPurchased.Upd.StackObjectsCount -= buyCount;

                    fenceService.AmendOrRemoveFenceOffer(buyRequestData.ItemId, buyCount);
                };

                var fenceItems = fenceService.GetRawFenceAssorts().Items;
                var rootItemIndex = fenceItems.FindIndex(item => item.Id == buyRequestData.ItemId);
                if (rootItemIndex == -1)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Tried to buy item {buyRequestData.ItemId} from fence that no longer exists");
                    }

                    var message = serverLocalisationService.GetText("ragfair-offer_no_longer_exists");
                    httpResponseUtil.AppendErrorToOutput(output, message);

                    return;
                }

                offerItems = fenceItems.GetItemWithChildren(buyRequestData.ItemId);
            }
            else
            {
                // Can only be trader purchase
                buyCallback = buyCount =>
                {
                    // Update assort/flea item values
                    var traderAssorts = traderHelper.GetTraderAssortsByTraderId(buyRequestData.TransactionId).Items;
                    var itemPurchased = traderAssorts.FirstOrDefault(item => item.Id == buyRequestData.ItemId);

                    // Ensure purchase does not exceed trader item limit
                    if (itemPurchased.HasBuyRestrictions())
                    // Will throw error if check fails
                    {
                        CheckPurchaseIsWithinTraderItemLimit(
                            sessionID,
                            pmcData,
                            buyRequestData.TransactionId,
                            itemPurchased,
                            buyRequestData.ItemId,
                            buyCount
                        );
                    }

                    // Check if trader has enough stock
                    if (itemPurchased.Upd.StackObjectsCount < buyCount)
                    {
                        throw new Exception(
                            $"Unable to purchase {buyCount} items, this would exceed the remaining stock left {itemPurchased.Upd.StackObjectsCount} from the traders assort: {buyRequestData.TransactionId} this refresh"
                        );
                    }

                    // Decrement trader item count
                    itemPurchased.Upd.StackObjectsCount -= buyCount;

                    if (itemPurchased.HasBuyRestrictions())
                    {
                        var itemPurchaseDat = new PurchaseDetails
                        {
                            Items = [new PurchaseItems { ItemId = buyRequestData.ItemId, Count = buyCount }],
                            TraderId = buyRequestData.TransactionId,
                        };

                        traderHelper.AddTraderPurchasesToPlayerProfile(sessionID, itemPurchaseDat, itemPurchased);
                    }
                };

                // Get all trader assort items
                var traderItems = traderAssortHelper.GetAssort(sessionID, buyRequestData.TransactionId).Items;

                // Get item + children for purchase
                var relevantItems = traderItems.GetItemWithChildren(buyRequestData.ItemId);
                if (relevantItems.Count == 0)
                {
                    logger.Error($"Purchased trader: {buyRequestData.TransactionId} offer: {buyRequestData.ItemId} has no items");
                }

                offerItems.AddRange(relevantItems);
            }

            // Get item details from db
            var itemDbDetails = itemHelper.GetItem(offerItems.FirstOrDefault().Template).Value;
            var itemMaxStackSize = itemDbDetails.Properties.StackMaxSize;
            var itemsToSendTotalCount = buyRequestData.Count;
            var itemsToSendRemaining = itemsToSendTotalCount;

            // Construct array of items to send to player
            List<List<Item>> itemsToSendToPlayer = [];
            while (itemsToSendRemaining > 0)
            {
                var offerClone = cloner.Clone(offerItems);
                // Handle stackable items that have a max stack size limit
                var itemCountToSend = Math.Min(itemMaxStackSize ?? 0, itemsToSendRemaining.Value);
                offerClone.FirstOrDefault().Upd.StackObjectsCount = itemCountToSend;

                // Prevent any collisions
                offerClone.RemapRootItemId();
                if (offerClone.Count > 1)
                {
                    itemHelper.ReparentItemAndChildren(offerClone.FirstOrDefault(), offerClone);
                }

                itemsToSendToPlayer.Add(offerClone);

                // Remove amount of items added to player stash
                itemsToSendRemaining -= itemCountToSend;
            }

            // Construct request
            var request = new AddItemsDirectRequest
            {
                ItemsWithModsToAdd = itemsToSendToPlayer,
                FoundInRaid = foundInRaid,
                Callback = buyCallback,
                UseSortingTable = false,
            };

            // Add items + their children to stash
            inventoryHelper.AddItemsToStash(sessionID, request, pmcData, output);
            if (output.Warnings?.Count > 0)
            {
                return;
            }

            // Pay for purchase
            paymentService.PayMoney(pmcData, buyRequestData, sessionID, output);
            if (output.Warnings?.Count > 0)
            {
                var errorMessage = $"Transaction failed: {output.Warnings.FirstOrDefault().ErrorMessage}";
                httpResponseUtil.AppendErrorToOutput(output, errorMessage, BackendErrorCodes.UnknownTradingError);
            }
        }
    }

    /// <summary>
    ///     Sell item to trader
    /// </summary>
    /// <param name="profileWithItemsToSell">Profile to remove items from</param>
    /// <param name="profileToReceiveMoney">Profile to accept the money for selling item</param>
    /// <param name="sellRequest">Request data</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="output">Item event router response</param>
    public void SellItem(
        PmcData profileWithItemsToSell,
        PmcData profileToReceiveMoney,
        ProcessSellTradeRequestData sellRequest,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        // Check for and increment SoldToTrader condition counters
        questHelper.IncrementSoldToTraderCounters(profileWithItemsToSell, profileToReceiveMoney, sellRequest);

        const string pattern = @"\s+";

        // Find item in inventory and remove it
        foreach (var itemToBeRemoved in sellRequest.Items)
        {
            var itemIdToFind = Regex.Replace(itemToBeRemoved.Id, pattern, ""); // Strip out whitespace
            // Find item in player inventory, or show error to player if not found
            var matchingItemInInventory = profileWithItemsToSell.Inventory.Items.FirstOrDefault(x => x.Id == itemIdToFind);
            if (matchingItemInInventory is null)
            {
                var errorMessage = $"Unable to sell item {itemToBeRemoved.Id}, cannot be found in player inventory";
                logger.Error(errorMessage);

                httpResponseUtil.AppendErrorToOutput(output, errorMessage);

                return;
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Selling: id: {matchingItemInInventory.Id} tpl: {matchingItemInInventory.Template}");
            }

            if (sellRequest.TransactionId == Traders.FENCE)
            {
                fenceService.AddItemsToFenceAssort(profileWithItemsToSell.Inventory.Items, matchingItemInInventory);
            }

            // Remove item from inventory + any child items it has
            inventoryHelper.RemoveItem(profileWithItemsToSell, itemToBeRemoved.Id, sessionID, output);
        }

        // Give player money for sold item(s)
        paymentService.GiveProfileMoney(profileToReceiveMoney, sellRequest.Price, sellRequest, output, sessionID);
    }

    /// <summary>
    ///     Traders allow a limited number of purchases per refresh cycle (default 60 mins)
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile making the purchase</param>
    /// <param name="traderId">Trader assort is purchased from</param>
    /// <param name="assortBeingPurchased">the item from trader being bought</param>
    /// <param name="assortId">Id of assort being purchased</param>
    /// <param name="count">How many of the item are being bought</param>
    protected void CheckPurchaseIsWithinTraderItemLimit(
        MongoId sessionId,
        PmcData pmcData,
        MongoId traderId,
        Item assortBeingPurchased,
        MongoId assortId,
        double count
    )
    {
        var traderPurchaseData = traderPurchasePersisterService.GetProfileTraderPurchase(sessionId, traderId, assortBeingPurchased.Id);
        var traderItemPurchaseLimit = traderHelper.GetAccountTypeAdjustedTraderPurchaseLimit(
            (double)assortBeingPurchased.Upd?.BuyRestrictionMax,
            pmcData.Info.GameVersion
        );
        if ((traderPurchaseData?.PurchaseCount ?? 0) + count > traderItemPurchaseLimit)
        {
            throw new Exception(
                $"Unable to purchase: {count} items, this would exceed your purchase limit of {traderItemPurchaseLimit} from the trader: {traderId} assort: {assortId} this refresh"
            );
        }
    }
}

public record PurchaseDetails
{
    public List<PurchaseItems> Items { get; set; }

    public MongoId TraderId { get; set; }
}

public record PurchaseItems
{
    public MongoId ItemId { get; set; }

    public double Count { get; set; }
}
