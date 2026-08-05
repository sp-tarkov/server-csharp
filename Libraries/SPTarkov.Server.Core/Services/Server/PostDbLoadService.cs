using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.InRaid;
using SPTarkov.Server.Core.Services.Items;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services.Server;

[Injectable(InjectionType.Singleton)]
public class PostDbLoadService(
    ISptLogger<PostDbLoadService> logger,
    TemplateTable templateTable,
    TradersTable traderTable,
    GlobalTable globalTable,
    HideoutTable hideoutTable,
    LocaleTable localeTable,
    LocationTable locationTable,
    ServerLocalisationService serverLocalisationService,
    SeasonalEventService seasonalEventService,
    CustomLocationWaveService customLocationWaveService,
    GoonLocationSpawnService goonLocationSpawnService,
    OpenZoneService openZoneService,
    ItemBaseClassService itemBaseClassService,
    RaidWeatherService raidWeatherService,
    BotConfig botConfig,
    PmcConfig pmcConfig,
    CoreConfig coreConfig,
    HideoutConfig hideoutConfig,
    ItemConfig itemConfig,
    LocationConfig locationConfig,
    LootConfig lootConfig,
    RagfairConfig ragfairConfig,
    RandomUtil randomUtil,
    ICloner cloner
)
{
    public void PerformPostDbLoadActions()
    {
        coreConfig.ServerStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Regenerate base cache now mods are loaded and game is starting
        // Mods that add items and use the baseClass service generate the cache including their items, the next mod that
        // add items gets left out,causing warnings
        itemBaseClassService.HydrateItemBaseClassCache();

        ReduceStaticItemWeight();

        AddCustomLooseLootPositions();

        MergeCustomAchievements();

        AdjustMinReserveRaiderSpawnChance();

        if (coreConfig.Fixes.FixShotgunDispersion)
        {
            FixShotgunDispersions(coreConfig.Fixes.ShotgunIdsToFix);
        }

        if (locationConfig.AddOpenZonesToAllMaps)
        {
            openZoneService.ApplyZoneChangesToAllMaps();
        }

        if (locationConfig.AddCustomBotWavesToMaps)
        {
            customLocationWaveService.ApplyWaveChangesToAllMaps();
        }

        if (locationConfig.EnableBotTypeLimits)
        {
            AdjustMapBotLimits();
        }

        FixDogtagCaseNotAcceptingAllDogtags();

        AdjustLooseLootSpawnProbabilities();

        AdjustLocationBotValues();

        MergeCustomHideoutAreas();

        if (locationConfig.RogueLighthouseSpawnTimeSettings.Enabled)
        {
            FixRoguesSpawningInstantlyOnLighthouse();
        }

        AdjustLabsRaiderSpawnRate();

        AdjustHideoutCraftTimes(hideoutConfig.OverrideCraftTimeSeconds);
        AdjustHideoutBuildTimes(hideoutConfig.OverrideBuildTimeSeconds);

        UnlockHideoutLootCrateCrafts(hideoutConfig.HideoutLootCrateCraftIdsToUnlockInHideout);

        CloneExistingCraftsAndAddNew();

        RemovePrestigeQuestRequirementsIfQuestNotFound();

        RemovePraporTestMessage();

        ValidateQuestAssortUnlocksExist();

        if (seasonalEventService.IsAutomaticEventDetectionEnabled())
        {
            seasonalEventService.CacheActiveEvents();
            seasonalEventService.EnableSeasonalEvents();
        }

        // Flea bsg blacklist is off
        if (!ragfairConfig.Dynamic.Blacklist.EnableBsgList)
        {
            SetAllDbItemsAsSellableOnFlea();
        }

        AddMissingTraderBuyRestrictionMaxValue();

        ApplyFleaPriceOverrides();

        AddCustomItemPresetsToGlobals();

        var currentSeason = seasonalEventService.GetActiveWeatherSeason();
        raidWeatherService.GenerateFutureWeatherAndCache(currentSeason);

        if (botConfig.WeeklyBoss.Enabled)
        {
            var chosenBoss = GetWeeklyBoss(botConfig.WeeklyBoss.BossPool, botConfig.WeeklyBoss.ResetDay);
            FlagMapAsGuaranteedBoss(chosenBoss);
        }

        if (botConfig.GoonSpawnSystem.Enabled)
        {
            goonLocationSpawnService.AdjustGoonMapSpawns();
        }

        if (botConfig.ReplaceScavWith != WildSpawnType.assault)
        {
            ReplaceScavWavesWithRole(botConfig.ReplaceScavWith);
        }

        if (coreConfig.Fixes.RenamePreRaidLocales)
        {
            RenamePreraidLocales();
        }
    }

    protected void RenamePreraidLocales()
    {
        if (localeTable.Global.TryGetValue("en", out var lazyloadedValue))
        {
            // We have to add a transformer here, because locales are lazy loaded due to them taking up huge space in memory
            // The transformer will make sure that each time the locales are requested, the ones changed or added below are included
            lazyloadedValue.AddTransformer(lazyloadedLocaleData =>
            {
                lazyloadedLocaleData["Offline raid test mode"] = "SPT";
                lazyloadedLocaleData["Offline raid description"] = " ";

                return lazyloadedLocaleData;
            });
        }
    }

    protected void ReplaceScavWavesWithRole(WildSpawnType newScavRole)
    {
        foreach (var location in locationTable.GetDictionary().Values)
        {
            if (location.Base?.Waves is null)
            {
                continue;
            }

            foreach (var wave in location.Base.Waves)
            {
                if (wave.WildSpawnType == WildSpawnType.assault)
                {
                    wave.WildSpawnType = newScavRole;
                }
            }
        }
    }

    /// <summary>
    /// BSG don't have all the new dogtag types in the containers allowed list
    /// </summary>
    protected void FixDogtagCaseNotAcceptingAllDogtags()
    {
        //Find case to add new ids to
        if (!templateTable.Items.TryGetValue(ItemTpl.CONTAINER_DOGTAG_CASE, out var dogtagCase))
        {
            return;
        }

        // Find the grid in case we want to add ids to
        var filterSet = dogtagCase.Properties?.Grids?.FirstOrDefault()?.Properties?.Filters?.FirstOrDefault()?.Filter;
        if (filterSet is null)
        {
            return;
        }

        MongoId[] dogtagTpls =
        [
            new("59f32bb586f774757e1e8442"),
            new("59f32c3b86f77472a31742f0"),
            new("6662ea05f6259762c56f3189"),
            new("6662e9f37fa79a6d83730fa0"),
            new("6662e9cda7e0b43baa3d5f76"),
            new("6662e9aca7e0b43baa3d5f74"),
            new("675dc9d37ae1a8792107ca96"),
            new("675dcb0545b1a2d108011b2b"),
            new("6764207f2fa5e32733055c4a"),
            new("6764202ae307804338014c1a"),
            new("6746fd09bafff85008048838"),
            new("67471928d17d6431550563b5"),
            new("674731c8bafff850080488bb"),
            new("684180bc51bf8645f7067bc8"),
            new("684181208d035f60230f63f9"),
            new("67471938bafff850080488b7"),
            new("6747193f170146228c0d2226"),
            new("674731d1170146228c0d222a"),
            new("68418091b5b0c9e4c60f0e7a"),
            new("684180ee9b6d80d840042e8a"),
        ];

        // Add all ids to grid
        filterSet.UnionWith(dogtagTpls);
    }

    /// <summary>
    /// Choose a boss that will spawn at 100% on a map from a predefined collection of bosses
    /// </summary>
    /// <param name="bosses">Pool of bosses to pick from</param>
    /// <param name="bossResetDay">Day of week choice of boss changes</param>
    /// <returns>Boss to spawn for this week</returns>
    protected WildSpawnType GetWeeklyBoss(List<WildSpawnType> bosses, DayOfWeek bossResetDay)
    {
        // Get closest monday to today
        var startOfWeek = DateTime.Today.GetMostRecentPreviousDay(bossResetDay);

        // Create a consistent seed for the week using the year and the day of the year of above monday chosen
        // This results in seed being identical for the week
        var seed = startOfWeek.Year * 1009 + startOfWeek.DayOfYear;

        // Init Random class with unique seed
        var random = new Random(seed);

        // First number generated by random.Next() will always be the same because of the seed
        return bosses[random.Next(0, bosses.Count)];
    }

    /// <summary>
    /// Given the provided boss, flag them as 100% spawn and add skull to the map they spawn on
    /// </summary>
    /// <param name="boss">Boss to flag</param>
    protected void FlagMapAsGuaranteedBoss(WildSpawnType boss)
    {
        // Get the corresponding map for the provided boss
        Location? location;
        switch (boss)
        {
            case WildSpawnType.bossBully:
                location = locationTable.Bigmap;
                break;
            case WildSpawnType.bossGluhar:
                location = locationTable.RezervBase;
                break;
            case WildSpawnType.bossKilla:
                location = locationTable.Interchange;
                break;
            case WildSpawnType.bossKojaniy:
                location = locationTable.Woods;
                break;
            case WildSpawnType.bossSanitar:
                location = locationTable.Shoreline;
                break;
            case WildSpawnType.bossKolontay:
                location = locationTable.TarkovStreets;
                break;
            case WildSpawnType.bossKnight:
                location = locationTable.Lighthouse;
                break;
            case WildSpawnType.bossTagilla:
                location = locationTable.Factory4Day;
                break;
            default:
                logger.Warning($"Unknown boss type: {boss}. Unable to set as weekly. Skipping");
                return;
        }

        var bossSpawn = location.Base.BossLocationSpawn.FirstOrDefault(x => x.BossName == boss.ToString());
        if (bossSpawn is null)
        {
            logger.Warning($"Boss: {boss} not found on map, unable to set as weekly. Skipping");
            return;
        }

        logger.Debug($"{boss} is boss of the week");
        bossSpawn.BossChance = 100;
        bossSpawn.ShowOnTarkovMap = true;
        bossSpawn.ShowOnTarkovMapPvE = true;
    }

    private void MergeCustomHideoutAreas()
    {
        foreach (var customArea in hideoutTable.CustomAreas ?? [])
        {
            // Check if exists
            if (hideoutTable.Areas.Exists(area => area.Id == customArea.Id))
            {
                logger.Warning($"Unable to add new hideout area with Id: {customArea.Id} as ID is already in use, skipping");

                continue;
            }

            hideoutTable.Areas.Add(customArea);
        }
    }

    /// <summary>
    ///     Merge custom achievements into achievement db table
    /// </summary>
    protected void MergeCustomAchievements()
    {
        var achievements = templateTable.Achievements;
        foreach (var customAchievement in templateTable.CustomAchievements)
        {
            if (achievements.Exists(a => a.Id == customAchievement.Id))
            {
                logger.Debug($"Unable to add custom achievement as id: {customAchievement.Id} already exists");
                continue;
            }

            achievements.Add(customAchievement);
        }
    }

    private void RemovePrestigeQuestRequirementsIfQuestNotFound()
    {
        var prestigeDb = templateTable.Prestige;

        foreach (var prestige in prestigeDb.Elements)
        {
            var conditionsToRemove = prestige
                .Conditions.Where(c => c.ConditionType == "Quest" && c.Target.IsItem && !templateTable.Quests.ContainsKey(c.Target.Item))
                .ToList();

            foreach (var conditionToRemove in conditionsToRemove)
            {
                logger.Debug($"Removing required quest from prestige: {conditionToRemove.Target.Item}");
                prestige.Conditions.Remove(conditionToRemove);
            }
        }
    }

    private void RemovePraporTestMessage()
    {
        foreach (var (_, lazyLoad) in localeTable.Global)
        {
            lazyLoad.AddTransformer(lazyloadedData =>
            {
                lazyloadedData["61687e2c3e526901fa76baf9"] = "";

                return lazyloadedData;
            });
        }
    }

    protected void CloneExistingCraftsAndAddNew()
    {
        var hideoutCraftDb = hideoutTable.Production;
        var craftsToAdd = hideoutConfig.HideoutCraftsToAdd;
        foreach (var craftToAdd in craftsToAdd)
        {
            var clonedCraft = cloner.Clone(hideoutCraftDb.Recipes.FirstOrDefault(x => x.Id == craftToAdd.CraftIdToCopy));
            if (clonedCraft is null)
            {
                logger.Warning($"Unable to find hideout craft: {craftToAdd.CraftIdToCopy}, skipping");

                continue;
            }

            clonedCraft.Id = craftToAdd.NewId;
            clonedCraft.Requirements = craftToAdd.Requirements;
            clonedCraft.EndProduct = craftToAdd.CraftOutputTpl;

            hideoutCraftDb.Recipes.Add(clonedCraft);
        }
    }

    protected void AdjustMinReserveRaiderSpawnChance()
    {
        // Get reserve base.json
        var reserveBase = locationTable.GetLocation(ELocationName.RezervBase.ToString()).Base;

        // Raiders are bosses, get only those from boss spawn array
        foreach (var raiderSpawn in reserveBase.BossLocationSpawn.Where(boss => boss.BossName == "pmcBot"))
        {
            var isTriggered = raiderSpawn.TriggerId.Length > 0; // Empty string if not triggered
            var newSpawnChance = isTriggered
                ? locationConfig.ReserveRaiderSpawnChanceOverrides.Triggered
                : locationConfig.ReserveRaiderSpawnChanceOverrides.NonTriggered;

            if (newSpawnChance == -1)
            {
                continue;
            }

            if (raiderSpawn.BossChance < newSpawnChance)
            // Desired chance is bigger than existing, override it
            {
                raiderSpawn.BossChance = newSpawnChance;
            }
        }
    }

    protected void AddCustomLooseLootPositions()
    {
        var looseLootPositionsToAdd = lootConfig.LooseLoot;
        foreach (var (locationId, positionsToAdd) in looseLootPositionsToAdd)
        {
            if (locationId is null)
            {
                logger.Warning(serverLocalisationService.GetText("location-unable_to_add_custom_loot_position", locationId));

                continue;
            }

            locationTable
                .GetLocation(locationId)
                .LooseLoot.AddTransformer(looseLootData =>
                {
                    if (looseLootData is null)
                    {
                        logger.Warning(serverLocalisationService.GetText("location-map_has_no_loose_loot_data", locationId));

                        return looseLootData;
                    }

                    foreach (var positionToAdd in positionsToAdd)
                    {
                        // Exists already, add new items to existing positions pool
                        var existingLootPosition = looseLootData.Spawnpoints.FirstOrDefault(x =>
                            x.Template.Id == positionToAdd.Template.Id
                        );

                        if (existingLootPosition is not null)
                        {
                            existingLootPosition.Template.Items = existingLootPosition.Template.Items.Union(positionToAdd.Template.Items);

                            existingLootPosition.ItemDistribution = existingLootPosition.ItemDistribution.Union(
                                positionToAdd.ItemDistribution
                            );

                            continue;
                        }

                        // New position, add entire object
                        looseLootData.Spawnpoints = looseLootData.Spawnpoints.Append(positionToAdd);
                    }

                    return looseLootData;
                });
        }
    }

    protected void ReduceStaticItemWeight()
    {
        foreach (var (locationId, itemTplWeightDict) in lootConfig.StaticItemWeightAdjustment)
        {
            locationTable
                .GetLocation(locationId)
                .StaticLoot.AddTransformer(staticLootData =>
                {
                    if (staticLootData is null)
                    {
                        return staticLootData;
                    }

                    foreach (var (itemTpl, percentAdjustment) in itemTplWeightDict)
                    {
                        foreach (var loot in staticLootData)
                        {
                            var itemsWithTpl = loot.Value.ItemDistribution.Where(item => item.Tpl == itemTpl);
                            foreach (var itemToAdjust in itemsWithTpl)
                            {
                                itemToAdjust.RelativeProbability = randomUtil.GetPercentOfValue(
                                    percentAdjustment,
                                    itemToAdjust.RelativeProbability.Value,
                                    0
                                );
                            }
                        }
                    }

                    return staticLootData;
                });
        }
    }

    // BSG have two values for shotgun dispersion, we make sure both have the same value
    protected void FixShotgunDispersions(IEnumerable<MongoId> shotgunIds)
    {
        foreach (var shotgunId in shotgunIds)
        {
            if (templateTable.Items.TryGetValue(shotgunId, out var shotgun) && shotgun.Properties.ShotgunDispersion.HasValue)
            {
                shotgun.Properties.shotgunDispersion = shotgun.Properties.ShotgunDispersion;
            }
        }
    }

    /// <summary>
    ///     Apply custom limits on bot types as defined in configs/location.json/botTypeLimits
    /// </summary>
    protected void AdjustMapBotLimits()
    {
        var mapsDb = locationTable.GetDictionary();
        if (locationConfig.BotTypeLimits is null)
        {
            return;
        }

        foreach (var (mapId, limits) in locationConfig.BotTypeLimits)
        {
            if (!mapsDb.TryGetValue(mapId, out var map))
            {
                logger.Warning(serverLocalisationService.GetText("bot-unable_to_edit_limits_of_unknown_map", mapId));

                continue;
            }

            foreach (var botToLimit in limits)
            {
                var index = map.Base.MinMaxBots.FindIndex(x => x.WildSpawnType == botToLimit.Type);
                if (index != -1)
                {
                    // Existing bot type found in MinMaxBots array, edit
                    var limitObjectToUpdate = map.Base.MinMaxBots[index];
                    limitObjectToUpdate.Min = botToLimit.Min;
                    limitObjectToUpdate.Max = botToLimit.Max;
                }
                else
                {
                    // Bot type not found, add new object
                    map.Base.MinMaxBots.Add(
                        new MinMaxBot
                        {
                            // Bot type not found, add new object
                            WildSpawnType = botToLimit.Type,
                            Min = botToLimit.Min,
                            Max = botToLimit.Max,
                        }
                    );
                }
            }
        }
    }

    protected void AdjustLooseLootSpawnProbabilities()
    {
        if (lootConfig.LooseLootSpawnPointAdjustments is null)
        {
            return;
        }

        foreach (var (mapId, mapAdjustments) in lootConfig.LooseLootSpawnPointAdjustments)
        {
            locationTable
                .GetLocation(mapId)
                .LooseLoot.AddTransformer(looselootData =>
                {
                    if (looselootData is null)
                    {
                        logger.Warning(serverLocalisationService.GetText("location-map_has_no_loose_loot_data", mapId));

                        return looselootData;
                    }

                    foreach (var (lootKey, newChanceValue) in mapAdjustments)
                    {
                        var lootPostionToAdjust = looselootData.Spawnpoints.FirstOrDefault(spawnPoint => spawnPoint.Template.Id == lootKey);
                        if (lootPostionToAdjust is null)
                        {
                            logger.Warning(
                                serverLocalisationService.GetText("location-unable_to_adjust_loot_position_on_map", new { lootKey, mapId })
                            );

                            continue;
                        }

                        lootPostionToAdjust.Probability = newChanceValue;
                    }

                    return looselootData;
                });
        }
    }

    protected void AdjustLocationBotValues()
    {
        var mapsDict = locationTable.GetDictionary();
        foreach (var (key, cap) in botConfig.MaxBotCap)
        {
            // Keys given are like this: "factory4_night" use GetMappedKey to change to "Factory4Night" which the dictionary contains
            if (!mapsDict.TryGetValue(locationTable.GetMappedKey(key), out var map))
            {
                continue;
            }

            map.Base.BotMax = cap;

            // make values no larger than 30 secs
            map.Base.BotStart = Math.Min(map.Base.BotStart, 30);
        }
    }

    /// <summary>
    ///     Make Rogues spawn later to allow for scavs to spawn first instead of rogues filling up all spawn positions
    /// </summary>
    protected void FixRoguesSpawningInstantlyOnLighthouse()
    {
        var rogueSpawnDelaySeconds = locationConfig.RogueLighthouseSpawnTimeSettings.WaitTimeSeconds;
        var lighthouse = locationTable.Lighthouse.Base;

        // Find Rogues that spawn instantly
        var instantRogueBossSpawns = lighthouse.BossLocationSpawn.Where(spawn => spawn.BossName == "exUsec" && spawn.Time == -1);
        foreach (var wave in instantRogueBossSpawns)
        {
            wave.Time = rogueSpawnDelaySeconds;
        }
    }

    /// <summary>
    ///     Make non-trigger-spawned raiders spawn earlier + always
    /// </summary>
    protected void AdjustLabsRaiderSpawnRate()
    {
        var labsBase = locationTable.Laboratory.Base;

        // Find spawns with empty string for triggerId/TriggerName
        var nonTriggerLabsBossSpawns = labsBase.BossLocationSpawn.Where(bossSpawn =>
            bossSpawn.TriggerId is null && bossSpawn.TriggerName is null
        );

        foreach (var boss in nonTriggerLabsBossSpawns)
        {
            boss.BossChance = 100;
            boss.Time /= 10;
        }
    }

    protected void AdjustHideoutCraftTimes(int overrideSeconds)
    {
        if (overrideSeconds == -1)
        {
            return;
        }

        foreach (var craft in hideoutTable.Production.Recipes)
        // Only adjust crafts ABOVE the override
        {
            craft.ProductionTime = Math.Min(craft.ProductionTime.Value, overrideSeconds);
        }
    }

    /// <summary>
    ///     Adjust all hideout craft times to be no higher than the override
    /// </summary>
    /// <param name="overrideSeconds"> Time in seconds </param>
    protected void AdjustHideoutBuildTimes(int overrideSeconds)
    {
        if (overrideSeconds == -1)
        {
            return;
        }

        foreach (var area in hideoutTable.Areas)
        foreach (var (_, stage) in area.Stages)
        // Only adjust crafts ABOVE the override
        {
            stage.ConstructionTime = Math.Min(stage.ConstructionTime.Value, overrideSeconds);
        }
    }

    protected void UnlockHideoutLootCrateCrafts(IEnumerable<MongoId> craftIdsToUnlock)
    {
        foreach (var craftId in craftIdsToUnlock)
        {
            var recipe = hideoutTable.Production.Recipes.FirstOrDefault(craft => craft.Id == craftId);
            if (recipe is not null)
            {
                recipe.Locked = false;
            }
        }
    }

    /// <summary>
    ///     Check for any missing assorts inside each traders assort.json data, checking against traders questassort.json
    /// </summary>
    protected void ValidateQuestAssortUnlocksExist()
    {
        var quests = templateTable.Quests;
        foreach (var (traderId, traderData) in traderTable)
        {
            var traderAssorts = traderData?.Assort;
            if (traderAssorts is null)
            {
                continue;
            }

            if (traderData.QuestAssort is null)
            {
                continue;
            }

            // Merge started/success/fail quest assorts into one dictionary
            var mergedQuestAssorts = new Dictionary<MongoId, MongoId>();
            mergedQuestAssorts = mergedQuestAssorts
                .Concat(traderData.QuestAssort["started"])
                .Concat(traderData.QuestAssort["success"])
                .Concat(traderData.QuestAssort["fail"])
                .ToDictionary();

            // Loop over all assorts for trader
            foreach (var (assortKey, questKey) in mergedQuestAssorts)
            // Does assort key exist in trader assort file
            {
                if (!traderAssorts.LoyalLevelItems.ContainsKey(assortKey))
                {
                    // Reverse lookup of enum key by value
                    var messageValues = new { traderName = traderId, questName = quests[questKey]?.QuestName ?? "UNKNOWN" };
                    logger.Warning(serverLocalisationService.GetText("assort-missing_quest_assort_unlock", messageValues));
                }
            }
        }
    }

    protected void SetAllDbItemsAsSellableOnFlea()
    {
        var dbItems = templateTable.Items.Values.ToList();
        foreach (
            var item in dbItems.Where(item =>
                string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && !item.Properties.CanSellOnRagfair.GetValueOrDefault(false)
                && !ragfairConfig.Dynamic.Blacklist.Custom.Contains(item.Id)
            )
        )
        {
            item.Properties.CanSellOnRagfair = true;
        }
    }

    protected void AddMissingTraderBuyRestrictionMaxValue()
    {
        var restrictions = globalTable.Configuration.TradingSettings.BuyRestrictionMaxBonus;
        restrictions["unheard_edition"] = new BuyRestrictionMaxBonus { Multiplier = restrictions["edge_of_darkness"].Multiplier };
    }

    protected void ApplyFleaPriceOverrides()
    {
        foreach (var (itemTpl, price) in ragfairConfig.Dynamic.ItemPriceOverrideRouble)
        {
            templateTable.Prices[itemTpl] = price;
        }
    }

    protected void AddCustomItemPresetsToGlobals()
    {
        foreach (var presetToAdd in itemConfig.CustomItemGlobalPresets)
        {
            if (globalTable.ItemPresets.ContainsKey(presetToAdd.Id))
            {
                logger.Warning($"Global ItemPreset with Id of: {presetToAdd.Id} already exists, unable to overwrite");
                continue;
            }

            globalTable.ItemPresets.TryAdd(presetToAdd.Id, presetToAdd);
        }
    }
}
