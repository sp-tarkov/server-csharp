using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Helpers;
using Core.Models.Common;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Generators;

[Injectable]
public class BotEquipmentModGenerator(
    ISptLogger<BotEquipmentModGenerator> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    ProbabilityHelper _probabilityHelper,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    BotEquipmentFilterService _botEquipmentFilterService,
    ItemFilterService _itemFilterService,
    ProfileHelper _profileHelper,
    BotWeaponModLimitService _botWeaponModLimitService,
    BotHelper _botHelper,
    BotGeneratorHelper _botGeneratorHelper,
    BotWeaponGeneratorHelper _botWeaponGeneratorHelper,
    WeightedRandomHelper _weightedRandomHelper,
    PresetHelper _presetHelper,
    LocalisationService _localisationService,
    BotEquipmentModPoolService _botEquipmentModPoolService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

    /// <summary>
    /// Check mods are compatible and add to array
    /// </summary>
    /// <param name="equipment">Equipment item to add mods to</param>
    /// <param name="parentId">Mod list to choose from</param>
    /// <param name="parentTemplate">parentId of item to add mod to</param>
    /// <param name="settings">Template object of item to add mods to</param>
    /// <param name="specificBlacklist">The relevant blacklist from bot.json equipment dictionary</param>
    /// <param name="shouldForceSpawn">should this mod be forced to spawn</param>
    /// <returns>Item + compatible mods as an array</returns>
    public List<Item> GenerateModsForEquipment(List<Item> equipment, string parentId, TemplateItem parentTemplate, GenerateEquipmentProperties settings,
        EquipmentFilterDetails specificBlacklist, bool shouldForceSpawn = false)
    {
        var forceSpawn = shouldForceSpawn;

        // Get mod pool for the desired item
        if (!settings.ModPool.TryGetValue(parentTemplate.Id, out var compatibleModsPool))
        {
            _logger.Warning($"bot: {settings.BotData.Role} lacks a mod slot pool for item: {parentTemplate.Id} {parentTemplate.Name}");
        }

        // Iterate over mod pool and choose mods to add to item
        foreach (var (modSlotName, modPool) in compatibleModsPool ?? [])
        {
            // Get the templates slot object from db
            var itemSlotTemplate = GetModItemSlotFromDb(modSlotName, parentTemplate);
            if (itemSlotTemplate is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "bot-mod_slot_missing_from_item",
                        new
                        {
                            modSlot = modSlotName,
                            parentId = parentTemplate.Id,
                            parentName = parentTemplate.Name,
                            botRole = settings.BotData.Role
                        }
                    )
                );

                continue;
            }

            var modSpawnResult = ShouldModBeSpawned(
                itemSlotTemplate,
                modSlotName,
                settings.SpawnChances.EquipmentModsChances,
                settings.BotEquipmentConfig
            );

            // Rolled to skip mod and it shouldn't be force-spawned
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
            var modPoolToChooseFrom = modPool;

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
                    parentTemplate
                );
                switch (plateSlotFilteringOutcome.Result)
                {
                    case Result.UNKNOWN_FAILURE or Result.NO_DEFAULT_FILTER:
                        if (_logger.IsLogEnabled(LogLevel.Debug))
                        {
                            _logger.Debug(
                                $"Plate slot: {modSlotName} selection for armor: {parentTemplate.Id} failed: {plateSlotFilteringOutcome.Result}, skipping"
                            );
                        }

                        continue;
                    case Result.LACKS_PLATE_WEIGHTS:
                        _logger.Warning(
                            $"Plate slot: {modSlotName} lacks weights for armor: {parentTemplate.Id}, unable to adjust plate choice, using existing data"
                        );
                        break;
                }

                // Replace mod pool with pool of chosen plate items
                modPoolToChooseFrom = plateSlotFilteringOutcome.PlateModTemplates;
            }

            // Choose random mod from pool and check its compatibility
            string modTpl = null;
            var found = false;
            var exhaustableModPool = CreateExhaustableArray(modPoolToChooseFrom.ToList());
            while (exhaustableModPool.HasValues())
            {
                modTpl = exhaustableModPool.GetRandomValue();
                if (modTpl is not null &&
                    !_botGeneratorHelper.IsItemIncompatibleWithCurrentItems(equipment, modTpl, modSlotName).Incompatible.GetValueOrDefault(false))
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
                    settings.BotData.Role
                )
            )
            {
                continue;
            }

            // Generate new id to ensure all items are unique on bot
            var modId = _hashUtil.Generate();
            equipment.Add(
                CreateModItem(modId, modTpl, parentId, modSlotName, modTemplate.Value, settings.BotData.Role)
            );

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
                    forceSpawn
                );
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
        HashSet<string> existingPlateTplPool, TemplateItem armorItem)
    {
        var result = new FilterPlateModsForSlotByLevelResult
        {
            Result = Result.UNKNOWN_FAILURE,
            PlateModTemplates = null,
        };

        // Not pmc or not a plate slot, return original mod pool array
        if (!_itemHelper.IsRemovablePlateSlot(modSlot))
        {
            result.Result = Result.NOT_PLATE_HOLDING_SLOT;
            result.PlateModTemplates = existingPlateTplPool;

            return result;
        }

        // Get the front/back/side weights based on bots level
        var plateSlotWeights = settings.BotEquipmentConfig?.ArmorPlateWeighting.FirstOrDefault(
            (armorWeight) =>
                settings.BotData.Level >= armorWeight.LevelRange.Min &&
                settings.BotData.Level <= armorWeight.LevelRange.Max
        );

        if (plateSlotWeights is null)
        {
            // No weights, return original array of plate tpls
            result.Result = Result.LACKS_PLATE_WEIGHTS;
            result.PlateModTemplates = existingPlateTplPool;

            return result;
        }

        // Get the specific plate slot weights (front/back/side)
        if (!plateSlotWeights.Values.TryGetValue(modSlot, out var plateWeights))
        {
            // No weights, return original array of plate tpls
            result.Result = Result.LACKS_PLATE_WEIGHTS;
            result.PlateModTemplates = existingPlateTplPool;

            return result;
        }

        // Choose a plate level based on weighting
        var chosenArmorPlateLevelString = _weightedRandomHelper.GetWeightedValue(plateWeights);

        // Convert the array of ids into database items
        var platesFromDb = existingPlateTplPool.Select((plateTpl) => _itemHelper.GetItem(plateTpl).Value);

        // Filter plates to the chosen level based on its armorClass property
        var platesOfDesiredLevel = platesFromDb.Where((item) => item.Properties.ArmorClass.Value == double.Parse(chosenArmorPlateLevelString));
        if (platesOfDesiredLevel.Any())
        {
            // Plates found
            result.Result = Result.SUCCESS;
            result.PlateModTemplates = platesOfDesiredLevel.Select((item) => item.Id).ToHashSet();

            return result;
        }

        // no plates found that fit requirements, lets get creative

        // Get lowest and highest plate classes available for this armor
        var minMaxArmorPlateClass = GetMinMaxArmorPlateClass(platesFromDb.ToList());

        // Increment plate class level in attempt to get useable plate
        var findCompatiblePlateAttempts = 0;
        var maxAttempts = 3;
        for (var i = 0; i < maxAttempts; i++)
        {
            var chosenArmorPlateLevelDouble = int.Parse(chosenArmorPlateLevelString) + 1;
            chosenArmorPlateLevelString = chosenArmorPlateLevelDouble.ToString();

            // New chosen plate class is higher than max, then set to min and check if valid
            if (chosenArmorPlateLevelDouble > minMaxArmorPlateClass.Max)
            {
                chosenArmorPlateLevelString = minMaxArmorPlateClass.Min.ToString();
            }

            findCompatiblePlateAttempts++;

            platesOfDesiredLevel = platesFromDb.Where((item) => item.Properties.ArmorClass == chosenArmorPlateLevelDouble);
            // Valid plates found, exit
            if (platesOfDesiredLevel.Any())
            {
                break;
            }

            // No valid plate class found in 3 tries, attempt default plates
            if (findCompatiblePlateAttempts >= maxAttempts)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Plate filter too restrictive for armor: {armorItem.Name} {armorItem.Id}, unable to find plates of level: {chosenArmorPlateLevelString}, using items default plate"
                    );
                }

                var defaultPlate = GetDefaultPlateTpl(armorItem, modSlot);
                if (defaultPlate is not null)
                {
                    // Return Default Plates cause couldn't get lowest level available from original selection
                    result.Result = Result.SUCCESS;
                    result.PlateModTemplates = [defaultPlate];

                    return result;
                }

                // No plate found after filtering AND no default plate

                // Last attempt, get default preset and see if it has a plate default
                var defaultPresetPlateSlot = GetDefaultPresetArmorSlot(armorItem.Id, modSlot);
                if (defaultPresetPlateSlot is not null)
                {
                    // Found a plate, exit
                    var plateItem = _itemHelper.GetItem(defaultPresetPlateSlot.Template);
                    platesOfDesiredLevel = [plateItem.Value];

                    break;
                }

                // Everything failed, no default plate or no default preset armor plate
                result.Result = Result.NO_DEFAULT_FILTER;

                return result;
            }
        }

        // Only return the items ids
        result.Result = Result.SUCCESS;
        result.PlateModTemplates = platesOfDesiredLevel.Select(item => item.Id).ToHashSet();

        return result;
    }

    private MinMax GetMinMaxArmorPlateClass(List<TemplateItem> platePool)
    {
        platePool.Sort(
            (x, y) =>
            {
                if (x.Properties.ArmorClass < y.Properties.ArmorClass)
                {
                    return -1;
                }

                if (x.Properties.ArmorClass > y.Properties.ArmorClass)
                {
                    return 1;
                }

                return 0;
            }
        );

        return new MinMax
        {
            Min = (platePool[0].Properties.ArmorClass),
            Max = (platePool[platePool.Count - 1].Properties.ArmorClass),
        };
    }

    /**
     * Get the default plate an armor has in its db item
     * @param armorItem Item to look up default plate
     * @param modSlot front/back
     * @returns Tpl of plate
     */
    protected string? GetDefaultPlateTpl(TemplateItem armorItem, string modSlot)
    {
        var relatedItemDbModSlot = armorItem.Properties.Slots?.FirstOrDefault(slot => slot.Name.ToLower() == modSlot);

        return relatedItemDbModSlot?.Props.Filters[0].Plate;
    }

    /**
     * Get the matching armor slot from the default preset matching passed in armor tpl
     * @param presetItemId Id of preset
     * @param modSlot front/back
     * @returns Armor IItem
     */
    protected Item? GetDefaultPresetArmorSlot(string armorItemTpl, string modSlot)
    {
        var defaultPreset = _presetHelper.GetDefaultPreset(armorItemTpl);

        return defaultPreset?.Items?.FirstOrDefault((item) => item.SlotId?.ToLower() == modSlot);
    }


    /// <summary>
    /// Add mods to a weapon using the provided mod pool
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">Data used to generate the weapon</param>
    /// <returns>Weapon + mods array</returns>
    public List<Item> GenerateModsForWeapon(string sessionId, GenerateWeaponRequest request)
    {
        if (ItemLacksSlotsCartridgesAndChambers(request.ParentTemplate))
        {
            _logger.Error(
                _localisationService.GetText(
                    "bot-unable_to_add_mods_to_weapon_missing_ammo_slot",
                    new
                    {
                        weaponName = request.ParentTemplate.Name,
                        weaponId = request.ParentTemplate.Id,
                        botRole = request.BotData.Role,
                    }
                )
            );

            return request.Weapon;
        }

        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        // Get pool of mods that fit weapon
        request.ModPool.TryGetValue(request.ParentTemplate.Id, out var compatibleModsPool);

        _botConfig.Equipment.TryGetValue(request.BotData.EquipmentRole, out var botEquipConfig);
        var botEquipBlacklist = _botEquipmentFilterService.GetBotEquipmentBlacklist(
            request.BotData.EquipmentRole,
            pmcProfile?.Info?.Level ?? 0
        );
        var botWeaponSightWhitelist = _botEquipmentFilterService.GetBotWeaponSightWhitelist(
            request.BotData.EquipmentRole
        );
        var randomisationSettings = _botHelper.GetBotRandomizationDetails(request.BotData.Level ?? 0, botEquipConfig);

        // Iterate over mod pool and choose mods to attach
        var sortedModKeys = SortModKeys(compatibleModsPool.Keys.ToList(), request.ParentTemplate.Id);
        foreach (var modSlot in sortedModKeys)
        {
            // Check weapon has slot for mod to fit in
            var modsParentSlot = GetModItemSlotFromDb(modSlot, request.ParentTemplate);
            if (modsParentSlot is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "bot-weapon_missing_mod_slot",
                        new
                        {
                            modSlot = modSlot,
                            weaponId = request.ParentTemplate.Id,
                            weaponName = request.ParentTemplate.Name,
                            botRole = request.BotData.Role,
                        }
                    )
                );

                continue;
            }

            // Check spawn chance of mod
            var modSpawnResult = ShouldModBeSpawned(
                modsParentSlot,
                modSlot,
                request.ModSpawnChances,
                botEquipConfig
            );
            if (modSpawnResult == ModSpawn.SKIP)
            {
                continue;
            }

            var isRandomisableSlot = randomisationSettings?.RandomisedWeaponModSlots?.Contains(modSlot) ?? false;
            ModToSpawnRequest modToSpawnRequest = new()
            {
                ModSlot = modSlot,
                IsRandomisableSlot = isRandomisableSlot,
                RandomisationSettings = randomisationSettings,
                BotWeaponSightWhitelist = botWeaponSightWhitelist,
                BotEquipBlacklist = botEquipBlacklist,
                ItemModPool = compatibleModsPool,
                Weapon = request.Weapon,
                AmmoTpl = request.AmmoTpl,
                ParentTemplate = request.ParentTemplate,
                ModSpawnResult = modSpawnResult,
                WeaponStats = request.WeaponStats,
                ConflictingItemTpls = request.ConflictingItemTpls,
                BotData = request.BotData
            };
            var modToAdd = ChooseModToPutIntoSlot(modToSpawnRequest);

            // Compatible mod not found
            if (modToAdd is null)
            {
                continue;
            }

            if (!IsModValidForSlot(modToAdd, modsParentSlot, modSlot, request.ParentTemplate, request.BotData.Role))
            {
                continue;
            }

            var modToAddTemplate = modToAdd.Value;
            // Skip adding mod to weapon if type limit reached
            if (
                _botWeaponModLimitService.WeaponModHasReachedLimit(
                    request.BotData.EquipmentRole,
                    modToAddTemplate.Value,
                    request.ModLimits,
                    request.ParentTemplate,
                    request.Weapon
                )
            )
            {
                continue;
            }

            // If item is a mount for scopes, set scope chance to 100%, this helps fix empty mounts appearing on weapons
            if (ModSlotCanHoldScope(modSlot, modToAddTemplate.Value.Parent))
            {
                // mod_mount was picked to be added to weapon, force scope chance to ensure its filled
                List<string> scopeSlots = ["mod_scope", "mod_scope_000", "mod_scope_001", "mod_scope_002", "mod_scope_003"];
                AdjustSlotSpawnChances(request.ModSpawnChances, scopeSlots, 100);

                // Hydrate pool of mods that fit into mount as its a randomisable slot
                if (isRandomisableSlot)
                {
                    // Add scope mods to modPool dictionary to ensure the mount has a scope in the pool to pick
                    AddCompatibleModsForProvidedMod(
                        "mod_scope",
                        modToAddTemplate.Value,
                        request.ModPool,
                        botEquipBlacklist
                    );
                }
            }

            // If picked item is muzzle adapter that can hold a child, adjust spawn chance
            if (ModSlotCanHoldMuzzleDevices(modSlot, modToAddTemplate.Value.Parent))
            {
                List<string> muzzleSlots = ["mod_muzzle", "mod_muzzle_000", "mod_muzzle_001"];
                // Make chance of muzzle devices 95%, nearly certain but not guaranteed
                AdjustSlotSpawnChances(request.ModSpawnChances, muzzleSlots, 95);
            }

            // If front/rear sight are to be added, set opposite to 100% chance
            if (ModIsFrontOrRearSight(modSlot, modToAddTemplate.Value.Id))
            {
                request.ModSpawnChances["mod_sight_front"] = 100;
                request.ModSpawnChances["mod_sight_rear"] = 100;
            }

            // Handguard mod can take a sub handguard mod + weapon has no UBGL (takes same slot)
            // Force spawn chance to be 100% to ensure it gets added
            if (
                modSlot == "mod_handguard" &&
                modToAddTemplate.Value.Properties.Slots.Any((slot) => slot.Name == "mod_handguard") &&
                !request.Weapon.Any((item) => item.SlotId == "mod_launcher")
            )
            {
                // Needed for handguards with lower
                request.ModSpawnChances["mod_handguard"] = 100;
            }

            // If stock mod can take a sub stock mod, force spawn chance to be 100% to ensure sub-stock gets added
            // Or if bot has stock force enabled
            if (ShouldForceSubStockSlots(modSlot, botEquipConfig, modToAddTemplate.Value))
            {
                // Stock mod can take additional stocks, could be a locking device, force 100% chance
                List<string> subStockSlots = ["mod_stock", "mod_stock_000", "mod_stock_001", "mod_stock_akms"];
                AdjustSlotSpawnChances(request.ModSpawnChances, subStockSlots, 100);
            }

            // Gather stats on mods being added to weapon
            if (_itemHelper.IsOfBaseclass(modToAddTemplate.Value.Id, BaseClasses.IRON_SIGHT))
            {
                if (modSlot == "mod_sight_front")
                {
                    request.WeaponStats.HasFrontIronSight = true;
                }
                else if (modSlot == "mod_sight_rear")
                {
                    request.WeaponStats.HasRearIronSight = true;
                }
            }
            else if (!(request.WeaponStats.HasOptic ?? false) && _itemHelper.IsOfBaseclass(modToAddTemplate.Value.Id, BaseClasses.SIGHTS))
            {
                request.WeaponStats.HasOptic = true;
            }

            var modId = _hashUtil.Generate();
            request.Weapon.Add(
                CreateModItem(
                    modId,
                    modToAddTemplate.Value.Id,
                    request.WeaponId,
                    modSlot,
                    modToAddTemplate.Value,
                    request.BotData.Role
                )
            );

            // Update conflicting item list now item has been chosen
            foreach (var conflictingItem in modToAddTemplate.Value.Properties.ConflictingItems)
            {
                request.ConflictingItemTpls.Add(conflictingItem);
            }

            // I first thought we could use the recursive generateModsForItems as previously for cylinder magazines.
            // However, the recursion doesn't go over the slots of the parent mod but over the modPool which is given by the bot config
            // where we decided to keep cartridges instead of camoras. And since a CylinderMagazine only has one cartridge entry and
            // this entry is not to be filled, we need a special handling for the CylinderMagazine
            var modParentItem = _itemHelper.GetItem(modToAddTemplate.Value.Parent).Value;
            if (_botWeaponGeneratorHelper.MagazineIsCylinderRelated(modParentItem.Name))
            {
                // We don't have child mods, we need to create the camoras for the magazines instead
                FillCamora(request.Weapon, request.ModPool, modId, modToAddTemplate.Value);
            }
            else
            {
                var containsModInPool = request.ModPool.Keys.Contains(modToAddTemplate.Value.Id);

                // Sometimes randomised slots are missing sub-mods, if so, get values from mod pool service
                // Check for a randomisable slot + without data in modPool + item being added as additional slots
                if (isRandomisableSlot && !containsModInPool && modToAddTemplate.Value.Properties.Slots.Any())
                {
                    var modFromService = _botEquipmentModPoolService.GetModsForWeaponSlot(modToAddTemplate.Value.Id);
                    if (modFromService?.Keys.Count > 0)
                    {
                        request.ModPool[modToAddTemplate.Value.Id] = modFromService.ToDictionary();
                        containsModInPool = true;
                    }
                }

                // Fallback when mods with REQUIRED children are not in the pool, add them and process
                if (!containsModInPool && !isRandomisableSlot)
                {
                    // Check for required mods the item we've added needs to be classified as 'valid'
                    var modFromService = _botEquipmentModPoolService.GetRequiredModsForWeaponSlot(modToAddTemplate.Value.Id);
                    if (modFromService?.Keys.Count > 0)
                    {
                        request.ModPool[modToAddTemplate.Value.Id] = modFromService;
                        containsModInPool = true;
                    }
                }

                if (containsModInPool)
                {
                    GenerateWeaponRequest recursiveRequestData = new()
                    {
                        Weapon = request.Weapon,
                        ModPool = request.ModPool,
                        WeaponId = modId,
                        ParentTemplate = modToAddTemplate.Value,
                        ModSpawnChances = request.ModSpawnChances,
                        AmmoTpl = request.AmmoTpl,
                        BotData = new()
                        {
                            Role = request.BotData.Role,
                            Level = request.BotData.Level,
                            EquipmentRole = request.BotData.EquipmentRole
                        },
                        ModLimits = request.ModLimits,
                        WeaponStats = request.WeaponStats,
                        ConflictingItemTpls = request.ConflictingItemTpls
                    };
                    // Call self recursively to add mods to this mod
                    GenerateModsForWeapon(sessionId, recursiveRequestData);
                }
            }
        }

        return request.Weapon;
    }

    protected bool ItemLacksSlotsCartridgesAndChambers(TemplateItem item)
    {
        return item.Properties.Slots?.Count == 0 && item.Properties.Cartridges?.Count == 0 && item.Properties.Chambers?.Count == 0;
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

        return ((string[]) ["mod_sight_front", "mod_sight_rear"]).Contains(modSlot);
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
            ((string[])
            [
                "mod_scope",
                "mod_mount",
                "mod_mount_000",
                "mod_scope_000",
                "mod_scope_001",
                "mod_scope_002",
                "mod_scope_003",
            ]).Contains(modSlot.ToLower()) &&
            modsParentId == BaseClasses.MOUNT
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

        foreach (var modName in modSlotsToAdjust)
        {
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
        return ((string[]) ["mod_muzzle", "mod_muzzle_000", "mod_muzzle_001"]).Contains(modSlot.ToLower());
    }

    /// <summary>
    /// Sort mod slots into an ordering that maximises chance of a successful weapon generation
    /// </summary>
    /// <param name="unsortedSlotKeys">Array of mod slot strings to sort</param>
    /// <param name="itemTplWithKeysToSort">The Tpl of the item with mod keys being sorted</param>
    /// <returns>Sorted array</returns>
    public List<string> SortModKeys(List<string> unsortedSlotKeys, string itemTplWithKeysToSort)
    {
        // No need to sort with only 1 item in array
        if (unsortedSlotKeys.Count <= 1)
        {
            return unsortedSlotKeys;
        }

        var isMount = _itemHelper.IsOfBaseclass(itemTplWithKeysToSort, BaseClasses.MOUNT);

        List<string> sortedKeys = [];
        var modRecieverKey = "mod_reciever";
        var modMount001Key = "mod_mount_001";
        var modGasBlockKey = "mod_gas_block";
        var modPistolGrip = "mod_pistol_grip";
        var modStockKey = "mod_stock";
        var modBarrelKey = "mod_barrel";
        var modHandguardKey = "mod_handguard";
        var modMountKey = "mod_mount";
        var modScopeKey = "mod_scope";
        var modScope000Key = "mod_scope_000";

        // Mounts are a special case, they need scopes first before more mounts
        if (isMount)
        {
            if (unsortedSlotKeys.Contains(modScope000Key))
            {
                sortedKeys.Add(modScope000Key);
                unsortedSlotKeys.Remove(modScope000Key);
            }

            if (unsortedSlotKeys.Contains(modScopeKey))
            {
                sortedKeys.Add(modScopeKey);
                unsortedSlotKeys.Remove(modScopeKey);
            }

            if (unsortedSlotKeys.Contains(modMountKey))
            {
                sortedKeys.Add(modMountKey);
                unsortedSlotKeys.Remove(modMountKey);
            }
        }
        else
        {
            if (unsortedSlotKeys.Contains(modHandguardKey))
            {
                sortedKeys.Add(modHandguardKey);
                unsortedSlotKeys.Remove(modHandguardKey);
            }

            if (unsortedSlotKeys.Contains(modBarrelKey))
            {
                sortedKeys.Add(modBarrelKey);
                unsortedSlotKeys.Remove(modBarrelKey);
            }

            if (unsortedSlotKeys.Contains(modMount001Key))
            {
                sortedKeys.Add(modMount001Key);
                unsortedSlotKeys.Remove(modMount001Key);
            }

            if (unsortedSlotKeys.Contains(modRecieverKey))
            {
                sortedKeys.Add(modRecieverKey);
                unsortedSlotKeys.Remove(modRecieverKey);
            }

            if (unsortedSlotKeys.Contains(modPistolGrip))
            {
                sortedKeys.Add(modPistolGrip);
                unsortedSlotKeys.Remove(modPistolGrip);
            }

            if (unsortedSlotKeys.Contains(modGasBlockKey))
            {
                sortedKeys.Add(modGasBlockKey);
                unsortedSlotKeys.Remove(modGasBlockKey);
            }

            if (unsortedSlotKeys.Contains(modStockKey))
            {
                sortedKeys.Add(modStockKey);
                unsortedSlotKeys.Remove(modStockKey);
            }

            if (unsortedSlotKeys.Contains(modMountKey))
            {
                sortedKeys.Add(modMountKey);
                unsortedSlotKeys.Remove(modMountKey);
            }

            if (unsortedSlotKeys.Contains(modScopeKey))
            {
                sortedKeys.Add(modScopeKey);
                unsortedSlotKeys.Remove(modScopeKey);
            }
        }

        sortedKeys.AddRange(unsortedSlotKeys);

        return sortedKeys;
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
                return parentTemplate?.Properties?.Chambers?.FirstOrDefault((chamber) => chamber.Name.Contains(modSlotLower));
            case "cartridges":
                return parentTemplate?.Properties?.Cartridges?.FirstOrDefault((c) => c.Name?.ToLower() == modSlotLower);
            default:
                return parentTemplate?.Properties?.Slots?.FirstOrDefault((s) => s.Name?.ToLower() == modSlotLower);
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

        var spawnMod = _probabilityHelper.RollChance(modSpawnChances.GetValueOrDefault(modSlotName.ToLower()));
        if (!spawnMod && (slotRequired.GetValueOrDefault(false) || (botEquipConfig.WeaponSlotIdsToMakeRequired?.Contains(modSlotName) ?? false)))
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
    public KeyValuePair<bool, TemplateItem>? ChooseModToPutIntoSlot(ModToSpawnRequest request)
    {
        /** Slot mod will fill */
        var parentSlot = request.ParentTemplate.Properties.Slots?.FirstOrDefault((i) => i.Name == request.ModSlot);
        var weaponTemplate = _itemHelper.GetItem(request.Weapon[0].Template).Value;

        // It's ammo, use predefined ammo parameter
        if (GetAmmoContainers().Contains(request.ModSlot) && request.ModSlot != "mod_magazine")
        {
            return _itemHelper.GetItem(request.AmmoTpl);
        }

        // Ensure there's a pool of mods to pick from
        var modPool = GetModPoolForSlot(request, weaponTemplate);
        if (modPool is null && !(parentSlot?.Required ?? false))
        {
            // Nothing in mod pool + item not required
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Mod pool for optional slot: {request.ModSlot} on item: {request.ParentTemplate.Name} was empty, skipping mod");
            }

            return null;
        }

        // Filter out non-whitelisted scopes, use full modpool if filtered pool would have no elements
        if (request.ModSlot.Contains("mod_scope") && request.BotWeaponSightWhitelist is not null)
        {
            // scope pool has more than one scope
            if (modPool.Count > 1)
            {
                modPool = FilterSightsByWeaponType(request.Weapon[0], modPool, request.BotWeaponSightWhitelist);
            }
        }

        if (request.ModSlot == "mod_gas_block")
        {
            if (request.WeaponStats.HasOptic ?? false && modPool.Count > 1)
            {
                // Attempt to limit modpool to low profile gas blocks when weapon has an optic
                var onlyLowProfileGasBlocks = modPool.Where(
                    (tpl) =>
                        _botConfig.LowProfileGasBlockTpls.Contains(tpl)
                );
                if (onlyLowProfileGasBlocks.Count() > 0)
                {
                    modPool = onlyLowProfileGasBlocks.ToHashSet();
                }
            }
            else if (request.WeaponStats.HasRearIronSight ?? false && modPool.Count() > 1)
            {
                // Attempt to limit modpool to high profile gas blocks when weapon has rear iron sight + no front iron sight
                var onlyHighProfileGasBlocks = modPool.Where(
                    (tpl) => !_botConfig.LowProfileGasBlockTpls.Contains(tpl)
                );
                if (onlyHighProfileGasBlocks.Count() > 0)
                {
                    modPool = onlyHighProfileGasBlocks.ToHashSet();
                }
            }
        }

        // Check if weapon has min magazine size limit
        if (
            request?.ModSlot == "mod_magazine" &&
            (request?.IsRandomisableSlot ?? false) &&
            request.RandomisationSettings.MinimumMagazineSize is not null
        )
        {
            modPool = GetFilterdMagazinePoolByCapacity(request, modPool).ToHashSet();
        }

        // Pick random mod that's compatible
        var chosenModResult = GetCompatibleWeaponModTplForSlotFromPool(
            request,
            modPool,
            parentSlot,
            request.ModSpawnResult,
            request.Weapon,
            request.ModSlot
        );
        if (chosenModResult.SlotBlocked.GetValueOrDefault(false) && !parentSlot.Required.GetValueOrDefault(false))
        {
            // Don't bother trying to fit mod, slot is completely blocked
            return null;
        }

        // Log if mod chosen was incompatible
        if (chosenModResult.Incompatible.GetValueOrDefault(false) && !(parentSlot.Required.GetValueOrDefault(false)))
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Unable to find compatible mod of type: {parentSlot.Name}, in slot: {request.ModSlot} reason: {chosenModResult.Reason}");
            }
        }

        // Get random mod to attach from items db for required slots if none found above
        if (!(chosenModResult.Found ?? false) && parentSlot != null && (parentSlot.Required ?? false))
        {
            chosenModResult.ChosenTemplate = GetRandomModTplFromItemDb("", parentSlot, request.ModSlot, request.Weapon);
            chosenModResult.Found = true;
        }

        // Compatible item not found + not required
        if (!(chosenModResult.Found.GetValueOrDefault(false)) && parentSlot is not null && (!parentSlot.Required.GetValueOrDefault(false)))
        {
            return null;
        }

        if (!(chosenModResult.Found ?? false) && parentSlot is not null)
        {
            if (parentSlot.Required.GetValueOrDefault(false))
            {
                _logger.Warning(
                    $"Required slot unable to be filled, {request.ModSlot} on {request.ParentTemplate.Name} {request.ParentTemplate.Id} for weapon: {request.Weapon[0].Template}"
                );
            }

            return null;
        }

        return _itemHelper.GetItem(chosenModResult.ChosenTemplate);
    }

    /// <summary>
    /// Given the passed in array of magaizne tpls, look up the min size set in config and return only those that have that size or larger
    /// </summary>
    /// <param name="modSpawnRequest">Request data</param>
    /// <param name="modPool">Pool of magazine tpls to filter</param>
    /// <returns>Filtered pool of magazine tpls</returns>
    public IEnumerable<string> GetFilterdMagazinePoolByCapacity(ModToSpawnRequest modSpawnRequest, HashSet<string> modPool)
    {
        var weaponTpl = modSpawnRequest.Weapon[0].Template;
        modSpawnRequest.RandomisationSettings.MinimumMagazineSize.TryGetValue(weaponTpl, out var minMagSizeFromSettings);
        var minMagazineSize = minMagSizeFromSettings;
        var desiredMagazineTpls = modPool.Where(
            (magTpl) =>
            {
                var magazineDb = _itemHelper.GetItem(magTpl).Value;
                return magazineDb.Properties is not null && magazineDb.Properties.Cartridges.FirstOrDefault().MaxCount >= minMagazineSize;
            }
        );

        if (!desiredMagazineTpls.Any())
        {
            _logger.Warning("Magazine size filter for ${ weaponTpl} was too strict, ignoring filter");

            return modPool;
        }

        return desiredMagazineTpls;
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
    public ChooseRandomCompatibleModResult GetCompatibleWeaponModTplForSlotFromPool(ModToSpawnRequest request, HashSet<string> modPool, Slot parentSlot,
        ModSpawn? choiceTypeEnum, List<Item> weapon, string modSlotName)
    {
        // Filter out incompatible mods from pool
        var preFilteredModPool = GetFilteredModPool(modPool, request.ConflictingItemTpls);
        if (preFilteredModPool.Count == 0)
        {
            return new()
            {
                Incompatible = true,
                Found = false,
                Reason = $"Unable to add mod to {choiceTypeEnum.ToString()} slot: {modSlotName}. All: {modPool.Count()} had conflicts"
            };
        }

        // Filter mod pool to only items that appear in parents allowed list
        preFilteredModPool = preFilteredModPool.Where((tpl) => parentSlot.Props.Filters[0].Filter.Contains(tpl)).ToList();
        if (preFilteredModPool.Count == 0)
        {
            return new() { Incompatible = true, Found = false, Reason = "No mods found in parents allowed list" };
        }

        return GetCompatibleModFromPool(preFilteredModPool, choiceTypeEnum, weapon);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modPool">Pool of item Tpls to choose from</param>
    /// <param name="modSpawnType">How should the slot choice be handled - forced/normal etc</param>
    /// <param name="weapon">Weapon mods at current time</param>
    /// <returns>IChooseRandomCompatibleModResult</returns>
    public ChooseRandomCompatibleModResult GetCompatibleModFromPool(List<string> modPool, ModSpawn? modSpawnType, List<Item> weapon)
    {
        // Create exhaustable pool to pick mod item from
        var exhaustableModPool = CreateExhaustableArray(modPool.ToList());

        // Create default response if no compatible item is found below
        ChooseRandomCompatibleModResult chosenModResult = new()
        {
            Incompatible = true,
            Found = false,
            Reason = "unknown",
        };

        // Limit how many attempts to find a compatible mod can occur before giving up
        var maxBlockedAttempts = Math.Round(modPool.Count * 0.75); // 75% of pool size
        var blockedAttemptCount = 0;
        string chosenTpl;
        while (exhaustableModPool.HasValues())
        {
            chosenTpl = exhaustableModPool.GetRandomValue();
            var pickedItemDetails = _itemHelper.GetItem(chosenTpl);
            if (!pickedItemDetails.Key)
            {
                // Not valid item, try again
                continue;
            }

            if (pickedItemDetails.Value.Properties is null)
            {
                // no props data, try again
                continue;
            }

            // Success - Default wanted + only 1 item in pool
            if (modSpawnType == ModSpawn.DEFAULT_MOD && modPool.Count == 1)
            {
                chosenModResult.Found = true;
                chosenModResult.Incompatible = false;
                chosenModResult.ChosenTemplate = chosenTpl;

                break;
            }

            // Check if existing weapon mods are incompatible with chosen item
            var existingItemBlockingChoice = weapon.FirstOrDefault(
                (item) =>
                    pickedItemDetails.Value.Properties.ConflictingItems?.Contains(item.Template) ?? false
            );
            if (existingItemBlockingChoice is not null)
            {
                // Give max of x attempts of picking a mod if blocked by another
                // OR Blocked and modpool only had 1 item
                if (blockedAttemptCount > maxBlockedAttempts || modPool.Count == 1)
                {
                    blockedAttemptCount = 0; // reset
                    //chosenModResult.SlotBlocked = true; // Later in code we try to find replacement, but only when "slotBlocked" is not true
                    chosenModResult.Reason = "Blocked";

                    break;
                }

                blockedAttemptCount++;

                // Not compatible - Try again
                ;
                continue;
            }

            // Edge case- Some mod combos will never work, make sure this isnt the case
            if (WeaponModComboIsIncompatible(weapon, chosenTpl))
            {
                chosenModResult.Reason = $"Chosen weapon mod: {chosenTpl} can never be compatible with existing weapon mods";
                break;
            }

            // Success
            chosenModResult.Found = true;
            chosenModResult.Incompatible = false;
            chosenModResult.ChosenTemplate = chosenTpl;

            break;
        }

        return chosenModResult;
    }

    public ExhaustableArray<T> CreateExhaustableArray<T>(List<T> itemsToAddToArray)
    {
        return new ExhaustableArray<T>(itemsToAddToArray.ToList(), _randomUtil, _cloner);
    }

    /// <summary>
    /// Get a list of mod tpls that are compatible with the current weapon
    /// </summary>
    /// <param name="modPool"></param>
    /// <param name="tplBlacklist">Tpls that are incompatible and should not be used</param>
    /// <returns>string array of compatible mod tpls with weapon</returns>
    public List<string> GetFilteredModPool(HashSet<string> modPool, HashSet<string> tplBlacklist)
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
    public HashSet<string> GetModPoolForSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
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

    public HashSet<string> GetModPoolForDefaultSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        var matchingModFromPreset = GetMatchingModFromPreset(request, weaponTemplate);
        if (matchingModFromPreset is null)
        {
            if (request.ItemModPool[request.ModSlot]?.Count > 1)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"{request.BotData.Role} No default: {request.ModSlot} mod found for: {weaponTemplate.Name}, using existing pool");
                }
            }

            // Couldn't find default in globals, use existing mod pool data
            return request.ItemModPool[request.ModSlot];
        }

        // Only filter mods down to single default item if it already exists in existing itemModPool, OR the default item has no children
        // Filtering mod pool to item that wasn't already there can have problems;
        // You'd have a mod being picked without any sub-mods in its chain, possibly resulting in missing required mods not being added
        // Mod is in existing mod pool
        if (request.ItemModPool[request.ModSlot].Contains(matchingModFromPreset.Template))
        {
            // Found mod on preset + it already exists in mod pool
            return [matchingModFromPreset.Template];
        }

        // Get an array of items that are allowed in slot from parent item
        // Check the filter of the slot to ensure a chosen mod fits
        var parentSlotCompatibleItems = request.ParentTemplate.Properties.Slots?.FirstOrDefault(
                (slot) => string.Equals(slot.Name.ToLower(), request.ModSlot.ToLower(), StringComparison.Ordinal)
            )
            ?.Props.Filters?[0].Filter;

        // Mod isn't in existing pool, only add if it has no children and exists inside parent filter
        if (
            (parentSlotCompatibleItems?.Contains(matchingModFromPreset.Template) ?? false) &&
            _itemHelper.GetItem(matchingModFromPreset.Template).Value.Properties.Slots?.Count == 0
        )
        {
            // Chosen mod has no conflicts + no children + is in parent compat list
            if (!request.ConflictingItemTpls.Contains(matchingModFromPreset.Template))
            {
                return [matchingModFromPreset.Template];
            }

            // Above chosen mod had conflicts with existing weapon mods
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"{request.BotData.Role} Chosen default: {request.ModSlot} mod found for: {weaponTemplate.Name} weapon conflicts with item on weapon, cannot use default"
                );
            }

            var existingModPool = request.ItemModPool[request.ModSlot];
            if (existingModPool.Count == 1)
            {
                // The only item in pool isn't compatible
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"{request.BotData.Role} {request.ModSlot} Mod pool for: {weaponTemplate.Name} weapon has only incompatible items, using parent list instead"
                    );
                }

                // Last ditch, use full pool of items minus conflicts
                var newListOfModsForSlot = parentSlotCompatibleItems.Where((tpl) => !request.ConflictingItemTpls.Contains(tpl));
                if (newListOfModsForSlot.Count() > 0)
                {
                    return newListOfModsForSlot.ToHashSet();
                }
            }

            // Return full mod pool
            return request.ItemModPool[request.ModSlot];
        }

        // Tried everything, return mod pool
        return request.ItemModPool[request.ModSlot];
    }

    public Item? GetMatchingModFromPreset(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        var matchingPreset = GetMatchingPreset(weaponTemplate, request.ParentTemplate.Id);
        return matchingPreset?.Items?.FirstOrDefault((item) => item?.SlotId?.ToLower() == request.ModSlot.ToLower());
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
        return new Item
        {
            Id = modId,
            Template = modTpl,
            ParentId = parentId,
            SlotId = modSlot,
            Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(modTemplate, botRole)
        };
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
    public bool IsModValidForSlot(KeyValuePair<bool, TemplateItem>? modToAdd, Slot slotAddedToTemplate, string modSlot, TemplateItem parentTemplate,
        string botRole)
    {
        var modBeingAddedDbTemplate = modToAdd.Value;

        // Mod lacks db template object
        if (modBeingAddedDbTemplate.Value is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "bot-no_item_template_found_when_adding_mod",
                    new
                    {
                        modId = modBeingAddedDbTemplate.Value?.Id ?? "UNKNOWN",
                        modSlot = modSlot,
                    }
                )
            );
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Item -> {parentTemplate?.Id}; Slot -> {modSlot}");
            }

            return false;
        }

        // Mod has invalid db item
        if (!modToAdd.HasValue)
        {
            // Parent slot must be filled but db object is invalid, show warning and return false
            if (slotAddedToTemplate.Required ?? false)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "bot-unable_to_add_mod_item_invalid",
                        new
                        {
                            itemName = modBeingAddedDbTemplate.Value?.Name ?? "UNKNOWN",
                            iodSlot = modSlot,
                            parentItemName = parentTemplate.Name,
                            botRole = botRole,
                        }
                    )
                );
            }

            return false;
        }

        // Mod was found in db
        return true;
    }

    /// <summary>
    /// Find mod tpls of a provided type and add to its modPool
    /// </summary>
    /// <param name="desiredSlotName">Slot to look up and add we are adding tpls for (e.g mod_scope)</param>
    /// <param name="modTemplate">db object for modItem we get compatible mods from</param>
    /// <param name="modPool">Pool of mods we are adding to</param>
    /// <param name="botEquipBlacklist">A blacklist of items that cannot be picked</param>
    public void AddCompatibleModsForProvidedMod(string desiredSlotName, TemplateItem modTemplate,
        Dictionary<string, Dictionary<string, HashSet<string>>> modPool,
        EquipmentFilterDetails botEquipBlacklist)
    {
        var desiredSlotObject = modTemplate.Properties.Slots?.FirstOrDefault((slot) => slot.Name.Contains(desiredSlotName));
        if (desiredSlotObject is null)
        {
            return;
        }

        var supportedSubMods = desiredSlotObject.Props.Filters[0].Filter;
        if (supportedSubMods is null)
        {
            return;
        }

        // Filter mods
        var filteredMods = FilterModsByBlacklist(supportedSubMods.ToHashSet(), botEquipBlacklist, desiredSlotName);
        if (!filteredMods.Any())
        {
            _logger.Warning(
                _localisationService
                    .GetText(
                        "bot-unable_to_filter_mods_all_blacklisted",
                        new
                        {
                            slotName = desiredSlotObject.Name,
                            itemName = modTemplate.Name,
                        }
                    )
            );
        }

        if (!modPool.ContainsKey(modTemplate.Id))
        {
            modPool[modTemplate.Id] = new();
        }

        modPool[modTemplate.Id][desiredSlotObject.Name] = supportedSubMods.ToHashSet();
    }

    /// <summary>
    /// Get the possible items that fit a slot
    /// </summary>
    /// <param name="parentItemId">item tpl to get compatible items for</param>
    /// <param name="modSlot">Slot item should fit in</param>
    /// <param name="botEquipBlacklist">Equipment that should not be picked</param>
    /// <returns>Array of compatible items for that slot</returns>
    public HashSet<string> GetDynamicModPool(string parentItemId, string modSlot, EquipmentFilterDetails botEquipBlacklist)
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
    public HashSet<string> FilterModsByBlacklist(HashSet<string> allowedMods, EquipmentFilterDetails? botEquipBlacklist, string modSlot)
    {
        // No blacklist, nothing to filter out
        if (botEquipBlacklist is null)
        {
            return allowedMods;
        }

        var result = new HashSet<string>();

        // Get item blacklist and mod equipment blacklist as one array
        botEquipBlacklist.Equipment.TryGetValue(modSlot, out var equipmentBlacklistValues);
        var blacklist = _itemFilterService.GetBlacklistedItems().Concat(equipmentBlacklistValues ?? []);
        result = allowedMods.Where((tpl) => !blacklist.Contains(tpl)).ToHashSet();

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
    public void FillCamora(List<Item> items, Dictionary<string, Dictionary<string, HashSet<string>>> modPool, string cylinderMagParentId,
        TemplateItem cylinderMagTemplate)
    {
        var itemModPool = modPool[cylinderMagTemplate.Id];
        if (itemModPool is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-unable_to_fill_camora_slot_mod_pool_empty",
                    new
                    {
                        weaponId = cylinderMagTemplate.Id,
                        weaponName = cylinderMagTemplate.Name,
                    }
                )
            );
            var camoraSlots = cylinderMagTemplate.Properties.Slots.Where((slot) => slot.Name.StartsWith("camora"));

            // Attempt to generate camora slots for item
            modPool[cylinderMagTemplate.Id] = new();
            foreach (var camora in camoraSlots)
            {
                modPool[cylinderMagTemplate.Id][camora.Name] = camora.Props.Filters?[0].Filter.ToHashSet();
            }

            itemModPool = modPool[cylinderMagTemplate.Id];
        }

        ExhaustableArray<string> exhaustableModPool = null;
        var modSlot = "cartridges";
        var camoraFirstSlot = "camora_000";
        if (itemModPool.TryGetValue(modSlot, out var value))
        {
            exhaustableModPool = CreateExhaustableArray(value.ToList());
        }
        else if (itemModPool.ContainsKey(camoraFirstSlot))
        {
            modSlot = camoraFirstSlot;
            exhaustableModPool = CreateExhaustableArray(MergeCamoraPools(itemModPool).ToList());
        }
        else
        {
            _logger.Error(_localisationService.GetText("bot-missing_cartridge_slot", cylinderMagTemplate.Id));

            return;
        }

        string modTpl = null;
        var found = false;
        while (exhaustableModPool.HasValues())
        {
            modTpl = exhaustableModPool.GetRandomValue();
            if (!_botGeneratorHelper.IsItemIncompatibleWithCurrentItems(items, modTpl, modSlot).Incompatible.GetValueOrDefault(false))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            _logger.Error(_localisationService.GetText("bot-no_compatible_camora_ammo_found", modSlot));

            return;
        }

        foreach (var slot in cylinderMagTemplate.Properties.Slots)
        {
            var modSlotId = slot.Name;
            var modId = _hashUtil.Generate();
            items.Add(new() { Id = modId, Template = modTpl, ParentId = cylinderMagParentId, SlotId = modSlotId });
        }
    }

    /// <summary>
    /// Take a record of camoras and merge the compatible shells into one array
    /// </summary>
    /// <param name="camorasWithShells">Dictionary of camoras we want to merge into one array</param>
    /// <returns>String array of shells for multiple camora sources</returns>
    public HashSet<string> MergeCamoraPools(Dictionary<string, HashSet<string>> camorasWithShells)
    {
        return camorasWithShells
            .SelectMany(shellKvP => shellKvP.Value)
            .Distinct()
            .ToHashSet();
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
    public HashSet<string> FilterSightsByWeaponType(Item weapon, HashSet<string> scopes, Dictionary<string, List<string>> botWeaponSightWhitelist)
    {
        var weaponDetails = _itemHelper.GetItem(weapon.Template);

        // Return original scopes array if whitelist not found
        var whitelistedSightTypes = botWeaponSightWhitelist[weaponDetails.Value.Parent];
        if (whitelistedSightTypes is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Unable to find whitelist for weapon type: {weaponDetails.Value.Parent} {weaponDetails.Value.Name}, skipping sight filtering"
                );
            }

            return scopes;
        }

        // Filter items that are not directly scopes OR mounts that do not hold the type of scope we allow for this weapon type
        HashSet<string> filteredScopesAndMods = [];
        foreach (var item in scopes)
        {
            // Mods is a scope, check base class is allowed
            if (_itemHelper.IsOfBaseclasses(item, whitelistedSightTypes))
            {
                // Add mod to allowed list
                filteredScopesAndMods.Add(item);
                continue;
            }

            // Edge case, what if item is a mount for a scope and not directly a scope?
            // Check item is mount + has child items
            var itemDetails = _itemHelper.GetItem(item).Value;
            if (_itemHelper.IsOfBaseclass(item, BaseClasses.MOUNT) && itemDetails.Properties.Slots.Count() > 0)
            {
                // Check to see if mount has a scope slot (only include primary slot, ignore the rest like the backup sight slots)
                // Should only find 1 as there's currently no items with a mod_scope AND a mod_scope_000
                List<string> filter = ["mod_scope", "mod_scope_000"];
                var scopeSlot = itemDetails.Properties.Slots.Where(
                    (slot) =>
                        filter.Contains(slot.Name)
                );

                // Mods scope slot found must allow ALL whitelisted scope types OR be a mount
                if (scopeSlot?.All(
                        (slot) =>
                            slot.Props.Filters[0]
                                .Filter.All(
                                    (tpl) =>
                                        _itemHelper.IsOfBaseclasses(tpl, whitelistedSightTypes) ||
                                        _itemHelper.IsOfBaseclass(tpl, BaseClasses.MOUNT)
                                )
                    ) ??
                    false)
                {
                    // Add mod to allowed list
                    filteredScopesAndMods.Add(item);
                }
            }
        }

        // No mods added to return list after filtering has occurred, send back the original mod list
        if (filteredScopesAndMods is null || filteredScopesAndMods.Count() == 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Scope whitelist too restrictive for: {weapon.Template} {weaponDetails.Value.Name}, skipping filter");
            }

            return scopes;
        }

        return filteredScopesAndMods;
    }
}
