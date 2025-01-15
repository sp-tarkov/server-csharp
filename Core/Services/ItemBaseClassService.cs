using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ItemBaseClassService
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly LocalisationService _localisationService;

    private bool _cacheGenerated;
    private Dictionary<string, List<string>> _itemBaseClassesCache;

    public ItemBaseClassService(
        ILogger logger,
        DatabaseService databaseService,
        LocalisationService localisationService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _localisationService = localisationService;
    }

    /**
     * Create cache and store inside ItemBaseClassService
     * Store a dict of an items tpl to the base classes it and its parents have
     */
    public void HydrateItemBaseClassCache()
    {
        // Clear existing cache
        _itemBaseClassesCache = new Dictionary<string, List<string>>();

        var items = _databaseService.GetItems();
        var filteredDbItems = (items).Where((x) => x.Value.Type == "Item");
        foreach (var item in filteredDbItems) {
            var itemIdToUpdate = item.Value.Id;
            if (!_itemBaseClassesCache.ContainsKey(item.Value.Id))
            {
                _itemBaseClassesCache[item.Value.Id] = [];
            }

            AddBaseItems(itemIdToUpdate, item.Value);
        }

        _cacheGenerated = true;
    }

    /**
     * Helper method, recursively iterate through items parent items, finding and adding ids to dictionary
     * @param itemIdToUpdate item tpl to store base ids against in dictionary
     * @param item item being checked
     */
    protected void AddBaseItems(string itemIdToUpdate, TemplateItem item)
    {
        _itemBaseClassesCache[itemIdToUpdate].Add(item.Parent);
        var parent = _databaseService.GetItems()[item.Parent];

        if (parent.Parent != "")
        {
            AddBaseItems(itemIdToUpdate, parent);
        }
    }

    /**
     * Does item tpl inherit from the requested base class
     * @param itemTpl item to check base classes of
     * @param baseClass base class to check for
     * @returns true if item inherits from base class passed in
     */
    public bool ItemHasBaseClass(string itemTpl, List<string> baseClasses)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (itemTpl is null)
        {
            _logger.Warning("Unable to check itemTpl base class as value passed is null");

            return false;
        }

        // The cache is only generated for item templates with `_type === "Item"`, so return false for any other type,
        // including item templates that simply don't exist.
        if (!CachedItemIsOfItemType(itemTpl))
        {
            return false;
        }

        // No item in cache
        if (_itemBaseClassesCache.ContainsKey(itemTpl))
        {
            return _itemBaseClassesCache[itemTpl].Any(baseClasses.Contains);
        }

        _logger.Debug(_localisationService.GetText("baseclass-item_not_found", itemTpl));

        // Not found in cache, Hydrate again - some mods add items late
        HydrateItemBaseClassCache();

        // Check for item again, return false if item not found a second time
        if (_itemBaseClassesCache.ContainsKey(itemTpl))
        {
            return _itemBaseClassesCache[itemTpl].Any(baseClasses.Contains);
        }

        _logger.Warning(_localisationService.GetText("baseclass-item_not_found_failed", itemTpl));

        return false;
    }

    /**
     * Check if cached item template is of type Item
     * @param itemTemplateId item to check
     * @returns true if item is of type Item
     */
    private bool CachedItemIsOfItemType(string itemTemplateId)
    {
        return _databaseService.GetItems()[itemTemplateId]?.Type == "Item";
    }

    /**
     * Get base classes item inherits from
     * @param itemTpl item to get base classes for
     * @returns array of base classes
     */
    public List<string> GetItemBaseClasses(string itemTpl)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (!_itemBaseClassesCache.ContainsKey(itemTpl))
        {
            return [];
        }

        return _itemBaseClassesCache[itemTpl];
    }
}
