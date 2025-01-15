using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Helpers;
using Core.Models.Spt.Server;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using System.Collections.Generic;

namespace Core.Generators;

[Injectable]
public class BotEquipmentModGenerator
{
    private readonly ISptLogger<BotEquipmentModGenerator> _logger;
    private readonly HashUtil _hashUtil;
    private readonly RandomUtil _randomUtil;
    private readonly ItemHelper _itemHelper;
    private readonly BotGeneratorHelper _botGeneratorHelper;
    private readonly BotEquipmentModPoolService _botEquipmentModPoolService;
    private readonly PresetHelper _presetHelper;
    private readonly ProbabilityHelper _probabilityHelper;
    private readonly LocalisationService _localisationService;
    private readonly ItemFilterService _itemFilterService;
    private readonly ConfigServer _configServer;
    private readonly ICloner _cloner;
    private BotConfig _botConfig;

    public BotEquipmentModGenerator(
        ISptLogger<BotEquipmentModGenerator> logger,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        ItemHelper itemHelper,
        BotGeneratorHelper botGeneratorHelper,
        BotEquipmentModPoolService botEquipmentModPoolService,
        PresetHelper presetHelper,
        ProbabilityHelper probabilityHelper,
        LocalisationService localisationService,
        ItemFilterService itemFilterService,
        ConfigServer configServer,
        ICloner cloner)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _itemHelper = itemHelper;
        _botGeneratorHelper = botGeneratorHelper;
        _botEquipmentModPoolService = botEquipmentModPoolService;
        _presetHelper = presetHelper;
        _probabilityHelper = probabilityHelper;
        _localisationService = localisationService;
        _itemFilterService = itemFilterService;
        _configServer = configServer;
        _cloner = cloner;

        _botConfig = _configServer.GetConfig<BotConfig>();
    }

    /// <summary>
    /// Check mods are compatible and add to array
    /// </summary>
    /// <param name="equipment">Equipment item to add mods to</param>
    /// <param name="parentId">Mod list to choose from</param>
    /// <param name="parentTemplate">parentid of item to add mod to</param>
    /// <param name="settings">Template object of item to add mods to</param>
    /// <param name="specificBlacklist">The relevant blacklist from bot.json equipment dictionary</param>
    /// <param name="shouldForceSpawn">should this mod be forced to spawn</param>
    /// <returns>Item + compatible mods as an array</returns>
    public List<Item> GenerateModsForEquipment(List<Item> equipment, string parentId, TemplateItem parentTemplate, GenerateEquipmentProperties settings,
        EquipmentFilterDetails specificBlacklist, bool shouldForceSpawn = false)
    {
        var forceSpawn = shouldForceSpawn;
        
        // Get mod pool for the desired item
        var compatibleModsPool = settings.ModPool[parentTemplate.Id];
        if (compatibleModsPool is null)
        {
            _logger.Warning($"bot: { settings.BotData.Role} lacks a mod slot pool for item: { parentTemplate.Id} { parentTemplate.Name}");
        }

        // Iterate over mod pool and choose mods to add to item
        foreach (var modSlotKvP in compatibleModsPool)
        {
            var modSlotName = modSlotKvP.Key;
            // Get the templates slot object from db
            var itemSlotTemplate = GetModItemSlotFromDb(modSlotName, parentTemplate);
            if (itemSlotTemplate is null)
            {
                _logger.Error(_localisationService.GetText("bot-mod_slot_missing_from_item", new {
                modSlot = modSlotName,
                        parentId= parentTemplate.Id,
                        parentName= parentTemplate.Name,
                        botRole= settings.BotData.Role
                    }));

                continue;
            }

            var modSpawnResult = ShouldModBeSpawned(
                itemSlotTemplate,
                modSlotName,
                settings.SpawnChances.EquipmentModsChances,
                settings.BotEquipmentConfig);

            // Rolled to skip mod and it shouldnt be force-spawned
            if (modSpawnResult == ModSpawn.SKIP && !forceSpawn)
            {
                continue;
            }

            // Ensure submods for nvgs all spawn together
            if (modSlotName == "mod_nvg")
            {
                forceSpawn = true;
            }

            // Get pool of items we can add for this slot
            var modPoolToChooseFrom = modSlotKvP.Value;

            // Filter the pool of items in blacklist
            var filteredModPool = FilterModsByBlacklist(modPoolToChooseFrom, specificBlacklist, modSlotName);
            if (filteredModPool.Count > 0)
            {
                // use filtered pool as it has items in it
                modPoolToChooseFrom = filteredModPool;
            }

            // Slot can hold armor plates + we are filtering possible items by bot level, handle
            if (
                settings.BotEquipmentConfig.FilterPlatesByLevel.GetValueOrDefault(false) &&
                _itemHelper.IsRemovablePlateSlot(modSlotName.ToLower())
            )
            {
                var plateSlotFilteringOutcome = FilterPlateModsForSlotByLevel(
                    settings,
                    modSlotName.ToLower(),
                    compatibleModsPool[modSlotName],
                    parentTemplate);
                if (plateSlotFilteringOutcome.Result is Result.UNKNOWN_FAILURE or Result.NO_DEFAULT_FILTER)
                {
                    _logger.Debug($"Plate slot: {modSlotName} selection for armor: {parentTemplate.Id} failed: {plateSlotFilteringOutcome.Result}, skipping");

                    continue;
                }

                if (plateSlotFilteringOutcome.Result == Result.LACKS_PLATE_WEIGHTS)
                {
                    _logger.Warning($"Plate slot: {modSlotName} lacks weights for armor: { parentTemplate.Id}, unable to adjust plate choice, using existing data");
                }

                // Replace mod pool with pool of chosen plate items
                modPoolToChooseFrom = plateSlotFilteringOutcome.PlateModTemplates;
            }

            // Choose random mod from pool and check its compatibility
            string modTpl = null;
            var found = false;
            var exhaustableModPool = CreateExhaustableArray(modPoolToChooseFrom);
            while (exhaustableModPool.HasValues())
            {
                modTpl = exhaustableModPool.GetRandomValue();
                if (modTpl is not null && !_botGeneratorHelper.IsItemIncompatibleWithCurrentItems(equipment, modTpl, modSlotName).Incompatible.GetValueOrDefault(false))
                {
                    found = true;
                    break;
                }
            }

            // Compatible item not found but slot REQUIRES item, get random item from db
            if (!found && itemSlotTemplate.Required.GetValueOrDefault(false))
            {
                modTpl = GetRandomModTplFromItemDb(modTpl, itemSlotTemplate, modSlotName, equipment);
                found = modTpl is not null;
            }

            // Compatible item not found + not required - skip
            if (!(found || itemSlotTemplate.Required.GetValueOrDefault(false)))
            {
                continue;
            }

            // Get chosen mods db template and check it fits into slot
            var modTemplate = _itemHelper.GetItem(modTpl);
            if (
                !IsModValidForSlot(
                    modTemplate,
                    itemSlotTemplate,
                    modSlotName,
                    parentTemplate,
                    settings.BotData.Role)
            )
            {
                continue;
            }

            // Generate new id to ensure all items are unique on bot
            var modId = _hashUtil.Generate();
            equipment.Add(
                CreateModItem(modId, modTpl, parentId, modSlotName, modTemplate.Value, settings.BotData.Role));

            // Does item being added exist in mod pool - has its own mod pool
            if (settings.ModPool.ContainsKey(modTpl))
            {
                // Call self again with mod being added as item to add child mods to
                GenerateModsForEquipment(
                    equipment,
                    modId,
                    modTemplate.Value,
                    settings,
                    specificBlacklist,
                    forceSpawn);
            }
        }

        return equipment;
    }

    /// <summary>
    /// Filter a bots plate pool based on its current level
    /// </summary>
    /// <param name="settings">Bot equipment generation settings</param>
    /// <param name="modSlot">Armor slot being filtered</param>
    /// <param name="existingPlateTplPool">Plates tpls to choose from</param>
    /// <param name="armorItem">The armor items db template</param>
    /// <returns>Array of plate tpls to choose from</returns>
    public FilterPlateModsForSlotByLevelResult FilterPlateModsForSlotByLevel(GenerateEquipmentProperties settings, string modSlot,
        List<string> existingPlateTplPool, TemplateItem armorItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add mods to a weapon using the provided mod pool
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">Data used to generate the weapon</param>
    /// <returns>Weapon + mods array</returns>
    public List<Item> GenerateModsForWeapon(string sessionId, GenerateWeaponRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Should the provided bot have its stock chance values altered to 100%
    /// </summary>
    /// <param name="modSlot">Slot to check</param>
    /// <param name="botEquipConfig">Bots equipment config/chance values</param>
    /// <param name="modToAddTemplate">Mod being added to bots weapon</param>
    /// <returns>True if it should</returns>
    public bool ShouldForceSubStockSlots(string modSlot, EquipmentFilters botEquipConfig, TemplateItem modToAddTemplate)
    {
        // Slots a weapon can store its stock in
        string[] stockSlots = ["mod_stock", "mod_stock_000", "mod_stock_001", "mod_stock_akms"];

        // Can the stock hold child items
        var hasSubSlots = modToAddTemplate.Properties.Slots?.Count > 0;

        return (stockSlots.Contains(modSlot) && hasSubSlots) || botEquipConfig.ForceStock.GetValueOrDefault(false);
    }

    /// <summary>
    /// Is this modslot a front or rear sight
    /// </summary>
    /// <param name="modSlot">Slot to check</param>
    /// <param name="tpl"></param>
    /// <returns>true if it's a front/rear sight</returns>
    public bool ModIsFrontOrRearSight(string modSlot, string tpl)
    {
        // Gas block /w front sight is special case, deem it a 'front sight' too
        if (modSlot == "mod_gas_block" && tpl == "5ae30e795acfc408fb139a0b")
        {
            // M4A1 front sight with gas block
            return true;
        }

        return ((string[])["mod_sight_front", "mod_sight_rear"]).Contains(modSlot);
    }

    /// <summary>
    /// Does the provided mod details show the mod can hold a scope
    /// </summary>
    /// <param name="modSlot">e.g. mod_scope, mod_mount</param>
    /// <param name="modsParentId">Parent id of mod item</param>
    /// <returns>true if it can hold a scope</returns>
    public bool ModSlotCanHoldScope(string modSlot, string modsParentId)
    {
        return (
            ((string[])[
                "mod_scope",
                "mod_mount",
                "mod_mount_000",
                "mod_scope_000",
                "mod_scope_001",
                "mod_scope_002",
                "mod_scope_003",
            ]).Contains(modSlot.ToLower()) && modsParentId == BaseClasses.MOUNT
        );
    }

    /// <summary>
    /// Set mod spawn chances to defined amount
    /// </summary>
    /// <param name="modSpawnChances">Chance dictionary to update</param>
    /// <param name="modSlotsToAdjust"></param>
    /// <param name="newChancePercent"></param>
    public void AdjustSlotSpawnChances(Dictionary<string, double>? modSpawnChances, List<string>? modSlotsToAdjust, double newChancePercent)
    {
        if (modSpawnChances is null)
        {
            _logger.Warning("AdjustSlotSpawnChances() modSpawnChances missing");

            return;
        }

        if (modSlotsToAdjust is null)
        {
            _logger.Warning("AdjustSlotSpawnChances() modSlotsToAdjust missing");

            return;
        }

        foreach (var modName in modSlotsToAdjust) {
            modSpawnChances[modName] = newChancePercent;
        }
    }

    /// <summary>
    /// Does the provided modSlot allow muzzle-related items
    /// </summary>
    /// <param name="modSlot">Slot id to check</param>
    /// <param name="modsParentId">OPTIONAL: parent id of modslot being checked</param>
    /// <returns>True if modSlot can have muzzle-related items</returns>
    public bool ModSlotCanHoldMuzzleDevices(string modSlot, string? modsParentId)
    {
        return ((string[])["mod_muzzle", "mod_muzzle_000", "mod_muzzle_001"]).Contains(modSlot.ToLower());
    }

    /// <summary>
    /// Sort mod slots into an ordering that maximises chance of a successful weapon generation
    /// </summary>
    /// <param name="unsortedSlotKeys">Array of mod slot strings to sort</param>
    /// <param name="itemTplWithKeysToSort">The Tpl of the item with mod keys being sorted</param>
    /// <returns>Sorted array</returns>
    public List<string> SortModKeys(List<string> unsortedSlotKeys, string itemTplWithKeysToSort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a Slot property for an item (chamber/cartridge/slot)
    /// </summary>
    /// <param name="modSlot">e.g patron_in_weapon</param>
    /// <param name="parentTemplate">item template</param>
    /// <returns>Slot item</returns>
    public Slot? GetModItemSlotFromDb(string modSlot, TemplateItem parentTemplate)
    {
        var modSlotLower = modSlot.ToLower();
        switch (modSlotLower)
        {
            case "patron_in_weapon":
            case "patron_in_weapon_000":
            case "patron_in_weapon_001":
                return parentTemplate.Properties.Chambers.FirstOrDefault((chamber) => chamber.Name.Contains(modSlotLower));
            case "cartridges":
                return parentTemplate.Properties.Cartridges.FirstOrDefault((c) => c.Name.ToLower() == modSlotLower);
            default:
                return parentTemplate.Properties.Slots.FirstOrDefault((s) => s.Name.ToLower() == modSlotLower);
        }
    }

    /// <summary>
    /// Randomly choose if a mod should be spawned, 100% for required mods OR mod is ammo slot
    /// </summary>
    /// <param name="itemSlot">slot the item sits in from db</param>
    /// <param name="modSlotName">Name of slot the mod sits in</param>
    /// <param name="modSpawnChances">Chances for various mod spawns</param>
    /// <param name="botEquipConfig">Various config settings for generating this type of bot</param>
    /// <returns>ModSpawn.SPAWN when mod should be spawned, ModSpawn.DEFAULT_MOD when default mod should spawn, ModSpawn.SKIP when mod is skipped</returns>
    public ModSpawn ShouldModBeSpawned(Slot itemSlot, string modSlotName, Dictionary<string, double> modSpawnChances, EquipmentFilters botEquipConfig)
    {
        var slotRequired = itemSlot.Required;
        if (GetAmmoContainers().Contains(modSlotName))
        {
            // Always force mags/cartridges in weapon to spawn
            return ModSpawn.SPAWN;
        }
        var spawnMod = _probabilityHelper.RollChance(modSpawnChances[modSlotName]);
        if (!spawnMod && (slotRequired.GetValueOrDefault(false) || botEquipConfig.WeaponSlotIdsToMakeRequired.Contains(modSlotName)))
        {
            // Edge case: Mod is required but spawn chance roll failed, choose default mod spawn for slot
            return ModSpawn.DEFAULT_MOD;
        }

        return spawnMod ? ModSpawn.SPAWN : ModSpawn.SKIP;
    }

    /// <summary>
    /// Choose a mod to fit into the desired slot
    /// </summary>
    /// <param name="request">Data used to choose an appropriate mod with</param>
    /// <returns>itemHelper.getItem() result</returns>
    public object? ChooseModToPutIntoSlot(ModToSpawnRequest request) // TODO: type fuckery: [boolean, ITemplateItem] | undefined
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Given the passed in array of magaizne tpls, look up the min size set in config and return only those that have that size or larger
    /// </summary>
    /// <param name="modSpawnRequest">Request data</param>
    /// <param name="modPool">Pool of magazine tpls to filter</param>
    /// <returns>Filtered pool of magazine tpls</returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<string> GetFilterdMagazinePoolByCapacity(ModToSpawnRequest modSpawnRequest, List<string> modPool)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a weapon mod tpl for a given slot from a pool of choices
    /// Checks chosen tpl is compatible with all existing weapon items
    /// </summary>
    /// <param name="request"></param>
    /// <param name="modPool">Pool of mods that can be picked from</param>
    /// <param name="parentSlot">Slot the picked mod will have as a parent</param>
    /// <param name="choiceTypeEnum">How should chosen tpl be treated: DEFAULT_MOD/SPAWN/SKIP</param>
    /// <param name="weapon">Array of weapon items chosen item will be added to</param>
    /// <param name="modSlotName">Name of slot picked mod will be placed into</param>
    /// <returns>Chosen weapon details</returns>
    public ChooseRandomCompatibleModResult GetCompatibleWeaponModTplForSlotFromPool(ModToSpawnRequest request, List<string> modPool, Slot parentSlot,
        ModSpawn choiceTypeEnum, List<Item> weapon, string modSlotName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modPool">Pool of item Tpls to choose from</param>
    /// <param name="modSpawnType">How should the slot choice be handled - forced/normal etc</param>
    /// <param name="weapon">Weapon mods at current time</param>
    /// <returns>IChooseRandomCompatibleModResult</returns>
    public ChooseRandomCompatibleModResult GetCompatibleModFromPool(List<string> modPool, ModSpawn modSpawnType, List<Item> weapon)
    {
        throw new NotImplementedException();
    }

    public ExhaustableArray<T> CreateExhaustableArray<T>(List<T> itemsToAddToArray) // TODO: this wont likely be needed, reimplement for C#
    {
        return new ExhaustableArray<T>(itemsToAddToArray, _randomUtil, _cloner);
    }

    /// <summary>
    /// Get a list of mod tpls that are compatible with the current weapon
    /// </summary>
    /// <param name="modPool"></param>
    /// <param name="tplBlacklist">Tpls that are incompatible and should not be used</param>
    /// <returns>string array of compatible mod tpls with weapon</returns>
    public List<string> GetFilteredModPool(List<string> modPool, List<string> tplBlacklist)
    {
        return modPool.Where((tpl) => !tplBlacklist.Contains(tpl)).ToList();
    }

    /// <summary>
    /// Filter mod pool down based on various criteria:
    /// Is slot flagged as randomisable
    /// Is slot required
    /// Is slot flagged as default mod only
    /// </summary>
    /// <param name="request"></param>
    /// <param name="weaponTemplate">Mods root parent (weapon/equipment)</param>
    /// <returns>Array of mod tpls</returns>
    public List<string> GetModPoolForSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        // Mod is flagged as being default only, try and find it in globals
        if (request.ModSpawnResult == ModSpawn.DEFAULT_MOD)
        {
            return GetModPoolForDefaultSlot(request, weaponTemplate);
        }

        if (request.IsRandomisableSlot.GetValueOrDefault(false))
        {
            return GetDynamicModPool(request.ParentTemplate.Id, request.ModSlot, request.BotEquipBlacklist);
        }

        // Required mod is not default or randomisable, use existing pool
        return request.ItemModPool[request.ModSlot];
    }

    public List<string> GetModPoolForDefaultSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    public Item GetMatchingModFromPreset(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        var matchingPreset = GetMatchingPreset(weaponTemplate, request.ParentTemplate.Id);
        return matchingPreset?.Items.FirstOrDefault((item) => item?.SlotId?.ToLower() == request.ModSlot.ToLower());
    }

    /// <summary>
    /// Get default preset for weapon OR get specific weapon presets for edge cases (mp5/silenced dvl)
    /// </summary>
    /// <param name="weaponTemplate">Weapons db template</param>
    /// <param name="parentItemTpl">Tpl of the parent item</param>
    /// <returns>Default preset found</returns>
    public Preset? GetMatchingPreset(TemplateItem weaponTemplate, string parentItemTpl)
    {
        // Edge case - using mp5sd reciever means default mp5 handguard doesn't fit
        var isMp5sd = parentItemTpl == "5926f2e086f7745aae644231";
        if (isMp5sd)
        {
            return _presetHelper.GetPreset("59411abb86f77478f702b5d2");
        }

        // Edge case - dvl 500mm is the silenced barrel and has specific muzzle mods
        var isDvl500mmSilencedBarrel = parentItemTpl == "5888945a2459774bf43ba385";
        if (isDvl500mmSilencedBarrel)
        {
            return _presetHelper.GetPreset("59e8d2b386f77445830dd299");
        }

        return _presetHelper.GetDefaultPreset(weaponTemplate.Id);
    }

    /// <summary>
    /// Temp fix to prevent certain combinations of weapons with mods that are known to be incompatible
    /// </summary>
    /// <param name="weapon">Array of items that make up a weapon</param>
    /// <param name="modTpl">Mod to check compatibility with weapon</param>
    /// <returns>True if incompatible</returns>
    public bool WeaponModComboIsIncompatible(List<Item> weapon, string modTpl)
    {
        // STM-9 + AR-15 Lone Star Ion Lite handguard
        if (weapon[0].Template == "60339954d62c9b14ed777c06" && modTpl == "5d4405f0a4b9361e6a4e6bd9")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Create a mod item with provided parameters as properties + add upd property
    /// </summary>
    /// <param name="modId">_id</param>
    /// <param name="modTpl">_tpl</param>
    /// <param name="parentId">parentId</param>
    /// <param name="modSlot">slotId</param>
    /// <param name="modTemplate">Used to add additional properties in the upd object</param>
    /// <param name="botRole">The bots role mod is being created for</param>
    /// <returns>Item object</returns>
    public Item CreateModItem(string modId, string modTpl, string parentId, string modSlot, TemplateItem modTemplate, string botRole)
    {
        return new Item {
            Id = modId,
            Template = modTpl,
            ParentId = parentId,
            SlotId = modSlot,
            Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(modTemplate, botRole)};
    }

    /// <summary>
    /// Get a list of containers that hold ammo
    /// e.g. mod_magazine / patron_in_weapon_000
    /// </summary>
    /// <returns>string array</returns>
    public List<string> GetAmmoContainers()
    {
        return ["mod_magazine", "patron_in_weapon", "patron_in_weapon_000", "patron_in_weapon_001", "cartridges"];
    }

    /// <summary>
    /// Get a random mod from an items compatible mods Filter array
    /// </summary>
    /// <param name="fallbackModTpl">Default value to return if parentSlot Filter is empty</param>
    /// <param name="parentSlot">Item mod will go into, used to get compatible items</param>
    /// <param name="modSlot">Slot to get mod to fill</param>
    /// <param name="items">Items to ensure picked mod is compatible with</param>
    /// <returns>Item tpl</returns>
    public string? GetRandomModTplFromItemDb(string fallbackModTpl, Slot parentSlot, string modSlot, List<Item> items)
    {
        // Find compatible mods and make an array of them
        var allowedItems = parentSlot.Props.Filters[0].Filter;

        // Find mod item that fits slot from sorted mod array
        var exhaustableModPool = CreateExhaustableArray(allowedItems);
        var tmpModTpl = fallbackModTpl;
        while (exhaustableModPool.HasValues())
        {
            tmpModTpl = exhaustableModPool.GetRandomValue();
            if (!_botGeneratorHelper.IsItemIncompatibleWithCurrentItems(items, tmpModTpl, modSlot).Incompatible.GetValueOrDefault(false))
            {
                return tmpModTpl;
            }
        }

        // No mod found
        return null;
    }

    /// <summary>
    /// Check if mod exists in db + is for a required slot
    /// </summary>
    /// <param name="modtoAdd">Db template of mod to check</param>
    /// <param name="slotAddedToTemplate">Slot object the item will be placed as child into</param>
    /// <param name="modSlot">Slot the mod will fill</param>
    /// <param name="parentTemplate">Db template of the mods being added</param>
    /// <param name="botRole">Bots wildspawntype (assault/pmcBot/exUsec etc)</param>
    /// <returns>True if valid for slot</returns>
    public bool IsModValidForSlot(KeyValuePair<bool, TemplateItem?> modToAdd, Slot slotAddedToTemplate, string modSlot, TemplateItem parentTemplate, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find mod tpls of a provided type and add to modPool
    /// </summary>
    /// <param name="desiredSlotName">Slot to look up and add we are adding tpls for (e.g mod_scope)</param>
    /// <param name="modTemplate">db object for modItem we get compatible mods from</param>
    /// <param name="modPool">Pool of mods we are adding to</param>
    /// <param name="botEquipBlacklist">A blacklist of items that cannot be picked</param>
    public void AddCompatibleModsForProvidedMod(string desiredSlotName, TemplateItem modTemplate, Dictionary<string, Dictionary<string, List<string>>> modPool,
        EquipmentFilterDetails botEquipBlacklist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the possible items that fit a slot
    /// </summary>
    /// <param name="parentItemId">item tpl to get compatible items for</param>
    /// <param name="modSlot">Slot item should fit in</param>
    /// <param name="botEquipBlacklist">Equipment that should not be picked</param>
    /// <returns>Array of compatible items for that slot</returns>
    public List<string> GetDynamicModPool(string parentItemId, string modSlot, EquipmentFilterDetails botEquipBlacklist)
    {
        var modsFromDynamicPool = _cloner.Clone(
            _botEquipmentModPoolService.GetCompatibleModsForWeaponSlot(parentItemId, modSlot)
        );

        var filteredMods = FilterModsByBlacklist(modsFromDynamicPool, botEquipBlacklist, modSlot);
        if (!filteredMods.Any())
        {
            _logger.Warning(_localisationService.GetText("bot-unable_to_filter_mod_slot_all_blacklisted", modSlot));

            return modsFromDynamicPool;
        }

        return filteredMods;
    }

    /// <summary>
    /// Take a list of tpls and filter out blacklisted values using itemFilterService + botEquipmentBlacklist
    /// </summary>
    /// <param name="allowedMods">Base mods to filter</param>
    /// <param name="botEquipBlacklist">Equipment blacklist</param>
    /// <param name="modSlot">Slot mods belong to</param>
    /// <returns>Filtered array of mod tpls</returns>
    public List<string> FilterModsByBlacklist(List<string> allowedMods, EquipmentFilterDetails? botEquipBlacklist, string modSlot)
    {
        // No blacklist, nothing to filter out
        if (botEquipBlacklist is null)
        {
            return allowedMods;
        }

        var result = new List<string>();

        // Get item blacklist and mod equipment blacklist as one array
        var blacklist = _itemFilterService.GetBlacklistedItems().Concat(botEquipBlacklist.Equipment[modSlot]);
        result = allowedMods.Where((tpl) => !blacklist.Contains(tpl)).ToList();

        return result;
    }

    /// <summary>
    /// With the shotgun revolver (60db29ce99594040e04c4a27) 12.12 introduced CylinderMagazines.
    /// Those magazines (e.g. 60dc519adf4c47305f6d410d) have a "Cartridges" entry with a _max_count=0.
    /// Ammo is not put into the magazine directly but assigned to the magazine's slots: The "camora_xxx" slots.
    /// This function is a helper called by generateModsForItem for mods with parent type "CylinderMagazine"
    /// </summary>
    /// <param name="items">The items where the CylinderMagazine's camora are appended to</param>
    /// <param name="modPool">ModPool which should include available cartridges</param>
    /// <param name="cylinderMagParentId">The CylinderMagazine's UID</param>
    /// <param name="cylinderMagTemplate">The CylinderMagazine's template</param>
    public void FillCamora(List<Item> items, Dictionary<string, Dictionary<string, List<string>>> modPool, string cylinderMagParentId,
        TemplateItem cylinderMagTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take a record of camoras and merge the compatible shells into one array
    /// </summary>
    /// <param name="camorasWithShells">Dictionary of camoras we want to merge into one array</param>
    /// <returns>String array of shells for multiple camora sources</returns>
    public List<string> MergeCamoraPools(Dictionary<string, List<string>> camorasWithShells)
    {
        var uniqueShells = new HashSet<string>();
        foreach (var shell in camorasWithShells
                     .SelectMany(shellKvP => shellKvP.Value))
        {
            uniqueShells.Add(shell);
        }

        return uniqueShells.ToList();
    }

    /// <summary>
    /// Filter out non-whitelisted weapon scopes
    /// Controlled by bot.json weaponSightWhitelist
    /// e.g. filter out rifle scopes from SMGs
    /// </summary>
    /// <param name="weapon">Weapon scopes will be added to</param>
    /// <param name="scopes">Full scope pool</param>
    /// <param name="botWeaponSightWhitelist">Whitelist of scope types by weapon base type</param>
    /// <returns>Array of scope tpls that have been filtered to just ones allowed for that weapon type</returns>
    public List<string> FilterSightsByWeaponType(Item weapon, List<string> scopes, Dictionary<string, List<string>> botWeaponSightWhitelist)
    {
        throw new NotImplementedException();
    }
}
