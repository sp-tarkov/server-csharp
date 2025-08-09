using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Cache the baseids for each item in the items db inside a dictionary
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemBaseClassService(
    ISptLogger<ItemBaseClassService> logger,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService
)
{
    private bool _cacheGenerated;

    /// <summary>
    /// Key = Item tpl, values = Ids of its parents
    /// </summary>
    private Dictionary<MongoId, HashSet<MongoId>> _itemBaseClassesCache = [];
    private readonly HashSet<MongoId> _rootNodeIds = [];

    /// <summary>
    ///     Create cache and store inside ItemBaseClassService <br />
    ///     Store a dict of an items tpl to the base classes it and its parents have
    /// </summary>
    public void HydrateItemBaseClassCache()
    {
        // Clear existing cache
        _itemBaseClassesCache = new Dictionary<MongoId, HashSet<MongoId>>();

        var items = databaseService.GetItems();
        foreach (var item in items)
        {
            if (string.Equals(item.Value.Type, "Item", StringComparison.OrdinalIgnoreCase))
            {
                var itemIdToUpdate = item.Value.Id;
                if (!_itemBaseClassesCache.ContainsKey(item.Value.Id))
                {
                    _itemBaseClassesCache[item.Value.Id] = [];
                }

                AddBaseItems(itemIdToUpdate, item.Value);
            }
            else
            {
                _rootNodeIds.Add(item.Key);
            }
        }

        _cacheGenerated = true;
    }

    /// <summary>
    ///     Helper method, recursively iterate through items parent items, finding and adding ids to dictionary
    /// </summary>
    /// <param name="itemIdToUpdate"> Item tpl to store base ids against in dictionary </param>
    /// <param name="item"> Item being checked </param>
    protected void AddBaseItems(MongoId itemIdToUpdate, TemplateItem item)
    {
        _itemBaseClassesCache[itemIdToUpdate].Add(item.Parent);
        databaseService.GetItems().TryGetValue(item.Parent, out var parent);

        if (parent is not null && !parent.Parent.IsEmpty)
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
    public bool ItemHasBaseClass(MongoId itemTpl, IEnumerable<MongoId> baseClasses)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (itemTpl.IsEmpty)
        {
            logger.Warning("Unable to check itemTpl base class as value passed is null");

            return false;
        }

        // The cache is only generated for item templates with `_type == "Item"`, so return false for any other type,
        // including item templates that simply don't exist.
        if (_rootNodeIds.Contains(itemTpl))
        {
            return false;
        }

        var existsInCache = _itemBaseClassesCache.TryGetValue(itemTpl, out var baseClassList);
        if (!existsInCache)
        {
            // Not found
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(serverLocalisationService.GetText("baseclass-item_not_found", itemTpl));
            }

            // Not found in cache, Hydrate again - some mods add items late in server startup lifecycle
            HydrateItemBaseClassCache();

            existsInCache = _itemBaseClassesCache.TryGetValue(itemTpl, out baseClassList);
        }

        if (existsInCache)
        {
            return baseClassList.Overlaps(baseClasses);
        }

        logger.Warning(serverLocalisationService.GetText("baseclass-item_not_found_failed", itemTpl));

        return false;
    }

    /// <summary>
    ///     Does item tpl inherit from the requested base class
    /// </summary>
    /// <param name="itemTpl"> ItemTpl item to check base classes of </param>
    /// <param name="baseClasses"> BaseClass base class to check for </param>
    /// <returns> true if item inherits from base class passed in </returns>
    public bool ItemHasBaseClass(MongoId itemTpl, MongoId baseClasses)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (itemTpl.IsEmpty)
        {
            logger.Warning("Unable to check itemTpl base class as value passed is null");

            return false;
        }

        // The cache is only generated for item templates with `_type == "Item"`, so return false for any other type,
        // including item templates that simply don't exist.
        if (_rootNodeIds.Contains(itemTpl))
        {
            return false;
        }

        var existsInCache = _itemBaseClassesCache.TryGetValue(itemTpl, out var baseClassList);
        if (!existsInCache)
        {
            // Not found
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(serverLocalisationService.GetText("baseclass-item_not_found", itemTpl));
            }

            // Not found in cache, Hydrate again - some mods add items late in server startup lifecycle
            HydrateItemBaseClassCache();

            existsInCache = _itemBaseClassesCache.TryGetValue(itemTpl, out baseClassList);
        }

        if (existsInCache)
        {
            return baseClassList.Contains(baseClasses);
        }

        logger.Warning(serverLocalisationService.GetText("baseclass-item_not_found_failed", itemTpl));

        return false;
    }

    /// <summary>
    ///     Get base classes item inherits from
    /// </summary>
    /// <param name="itemTpl"> ItemTpl item to get base classes for </param>
    /// <returns> array of base classes </returns>
    public HashSet<MongoId> GetItemBaseClasses(MongoId itemTpl)
    {
        if (!_cacheGenerated)
        {
            HydrateItemBaseClassCache();
        }

        if (!_itemBaseClassesCache.TryGetValue(itemTpl, out var value))
        {
            return [];
        }

        return value;
    }
}
