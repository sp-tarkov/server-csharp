using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

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

    private ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    >? _gearModPool;
    protected ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    > GearModPool
    {
        get
        {
            lock (_lockObject)
            {
                return _gearModPool ??= GenerateGearPool();
            }
        }
    }

    private ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    >? _weaponModPool;
    protected ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    > WeaponModPool
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
    ///     Get dictionary of mods for each item passed in
    /// </summary>
    /// <param name="inputItems"> Items to find related mods and store in modPool </param>
    /// <param name="poolType"> Mod pool to choose from e.g. "weapon" for weaponModPool </param>
    protected ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    > GeneratePool(IEnumerable<TemplateItem>? inputItems, string poolType)
    {
        if (inputItems is null || !inputItems.Any())
        {
            logger.Error(
                localisationService.GetText("bot-unable_to_generate_item_pool_no_items", poolType)
            );

            return [];
        }

        var pool =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>>();
        foreach (var item in inputItems)
        {
            if (item.Properties is null)
            {
                logger.Error(
                    localisationService.GetText(
                        "bot-item_missing_props_property",
                        new { itemTpl = item.Id, name = item.Name }
                    )
                );

                continue;
            }

            // Skip item without slots
            if (item.Properties.Slots is null || item.Properties.Slots.Count == 0)
            {
                continue;
            }

            // Add base item (weapon/armor) to pool
            pool.TryAdd(item.Id, new ConcurrentDictionary<string, HashSet<string>>());

            // Iterate over each items mod slots e.g. mod_muzzle
            foreach (var slot in item.Properties.Slots)
            {
                // Get mods that fit into the current mod slot
                var itemsThatFit = slot.Props.Filters.FirstOrDefault().Filter;

                // Get weapon/armor pool to add mod slots + mod tpls to

                var itemModPool = pool[item.Id];
                foreach (var itemToAddTpl in itemsThatFit)
                {
                    // Ensure Mod slot key + blank dict value exist
                    InitSetInDict(itemModPool, slot.Name);

                    // Does tpl exist inside mod_slots hashset
                    if (!SetContainsTpl(itemModPool[slot.Name], itemToAddTpl))
                    // Keyed by mod slot
                    {
                        AddTplToSet(itemModPool[slot.Name], itemToAddTpl);
                    }

                    var subItemDetails = itemHelper.GetItem(itemToAddTpl).Value;
                    var hasSubItemsToAdd = (subItemDetails?.Properties?.Slots?.Count ?? 0) > 0;

                    // Item has Slots + pool doesn't have value
                    if (hasSubItemsToAdd && !pool.ContainsKey(subItemDetails.Id))
                    // Recursive call
                    {
                        GeneratePool([subItemDetails], poolType);
                    }
                }
            }
        }

        return pool;
    }

    private bool SetContainsTpl(HashSet<string> itemSet, string tpl)
    {
        lock (_lockObject)
        {
            return itemSet.Contains(tpl);
        }
    }

    private bool AddTplToSet(HashSet<string> itemSet, string itemToAddTpl)
    {
        lock (_lockObject)
        {
            return itemSet.Add(itemToAddTpl);
        }
    }

    private bool InitSetInDict(
        ConcurrentDictionary<string, HashSet<string>> dictionary,
        string slotName
    )
    {
        lock (_lockObject)
        {
            return dictionary.TryAdd(slotName, []);
        }
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
    public HashSet<string> GetCompatibleModsForWeaponSlot(string itemTpl, string slotName)
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
    public ConcurrentDictionary<string, HashSet<string>> GetModsForGearSlot(string itemTpl)
    {
        return GearModPool.TryGetValue(itemTpl, out var value) ? value : [];
    }

    /// <summary>
    ///     Get mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public ConcurrentDictionary<string, HashSet<string>> GetModsForWeaponSlot(string itemTpl)
    {
        return WeaponModPool.TryGetValue(itemTpl, out var value) ? value : [];
    }

    /// <summary>
    ///     Get required mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public Dictionary<string, HashSet<string>>? GetRequiredModsForWeaponSlot(string itemTpl)
    {
        var result = new Dictionary<string, HashSet<string>>();

        // Get item from db
        var itemDb = itemHelper.GetItem(itemTpl).Value;
        if (itemDb.Properties.Slots is not null)
        // Loop over slots flagged as 'required'
        {
            foreach (
                var slot in itemDb.Properties.Slots.Where(slot =>
                    slot.Required.GetValueOrDefault(false)
                )
            )
            {
                // Create dict entry for mod slot
                result.TryAdd(slot.Name, []);

                // Add compatible tpls to dicts hashset
                foreach (var compatibleItemTpl in slot.Props.Filters.FirstOrDefault().Filter)
                {
                    result[slot.Name].Add(compatibleItemTpl);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Create weapon mod pool and set generated flag to true
    /// </summary>
    protected ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    > GenerateWeaponPool()
    {
        var weaponsAndMods = databaseService
            .GetItems()
            .Values.Where(item =>
                string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && itemHelper.IsOfBaseclasses(item.Id, [BaseClasses.WEAPON, BaseClasses.MOD])
            );
        logger.Warning("generating weapon pool");

        return GeneratePool(weaponsAndMods, "weapon");
    }

    /// <summary>
    ///     Create gear mod pool and set generated flag to true
    /// </summary>
    protected ConcurrentDictionary<
        string,
        ConcurrentDictionary<string, HashSet<string>>
    > GenerateGearPool()
    {
        var gearAndMods = databaseService
            .GetItems()
            .Values.Where(item =>
                string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && itemHelper.IsOfBaseclasses(
                    item.Id,
                    [
                        BaseClasses.ARMORED_EQUIPMENT,
                        BaseClasses.VEST,
                        BaseClasses.ARMOR,
                        BaseClasses.HEADWEAR,
                        BaseClasses.MOD,
                    ]
                )
            );

        return GeneratePool(gearAndMods, "gear");
    }
}
