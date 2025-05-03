using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

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
    protected RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /**
     * Gets currency TAG from TPL
     * @param {string} currency
     * @returns string
     */
    public string GetCurrencyTag(string currency)
    {
        switch (currency)
        {
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

    public List<string> FilterCategories(string sessionId, SearchRequestData request)
    {
        var result = new List<string>();

        // Case: weapon builds
        if (request.BuildCount > 0)
        {
            return request.BuildItems.Keys.ToList();
        }

        // Case: search
        if (!string.IsNullOrEmpty(request.LinkedSearchId))
        {
            var data = ragfairLinkedItemService.GetLinkedItems(request.LinkedSearchId);
            result = data == null ? [] : [.. data];
        }

        // Case: category
        if (!string.IsNullOrEmpty(request.HandbookId))
        {
            var handbook = GetCategoryList(request.HandbookId);

            if (result.Count != null && result.Count > 0)
            {
                result = utilityHelper.ArrayIntersect(result, handbook);
            }
            else
            {
                result = handbook;
            }
        }

        return result;
    }

    public Dictionary<string, TraderAssort> GetDisplayableAssorts(string sessionId)
    {
        var result = new Dictionary<string, TraderAssort>();
        foreach (var traderId in databaseService.GetTraders()
                     .Keys
                     .Where(traderId =>
                     {
                         return _ragfairConfig.Traders.ContainsKey(traderId);
                     }))
        {
            result[traderId] = traderAssortHelper.GetAssort(sessionId, traderId, true);
        }

        return result;
    }

    protected List<string> GetCategoryList(string handbookId)
    {
        var result = new List<string>();

        // if its "mods" great-parent category, do double recursive loop
        if (handbookId == "5b5f71a686f77447ed5636ab")
        {
            foreach (var category in handbookHelper.ChildrenCategories(handbookId))
            {
                foreach (var subCategory in handbookHelper.ChildrenCategories(category))
                {
                    result = [.. result, .. handbookHelper.TemplatesWithParent(subCategory)];
                }
            }

            return result;
        }

        // item is in any other category
        if (handbookHelper.IsCategory(handbookId))
        {
            // list all item of the category
            result = handbookHelper.TemplatesWithParent(handbookId);

            return handbookHelper.ChildrenCategories(handbookId)
                .Aggregate(result, (current, category) =>
                {
                    return [.. current, .. handbookHelper.TemplatesWithParent(category)];
                });
        }

        // It's a specific item searched
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

        foreach (var item in items)
        {
            var itemFixed = itemHelper.FixItemStackCount(item);

            var isChild = items.Any(it =>
            {
                return it.Id == itemFixed.ParentId;
            });
            if (!isChild)
            {
                if (rootItem == null)
                {
                    rootItem = cloner.Clone(itemFixed);
                    rootItem.Upd.OriginalStackObjectsCount = rootItem.Upd.StackObjectsCount;
                }
                else
                {
                    rootItem.Upd.StackObjectsCount += itemFixed.Upd.StackObjectsCount;
                    list.Add(itemFixed);
                }
            }
            else
            {
                list.Add(itemFixed);
            }
        }

        return [rootItem, .. list];
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
