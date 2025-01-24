using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;

namespace Core.Helpers;

[Injectable]
public class RagfairHelper(
    TraderAssortHelper traderAssortHelper,
    DatabaseService databaseService,
    HandbookHelper handbookHelper,
    ItemHelper itemHelper,
    RagfairLinkedItemService ragfairLinkedItemService,
    UtilityHelper utilityHelper,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();
    
    /**
     * Gets currency TAG from TPL
     * @param {string} currency
     * @returns string
     */
    public string GetCurrencyTag(string currency)
    {
        switch (currency) {
            case Money.EUROS:
                return "EUR";
            case Money.DOLLARS:
                return "USD";
            case Money.ROUBLES:
                return "RUB";
            case Money.GP:
                return "GP";
            default:
                return "";
        }
    }

    public List<string> FilterCategories(string sessionID, SearchRequestData request)
    {
        var result = new List<string>();

        // Case: weapon builds
        if (request.BuildCount != null) {
            return request.BuildItems.Keys.ToList();
        }

        // Case: search
        if (request.LinkedSearchId != null) {
            var data = ragfairLinkedItemService.GetLinkedItems(request.LinkedSearchId);
            result = data == null ? [] : [..data];
        }

        // Case: category
        if (request.HandbookId != null) {
            var handbook = GetCategoryList(request.HandbookId);

            if (result.Count != null && result.Count > 0) {
                result = utilityHelper.ArrayIntersect(result, handbook);
            } else {
                result = handbook;
            }
        }

        return result;
    }

    public Dictionary<string, TraderAssort> GetDisplayableAssorts(string sessionID)
    {
        var result = new Dictionary<string, TraderAssort>();

        foreach (var traderID in databaseService.GetTraders().Keys) {
            if (ragfairConfig.Traders.ContainsKey(traderID)) {
                result[traderID] = traderAssortHelper.GetAssort(sessionID, traderID, true);
            }
        }

        return result;
    }

    protected List<string> GetCategoryList(string handbookId)
    {
        var result = new List<string>();

        // if its "mods" great-parent category, do double recursive loop
        if (handbookId == "5b5f71a686f77447ed5636ab") {
            foreach (var categ in handbookHelper.ChildrenCategories(handbookId)) {
                foreach (var subcateg in handbookHelper.ChildrenCategories(categ)) {
                    result = [..result, ..handbookHelper.TemplatesWithParent(subcateg)];
                }
            }

            return result;
        }

        // item is in any other category
        if (handbookHelper.IsCategory(handbookId)) {
            // list all item of the category
            result = handbookHelper.TemplatesWithParent(handbookId);

            foreach (var categ in handbookHelper.ChildrenCategories(handbookId)) {
                result = [..result, ..handbookHelper.TemplatesWithParent(categ)];
            }

            return result;
        }

        // its a specific item searched
        result.Add(handbookId);
        return result;
    }

    /**
     * Iterate over array of identical items and merge stack count
     * Ragfair allows abnormally large stacks.
     */
    public List<Item> MergeStackable(List<Item> items)
    {
        var list = new List<Item>();
        Item rootItem = null;

        foreach (var item in items) {
            var itemFixed = itemHelper.FixItemStackCount(item);

            var isChild = items.Any(it => it.Id == itemFixed.ParentId);
            if (!isChild) {
                if (rootItem == null) {
                    rootItem = cloner.Clone(itemFixed);
                    rootItem.Upd.OriginalStackObjectsCount = rootItem.Upd.StackObjectsCount;
                } else {
                    rootItem.Upd.StackObjectsCount += itemFixed.Upd.StackObjectsCount;
                    list.Add(itemFixed);
                }
            } else {
                list.Add(itemFixed);
            }
        }

        return [rootItem, ..list];
    }

    /**
     * Return the symbol for a currency
     * e.g. 5449016a4bdc2d6f028b456f return ₽
     * @param currencyTpl currency to get symbol for
     * @returns symbol of currency
     */
    public string GetCurrencySymbol(string currencyTpl)
    {
        return currencyTpl switch
        {
            Money.EUROS => "€",
            Money.DOLLARS => "$",
            _ => "₽"
        };
    }
}
