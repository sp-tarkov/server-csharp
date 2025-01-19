using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PaymentService
{
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
        decimal amountToPay,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /**
     * Take money and insert items into return to server request
     * @param pmcData Pmc profile
     * @param request Buy item request
     * @param sessionID Session id
     * @param output Client response
     */
    public void PayMoney(PmcData pmcData, ProcessBuyTradeRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }
}
