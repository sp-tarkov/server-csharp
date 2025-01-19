using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class HandbookHelper
{
    /// <summary>
    /// Create an in-memory cache of all items with associated handbook price in handbookPriceCache class
    /// </summary>
    public void HydrateLookup()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get price from internal cache, if cache empty look up price directly in handbook (expensive)
    /// If no values found, return 0
    /// </summary>
    /// <param name="tpl">Item tpl to look up price for</param>
    /// <returns>price in roubles</returns>
    public double GetTemplatePrice(string tpl)
    {
        throw new NotImplementedException();
    }

    public double GetTemplatePriceForItems(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all items in template with the given parent category
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns>string array</returns>
    public List<string> TemplatesWithParent(string parentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does category exist in handbook cache
    /// </summary>
    /// <param name="category"></param>
    /// <returns>true if exists in cache</returns>
    public bool IsCategory(string category)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all items associated with a categories parent
    /// </summary>
    /// <param name="categoryParent"></param>
    /// <returns>string array</returns>
    public List<string> ChildrenCategories(string categoryParent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert non-roubles into roubles
    /// </summary>
    /// <param name="nonRoubleCurrencyCount">Currency count to convert</param>
    /// <param name="currencyTypeFrom">What current currency is</param>
    /// <returns>Count in roubles</returns>
    public double InRUB(double nonRoubleCurrencyCount, string currencyTypeFrom)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert roubles into another currency
    /// </summary>
    /// <param name="roubleCurrencyCount">roubles to convert</param>
    /// <param name="currencyTypeTo">Currency to convert roubles into</param>
    /// <returns>currency count in desired type</returns>
    public double FromRUB(double roubleCurrencyCount, string currencyTypeTo)
    {
        throw new NotImplementedException();
    }

    public HandbookCategory GetCategoryById(string handbookId)
    {
        throw new NotImplementedException();
    }
}
