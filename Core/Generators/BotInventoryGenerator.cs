using Core.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Equipment = Core.Models.Eft.Common.Tables.Equipment;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Generators;

[Injectable]
public class BotInventoryGenerator
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly RandomUtil _randomUtil;
    private readonly DatabaseService _databaseService;
    private readonly ApplicationContext _applicationContext;
    private readonly BotWeaponGenerator _botWeaponGenerator;
    private readonly BotLootGenerator _botLootGenerator;
    private readonly BotGeneratorHelper _botGeneratorHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly BotHelper _botHelper;
    private readonly WeightedRandomHelper _weightedRandomHelper;
    private readonly ItemHelper _itemHelper;
    private readonly WeatherHelper _weatherHelper;
    private readonly LocalisationService _localisationService;
    private readonly BotEquipmentFilterService _botEquipmentFilterService;
    private readonly BotEquipmentModPoolService _botEquipmentModPoolService;
    private readonly BotEquipmentModGenerator _botEquipmentModGenerator;
    private readonly ConfigServer _configServer;

    private BotConfig _botConfig;

    public BotInventoryGenerator(
        ILogger logger,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        DatabaseService databaseService,
        ApplicationContext applicationContext,
        BotWeaponGenerator botWeaponGenerator,
        BotLootGenerator botLootGenerator,
        BotGeneratorHelper botGeneratorHelper,
        ProfileHelper profileHelper,
        BotHelper botHelper,
        WeightedRandomHelper weightedRandomHelper,
        ItemHelper itemHelper,
        WeatherHelper weatherHelper,
        LocalisationService localisationService,
        BotEquipmentFilterService botEquipmentFilterService,
        BotEquipmentModPoolService botEquipmentModPoolService,
        BotEquipmentModGenerator botEquipmentModGenerator,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _databaseService = databaseService;
        _applicationContext = applicationContext;
        _botWeaponGenerator = botWeaponGenerator;
        _botLootGenerator = botLootGenerator;
        _botGeneratorHelper = botGeneratorHelper;
        _profileHelper = profileHelper;
        _botHelper = botHelper;
        _weightedRandomHelper = weightedRandomHelper;
        _itemHelper = itemHelper;
        _weatherHelper = weatherHelper;
        _localisationService = localisationService;
        _botEquipmentFilterService = botEquipmentFilterService;
        _botEquipmentModPoolService = botEquipmentModPoolService;
        _botEquipmentModGenerator = botEquipmentModGenerator;
        _configServer = configServer;

        _botConfig = _configServer.GetConfig<BotConfig>();
    }

    /// <summary>
    /// Add equipment/weapons/loot to bot
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="isPmc">Is bot being converted into a pmc</param>
    /// <param name="botLevel">Level of bot being generated</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <returns>PmcInventory object with equipment/weapons/loot</returns>
    public BotBaseInventory GenerateInventory(string sessionId, BotType botJsonTemplate, string botRole, bool isPmc, int botLevel, string chosenGameVersion)
    {
        var templateInventory = botJsonTemplate.BotInventory;
        var wornItemChances = botJsonTemplate.BotChances;
        var itemGenerationLimitsMinMax = botJsonTemplate.BotGeneration;

        // Generate base inventory with no items
        var botInventory = GenerateInventoryBase();

        // Get generated raid details bot will be spawned in
        var raidConfig = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();

        GenerateAndAddEquipmentToBot(
            sessionId,
            templateInventory,
            wornItemChances,
            botRole,
            botInventory,
            botLevel,
            chosenGameVersion,
            raidConfig);

        // Roll weapon spawns (primary/secondary/holster) and generate a weapon for each roll that passed
        GenerateAndAddWeaponsToBot(
            templateInventory,
            wornItemChances,
            sessionId,
            botInventory,
            botRole,
            isPmc,
            itemGenerationLimitsMinMax,
            botLevel);

        // Pick loot and add to bots containers (rig/backpack/pockets/secure)
        _botLootGenerator.GenerateLoot(sessionId, botJsonTemplate, isPmc, botRole, botInventory, botLevel);

        return botInventory;
    }

    /// <summary>
    /// Create a pmcInventory object with all the base/generic items needed
    /// </summary>
    /// <returns>PmcInventory object</returns>
    public BotBaseInventory GenerateInventoryBase()
    {
        var equipmentId = _hashUtil.Generate();
        var stashId = _hashUtil.Generate();
        var questRaidItemsId = _hashUtil.Generate();
        var questStashItemsId = _hashUtil.Generate();
        var sortingTableId = _hashUtil.Generate();

        return new BotBaseInventory
        {
            Items =
            [
                new() { Id = equipmentId, Template = ItemTpl.INVENTORY_DEFAULT },
                new() { Id = stashId, Template = ItemTpl.STASH_STANDARD_STASH_10X30 },
                new() { Id = questRaidItemsId, Template = ItemTpl.STASH_QUESTRAID },
                new() { Id = questStashItemsId, Template = ItemTpl.STASH_QUESTOFFLINE },
                new() { Id = sortingTableId, Template = ItemTpl.SORTINGTABLE_SORTING_TABLE }
            ],
            Equipment = equipmentId,
            Stash = stashId,
            QuestRaidItems = questRaidItemsId,
            QuestStashItems = questStashItemsId,
            SortingTable = sortingTableId,
            HideoutAreaStashes = { },
            FastPanel = { },
            FavoriteItems = [],
            HideoutCustomizationStashId = "",
        };
    }

    /// <summary>
    /// Add equipment to a bot
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="wornItemChances">Chances items will be added to bot</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="botInventory">Inventory to add equipment to</param>
    /// <param name="botLevel">Level of bot</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <param name="raidConfig">RadiConfig</param>
    public void GenerateAndAddEquipmentToBot(string sessionId, BotTypeInventory templateInventory, Chances wornItemChances, string botRole,
        BotBaseInventory botInventory, int botLevel, string chosenGameVersion, GetRaidConfigurationRequestData raidConfig)
    {
        // These will be handled later
        var excludedSlots = new List<EquipmentSlots>()
        {
            EquipmentSlots.Pockets,
            EquipmentSlots.FirstPrimaryWeapon,
            EquipmentSlots.SecondPrimaryWeapon,
            EquipmentSlots.Holster,
            EquipmentSlots.ArmorVest,
            EquipmentSlots.TacticalVest,
            EquipmentSlots.FaceCover,
            EquipmentSlots.Headwear,
            EquipmentSlots.Earpiece
        };

        _botConfig.Equipment.TryGetValue(_botGeneratorHelper.GetBotEquipmentRole(botRole), out var botEquipConfig);
        var randomistionDetails = _botHelper.GetBotRandomizationDetails(botLevel, botEquipConfig);

        // Apply nighttime changes if its nighttime + there's changes to make
        if (
            randomistionDetails?.NighttimeChanges is not null &&
            raidConfig is not null &&
            _weatherHelper.IsNightTime(raidConfig.TimeVariant)
        )
        {
            foreach (var equipmentSlotKvP in (randomistionDetails.NighttimeChanges.EquipmentModsModifiers))
            {
                // Never let mod chance go outside of 0 - 100
                randomistionDetails.EquipmentMods[equipmentSlotKvP.Key] = Math.Min(
                    Math.Max(randomistionDetails.EquipmentMods[equipmentSlotKvP.Key] + equipmentSlotKvP.Value, 0), 100);
            }
        }

        // Get profile of player generating bots, we use their level later on
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);
        var botEquipmentRole = _botGeneratorHelper.GetBotEquipmentRole(botRole);


        // Iterate over all equipment slots of bot, do it in specifc order to reduce conflicts
        // e.g. ArmorVest should be generated after TactivalVest
        // or FACE_COVER before HEADWEAR
        foreach (var equipmentSlotKvP in templateInventory.Equipment)
        {
            // Skip some slots as they need to be done in a specific order + with specific parameter values
            // e.g. Weapons
            if (excludedSlots.Contains(equipmentSlotKvP.Key))
            {
                continue;
            }

            GenerateEquipment(new GenerateEquipmentProperties
            {
                RootEquipmentSlot = equipmentSlotKvP.Key,
                RootEquipmentPool = equipmentSlotKvP.Value,
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile.Info.Level,
            });
        }

        // Generate below in specific order
        GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.Pockets,
            // Unheard profiles have unique sized pockets, TODO - handle this somewhere else in a better way
            RootEquipmentPool =
                chosenGameVersion == GameEditions.UNHEARD
                    ? new Dictionary<string, double> { [ItemTpl.POCKETS_1X4_TUE] = 1 }
                    : templateInventory.Equipment[EquipmentSlots.Pockets],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GenerateModsBlacklist = [ItemTpl.POCKETS_1X4_TUE],
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });

        GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.FaceCover,
            RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.FaceCover],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });

        GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.Headwear,
            RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Headwear],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });

        GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.Earpiece,
            RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Earpiece],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });

        var hasArmorVest = GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.ArmorVest,
            RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.ArmorVest],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });

        // Bot has no armor vest and flagged to be forced to wear armored rig in this event
        if (botEquipConfig.ForceOnlyArmoredRigWhenNoArmor.GetValueOrDefault(false) && !hasArmorVest)
        {
            // Filter rigs down to only those with armor
            FilterRigsToThoseWithProtection(templateInventory.Equipment, botRole);
        }

        // Optimisation - Remove armored rigs from pool
        if (hasArmorVest)
        {
            // Filter rigs down to only those with armor
            FilterRigsToThoseWithoutProtection(templateInventory.Equipment, botRole);
        }

        // Bot is flagged as always needing a vest
        if (botEquipConfig.ForceRigWhenNoVest.GetValueOrDefault(false) && !hasArmorVest)
        {
            wornItemChances.EquipmentChances["TacticalVest"] = 100;
        }

        GenerateEquipment(new GenerateEquipmentProperties
        {
            RootEquipmentSlot = EquipmentSlots.Earpiece,
            RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Earpiece],
            ModPool = templateInventory.Mods,
            SpawnChances = wornItemChances,
            BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
            Inventory = botInventory,
            BotEquipmentConfig = botEquipConfig,
            RandomisationDetails = randomistionDetails,
            GeneratingPlayerLevel = pmcProfile.Info.Level,
        });
    }

    /// <summary>
    /// Remove non-armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equpiment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    public void FilterRigsToThoseWithProtection(Dictionary<EquipmentSlots, Dictionary<string, double>> templateEquipment, string botRole)
    {
        var tacVestsWithArmor = templateEquipment[EquipmentSlots.TacticalVest].Where(kvp => _itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (tacVestsWithArmor.Count() == 0)
        {
            _logger.Debug($"Unable to filter to only armored rigs as bot: {botRole} has none in pool");

            return;
        }

        templateEquipment[EquipmentSlots.TacticalVest] = tacVestsWithArmor;
    }

    /// <summary>
    /// Remove armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equpiment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    /// <param name="allowEmptyRequest">Should the function return all rigs when 0 unarmored are found</param>
    public void FilterRigsToThoseWithoutProtection(Dictionary<EquipmentSlots, Dictionary<string, double>> templateEquipment, string botRole,
        bool allowEmptyResult = true)
    {
        var tacVestsWithoutArmor = templateEquipment[EquipmentSlots.TacticalVest].Where(kvp => !_itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!allowEmptyResult && tacVestsWithoutArmor.Count() == 0)
        {
            _logger.Debug($"Unable to filter to only unarmored rigs as bot: {botRole} has none in pool");

            return;
        }

        templateEquipment[EquipmentSlots.TacticalVest] = tacVestsWithoutArmor;
    }

    /// <summary>
    /// Add a piece of equipment with mods to inventory from the provided pools
    /// </summary>
    /// <param name="settings">Values to adjust how item is chosen and added to bot</param>
    /// <returns>true when item added</returns>
    public bool GenerateEquipment(GenerateEquipmentProperties settings)
    {
        _logger.Error("NOT IMPLEMENTED - GenerateEquipment");
        List<string> slotsToCheck = [EquipmentSlots.Pockets.ToString(), EquipmentSlots.SecuredContainer.ToString()];
        double? spawnChance = slotsToCheck.Contains(settings.RootEquipmentSlot.ToString())
            ? 100
            : settings.SpawnChances.EquipmentChances[settings.RootEquipmentSlot.ToString()];

        if (spawnChance is null)
        {
            _logger.Warning(_localisationService.GetText("bot-no_spawn_chance_defined_for_equipment_slot",
                settings.RootEquipmentSlot));

            return false;
        }

        // Roll dice on equipment item
        var shouldSpawn = _randomUtil.GetChance100(spawnChance ?? 0);
        if (shouldSpawn && settings.RootEquipmentPool.Count() == 0)
        {
            TemplateItem pickedItemDb = new TemplateItem();
            var found = false;

            // Limit attempts to find a compatible item as its expensive to check them all
            var maxAttempts = Math.Round(settings.RootEquipmentPool.Count() * 0.75); // Roughly 75% of pool size
            var attempts = 0;
            while (!found)
            {
                if (settings.RootEquipmentPool.Count() == 0)
                {
                    return false;
                }

                var chosenItemTpl = _weightedRandomHelper.GetWeightedValue<string>(settings.RootEquipmentPool);
                var dbResult = _itemHelper.GetItem(chosenItemTpl);

                if (!dbResult.Key)
                {
                    _logger.Error(_localisationService.GetText("bot-missing_item_template", chosenItemTpl));
                    _logger.Debug($"EquipmentSlot-> {settings.RootEquipmentSlot}");

                    // Remove picked item
                    settings.RootEquipmentPool.Remove(chosenItemTpl);

                    attempts++;

                    continue;
                }

                // Is the chosen item compatible with other items equipped
                var compatibilityResult = _botGeneratorHelper.IsItemIncompatibleWithCurrentItems(
                    settings.Inventory.Items,
                    chosenItemTpl,
                    settings.RootEquipmentSlot.ToString()
                );
                if (compatibilityResult.Incompatible ?? false)
                {
                    // Tried x different items that failed, stop
                    if (attempts > maxAttempts)
                    {
                        return false;
                    }

                    // Remove picked item from pool
                    settings.RootEquipmentPool.Remove(chosenItemTpl);

                    // Increment times tried
                    attempts++;
                }
                else
                {
                    // Success
                    found = true;
                    pickedItemDb = dbResult.Value;
                }
            }

            // Create root item
            var id = _hashUtil.Generate();
            Item item = new()
            {
                Id = id,
                Template = pickedItemDb.Id,
                ParentId = settings.Inventory.Equipment,
                SlotId = settings.RootEquipmentSlot.ToString(),
                Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(pickedItemDb, settings.BotData.Role)
            };

            var botEquipBlacklist = _botEquipmentFilterService.GetBotEquipmentBlacklist(
                settings.BotData.EquipmentRole,
                (double)settings.GeneratingPlayerLevel
            );

            // Edge case: Filter the armor items mod pool if bot exists in config dict + config has armor slot
            if ((_botConfig.Equipment[settings.BotData.EquipmentRole] is not null) &&
                (settings.RandomisationDetails.RandomisedArmorSlots.Contains(settings.RootEquipmentSlot.ToString())))
            {
                // Filter out mods from relevant blacklist
                settings.ModPool[pickedItemDb.Id] = GetFilteredDynamicModsForItem(
                    pickedItemDb.Id,
                    botEquipBlacklist.Equipment
                );
            }

            // Does item have slots for sub-mods to be inserted into
            if (pickedItemDb.Properties.Slots?.Count() > 0 && (settings.GenerateModsBlacklist.Contains(pickedItemDb.Id)))
            {
                var childItemsToAdd = _botEquipmentModGenerator.GenerateModsForEquipment(
                    [item],
                    id,
                    pickedItemDb,
                    settings,
                    botEquipBlacklist
                );
                settings.Inventory.Items.AddRange(childItemsToAdd);
            }
            else
            {
                // No slots, add root item only
                settings.Inventory.Items.Add(item);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Get all possible mods for item and filter down based on equipment blacklist from bot.json config
    /// </summary>
    /// <param name="itemTpl">Item mod pool is being retrieved and filtered</param>
    /// <param name="equipmentBlacklist">Blacklist to filter mod pool with</param>
    /// <returns>Filtered pool of mods</returns>
    public Dictionary<string, List<string>> GetFilteredDynamicModsForItem(string itemTpl, Dictionary<string, List<string>> equipmentBlacklist)
    {
        var modPool = _botEquipmentModPoolService.GetModsForGearSlot(itemTpl);
        foreach (var modSlot in modPool.Keys ?? Enumerable.Empty<string>())
        {
            var blacklistedMods = equipmentBlacklist[modSlot] ?? [];
            var filteredMods = modPool[modSlot].Where((slotName) => !blacklistedMods.Contains(slotName));

            if (filteredMods.Count() > 0)
            {
                modPool[modSlot] = filteredMods.ToList();
            }
        }

        return modPool;
    }

    /// <summary>
    /// Work out what weapons bot should have equipped and add them to bot inventory
    /// </summary>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="botInventory">Inventory to add weapons to</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationLimitsMinMax">Limits for items the bot can have</param>
    /// <param name="botLevel">level of bot having weapon generated</param>
    public void GenerateAndAddWeaponsToBot(BotTypeInventory templateInventory, Chances equipmentChances, string sessionId, BotBaseInventory botInventory,
        string botRole, bool isPmc, Generation itemGenerationLimitsMinMax, int botLevel)
    {
        var weaponSlotsToFill = GetDesiredWeaponsForBot(equipmentChances);
        foreach (var weaponSlot in weaponSlotsToFill)
        {
            // Add weapon to bot if true and bot json has something to put into the slot
            if (weaponSlot.ShouldSpawn && templateInventory.Equipment[weaponSlot.Slot].Any())
            {
                AddWeaponAndMagazinesToInventory(
                    sessionId,
                    weaponSlot,
                    templateInventory,
                    botInventory,
                    equipmentChances,
                    botRole,
                    isPmc,
                    itemGenerationLimitsMinMax,
                    botLevel
                );
            }
        }
    }

    /// <summary>
    /// Calculate if the bot should have weapons in Primary/Secondary/Holster slots
    /// </summary>
    /// <param name="equipmentChances">Chances bot has certain equipment</param>
    /// <returns>What slots bot should have weapons generated for</returns>
    public List<DesiredWeapons> GetDesiredWeaponsForBot(Chances equipmentChances) // TODO: Type fuckery { slot: EquipmentSlots; shouldSpawn: boolean }[]
    {
        var shouldSpawnPrimary = _randomUtil.GetChance100(equipmentChances.EquipmentChances["FirstPrimaryWeapon"]);
        return
        [
            new()
            {
                Slot = EquipmentSlots.FirstPrimaryWeapon, ShouldSpawn = shouldSpawnPrimary
            },
            new()
            {
                Slot = EquipmentSlots.SecondPrimaryWeapon,
                ShouldSpawn = shouldSpawnPrimary
                    ? _randomUtil.GetChance100(equipmentChances.EquipmentChances["SecondPrimaryWeapon"])
                    : false
            },
            new()
            {
                Slot = EquipmentSlots.Holster,
                ShouldSpawn = shouldSpawnPrimary
                    ? _randomUtil.GetChance100(equipmentChances.EquipmentChances["Holster"]) // Primary weapon = roll for chance at pistol
                    : true // No primary = force pistol
            }
        ];
    }

    /// <summary>
    /// Add weapon + spare mags/ammo to bots inventory
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="weaponSlot">Weapon slot being generated</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="botInventory">Inventory to add weapon+mags/ammo to</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationWeights"></param>
    /// <param name="botLevel"></param>
    public void AddWeaponAndMagazinesToInventory(string sessionId, DesiredWeapons weaponSlot, BotTypeInventory templateInventory, BotBaseInventory botInventory,
        Chances equipmentChances, string botRole,
        bool isPmc, Generation itemGenerationWeights, int botLevel)
    {
        var generatedweapon = _botWeaponGenerator.GenerateRandomWeapon(
            sessionId,
            weaponSlot.Slot.ToString(),
            templateInventory,
            botInventory.Equipment,
            equipmentChances.WeaponModsChances,
            botRole,
            isPmc,
            botLevel
        );
        
        botInventory.Items.AddRange(generatedweapon.Weapon);
        
        _botWeaponGenerator.AddExtraMagazinesToInventory(
            generatedweapon,
            itemGenerationWeights.Items.Magazines,
            botInventory,
            botRole);
    }
}

public class DesiredWeapons
{
    public EquipmentSlots Slot { get; set; }

    public bool ShouldSpawn { get; set; }
}
