using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators.WeaponGen;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable(InjectionType.Singleton)]
public class BotWeaponGenerator(
    ISptLogger<BotWeaponGenerator> logger,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    WeightedRandomHelper weightedRandomHelper,
    BotGeneratorHelper botGeneratorHelper,
    RandomUtil randomUtil,
    BotWeaponGeneratorHelper botWeaponGeneratorHelper,
    BotWeaponModLimitService botWeaponModLimitService,
    BotEquipmentModGenerator botEquipmentModGenerator,
    ServerLocalisationService serverLocalisationService,
    RepairService repairService,
    ICloner cloner,
    ConfigServer configServer,
    IEnumerable<IInventoryMagGen> inventoryMagGenComponents
)
{
    private const string ModMagazineSlotId = "mod_magazine";
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly IEnumerable<IInventoryMagGen> InventoryMagGenComponents = MagGenSetUp(inventoryMagGenComponents);
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();
    protected readonly RepairConfig RepairConfig = configServer.GetConfig<RepairConfig>();

    protected static List<IInventoryMagGen> MagGenSetUp(IEnumerable<IInventoryMagGen> components)
    {
        var inventoryMagGens = components.ToList();
        inventoryMagGens.Sort((a, b) => a.GetPriority() - b.GetPriority());
        return inventoryMagGens;
    }

    /// <summary>
    ///     Pick a random weapon based on weightings and generate a functional weapon
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <param name="botGenerationDetails">Details related to generating a bot</param>
    /// <param name="weaponParentId">Details related to generating a bot</param>
    /// <param name="modChances"></param>
    /// <returns>GenerateWeaponResult object</returns>
    public GenerateWeaponResult? GenerateRandomWeapon(
        MongoId sessionId,
        string equipmentSlot,
        BotTypeInventory botTemplateInventory,
        BotGenerationDetails botGenerationDetails,
        MongoId weaponParentId,
        Dictionary<string, double> modChances
    )
    {
        var weaponTpl = PickWeightedWeaponTemplateFromPool(equipmentSlot, botTemplateInventory);
        return GenerateWeaponByTpl(
            sessionId,
            weaponTpl,
            equipmentSlot,
            botTemplateInventory,
            weaponParentId,
            modChances,
            botGenerationDetails
        );
    }

    /// <summary>
    ///     Gets a random weighted weapon from a bot's pool of weapons.
    /// </summary>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <returns>Weapon template</returns>
    public MongoId PickWeightedWeaponTemplateFromPool(string equipmentSlot, BotTypeInventory botTemplateInventory)
    {
        if (!Enum.TryParse(equipmentSlot, out EquipmentSlots key))
        {
            logger.Error($"Unable to parse equipment slot: {equipmentSlot}");
        }

        var weaponPool = botTemplateInventory.Equipment[key];
        return weightedRandomHelper.GetWeightedValue(weaponPool);
    }

    /// <summary>
    ///     Generates a weapon based on the supplied weapon template.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="weaponTpl">Weapon template to generate (use pickWeightedWeaponTplFromPool()).</param>
    /// <param name="slotName">Slot to fit into, primary/secondary/holster.</param>
    /// <param name="botTemplateInventory">e.g. assault.json.</param>
    /// <param name="weaponParentId">Parent ID of the weapon being generated.</param>
    /// <param name="modChances">Dictionary of item types and % chance weapon will have that mod.</param>
    /// <param name="botGenerationDetails"></param>
    /// <returns>GenerateWeaponResult object.</returns>
    public GenerateWeaponResult? GenerateWeaponByTpl(
        MongoId sessionId,
        MongoId weaponTpl,
        string slotName,
        BotTypeInventory botTemplateInventory,
        MongoId weaponParentId,
        Dictionary<string, double> modChances,
        BotGenerationDetails botGenerationDetails
    )
    {
        var modPool = botTemplateInventory.Mods;
        var weaponItemTemplate = itemHelper.GetItem(weaponTpl).Value;

        if (weaponItemTemplate is null)
        {
            logger.Error(serverLocalisationService.GetText("bot-missing_item_template", weaponTpl));
            logger.Error($"WeaponSlot -> {slotName}");

            return null;
        }

        // Find ammo to use when filling magazines/chamber
        if (botTemplateInventory.Ammo is null)
        {
            logger.Error(serverLocalisationService.GetText("bot-no_ammo_found_in_bot_json", botGenerationDetails.RoleLowercase));
            logger.Error(serverLocalisationService.GetText("bot-generation_failed"));
        }

        var ammoTpl = GetWeightedCompatibleAmmo(botTemplateInventory.Ammo, weaponItemTemplate);

        // Create with just base weapon item
        var weaponWithModsArray = ConstructWeaponBaseList(
                weaponTpl,
                weaponParentId,
                slotName,
                weaponItemTemplate,
                botGenerationDetails.RoleLowercase
            )
            .ToList();

        // Chance to add randomised weapon enhancement
        if (botGenerationDetails.IsPmc && randomUtil.GetChance100(PMCConfig.WeaponHasEnhancementChancePercent))
        // Add buff to weapon root
        {
            repairService.AddBuff(RepairConfig.RepairKit.Weapon, weaponWithModsArray[0]);
        }

        // Add mods to weapon base
        if (modPool.ContainsKey(weaponTpl))
        {
            // Role to treat bot as e.g. pmc/scav/boss
            var botEquipmentRole = botGeneratorHelper.GetBotEquipmentRole(botGenerationDetails.RoleLowercase);

            // Different limits if bot is boss vs scav
            var modLimits = botWeaponModLimitService.GetWeaponModLimits(botEquipmentRole);

            GenerateWeaponRequest generateWeaponModsRequest = new()
            {
                Weapon = weaponWithModsArray, // Will become hydrated array of weapon + mods
                ModPool = modPool,
                WeaponId = weaponWithModsArray[0].Id, // Weapon root id
                ParentTemplate = weaponItemTemplate,
                ModSpawnChances = modChances,
                AmmoTpl = ammoTpl,
                BotData = new BotData
                {
                    Role = botGenerationDetails.RoleLowercase,
                    Level = botGenerationDetails.BotLevel,
                    EquipmentRole = botEquipmentRole,
                },
                ModLimits = modLimits,
                WeaponStats = new WeaponStats(),
                ConflictingItemTpls = [],
            };
            weaponWithModsArray = botEquipmentModGenerator.GenerateModsForWeapon(sessionId, generateWeaponModsRequest);
        }

        // Use weapon preset from globals.json if weapon isn't valid
        if (!IsWeaponValid(weaponWithModsArray, botGenerationDetails.RoleLowercase))
        // Weapon is bad, fall back to weapons preset
        {
            weaponWithModsArray = GetPresetWeaponMods(
                weaponTpl,
                slotName,
                weaponParentId,
                weaponItemTemplate,
                botGenerationDetails.RoleLowercase
            );
        }

        var tempList = cloner.Clone(weaponWithModsArray.Where(item => item.SlotId == ModMagazineSlotId));
        // Fill existing magazines to full and sync ammo type
        foreach (var magazine in tempList)
        {
            FillExistingMagazines(weaponWithModsArray, magazine, ammoTpl);
        }

        // Add cartridge(s) to gun chamber(s)
        if (
            (weaponItemTemplate.Properties?.Chambers).Any()
            && weaponItemTemplate.Properties.Chambers.FirstOrDefault().Properties.Filters.FirstOrDefault().Filter.Contains(ammoTpl)
        )
        {
            // Guns have variety of possible Chamber ids, patron_in_weapon/patron_in_weapon_000/patron_in_weapon_001
            var chamberSlotNames = weaponItemTemplate.Properties.Chambers.Select(chamberSlot => chamberSlot.Name);
            AddCartridgeToChamber(weaponWithModsArray, ammoTpl, chamberSlotNames.ToList());
        }

        // Fill UBGL if found
        var ubglMod = weaponWithModsArray.FirstOrDefault(x => x.SlotId == "mod_launcher");
        MongoId? ubglAmmoTpl = null;
        if (ubglMod is not null)
        {
            var ubglTemplate = itemHelper.GetItem(ubglMod.Template).Value;
            ubglAmmoTpl = GetWeightedCompatibleAmmo(botTemplateInventory.Ammo, ubglTemplate);
            // this can be null - example - FollowerBoarClose2 can have an UBGL but doesn't have the ammo caliber defined in its json
            // the default ammo passed from GetWeightCompatibleAmmo can be null
            if (ubglAmmoTpl is not null)
            {
                FillUbgl(weaponWithModsArray, ubglMod, ubglAmmoTpl.Value);
            }
        }

        return new GenerateWeaponResult
        {
            Weapon = weaponWithModsArray,
            ChosenAmmoTemplate = ammoTpl,
            ChosenUbglAmmoTemplate = ubglAmmoTpl,
            WeaponMods = modPool,
            WeaponTemplate = weaponItemTemplate,
        };
    }

    /// <summary>
    ///     Insert cartridge(s) into a weapon
    ///     Handles all chambers - patron_in_weapon, patron_in_weapon_000 etc
    /// </summary>
    /// <param name="weaponWithModsList">Weapon and mods</param>
    /// <param name="ammoTemplate">Cartridge to add to weapon</param>
    /// <param name="chamberSlotIds">Name of slots to create or add ammo to</param>
    protected void AddCartridgeToChamber(List<Item> weaponWithModsList, MongoId ammoTemplate, IEnumerable<string> chamberSlotIds)
    {
        foreach (var slotId in chamberSlotIds)
        {
            var existingItemWithSlot = weaponWithModsList.FirstOrDefault(x => x.SlotId == slotId);
            if (existingItemWithSlot is null)
            {
                // Not found, add new slot to weapon
                weaponWithModsList.Add(
                    new Item
                    {
                        Id = new MongoId(),
                        Template = ammoTemplate,
                        ParentId = weaponWithModsList[0].Id,
                        SlotId = slotId,
                        Upd = new Upd { StackObjectsCount = 1 },
                    }
                );
            }
            else
            {
                // Already exists, update values
                existingItemWithSlot.Template = ammoTemplate;
                existingItemWithSlot.Upd = new Upd { StackObjectsCount = 1 };
            }
        }
    }

    /// <summary>
    ///     Create a list with weapon base as the only element and
    ///     add additional properties based on weapon type
    /// </summary>
    /// <param name="weaponTemplate">Weapon template to create item with</param>
    /// <param name="weaponParentId">Weapons parent id</param>
    /// <param name="equipmentSlot">e.g. primary/secondary/holster</param>
    /// <param name="weaponItemTemplate">Database template for weapon</param>
    /// <param name="botRole">For durability values</param>
    /// <returns>Base weapon item in a list</returns>
    protected IEnumerable<Item> ConstructWeaponBaseList(
        MongoId weaponTemplate,
        string weaponParentId,
        string equipmentSlot,
        TemplateItem weaponItemTemplate,
        string botRole
    )
    {
        return
        [
            new Item
            {
                Id = new MongoId(),
                Template = weaponTemplate,
                ParentId = weaponParentId,
                SlotId = equipmentSlot,
                Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(weaponItemTemplate, botRole),
            },
        ];
    }

    /// <summary>
    ///     Get the mods necessary to kit out a weapon to its preset level
    /// </summary>
    /// <param name="weaponTemplate">Weapon to find preset for</param>
    /// <param name="equipmentSlot">The slot the weapon will be placed in</param>
    /// <param name="weaponParentId">Value used for the parent id</param>
    /// <param name="itemTemplate">Item template</param>
    /// <param name="botRole">Bot role</param>
    /// <returns>List of weapon mods</returns>
    protected List<Item> GetPresetWeaponMods(
        MongoId weaponTemplate,
        string equipmentSlot,
        string weaponParentId,
        TemplateItem itemTemplate,
        string botRole
    )
    {
        // Invalid weapon generated, fallback to preset
        logger.Warning(
            serverLocalisationService.GetText("bot-weapon_generated_incorrect_using_default", $"{weaponTemplate} - {itemTemplate.Name}")
        );
        List<Item> weaponMods = [];

        // TODO: Preset weapons trigger a lot of warnings regarding missing ammo in magazines & such
        Preset? preset = null;
        foreach (var (_, itemPreset) in databaseService.GetGlobals().ItemPresets)
        {
            if (itemPreset.Items[0].Template == weaponTemplate)
            {
                preset = cloner.Clone(itemPreset);

                break;
            }
        }

        if (preset is not null)
        {
            var parentItem = preset.Items[0];
            parentItem.ParentId = weaponParentId;
            parentItem.SlotId = equipmentSlot;
            parentItem.Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(itemTemplate, botRole);
            preset.Items[0] = parentItem;
            weaponMods.AddRange(preset.Items);
        }
        else
        {
            logger.Error(serverLocalisationService.GetText("bot-missing_weapon_preset", weaponTemplate));
        }

        return weaponMods;
    }

    /// <summary>
    ///     Checks if all required slots are occupied on a weapon and all its mods.
    /// </summary>
    /// <param name="weaponAndChildren">Weapon + mods</param>
    /// <param name="botRole">Role of bot weapon is for</param>
    /// <returns>True if valid</returns>
    protected bool IsWeaponValid(List<Item> weaponAndChildren, string botRole)
    {
        // Key weapon + children by parentId + slot name, ignore items without parentId or slotId
        var slotItemLookup = weaponAndChildren.ToLookup(item => (item.ParentId, item.SlotId));

        foreach (var item in weaponAndChildren)
        {
            var modTemplate = itemHelper.GetItem(item.Template).Value;
            if (!modTemplate?.Properties?.Slots?.Any() ?? false)
            {
                continue;
            }

            var requiredSlots = modTemplate?.Properties?.Slots?.Where(slot => slot.Required.GetValueOrDefault(false)) ?? [];
            if (!requiredSlots.Any())
            {
                // No required slots, skip to next item in weapon
                continue;
            }

            foreach (var requiredSlot in requiredSlots.ToList())
            {
                // Check if slot exists in cache
                if (!slotItemLookup[(item.Id, requiredSlot.Name)].Any())
                {
                    logger.Warning(
                        serverLocalisationService.GetText(
                            "bot-weapons_required_slot_missing_item",
                            new
                            {
                                modSlot = requiredSlot.Name,
                                modName = modTemplate.Name,
                                slotId = item.SlotId,
                                botRole,
                            }
                        )
                    );

                    return false;
                }
            }

            return true;
        }

        return true;
    }

    /// <summary>
    ///     Generates extra magazines or bullets (if magazine is internal) and adds them to TacticalVest and Pockets.
    ///     Additionally, adds extra bullets to SecuredContainer
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="generatedWeaponResult">Object with properties for generated weapon (weapon mods pool / weapon template / ammo tpl)</param>
    /// <param name="magWeights">Magazine weights for count to add to inventory</param>
    /// <param name="inventory">Inventory to add magazines to</param>
    /// <param name="botRole">The bot type we're generating extra mags for</param>
    public void AddExtraMagazinesToInventory(
        MongoId botId,
        GenerateWeaponResult generatedWeaponResult,
        GenerationData magWeights,
        BotBaseInventory inventory,
        string botRole
    )
    {
        var weaponAndMods = generatedWeaponResult.Weapon;
        var weaponTemplate = generatedWeaponResult.WeaponTemplate;
        var magazineTpl = GetMagazineTemplateFromWeaponTemplate(weaponAndMods, weaponTemplate, botRole);

        var magTemplate = itemHelper.GetItem(magazineTpl.Value).Value;
        if (magTemplate is null)
        {
            logger.Error(serverLocalisationService.GetText("bot-unable_to_find_magazine_item", magazineTpl));

            return;
        }

        //var isInternalMag = magTemplate.Properties.ReloadMagType == ReloadMode.InternalMagazine;
        var ammoTemplate = itemHelper.GetItem(generatedWeaponResult.ChosenAmmoTemplate);
        if (!ammoTemplate.Key)
        {
            logger.Error(serverLocalisationService.GetText("bot-unable_to_find_ammo_item", generatedWeaponResult.ChosenAmmoTemplate));

            return;
        }

        // Has an UBGL
        if (generatedWeaponResult.ChosenUbglAmmoTemplate is not null && !generatedWeaponResult.ChosenUbglAmmoTemplate.Value.IsEmpty)
        {
            AddUbglGrenadesToBotInventory(botId, weaponAndMods, generatedWeaponResult, inventory);
        }

        var inventoryMagGenModel = new InventoryMagGen(magWeights, magTemplate, weaponTemplate, ammoTemplate.Value, inventory, botId);

        InventoryMagGenComponents.FirstOrDefault(v => v.CanHandleInventoryMagGen(inventoryMagGenModel)).Process(inventoryMagGenModel);

        // Add x stacks of bullets to SecuredContainer (bots use a magic mag packing skill to reload instantly)
        AddAmmoToSecureContainer(
            botId,
            BotConfig.SecureContainerAmmoStackCount,
            generatedWeaponResult.ChosenAmmoTemplate,
            ammoTemplate.Value.Properties.StackMaxSize ?? 0,
            inventory
        );
    }

    /// <summary>
    ///     Add Grenades for UBGL to bot's vest and secure container
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="weaponMods">Weapon list with mods</param>
    /// <param name="generatedWeaponResult">Result of weapon generation</param>
    /// <param name="inventory">Bot inventory to add grenades to</param>
    protected void AddUbglGrenadesToBotInventory(
        MongoId botId,
        List<Item> weaponMods,
        GenerateWeaponResult generatedWeaponResult,
        BotBaseInventory inventory
    )
    {
        // Find ubgl mod item + get details of it from db
        var ubglMod = weaponMods.FirstOrDefault(x => x.SlotId == "mod_launcher");
        var ubglDbTemplate = itemHelper.GetItem(ubglMod.Template).Value;

        // Define min/max of how many grenades bot will have
        GenerationData ubglMinMax = new()
        {
            Weights = new Dictionary<double, double> { { 1, 1 }, { 2, 1 } },
            Whitelist = new Dictionary<MongoId, double>(),
        };

        // get ammo template from db
        var ubglAmmoDbTemplate = itemHelper.GetItem(generatedWeaponResult.ChosenUbglAmmoTemplate.Value).Value;

        // Add grenades to bot inventory
        var ubglAmmoGenModel = new InventoryMagGen(ubglMinMax, ubglDbTemplate, ubglDbTemplate, ubglAmmoDbTemplate, inventory, botId);
        InventoryMagGenComponents.FirstOrDefault(v => v.CanHandleInventoryMagGen(ubglAmmoGenModel)).Process(ubglAmmoGenModel);

        // Store extra grenades in secure container
        AddAmmoToSecureContainer(botId, 5, generatedWeaponResult.ChosenUbglAmmoTemplate.Value, 20, inventory);
    }

    /// <summary>
    ///     Add ammo to the secure container.
    /// </summary>
    /// <param name="botId">Id of bot we're adding ammo to</param>
    /// <param name="stackCount">How many stacks of ammo to add.</param>
    /// <param name="ammoTpl">Ammo type to add.</param>
    /// <param name="stackSize">Size of the ammo stack to add.</param>
    /// <param name="inventory">Player inventory.</param>
    protected void AddAmmoToSecureContainer(MongoId botId, int stackCount, MongoId ammoTpl, int stackSize, BotBaseInventory inventory)
    {
        var container = new HashSet<EquipmentSlots> { EquipmentSlots.SecuredContainer };
        for (var i = 0; i < stackCount; i++)
        {
            var id = new MongoId();
            botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                botId,
                container,
                id,
                ammoTpl,
                [
                    new Item
                    {
                        Id = id,
                        Template = ammoTpl,
                        Upd = new Upd { StackObjectsCount = stackSize },
                    },
                ],
                inventory
            );
        }
    }

    /// <summary>
    ///     Get a weapons magazine template from a weapon template.
    /// </summary>
    /// <param name="weaponMods">Mods from a weapon template.</param>
    /// <param name="weaponTemplate">Weapon to get magazine template for.</param>
    /// <param name="botRole">The bot type we are getting the magazine for.</param>
    /// <returns>Magazine template string.</returns>
    protected MongoId? GetMagazineTemplateFromWeaponTemplate(IEnumerable<Item> weaponMods, TemplateItem weaponTemplate, string botRole)
    {
        var magazine = weaponMods.FirstOrDefault(m => m.SlotId == ModMagazineSlotId);
        if (magazine is null)
        {
            // Edge case - magazineless chamber loaded weapons don't have magazines, e.g. mp18
            // return default mag tpl
            if (weaponTemplate.Properties.ReloadMode == ReloadMode.OnlyBarrel)
            {
                return weaponTemplate.GetWeaponsDefaultMagazineTpl();
            }

            // log error if no magazine AND not a chamber loaded weapon (e.g. shotgun revolver)
            if (!weaponTemplate.Properties.IsChamberLoad ?? false)
            // Shouldn't happen
            {
                logger.Warning(
                    serverLocalisationService.GetText(
                        "bot-weapon_missing_magazine_or_chamber",
                        new { weaponId = weaponTemplate.Id, botRole }
                    )
                );
            }

            var defaultMagTplId = weaponTemplate.GetWeaponsDefaultMagazineTpl();

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(
                    $"[{botRole}] Unable to find magazine for weapon: {weaponTemplate.Id} {weaponTemplate.Name}, using mag template default: {defaultMagTplId}."
                );
            }

            return defaultMagTplId;
        }

        return magazine.Template;
    }

    /// <summary>
    ///     Finds and returns a compatible ammo template based on the bots ammo weightings (x.json/inventory/equipment/ammo)
    /// </summary>
    /// <param name="cartridgePool">Dictionary of all cartridges keyed by type e.g. Caliber556x45NATO</param>
    /// <param name="weaponTemplate">Weapon details from database we want to pick ammo for</param>
    /// <returns>Ammo template that works with the desired gun</returns>
    protected MongoId GetWeightedCompatibleAmmo(Dictionary<string, Dictionary<MongoId, double>> cartridgePool, TemplateItem weaponTemplate)
    {
        var desiredCaliber = GetWeaponCaliber(weaponTemplate);
        if (!cartridgePool.TryGetValue(desiredCaliber, out var cartridgePoolForWeapon) || cartridgePoolForWeapon?.Count == 0)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(
                    serverLocalisationService.GetText(
                        "bot-no_caliber_data_for_weapon_falling_back_to_default",
                        new
                        {
                            weaponId = weaponTemplate.Id,
                            weaponName = weaponTemplate.Name,
                            defaultAmmo = weaponTemplate.Properties.DefAmmo,
                        }
                    )
                );
            }

            // Immediately returns, default ammo is guaranteed to be compatible
            // it is not guaranteed to even have a default ammo
            return weaponTemplate.Properties.DefAmmo.Value;
        }

        // Get cartridges the weapons first chamber allow
        var compatibleCartridgesInTemplate = GetCompatibleCartridgesFromWeaponTemplate(weaponTemplate);
        if (compatibleCartridgesInTemplate.Count == 0)
        // No chamber data found in weapon, send default
        {
            return weaponTemplate.Properties.DefAmmo.Value;
        }

        // Inner join the weapons allowed + passed in cartridge pool to get compatible cartridges
        Dictionary<MongoId, double> compatibleCartridges = new();
        foreach (var cartridge in cartridgePoolForWeapon)
        {
            if (compatibleCartridgesInTemplate.Contains(cartridge.Key))
            {
                compatibleCartridges[cartridge.Key] = cartridge.Value;
            }
        }

        // No cartridges found, try and get something that's compatible with the gun
        if (!compatibleCartridges.Any())
        {
            // Get cartridges from the weapons first magazine in filters
            var compatibleCartridgesInMagazine = GetCompatibleCartridgesFromMagazineTemplate(weaponTemplate);
            if (compatibleCartridgesInMagazine.Count == 0)
            {
                // No compatible cartridges found in magazine, use default
                return weaponTemplate.Properties.DefAmmo.Value;
            }

            // Get the caliber data from the first compatible round in the magazine
            var magazineCaliberData = itemHelper.GetItem(compatibleCartridgesInMagazine.FirstOrDefault()).Value.Properties.Caliber;
            cartridgePoolForWeapon = cartridgePool[magazineCaliberData];

            foreach (var cartridgeKvP in cartridgePoolForWeapon)
            {
                if (compatibleCartridgesInMagazine.Contains(cartridgeKvP.Key))
                {
                    compatibleCartridges[cartridgeKvP.Key] = cartridgeKvP.Value;
                }
            }

            // Nothing found after also checking magazines, return default ammo
            if (compatibleCartridges.Count == 0)
            {
                return weaponTemplate.Properties.DefAmmo.Value;
            }
        }

        return weightedRandomHelper.GetWeightedValue(compatibleCartridges);
    }

    /// <summary>
    ///     Get the cartridge ids from a weapon template that work with the weapon
    /// </summary>
    /// <param name="weaponTemplate">Weapon db template to get cartridges for</param>
    /// <returns>List of cartridge tpls</returns>
    protected HashSet<MongoId> GetCompatibleCartridgesFromWeaponTemplate(TemplateItem weaponTemplate)
    {
        ArgumentNullException.ThrowIfNull(weaponTemplate);

        var cartridges = weaponTemplate.Properties?.Chambers?.FirstOrDefault()?.Properties?.Filters?.First().Filter;
        if (cartridges is not null)
        {
            return cartridges;
        }

        // Fallback to the magazine if possible, e.g. for revolvers
        return GetCompatibleCartridgesFromMagazineTemplate(weaponTemplate);
    }

    /// <summary>
    ///     Get the cartridge ids from a weapon's magazine template that work with the weapon
    /// </summary>
    /// <param name="weaponTemplate">Weapon db template to get magazine cartridges for</param>
    /// <returns>Hashset of cartridge tpls</returns>
    /// <exception cref="ArgumentNullException">Thrown when weaponTemplate is null.</exception>
    protected HashSet<MongoId> GetCompatibleCartridgesFromMagazineTemplate(TemplateItem weaponTemplate)
    {
        ArgumentNullException.ThrowIfNull(weaponTemplate);

        // Get the first magazine's template from the weapon
        var magazineSlot = weaponTemplate.Properties.Slots?.FirstOrDefault(slot => slot.Name == "mod_magazine");
        if (magazineSlot is null)
        {
            return [];
        }

        var magazineTemplate = itemHelper.GetItem(
            magazineSlot.Properties?.Filters.FirstOrDefault()?.Filter?.FirstOrDefault() ?? new MongoId(null)
        );
        if (!magazineTemplate.Key)
        {
            return [];
        }

        // Try to get cartridges from slots array first, if none found, try Cartridges array
        var cartridges =
            magazineTemplate.Value.Properties.Slots.FirstOrDefault()?.Properties?.Filters.FirstOrDefault()?.Filter ?? magazineTemplate
                .Value.Properties.Cartridges.FirstOrDefault()
                ?.Properties?.Filters.FirstOrDefault()
                ?.Filter;

        return cartridges ?? [];
    }

    /// <summary>
    ///     Get a weapons compatible cartridge caliber
    /// </summary>
    /// <param name="weaponTemplate">Weapon to look up caliber of</param>
    /// <returns>Caliber as string</returns>
    protected string? GetWeaponCaliber(TemplateItem weaponTemplate)
    {
        if (!string.IsNullOrEmpty(weaponTemplate.Properties.Caliber))
        {
            return weaponTemplate.Properties.Caliber;
        }

        if (!string.IsNullOrEmpty(weaponTemplate.Properties.AmmoCaliber))
        // 9x18pmm has a typo, should be Caliber9x18PM
        {
            return weaponTemplate.Properties.AmmoCaliber == "Caliber9x18PMM" ? "Caliber9x18PM" : weaponTemplate.Properties.AmmoCaliber;
        }

        if (!string.IsNullOrEmpty(weaponTemplate.Properties.LinkedWeapon))
        {
            var ammoInChamber = itemHelper.GetItem(
                weaponTemplate.Properties.Chambers.First().Properties.Filters.First().Filter.FirstOrDefault()
            );
            return !ammoInChamber.Key ? null : ammoInChamber.Value.Properties.Caliber;
        }

        return null;
    }

    /// <summary>
    ///     Fill existing magazines to full, while replacing their contents with specified ammo
    /// </summary>
    /// <param name="weaponMods">Weapon with children</param>
    /// <param name="magazine">Magazine item</param>
    /// <param name="cartridgeTemplate">Cartridge to insert into magazine</param>
    protected void FillExistingMagazines(List<Item> weaponMods, Item magazine, MongoId cartridgeTemplate)
    {
        var magazineTemplate = itemHelper.GetItem(magazine.Template).Value;
        if (magazineTemplate is null)
        {
            logger.Error(serverLocalisationService.GetText("bot-unable_to_find_magazine_item", magazine.Template));

            return;
        }

        // Magazine, usually
        var parentDbItem = itemHelper.GetItem(magazineTemplate.Parent).Value;

        // Revolver shotgun (MTs-255-12) uses a magazine with chambers, not cartridges ("camora_xxx")
        // Exchange of the camora ammo is not necessary we could also just check for stackSize > 0 here
        // and remove the else
        if (botWeaponGeneratorHelper.MagazineIsCylinderRelated(parentDbItem.Name))
        {
            FillCamorasWithAmmo(weaponMods, magazine.Id, cartridgeTemplate);
        }
        else
        {
            AddOrUpdateMagazinesChildWithAmmo(weaponMods, magazine, cartridgeTemplate, magazineTemplate);
        }
    }

    /// <summary>
    ///     Add desired ammo template as item to weapon modifications list, placed as child to UBGL.
    /// </summary>
    /// <param name="weaponMods">Weapon with children.</param>
    /// <param name="ubglMod">Underbarrrel grenade launcher item.</param>
    /// <param name="ubglAmmoTpl">Grenade ammo template.</param>
    protected void FillUbgl(List<Item> weaponMods, Item ubglMod, MongoId ubglAmmoTpl)
    {
        weaponMods.Add(
            new Item
            {
                Id = new MongoId(),
                Template = ubglAmmoTpl,
                ParentId = ubglMod.Id,
                SlotId = "patron_in_weapon",
                Upd = new Upd { StackObjectsCount = 1 },
            }
        );
    }

    /// <summary>
    ///     Add cartridges to a weapons magazine
    /// </summary>
    /// <param name="weaponWithMods">Weapon with magazine to amend</param>
    /// <param name="magazine">Magazine item details we're adding cartridges to</param>
    /// <param name="chosenAmmoTpl">Cartridge to put into the magazine</param>
    /// <param name="magazineTemplate">Magazines db template</param>
    protected void AddOrUpdateMagazinesChildWithAmmo(
        List<Item> weaponWithMods,
        Item magazine,
        MongoId chosenAmmoTpl,
        TemplateItem magazineTemplate
    )
    {
        var magazineCartridgeChildItem = weaponWithMods.FirstOrDefault(m => m.ParentId == magazine.Id && m.SlotId == "cartridges");
        if (magazineCartridgeChildItem is not null)
        {
            // Delete the existing cartridge object and create fresh below
            weaponWithMods.Remove(magazineCartridgeChildItem);
        }

        // Create array with just magazine
        List<Item> magazineWithCartridges = [magazine];

        // Add cartridges as children to above mag array
        itemHelper.FillMagazineWithCartridge(magazineWithCartridges, magazineTemplate, chosenAmmoTpl, 1);

        // Replace existing magazine with above array of mag + cartridge stacks
        var magazineIndex = weaponWithMods.FindIndex(i => i.Id == magazine.Id); // magazineWithCartridges
        if (magazineIndex == -1)
        {
            logger.Error($"Unable to add cartridges: {chosenAmmoTpl} to magazine: {magazine.Id} as none found");

            return;
        }

        weaponWithMods.RemoveAt(magazineIndex);

        // Insert new mag at same index position original was
        weaponWithMods.InsertRange(magazineIndex, magazineWithCartridges);
    }

    /// <summary>
    ///     Fill each Camora with a bullet
    /// </summary>
    /// <param name="weaponMods">Weapon mods to find and update camora mod(s) from</param>
    /// <param name="magazineId">Magazine id to find and add to</param>
    /// <param name="ammoTpl">Ammo template id to hydrate with</param>
    protected void FillCamorasWithAmmo(IEnumerable<Item> weaponMods, MongoId magazineId, MongoId ammoTpl)
    {
        // for CylinderMagazine we exchange the ammo in the "camoras".
        // This might not be necessary since we already filled the camoras with a random whitelisted and compatible ammo type,
        // but I'm not sure whether this is also used elsewhere
        var camoras = weaponMods.Where(x => x.ParentId == magazineId && x.SlotId.StartsWith("camora", StringComparison.Ordinal)).ToList();

        if (camoras.Count == 0)
        {
            return;
        }

        foreach (var camora in camoras)
        {
            camora.Template = ammoTpl;
            if (camora.Upd is not null)
            {
                camora.Upd.StackObjectsCount = 1;
            }
            else
            {
                camora.Upd = new Upd { StackObjectsCount = 1 };
            }
        }
    }
}
