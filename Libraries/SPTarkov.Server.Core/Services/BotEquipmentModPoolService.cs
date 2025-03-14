using System.Collections.Concurrent;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentModPoolService
{
    private readonly Lock _lockObject = new();
    protected bool _armorPoolGenerated;
    protected BotConfig _botConfig;
    protected ConfigServer _configServer;
    protected DatabaseService _databaseService;
    protected ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>> _gearModPool;
    protected ItemHelper _itemHelper;
    protected LocalisationService _localisationService;
    protected ISptLogger<BotEquipmentModPoolService> _logger;
    protected ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>> _weaponModPool;

    protected bool _weaponPoolGenerated;

    public BotEquipmentModPoolService(
        ISptLogger<BotEquipmentModPoolService> logger,
        ItemHelper itemHelper,
        DatabaseService databaseService,
        LocalisationService localisationService,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _itemHelper = itemHelper;
        _databaseService = databaseService;
        _localisationService = localisationService;
        _configServer = configServer;
        _botConfig = _configServer.GetConfig<BotConfig>();

        _weaponModPool = new ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>>();
        _gearModPool = new ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>>();
    }

    /// <summary>
    /// Store dictionary of mods for each item passed in
    /// </summary>
    /// <param name="items"> Items to find related mods and store in modPool </param>
    /// <param name="poolType"> Mod pool to choose from e.g. "weapon" for weaponModPool </param>
    protected void GeneratePool(IEnumerable<TemplateItem>? items, string poolType)
    {
        if (items is null)
        {
            _logger.Error(_localisationService.GetText("bot-unable_to_generate_item_pool_no_items", poolType));

            return;
        }

        // Get weapon or gear pool
        var pool = poolType == "weapon" ? _weaponModPool : _gearModPool;
        foreach (var item in items)
        {
            if (item.Properties is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "bot-item_missing_props_property",
                        new
                        {
                            itemTpl = item.Id,
                            name = item.Name
                        }
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

            // iterate over each items mod slots e.g. mod_muzzle
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

                    var subItemDetails = _itemHelper.GetItem(itemToAddTpl).Value;
                    var hasSubItemsToAdd = (subItemDetails?.Properties?.Slots?.Count ?? 0) > 0;
                    if (hasSubItemsToAdd && !pool.ContainsKey(subItemDetails.Id))
                        // Recursive call
                    {
                        GeneratePool([subItemDetails], poolType);
                    }
                }
            }
        }
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

    private bool InitSetInDict(ConcurrentDictionary<string, HashSet<string>> dictionary, string slotName)
    {
        lock (_lockObject)
        {
            return dictionary.TryAdd(slotName, []);
        }
    }

    /// <summary>
    /// Empty the mod pool
    /// </summary>
    public void ResetPool()
    {
        _weaponModPool.Clear();
    }

    /// <summary>
    /// Get array of compatible mods for an items mod slot (generate pool if it doesn't exist already)
    /// </summary>
    /// <param name="itemTpl"> Item to look up </param>
    /// <param name="slotName"> Slot to get compatible mods for </param>
    /// <returns> Hashset of tpls that fit the slot </returns>
    public HashSet<string> GetCompatibleModsForWeaponSlot(string itemTpl, string slotName)
    {
        if (!_weaponPoolGenerated)
            // Get every weapon in db and generate mod pool
        {
            GenerateWeaponPool();
        }

        return _weaponModPool[itemTpl][slotName];
    }

    /// <summary>
    /// Get mods for a piece of gear by its tpl
    /// </summary>
    /// <param name="itemTpl"> Items tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public ConcurrentDictionary<string, HashSet<string>> GetModsForGearSlot(string itemTpl)
    {
        if (!_armorPoolGenerated)
        {
            GenerateGearPool();
        }

        return _gearModPool.TryGetValue(itemTpl, out var value)
            ? value
            : [];
    }

    /// <summary>
    /// Get mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public ConcurrentDictionary<string, HashSet<string>> GetModsForWeaponSlot(string itemTpl)
    {
        if (!_weaponPoolGenerated)
        {
            GenerateWeaponPool();
        }

        return _weaponModPool[itemTpl];
    }

    /// <summary>
    /// Get required mods for a weapon by its tpl
    /// </summary>
    /// <param name="itemTpl"> Weapons tpl to look up mods for </param>
    /// <returns> Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value </returns>
    public Dictionary<string, HashSet<string>>? GetRequiredModsForWeaponSlot(string itemTpl)
    {
        var result = new Dictionary<string, HashSet<string>>();

        // Get item from db
        var itemDb = _itemHelper.GetItem(itemTpl).Value;
        if (itemDb.Properties.Slots is not null)
            // Loop over slots flagged as 'required'
        {
            foreach (var slot in itemDb.Properties.Slots.Where(slot => slot.Required.GetValueOrDefault(false)))
            {
                // Create dict entry for mod slot
                result.Add(slot.Name, []);

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
    /// Create weapon mod pool and set generated flag to true
    /// </summary>
    protected void GenerateWeaponPool()
    {
        var weapons = _databaseService.GetItems()
            .Values.Where(
                item => string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase) && _itemHelper.IsOfBaseclass(item.Id, BaseClasses.WEAPON)
            );
        GeneratePool(weapons, "weapon");

        // Flag pool as being complete
        _weaponPoolGenerated = true;
    }

    /// <summary>
    /// Create gear mod pool and set generated flag to true
    /// </summary>
    protected void GenerateGearPool()
    {
        var gear = _databaseService.GetItems()
            .Values.Where(
                item => string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase) &&
                        _itemHelper.IsOfBaseclasses(
                            item.Id,
                            [
                                BaseClasses.ARMORED_EQUIPMENT,
                                BaseClasses.VEST,
                                BaseClasses.ARMOR,
                                BaseClasses.HEADWEAR
                            ]
                        )
            );
        GeneratePool(gear, "gear");

        // Flag pool as being complete
        _armorPoolGenerated = true;
    }
}
