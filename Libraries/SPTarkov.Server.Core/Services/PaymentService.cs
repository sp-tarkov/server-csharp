using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

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

    /// <summary>
    ///     Take money and insert items into return to server request
    /// </summary>
    /// <param name="pmcData"> PMC Profile </param>
    /// <param name="request"> Buy item request </param>
    /// <param name="sessionID"> Session ID </param>
    /// <param name="output"> Client response </param>
    public void PayMoney(PmcData pmcData, ProcessBuyTradeRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        // May need to convert to trader currency
        var trader = _traderHelper.GetTrader(request.TransactionId, sessionID);
        var payToTrader = _traderHelper.TraderEnumHasValue(request.TransactionId);

        // Track the amounts of each type of currency involved in the trade.
        var currencyAmounts = new Dictionary<string, double?>();

        // Delete barter items and track currencies
        foreach (var itemRequest in request.SchemeItems)
        {
            // Find the corresponding item in the player's inventory.
            var item = pmcData.Inventory.Items.FirstOrDefault(i => i.Id == itemRequest.Id);
            if (item is not null)
            {
                if (!_paymentHelper.IsMoneyTpl(item.Template))
                {
                    // If the item is not money, remove it from the inventory.
                    _inventoryHelper.RemoveItemByCount(
                        pmcData,
                        item.Id,
                        (int) itemRequest.Count,
                        sessionID,
                        output
                    );
                    itemRequest.Count = 0;
                }
                else
                {
                    // If the item is money, add its count to the currencyAmounts object.
                    // sometimes the currency can be in two parts, so it fails to tryadd the second part
                    if (!currencyAmounts.TryAdd(item.Template, currencyAmounts.GetValueOrDefault(item.Template, 0) + itemRequest.Count))
                    {
                        // if it fails, add the amount to the existing amount
                        currencyAmounts[item.Template] += itemRequest.Count;
                    }
                }
            }
            else
            {
                // Used by `SptInsure`
                // Handle differently, `id` is the money type tpl
                var currencyTpl = itemRequest.Id;
                // sometimes the currency can be in two parts, so it fails to tryadd the second part
                if (!currencyAmounts.TryAdd(currencyTpl, currencyAmounts.GetValueOrDefault(currencyTpl, 0) + itemRequest.Count))
                {
                    // if it fails, add the amount to the existing amount
                    currencyAmounts[currencyTpl] += itemRequest.Count;
                }
            }
        }

        // Track the total amount of all currencies.
        var totalCurrencyAmount = 0d;

        // Loop through each type of currency involved in the trade.
        foreach (var currencyTpl in currencyAmounts)
        {
            if (currencyTpl.Value <= 0)
            {
                continue;
            }

            var currencyAmount = currencyTpl.Value;
            totalCurrencyAmount += currencyAmount.Value;

            // Find money stacks in inventory and remove amount needed + update output object to inform client of changes
            AddPaymentToOutput(pmcData, currencyTpl.Key, currencyAmount.Value, sessionID, output);

            // If there are warnings, exit early.
            if (output.Warnings?.Count > 0)
            {
                return;
            }

            if (payToTrader)
            {
                // Convert the amount to the trader's currency and update the sales sum.
                var costOfPurchaseInCurrency = _handbookHelper.FromRUB(
                    _handbookHelper.InRUB(currencyAmount ?? 0, currencyTpl.Key),
                    _paymentHelper.GetCurrency(trader.Currency)
                );

                // Only update traders
                pmcData.TradersInfo[request.TransactionId].SalesSum += costOfPurchaseInCurrency;
            }
        }

        // If no currency-based payment is involved, handle it separately
        if (totalCurrencyAmount == 0 && payToTrader)
        {
            _logger.Debug(_localisationService.GetText("payment-zero_price_no_payment"));

            // Convert the handbook price to the trader's currency and update the sales sum.
            var costOfPurchaseInCurrency = _handbookHelper.FromRUB(
                GetTraderItemHandbookPriceRouble(request.ItemId, request.TransactionId) ?? 0,
                _paymentHelper.GetCurrency(trader.Currency)
            );

            pmcData.TradersInfo[request.TransactionId].SalesSum += costOfPurchaseInCurrency;
        }

        if (payToTrader)
        {
            _traderHelper.LevelUp(request.TransactionId, pmcData);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug("Item(s) taken. Status OK.");
        }
    }

    /// <summary>
    ///     Get the item price of a specific traders assort
    /// </summary>
    /// <param name="traderAssortId"> ID of the assort to look up</param>
    /// <param name="traderId"> ID of trader with assort </param>
    /// <returns> Handbook rouble price of the item </returns>
    private double? GetTraderItemHandbookPriceRouble(string? traderAssortId, string traderId)
    {
        var purchasedAssortItem = _traderHelper.GetTraderAssortItemByAssortId(traderId, traderAssortId);
        if (purchasedAssortItem is null)
        {
            return 1;
        }

        var assortItemPriceRouble = _handbookHelper.GetTemplatePrice(purchasedAssortItem.Template);
        if (assortItemPriceRouble == 0)
        {
            _logger.Debug($"No item price found for {purchasedAssortItem.Template} on trader: {traderId} in assort: {traderAssortId}");

            return 1;
        }

        return assortItemPriceRouble;
    }

    /// <summary>
    ///     Receive money back after selling
    /// </summary>
    /// <param name="pmcData"> PMC Profile</param>
    /// <param name="amountToSend"> Money to send back </param>
    /// <param name="request"> Sell Trade request data </param>
    /// <param name="output"> Client response </param>
    /// <param name="sessionID"> Session ID </param>
    public void GiveProfileMoney(PmcData pmcData, double? amountToSend, ProcessSellTradeRequestData request,
        ItemEventRouterResponse output, string sessionID)
    {
        var trader = _traderHelper.GetTrader(request.TransactionId, sessionID);
        if (trader is null)
        {
            _logger.Error($"Unable to add currency to profile as trader: {request.TransactionId} does not exist");

            return;
        }

        var currencyTpl = _paymentHelper.GetCurrency(trader.Currency);
        var calcAmount = _handbookHelper.FromRUB(_handbookHelper.InRUB(amountToSend ?? 0, currencyTpl), currencyTpl);
        var currencyMaxStackSize = _itemHelper.GetItem(currencyTpl).Value.Properties?.StackMaxSize;
        if (currencyMaxStackSize is null)
        {
            _logger.Error($"Unable to add currency: {currencyTpl} to profile as it lacks a _props property");

            return;
        }

        var skipSendingMoneyToStash = false;

        foreach (var item in pmcData.Inventory.Items)
        {
            // Item is not currency
            if (item.Template != currencyTpl)
            {
                continue;
            }

            // Item is not in the stash
            if (!_inventoryHelper.IsItemInStash(pmcData, item))
            {
                continue;
            }

            // Found currency item
            if (item.Upd.StackObjectsCount < currencyMaxStackSize)
            {
                if (item.Upd.StackObjectsCount + calcAmount > currencyMaxStackSize)
                {
                    // calculate difference
                    calcAmount -= (int) (currencyMaxStackSize - item.Upd.StackObjectsCount ?? 0);
                    item.Upd.StackObjectsCount = currencyMaxStackSize;
                }
                else
                {
                    skipSendingMoneyToStash = true;
                    item.Upd.StackObjectsCount += calcAmount;
                }

                // Inform client of change to items StackObjectsCount
                output.ProfileChanges[sessionID].Items.ChangedItems.Add(item);

                if (skipSendingMoneyToStash)
                {
                    break;
                }
            }
        }

        // Create single currency item with all currency on it
        var rootCurrencyReward = new Item
        {
            Id = _hashUtil.Generate(),
            Template = currencyTpl,
            Upd = new Upd
            {
                StackObjectsCount = Math.Round(calcAmount)
            }
        };

        // Ensure money is properly split to follow its max stack size limit
        var rewards = _itemHelper.SplitStackIntoSeparateItems(rootCurrencyReward);

        if (!skipSendingMoneyToStash)
        {
            var addItemToStashRequest = new AddItemsDirectRequest
            {
                ItemsWithModsToAdd = rewards,
                FoundInRaid = false,
                Callback = null,
                UseSortingTable = true
            };
            _inventoryHelper.AddItemsToStash(sessionID, addItemToStashRequest, pmcData, output);
        }

        // Calcualte new total sale sum with trader item sold to
        var saleSum = pmcData.TradersInfo[request.TransactionId].SalesSum + amountToSend;

        pmcData.TradersInfo[request.TransactionId].SalesSum = saleSum;
        _traderHelper.LevelUp(request.TransactionId, pmcData);
    }

    /// <summary>
    ///     Remove currency from player stash/inventory and update client object with changes
    /// </summary>
    /// <param name="pmcData"> Player profile to find and remove currency from</param>
    /// <param name="currencyTpl"> Type of currency to pay </param>
    /// <param name="amountToPay"> Money value to pay </param>
    /// <param name="sessionID"> Session ID </param>
    /// <param name="output"> Client response </param>
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
        foreach (var moneyStack in moneyItemsInInventory)
        {
            moneyStack.Upd ??= new Upd
            {
                StackObjectsCount = 1
            };
        }

        var amountAvailable = moneyItemsInInventory.Aggregate(
            0d,
            (accumulator, item) => accumulator + item.Upd.StackObjectsCount.Value
        );

        // If no money in inventory or amount is not enough we return false
        if (moneyItemsInInventory.Count <= 0 || amountAvailable < amountToPay)
        {
            _logger.Error(
                _localisationService.GetText(
                    "payment-not_enough_money_to_complete_transation", // Typo, needs locale updated if fixed
                    new
                    {
                        amountToPay,
                        amountAvailable
                    }
                )
            );
            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("payment-not_enough_money_to_complete_transation_short", amountToPay), // Typo, needs locale updated if fixed
                BackendErrorCodes.UnknownTradingError
            );

            return;
        }

        var leftToPay = amountToPay;
        foreach (var profileMoneyItem in moneyItemsInInventory)
        {
            var itemAmount = profileMoneyItem.Upd.StackObjectsCount;
            if (leftToPay >= itemAmount)
            {
                leftToPay -= itemAmount ?? 0;
                _inventoryHelper.RemoveItem(pmcData, profileMoneyItem.Id, sessionID, output);
            }
            else
            {
                profileMoneyItem.Upd.StackObjectsCount -= leftToPay;
                leftToPay = 0;
                output.ProfileChanges[sessionID].Items.ChangedItems.Add(profileMoneyItem);
            }

            if (leftToPay == 0)
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Get all money stacks in inventory and prioritise items in stash
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="currencyTpl"> Currency to find </param>
    /// <param name="playerStashId"> Players stash ID </param>
    /// <returns> List of sorted money items </returns>
    // TODO - ensure money in containers inside secure container are LAST
    protected List<Item> GetSortedMoneyItemsInInventory(PmcData pmcData, string currencyTpl, string playerStashId)
    {
        var moneyItemsInInventory = _itemHelper.FindBarterItems("tpl", pmcData.Inventory.Items, currencyTpl);
        if (moneyItemsInInventory.Count == 0)
        {
            _logger.Debug($"No {currencyTpl} money items found in inventory");
        }

        // Prioritise items in stash to top of array
        moneyItemsInInventory.Sort((a, b) => PrioritiseStashSort(a, b, pmcData.Inventory.Items, playerStashId));

        return moneyItemsInInventory;
    }

    /// <summary>
    ///     Prioritise player stash first over player inventory.
    ///     Post-raid healing would often take money out of the players pockets/secure container.
    /// </summary>
    /// <param name="a"> First money stack item </param>
    /// <param name="b"> Second money stack item </param>
    /// <param name="inventoryItems"> Players inventory items </param>
    /// <param name="playerStashId"> Players stash ID </param>
    /// <returns> Sort order, -1 if in a, 1 if in b, 0 if they match </returns>
    protected int PrioritiseStashSort(Item a, Item b, List<Item> inventoryItems, string playerStashId)
    {
        // a in root of stash, prioritise
        if (a.ParentId == playerStashId && b.ParentId != playerStashId)
        {
            return -1;
        }

        // b in root stash, prioritise
        if (a.ParentId != playerStashId && b.ParentId == playerStashId)
        {
            return 1;
        }

        // both in containers
        if (a.SlotId == "main" && b.SlotId == "main")
        {
            // Both items are in containers
            var aInStash = IsInStash(a.ParentId, inventoryItems, playerStashId);
            var bInStash = IsInStash(b.ParentId, inventoryItems, playerStashId);

            // a in stash in container, prioritise
            if (aInStash && !bInStash)
            {
                return -1;
            }

            // b in stash in container, prioritise
            if (!aInStash && bInStash)
            {
                return 1;
            }

            // Both in stash in containers
            if (aInStash && bInStash)
            {
                // Containers where taking money from would inconvinence player
                var deprioritisedContainers = _inventoryConfig.DeprioritisedMoneyContainers;
                var aImmediateParent = inventoryItems.FirstOrDefault(item => item.Id == a.ParentId);
                var bImmediateParent = inventoryItems.FirstOrDefault(item => item.Id == b.ParentId);

                // A is not a deprioritised container, B is
                if (
                    !deprioritisedContainers.Contains(aImmediateParent.Template) &&
                    deprioritisedContainers.Contains(bImmediateParent.Template)
                )
                {
                    return -1;
                }

                // B is not a deprioritised container, A is
                if (
                    deprioritisedContainers.Contains(aImmediateParent.Template) &&
                    !deprioritisedContainers.Contains(bImmediateParent.Template)
                )
                {
                    return 1;
                }
            }
        }

        // they match
        return 0;
    }

    /// <summary>
    ///     Recursively check items parents to see if it is inside the players inventory, not stash
    /// </summary>
    /// <param name="itemId"> Item ID to check </param>
    /// <param name="inventoryItems"> Player inventory </param>
    /// <param name="playerStashId"> Players stash ID </param>
    /// <returns> True if it's in inventory </returns>
    protected bool IsInStash(string itemId, List<Item> inventoryItems, string playerStashId)
    {
        var itemParent = inventoryItems.FirstOrDefault(item => item.Id == itemId);

        if (itemParent is not null)
        {
            if (itemParent.SlotId == "hideout")
            {
                return true;
            }

            if (itemParent.Id == playerStashId)
            {
                return true;
            }

            return IsInStash(itemParent.ParentId, inventoryItems, playerStashId);
        }

        return false;
    }
}
