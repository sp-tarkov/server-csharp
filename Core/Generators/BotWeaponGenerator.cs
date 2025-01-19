using Core.Annotations;
using Core.Generators.WeaponGen;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Generators;

[Injectable(InjectionType.Singleton)]
public class BotWeaponGenerator(
    ISptLogger<BotWeaponGenerator> _logger,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    WeightedRandomHelper _weightedRandomHelper,
    BotGeneratorHelper _botGeneratorHelper,
    RandomUtil _randomUtil,
    BotWeaponGeneratorHelper _botWeaponGeneratorHelper,
    BotWeaponModLimitService _botWeaponModLimitService,
    BotEquipmentModGenerator _botEquipmentModGenerator,
    LocalisationService _localisationService,
    RepairService _repairService,
    ICloner _cloner,
    ConfigServer _configServer,
    IEnumerable<IInventoryMagGen> inventoryMagGenComponents
)
{
    protected List<IInventoryMagGen> _inventoryMagGenComponents = MagGenSetUp(inventoryMagGenComponents);
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();
    protected RepairConfig _repairConfig = _configServer.GetConfig<RepairConfig>();
    protected const string _modMagazineSlotId = "mod_magazine";

    private static List<IInventoryMagGen> MagGenSetUp(IEnumerable<IInventoryMagGen> components)
    {
        var inventoryMagGens = components.ToList();
        inventoryMagGens.ToList()
            .Sort(
                (a, b) =>
                    a.GetPriority() -
                    b.GetPriority()
            );
        return inventoryMagGens.ToList();
    }

    /// <summary>
    /// Pick a random weapon based on weightings and generate a functional weapon
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <param name="weaponParentId"></param>
    /// <param name="modChances"></param>
    /// <param name="botRole">Role of bot, e.g. assault/followerBully</param>
    /// <param name="isPmc">Is weapon generated for a pmc</param>
    /// <param name="botLevel"></param>
    /// <returns>GenerateWeaponResult object</returns>
    public GenerateWeaponResult GenerateRandomWeapon(string sessionId, string equipmentSlot, BotTypeInventory botTemplateInventory, string weaponParentId,
        Dictionary<string, double> modChances, string botRole, bool isPmc, int botLevel)
    {
        var weaponTpl = PickWeightedWeaponTemplateFromPool(equipmentSlot, botTemplateInventory);
        return GenerateWeaponByTpl(
            sessionId,
            weaponTpl,
            equipmentSlot,
            botTemplateInventory,
            weaponParentId,
            modChances,
            botRole,
            isPmc,
            botLevel
        );
    }

    /// <summary>
    /// Gets a random weighted weapon from a bot's pool of weapons.
    /// </summary>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <returns>Weapon template</returns>
    public string PickWeightedWeaponTemplateFromPool(string equipmentSlot, BotTypeInventory botTemplateInventory)
    {
        EquipmentSlots key;
        if (!EquipmentSlots.TryParse(equipmentSlot, out key))
            _logger.Error($"Unable to parse equipment slot '{equipmentSlot}'");

        var weaponPool = botTemplateInventory.Equipment[key];
        return _weightedRandomHelper.GetWeightedValue<string>(weaponPool);
    }

    /// <summary>
    /// Generates a weapon based on the supplied weapon template.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="weaponTpl">Weapon template to generate (use pickWeightedWeaponTplFromPool()).</param>
    /// <param name="slotName">Slot to fit into, primary/secondary/holster.</param>
    /// <param name="botTemplateInventory">e.g. assault.json.</param>
    /// <param name="weaponParentId">Parent ID of the weapon being generated.</param>
    /// <param name="modChances">Dictionary of item types and % chance weapon will have that mod.</param>
    /// <param name="botRole">e.g. assault/exusec.</param>
    /// <param name="isPmc">Is weapon being generated for a PMC.</param>
    /// <param name="botLevel">The level of the bot.</param>
    /// <returns>GenerateWeaponResult object.</returns>
    public GenerateWeaponResult? GenerateWeaponByTpl(string sessionId, string weaponTpl, string slotName, BotTypeInventory botTemplateInventory,
        string weaponParentId, Dictionary<string, double> modChances, string botRole, bool isPmc, int botLevel)
    {
        var modPool = botTemplateInventory.Mods;
        var weaponItemTemplate = _itemHelper.GetItem(weaponTpl).Value;

        if (weaponItemTemplate is null)
        {
            _logger.Error(_localisationService.GetText("bot-missing_item_template", weaponTpl));
            _logger.Error($"WeaponSlot -> {slotName}");

            return null;
        }

        // Find ammo to use when filling magazines/chamber
        if (botTemplateInventory.Ammo is null)
        {
            _logger.Error(_localisationService.GetText("bot-no_ammo_found_in_bot_json", botRole));
            _logger.Error(_localisationService.GetText("bot-generation_failed"));
        }

        var ammoTpl = GetWeightedCompatibleAmmo(botTemplateInventory.Ammo, weaponItemTemplate);

        // Create with just base weapon item
        var weaponWithModsArray = ConstructWeaponBaseList(
            weaponTpl,
            weaponParentId,
            slotName,
            weaponItemTemplate,
            botRole
        );

        // Chance to add randomised weapon enhancement
        if (isPmc && _randomUtil.GetChance100(_pmcConfig.WeaponHasEnhancementChancePercent))
        {
            // Add buff to weapon root
            _repairService.AddBuff(_repairConfig.RepairKit.Weapon, weaponWithModsArray[0]);
        }

        // Add mods to weapon base
        if (modPool.Keys.Contains(weaponTpl))
        {
            // Role to treat bot as e.g. pmc/scav/boss
            var botEquipmentRole = _botGeneratorHelper.GetBotEquipmentRole(botRole);

            // Different limits if bot is boss vs scav
            var modLimits = _botWeaponModLimitService.GetWeaponModLimits(botEquipmentRole);

            GenerateWeaponRequest generateWeaponModsRequest = new()
            {
                Weapon = weaponWithModsArray, // Will become hydrated array of weapon + mods
                ModPool = modPool,
                WeaponId = weaponWithModsArray[0].Id, // Weapon root id
                ParentTemplate = weaponItemTemplate,
                ModSpawnChances = modChances,
                AmmoTpl = ammoTpl,
                BotData = new() { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                ModLimits = modLimits,
                WeaponStats = new(),
                ConflictingItemTpls = new(),
            };
            weaponWithModsArray = _botEquipmentModGenerator.GenerateModsForWeapon(
                sessionId,
                generateWeaponModsRequest
            );
        }

        // Use weapon preset from globals.json if weapon isnt valid
        if (!IsWeaponValid(weaponWithModsArray, botRole))
        {
            // Weapon is bad, fall back to weapons preset
            weaponWithModsArray = GetPresetWeaponMods(
                weaponTpl,
                slotName,
                weaponParentId,
                weaponItemTemplate,
                botRole
            );
        }

        var tempList = _cloner.Clone(weaponWithModsArray.Where((item) => item.SlotId == _modMagazineSlotId));
        // Fill existing magazines to full and sync ammo type
        foreach (var magazine in tempList)
        {
            FillExistingMagazines(weaponWithModsArray, magazine, ammoTpl);
        }

        // Add cartridge(s) to gun chamber(s)
        if (weaponItemTemplate.Properties.Chambers?.Any() ??
            false &&
            weaponItemTemplate.Properties.Chambers[0].Props.Filters[0].Filter.Contains(ammoTpl))
        {
            // Guns have variety of possible Chamber ids, patron_in_weapon/patron_in_weapon_000/patron_in_weapon_001
            var chamberSlotNames = weaponItemTemplate.Properties.Chambers.Select((chamberSlot) => chamberSlot.Name);
            AddCartridgeToChamber(weaponWithModsArray, ammoTpl, chamberSlotNames.ToList());
        }

        // Fill UBGL if found
        var ubglMod = weaponWithModsArray.FirstOrDefault((x) => x.SlotId == "mod_launcher");
        string? ubglAmmoTpl = null;
        if (ubglMod is not null)
        {
            var ubglTemplate = _itemHelper.GetItem(ubglMod.Template).Value;
            ubglAmmoTpl = GetWeightedCompatibleAmmo(botTemplateInventory.Ammo, ubglTemplate);
            FillUbgl(weaponWithModsArray, ubglMod, ubglAmmoTpl);
        }

        return new()
        {
            Weapon = weaponWithModsArray,
            ChosenAmmoTemplate = ammoTpl,
            ChosenUbglAmmoTemplate = ubglAmmoTpl,
            WeaponMods = modPool,
            WeaponTemplate = weaponItemTemplate,
        };
    }

    /// <summary>
    /// Insert cartridge(s) into a weapon
    /// Handles all chambers - patron_in_weapon, patron_in_weapon_000 etc
    /// </summary>
    /// <param name="weaponWithModsList">Weapon and mods</param>
    /// <param name="ammoTemplate">Cartridge to add to weapon</param>
    /// <param name="chamberSlotIdentifiers">Name of slots to create or add ammo to</param>
    protected void AddCartridgeToChamber(List<Item> weaponWithModsList, string ammoTemplate, List<string> chamberSlotIds)
    {
        foreach (var slotId in chamberSlotIds)
        {
            var existingItemWithSlot = weaponWithModsList.FirstOrDefault((x) => x.SlotId == slotId);
            if (existingItemWithSlot is null)
            {
                // Not found, add new slot to weapon
                weaponWithModsList.Add(
                    new()
                    {
                        Id = _hashUtil.Generate(),
                        Template = ammoTemplate,
                        ParentId = weaponWithModsList[0].Id,
                        SlotId = slotId,
                        Upd = new() { StackObjectsCount = 1 },
                    }
                );
            }
            else
            {
                // Already exists, update values
                existingItemWithSlot.Template = ammoTemplate;
                existingItemWithSlot.Upd = new() { StackObjectsCount = 1 };
            }
        }
    }

    /// <summary>
    /// Create a list with weapon base as the only element and
    /// add additional properties based on weapon type
    /// </summary>
    /// <param name="weaponTemplate">Weapon template to create item with</param>
    /// <param name="weaponParentId">Weapons parent id</param>
    /// <param name="equipmentSlot">e.g. primary/secondary/holster</param>
    /// <param name="weaponItemTemplate">Database template for weapon</param>
    /// <param name="botRole">For durability values</param>
    /// <returns>Base weapon item in a list</returns>
    protected List<Item> ConstructWeaponBaseList(string weaponTemplate, string weaponParentId, string equipmentSlot, TemplateItem weaponItemTemplate,
        string botRole)
    {
        return
        [
            new()
            {
                Id = _hashUtil.Generate(),
                Template = weaponTemplate,
                ParentId = weaponParentId,
                SlotId = equipmentSlot,
                Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(weaponItemTemplate, botRole)
            }
        ];
    }

    /// <summary>
    /// Get the mods necessary to kit out a weapon to its preset level
    /// </summary>
    /// <param name="weaponTemplate">Weapon to find preset for</param>
    /// <param name="equipmentSlot">The slot the weapon will be placed in</param>
    /// <param name="weaponParentId">Value used for the parent id</param>
    /// <param name="itemTemplate">Item template</param>
    /// <param name="botRole">Bot role</param>
    /// <returns>List of weapon mods</returns>
    protected List<Item> GetPresetWeaponMods(string weaponTemplate, string equipmentSlot, string weaponParentId, TemplateItem itemTemplate, string botRole)
    {
        // Invalid weapon generated, fallback to preset
        _logger.Warning(
            _localisationService.GetText($"bot-weapon_generated_incorrect_using_default {weaponTemplate} {itemTemplate.Name}")
        );
        List<Item> weaponMods = [];

        // TODO: Preset weapons trigger a lot of warnings regarding missing ammo in magazines & such
        Preset preset = null;
        foreach (var (_, itemPreset) in _databaseService.GetGlobals().ItemPresets)
        {
            if (itemPreset.Items[0].Template == weaponTemplate)
            {
                preset = _cloner.Clone(itemPreset);

                break;
            }
        }

        if (preset is not null)
        {
            var parentItem = preset.Items[0];
            parentItem.ParentId = weaponParentId;
            parentItem.SlotId = equipmentSlot;
            parentItem.Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(itemTemplate, botRole);
            preset.Items[0] = parentItem;
            weaponMods.AddRange(preset.Items);
        }
        else
        {
            _logger.Error(_localisationService.GetText("bot-missing_weapon_preset", weaponTemplate));
        }

        return weaponMods;
    }

    /// <summary>
    /// Checks if all required slots are occupied on a weapon and all its mods.
    /// </summary>
    /// <param name="weaponItemList">Weapon + mods</param>
    /// <param name="botRole">Role of bot weapon is for</param>
    /// <returns>True if valid</returns>
    protected bool IsWeaponValid(List<Item> weaponItemList, string botRole)
    {
        foreach (var mod in weaponItemList)
        {
            var modTemplate = _itemHelper.GetItem(mod.Template).Value;
            if (!modTemplate.Properties.Slots?.Any() ?? false)
            {
                continue;
            }

            // Iterate over required slots in db item, check mod exists for that slot
            foreach (var modSlotTemplate in modTemplate.Properties.Slots.Where((slot) => slot.Required ?? false))
            {
                var slotName = modSlotTemplate.Name;
                var hasWeaponSlotItem = weaponItemList.Any(
                    (weaponItem) => weaponItem.ParentId == mod.Id && weaponItem.SlotId == slotName
                );
                if (!hasWeaponSlotItem)
                {
                    _logger.Warning(
                        _localisationService.GetText(
                            "bot-weapons_required_slot_missing_item",
                            new
                            {
                                modSlot = modSlotTemplate.Name,
                                modName = modTemplate.Name,
                                slotId = mod.SlotId,
                                botRole = botRole,
                            }
                        )
                    );

                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Generates extra magazines or bullets (if magazine is internal) and adds them to TacticalVest and Pockets.
    /// Additionally, adds extra bullets to SecuredContainer
    /// </summary>
    /// <param name="generatedWeaponResult">Object with properties for generated weapon (weapon mods pool / weapon template / ammo tpl)</param>
    /// <param name="magWeights">Magazine weights for count to add to inventory</param>
    /// <param name="inventory">Inventory to add magazines to</param>
    /// <param name="botRole">The bot type we're generating extra mags for</param>
    public void AddExtraMagazinesToInventory(GenerateWeaponResult generatedWeaponResult, GenerationData magWeights, BotBaseInventory inventory, string botRole)
    {
        var weaponAndMods = generatedWeaponResult.Weapon;
        var weaponTemplate = generatedWeaponResult.WeaponTemplate;
        var magazineTpl = GetMagazineTemplateFromWeaponTemplate(weaponAndMods, weaponTemplate, botRole);

        var magTemplate = _itemHelper.GetItem(magazineTpl).Value;
        if (magTemplate is null)
        {
            _logger.Error(_localisationService.GetText("bot-unable_to_find_magazine_item", magazineTpl));

            return;
        }

        var ammoTemplate = _itemHelper.GetItem(generatedWeaponResult.ChosenAmmoTemplate).Value;
        if (ammoTemplate is null)
        {
            _logger.Error(
                _localisationService.GetText("bot-unable_to_find_ammo_item", generatedWeaponResult.ChosenAmmoTemplate)
            );

            return;
        }

        // Has an UBGL
        if (generatedWeaponResult.ChosenUbglAmmoTemplate is not null)
        {
            AddUbglGrenadesToBotInventory(weaponAndMods, generatedWeaponResult, inventory);
        }

        var inventoryMagGenModel = new InventoryMagGen(
            magWeights,
            magTemplate,
            weaponTemplate,
            ammoTemplate,
            inventory
        );

        _inventoryMagGenComponents.FirstOrDefault((v) => v.CanHandleInventoryMagGen(inventoryMagGenModel))
            .Process(inventoryMagGenModel);

        // Add x stacks of bullets to SecuredContainer (bots use a magic mag packing skill to reload instantly)
        AddAmmoToSecureContainer(
            _botConfig.SecureContainerAmmoStackCount,
            generatedWeaponResult.ChosenAmmoTemplate,
            ammoTemplate.Properties.StackMaxSize ?? 0,
            inventory
        );
    }

    /// <summary>
    /// Add Grenades for UBGL to bot's vest and secure container
    /// </summary>
    /// <param name="weaponMods">Weapon list with mods</param>
    /// <param name="generatedWeaponResult">Result of weapon generation</param>
    /// <param name="inventory">Bot inventory to add grenades to</param>
    protected void AddUbglGrenadesToBotInventory(List<Item> weaponMods, GenerateWeaponResult generatedWeaponResult, BotBaseInventory inventory)
    {
        // Find ubgl mod item + get details of it from db
        var ubglMod = weaponMods.FirstOrDefault((x) => x.SlotId == "mod_launcher");
        var ubglDbTemplate = _itemHelper.GetItem(ubglMod.Template).Value;

        // Define min/max of how many grenades bot will have
        GenerationData ubglMinMax = new()
        {
            Weights = new() { { 1, 1 }, { 2, 1 } },
            Whitelist = new(),
        };

        // get ammo template from db
        var ubglAmmoDbTemplate = _itemHelper.GetItem(generatedWeaponResult.ChosenUbglAmmoTemplate).Value;

        // Add greandes to bot inventory
        var ubglAmmoGenModel = new InventoryMagGen(
            ubglMinMax,
            ubglDbTemplate,
            ubglDbTemplate,
            ubglAmmoDbTemplate,
            inventory
        );
        _inventoryMagGenComponents
            .FirstOrDefault((v) => v.CanHandleInventoryMagGen(ubglAmmoGenModel))
            .Process(ubglAmmoGenModel);

        // Store extra grenades in secure container
        AddAmmoToSecureContainer(5, generatedWeaponResult.ChosenUbglAmmoTemplate, 20, inventory);
    }

    /// <summary>
    /// Add ammo to the secure container.
    /// </summary>
    /// <param name="stackCount">How many stacks of ammo to add.</param>
    /// <param name="ammoTpl">Ammo type to add.</param>
    /// <param name="stackSize">Size of the ammo stack to add.</param>
    /// <param name="inventory">Player inventory.</param>
    protected void AddAmmoToSecureContainer(int stackCount, string ammoTemplate, int stackSize, BotBaseInventory inventory)
    {
        for (var i = 0; i < stackCount; i++)
        {
            var id = _hashUtil.Generate();
            _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                new() { EquipmentSlots.SecuredContainer },
                id,
                ammoTemplate,
                new()
                {
                    new() { Id = id, Template = ammoTemplate, Upd = new() { StackObjectsCount = stackSize } }
                },
                inventory
            );
        }
    }

    /// <summary>
    /// Get a weapons magazine template from a weapon template.
    /// </summary>
    /// <param name="weaponMods">Mods from a weapon template.</param>
    /// <param name="weaponTemplate">Weapon to get magazine template for.</param>
    /// <param name="botRole">The bot type we are getting the magazine for.</param>
    /// <returns>Magazine template string.</returns>
    protected string GetMagazineTemplateFromWeaponTemplate(List<Item> weaponMods, TemplateItem weaponTemplate, string botRole)
    {
        var magazine = weaponMods.FirstOrDefault((m) => m.SlotId == _modMagazineSlotId);
        if (magazine is null)
        {
            // Edge case - magazineless chamber loaded weapons dont have magazines, e.g. mp18
            // return default mag tpl
            if (weaponTemplate.Properties.ReloadMode == "OnlyBarrel")
            {
                return _botWeaponGeneratorHelper.GetWeaponsDefaultMagazineTpl(weaponTemplate);
            }

            // log error if no magazine AND not a chamber loaded weapon (e.g. shotgun revolver)
            if (!weaponTemplate.Properties.IsChamberLoad ?? false)
            {
                // Shouldn't happen
                _logger.Warning(
                    _localisationService.GetText(
                        "bot-weapon_missing_magazine_or_chamber",
                        new
                        {
                            weaponId = weaponTemplate.Id,
                            botRole = botRole,
                        }
                    )
                );
            }

            var defaultMagTplId = _botWeaponGeneratorHelper.GetWeaponsDefaultMagazineTpl(weaponTemplate);
            _logger.Debug(
                $"[{botRole}] Unable to find magazine for weapon: {weaponTemplate.Id} {weaponTemplate.Name}, using mag template default: {defaultMagTplId}."
            );

            return defaultMagTplId;
        }

        return magazine.Template;
    }

    /// <summary>
    /// Finds and returns a compatible ammo template based on the bots ammo weightings (x.json/inventory/equipment/ammo)
    /// </summary>
    /// <param name="cartridgePool">Dictionary of all cartridges keyed by type e.g. Caliber556x45NATO</param>
    /// <param name="weaponTemplate">Weapon details from database we want to pick ammo for</param>
    /// <returns>Ammo template that works with the desired gun</returns>
    protected string GetWeightedCompatibleAmmo(Dictionary<string, Dictionary<string, double>> cartridgePool, TemplateItem weaponTemplate)
    {
        var desiredCaliber = GetWeaponCaliber(weaponTemplate);

        var cartridgePoolForWeapon = cartridgePool[desiredCaliber];
        if (cartridgePoolForWeapon is not null || cartridgePoolForWeapon?.Count == 0)
        {
            _logger.Debug(
                _localisationService.GetText(
                    "bot-no_caliber_data_for_weapon_falling_back_to_default",
                    new
                    {
                        weaponId = weaponTemplate.Id,
                        weaponName = weaponTemplate.Name,
                        defaultAmmo = weaponTemplate.Properties.DefAmmo,
                    }
                )
            );

            // Immediately returns, default ammo is guaranteed to be compatible
            return weaponTemplate.Properties.DefAmmo;
        }

        // Get cartridges the weapons first chamber allow
        var compatibleCartridgesInTemplate = GetCompatibleCartridgesFromWeaponTemplate(weaponTemplate);
        if (compatibleCartridgesInTemplate is null)
        {
            // No chamber data found in weapon, send default
            return weaponTemplate.Properties.DefAmmo;
        }

        // Inner join the weapons allowed + passed in cartridge pool to get compatible cartridges
        Dictionary<string, double> compatibleCartridges = new();
        foreach (var cartridge in cartridgePoolForWeapon)
        {
            if (compatibleCartridgesInTemplate.Contains(cartridge.Key))
            {
                compatibleCartridges[cartridge.Key] = cartridgePoolForWeapon[cartridge.Key];
            }
        }

        if (!compatibleCartridges.Any())
        {
            // No compatible cartridges, use default
            return weaponTemplate.Properties.DefAmmo;
        }

        return _weightedRandomHelper.GetWeightedValue<string>(compatibleCartridges);
    }

    /// <summary>
    /// Get the cartridge ids from a weapon template that work with the weapon
    /// </summary>
    /// <param name="weaponTemplate">Weapon db template to get cartridges for</param>
    /// <returns>List of cartridge tpls</returns>
    protected List<string> GetCompatibleCartridgesFromWeaponTemplate(TemplateItem weaponTemplate)
    {
        var cartridges = weaponTemplate.Properties.Chambers[0]?.Props?.Filters[0]?.Filter;
        if (cartridges is null)
        {
            // Fallback to the magazine if possible, e.g. for revolvers
            //  Grab the magazines template
            var firstMagazine = weaponTemplate.Properties.Slots.FirstOrDefault((slot) => slot.Name == "mod_magazine");
            var magazineTemplate = _itemHelper.GetItem(firstMagazine.Props.Filters[0].Filter[0]);

            // Get the first slots array of cartridges
            cartridges = magazineTemplate.Value.Properties.Slots[0]?.Props.Filters[0].Filter;
            if (cartridges is null)
            {
                // Normal magazines
                // None found, try the cartridges array
                cartridges = magazineTemplate.Value.Properties.Cartridges[0]?.Props.Filters[0].Filter;
            }
        }

        return cartridges;
    }

    /// <summary>
    /// Get a weapons compatible cartridge caliber
    /// </summary>
    /// <param name="weaponTemplate">Weapon to look up caliber of</param>
    /// <returns>Caliber as string</returns>
    protected string? GetWeaponCaliber(TemplateItem weaponTemplate)
    {
        if (weaponTemplate.Properties.Caliber is not null)
        {
            return weaponTemplate.Properties.Caliber;
        }

        if (weaponTemplate.Properties.AmmoCaliber is not null)
        {
            // 9x18pmm has a typo, should be Caliber9x18PM
            return weaponTemplate.Properties.AmmoCaliber == "Caliber9x18PMM"
                ? "Caliber9x18PM"
                : weaponTemplate.Properties.AmmoCaliber;
        }

        if (weaponTemplate.Properties.LinkedWeapon is not null)
        {
            var ammoInChamber = _itemHelper.GetItem(
                weaponTemplate.Properties.Chambers[0].Props.Filters[0].Filter[0]
            );
            if (!ammoInChamber.Key)
            {
                return null;
            }

            return ammoInChamber.Value.Properties.Caliber;
        }

        return null;
    }

    /// <summary>
    /// Fill existing magazines to full, while replacing their contents with specified ammo
    /// </summary>
    /// <param name="weaponMods">Weapon with children</param>
    /// <param name="magazine">Magazine item</param>
    /// <param name="cartridgeTemplate">Cartridge to insert into magazine</param>
    protected void FillExistingMagazines(List<Item> weaponMods, Item magazine, string cartridgeTemplate)
    {
        var magazineTemplate = _itemHelper.GetItem(magazine.Template).Value;
        if (magazineTemplate is null)
        {
            _logger.Error(_localisationService.GetText("bot-unable_to_find_magazine_item", magazine.Template));

            return;
        }

        // Magazine, usually
        var parentItem = _itemHelper.GetItem(magazineTemplate.Parent).Value;

        // the revolver shotgun uses a magazine with chambers, not cartridges ("camora_xxx")
        // Exchange of the camora ammo is not necessary we could also just check for stackSize > 0 here
        // and remove the else
        if (_botWeaponGeneratorHelper.MagazineIsCylinderRelated(parentItem.Name))
        {
            FillCamorasWithAmmo(weaponMods, magazine.Id, cartridgeTemplate);
        }
        else
        {
            AddOrUpdateMagazinesChildWithAmmo(weaponMods, magazine, cartridgeTemplate, magazineTemplate);
        }
    }

    /// <summary>
    /// Add desired ammo template as item to weapon modifications list, placed as child to UBGL.
    /// </summary>
    /// <param name="weaponMods">Weapon with children.</param>
    /// <param name="ubglMod">UBGL item.</param>
    /// <param name="ubglAmmoTpl">Grenade ammo template.</param>
    protected void FillUbgl(List<Item> weaponMods, Item ubglMod, string ubglAmmoTpl)
    {
        weaponMods.Add(
            new()
            {
                Id = _hashUtil.Generate(),
                Template = ubglAmmoTpl,
                ParentId = ubglMod.Id,
                SlotId = "patron_in_weapon",
                Upd = new() { StackObjectsCount = 1 },
            }
        );
    }

    /// <summary>
    /// Add cartridge item to weapon item list, if it already exists, update
    /// </summary>
    /// <param name="weaponWithMods">Weapon items list to amend</param>
    /// <param name="magazine">Magazine item details we're adding cartridges to</param>
    /// <param name="chosenAmmoTpl">Cartridge to put into the magazine</param>
    /// <param name="newStackSize">How many cartridges should go into the magazine</param>
    /// <param name="magazineTemplate">Magazines db template</param>
    protected void AddOrUpdateMagazinesChildWithAmmo(List<Item> weaponWithMods, Item magazine, string chosenAmmoTpl, TemplateItem magazineTemplate)
    {
        var magazineCartridgeChildItem = weaponWithMods.FirstOrDefault(
            (m) => m.ParentId == magazine.Id && m.SlotId == "cartridges"
        );
        if (magazineCartridgeChildItem is not null)
        {
            // Delete the existing cartridge object and create fresh below
            weaponWithMods.Remove(magazineCartridgeChildItem);
        }

        // Create array with just magazine
        List<Item> magazineWithCartridges = new();
        magazineWithCartridges.AddRange(magazine);

        // Add full cartridge child items to above array
        _itemHelper.FillMagazineWithCartridge(magazineWithCartridges, magazineTemplate, chosenAmmoTpl, 1);

        // Replace existing magazine with above array of mag + cartridge stacks
        var index = weaponWithMods.FindIndex(i => i.Id == magazine.Id); // magazineWithCartridges
        weaponWithMods.RemoveAt(index);
        weaponWithMods.AddRange(magazineWithCartridges); // this might need to be at the specific index
    }

    /// <summary>
    /// Fill each Camora with a bullet
    /// </summary>
    /// <param name="weaponMods">Weapon mods to find and update camora mod(s) from</param>
    /// <param name="magazineId">Magazine id to find and add to</param>
    /// <param name="ammoTpl">Ammo template id to hydrate with</param>
    protected void FillCamorasWithAmmo(List<Item> weaponMods, string magazineId, string ammoTpl)
    {
        // for CylinderMagazine we exchange the ammo in the "camoras".
        // This might not be necessary since we already filled the camoras with a random whitelisted and compatible ammo type,
        // but I'm not sure whether this is also used elsewhere
        var camoras = weaponMods.Where((x) => x.ParentId == magazineId && x.SlotId.StartsWith("camora"));
        foreach (var camora in camoras)
        {
            camora.Template = ammoTpl;
            if (camora.Upd is not null)
            {
                camora.Upd.StackObjectsCount = 1;
            }
            else
            {
                camora.Upd = new() { StackObjectsCount = 1 };
            }
        }
    }
}
