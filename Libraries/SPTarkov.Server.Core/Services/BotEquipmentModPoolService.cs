using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentModPoolService(
    ISptLogger<BotEquipmentModPoolService> logger,
    ItemHelper itemHelper,
    DatabaseService databaseService,
    ServerLocalisationService localisationService
)
{
    private readonly Lock _lockObject = new();

    private ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>>? _gearModPool;
    protected ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>> GearModPool
    {
        get
        {
            lock (_lockObject)
            {
                return _gearModPool ??= GenerateGearPool();
            }
        }
    }

    private ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>>? _weaponModPool;
    protected ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>> WeaponModPool
    {
        get
        {
            lock (_lockObject)
            {
                return _weaponModPool ??= GenerateWeaponPool();
            }
        }
    }

    /// <summary>
    ///     Create a dictionary of mods for each item passed in
    /// </summary>
    /// <param name="inputItems"> Items to find related mods and store in modPool </param>
    /// <param name="poolKey"> Mod pool to choose from e.g. "weapon" for weaponModPool </param>
    protected ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>> GeneratePool(
        IEnumerable<TemplateItem>? inputItems,
        string poolKey
    )
    {
        if (inputItems is null || !inputItems.Any())
        {
            logger.Error(localisationService.GetText("bot-unable_to_generate_item_pool_no_items", poolKey));
            return [];
        }

        // Create pool we want to return
        var pool = new ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>>();

        // Create queue to hold items we need to process/check for mods to add into the pool
        // Add items passed in to method initially, add sub-mods later
        var itemsToProcess = new Queue<TemplateItem>(inputItems);

        // Keep track of processed items to reduce unnecessary work
        var processedItems = new HashSet<MongoId>();

        while (itemsToProcess.TryDequeue(out var currentItem))
        {
            // Null guard / we've already processed this item
            if (!processedItems.Add(currentItem.Id))
            {
                continue;
            }

            // No slots = skip
            if (currentItem.Properties?.Slots is null || !currentItem.Properties.Slots.Any())
            {
                continue;
            }

            // Get top-level pool, create if it doesn't exist
            var itemPool = pool.GetOrAdd(currentItem.Id, new ConcurrentDictionary<string, HashSet<MongoId>>());

            foreach (var slot in currentItem.Properties.Slots)
            {
                var compatibleMods = slot.Properties?.Filters?.FirstOrDefault()?.Filter;
                if (compatibleMods is null || compatibleMods.Count == 0)
                {
                    // No mod items in whitelist, skip
                    continue;
                }

                // Get or add set for this specific mod slot (e.g., "mod_scope").
                var modItemPool = itemPool.GetOrAdd(slot.Name!, []);

                foreach (var modTpl in compatibleMods)
                {
                    modItemPool.Add(modTpl);

                    // Also heck if mod ALSO has its own sub slots to process
                    var modItemDetails = itemHelper.GetItem(modTpl).Value;
                    if (modItemDetails?.Properties?.Slots?.Any() == true)
                    {
                        // Has slots we need to check, add to processing queue
                        itemsToProcess.Enqueue(modItemDetails);
                    }
                }
            }
        }

        return pool;
    }

    /// <summary>
    ///     Empty the mod pool
    /// </summary>
    public void ResetWeaponPool()
    {
        WeaponModPool.Clear();
    }

    /// <summary>
    ///     Get array of compatible mods for an items mod slot (generate pool if it doesn't exist already)
    /// </summary>
    /// <param name="itemTpl"> Item to look up </param>
    /// <param name="slotName"> Slot to get compatible mods for </param>
    /// <returns> Hashset of tpls that fit the slot </returns>
    public HashSet<MongoId> GetCompatibleModsForWeaponSlot(MongoId itemTpl, string slotName)
    {
        if (WeaponModPool.TryGetValue(itemTpl, out var value))
        {
            if (value.TryGetValue(slotName, out var tplsForSlotHashSet))
            {
                return tplsForSlotHashSet;
            }
        }
        logger.Warning($"Slot: {slotName} not found for item: {itemTpl} in cache");

        return [];
    }

    /// <summary>
    ///     Get mods for a piece of gear by its tpl
    /// </summary>
    /// <param name="itemTpl"> Items tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public ConcurrentDictionary<string, HashSet<MongoId>> GetModsForGearSlot(MongoId itemTpl)
    {
        return GearModPool.TryGetValue(itemTpl, out var value) ? value : [];
    }

    /// <summary>
    ///     Get mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public ConcurrentDictionary<string, HashSet<MongoId>> GetModsForWeaponSlot(MongoId itemTpl)
    {
        return WeaponModPool.TryGetValue(itemTpl, out var value) ? value : [];
    }

    /// <summary>
    ///     Get required mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public Dictionary<string, HashSet<MongoId>> GetRequiredModsForWeaponSlot(MongoId itemTpl)
    {
        var result = new Dictionary<string, HashSet<MongoId>>();

        // Get item from db
        var itemDb = itemHelper.GetItem(itemTpl).Value;
        if (itemDb?.Properties?.Slots is not null)
        // Loop over slots flagged as 'required'
        {
            foreach (var slot in itemDb.Properties.Slots.Where(slot => slot.Required.GetValueOrDefault(false)))
            {
                // Create dict entry for mod slot
                result.TryAdd(slot.Name!, []);

                // Add compatible tpls to dicts hashset
                foreach (var compatibleItemTpl in slot.Properties!.Filters!.FirstOrDefault()!.Filter!)
                {
                    result[slot.Name!].Add(compatibleItemTpl);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Create weapon mod pool and set generated flag to true
    /// </summary>
    protected ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>> GenerateWeaponPool()
    {
        var weaponsAndMods = databaseService
            .GetItems()
            .Values.Where(item =>
                string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && itemHelper.IsOfBaseclasses(item.Id, [BaseClasses.WEAPON, BaseClasses.MOD])
            );

        return GeneratePool(weaponsAndMods, "weapon");
    }

    /// <summary>
    ///     Create gear mod pool and set generated flag to true
    /// </summary>
    protected ConcurrentDictionary<MongoId, ConcurrentDictionary<string, HashSet<MongoId>>> GenerateGearPool()
    {
        var gearAndMods = databaseService
            .GetItems()
            .Values.Where(item =>
                string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && itemHelper.IsOfBaseclasses(
                    item.Id,
                    [BaseClasses.ARMORED_EQUIPMENT, BaseClasses.VEST, BaseClasses.ARMOR, BaseClasses.HEADWEAR, BaseClasses.MOD]
                )
            );

        return GeneratePool(gearAndMods, "gear");
    }
}
