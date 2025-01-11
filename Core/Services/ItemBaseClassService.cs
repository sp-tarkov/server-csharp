using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ItemBaseClassService
{
    /**
     * Create cache and store inside ItemBaseClassService
     * Store a dict of an items tpl to the base classes it and its parents have
     */
    public void HydrateItemBaseClassCache()
    {
        throw new NotImplementedException();
    }

    /**
     * Helper method, recursively iterate through items parent items, finding and adding ids to dictionary
     * @param itemIdToUpdate item tpl to store base ids against in dictionary
     * @param item item being checked
     */
    protected void AddBaseItems(string itemIdToUpdate, TemplateItem item)
    {
        throw new NotImplementedException();
    }

    /**
     * Does item tpl inherit from the requested base class
     * @param itemTpl item to check base classes of
     * @param baseClass base class to check for
     * @returns true if item inherits from base class passed in
     */
    public bool ItemHasBaseClass(string itemTpl, List<string> baseClasses)
    {
        throw new NotImplementedException();
    }

    /**
     * Check if cached item template is of type Item
     * @param itemTemplateId item to check
     * @returns true if item is of type Item
     */
    private bool CachedItemIsOfItemType(string itemTemplateId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get base classes item inherits from
     * @param itemTpl item to get base classes for
     * @returns array of base classes
     */
    public List<string> GetItemBaseClasses(string itemTpl)
    {
        throw new NotImplementedException();
    }
}
