using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
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
    ISptLogger<PaymentService> logger,
    HttpResponseUtil httpResponseUtil,
    HandbookHelper handbookHelper,
    TraderHelper traderHelper,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    ServerLocalisationService serverLocalisationService,
    PaymentHelper paymentHelper,
    ConfigServer configServer
)
{
    protected readonly InventoryConfig InventoryConfig = configServer.GetConfig<InventoryConfig>();

    /// <summary>
    ///     Take money and insert items into return to server request
    /// </summary>
    /// <param name="pmcData"> PMC Profile </param>
    /// <param name="request"> Buy item request </param>
    /// <param name="sessionID"> Session ID </param>
    /// <param name="output"> Client response </param>
    public void PayMoney(PmcData pmcData, ProcessBuyTradeRequestData request, MongoId sessionID, ItemEventRouterResponse output)
    {
        // Track the amounts of each type of currency involved in the trade.
        var currencyAmounts = new Dictionary<MongoId, double>();

        // Delete barter items and track currencies
        foreach (var itemRequest in request.SchemeItems)
        {
            // Find the corresponding item in the player's inventory.
            var item = pmcData.Inventory.Items.FirstOrDefault(i => i.Id == itemRequest.Id);
            if (item is not null)
            {
                if (!paymentHelper.IsMoneyTpl(item.Template))
                {
                    // If the item is not money, remove it from the inventory.
                    inventoryHelper.RemoveItemByCount(pmcData, item.Id, (int)itemRequest.Count, sessionID, output);
                    itemRequest.Count = 0;
                }
                else
                {
                    // If the item is money, add its count to the currencyAmounts object.
                    // sometimes the currency can be in two parts, so it fails to tryadd the second part
                    currencyAmounts.AddOrUpdate(item.Template, itemRequest.Count.Value);
                }
            }
            else
            {
                // Used by `SptInsure`
                // Handle differently, `id` is the money type tpl
                var currencyTpl = itemRequest.Id;
                // Sometimes the currency can be in two parts, so it fails to tryadd the second part
                currencyAmounts.AddOrUpdate(currencyTpl, itemRequest.Count.Value);
            }
        }

        // Track the total amount of all currencies.
        var totalCurrencyAmount = 0d;

        var requestTransactionId = new MongoId(request.TransactionId);

        // Who is recipient of money player is sending
        var payToTrader = request.Type == "buy_from_ragfair_trader" && traderHelper.TraderExists(requestTransactionId);

        // May need to convert to trader currency
        var trader = payToTrader ? traderHelper.GetTrader(requestTransactionId, sessionID) : new TraderBase { Currency = CurrencyType.RUB }; // TODO: cleanup

        // Loop through each type of currency involved in the trade
        foreach (var (currencyTpl, currencyAmount) in currencyAmounts)
        {
            if (currencyAmount <= 0)
            {
                continue;
            }

            totalCurrencyAmount += currencyAmount;

            // Find money stacks in inventory and remove amount needed + update output object to inform client of changes
            AddPaymentToOutput(pmcData, currencyTpl, currencyAmount, sessionID, output);

            // If there are warnings, exit early
            if (output.Warnings?.Count > 0)
            {
                return;
            }

            if (payToTrader)
            {
                // Convert the amount to the trader's currency and update the sales sum
                var costOfPurchaseInCurrency = handbookHelper.FromRoubles(
                    handbookHelper.InRoubles(currencyAmount, currencyTpl),
                    trader.Currency.Value.GetCurrencyTpl()
                );

                // Only update traders
                pmcData.TradersInfo[requestTransactionId].SalesSum += costOfPurchaseInCurrency;
            }
        }

        // If no currency-based payment is involved, handle it separately
        if (totalCurrencyAmount == 0 && payToTrader)
        {
            logger.Debug(serverLocalisationService.GetText("payment-zero_price_no_payment"));

            // Convert the handbook price to the trader's currency and update the sales sum.
            var costOfPurchaseInCurrency = handbookHelper.FromRoubles(
                GetTraderItemHandbookPriceRouble(request.ItemId, requestTransactionId) ?? 0,
                trader.Currency.Value.GetCurrencyTpl()
            );

            pmcData.TradersInfo[requestTransactionId].SalesSum += costOfPurchaseInCurrency;
        }

        if (payToTrader)
        {
            traderHelper.LevelUp(requestTransactionId, pmcData);
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug("Item(s) taken. Status OK.");
        }
    }

    /// <summary>
    ///     Get the item price of a specific traders assort
    /// </summary>
    /// <param name="traderAssortId"> ID of the assort to look up</param>
    /// <param name="traderId"> ID of trader with assort </param>
    /// <returns> Handbook rouble price of the item </returns>
    private double? GetTraderItemHandbookPriceRouble(MongoId traderAssortId, MongoId traderId)
    {
        var purchasedAssortItem = traderHelper.GetTraderAssortItemByAssortId(traderId, traderAssortId);
        if (purchasedAssortItem is null)
        {
            return 1;
        }

        var assortItemPriceRouble = handbookHelper.GetTemplatePrice(purchasedAssortItem.Template);
        if (assortItemPriceRouble == 0)
        {
            logger.Debug($"No item price found for {purchasedAssortItem.Template} on trader: {traderId} in assort: {traderAssortId}");

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
    public void GiveProfileMoney(
        PmcData pmcData,
        double? amountToSend,
        ProcessSellTradeRequestData request,
        ItemEventRouterResponse output,
        MongoId sessionID
    )
    {
        var trader = traderHelper.GetTrader(request.TransactionId, sessionID);
        if (trader is null)
        {
            logger.Error($"Unable to add currency to profile as trader: {request.TransactionId} does not exist");

            return;
        }

        var currencyTpl = trader.Currency.Value.GetCurrencyTpl();
        var calcAmount = handbookHelper.FromRoubles(handbookHelper.InRoubles(amountToSend ?? 0, currencyTpl), currencyTpl);
        var currencyMaxStackSize = itemHelper.GetItem(currencyTpl).Value.Properties?.StackMaxSize;
        if (currencyMaxStackSize is null)
        {
            logger.Error($"Unable to add currency: {currencyTpl} to profile as it lacks a _props property");

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
            if (!pmcData.IsItemInStash(item))
            {
                continue;
            }

            // Found currency item
            if (item.Upd?.StackObjectsCount < currencyMaxStackSize)
            {
                if (item.Upd.StackObjectsCount + calcAmount > currencyMaxStackSize)
                {
                    // calculate difference
                    calcAmount -= (int)(currencyMaxStackSize - item.Upd.StackObjectsCount);
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
            Id = new MongoId(),
            Template = currencyTpl,
            Upd = new Upd { StackObjectsCount = Math.Round(calcAmount) },
        };

        // Ensure money is properly split to follow its max stack size limit
        var rewards = itemHelper.SplitStackIntoSeparateItems(rootCurrencyReward);

        if (!skipSendingMoneyToStash)
        {
            var addItemToStashRequest = new AddItemsDirectRequest
            {
                ItemsWithModsToAdd = rewards,
                FoundInRaid = false,
                Callback = null,
                UseSortingTable = true,
            };
            inventoryHelper.AddItemsToStash(sessionID, addItemToStashRequest, pmcData, output);
        }

        // Calcualte new total sale sum with trader item sold to
        var saleSum = pmcData.TradersInfo[request.TransactionId].SalesSum + amountToSend;

        pmcData.TradersInfo[request.TransactionId].SalesSum = saleSum;
        traderHelper.LevelUp(request.TransactionId, pmcData);
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
        MongoId currencyTpl,
        double amountToPay,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        var moneyItemsInInventory = GetSortedMoneyItemsInInventory(pmcData, currencyTpl, pmcData.Inventory.Stash.Value);

        //Ensure all money items found have a upd
        foreach (var moneyStack in moneyItemsInInventory)
        {
            moneyStack.Upd ??= new Upd { StackObjectsCount = 1 };
        }

        var amountAvailable = moneyItemsInInventory.Aggregate(0d, (accumulator, item) => accumulator + item.Upd.StackObjectsCount.Value);

        // If no money in inventory or amount is not enough we return false
        if (moneyItemsInInventory.Count <= 0 || amountAvailable < amountToPay)
        {
            logger.Error(
                serverLocalisationService.GetText(
                    "payment-not_enough_money_to_complete_transation", // Typo, needs locale updated if fixed
                    new { amountToPay, amountAvailable }
                )
            );
            httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("payment-not_enough_money_to_complete_transation_short", amountToPay), // Typo, needs locale updated if fixed
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
                leftToPay -= itemAmount.Value;
                inventoryHelper.RemoveItem(pmcData, profileMoneyItem.Id, sessionID, output);
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
    /// Get all money stacks in inventory and prioritise items in stash
    /// Ignore locked stacks
    /// Prioritise the lowest sized stack
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="currencyTpl"> Currency to find </param>
    /// <param name="playerStashId"> Players stash ID </param>
    /// <returns> List of sorted money items </returns>
    // TODO - ensure money in containers inside secure container are LAST
    protected List<Item> GetSortedMoneyItemsInInventory(PmcData pmcData, MongoId currencyTpl, MongoId playerStashId)
    {
        // Get money stacks player has
        var moneyItemsInInventory = itemHelper.FindBarterItems("tpl", pmcData.Inventory.Items, [currencyTpl]);
        if (moneyItemsInInventory.Count == 0)
        {
            logger.Debug($"No {currencyTpl} money items found in inventory");

            return moneyItemsInInventory;
        }

        // Create a cache inventory items with a bool of being in stash or not
        var itemsInStashCache = GetItemInStashCache(pmcData.Inventory.Items, playerStashId);

        // Filter out 'Locked' money stacks as they cannot be used
        var noLocked = moneyItemsInInventory.Where(moneyItem => moneyItem.Upd.PinLockState != PinLockState.Locked);
        if (noLocked.Any())
        {
            // We found unlocked money
            moneyItemsInInventory = noLocked.ToList();
        }

        // Prioritise items in stash to top of array
        moneyItemsInInventory.Sort((a, b) => PrioritiseStashSort(a, b, pmcData.Inventory.Items, itemsInStashCache));

        return moneyItemsInInventory;
    }

    /// <summary>
    /// Create a dictionary of all items from player inventory that are in the players stash
    /// </summary>
    /// <param name="items">Inventory items to check</param>
    /// <param name="playerStashId">Id of players stash</param>
    /// <returns>Dictionary</returns>
    protected IReadOnlyDictionary<MongoId, InventoryLocation> GetItemInStashCache(List<Item> items, MongoId playerStashId)
    {
        var itemsInStashCache = new Dictionary<MongoId, InventoryLocation>();
        foreach (var inventoryItem in items)
        {
            itemsInStashCache.TryAdd(inventoryItem.Id, GetItemLocation(inventoryItem.Id, items, playerStashId));
        }

        return itemsInStashCache;
    }

    /// <summary>
    /// Get stacks of money from player inventory, ordered by priority to use from
    /// Post-raid healing would often take money out of the players pockets/secure container.
    /// Return money stacks in root of stash first, with the smallest stacks taking priority
    /// Stacks inside secure are returned last
    /// </summary>
    /// <param name="a"> First money stack item </param>
    /// <param name="b"> Second money stack item </param>
    /// <param name="inventoryItems"> Players inventory items</param>
    /// <param name="itemInStashCache">Cache of item IDs and if they're in stash</param>
    /// <returns> Sort order, -1 if A has priority, 1 if B has priority, 0 if they match </returns>
    protected int PrioritiseStashSort(
        Item a,
        Item b,
        List<Item> inventoryItems,
        IReadOnlyDictionary<MongoId, InventoryLocation> itemInStashCache
    )
    {
        // Get the location of A and B
        itemInStashCache.TryGetValue(a.Id, out var aLocation);
        itemInStashCache.TryGetValue(b.Id, out var bLocation);

        // Helper fields
        var aInStash = aLocation == InventoryLocation.Stash;
        var bInStash = bLocation == InventoryLocation.Stash;
        var aInSecure = aLocation == InventoryLocation.Secure;
        var bInSecure = bLocation == InventoryLocation.Secure;
        var onlyAInStash = aInStash && !bInStash;
        var onlyBInStash = !aInStash && bInStash;
        var bothInStash = aInStash && bInStash;

        if (bothInStash)
        {
            // Determine if they're in containers
            var aInContainer = string.Equals(a.SlotId, "main", StringComparison.InvariantCultureIgnoreCase);
            var bInContainer = string.Equals(b.SlotId, "main", StringComparison.InvariantCultureIgnoreCase);

            // Return item not in container
            var compare = aInContainer.CompareTo(bInContainer);
            if (compare != 0)
            {
                return compare;
            }

            // Both in containers, deprioritized item in 'bad' container
            if (aInContainer && bInContainer)
            {
                // Containers where taking money from would inconvenience player
                var aImmediateParent = inventoryItems.FirstOrDefault(item => item.Id == a.ParentId);
                var bImmediateParent = inventoryItems.FirstOrDefault(item => item.Id == b.ParentId);

                var aInDeprioContainer = InventoryConfig.DeprioritisedMoneyContainers.Contains(aImmediateParent.Template);
                var bInDeprioContainer = InventoryConfig.DeprioritisedMoneyContainers.Contains(bImmediateParent.Template);

                // Prioritize B
                if (!aInDeprioContainer && bInDeprioContainer)
                {
                    return -1;
                }

                // Prioritize A
                if (aInDeprioContainer && !bInDeprioContainer)
                {
                    return 1;
                }

                // Both in bad containers, fall out of IF and run GetPriorityBySmallestStackSize
            }

            return GetPriorityBySmallestStackSize(a, b);
        }

        // Prioritise A
        if (onlyAInStash)
        {
            return -1;
        }

        // Prioritise B
        if (onlyBInStash)
        {
            return 1;
        }

        // A in secure, B not, prioritise B
        if (aInSecure && !bInSecure)
        {
            return 1;
        }

        // B in secure, A not, prioritise A
        if (!aInSecure && bInSecure)
        {
            return -1;
        }

        // Both in secure
        if (aInSecure && bInSecure)
        {
            return 0;
        }

        // They match / we don't know
        return 0;
    }

    /// <summary>
    /// Get priority of items based on their stack size
    /// Smallest stack size has priority
    /// </summary>
    /// <param name="a">Item A</param>
    /// <param name="b">Item B</param>
    /// <returns>-1 = a, 1 = b, 0 = same</returns>
    protected static int GetPriorityBySmallestStackSize(Item a, Item b)
    {
        var aStackSize = a.Upd?.StackObjectsCount ?? 1;
        var bStackSize = b.Upd?.StackObjectsCount ?? 1;

        return aStackSize.CompareTo(bStackSize);
    }

    /// <summary>
    ///     Recursively check items parents to see if it is inside the players inventory, not stash
    /// </summary>
    /// <param name="itemId"> Item ID to check </param>
    /// <param name="inventoryItems"> Player inventory </param>
    /// <param name="playerStashId"> Players stash ID </param>
    /// <returns> True if it's in inventory </returns>
    protected InventoryLocation GetItemLocation(MongoId itemId, List<Item> inventoryItems, MongoId playerStashId)
    {
        var inventoryItem = inventoryItems.FirstOrDefault(item => item.Id == itemId);
        if (inventoryItem is null)
        {
            // Doesn't exist
            return InventoryLocation.Other;
        }

        // is root item and its parent is the player stash
        if (inventoryItem.Id == playerStashId)
        {
            return InventoryLocation.Stash;
        }

        // is child item and its parent is a root item
        if (inventoryItem.SlotId == "hideout")
        {
            return InventoryLocation.Stash;
        }

        if (inventoryItem.SlotId == "SecuredContainer")
        {
            return InventoryLocation.Secure;
        }

        // Recursive call for parentId
        return GetItemLocation(inventoryItem.ParentId, inventoryItems, playerStashId);
    }

    protected enum InventoryLocation
    {
        Other = 0,
        Stash = 1,
        Secure = 2,
    }
}
