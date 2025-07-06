using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
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
    RagfairLinkedItemService ragfairLinkedItemService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    /// Gets currency TAG from currency tpl value
    /// </summary>
    /// <param name="currencyTpl">Currency tpl</param>
    /// <returns>Currency tag, e.g. RUB</returns>
    public string GetCurrencyTag(MongoId currencyTpl)
    {
        if (currencyTpl == Money.EUROS)
        {
            return "EUR";
        }
        if (currencyTpl == Money.DOLLARS)
        {
            return "USD";
        }
        if (currencyTpl == Money.ROUBLES)
        {
            return "RUB";
        }
        if (currencyTpl == Money.GP)
        {
            return "GP";
        }

        return "";
    }

    /// <summary>
    /// Get a currency TAG by its search filter value (e.g. 0 = all, 1 = RUB)
    /// </summary>
    /// <param name="currencyFilter">Search filter choice</param>
    /// <returns>Currency tag</returns>
    public string GetCurrencyTag(int currencyFilter)
    {
        switch (currencyFilter)
        {
            case 3:
                return "EUR";
            case 2:
                return "USD";
            case 1:
                return "RUB";
            default:
                return "all";
        }
    }

    public List<MongoId> FilterCategories(MongoId sessionId, SearchRequestData request)
    {
        var result = new List<MongoId>();

        // Case: weapon builds
        if (request.BuildCount > 0)
        {
            return request.BuildItems.Keys.ToList();
        }

        // Case: search
        if (!string.IsNullOrEmpty(request.LinkedSearchId))
        {
            var data = ragfairLinkedItemService.GetLinkedItems(request.LinkedSearchId);
            result = [.. data];
        }

        // Case: category
        if (!string.IsNullOrEmpty(request.HandbookId))
        {
            var handbook = GetCategoryList(request.HandbookId);
            result = result?.Count > 0 ? result.IntersectWith(handbook) : handbook;
        }

        return result;
    }

    public Dictionary<MongoId, TraderAssort> GetDisplayableAssorts(MongoId sessionId)
    {
        var result = new Dictionary<MongoId, TraderAssort>();
        foreach (
            var traderId in databaseService
                .GetTraders()
                .Keys.Where(traderId => _ragfairConfig.Traders.ContainsKey(traderId))
        )
        {
            result[traderId] = traderAssortHelper.GetAssort(sessionId, traderId, true);
        }

        return result;
    }

    protected List<MongoId> GetCategoryList(MongoId handbookId)
    {
        var result = new List<MongoId>();

        // if its "mods" great-parent category, do double recursive loop
        if (handbookId == new MongoId("5b5f71a686f77447ed5636ab"))
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

            return handbookHelper
                .ChildrenCategories(handbookId)
                .Aggregate(
                    result,
                    (current, category) =>
                        [.. current, .. handbookHelper.TemplatesWithParent(category)]
                );
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
        Item? rootItem = null;

        foreach (var item in items)
        {
            item.FixItemStackCount();

            var isChild = items.Any(it => it.Id == item.ParentId);
            if (!isChild)
            {
                if (rootItem == null)
                {
                    rootItem = cloner.Clone(item);
                    rootItem.Upd.OriginalStackObjectsCount = rootItem.Upd.StackObjectsCount;
                }
                else
                {
                    rootItem.Upd.StackObjectsCount += item.Upd.StackObjectsCount;
                    list.Add(item);
                }
            }
            else
            {
                list.Add(item);
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
        return currencyTpl == Money.EUROS ? "€"
            : currencyTpl == Money.DOLLARS ? "$"
            : "₽";
    }
}
