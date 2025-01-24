using Core.Helpers;
using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PaymentService(
    ISptLogger<PaymentService> _logger,
    HashUtil _hashUtil,
    HttpResponseUtil _httpResponseUtil,
    DatabaseService _databaseService,
    HandbookHelper _handbookHelper,
    TraderHelper _traderHelper,
    ItemHelper _itemHelper,
    InventoryHelper _inventoryHelper,
    LocalisationService _localisationService,
    PaymentHelper _paymentHelper,
    ConfigServer _configServer
)
{
    protected InventoryConfig _inventoryConfig = _configServer.GetConfig<InventoryConfig>();

    /**
     * Take money and insert items into return to server request
     * @param pmcData Pmc profile
     * @param request Buy item request
     * @param sessionID Session id
     * @param output Client response
     */
    public void PayMoney(PmcData pmcData, ProcessBuyTradeRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        // May need to convert to trader currency
        var trader = _traderHelper.GetTrader(request.TransactionId, sessionID);
        var payToTrader = _traderHelper.TraderEnumHasValue(request.TransactionId);

        // Track the amounts of each type of currency involved in the trade.
         Dictionary<string, double?> currencyAmounts = new Dictionary<string, double?>();

        // Delete barter items and track currencies
        foreach (var itemRequest in request.SchemeItems) {
            // Find the corresponding item in the player's inventory.
            var item = pmcData.Inventory.Items.FirstOrDefault((i) => i.Id == itemRequest.Id);
            if (item is not null) {
                if (!_paymentHelper.IsMoneyTpl(item.Template)) {
                    // If the item is not money, remove it from the inventory.
                    _inventoryHelper.RemoveItemByCount(
                        pmcData,
                        item.Id,
                        (int)itemRequest.Count,
                        sessionID,
                        output
                    );
                    itemRequest.Count = 0;
                } else {
                    // If the item is money, add its count to the currencyAmounts object.
                    currencyAmounts.TryAdd(item.Template, (currencyAmounts.GetValueOrDefault(item.Template, 0)) + itemRequest.Count);
                }
            } else {
                // Used by `SptInsure`
                // Handle differently, `id` is the money type tpl
                var currencyTpl = itemRequest.Id;
                currencyAmounts.TryAdd(currencyTpl, (currencyAmounts.GetValueOrDefault(currencyTpl, 0)) + itemRequest.Count);
            }
        }

        // Track the total amount of all currencies.
        var totalCurrencyAmount = 0;

        // Loop through each type of currency involved in the trade.
        foreach (var currencyTpl in currencyAmounts) {
            var currencyAmount = currencyTpl.Value;
            totalCurrencyAmount += (int)currencyAmount;

            if (currencyAmount > 0) {
                // Find money stacks in inventory and remove amount needed + update output object to inform client of changes
                AddPaymentToOutput(pmcData, currencyTpl.Key, (int)currencyAmount, sessionID, output);

                // If there are warnings, exit early.
                if (output.Warnings?.Count > 0) {
                    return;
                }

                if (payToTrader) {
                    // Convert the amount to the trader's currency and update the sales sum.
                    var costOfPurchaseInCurrency = _handbookHelper.FromRUB(
                        _handbookHelper.InRUB(currencyAmount ?? 0, currencyTpl.Key),
                        _paymentHelper.GetCurrency(trader.Currency)
                    );

                    // Only update traders
                    pmcData.TradersInfo[request.TransactionId].SalesSum += costOfPurchaseInCurrency;
                }
            }
        }

        // If no currency-based payment is involved, handle it separately
        if (totalCurrencyAmount == 0 && payToTrader) {
            _logger.Debug(_localisationService.GetText("payment-zero_price_no_payment"));

            // Convert the handbook price to the trader's currency and update the sales sum.
            var costOfPurchaseInCurrency = _handbookHelper.FromRUB(
                GetTraderItemHandbookPriceRouble(request.ItemId, request.TransactionId) ?? 0,
                _paymentHelper.GetCurrency(trader.Currency)
            );

            pmcData.TradersInfo[request.TransactionId].SalesSum += costOfPurchaseInCurrency;
        }

        if (payToTrader) {
            _traderHelper.LevelUp(request.TransactionId, pmcData);
        }

        _logger.Debug("Item(s) taken. Status OK.");
    }

    private double? GetTraderItemHandbookPriceRouble(string? traderAssortId, string traderId)
    {
        var purchasedAssortItem = _traderHelper.GetTraderAssortItemByAssortId(traderId, traderAssortId);
        if (purchasedAssortItem is null) {
            return 1;
        }

        var assortItemPriceRouble = _handbookHelper.GetTemplatePrice(purchasedAssortItem.Template);
        if (assortItemPriceRouble is null) {
            _logger.Debug($"No item price found for {purchasedAssortItem.Template} on trader: {traderId} in assort: {traderAssortId}");

            return 1;
        }

        return assortItemPriceRouble;
    }
    
    public void GiveProfileMoney(PmcData pmcData, double? amountToSend, ProcessSellTradeRequestData request,
        ItemEventRouterResponse output, string sessionID)
    {
        var trader = _traderHelper.GetTrader(request.TransactionId, sessionID);
        if (trader is null) {
            _logger.Error($"Unable to add currency to profile as trader: {request.TransactionId} does not exist");

            return;
        }

        var currencyTpl = _paymentHelper.GetCurrency(trader.Currency);
        var calcAmount = _handbookHelper.FromRUB(_handbookHelper.InRUB(amountToSend ?? 0, currencyTpl), currencyTpl);
        var currencyMaxStackSize = _itemHelper.GetItem(currencyTpl).Value.Properties?.StackMaxSize;
        if (currencyMaxStackSize is null) {
            _logger.Error($"Unable to add currency: {currencyTpl} to profile as it lacks a _props property");

            return;
        }
        var skipSendingMoneyToStash = false;

        foreach (var item in pmcData.Inventory.Items) {
            // Item is not currency
            if (item.Template != currencyTpl) {
                continue;
            }

            // Item is not in the stash
            if (!_inventoryHelper.IsItemInStash(pmcData, item)) {
                continue;
            }

            // Found currency item
            if (item.Upd.StackObjectsCount < currencyMaxStackSize) {
                if (item.Upd.StackObjectsCount + calcAmount > currencyMaxStackSize) {
                    // calculate difference
                    calcAmount -= (int) ((currencyMaxStackSize - item.Upd.StackObjectsCount) ?? 0);
                    item.Upd.StackObjectsCount = currencyMaxStackSize;
                } else {
                    skipSendingMoneyToStash = true;
                    item.Upd.StackObjectsCount += calcAmount;
                }

                // Inform client of change to items StackObjectsCount
                output.ProfileChanges[sessionID].Items.ChangedItems.Add(item);

                if (skipSendingMoneyToStash) {
                    break;
                }
            }
        }

        // Create single currency item with all currency on it
        Item rootCurrencyReward = new Item {
            Id = _hashUtil.Generate(),
            Template = currencyTpl,
            Upd = new Upd { StackObjectsCount = Math.Round((double) calcAmount) }
        };

        // Ensure money is properly split to follow its max stack size limit
        var rewards = _itemHelper.SplitStackIntoSeparateItems(rootCurrencyReward);

        if (!skipSendingMoneyToStash) {
            AddItemsDirectRequest addItemToStashRequest = new AddItemsDirectRequest {
                ItemsWithModsToAdd = rewards,
                FoundInRaid = false,
                Callback = null,
                UseSortingTable = true,
            };
            _inventoryHelper.AddItemsToStash(sessionID, addItemToStashRequest, pmcData, output);
        }

        // Calcualte new total sale sum with trader item sold to
        var saleSum = pmcData.TradersInfo[request.TransactionId].SalesSum + amountToSend;

        pmcData.TradersInfo[request.TransactionId].SalesSum = saleSum;
        _traderHelper.LevelUp(request.TransactionId, pmcData);
    }

    /**
     * Remove currency from player stash/inventory and update client object with changes
     * @param pmcData Player profile to find and remove currency from
     * @param currencyTpl Type of currency to pay
     * @param amountToPay money value to pay
     * @param sessionID Session id
     * @param output output object to send to client
     */
    public void AddPaymentToOutput(
        PmcData pmcData,
        string currencyTpl,
        double amountToPay,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        var moneyItemsInInventory = GetSortedMoneyItemsInInventory(
            pmcData,
            currencyTpl,
            pmcData.Inventory.Stash
        );

        //Ensure all money items found have a upd
        foreach (var moneyStack in moneyItemsInInventory) {
            moneyStack.Upd ??= new Upd { StackObjectsCount = 1 };
        }

        var amountAvailable = moneyItemsInInventory.Aggregate(0, 
            (accumulator, item) => (int)(accumulator + item.Upd.StackObjectsCount)
        );

        // If no money in inventory or amount is not enough we return false
        if (moneyItemsInInventory.Count <= 0 || amountAvailable < amountToPay) {
            _logger.Error(
                _localisationService.GetText("payment-not_enough_money_to_complete_transation", new {
                amountToPay = amountToPay,
                amountAvailable = amountAvailable,
            })
            );
            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("payment-not_enough_money_to_complete_transation_short", amountToPay),
                BackendErrorCodes.UnknownTradingError
            );

            return;
        }

        var leftToPay = amountToPay;
        foreach (var profileMoneyItem in moneyItemsInInventory) {
            var itemAmount = profileMoneyItem.Upd.StackObjectsCount;
            if (leftToPay >= itemAmount) {
                leftToPay -= itemAmount ?? 0;
                _inventoryHelper.RemoveItem(pmcData, profileMoneyItem.Id, sessionID, output);
            } else {
                profileMoneyItem.Upd.StackObjectsCount -= leftToPay;
                leftToPay = 0;
                output.ProfileChanges[sessionID].Items.ChangedItems.Add(profileMoneyItem);
            }

            if (leftToPay == 0) {
                break;
            }
        }
    }

    /**
     * TODO - ensure money in containers inside secure container are LAST
     * Get all money stacks in inventory and prioritise items in stash
     * @param pmcData Player profile
     * @param currencyTpl
     * @param playerStashId Players stash id
     * @returns Sorting money items
     */
    protected List<Item> GetSortedMoneyItemsInInventory(PmcData pmcData, string currencyTpl, string playerStashId)
    {
        var moneyItemsInInventory = _itemHelper.FindBarterItems("tpl", pmcData.Inventory.Items, currencyTpl);
        if (moneyItemsInInventory.Count == 0) {
            _logger.Debug($"No {currencyTpl} money items found in inventory");
        }

        // Prioritise items in stash to top of array
        moneyItemsInInventory.Sort((a, b) => PrioritiseStashSort(a, b, pmcData.Inventory.Items, playerStashId));

        return moneyItemsInInventory;
    }

    /**
     * Prioritise player stash first over player inventory
     * Post-raid healing would often take money out of the players pockets/secure container
     * @param a First money stack item
     * @param b Second money stack item
     * @param inventoryItems players inventory items
     * @param playerStashId Players stash id
     * @returns sort order
     */
    protected int PrioritiseStashSort(Item a, Item b, List<Item> inventoryItems, string playerStashId)
    {
        // a in root of stash, prioritise
        if (a.ParentId == playerStashId && b.ParentId != playerStashId) {
            return -1;
        }

        // b in root stash, prioritise
        if (a.ParentId != playerStashId && b.ParentId == playerStashId) {
            return 1;
        }

        // both in containers
        if (a.SlotId == "main" && b.SlotId == "main") {
            // Both items are in containers
            var aInStash = this.IsInStash(a.ParentId, inventoryItems, playerStashId);
            var bInStash = this.IsInStash(b.ParentId, inventoryItems, playerStashId);

            // a in stash in container, prioritise
            if (aInStash && !bInStash) {
                return -1;
            }

            // b in stash in container, prioritise
            if (!aInStash && bInStash) {
                return 1;
            }

            // Both in stash in containers
            if (aInStash && bInStash) {
                // Containers where taking money from would inconvinence player
                var deprioritisedContainers = _inventoryConfig.DeprioritisedMoneyContainers;
                var aImmediateParent = inventoryItems.FirstOrDefault((item) => item.Id == a.ParentId);
                var bImmediateParent = inventoryItems.FirstOrDefault((item) => item.Id == b.ParentId);

                // A is not a deprioritised container, B is
                if (
                    !deprioritisedContainers.Contains(aImmediateParent.Template) &&
                    deprioritisedContainers.Contains(bImmediateParent.Template)
                ) {
                    return -1;
                }

                // B is not a deprioritised container, A is
                if (
                    deprioritisedContainers.Contains(aImmediateParent.Template) &&
                    !deprioritisedContainers.Contains(bImmediateParent.Template)
                ) {
                    return 1;
                }
            }
        }

        // they match
        return 0;
    }

    /**
     * Recursively check items parents to see if it is inside the players inventory, not stash
     * @param itemId item id to check
     * @param inventoryItems player inventory
     * @param playerStashId Players stash id
     * @returns true if its in inventory
     */
    protected bool IsInStash(string itemId, List<Item> inventoryItems, string playerStashId)
    {
        var itemParent = inventoryItems.FirstOrDefault((item) => item.Id == itemId);

        if (itemParent is not null) {
            if (itemParent.SlotId == "hideout") {
                return true;
            }

            if (itemParent.Id == playerStashId) {
                return true;
            }

            return IsInStash(itemParent.ParentId, inventoryItems, playerStashId);
        }

        return false;
    }
}
