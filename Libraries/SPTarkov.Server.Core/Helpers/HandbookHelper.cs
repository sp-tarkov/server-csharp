using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class HandbookHelper(ISptLogger<HandbookHelper> logger, DatabaseService databaseService, ConfigServer configServer, ICloner cloner)
{
    private LookupCollection? _handbookPriceCache;
    protected virtual LookupCollection HandbookPriceCache
    {
        get { return _handbookPriceCache ??= HydrateHandbookCache(); }
    }

    protected readonly ItemConfig ItemConfig = configServer.GetConfig<ItemConfig>();

    /// <summary>
    ///     Create an in-memory cache of all items with associated handbook price in handbookPriceCache class
    /// </summary>
    protected LookupCollection HydrateHandbookCache()
    {
        var result = new LookupCollection();
        var handbook = databaseService.GetHandbook();
        // Add handbook overrides found in items.json config into db
        foreach (var (key, priceOverride) in ItemConfig.HandbookPriceOverride)
        {
            var itemToUpdate = handbook.Items.FirstOrDefault(item => item.Id == key);
            if (itemToUpdate is null)
            {
                handbook.Items.Add(
                    new HandbookItem
                    {
                        Id = key,
                        ParentId = priceOverride.ParentId,
                        Price = priceOverride.Price,
                    }
                );
                itemToUpdate = handbook.Items.FirstOrDefault(item => item.Id == key);
            }

            itemToUpdate!.Price = priceOverride.Price;
            itemToUpdate.ParentId = priceOverride.ParentId;
        }

        var handbookDbClone = cloner.Clone(handbook)!;
        foreach (var handbookItem in handbookDbClone.Items)
        {
            result.Items.ById.TryAdd(handbookItem.Id, handbookItem.Price ?? 0);
            if (!result.Items.ByParent.TryGetValue(handbookItem.ParentId, out _))
            {
                result.Items.ByParent.TryAdd(handbookItem.ParentId, []);
            }

            if (!result.Items.ByParent.TryGetValue(handbookItem.ParentId, out var itemIds))
            {
                throw new HandbookHelperException(
                    $"Cannot add item id `{handbookItem.Id}` to parent id `{handbookItem.ParentId}`. Parent does not exist."
                );
            }

            itemIds.Add(handbookItem.Id);
        }

        foreach (var handbookCategory in handbookDbClone.Categories)
        {
            if (!result.Categories.ById.TryAdd(handbookCategory.Id, handbookCategory.ParentId))
            {
                throw new HandbookHelperException($"Unable to add `{handbookCategory.Id}`. Key already exists.");
            }

            if (handbookCategory.ParentId is not null)
            {
                if (!result.Categories.ByParent.TryGetValue(handbookCategory.ParentId.Value, out _))
                {
                    result.Categories.ByParent.TryAdd(handbookCategory.ParentId.Value, []);
                }

                if (!result.Categories.ByParent.TryGetValue(handbookCategory.ParentId.Value, out var itemIds))
                {
                    throw new HandbookHelperException(
                        $"Cannot add item id `{handbookCategory.Id}` to parent id `{handbookCategory.ParentId.Value}`. Parent does not exist."
                    );
                }

                itemIds.Add(handbookCategory.Id);
            }
        }

        return result;
    }

    /// <summary>
    ///     Get price from internal cache, if cache empty look up price directly in handbook (expensive)
    ///     If no values found, return 0
    /// </summary>
    /// <param name="tpl">Item tpl to look up price for</param>
    /// <returns>price in roubles</returns>
    public double GetTemplatePrice(MongoId tpl)
    {
        if (HandbookPriceCache.Items.ById.TryGetValue(tpl, out var itemPrice))
        {
            return itemPrice;
        }

        var handbookItem = databaseService.GetHandbook().Items?.FirstOrDefault(item => item.Id == tpl);
        if (handbookItem is null)
        {
            const int newValue = 0;

            if (!HandbookPriceCache.Items.ById.TryAdd(tpl, newValue))
            {
                // Overwrite
                HandbookPriceCache.Items.ById[tpl] = newValue;
            }

            return newValue;
        }

        if (!HandbookPriceCache.Items.ById.TryAdd(tpl, handbookItem.Price ?? 0))
        {
            // Overwrite
            HandbookPriceCache.Items.ById[tpl] = handbookItem.Price ?? 0;
        }

        return handbookItem.Price.Value;
    }

    /// <summary>
    ///     Sum price of supplied items with handbook prices
    /// </summary>
    /// <param name="items">Items to Sum</param>
    /// <returns></returns>
    public double GetTemplatePriceForItems(IEnumerable<Item> items)
    {
        return items.Sum(item => GetTemplatePrice(item.Template));
    }

    /// <summary>
    ///     Get all items in template with the given parent category
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns>string array</returns>
    public List<MongoId> TemplatesWithParent(MongoId parentId)
    {
        if (HandbookPriceCache.Items.ByParent.TryGetValue(parentId, out var templates))
        {
            return templates;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Template ids with parent id `{parentId}` not found when trying to get templates by parent");
        }

        return [];
    }

    /// <summary>
    ///     Does category exist in handbook cache
    /// </summary>
    /// <param name="category"></param>
    /// <returns>true if exists in cache</returns>
    public bool IsCategory(MongoId category)
    {
        return HandbookPriceCache.Categories.ById.TryGetValue(category, out _);
    }

    /// <summary>
    ///     Get all items associated with a categories parent
    /// </summary>
    /// <param name="categoryParent"></param>
    /// <returns>string array</returns>
    public List<string> ChildrenCategories(MongoId categoryParent)
    {
        if (HandbookPriceCache.Categories.ByParent.TryGetValue(categoryParent, out var childrenCategories))
        {
            return childrenCategories;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Children categories with parent id `{categoryParent}` not found when trying to get children categories");
        }

        return [];
    }

    /// <summary>
    ///     Convert non-roubles into roubles
    /// </summary>
    /// <param name="nonRoubleCurrencyCount">Currency count to convert</param>
    /// <param name="currencyTypeFrom">What current currency is</param>
    /// <returns>Count in roubles</returns>
    public double InRoubles(double nonRoubleCurrencyCount, MongoId currencyTypeFrom)
    {
        return currencyTypeFrom == Money.ROUBLES
            ? nonRoubleCurrencyCount
            : Math.Round(nonRoubleCurrencyCount * GetTemplatePrice(currencyTypeFrom));
    }

    /// <summary>
    ///     Convert roubles into another currency
    /// </summary>
    /// <param name="roubleCurrencyCount">roubles to convert</param>
    /// <param name="currencyTypeTo">Currency to convert roubles into</param>
    /// <returns>currency count in desired type</returns>
    public double FromRoubles(double roubleCurrencyCount, MongoId currencyTypeTo)
    {
        if (currencyTypeTo == Money.ROUBLES)
        {
            return roubleCurrencyCount;
        }

        // Get price of currency from handbook
        var price = GetTemplatePrice(currencyTypeTo);
        return price > 0 ? Math.Max(1, Math.Round(roubleCurrencyCount / price)) : 0;
    }

    public HandbookCategory? GetCategoryById(MongoId handbookId)
    {
        return databaseService.GetHandbook().Categories.FirstOrDefault(category => category.Id == handbookId);
    }

    protected record LookupItem<T, I>
    {
        public LookupItem()
        {
            ById = new Dictionary<MongoId, T>();
            ByParent = new Dictionary<MongoId, List<I>>();
        }

        public Dictionary<MongoId, T> ById { get; set; }

        public Dictionary<MongoId, List<I>> ByParent { get; set; }
    }

    protected record LookupCollection
    {
        public LookupCollection()
        {
            Items = new LookupItem<double, MongoId>();
            Categories = new LookupItem<string, string>();
        }

        public LookupItem<double, MongoId> Items { get; set; }

        public LookupItem<string, string> Categories { get; set; }
    }
}
