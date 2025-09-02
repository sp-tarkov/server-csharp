using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotInventoryGenerator(
    ISptLogger<BotInventoryGenerator> logger,
    RandomUtil randomUtil,
    ProfileActivityService profileActivityService,
    BotWeaponGenerator botWeaponGenerator,
    BotLootGenerator botLootGenerator,
    BotGeneratorHelper botGeneratorHelper,
    ProfileHelper profileHelper,
    BotHelper botHelper,
    WeightedRandomHelper weightedRandomHelper,
    ItemHelper itemHelper,
    WeatherHelper weatherHelper,
    ServerLocalisationService serverLocalisationService,
    BotEquipmentFilterService botEquipmentFilterService,
    BotEquipmentModPoolService botEquipmentModPoolService,
    BotEquipmentModGenerator botEquipmentModGenerator,
    BotInventoryContainerService botInventoryContainerService,
    ConfigServer configServer
)
{
    // Slots handled individually inside `GenerateAndAddEquipmentToBot`
    private static readonly FrozenSet<EquipmentSlots> _equipmentSlotsWithInventory =
    [
        EquipmentSlots.Pockets,
        EquipmentSlots.TacticalVest,
        EquipmentSlots.Backpack,
        EquipmentSlots.SecuredContainer,
    ];

    // Slots handled individually inside `GenerateAndAddEquipmentToBot`
    private static readonly FrozenSet<EquipmentSlots> _excludedEquipmentSlots =
    [
        EquipmentSlots.Pockets,
        EquipmentSlots.FirstPrimaryWeapon,
        EquipmentSlots.SecondPrimaryWeapon,
        EquipmentSlots.Holster,
        EquipmentSlots.ArmorVest,
        EquipmentSlots.TacticalVest,
        EquipmentSlots.FaceCover,
        EquipmentSlots.Headwear,
        EquipmentSlots.Earpiece,
    ];

    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();

    private readonly FrozenSet<string> _slotsToCheck = [nameof(EquipmentSlots.Pockets), nameof(EquipmentSlots.SecuredContainer)];

    /// <summary>
    ///     Add equipment/weapons/loot to bot
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="botGenerationDetails">Details related to generating a bot</param>
    /// <param name="botLevel">Level of bot being generated</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <returns>PmcInventory object with equipment/weapons/loot</returns>
    public BotBaseInventory GenerateInventory(
        MongoId botId,
        MongoId sessionId,
        BotType botJsonTemplate,
        string botRole,
        BotGenerationDetails botGenerationDetails,
        int botLevel,
        string chosenGameVersion
    )
    {
        var templateInventory = botJsonTemplate.BotInventory;
        var wornItemChances = botJsonTemplate.BotChances;
        var itemGenerationLimitsMinMax = botJsonTemplate.BotGeneration;

        var isPmc = botGenerationDetails.IsPmc;

        // Generate base inventory with no items
        var botInventory = GenerateInventoryBase();

        // Get generated raid details bot will be spawned in
        var raidConfig = profileActivityService.GetProfileActivityRaidData(sessionId)?.RaidConfiguration;

        GenerateAndAddEquipmentToBot(
            botId,
            sessionId,
            templateInventory,
            wornItemChances,
            botRole,
            botInventory,
            botLevel,
            chosenGameVersion,
            isPmc,
            raidConfig
        );

        // Roll weapon spawns (primary/secondary/holster) and generate a weapon for each roll that passed
        GenerateAndAddWeaponsToBot(
            botId,
            templateInventory,
            wornItemChances,
            sessionId,
            botInventory,
            botRole,
            isPmc,
            itemGenerationLimitsMinMax,
            botLevel
        );

        // Pick loot and add to bots containers (rig/backpack/pockets/secure)
        botLootGenerator.GenerateLoot(botId, sessionId, botJsonTemplate, botGenerationDetails, isPmc, botRole, botInventory, botLevel);

        // Inventory cache isn't needed, clear to save memory
        if (botGenerationDetails.ClearBotContainerCacheAfterGeneration)
        {
            botInventoryContainerService.ClearCache(botId);
        }

        return botInventory;
    }

    /// <summary>
    ///     Create a pmcInventory object with all the base/generic items needed
    /// </summary>
    /// <returns>PmcInventory object</returns>
    public BotBaseInventory GenerateInventoryBase()
    {
        var equipmentId = new MongoId();
        var stashId = new MongoId();
        var questRaidItemsId = new MongoId();
        var questStashItemsId = new MongoId();
        var sortingTableId = new MongoId();
        var hideoutCustomizationStashId = new MongoId();

        return new BotBaseInventory
        {
            Items =
            [
                new Item { Id = equipmentId, Template = ItemTpl.INVENTORY_DEFAULT },
                new Item { Id = stashId, Template = ItemTpl.STASH_STANDARD_STASH_10X30 },
                new Item { Id = questRaidItemsId, Template = ItemTpl.STASH_QUESTRAID },
                new Item { Id = questStashItemsId, Template = ItemTpl.STASH_QUESTOFFLINE },
                new Item { Id = sortingTableId, Template = ItemTpl.SORTINGTABLE_SORTING_TABLE },
                new Item { Id = hideoutCustomizationStashId, Template = ItemTpl.HIDEOUTAREACONTAINER_CUSTOMIZATION },
            ],
            Equipment = equipmentId,
            Stash = stashId,
            QuestRaidItems = questRaidItemsId,
            QuestStashItems = questStashItemsId,
            SortingTable = sortingTableId,
            HideoutAreaStashes = new Dictionary<string, MongoId>(),
            FastPanel = new Dictionary<string, MongoId>(),
            FavoriteItems = [],
            HideoutCustomizationStashId = hideoutCustomizationStashId,
        };
    }

    /// <summary>
    ///     Add equipment to a bot
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="wornItemChances">Chances items will be added to bot</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="botInventory">Inventory to add equipment to</param>
    /// <param name="botLevel">Level of bot</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <param name="isPmc">Is the generated bot a PMC</param>
    /// <param name="raidConfig">RadiConfig</param>
    public void GenerateAndAddEquipmentToBot(
        MongoId botId,
        MongoId sessionId,
        BotTypeInventory templateInventory,
        Chances wornItemChances,
        string botRole,
        BotBaseInventory botInventory,
        int botLevel,
        string chosenGameVersion,
        bool isPmc,
        GetRaidConfigurationRequestData? raidConfig
    )
    {
        if (!BotConfig.Equipment.TryGetValue(botGeneratorHelper.GetBotEquipmentRole(botRole), out var botEquipConfig))
        {
            logger.Error($"Bot Equipment generation failed, unable to find equipment filters for: {botRole}");

            return;
        }
        var randomistionDetails = botHelper.GetBotRandomizationDetails(botLevel, botEquipConfig);

        // Apply nighttime changes if its nighttime + there's changes to make
        if (
            randomistionDetails?.NighttimeChanges is not null
            && raidConfig is not null
            && weatherHelper.IsNightTime(raidConfig.TimeVariant, raidConfig.Location)
        )
        {
            foreach (var (equipment, weight) in randomistionDetails.NighttimeChanges.EquipmentModsModifiers)
            {
                if (randomistionDetails.EquipmentMods.TryGetValue(equipment, out var value))
                {
                    // Never let mod chance go outside 0 - 100
                    var newWeight = weight + value;
                    randomistionDetails.EquipmentMods[equipment] = Math.Clamp(newWeight, 0, 100);
                }
            }
        }

        // Is PMC + generating armband + armband forcing is enabled
        if (PMCConfig.ForceArmband.Enabled && isPmc)
        {
            // Replace armband pool with single tpl from config
            if (templateInventory.Equipment.TryGetValue(EquipmentSlots.ArmBand, out var armbands))
            {
                // Get tpl based on pmc side
                var armbandTpl = botRole == "pmcusec" ? PMCConfig.ForceArmband.Usec : PMCConfig.ForceArmband.Bear;

                armbands.Clear();
                armbands.Add(armbandTpl, 1);

                // Force armband spawn to 100%
                wornItemChances.EquipmentChances["Armband"] = 100;
            }
        }

        // Get profile of player generating bots, we use their level later on
        var pmcProfile = profileHelper.GetPmcProfile(sessionId);
        var botEquipmentRole = botGeneratorHelper.GetBotEquipmentRole(botRole);

        // Iterate over all equipment slots of bot, do it in specific order to reduce conflicts
        // e.g. ArmorVest should be generated after TacticalVest
        // or FACE_COVER before HEADWEAR
        foreach (var (equipmentSlot, itemsWithWeightPool) in templateInventory.Equipment)
        {
            // Skip some slots as they need to be done in a specific order + with specific parameter values
            // e.g. Weapons
            if (_excludedEquipmentSlots.Contains(equipmentSlot))
            {
                continue;
            }

            GenerateEquipment(
                new GenerateEquipmentProperties
                {
                    BotId = botId,
                    RootEquipmentSlot = equipmentSlot,
                    RootEquipmentPool = itemsWithWeightPool,
                    ModPool = templateInventory.Mods,
                    SpawnChances = wornItemChances,
                    BotData = new BotData
                    {
                        Role = botRole,
                        Level = botLevel,
                        EquipmentRole = botEquipmentRole,
                    },
                    Inventory = botInventory,
                    BotEquipmentConfig = botEquipConfig,
                    RandomisationDetails = randomistionDetails,
                    GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
                }
            );
        }

        // Generate below in specific order
        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.Pockets,
                // Unheard profiles have unique sized pockets
                RootEquipmentPool = GetPocketPoolByGameEdition(chosenGameVersion, templateInventory, isPmc),
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GenerateModsBlacklist = [ItemTpl.POCKETS_1X4_TUE, ItemTpl.POCKETS_LARGE],
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.FaceCover,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.FaceCover],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.Headwear,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Headwear],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.Earpiece,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Earpiece],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );

        var hasArmorVest = GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.ArmorVest,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.ArmorVest],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );

        // Bot has no armor vest and flagged to be forced to wear armored rig in this event
        if (botEquipConfig.ForceOnlyArmoredRigWhenNoArmor.GetValueOrDefault(false) && !hasArmorVest)
        // Filter rigs down to only those with armor
        {
            FilterRigsToThoseWithProtection(templateInventory.Equipment, botRole);
        }

        // Optimisation - Remove armored rigs from pool
        if (hasArmorVest)
        // Filter rigs down to only those with armor
        {
            FilterRigsToThoseWithoutProtection(templateInventory.Equipment, botRole);
        }

        // Bot is flagged as always needing a vest
        if (botEquipConfig.ForceRigWhenNoVest.GetValueOrDefault(false) && !hasArmorVest)
        {
            wornItemChances.EquipmentChances["TacticalVest"] = 100;
        }

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                BotId = botId,
                RootEquipmentSlot = EquipmentSlots.TacticalVest,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.TacticalVest],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData
                {
                    Role = botRole,
                    Level = botLevel,
                    EquipmentRole = botEquipmentRole,
                },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1,
            }
        );
    }

    /// <summary>
    ///     Get RootEquipmentPool id based on game version
    /// </summary>
    /// <param name="chosenGameVersion"></param>
    /// <param name="templateInventory"></param>
    /// <param name="isPmc">is bot a PMC</param>
    /// <returns></returns>
    protected Dictionary<MongoId, double>? GetPocketPoolByGameEdition(
        string chosenGameVersion,
        BotTypeInventory templateInventory,
        bool isPmc
    )
    {
        return chosenGameVersion == GameEditions.UNHEARD && isPmc
            ? new Dictionary<MongoId, double> { [ItemTpl.POCKETS_1X4_TUE] = 1 }
            : templateInventory.Equipment.GetValueOrDefault(EquipmentSlots.Pockets);
    }

    /// <summary>
    ///     Remove non-armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equipment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    public void FilterRigsToThoseWithProtection(Dictionary<EquipmentSlots, Dictionary<MongoId, double>> templateEquipment, string botRole)
    {
        var tacVestsWithArmor = templateEquipment[EquipmentSlots.TacticalVest]
            .Where(kvp => itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!tacVestsWithArmor.Any())
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Unable to filter to only armored rigs as bot: {botRole} has none in pool");
            }

            return;
        }

        templateEquipment[EquipmentSlots.TacticalVest] = tacVestsWithArmor;
    }

    /// <summary>
    ///     Remove armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equipment to filter TacticalVest by</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    /// <param name="allowEmptyResult">Should the function return all rigs when 0 unarmored are found</param>
    public void FilterRigsToThoseWithoutProtection(
        Dictionary<EquipmentSlots, Dictionary<MongoId, double>> templateEquipment,
        string botRole,
        bool allowEmptyResult = true
    )
    {
        var tacVestsWithoutArmor = templateEquipment[EquipmentSlots.TacticalVest]
            .Where(kvp => !itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!allowEmptyResult && !tacVestsWithoutArmor.Any())
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Unable to filter to only unarmored rigs as bot: {botRole} has none in pool");
            }

            return;
        }

        templateEquipment[EquipmentSlots.TacticalVest] = tacVestsWithoutArmor;
    }

    /// <summary>
    ///     Add a piece of equipment with mods to inventory from the provided pools
    /// </summary>
    /// <param name="settings">Values to adjust how item is chosen and added to bot</param>
    /// <returns>true when item added</returns>
    public bool GenerateEquipment(GenerateEquipmentProperties settings)
    {
        double? spawnChance = _slotsToCheck.Contains(settings.RootEquipmentSlot.ToString())
            ? 100
            : settings.SpawnChances.EquipmentChances.GetValueOrDefault(settings.RootEquipmentSlot.ToString());

        if (!spawnChance.HasValue)
        {
            logger.Warning(serverLocalisationService.GetText("bot-no_spawn_chance_defined_for_equipment_slot", settings.RootEquipmentSlot));

            return false;
        }

        // Roll dice on equipment item
        var shouldSpawn = randomUtil.GetChance100(spawnChance.Value);
        if (shouldSpawn && settings.RootEquipmentPool.Any())
        {
            TemplateItem? pickedItemDb = null;
            var found = false;

            // Limit attempts to find a compatible item as it's expensive to check them all
            var maxAttempts = Math.Round(settings.RootEquipmentPool.Count * 0.75); // Roughly 75% of pool size
            var attempts = 0;
            while (!found)
            {
                if (!settings.RootEquipmentPool.Any())
                {
                    return false;
                }

                var chosenItemTpl = weightedRandomHelper.GetWeightedValue(settings.RootEquipmentPool);
                var dbResult = itemHelper.GetItem(chosenItemTpl);

                if (!dbResult.Key)
                {
                    logger.Error(serverLocalisationService.GetText("bot-missing_item_template", chosenItemTpl));
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"EquipmentSlot-> {settings.RootEquipmentSlot}");
                    }

                    // Remove picked item
                    settings.RootEquipmentPool.Remove(chosenItemTpl);

                    attempts++;

                    continue;
                }

                // Is the chosen item compatible with other items equipped
                var compatibilityResult = botGeneratorHelper.IsItemIncompatibleWithCurrentItems(
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
            var id = new MongoId();
            Item item = new()
            {
                Id = id,
                Template = pickedItemDb.Id,
                ParentId = settings.Inventory.Equipment,
                SlotId = settings.RootEquipmentSlot.ToString(),
                Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(pickedItemDb, settings.BotData.Role),
            };

            var botEquipBlacklist = botEquipmentFilterService.GetBotEquipmentBlacklist(
                settings.BotData.EquipmentRole,
                settings.GeneratingPlayerLevel.GetValueOrDefault(1)
            );

            // Edge case: Filter the armor items mod pool if bot exists in config dict + config has armor slot
            if (
                BotConfig.Equipment.ContainsKey(settings.BotData.EquipmentRole)
                && settings.RandomisationDetails?.RandomisedArmorSlots != null
                && settings.RandomisationDetails.RandomisedArmorSlots.Contains(settings.RootEquipmentSlot.ToString())
            )
            // Filter out mods from relevant blacklist
            {
                settings.ModPool[pickedItemDb.Id] = GetFilteredDynamicModsForItem(pickedItemDb.Id, botEquipBlacklist.Equipment);
            }

            var itemIsOnGenerateModBlacklist =
                settings.GenerateModsBlacklist != null && settings.GenerateModsBlacklist.Contains(pickedItemDb.Id);
            // Does item have slots for sub-mods to be inserted into
            if (pickedItemDb.Properties?.Slots is not null && pickedItemDb.Properties.Slots.Any() && !itemIsOnGenerateModBlacklist)
            {
                var childItemsToAdd = botEquipmentModGenerator.GenerateModsForEquipment(
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

            // Cache container ready for items to be added in
            if (_equipmentSlotsWithInventory.Contains(settings.RootEquipmentSlot))
            {
                botInventoryContainerService.AddEmptyContainerToBot(settings.BotId, settings.RootEquipmentSlot, item);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Get all possible mods for item and filter down based on equipment blacklist from bot.json config
    /// </summary>
    /// <param name="itemTpl">Item mod pool is being retrieved and filtered</param>
    /// <param name="equipmentBlacklist">Blacklist to filter mod pool with</param>
    /// <returns>Filtered pool of mods</returns>
    public Dictionary<string, HashSet<MongoId>> GetFilteredDynamicModsForItem(
        MongoId itemTpl,
        Dictionary<string, HashSet<MongoId>> equipmentBlacklist
    )
    {
        var modPool = botEquipmentModPoolService.GetModsForGearSlot(itemTpl);

        return modPool.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var (modSlot, modsForSlot) = kvp;

                if (!equipmentBlacklist.TryGetValue(modSlot, out var blacklistedMods))
                {
                    // No blacklist for slot, return all mods
                    return modsForSlot;
                }

                var filteredMods = modsForSlot.Where(mod => !blacklistedMods.Contains(mod)).ToHashSet();
                if (filteredMods.Any())
                {
                    // There's at least one tpl remaining, send it
                    return filteredMods;
                }

                logger.Warning($"Filtering: '{modSlot}' resulted in 0 mods. Reverting to original set for slot");

                // Return original
                return modsForSlot;
            }
        );
    }

    /// <summary>
    ///     Work out what weapons bot should have equipped and add them to bot inventory
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="botInventory">Inventory to add weapons to</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationLimitsMinMax">Limits for items the bot can have</param>
    /// <param name="botLevel">level of bot having weapon generated</param>
    public void GenerateAndAddWeaponsToBot(
        MongoId botId,
        BotTypeInventory templateInventory,
        Chances equipmentChances,
        MongoId sessionId,
        BotBaseInventory botInventory,
        string botRole,
        bool isPmc,
        Generation itemGenerationLimitsMinMax,
        int botLevel
    )
    {
        var weaponSlotsToFill = GetDesiredWeaponsForBot(equipmentChances);
        foreach (var desiredWeapons in weaponSlotsToFill)
        // Add weapon to bot if true and bot json has something to put into the slot
        {
            if (desiredWeapons.ShouldSpawn && templateInventory.Equipment[desiredWeapons.Slot].Any())
            {
                AddWeaponAndMagazinesToInventory(
                    botId,
                    sessionId,
                    desiredWeapons,
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
    ///     Calculate if the bot should have weapons in Primary/Secondary/Holster slots
    /// </summary>
    /// <param name="equipmentChances">Chances bot has certain equipment</param>
    /// <returns>What slots bot should have weapons generated for</returns>
    public IEnumerable<DesiredWeapons> GetDesiredWeaponsForBot(Chances equipmentChances)
    {
        var shouldSpawnPrimary = randomUtil.GetChance100(equipmentChances.EquipmentChances["FirstPrimaryWeapon"]);
        return
        [
            new DesiredWeapons { Slot = EquipmentSlots.FirstPrimaryWeapon, ShouldSpawn = shouldSpawnPrimary },
            new DesiredWeapons
            {
                Slot = EquipmentSlots.SecondPrimaryWeapon,
                ShouldSpawn = shouldSpawnPrimary && randomUtil.GetChance100(equipmentChances.EquipmentChances["SecondPrimaryWeapon"]),
            },
            new DesiredWeapons
            {
                Slot = EquipmentSlots.Holster,
                ShouldSpawn = !shouldSpawnPrimary || randomUtil.GetChance100(equipmentChances.EquipmentChances["Holster"]), // No primary = force pistol
            },
        ];
    }

    /// <summary>
    ///     Add weapon + spare mags/ammo to bots inventory
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="weaponSlot">Weapon slot being generated</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="botInventory">Inventory to add weapon+mags/ammo to</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationWeights"></param>
    /// <param name="botLevel"></param>
    public void AddWeaponAndMagazinesToInventory(
        MongoId botId,
        MongoId sessionId,
        DesiredWeapons weaponSlot,
        BotTypeInventory templateInventory,
        BotBaseInventory botInventory,
        Chances equipmentChances,
        string botRole,
        bool isPmc,
        Generation itemGenerationWeights,
        int botLevel
    )
    {
        var generatedWeapon = botWeaponGenerator.GenerateRandomWeapon(
            sessionId,
            weaponSlot.Slot.ToString(),
            templateInventory,
            botInventory.Equipment.Value,
            equipmentChances.WeaponModsChances,
            botRole,
            isPmc,
            botLevel
        );

        botInventory.Items.AddRange(generatedWeapon.Weapon);

        botWeaponGenerator.AddExtraMagazinesToInventory(
            botId,
            generatedWeapon,
            itemGenerationWeights.Items.Magazines,
            botInventory,
            botRole
        );
    }
}

public class DesiredWeapons
{
    public EquipmentSlots Slot { get; set; }

    public bool ShouldSpawn { get; set; }
}
