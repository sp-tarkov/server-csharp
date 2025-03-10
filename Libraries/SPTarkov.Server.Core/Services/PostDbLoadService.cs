using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class PostDbLoadService(
    ISptLogger<PostDbLoadService> _logger,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    SeasonalEventService _seasonalEventService,
    CustomLocationWaveService _customLocationWaveService,
    OpenZoneService _openZoneService,
    ItemBaseClassService _itemBaseClassService,
    RaidWeatherService _raidWeatherService,
    ConfigServer _configServer,
    ICloner _cloner)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
    protected ItemConfig _itemConfig = _configServer.GetConfig<ItemConfig>();
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();
    protected LootConfig _lootConfig = _configServer.GetConfig<LootConfig>();
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();

    public void PerformPostDbLoadActions()
    {
        // Regenerate base cache now mods are loaded and game is starting
        // Mods that add items and use the baseClass service generate the cache including their items, the next mod that
        // add items gets left out,causing warnings
        _itemBaseClassService.HydrateItemBaseClassCache();

        // Validate that only mongoIds exist in items, quests, and traders
        // Kill the startup if not.
        // TODO: We can probably remove this in a couple versions
        _databaseService.ValidateDatabase();
        if (!_databaseService.IsDatabaseValid())
        {
            throw new Exception("Server start failure, database invalid");
        }

        AddCustomLooseLootPositions();

        AdjustMinReserveRaiderSpawnChance();

        if (_coreConfig.Fixes.FixShotgunDispersion)
        {
            FixShotgunDispersions();
        }

        if (_locationConfig.AddOpenZonesToAllMaps)
        {
            _openZoneService.ApplyZoneChangesToAllMaps();
        }

        if (_pmcConfig.RemoveExistingPmcWaves.GetValueOrDefault(false))
        {
            RemoveExistingPmcWaves();
        }

        if (_locationConfig.AddCustomBotWavesToMaps)
        {
            _customLocationWaveService.ApplyWaveChangesToAllMaps();
        }

        if (_locationConfig.EnableBotTypeLimits)
        {
            AdjustMapBotLimits();
        }

        AdjustLooseLootSpawnProbabilities();

        AdjustLocationBotValues();

        if (_locationConfig.RogueLighthouseSpawnTimeSettings.Enabled)
        {
            FixRoguesSpawningInstantlyOnLighthouse();
        }

        AdjustLabsRaiderSpawnRate();

        AdjustHideoutCraftTimes(_hideoutConfig.OverrideCraftTimeSeconds);
        AdjustHideoutBuildTimes(_hideoutConfig.OverrideBuildTimeSeconds);

        UnlockHideoutLootCrateCrafts();

        CloneExistingCraftsAndAddNew();

        RemoveNewBeginningRequirementFromPrestige();

        RemovePraporTestMessage();

        ValidateQuestAssortUnlocksExist();

        if (_seasonalEventService.IsAutomaticEventDetectionEnabled())
        {
            _seasonalEventService.CacheActiveEvents();
            _seasonalEventService.EnableSeasonalEvents();
        }

        // Flea bsg blacklist is off
        if (!_ragfairConfig.Dynamic.Blacklist.EnableBsgList)
        {
            SetAllDbItemsAsSellableOnFlea();
        }

        AddMissingTraderBuyRestrictionMaxValue();

        ApplyFleaPriceOverrides();

        AddCustomItemPresetsToGlobals();

        var currentSeason = _seasonalEventService.GetActiveWeatherSeason();
        _raidWeatherService.GenerateWeather(currentSeason);
    }

    private void RemoveNewBeginningRequirementFromPrestige()
    {
        var prestigeDb = _databaseService.GetTemplates().Prestige;
        var newBeginningQuestId = new HashSet<string>
        {
            "6761f28a022f60bb320f3e95",
            "6761ff17cdc36bd66102e9d0"
        };
        foreach (var prestige in prestigeDb.Elements)
        {
            var itemToRemove = prestige.Conditions?.FirstOrDefault(cond => newBeginningQuestId.Contains(cond.Target?.Item));
            if (itemToRemove is null)
            {
                continue;
            }

            var indexToRemove = prestige.Conditions.IndexOf(itemToRemove);
            if (indexToRemove != -1)
            {
                prestige.Conditions.RemoveAt(indexToRemove);
            }
        }
    }

    protected void CloneExistingCraftsAndAddNew()
    {
        var hideoutCraftDb = _databaseService.GetHideout().Production;
        var craftsToAdd = _hideoutConfig.HideoutCraftsToAdd;
        foreach (var craftToAdd in craftsToAdd)
        {
            var clonedCraft = _cloner.Clone(
                hideoutCraftDb.Recipes.FirstOrDefault(x => x.Id == craftToAdd.CraftIdToCopy)
            );
            if (clonedCraft is null)
            {
                _logger.Warning($"Unable to find hideout craft: {craftToAdd.CraftIdToCopy}, skipping");

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
        var reserveBase = _databaseService.GetLocation(ELocationName.RezervBase.ToString()).Base;

        // Raiders are bosses, get only those from boss spawn array
        foreach (var raiderSpawn in reserveBase.BossLocationSpawn.Where(boss => boss.BossName == "pmcBot"))
        {
            var isTriggered = raiderSpawn.TriggerId.Length > 0; // Empty string if not triggered
            var newSpawnChance = isTriggered
                ? _locationConfig.ReserveRaiderSpawnChanceOverrides.Triggered
                : _locationConfig.ReserveRaiderSpawnChanceOverrides.NonTriggered;

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
        var looseLootPositionsToAdd = _lootConfig.LooseLoot;
        foreach (var (mapId, positionsToAdd) in looseLootPositionsToAdd)
        {
            if (mapId is null)
            {
                _logger.Warning(_localisationService.GetText("location-unable_to_add_custom_loot_position", mapId));

                continue;
            }

            var mapLooseLoot = _databaseService.GetLocation(mapId).LooseLoot.Value;
            if (mapLooseLoot is null)
            {
                _logger.Warning(_localisationService.GetText("location-map_has_no_loose_loot_data", mapId));

                continue;
            }

            foreach (var positionToAdd in positionsToAdd)
            {
                // Exists already, add new items to existing positions pool
                var existingLootPosition = mapLooseLoot.Spawnpoints.FirstOrDefault(
                    x => x.Template.Id == positionToAdd.Template.Id
                );

                if (existingLootPosition is not null)
                {
                    existingLootPosition.Template.Items.AddRange(positionToAdd.Template.Items);
                    existingLootPosition.ItemDistribution.AddRange(positionToAdd.ItemDistribution);

                    continue;
                }

                // New position, add entire object
                mapLooseLoot.Spawnpoints.Add(positionToAdd);
            }
        }
    }

    // BSG have two values for shotgun dispersion, we make sure both have the same value
    protected void FixShotgunDispersions()
    {
        var itemDb = _databaseService.GetItems();

        var shotguns = new List<string>
        {
            Weapons.SHOTGUN_12G_SAIGA_12K,
            Weapons.SHOTGUN_20G_TOZ_106,
            Weapons.SHOTGUN_12G_M870
        };
        foreach (var shotgunId in shotguns)
        {
            if (itemDb[shotgunId].Properties.ShotgunDispersion.HasValue)
            {
                itemDb[shotgunId].Properties.shotgunDispersion = itemDb[shotgunId].Properties.ShotgunDispersion;
            }
        }
    }

    protected void RemoveExistingPmcWaves()
    {
        var locations = _databaseService.GetLocations().GetDictionary();

        var pmcTypes = new HashSet<string> { "pmcUSEC", "pmcBEAR" };
        foreach (var locationkvP in locations)
        {
            if (locationkvP.Value?.Base?.BossLocationSpawn is null)
            {
                continue;
            }

            locationkvP.Value.Base.BossLocationSpawn = locationkvP.Value.Base.BossLocationSpawn.Where(
                (bossSpawn) => !pmcTypes.Contains(bossSpawn.BossName)).ToList();
        }
    }

    // Apply custom limits on bot types as defined in configs/location.json/botTypeLimits
    protected void AdjustMapBotLimits()
    {
        var mapsDb = _databaseService.GetLocations().GetDictionary();
        if (_locationConfig.BotTypeLimits is null)
        {
            return;
        }

        foreach (var (mapId, limits) in _locationConfig.BotTypeLimits)
        {
            if (!mapsDb.TryGetValue(mapId, out var map))
            {
                _logger.Warning(
                    _localisationService.GetText("bot-unable_to_edit_limits_of_unknown_map", mapId)
                );

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
                            Max = botToLimit.Max
                        }
                    );
                }
            }
        }
    }

    protected void AdjustLooseLootSpawnProbabilities()
    {
        if (_lootConfig.LooseLootSpawnPointAdjustments is null)
        {
            return;
        }

        foreach (var (mapId, mapAdjustments) in _lootConfig.LooseLootSpawnPointAdjustments)
        {
            var mapLooseLootData = _databaseService.GetLocation(mapId).LooseLoot.Value;
            if (mapLooseLootData is null)
            {
                _logger.Warning(_localisationService.GetText("location-map_has_no_loose_loot_data", mapId));

                continue;
            }

            foreach (var (lootKey, newChanceValue) in mapAdjustments)
            {
                var lootPostionToAdjust = mapLooseLootData.Spawnpoints.FirstOrDefault(
                    spawnPoint => spawnPoint.Template.Id == lootKey
                );
                if (lootPostionToAdjust is null)
                {
                    _logger.Warning(
                        _localisationService.GetText(
                            "location-unable_to_adjust_loot_position_on_map",
                            new
                            {
                                lootKey,
                                mapId
                            }
                        )
                    );

                    continue;
                }

                lootPostionToAdjust.Probability = newChanceValue;
            }
        }
    }


    protected void AdjustLocationBotValues()
    {
        var mapsDb = _databaseService.GetLocations();
        var mapsDict = mapsDb.GetDictionary();
        foreach (var (key, cap) in _botConfig.MaxBotCap)
        {
            // Keys given are like this: "factory4_night" use GetMappedKey to change to "Factory4Night" which the dictionary contains
            if (!mapsDict.TryGetValue(mapsDb.GetMappedKey(key), out var map))
            {
                continue;
            }

            map.Base.BotMaxPvE = cap;
            map.Base.BotMax = cap;

            // make values no larger than 30 secs
            map.Base.BotStart = Math.Min(map.Base.BotStart.Value, 30);
        }
    }

// Make Rogues spawn later to allow for scavs to spawn first instead of rogues filling up all spawn positions
    protected void FixRoguesSpawningInstantlyOnLighthouse()
    {
        var rogueSpawnDelaySeconds = _locationConfig.RogueLighthouseSpawnTimeSettings.WaitTimeSeconds;
        var lighthouse = _databaseService.GetLocations().Lighthouse?.Base;
        if (lighthouse is null)
            // Just in case they remove this cursed map
        {
            return;
        }

        // Find Rogues that spawn instantly
        var instantRogueBossSpawns = lighthouse.BossLocationSpawn
            .Where(spawn => spawn.BossName == "exUsec" && spawn.Time == -1);
        foreach (var wave in instantRogueBossSpawns)
        {
            wave.Time = rogueSpawnDelaySeconds;
        }
    }

// Make non-trigger-spawned raiders spawn earlier + always
    protected void AdjustLabsRaiderSpawnRate()
    {
        var labsBase = _databaseService.GetLocations().Laboratory.Base;

        // Find spawns with empty string for triggerId/TriggerName
        var nonTriggerLabsBossSpawns = labsBase.BossLocationSpawn.Where(
            bossSpawn => bossSpawn.TriggerId is null && bossSpawn.TriggerName is null
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

        foreach (var craft in _databaseService.GetHideout().Production.Recipes)
            // Only adjust crafts ABOVE the override
        {
            craft.ProductionTime = Math.Min(craft.ProductionTime.Value, overrideSeconds);
        }
    }

// Adjust all hideout craft times to be no higher than the override
    protected void AdjustHideoutBuildTimes(int overrideSeconds)
    {
        if (overrideSeconds == -1)
        {
            return;
        }

        foreach (var area in _databaseService.GetHideout().Areas)
        foreach (var (key, stage) in area.Stages)
            // Only adjust crafts ABOVE the override
        {
            stage.ConstructionTime = Math.Min(stage.ConstructionTime.Value, overrideSeconds);
        }
    }

    protected void UnlockHideoutLootCrateCrafts()
    {
        var hideoutLootBoxCraftIds = new List<string>
        {
            "66582be04de4820934746cea",
            "6745925da9c9adf0450d5bca",
            "67449c79268737ef6908d636"
        };

        foreach (var craftId in hideoutLootBoxCraftIds)
        {
            var recipe = _databaseService.GetHideout().Production.Recipes.FirstOrDefault(craft => craft.Id == craftId);
            if (recipe is not null)
            {
                recipe.Locked = false;
            }
        }
    }

// Blank out the "test" mail message from prapor
    protected void RemovePraporTestMessage()
    {
        // Iterate over all languages (e.g. "en", "fr")
        var locales = _databaseService.GetLocales();
        foreach (var localeKvP in locales.Global)
        {
            locales.Global[localeKvP.Key].Value["61687e2c3e526901fa76baf9"] = "";
        }
    }

// Check for any missing assorts inside each traders assort.json data, checking against traders questassort.json
    protected void ValidateQuestAssortUnlocksExist()
    {
        var db = _databaseService.GetTables();
        var traders = db.Traders;
        var quests = db.Templates.Quests;
        foreach (var (traderId, traderData) in traders)
        {
            var traderAssorts = traderData?.Assort;
            if (traderAssorts is null)
            {
                continue;
            }

            // Merge started/success/fail quest assorts into one dictionary
            var mergedQuestAssorts = new Dictionary<MongoId, string>();
            mergedQuestAssorts = mergedQuestAssorts.Concat(traderData.QuestAssort["started"])
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
                    var messageValues = new
                    {
                        traderName = traderId,
                        questName = quests[questKey]?.QuestName ?? "UNKNOWN"
                    };
                    _logger.Warning(
                        _localisationService.GetText("assort-missing_quest_assort_unlock", messageValues)
                    );
                }
            }
        }
    }

    protected void SetAllDbItemsAsSellableOnFlea()
    {
        var dbItems = _databaseService.GetItems().Values.ToList();
        foreach (var item in dbItems.Where(
                     item => string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase) &&
                             !item.Properties.CanSellOnRagfair.GetValueOrDefault(false) &&
                             !_ragfairConfig.Dynamic.Blacklist.Custom.Contains(item.Id)
                 ))
        {
            item.Properties.CanSellOnRagfair = true;
        }
    }

    protected void AddMissingTraderBuyRestrictionMaxValue()
    {
        var restrictions = _databaseService.GetGlobals().Configuration.TradingSettings.BuyRestrictionMaxBonus;
        restrictions["unheard_edition"] = new BuyRestrictionMaxBonus
        {
            Multiplier = restrictions["edge_of_darkness"].Multiplier
        };
    }

    protected void ApplyFleaPriceOverrides()
    {
        var fleaPrices = _databaseService.GetPrices();
        foreach (var (itemTpl, price) in _ragfairConfig.Dynamic.ItemPriceOverrideRouble)
        {
            fleaPrices[itemTpl] = price;
        }
    }

    protected void AddCustomItemPresetsToGlobals()
    {
        foreach (var presetToAdd in _itemConfig.CustomItemGlobalPresets)
        {
            if (_databaseService.GetGlobals().ItemPresets.ContainsKey(presetToAdd.Id))
            {
                _logger.Warning($"Global ItemPreset with Id of: {presetToAdd.Id} already exists, unable to overwrite");
                continue;
            }

            _databaseService.GetGlobals().ItemPresets.TryAdd(presetToAdd.Id, presetToAdd);
        }
    }
}
