using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;
using Core.Helpers;
using Core.Servers;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentModPoolService
{
    protected ISptLogger<BotEquipmentModPoolService> _logger;
    protected ItemHelper _itemHelper;
    protected DatabaseService _databaseService;
    protected LocalisationService _localisationService;
    protected ConfigServer _configServer;

    protected bool _weaponPoolGenerated;
    protected bool _armorPoolGenerated;
    protected Dictionary<string, Dictionary<string, HashSet<string>>> _weaponModPool;
    protected Dictionary<string, Dictionary<string, HashSet<string>>> _gearModPool;
    protected BotConfig _botConfig;

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

        _weaponModPool = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        _gearModPool = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    }

    /**
     * Store dictionary of mods for each item passed in
     * @param items items to find related mods and store in modPool
     */
    protected void GeneratePool(IEnumerable<TemplateItem>? items, string poolType)
    {
        if (items is null)
        {
            _logger.Error(_localisationService.GetText("bot-unable_to_generate_item_pool_no_items", poolType));

            return;
        }

        // Get weapon or gear pool
        var pool = poolType == "weapon" ? _weaponModPool : _gearModPool;
        foreach (var item in items) {
            if (item.Properties is null)
            {
                _logger.Error(_localisationService.GetText("bot-item_missing_props_property", new {
                    itemTpl = item.Id,
                    name = item.Name,
                }));

                continue;
            }

            // Skip item without slots
            if (item.Properties.Slots is null || item.Properties.Slots.Count == 0)
            {
                continue;
            }

            // Add base item (weapon/armor) to pool
            if (!pool.ContainsKey(item.Id))
            {
                pool[item.Id] = new Dictionary<string, HashSet<string>>();
            }

            // iterate over each items mod slots e.g. mod_muzzle
            foreach (var slot in item.Properties.Slots) {
                // Get mods that fit into the current mod slot
                var itemsThatFit = slot.Props.Filters.FirstOrDefault().Filter;

                // Get weapon/armor pool to add mod slots + mod tpls to
                
                foreach (var itemToAddTpl in itemsThatFit)
                {
                    if (!pool[item.Id].ContainsKey(slot.Name))
                    {
                        // Ensure Mod slot key + blank dict value exist
                        pool[item.Id][slot.Name] = new();
                    }

                    if (!pool[item.Id][slot.Name].Contains(itemToAddTpl))
                    {
                        // Add tpl to list keyed by mod slot
                        pool[item.Id][slot.Name].Add(itemToAddTpl);
                    }

                    var subItemDetails = _itemHelper.GetItem(itemToAddTpl).Value;
                    var hasSubItemsToAdd = (subItemDetails?.Properties?.Slots?.Count ?? 0) > 0;
                    if (hasSubItemsToAdd && !pool.ContainsKey(subItemDetails.Id))
                    {
                        // Recursive call
                        GeneratePool([subItemDetails], poolType);
                    }
                }
            }
        }
    }

    /**
     * Empty the mod pool
     */
    public void ResetPool()
    {
        throw new NotImplementedException();
    }

    /**
     * Get array of compatible mods for an items mod slot (generate pool if it doesn't exist already)
     * @param itemTpl item to look up
     * @param slotName slot to get compatible mods for
     * @returns tpls that fit the slot
     */
    public HashSet<string> GetCompatibleModsForWeaponSlot(string itemTpl, string slotName)
    {
        if (!_weaponPoolGenerated)
        {
            // Get every weapon in db and generate mod pool
            GenerateWeaponPool();
        }

        return _weaponModPool[itemTpl][slotName];
    }

    /**
     * Get mods for a piece of gear by its tpl
     * @param itemTpl items tpl to look up mods for
     * @returns Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value
     */
    public Dictionary<string, HashSet<string>>? GetModsForGearSlot(string itemTpl)
    {
        if (!_armorPoolGenerated)
        {
            GenerateGearPool();
        }

        return _gearModPool.TryGetValue(itemTpl, out var value)
            ? value
            : null;
    }

    /**
     * Get mods for a weapon by its tpl
     * @param itemTpl Weapons tpl to look up mods for
     * @returns Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value
     */
    public Dictionary<string, HashSet<string>> GetModsForWeaponSlot(string itemTpl)
    {
        if (!_weaponPoolGenerated)
        {
            GenerateWeaponPool();
        }

        return _weaponModPool[itemTpl];
    }

    /**
     * Create weapon mod pool and set generated flag to true
     */
    protected void GenerateWeaponPool()
    {
        var weapons = _databaseService.GetItems().Values.Where(
            (item) => item.Type == "Item" && _itemHelper.IsOfBaseclass(item.Id, BaseClasses.WEAPON));
        GeneratePool(weapons, "weapon");

        // Flag pool as being complete
        _weaponPoolGenerated = true;
    }

    /**
     * Create gear mod pool and set generated flag to true
     */
    protected void GenerateGearPool()
    {
        var gear = _databaseService.GetItems().Values.Where(
            (item) => item.Type == "Item" 
                      && _itemHelper.IsOfBaseclasses(item.Id, [
                                         BaseClasses.ARMORED_EQUIPMENT,
                                         BaseClasses.VEST,
                                         BaseClasses.ARMOR,
                                         BaseClasses.HEADWEAR,
                                     ]));
        GeneratePool(gear, "gear");

        // Flag pool as being complete
        _armorPoolGenerated = true;
    }
}
