using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocationLifecycleService(
    ISptLogger<LocationLifecycleService> logger,
    RewardHelper rewardHelper,
    ConfigServer configServer,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    ProfileHelper profileHelper,
    ProfileActivityService profileActivityService,
    BotNameService botNameService,
    ICloner cloner,
    RaidTimeAdjustmentService raidTimeAdjustmentService,
    LocationLootGenerator locationLootGenerator,
    ServerLocalisationService serverLocalisationService,
    BotLootCacheService botLootCacheService,
    LootGenerator lootGenerator,
    MailSendService mailSendService,
    TraderHelper traderHelper,
    RandomUtil randomUtil,
    InRaidHelper inRaidHelper,
    PlayerScavGenerator playerScavGenerator,
    SaveServer saveServer,
    HealthHelper healthHelper,
    PmcChatResponseService pmcChatResponseService,
    PmcWaveGenerator pmcWaveGenerator,
    QuestHelper questHelper,
    InsuranceService insuranceService,
    MatchBotDetailsCacheService matchBotDetailsCacheService,
    BtrDeliveryService btrDeliveryService
)
{
    protected readonly LocationConfig LocationConfig = configServer.GetConfig<LocationConfig>();
    protected readonly InRaidConfig InRaidConfig = configServer.GetConfig<InRaidConfig>();
    protected readonly TraderConfig TraderConfig = configServer.GetConfig<TraderConfig>();
    protected readonly RagfairConfig RagfairConfig = configServer.GetConfig<RagfairConfig>();
    protected readonly HideoutConfig HideoutConfig = configServer.GetConfig<HideoutConfig>();
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();
    protected readonly LostOnDeathConfig LostOnDeathConfig = configServer.GetConfig<LostOnDeathConfig>();

    protected const string Pmc = "pmc";
    protected const string Savage = "savage";
    protected const string Scav = "scav";

    /// <summary>
    /// Check player type for pmc or scav
    /// </summary>
    /// <param name="playerSide">string</param>
    /// <param name="sideCheck">What to check the bot against, default = PMC</param>
    /// <returns>bool</returns>
    protected internal bool IsSide(string playerSide, string sideCheck = Pmc)
    {
        return string.Equals(playerSide, sideCheck, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Handle client/match/local/start
    /// </summary>
    public virtual StartLocalRaidResponseData StartLocalRaid(MongoId sessionId, StartLocalRaidRequestData request)
    {
        logger.Debug($"Starting: {request.Location}");

        var playerProfile = profileHelper.GetFullProfile(sessionId);

        // Remove skill fatigue values
        ResetSkillPointsEarnedDuringRaid(
            IsSide(request.PlayerSide)
                ? playerProfile.CharacterData.PmcData.Skills.Common
                : playerProfile.CharacterData.ScavData.Skills.Common
        );

        // Raid is starting, adjust run times to reduce server load while player is in raid
        RagfairConfig.RunIntervalSeconds = RagfairConfig.RunIntervalValues.InRaid;
        HideoutConfig.RunIntervalSeconds = HideoutConfig.RunIntervalValues.InRaid;

        var result = new StartLocalRaidResponseData
        {
            // PVE_OFFLINE_xxxxxxxx_27_06_2025_20_20_44
            ServerId = $"{request.Location}.{request.PlayerSide} {timeUtil.GetTimeStamp()}", // Only used for metrics in client
            ServerSettings = databaseService.GetLocationServices(), // TODO - is this per map or global?
            Profile = new ProfileInsuredItems { InsuredItems = playerProfile.CharacterData.PmcData.InsuredItems },
            LocationLoot = GenerateLocationAndLoot(sessionId, request.Location, !request.ShouldSkipLootGeneration ?? true),
            TransitionType = TransitionType.NONE,
            Transition = new Transition
            {
                TransitionType = TransitionType.NONE,
                TransitionRaidId = new MongoId(),
                TransitionCount = 0,
                VisitedLocations = [],
            },
            ExcludedBosses = [],
        };

        // Only has value when transitioning into map from previous one
        if (request.Transition is not null)
        // TODO - why doesn't the raid after transit have any transit data?
        {
            result.Transition = request.Transition;
        }

        // Get data stored at end of previous raid (if any)
        var transitionData = profileActivityService.GetProfileActivityRaidData(sessionId)?.LocationTransit;

        if (transitionData is not null)
        {
            logger.Success($"Player: {sessionId} is in transit to {request.Location}");
            result.Transition.TransitionType = TransitionType.COMMON;
            result.Transition.TransitionRaidId = transitionData.TransitionRaidId;
            result.Transition.TransitionCount += 1;

            // Used by client to determine infil location - client adds the map player is transiting to later
            result.Transition.VisitedLocations.Add(transitionData.SptLastVisitedLocation);

            // Complete, clean up as no longer needed
            profileActivityService.GetProfileActivityRaidData(sessionId).LocationTransit = null;
        }

        // Apply changes from pmcConfig to bot hostility values
        AdjustBotHostilitySettings(result.LocationLoot);

        AdjustExtracts(request.PlayerSide, request.Location, result.LocationLoot);

        // Clear bot cache ready for bot generation call that occurs after this
        botNameService.ClearNameCache();

        // Handle Player Inventory Wiping checks for alt-f4 prevention
        HandlePreRaidInventoryChecks(request.PlayerSide, playerProfile.CharacterData.PmcData, sessionId);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        return result;
    }

    /// <summary>
    /// Handle Pre Raid checks Alt-F4 Prevention and player inventory wiping
    /// </summary>
    protected void HandlePreRaidInventoryChecks(string playerSide, PmcData pmcData, MongoId sessionId)
    {
        // If config enabled, remove players equipped items to prevent alt-F4 from persisting items
        if (!IsSide(playerSide) || !LostOnDeathConfig.WipeOnRaidStart)
        {
            return;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug("Wiping player inventory on raid start to prevent alt-f4");
        }

        inRaidHelper.DeleteInventory(pmcData, sessionId);
    }

    /// <summary>
    ///     Replace map exits with scav exits when player is scavving
    /// </summary>
    /// <param name="playerSide"> Players side (savage/usec/bear) </param>
    /// <param name="location"> ID of map being loaded </param>
    /// <param name="locationData"> Maps location base data </param>
    protected void AdjustExtracts(string playerSide, string location, LocationBase locationData)
    {
        var playerIsScav = IsSide(playerSide, Savage);
        if (!playerIsScav)
        {
            return;
        }

        // Get relevant extract data for map
        var mapExtracts = databaseService.GetLocation(location)?.AllExtracts;
        if (mapExtracts is null)
        {
            logger.Warning($"Unable to find map: {location} extract data, no adjustments made");

            return;
        }

        // Find only scav extracts and overwrite existing exits with them
        var scavExtracts = mapExtracts.Where(extract => IsSide(extract.Side, Scav));
        if (scavExtracts.Any())
        // Scav extracts found, use them
        {
            locationData.Exits = locationData.Exits.Union(scavExtracts);
        }
    }

    /// <summary>
    ///     Adjust the bot hostility values prior to entering a raid
    /// </summary>
    /// <param name="location"> Map to adjust values of </param>
    protected void AdjustBotHostilitySettings(LocationBase location)
    {
        foreach (var botId in PMCConfig.HostilitySettings)
        {
            var configHostilityChanges = PMCConfig.HostilitySettings[botId.Key];
            var locationBotHostilityDetails = location.BotLocationModifier.AdditionalHostilitySettings.FirstOrDefault(botSettings =>
                string.Equals(botSettings.BotRole, botId.Key, StringComparison.OrdinalIgnoreCase)
            );

            // No matching bot in config, skip
            if (locationBotHostilityDetails is null)
            {
                logger.Warning($"No bot: {botId} hostility values found on: {location.Id}, can only edit existing. Skipping");

                continue;
            }

            // Add new permanent enemies if they don't already exist
            if (configHostilityChanges.AdditionalEnemyTypes is not null)
            {
                foreach (var enemyTypeToAdd in configHostilityChanges.AdditionalEnemyTypes)
                {
                    locationBotHostilityDetails.AlwaysEnemies.Add(enemyTypeToAdd);
                }
            }

            // Add/edit chance settings
            if (configHostilityChanges.ChancedEnemies is not null)
            {
                locationBotHostilityDetails.ChancedEnemies = [];
                foreach (var chanceDetailsToApply in configHostilityChanges.ChancedEnemies)
                {
                    var locationBotDetails = locationBotHostilityDetails.ChancedEnemies.FirstOrDefault(botChance =>
                        botChance.Role == chanceDetailsToApply.Role
                    );
                    if (locationBotDetails is not null)
                    // Existing
                    {
                        locationBotDetails.EnemyChance = chanceDetailsToApply.EnemyChance;
                    }
                    else
                    // Add new
                    {
                        locationBotHostilityDetails.ChancedEnemies.Add(chanceDetailsToApply);
                    }
                }
            }

            // Add new permanent friends if they don't already exist
            if (configHostilityChanges.AdditionalFriendlyTypes is not null)
            {
                locationBotHostilityDetails.AlwaysFriends = [];
                foreach (var friendlyTypeToAdd in configHostilityChanges.AdditionalFriendlyTypes)
                {
                    locationBotHostilityDetails.AlwaysFriends.Add(friendlyTypeToAdd);
                }
            }

            // Adjust vs bear hostility chance
            if (configHostilityChanges.BearEnemyChance is not null)
            {
                locationBotHostilityDetails.BearEnemyChance = configHostilityChanges.BearEnemyChance;
            }

            // Adjust vs usec hostility chance
            if (configHostilityChanges.UsecEnemyChance is not null)
            {
                locationBotHostilityDetails.UsecEnemyChance = configHostilityChanges.UsecEnemyChance;
            }

            // Adjust vs savage hostility chance
            if (configHostilityChanges.SavageEnemyChance is not null)
            {
                locationBotHostilityDetails.SavageEnemyChance = configHostilityChanges.SavageEnemyChance;
            }

            // Adjust vs scav hostility behaviour
            if (configHostilityChanges.SavagePlayerBehaviour is not null)
            {
                locationBotHostilityDetails.SavagePlayerBehaviour = configHostilityChanges.SavagePlayerBehaviour;
            }
        }
    }

    /// <summary>
    ///     Generate a maps base location (cloned) and loot
    /// </summary>
    /// <param name="sessionId"> Session/Player id </param>
    /// <param name="name"> Map name </param>
    /// <param name="generateLoot"> OPTIONAL - Should loot be generated for the map before being returned </param>
    /// <returns>LocationBase with loot</returns>
    public virtual LocationBase GenerateLocationAndLoot(MongoId sessionId, string name, bool generateLoot = true)
    {
        var location = databaseService.GetLocation(name);
        var locationBaseClone = cloner.Clone(location.Base);

        // Update datetime property to now
        locationBaseClone.UnixDateTime = timeUtil.GetTimeStamp();

        // Don't generate loot for hideout
        if (string.Equals(name, "hideout", StringComparison.OrdinalIgnoreCase))
        {
            return locationBaseClone;
        }

        // Only requested base data, not loot
        if (!generateLoot)
        {
            return locationBaseClone;
        }

        // Add custom PMCs to map every time its run
        pmcWaveGenerator.ApplyWaveChangesToMap(locationBaseClone);

        // Adjust raid values based raid type (e.g. Scav or PMC)
        LocationConfig? locationConfigClone = null;
        var raidAdjustments = profileActivityService.GetProfileActivityRaidData(sessionId)?.RaidAdjustments;
        if (raidAdjustments is not null)
        {
            locationConfigClone = cloner.Clone(LocationConfig); // Clone values so they can be used to reset originals later
            raidTimeAdjustmentService.MakeAdjustmentsToMap(raidAdjustments, locationBaseClone);
        }

        // Generate loot for location
        locationBaseClone.Loot = locationLootGenerator.GenerateLocationLoot(name);

        // Reset loot multipliers back to original values
        if (raidAdjustments is not null && locationConfigClone is not null)
        {
            logger.Debug("Resetting loot multipliers back to their original values");
            LocationConfig.StaticLootMultiplier = locationConfigClone.StaticLootMultiplier;
            LocationConfig.LooseLootMultiplier = locationConfigClone.LooseLootMultiplier;

            profileActivityService.GetProfileActivityRaidData(sessionId).RaidAdjustments = null;
        }

        return locationBaseClone;
    }

    /// <summary>
    ///     Handle client/match/local/end
    /// </summary>
    public virtual void EndLocalRaid(MongoId sessionId, EndLocalRaidRequestData request)
    {
        // Clear bot loot cache
        botLootCacheService.ClearCache();

        var fullProfile = profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile.CharacterData.PmcData;
        var scavProfile = fullProfile.CharacterData.ScavData;

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Raid: {request.ServerId} outcome: {request.Results.Result}");
        }

        // Reset flea interval time to out-of-raid value
        RagfairConfig.RunIntervalSeconds = RagfairConfig.RunIntervalValues.OutOfRaid;
        HideoutConfig.RunIntervalSeconds = HideoutConfig.RunIntervalValues.OutOfRaid;

        // ServerId has various info stored in it, delimited by a period
        var serverDetails = request.ServerId.Split(".");

        var locationName = serverDetails[0].ToLowerInvariant();
        var isPmc = serverDetails[1].ToLowerInvariant().Contains("pmc");
        var isDead = request.Results.IsPlayerDead();
        var isTransfer = request.Results.IsMapToMapTransfer();
        var isSurvived = request.Results.IsPlayerSurvived();

        // Handle items transferred via BTR or transit to player mailbox
        btrDeliveryService.HandleItemTransferEvent(sessionId, request);

        // Player is moving between maps
        if (isTransfer && request.LocationTransit is not null)
        {
            // Manually store the map player just left
            request.LocationTransit.SptLastVisitedLocation = locationName;
            // TODO - Persist each players last visited location history over multiple transits, e.g. using InMemoryCacheService, need to take care to not let data get stored forever
            // Store transfer data for later use in `startLocalRaid()` when next raid starts
            request.LocationTransit.SptExitName = request.Results.ExitName;
            profileActivityService.GetProfileActivityRaidData(sessionId).LocationTransit = request.LocationTransit;
        }

        if (!isPmc)
        {
            HandlePostRaidPlayerScav(sessionId, pmcProfile, scavProfile, isDead, isTransfer, isSurvived, request);

            return;
        }

        HandlePostRaidPmc(sessionId, fullProfile, scavProfile, isDead, isSurvived, isTransfer, request, locationName);

        // Handle car extracts
        if (request.Results.TookCarExtract(InRaidConfig.CarExtracts))
        {
            HandleCarExtract(request.Results.ExitName, pmcProfile, sessionId);
        }

        // Handle coop exit
        if (request.Results.TookCoopExtract(InRaidConfig.CoopExtracts) && TraderConfig.Fence.CoopExtractGift.SendGift)
        {
            HandleCoopExtract(sessionId, pmcProfile, request.Results.ExitName);
            SendCoopTakenFenceMessage(sessionId);
        }
    }

    /// <summary>
    /// After taking a COOP extract, send player a gift via mail
    /// </summary>
    /// <param name="sessionId">Player/Session id</param>
    protected void SendCoopTakenFenceMessage(MongoId sessionId)
    {
        // Generate randomised reward for taking coop extract
        var loot = lootGenerator.CreateRandomLoot(TraderConfig.Fence.CoopExtractGift);

        var parentId = new MongoId();
        foreach (var itemAndChildren in loot)
        {
            // Set all root items parent to new id
            itemAndChildren.FirstOrDefault().ParentId = parentId;
        }

        // Flatten
        IEnumerable<Item> mailableLoot = [.. loot.SelectMany(x => x)];

        // Send message from fence giving player reward generated above
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            Traders.FENCE,
            MessageType.MessageWithItems,
            randomUtil.GetArrayValue(TraderConfig.Fence.CoopExtractGift.MessageLocaleIds),
            mailableLoot,
            timeUtil.GetHoursAsSeconds(TraderConfig.Fence.CoopExtractGift.GiftExpiryHours)
        );
    }

    /// <summary>
    ///     Handle when a player extracts using a car - Add rep to fence
    /// </summary>
    /// <param name="extractName"> Name of the extract used </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="sessionId"> Session ID </param>
    protected void HandleCarExtract(string extractName, PmcData pmcData, MongoId sessionId)
    {
        pmcData.CarExtractCounts?.TryAdd(extractName, 0);

        // Increment extract count value
        pmcData.CarExtractCounts[extractName] += 1;

        var newFenceStanding = GetFenceStandingAfterExtract(
            pmcData,
            InRaidConfig.CarExtractBaseStandingGain,
            pmcData.CarExtractCounts[extractName]
        );

        var fenceId = Traders.FENCE;
        pmcData.TradersInfo[fenceId].Standing = newFenceStanding;

        // Check if new standing has leveled up trader
        traderHelper.LevelUp(fenceId, pmcData);
        pmcData.TradersInfo[fenceId].LoyaltyLevel = Math.Max((int)pmcData.TradersInfo[fenceId].LoyaltyLevel, 1);

        logger.Debug($"Car extract: {extractName} used, total times taken: {pmcData.CarExtractCounts[extractName]}");

        // Copy updated fence rep values into scav profile to ensure consistency
        var scavData = profileHelper.GetScavProfile(sessionId);
        scavData.TradersInfo[fenceId].Standing = pmcData.TradersInfo[fenceId].Standing;
        scavData.TradersInfo[fenceId].LoyaltyLevel = pmcData.TradersInfo[fenceId].LoyaltyLevel;
    }

    /// <summary>
    ///     Handle when a player extracts using a coop extract - add rep to fence
    /// </summary>
    /// <param name="sessionId"> Session/player id </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="extractName"> Name of extract taken </param>
    protected void HandleCoopExtract(MongoId sessionId, PmcData pmcData, string extractName)
    {
        pmcData.CoopExtractCounts?.TryAdd(extractName, 0);

        pmcData.CoopExtractCounts[extractName] += 1;

        var newFenceStanding = GetFenceStandingAfterExtract(
            pmcData,
            InRaidConfig.CoopExtractBaseStandingGain,
            pmcData.CoopExtractCounts[extractName]
        );

        var fenceId = Traders.FENCE;
        pmcData.TradersInfo[fenceId].Standing = newFenceStanding;

        // Check if new standing has leveled up trader
        traderHelper.LevelUp(fenceId, pmcData);
        pmcData.TradersInfo[fenceId].LoyaltyLevel = Math.Max((int)pmcData.TradersInfo[fenceId].LoyaltyLevel, 1);

        logger.Debug($"COOP extract: {extractName} used");

        // Copy updated fence rep values into scav profile to ensure consistency
        var scavData = profileHelper.GetScavProfile(sessionId);
        scavData.TradersInfo[fenceId].Standing = pmcData.TradersInfo[fenceId].Standing;
        scavData.TradersInfo[fenceId].LoyaltyLevel = pmcData.TradersInfo[fenceId].LoyaltyLevel;
    }

    /// <summary>
    ///     Get the fence rep gain from using a car or coop extract
    /// </summary>
    /// <param name="pmcData"> Profile </param>
    /// <param name="baseGain"> Amount gained for the first extract </param>
    /// <param name="extractCount"> Number of times extract was taken </param>
    /// <returns> Fence standing after taking extract </returns>
    protected double GetFenceStandingAfterExtract(PmcData pmcData, double baseGain, double extractCount)
    {
        var fenceId = Traders.FENCE;
        var fenceStanding = pmcData.TradersInfo[fenceId].Standing;

        // get standing after taking extract x times, x.xx format, gain from extract can be no smaller than 0.01
        fenceStanding += Math.Max(baseGain / extractCount, 0.01);

        // Ensure fence loyalty level is not above/below the range -7 to 15
        var fenceMax = TraderConfig.Fence.PlayerRepMax;
        var fenceMin = TraderConfig.Fence.PlayerRepMin;
        var newFenceStanding = Math.Clamp(fenceStanding.GetValueOrDefault(0), fenceMin, fenceMax);
        logger.Debug($"Old vs new fence standing: {pmcData.TradersInfo[fenceId].Standing}, {newFenceStanding}");

        return Math.Round(newFenceStanding, 2);
    }

    /// <summary>
    /// Perform post-raid profile changes
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <param name="pmcProfile">Players PMC profile</param>
    /// <param name="scavProfile">Players scav profile</param>
    /// <param name="isDead">Did player die</param>
    /// <param name="isTransfer">Did player transfer to new map</param>
    /// <param name="isSurvived">DId player get 'survived' exit status</param>
    /// <param name="request">End raid request</param>
    protected void HandlePostRaidPlayerScav(
        MongoId sessionId,
        PmcData pmcProfile,
        PmcData scavProfile,
        bool isDead,
        bool isTransfer,
        bool isSurvived,
        EndLocalRaidRequestData request
    )
    {
        var postRaidProfile = request.Results.Profile;

        if (isTransfer || request.Results.Result == ExitStatus.RUNNER)
        {
            // Transfer over hp and effects - not necessary for runthroughs, but it causes no issues
            scavProfile.Health = postRaidProfile.Health;

            // Adjust limb hp and effects while transiting
            UpdateLimbValuesAfterTransit(scavProfile.Health);

            // We want scav inventory to persist into next raid when pscav is moving between maps
            // Also adjust FiR status when exit was runthrough
            inRaidHelper.SetInventory(sessionId, scavProfile, postRaidProfile, isSurvived, isTransfer);
        }

        scavProfile.Info.Level = postRaidProfile.Info.Level;
        scavProfile.Skills = postRaidProfile.Skills;
        scavProfile.Stats = postRaidProfile.Stats;
        scavProfile.Encyclopedia = postRaidProfile.Encyclopedia;
        scavProfile.TaskConditionCounters = postRaidProfile.TaskConditionCounters;
        scavProfile.SurvivorClass = postRaidProfile.SurvivorClass;

        // Scavs don't have achievements, but copy anyway
        scavProfile.Achievements = postRaidProfile.Achievements;

        scavProfile.Info.Experience = postRaidProfile.Info.Experience;

        // Must occur after experience is set and stats copied over
        scavProfile.Stats.Eft.TotalSessionExperience = 0;

        ApplyTraderStandingAdjustments(scavProfile.TradersInfo, postRaidProfile.TradersInfo);

        // Clamp fence standing within -7 to 15 range
        var fenceMax = TraderConfig.Fence.PlayerRepMax; // 15
        var fenceMin = TraderConfig.Fence.PlayerRepMin; //-7
        if (!postRaidProfile.TradersInfo.TryGetValue(Traders.FENCE, out var postRaidFenceData))
        {
            logger.Error($"post raid fence data not found for: {sessionId}");
        }

        scavProfile.TradersInfo[Traders.FENCE].Standing = Math.Clamp(postRaidFenceData.Standing.Value, fenceMin, fenceMax);

        // Successful extract as scav, give some rep
        if (request.Results.IsPlayerSurvived() && scavProfile.TradersInfo[Traders.FENCE].Standing < fenceMax)
        {
            scavProfile.TradersInfo[Traders.FENCE].Standing += InRaidConfig.ScavExtractStandingGain;
        }

        // Copy scav fence values to PMC profile
        pmcProfile.TradersInfo[Traders.FENCE] = scavProfile.TradersInfo[Traders.FENCE];

        if (scavProfile.ProfileHasConditionCounters())
        // Scav quest progress needs to be moved to pmc so player can see it in menu / hand them in
        {
            MigrateScavQuestProgressToPmcProfile(scavProfile, pmcProfile);
        }

        // Must occur after encyclopedia updated
        MergePmcAndScavEncyclopedias(scavProfile, pmcProfile);

        // Scav died, regen scav loadout and reset timer
        if (isDead)
        {
            playerScavGenerator.Generate(sessionId);
        }

        // Update last played property
        pmcProfile.Info.LastTimePlayedAsSavage = timeUtil.GetTimeStamp();

        // Force a profile save
        saveServer.SaveProfileAsync(sessionId);
    }

    /// <summary>
    ///     Scav quest progress isn't transferred automatically from scav to pmc, we do this manually
    /// </summary>
    /// <param name="scavProfile"> Scav profile with quest progress post-raid </param>
    /// <param name="pmcProfile"> Server pmc profile to copy scav quest progress into </param>
    protected void MigrateScavQuestProgressToPmcProfile(PmcData scavProfile, PmcData pmcProfile)
    {
        foreach (var scavQuest in scavProfile.Quests)
        {
            var pmcQuest = pmcProfile.Quests.FirstOrDefault(quest => quest.QId == scavQuest.QId);
            if (pmcQuest is null)
            {
                logger.Warning(serverLocalisationService.GetText("inraid-unable_to_migrate_pmc_quest_not_found_in_profile", scavQuest.QId));
                continue;
            }

            // Get counters related to scav quest
            var matchingCounters = scavProfile.TaskConditionCounters.Where(counter => counter.Value.SourceId == scavQuest.QId);

            if (matchingCounters is null)
            {
                continue;
            }

            // insert scav quest counters into pmc profile
            foreach (var counter in matchingCounters)
            {
                pmcProfile.TaskConditionCounters[counter.Value.Id.Value] = counter.Value;
            }

            // Find Matching PMC Quest
            // Update Status and StatusTimer properties
            pmcQuest.Status = scavQuest.Status;
            pmcQuest.StatusTimers = scavQuest.StatusTimers;
        }
    }

    /// <summary>
    /// Slightly fix broken limbs and remove effects
    /// </summary>
    /// <param name="profileHealth">Profile health data to adjust</param>
    protected void UpdateLimbValuesAfterTransit(BotBaseHealth? profileHealth)
    {
        var transitSettings = LocationConfig.TransitSettings;
        if (transitSettings == null)
        {
            logger.Warning("Unable to find: _locationConfig.TransitSettings");

            return;
        }

        // Check each body part
        foreach (var (_, hpValues) in profileHealth.BodyParts)
        {
            if (transitSettings.AdjustLimbHealthPoints.GetValueOrDefault() && hpValues.Health.Minimum <= 0)
            {
                // Limb has been destroyed, reset
                hpValues.Health.Current = randomUtil.GetPercentOfValue(
                    transitSettings.LimbHealPercent.GetValueOrDefault(30),
                    hpValues.Health.Maximum.Value
                );
            }

            if (!(hpValues.Effects?.Count > 0))
            {
                // No effects on limb, skip
                continue;
            }

            // Limb has effects, check for blacklisted values and remove
            var keysToRemove = hpValues.Effects.Keys.Where(key => transitSettings.EffectsToRemove.Contains(key)).ToHashSet();

            foreach (var key in keysToRemove)
            {
                hpValues.Effects.Remove(key);
            }
        }
    }

    /// <summary>
    ///     Handles PMC Profile after the raid
    /// </summary>
    /// <param name="sessionId"> Player id </param>
    /// <param name="fullServerProfile"> Pmc profile from server</param>
    /// <param name="scavProfile"> Scav profile </param>
    /// <param name="isDead"> Player died/got left behind in raid </param>
    /// <param name="isSurvived"> Not same as opposite of `isDead`, specific status </param>
    /// <param name="isTransfer"> Player transferred to another map </param>
    /// <param name="request"> Client request data </param>
    /// <param name="locationName"> Current finished Raid location </param>
    protected void HandlePostRaidPmc(
        MongoId sessionId,
        SptProfile fullServerProfile,
        PmcData scavProfile,
        bool isDead,
        bool isSurvived,
        bool isTransfer,
        EndLocalRaidRequestData request,
        string locationName
    )
    {
        var serverPmcProfile = fullServerProfile.CharacterData.PmcData;
        var postRaidProfile = request.Results.Profile;
        var preRaidProfileQuestDataClone = cloner.Clone(serverPmcProfile.Quests);

        // MUST occur BEFORE inventory actions (setInventory()) occur
        // Player died, get quest items they lost for use later
        var lostQuestItems = postRaidProfile.GetQuestItemsInProfile();

        // Update inventory
        inRaidHelper.SetInventory(sessionId, serverPmcProfile, postRaidProfile, isSurvived, isTransfer);

        serverPmcProfile.Info.Level = postRaidProfile.Info.Level;
        serverPmcProfile.Skills = postRaidProfile.Skills;
        serverPmcProfile.Stats.Eft = postRaidProfile.Stats.Eft;
        serverPmcProfile.Encyclopedia = postRaidProfile.Encyclopedia;
        serverPmcProfile.TaskConditionCounters = postRaidProfile.TaskConditionCounters;
        serverPmcProfile.SurvivorClass = postRaidProfile.SurvivorClass;

        // MUST occur prior to profile achievements being overwritten by post-raid achievements
        ProcessAchievementRewards(fullServerProfile, postRaidProfile.Achievements);

        // MUST occur AFTER ProcessAchievementRewards()
        serverPmcProfile.Achievements = postRaidProfile.Achievements;
        serverPmcProfile.Quests = ProcessPostRaidQuests(postRaidProfile.Quests);

        // MUST occur AFTER processPostRaidQuests()
        LightkeeperQuestWorkaround(sessionId, postRaidProfile.Quests, preRaidProfileQuestDataClone, serverPmcProfile);

        serverPmcProfile.WishList = postRaidProfile.WishList;

        serverPmcProfile.Variables = postRaidProfile.Variables;

        serverPmcProfile.Info.Experience = postRaidProfile.Info.Experience;

        ApplyTraderStandingAdjustments(serverPmcProfile.TradersInfo, postRaidProfile.TradersInfo);

        // Must occur AFTER experience is set and stats copied over
        serverPmcProfile.Stats.Eft.TotalSessionExperience = 0;

        var fenceId = Traders.FENCE;

        // Clamp fence standing
        var fenceMax = TraderConfig.Fence.PlayerRepMax; // 15
        var fenceMin = TraderConfig.Fence.PlayerRepMin; //-7

        serverPmcProfile.TradersInfo[fenceId].Standing = Math.Clamp(
            postRaidProfile.TradersInfo[fenceId].Standing ?? 0d,
            fenceMin,
            fenceMax
        );

        // Copy fence values to Scav
        scavProfile.TradersInfo[fenceId] = serverPmcProfile.TradersInfo[fenceId];

        // MUST occur AFTER encyclopedia updated
        MergePmcAndScavEncyclopedias(serverPmcProfile, scavProfile);

        // Handle temp, hydration, limb hp/effects
        healthHelper.ApplyHealthChangesToProfile(sessionId, serverPmcProfile, postRaidProfile.Health);

        // Required when player loses limb in-raid and fixes it, max now stuck at 50% or less if lost multiple times
        var profileTemplate = profileHelper.GetProfileTemplateForSide(fullServerProfile.ProfileInfo.Edition, serverPmcProfile.Info.Side);
        serverPmcProfile.ResetMaxLimbHp(profileTemplate);

        if (isTransfer)
        {
            // Adjust limb hp and effects while transiting
            UpdateLimbValuesAfterTransit(serverPmcProfile.Health);
        }

        // This must occur _BEFORE_ `deleteInventory`, as that method clears insured items
        HandleInsuredItemLostEvent(sessionId, serverPmcProfile, request, locationName);

        if (isDead)
        {
            if (lostQuestItems.Any())
            // MUST occur AFTER quests have post raid quest data has been merged "processPostRaidQuests()"
            // Player is dead + had quest items, check and fix any broken find item quests
            {
                CheckForAndFixPickupQuestsAfterDeath(sessionId, lostQuestItems, serverPmcProfile.Quests);
            }

            if (postRaidProfile.Stats.Eft.Aggressor is not null)
            {
                // get the aggressor ID from the client request body
                postRaidProfile.Stats.Eft.Aggressor.ProfileId = request.Results.KillerId;
                pmcChatResponseService.SendKillerResponse(sessionId, serverPmcProfile, postRaidProfile.Stats.Eft.Aggressor);
            }

            inRaidHelper.DeleteInventory(serverPmcProfile, sessionId);

            serverPmcProfile.RemoveFiRStatusFromItemsInContainer("SecuredContainer");
        }

        // Must occur AFTER killer messages have been sent
        matchBotDetailsCacheService.ClearCache();

        var roles = new HashSet<string> { "pmcbear", "pmcusec" };

        var victims = postRaidProfile.Stats.Eft.Victims.Where(victim => roles.Contains(victim.Role.ToLowerInvariant()));
        if (victims is not null && victims.Any())
        // Player killed PMCs, send some mail responses to them
        {
            pmcChatResponseService.SendVictimResponse(sessionId, victims, serverPmcProfile);
        }
    }

    /// <summary>
    ///     On death Quest items are lost, the client does not clean up completed conditions for picking up those quest items,
    ///     If the completed conditions remain in the profile the player is unable to pick the item up again
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="lostQuestItems"> Quest items lost on player death </param>
    /// <param name="profileQuests"> Quest status data from player profile </param>
    protected void CheckForAndFixPickupQuestsAfterDeath(
        MongoId sessionId,
        IEnumerable<Item> lostQuestItems,
        IEnumerable<QuestStatus> profileQuests
    )
    {
        // Exclude completed quests
        var activeQuestIdsInProfile = profileQuests
            .Where(quest => quest.Status is not QuestStatusEnum.AvailableForStart and not QuestStatusEnum.Success)
            .Select(status => status.QId)
            .ToHashSet();

        // Get db details of quests we found above
        var questDb = databaseService.GetQuests().Values.Where(quest => activeQuestIdsInProfile.Contains(quest.Id));

        foreach (var lostItem in lostQuestItems)
        {
            var matchingConditionId = string.Empty;
            // Find a quest that has a FindItem condition that has the list items tpl as a target
            var matchingQuests = questDb
                .Where(quest =>
                {
                    var matchingCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(questCondition =>
                        questCondition.ConditionType == "FindItem"
                        && (questCondition.Target.IsList ? questCondition.Target.List : [questCondition.Target.Item]).Contains(
                            lostItem.Template
                        )
                    );
                    if (matchingCondition is null)
                    // Quest doesnt have a matching condition
                    {
                        return false;
                    }

                    // We found a condition, save id for later
                    matchingConditionId = matchingCondition.Id;
                    return true;
                })
                .ToList();

            // Fail if multiple were found
            if (matchingQuests.Count != 1)
            {
                logger.Error($"Unable to fix quest item: {lostItem}, {matchingQuests.Count} matching quests found, expected 1");

                continue;
            }

            var matchingQuest = matchingQuests[0];
            // We have a match, remove the condition id from profile to reset progress and let player pick item up again
            var profileQuestToUpdate = profileQuests.FirstOrDefault(questStatus => questStatus.QId == matchingQuest.Id);
            if (profileQuestToUpdate is null)
            // Profile doesn't have a matching quest
            {
                continue;
            }

            // Filter out the matching condition we found
            profileQuestToUpdate.CompletedConditions = profileQuestToUpdate
                .CompletedConditions.Where(conditionId => conditionId != matchingConditionId)
                .ToList();
        }
    }

    /// <summary>
    ///     In 0.15 Lightkeeper quests do not give rewards in PvE, this issue also occurs in spt.
    ///     We check for newly completed Lk quests and run them through the servers `CompleteQuest` process.
    ///     This rewards players with items + craft unlocks + new trader assorts.
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="postRaidQuests"> Quest statuses post-raid </param>
    /// <param name="preRaidQuests"> Quest statuses pre-raid </param>
    /// <param name="pmcProfile"> Players profile </param>
    protected void LightkeeperQuestWorkaround(
        MongoId sessionId,
        List<QuestStatus> postRaidQuests,
        List<QuestStatus> preRaidQuests,
        PmcData pmcProfile
    )
    {
        // LK quests that were not completed before raid but now are
        var newlyCompletedLightkeeperQuests = postRaidQuests.Where(postRaidQuest =>
            postRaidQuest.Status == QuestStatusEnum.Success
            && // Quest is complete
            preRaidQuests.Any(preRaidQuest =>
                preRaidQuest.QId == postRaidQuest.QId
                && // Get matching pre-raid quest
                preRaidQuest.Status != QuestStatusEnum.Success
            )
            && // Completed quest was not completed before raid started
            databaseService.GetQuests().TryGetValue(postRaidQuest.QId, out var quest)
            && quest?.TraderId == Traders.LIGHTHOUSEKEEPER
        ); // Quest is from LK

        // Run server complete quest process to ensure player gets rewards
        foreach (var questToComplete in newlyCompletedLightkeeperQuests)
        {
            questHelper.CompleteQuest(
                pmcProfile,
                new CompleteQuestRequestData
                {
                    Action = "CompleteQuest",
                    QuestId = questToComplete.QId,
                    RemoveExcessItems = false,
                },
                sessionId
            );
        }
    }

    /// <summary>
    ///     Convert post-raid quests into correct format.
    ///     Quest status comes back as a string version of the enum `Success`, not the expected value of 1.
    /// </summary>
    /// <param name="questsToProcess"> Quests data from client </param>
    /// <returns> List of adjusted QuestStatus post-raid </returns>
    protected List<QuestStatus> ProcessPostRaidQuests(List<QuestStatus> questsToProcess)
    {
        var failedQuests = questsToProcess.Where(quest => quest.Status == QuestStatusEnum.MarkedAsFailed);
        foreach (var failedQuest in failedQuests)
        {
            if (!databaseService.GetQuests().TryGetValue(failedQuest.QId, out var dbQuest))
            {
                continue;
            }

            // Handle this somewhat close to QuestClass.SetStatus in the client
            failedQuest.Status = dbQuest.Restartable ? QuestStatusEnum.FailRestartable : QuestStatusEnum.Fail;
        }

        return questsToProcess;
    }

    /// <summary>
    ///     Adjust server trader settings if they differ from data sent by client
    /// </summary>
    /// <param name="tradersServerProfile"> Server </param>
    /// <param name="tradersClientProfile"> Client </param>
    protected void ApplyTraderStandingAdjustments(
        Dictionary<MongoId, TraderInfo>? tradersServerProfile,
        Dictionary<MongoId, TraderInfo>? tradersClientProfile
    )
    {
        foreach (var traderId in tradersClientProfile)
        {
            var serverProfileTrader = tradersServerProfile.FirstOrDefault(x => x.Key == traderId.Key).Value;
            var clientProfileTrader = tradersClientProfile.FirstOrDefault(x => x.Key == traderId.Key).Value;
            if (serverProfileTrader is null || clientProfileTrader is null)
            {
                continue;
            }

            if (clientProfileTrader.Standing != serverProfileTrader.Standing)
            // Difference found, update server profile with values from client profile
            {
                tradersServerProfile[traderId.Key].Standing = clientProfileTrader.Standing;
            }
        }
    }

    protected void HandleInsuredItemLostEvent(
        MongoId sessionId,
        PmcData preRaidPmcProfile,
        EndLocalRaidRequestData request,
        string locationName
    )
    {
        if (request.LostInsuredItems is not null && request.LostInsuredItems.Any())
        {
            var mappedItems = insuranceService.MapInsuredItemsToTrader(sessionId, request.LostInsuredItems, preRaidPmcProfile);

            // Is possible to have items in lostInsuredItems but removed before reaching mappedItems
            if (mappedItems.Count == 0)
            {
                return;
            }

            insuranceService.StoreGearLostInRaidToSendLater(sessionId, mappedItems);

            insuranceService.StartPostRaidInsuranceLostProcess(preRaidPmcProfile, sessionId, locationName);
        }
    }

    /// <summary>
    ///     Reset the skill points earned in a raid to 0, ready for next raid
    /// </summary>
    /// <param name="commonSkills"> Profile common skills to update </param>
    protected void ResetSkillPointsEarnedDuringRaid(IEnumerable<CommonSkill> commonSkills)
    {
        foreach (var skill in commonSkills)
        {
            skill.PointsEarnedDuringSession = 0;
        }
    }

    /// <summary>
    ///     Merge two dictionaries together.
    ///     Prioritise pair that has true as a value
    /// </summary>
    /// <param name="primary"> Main dictionary </param>
    /// <param name="secondary"> Secondary dictionary </param>
    protected void MergePmcAndScavEncyclopedias(PmcData primary, PmcData secondary)
    {
        var mergedDicts = primary
            .Encyclopedia?.UnionBy(secondary.Encyclopedia, kvp => kvp.Key)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(g => g.Key, g => g.Any(kvp => kvp.Value));

        primary.Encyclopedia = mergedDicts;
        secondary.Encyclopedia = mergedDicts;
    }

    /// <summary>
    ///     Check for and add any rewards found via the gained achievements this raid
    /// </summary>
    /// <param name="fullProfile"> Profile to add customisations to </param>
    /// <param name="postRaidAchievements"> All profile achievements at the end of a raid </param>
    protected void ProcessAchievementRewards(SptProfile fullProfile, Dictionary<MongoId, long>? postRaidAchievements)
    {
        var sessionId = fullProfile.ProfileInfo.ProfileId;
        var pmcProfile = fullProfile.CharacterData.PmcData;
        var preRaidAchievementIds = fullProfile.CharacterData.PmcData.Achievements;
        var postRaidAchievementIds = postRaidAchievements;
        var achievementIdsAcquiredThisRaid = postRaidAchievementIds.Where(id => !preRaidAchievementIds.Contains(id));

        // Get achievement data from db
        var achievementsDb = databaseService.GetTemplates().Achievements;

        // Map the achievement ids player obtained in raid with matching achievement data from db
        var achievements = achievementIdsAcquiredThisRaid.Select(achievementId =>
            achievementsDb.FirstOrDefault(achievementDb => achievementDb.Id == achievementId.Key)
        );
        if (achievements is null)
        // No achievements found
        {
            return;
        }

        foreach (var achievement in achievements)
        {
            var rewardItems = rewardHelper.ApplyRewards(
                achievement.Rewards,
                CustomisationSource.ACHIEVEMENT,
                fullProfile,
                pmcProfile,
                achievement.Id
            );

            if (rewardItems?.Count > 0)
            {
                mailSendService.SendLocalisedSystemMessageToPlayer(
                    sessionId.Value,
                    "670547bb5fa0b1a7c30d5836 0",
                    rewardItems,
                    [],
                    timeUtil.GetHoursAsSeconds(24 * 7)
                );
            }
        }
    }
}
