using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;

namespace Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class HandbookHelper(
    DatabaseService _databaseService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected ItemConfig _itemConfig = _configServer.GetConfig<ItemConfig>();
    protected bool _lookupCacheGenerated = false;
    protected LookupCollection _handbookPriceCache = new();

    /// <summary>
    /// Create an in-memory cache of all items with associated handbook price in handbookPriceCache class
    /// </summary>
    public void HydrateLookup()
    {
        var handbook = _databaseService.GetHandbook();
        // Add handbook overrides found in items.json config into db
        foreach (var itemTplKey in _itemConfig.HandbookPriceOverride)
        {
            var data = _itemConfig.HandbookPriceOverride[itemTplKey.Key];

            var itemToUpdate = handbook.Items.FirstOrDefault(item => item.Id == itemTplKey.Key);
            if (itemToUpdate is null)
            {
                handbook.Items.Add(
                    new HandbookItem
                    {
                        Id = itemTplKey.Key,
                        ParentId = data.ParentId,
                        Price = data.Price
                    }
                );
                itemToUpdate = handbook.Items.FirstOrDefault(item => item.Id == itemTplKey.Key);
            }

            itemToUpdate.Price = data.Price;
        }

        var handbookDbClone = _cloner.Clone(handbook);
        foreach (var handbookItem in handbookDbClone.Items)
        {
            _handbookPriceCache.Items.ById.TryAdd(handbookItem.Id, handbookItem.Price ?? 0);
            if (!_handbookPriceCache.Items.ByParent.TryGetValue(handbookItem.ParentId, out _))
                _handbookPriceCache.Items.ByParent.TryAdd(handbookItem.ParentId, []);

            _handbookPriceCache.Items.ByParent.TryGetValue(handbookItem.ParentId, out var array);
            array.Add(handbookItem.Id);
        }

        foreach (var handbookCategory in handbookDbClone.Categories)
        {
            _handbookPriceCache.Categories.ById.TryAdd(handbookCategory.Id, handbookCategory.ParentId);
            if (handbookCategory.ParentId is not null)
            {
                if (!_handbookPriceCache.Categories.ByParent.TryGetValue(handbookCategory.ParentId, out _))
                    _handbookPriceCache.Categories.ByParent.TryAdd(handbookCategory.ParentId, []);

                _handbookPriceCache.Categories.ByParent.TryGetValue(handbookCategory.ParentId, out var array);
                array.Add(handbookCategory.Id);
            }
        }
    }

    /// <summary>
    /// Get price from internal cache, if cache empty look up price directly in handbook (expensive)
    /// If no values found, return 0
    /// </summary>
    /// <param name="tpl">Item tpl to look up price for</param>
    /// <returns>price in roubles</returns>
    public double GetTemplatePrice(string tpl)
    {
        if (!_lookupCacheGenerated)
        {
            HydrateLookup();
            _lookupCacheGenerated = true;
        }

        if (_handbookPriceCache.Items.ById.TryGetValue(tpl, out var item)) return item;

        var handbookItem = _databaseService.GetHandbook().Items?.FirstOrDefault(item => item.Id == tpl);
        if (handbookItem is null)
        {
            var newValue = 0;

            if (!_handbookPriceCache.Items.ById.TryAdd(tpl, newValue)) _handbookPriceCache.Items.ById[tpl] = newValue;

            return newValue;
        }

        if (!_handbookPriceCache.Items.ById.TryAdd(tpl, handbookItem.Price ?? 0)) _handbookPriceCache.Items.ById[tpl] = handbookItem.Price ?? 0;

        return handbookItem.Price.Value;
    }

    public double GetTemplatePriceForItems(List<Item> items)
    {
        var total = 0D;
        foreach (var item in items) total += GetTemplatePrice(item.Template);

        return total;
    }

    /// <summary>
    /// Get all items in template with the given parent category
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns>string array</returns>
    public List<string> TemplatesWithParent(string parentId)
    {
        _handbookPriceCache.Items.ByParent.TryGetValue(parentId, out var template);

        return template ?? [];
    }

    /// <summary>
    /// Does category exist in handbook cache
    /// </summary>
    /// <param name="category"></param>
    /// <returns>true if exists in cache</returns>
    public bool IsCategory(string category)
    {
        return _handbookPriceCache.Categories.ById.TryGetValue(category, out _);
    }

    /// <summary>
    /// Get all items associated with a categories parent
    /// </summary>
    /// <param name="categoryParent"></param>
    /// <returns>string array</returns>
    public List<string> ChildrenCategories(string categoryParent)
    {
        _handbookPriceCache.Categories.ByParent.TryGetValue(categoryParent, out var category);
        return category ?? [];
    }

    /// <summary>
    /// Convert non-roubles into roubles
    /// </summary>
    /// <param name="nonRoubleCurrencyCount">Currency count to convert</param>
    /// <param name="currencyTypeFrom">What current currency is</param>
    /// <returns>Count in roubles</returns>
    public int InRUB(double nonRoubleCurrencyCount, string currencyTypeFrom)
    {
        return (int)(currencyTypeFrom == Money.ROUBLES
            ? nonRoubleCurrencyCount
            : Math.Round(nonRoubleCurrencyCount * GetTemplatePrice(currencyTypeFrom)));
    }

    /// <summary>
    /// Convert roubles into another currency
    /// </summary>
    /// <param name="roubleCurrencyCount">roubles to convert</param>
    /// <param name="currencyTypeTo">Currency to convert roubles into</param>
    /// <returns>currency count in desired type</returns>
    public int FromRUB(double roubleCurrencyCount, string currencyTypeTo)
    {
        if (currencyTypeTo == Money.ROUBLES) return (int)roubleCurrencyCount;

        // Get price of currency from handbook
        var price = GetTemplatePrice(currencyTypeTo);
        return (int)(price > 0 ? Math.Max(1, Math.Round(roubleCurrencyCount / price)) : 0);
    }

    public HandbookCategory GetCategoryById(string handbookId)
    {
        return _databaseService.GetHandbook().Categories.FirstOrDefault(category => category.Id == handbookId);
    }
}

public class LookupItem<T, I>
{
    public Dictionary<string, T> ById { get; set; }
    public Dictionary<string, List<I>> ByParent { get; set; }

    public LookupItem()
    {
        ById = new Dictionary<string, T>();
        ByParent = new Dictionary<string, List<I>>();
    }
}

public class LookupCollection
{
    public LookupItem<double, string> Items { get; set; }
    public LookupItem<string, string> Categories { get; set; }

    public LookupCollection()
    {
        Items = new LookupItem<double, string>();
        Categories = new LookupItem<string, string>();
    }
}
