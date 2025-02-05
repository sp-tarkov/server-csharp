using SptCommon.Annotations;
using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using LogLevel = Core.Models.Spt.Logging.LogLevel;


namespace Core.Generators;

[Injectable]
public class BotInventoryGenerator(
    ISptLogger<BotInventoryGenerator> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    DatabaseService _databaseService,
    ApplicationContext _applicationContext,
    BotWeaponGenerator _botWeaponGenerator,
    BotLootGenerator _botLootGenerator,
    BotGeneratorHelper _botGeneratorHelper,
    ProfileHelper _profileHelper,
    BotHelper _botHelper,
    WeightedRandomHelper _weightedRandomHelper,
    ItemHelper _itemHelper,
    WeatherHelper _weatherHelper,
    LocalisationService _localisationService,
    BotEquipmentFilterService _botEquipmentFilterService,
    BotEquipmentModPoolService _botEquipmentModPoolService,
    BotEquipmentModGenerator _botEquipmentModGenerator,
    ConfigServer _configServer
)
{
    private BotConfig _botConfig = _configServer.GetConfig<BotConfig>();

    // Slots handled individually inside `GenerateAndAddEquipmentToBot`
    private List<EquipmentSlots> _excludedEquipmentSlots =
    [
        EquipmentSlots.Pockets,
        EquipmentSlots.FirstPrimaryWeapon,
        EquipmentSlots.SecondPrimaryWeapon,
        EquipmentSlots.Holster,
        EquipmentSlots.ArmorVest,
        EquipmentSlots.TacticalVest,
        EquipmentSlots.FaceCover,
        EquipmentSlots.Headwear,
        EquipmentSlots.Earpiece
    ];

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
            raidConfig
        );

        // Roll weapon spawns (primary/secondary/holster) and generate a weapon for each roll that passed
        GenerateAndAddWeaponsToBot(
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
        var hideoutCustomizationStashId = _hashUtil.Generate();

        return new BotBaseInventory
        {
            Items =
            [
                new Item { Id = equipmentId, Template = ItemTpl.INVENTORY_DEFAULT },
                new Item { Id = stashId, Template = ItemTpl.STASH_STANDARD_STASH_10X30 },
                new Item { Id = questRaidItemsId, Template = ItemTpl.STASH_QUESTRAID },
                new Item { Id = questStashItemsId, Template = ItemTpl.STASH_QUESTOFFLINE },
                new Item { Id = sortingTableId, Template = ItemTpl.SORTINGTABLE_SORTING_TABLE },
                new Item { Id = hideoutCustomizationStashId, Template = ItemTpl.HIDEOUTAREACONTAINER_CUSTOMIZATION }
            ],
            Equipment = equipmentId,
            Stash = stashId,
            QuestRaidItems = questRaidItemsId,
            QuestStashItems = questStashItemsId,
            SortingTable = sortingTableId,
            HideoutAreaStashes = new Dictionary<string, string>(),
            FastPanel = new Dictionary<string, string>(),
            FavoriteItems = [],
            HideoutCustomizationStashId = hideoutCustomizationStashId
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
        _botConfig.Equipment.TryGetValue(_botGeneratorHelper.GetBotEquipmentRole(botRole), out var botEquipConfig);
        var randomistionDetails = _botHelper.GetBotRandomizationDetails(botLevel, botEquipConfig);

        // Apply nighttime changes if its nighttime + there's changes to make
        if (
            randomistionDetails?.NighttimeChanges is not null &&
            raidConfig is not null &&
            _weatherHelper.IsNightTime(raidConfig.TimeVariant)
        )
            foreach (var equipmentSlotKvP in randomistionDetails.NighttimeChanges.EquipmentModsModifiers)
                // Never let mod chance go outside of 0 - 100
                randomistionDetails.EquipmentMods[equipmentSlotKvP.Key] = Math.Min(
                    Math.Max(randomistionDetails.EquipmentMods[equipmentSlotKvP.Key] + equipmentSlotKvP.Value, 0),
                    100
                );

        // Get profile of player generating bots, we use their level later on
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);
        var botEquipmentRole = _botGeneratorHelper.GetBotEquipmentRole(botRole);


        // Iterate over all equipment slots of bot, do it in specifc order to reduce conflicts
        // e.g. ArmorVest should be generated after TactivalVest
        // or FACE_COVER before HEADWEAR
        foreach (var (equipmentSlot, value) in templateInventory.Equipment)
        {
            // Skip some slots as they need to be done in a specific order + with specific parameter values
            // e.g. Weapons
            if (_excludedEquipmentSlots.Contains(equipmentSlot)) continue;

            GenerateEquipment(
                new GenerateEquipmentProperties
                {
                    RootEquipmentSlot = equipmentSlot,
                    RootEquipmentPool = value,
                    ModPool = templateInventory.Mods,
                    SpawnChances = wornItemChances,
                    BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                    Inventory = botInventory,
                    BotEquipmentConfig = botEquipConfig,
                    RandomisationDetails = randomistionDetails,
                    GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
                }
            );
        }

        // Generate below in specific order
        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.Pockets,
                // Unheard profiles have unique sized pockets
                RootEquipmentPool = GetPocketPoolByGameEdition(chosenGameVersion, templateInventory),
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GenerateModsBlacklist = [ItemTpl.POCKETS_1X4_TUE, ItemTpl.POCKETS_LARGE],
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.FaceCover,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.FaceCover],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.Headwear,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Headwear],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.Earpiece,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.Earpiece],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );

        var hasArmorVest = GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.ArmorVest,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.ArmorVest],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );

        // Bot has no armor vest and flagged to be forced to wear armored rig in this event
        if (botEquipConfig.ForceOnlyArmoredRigWhenNoArmor.GetValueOrDefault(false) && !hasArmorVest)
            // Filter rigs down to only those with armor
            FilterRigsToThoseWithProtection(templateInventory.Equipment, botRole);

        // Optimisation - Remove armored rigs from pool
        if (hasArmorVest)
            // Filter rigs down to only those with armor
            FilterRigsToThoseWithoutProtection(templateInventory.Equipment, botRole);

        // Bot is flagged as always needing a vest
        if (botEquipConfig.ForceRigWhenNoVest.GetValueOrDefault(false) && !hasArmorVest) wornItemChances.EquipmentChances["TacticalVest"] = 100;

        GenerateEquipment(
            new GenerateEquipmentProperties
            {
                RootEquipmentSlot = EquipmentSlots.TacticalVest,
                RootEquipmentPool = templateInventory.Equipment[EquipmentSlots.TacticalVest],
                ModPool = templateInventory.Mods,
                SpawnChances = wornItemChances,
                BotData = new BotData { Role = botRole, Level = botLevel, EquipmentRole = botEquipmentRole },
                Inventory = botInventory,
                BotEquipmentConfig = botEquipConfig,
                RandomisationDetails = randomistionDetails,
                GeneratingPlayerLevel = pmcProfile?.Info?.Level ?? 1
            }
        );
    }

    protected Dictionary<string, double> GetPocketPoolByGameEdition(string chosenGameVersion, BotTypeInventory templateInventory)
    {
        return chosenGameVersion == GameEditions.UNHEARD
            ? new Dictionary<string, double> { [ItemTpl.POCKETS_1X4_TUE] = 1 }
            : templateInventory.Equipment.GetValueOrDefault(EquipmentSlots.Pockets);
    }

    /// <summary>
    /// Remove non-armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equipment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    public void FilterRigsToThoseWithProtection(Dictionary<EquipmentSlots, Dictionary<string, double>> templateEquipment, string botRole)
    {
        var tacVestsWithArmor = templateEquipment[EquipmentSlots.TacticalVest]
            .Where(kvp => _itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!tacVestsWithArmor.Any())
        {
            if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Unable to filter to only armored rigs as bot: {botRole} has none in pool");

            return;
        }

        templateEquipment[EquipmentSlots.TacticalVest] = tacVestsWithArmor;
    }

    /// <summary>
    /// Remove armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equipment to filter TacticalVest by</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    /// <param name="allowEmptyResult">Should the function return all rigs when 0 unarmored are found</param>
    public void FilterRigsToThoseWithoutProtection(Dictionary<EquipmentSlots, Dictionary<string, double>> templateEquipment, string botRole,
        bool allowEmptyResult = true)
    {
        var tacVestsWithoutArmor = templateEquipment[EquipmentSlots.TacticalVest]
            .Where(kvp => !_itemHelper.ItemHasSlots(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!allowEmptyResult && !tacVestsWithoutArmor.Any())
        {
            if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Unable to filter to only unarmored rigs as bot: {botRole} has none in pool");

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
        List<string> slotsToCheck = [EquipmentSlots.Pockets.ToString(), EquipmentSlots.SecuredContainer.ToString()];
        double? spawnChance = slotsToCheck.Contains(settings.RootEquipmentSlot.ToString())
            ? 100
            : settings.SpawnChances.EquipmentChances.GetValueOrDefault(settings.RootEquipmentSlot.ToString());

        if (!spawnChance.HasValue)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-no_spawn_chance_defined_for_equipment_slot",
                    settings.RootEquipmentSlot
                )
            );

            return false;
        }

        // Roll dice on equipment item
        var shouldSpawn = _randomUtil.GetChance100(spawnChance ?? 0);
        if (shouldSpawn && settings.RootEquipmentPool.Any())
        {
            TemplateItem pickedItemDb = null;
            var found = false;

            // Limit attempts to find a compatible item as it's expensive to check them all
            var maxAttempts = Math.Round(settings.RootEquipmentPool.Count * 0.75); // Roughly 75% of pool size
            var attempts = 0;
            while (!found)
            {
                if (!settings.RootEquipmentPool.Any()) return false;

                var chosenItemTpl = _weightedRandomHelper.GetWeightedValue(settings.RootEquipmentPool);
                var dbResult = _itemHelper.GetItem(chosenItemTpl);

                if (!dbResult.Key)
                {
                    _logger.Error(_localisationService.GetText("bot-missing_item_template", chosenItemTpl));
                    if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"EquipmentSlot-> {settings.RootEquipmentSlot}");

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
                    if (attempts > maxAttempts) return false;

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
                settings.GeneratingPlayerLevel.Value
            );

            // Edge case: Filter the armor items mod pool if bot exists in config dict + config has armor slot
            if (_botConfig.Equipment.ContainsKey(settings.BotData.EquipmentRole) &&
                settings.RandomisationDetails?.RandomisedArmorSlots != null &&
                settings.RandomisationDetails.RandomisedArmorSlots.Contains(settings.RootEquipmentSlot.ToString()))
                // Filter out mods from relevant blacklist
                settings.ModPool[pickedItemDb.Id] = GetFilteredDynamicModsForItem(
                    pickedItemDb.Id,
                    botEquipBlacklist.Equipment
                );
            var itemIsOnGenerateModBlacklist = settings.GenerateModsBlacklist != null && settings.GenerateModsBlacklist.Contains(pickedItemDb.Id);
            // Does item have slots for sub-mods to be inserted into
            if (pickedItemDb.Properties?.Slots?.Count > 0 && !itemIsOnGenerateModBlacklist)
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
    public Dictionary<string, HashSet<string>> GetFilteredDynamicModsForItem(string itemTpl, Dictionary<string, List<string>> equipmentBlacklist)
    {
        var modPool = _botEquipmentModPoolService.GetModsForGearSlot(itemTpl);
        foreach (var modSlot in modPool)
        {
            // Get blacklist
            if (!equipmentBlacklist.TryGetValue(modSlot.Key, out var blacklistedMods)) blacklistedMods = [];
            ;

            // Get mods not on blacklist
            var filteredMods = modPool[modSlot.Key].Where((slotName) => !blacklistedMods.Contains(slotName));
            if (!filteredMods.Any())
            {
                _logger.Warning($"Filtering {modSlot.Key} pool resulting in 0 items, skipping filter");
                continue;
            }

            modPool[modSlot.Key] = filteredMods.ToHashSet();
        }

        return modPool.ToDictionary();
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
        foreach (var desiredWeapons in weaponSlotsToFill)
            // Add weapon to bot if true and bot json has something to put into the slot
            if (desiredWeapons.ShouldSpawn && templateInventory.Equipment[desiredWeapons.Slot].Any())
                AddWeaponAndMagazinesToInventory(
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

    /// <summary>
    /// Calculate if the bot should have weapons in Primary/Secondary/Holster slots
    /// </summary>
    /// <param name="equipmentChances">Chances bot has certain equipment</param>
    /// <returns>What slots bot should have weapons generated for</returns>
    public List<DesiredWeapons> GetDesiredWeaponsForBot(Chances equipmentChances)
    {
        var shouldSpawnPrimary = _randomUtil.GetChance100(equipmentChances.EquipmentChances["FirstPrimaryWeapon"]);
        return
        [
            new DesiredWeapons
            {
                Slot = EquipmentSlots.FirstPrimaryWeapon, ShouldSpawn = shouldSpawnPrimary
            },
            new DesiredWeapons
            {
                Slot = EquipmentSlots.SecondPrimaryWeapon,
                ShouldSpawn = shouldSpawnPrimary && _randomUtil.GetChance100(equipmentChances.EquipmentChances["SecondPrimaryWeapon"])
            },
            new DesiredWeapons
            {
                Slot = EquipmentSlots.Holster,
                ShouldSpawn = !shouldSpawnPrimary || _randomUtil.GetChance100(equipmentChances.EquipmentChances["Holster"]) // No primary = force pistol
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
        var generatedWeapon = _botWeaponGenerator.GenerateRandomWeapon(
            sessionId,
            weaponSlot.Slot.ToString(),
            templateInventory,
            botInventory.Equipment,
            equipmentChances.WeaponModsChances,
            botRole,
            isPmc,
            botLevel
        );

        botInventory.Items.AddRange(generatedWeapon.Weapon);

        _botWeaponGenerator.AddExtraMagazinesToInventory(
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
