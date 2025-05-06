using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Cache the baseids for each item in the items db inside a dictionary
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemBaseClassService(
    ISptLogger<ItemBaseClassService> _logger,
    DatabaseService _databaseService,
    LocalisationService _localisationService
)
{
    private bool _cacheGenerated;
    private Dictionary<string, HashSet<string>> _itemBaseClassesCache;

    /// <summary>
    ///     Create cache and store inside ItemBaseClassService <br />
    ///     Store a dict of an items tpl to the base classes it and its parents have
    /// </summary>
    public void HydrateItemBaseClassCache()
    {
        // Clear existing cache
        _itemBaseClassesCache = new Dictionary<string, HashSet<string>>();

        var items = _databaseService.GetItems();
        var filteredDbItems = items.Where(x => string.Equals(x.Value.Type, "Item", StringComparison.OrdinalIgnoreCase));
        foreach (var item in filteredDbItems)
        {
            var itemIdToUpdate = item.Value.Id;
            if (!_itemBaseClassesCache.ContainsKey(item.Value.Id))
            {
                _itemBaseClassesCache[item.Value.Id] = [];
            }

            AddBaseItems(itemIdToUpdate, item.Value);
        }

        _cacheGenerated = true;
    }

    /// <summary>
    ///     Helper method, recursively iterate through items parent items, finding and adding ids to dictionary
    /// </summary>
    /// <param name="itemIdToUpdate"> Item tpl to store base ids against in dictionary </param>
    /// <param name="item"> Item being checked </param>
    protected void AddBaseItems(string itemIdToUpdate, TemplateItem item)
    {
        _itemBaseClassesCache[itemIdToUpdate].Add(item.Parent);
        var parent = _databaseService.GetItems()[item.Parent];

        if (!string.IsNullOrEmpty(parent.Parent))
        {
            AddBaseItems(itemIdToUpdate, parent);
        }
    }

    /// <summary>
    ///     Does item tpl inherit from the requested base class
    /// </summary>
    /// <param name="itemTpl"> ItemTpl item to check base classes of </param>
    /// <param name="baseClasses"> BaseClass base class to check for </param>
    /// <returns> true if item inherits from base class passed in </returns>
    public bool ItemHasBaseClass(string itemTpl, ICollection<string> baseClasses)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (string.IsNullOrEmpty(itemTpl))
        {
            _logger.Warning("Unable to check itemTpl base class as value passed is null");

            return false;
        }

        // The cache is only generated for item templates with `_type == "Item"`, so return false for any other type,
        // including item templates that simply don't exist.
        if (!CachedItemIsOfItemType(itemTpl))
        {
            return false;
        }

        if (_itemBaseClassesCache.TryGetValue(itemTpl, out var baseClassList))
        {
            return baseClassList.Overlaps(baseClasses);
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(_localisationService.GetText("baseclass-item_not_found", itemTpl));
        }

        // Not found in cache, Hydrate again - some mods add items late
        HydrateItemBaseClassCache();

        // Check for item again, return false if item not found a second time
        if (_itemBaseClassesCache.TryGetValue(itemTpl, out var value))
        {
            return value.Any(baseClasses.Contains);
        }

        _logger.Warning(_localisationService.GetText("baseclass-item_not_found_failed", itemTpl));

        return false;
    }

    /// <summary>
    ///     Check if cached item template is of type Item
    /// </summary>
    /// <param name="itemTemplateId"> ItemTemplateId item to check </param>
    /// <returns> True if item is of type Item </returns>
    private bool CachedItemIsOfItemType(string itemTemplateId)
    {
        return string.Equals(_databaseService.GetItems()[itemTemplateId]?.Type, "Item", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Get base classes item inherits from
    /// </summary>
    /// <param name="itemTpl"> ItemTpl item to get base classes for </param>
    /// <returns> array of base classes </returns>
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

        return _itemBaseClassesCache[itemTpl].ToList();
    }
}
