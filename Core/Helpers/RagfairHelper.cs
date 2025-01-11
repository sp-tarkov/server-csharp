using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;

namespace Core.Helpers;

[Injectable]
public class RagfairHelper
{
    /// <summary>
    /// Gets currency TAG from TPL
    /// </summary>
    /// <param name="currency">currency</param>
    /// <returns>string</returns>
    public string GetCurrencyTag(string currency)
    {
        throw new NotImplementedException();
    }

    public List<string> FilterCategories(string sessionID, SearchRequestData request)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, TraderAssort> GetDisplayableAssorts(string sessionID)
    {
        throw new NotImplementedException();
    }

    protected List<string> GetCategoryList(string handbookId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over array of identical items and merge stack count
    /// Ragfair allows abnormally large stacks.
    /// </summary>
    public List<Item> MergeStackable(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the symbol for a currency
    /// e.g. 5449016a4bdc2d6f028b456f return ₽
    /// </summary>
    /// <param name="currencyTpl">currency to get symbol for</param>
    /// <returns>symbol of currency</returns>
    public string GetCurrencySymbol(string currencyTpl)
    {
        throw new NotImplementedException();
    }
}
