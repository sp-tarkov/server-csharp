using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Location;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class LocationLifecycleService
{
    protected ApplicationContext _applicationContext;
    protected BotGenerationCacheService _botGenerationCacheService;
    protected BotLootCacheService _botLootCacheService;
    protected BotNameService _botNameService;
    protected ICloner _cloner;
    protected ConfigServer _configServer;
    protected DatabaseService _databaseService;
    protected HashUtil _hashUtil;
    protected HealthHelper _healthHelper;
    protected HideoutConfig _hideoutConfig;
    protected InRaidConfig _inRaidConfig;
    protected InRaidHelper _inRaidHelper;
    protected InsuranceService _insuranceService;
    protected LocalisationService _localisationService;
    protected LocationConfig _locationConfig;
    protected LocationLootGenerator _locationLootGenerator;
    protected ISptLogger<LocationLifecycleService> _logger;
    protected LootGenerator _lootGenerator;
    protected MailSendService _mailSendService;
    protected MatchBotDetailsCacheService _matchBotDetailsCacheService;
    protected PlayerScavGenerator _playerScavGenerator;
    protected PmcChatResponseService _pmcChatResponseService;
    protected PmcConfig _pmcConfig;
    protected PmcWaveGenerator _pmcWaveGenerator;
    protected ProfileHelper _profileHelper;
    protected QuestHelper _questHelper;
    protected RagfairConfig _ragfairConfig;
    protected RaidTimeAdjustmentService _raidTimeAdjustmentService;
    protected RandomUtil _randomUtil;
    protected RewardHelper _rewardHelper;
    protected SaveServer _saveServer;
    protected TimeUtil _timeUtil;
    protected TraderConfig _traderConfig;
    protected TraderHelper _traderHelper;

    public LocationLifecycleService(
        ISptLogger<LocationLifecycleService> logger,
        RewardHelper rewardHelper,
        ConfigServer configServer,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        ProfileHelper profileHelper,
        HashUtil hashUtil,
        ApplicationContext applicationContext,
        BotGenerationCacheService botGenerationCacheService,
        BotNameService botNameService,
        ICloner cloner,
        RaidTimeAdjustmentService raidTimeAdjustmentService,
        LocationLootGenerator locationLootGenerator,
        LocalisationService localisationService,
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
        MatchBotDetailsCacheService matchBotDetailsCacheService
    )
    {
        _logger = logger;
        _rewardHelper = rewardHelper;
        _configServer = configServer;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _profileHelper = profileHelper;
        _hashUtil = hashUtil;
        _applicationContext = applicationContext;
        _botGenerationCacheService = botGenerationCacheService;
        _botNameService = botNameService;
        _cloner = cloner;
        _raidTimeAdjustmentService = raidTimeAdjustmentService;
        _locationLootGenerator = locationLootGenerator;
        _localisationService = localisationService;
        _botLootCacheService = botLootCacheService;
        _lootGenerator = lootGenerator;
        _mailSendService = mailSendService;
        _traderHelper = traderHelper;
        _randomUtil = randomUtil;
        _inRaidHelper = inRaidHelper;
        _playerScavGenerator = playerScavGenerator;
        _saveServer = saveServer;
        _healthHelper = healthHelper;
        _pmcChatResponseService = pmcChatResponseService;
        _pmcWaveGenerator = pmcWaveGenerator;
        _questHelper = questHelper;
        _insuranceService = insuranceService;
        _matchBotDetailsCacheService = matchBotDetailsCacheService;

        _locationConfig = _configServer.GetConfig<LocationConfig>();
        _inRaidConfig = _configServer.GetConfig<InRaidConfig>();
        _traderConfig = _configServer.GetConfig<TraderConfig>();
        _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    ///     Handle client/match/local/start
    /// </summary>
    public virtual StartLocalRaidResponseData StartLocalRaid(string sessionId, StartLocalRaidRequestData request)
    {
        _logger.Debug($"Starting: {request.Location}");

        var playerProfile = _profileHelper.GetFullProfile(sessionId);

        // Remove skill fatigue values
        ResetSkillPointsEarnedDuringRaid(string.Equals(request.PlayerSide, "pmc", StringComparison.OrdinalIgnoreCase)
            ? playerProfile.CharacterData.PmcData.Skills.Common
            : playerProfile.CharacterData.ScavData.Skills.Common);

        // Raid is starting, adjust run times to reduce server load while player is in raid
        _ragfairConfig.RunIntervalSeconds = _ragfairConfig.RunIntervalValues.InRaid;
        _hideoutConfig.RunIntervalSeconds = _hideoutConfig.RunIntervalValues.InRaid;

        var result = new StartLocalRaidResponseData
        {
            ServerId = $"{request.Location}.{request.PlayerSide} {_timeUtil.GetTimeStamp()}", // TODO - does this need to be more verbose - investigate client?
            ServerSettings = _databaseService.GetLocationServices(), // TODO - is this per map or global?
            Profile = new ProfileInsuredItems
            {
                InsuredItems = playerProfile.CharacterData.PmcData.InsuredItems
            },
            LocationLoot = GenerateLocationAndLoot(request.Location, !request.ShouldSkipLootGeneration ?? true),
            TransitionType = TransitionType.NONE,
            Transition = new Transition
            {
                TransitionType = TransitionType.NONE,
                TransitionRaidId = _hashUtil.Generate(),
                TransitionCount = 0,
                VisitedLocations = []
            }
        };

        // Only has value when transitioning into map from previous one
        if (request.Transition is not null)
            // TODO - why doesn't the raid after transit have any transit data?
        {
            result.Transition = request.Transition;
        }

        // Get data stored at end of previous raid (if any)
        var transitionData = _applicationContext
            .GetLatestValue(ContextVariableType.TRANSIT_INFO)
            ?.GetValue<LocationTransit>();

        if (transitionData is not null)
        {
            _logger.Success($"Player: {sessionId} is in transit to {request.Location}");
            result.Transition.TransitionType = TransitionType.COMMON;
            result.Transition.TransitionRaidId = transitionData.TransitionRaidId;
            result.Transition.TransitionCount += 1;

            // Used by client to determine infil location) - client adds the map player is transiting to later
            result.Transition.VisitedLocations.Add(transitionData.SptLastVisitedLocation);

            // Complete, clean up as no longer needed
            _applicationContext.ClearValues(ContextVariableType.TRANSIT_INFO);
        }

        // Apply changes from pmcConfig to bot hostility values
        AdjustBotHostilitySettings(result.LocationLoot);

        AdjustExtracts(request.PlayerSide, request.Location, result.LocationLoot);

        // Clear bot cache ready for bot generation call that occurs after this
        _botGenerationCacheService.ClearStoredBots();
        _botNameService.ClearNameCache();

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        return result;
    }

    /// <summary>
    ///     Replace map exits with scav exits when player is scavving
    /// </summary>
    /// <param name="playerSide"> Players side (savage/usec/bear) </param>
    /// <param name="location"> ID of map being loaded </param>
    /// <param name="locationData"> Maps location base data </param>
    protected void AdjustExtracts(string playerSide, string location, LocationBase locationData)
    {
        var playerIsScav = string.Equals(playerSide, "savage", StringComparison.OrdinalIgnoreCase);
        if (!playerIsScav)
        {
            return;
        }

        // Get relevant extract data for map
        var mapExtracts = _databaseService.GetLocation(location)?.AllExtracts;
        if (mapExtracts is null)
        {
            _logger.Warning($"Unable to find map: {location} extract data, no adjustments made");

            return;
        }

        // Find only scav extracts and overwrite existing exits with them
        var scavExtracts = mapExtracts.Where(extract => string.Equals(extract.Side, "scav", StringComparison.OrdinalIgnoreCase)).ToList();
        if (scavExtracts.Count > 0)
            // Scav extracts found, use them
        {
            locationData.Exits.AddRange(scavExtracts);
        }
    }

    /// <summary>
    ///     Adjust the bot hostility values prior to entering a raid
    /// </summary>
    /// <param name="location"> Map to adjust values of </param>
    protected void AdjustBotHostilitySettings(LocationBase location)
    {
        foreach (var botId in _pmcConfig.HostilitySettings)
        {
            var configHostilityChanges = _pmcConfig.HostilitySettings[botId.Key];
            var locationBotHostilityDetails = location.BotLocationModifier.AdditionalHostilitySettings.FirstOrDefault(botSettings =>
                string.Equals(botSettings.BotRole, botId.Key, StringComparison.OrdinalIgnoreCase)
            );

            // No matching bot in config, skip
            if (locationBotHostilityDetails is null)
            {
                _logger.Warning($"No bot: {botId} hostility values found on: {location.Id}, can only edit existing. Skipping");

                continue;
            }

            // Add new permanent enemies if they don't already exist
            if (configHostilityChanges.AdditionalEnemyTypes is not null)
            {
                foreach (var enemyTypeToAdd in configHostilityChanges.AdditionalEnemyTypes)
                {
                    if (!locationBotHostilityDetails.AlwaysEnemies.Contains(enemyTypeToAdd))
                    {
                        locationBotHostilityDetails.AlwaysEnemies.Add(enemyTypeToAdd);
                    }
                }
            }

            // Add/edit chance settings
            if (configHostilityChanges.ChancedEnemies is not null)
            {
                locationBotHostilityDetails.ChancedEnemies = [];
                foreach (var chanceDetailsToApply in configHostilityChanges.ChancedEnemies)
                {
                    var locationBotDetails = locationBotHostilityDetails.ChancedEnemies.FirstOrDefault(botChance => botChance.Role == chanceDetailsToApply.Role
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
                    if (!locationBotHostilityDetails.AlwaysFriends.Contains(friendlyTypeToAdd))
                    {
                        locationBotHostilityDetails.AlwaysFriends.Add(friendlyTypeToAdd);
                    }
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
    /// <param name="name"> Map name </param>
    /// <param name="generateLoot"> OPTIONAL - Should loot be generated for the map before being returned </param>
    /// <returns> LocationBase </returns>
    /// <returns> LocationBase </returns>
    /// <param name="generateLoot"> OPTIONAL - Should loot be generated for the map before being returned </param>
    /// <param name="name"> Map name </param>
    /// </summary>
    /// Generate a maps base location (cloned) and loot
    /// <summary>
    public virtual LocationBase GenerateLocationAndLoot(string name, bool generateLoot = true)
    {
        var location = _databaseService.GetLocation(name);
        var locationBaseClone = _cloner.Clone(location.Base);

        // Update datetime property to now
        locationBaseClone.UnixDateTime = _timeUtil.GetTimeStamp();

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

        // Add cusom pmcs to map every time its run
        _pmcWaveGenerator.ApplyWaveChangesToMap(locationBaseClone);

        // Adjust raid based on whether this is a scav run
        LocationConfig? locationConfigClone = null;
        var raidAdjustments = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_ADJUSTMENTS)
            ?.GetValue<RaidChanges>();
        if (raidAdjustments is not null)
        {
            locationConfigClone = _cloner.Clone(_locationConfig); // Clone values so they can be used to reset originals later
            _raidTimeAdjustmentService.MakeAdjustmentsToMap(raidAdjustments, locationBaseClone);
        }

        var staticAmmoDist = _cloner.Clone(location.StaticAmmo);

        // Create containers and add loot to them
        var staticLoot = _locationLootGenerator.GenerateStaticContainers(locationBaseClone, staticAmmoDist);
        locationBaseClone.Loot.AddRange(staticLoot);

        // Add dynamic loot to output loot
        var dynamicLootDistClone = _cloner.Clone(location.LooseLoot.Value);
        var dynamicSpawnPoints = _locationLootGenerator.GenerateDynamicLoot(
            dynamicLootDistClone,
            staticAmmoDist,
            name.ToLower()
        );

        // Push chosen spawn points into returned object
        foreach (var spawnPoint in dynamicSpawnPoints)
        {
            locationBaseClone.Loot.Add(spawnPoint);
        }

        // Done generating, log results
        _logger.Success(
            _localisationService.GetText("location-dynamic_items_spawned_success", dynamicSpawnPoints.Count)
        );
        _logger.Success(_localisationService.GetText("location-generated_success", name));

        // Reset loot multipliers back to original values
        if (raidAdjustments is not null && locationConfigClone is not null)
        {
            _logger.Debug("Resetting loot multipliers back to their original values");
            _locationConfig.StaticLootMultiplier = locationConfigClone.StaticLootMultiplier;
            _locationConfig.LooseLootMultiplier = locationConfigClone.LooseLootMultiplier;

            _applicationContext.ClearValues(ContextVariableType.RAID_ADJUSTMENTS);
        }

        return locationBaseClone;
    }

    /// <summary>
    ///     Handle client/match/local/end
    /// </summary>
    public virtual void EndLocalRaid(string sessionId, EndLocalRaidRequestData request)
    {
        // Clear bot loot cache
        _botLootCacheService.ClearCache();

        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile.CharacterData.PmcData;
        var scavProfile = fullProfile.CharacterData.ScavData;

        // TODO:
        // Quest status?
        // stats/eft/aggressor - weird values (EFT.IProfileDataContainer.Nickname)

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Raid: {request.ServerId} outcome: {request.Results.Result}");
        }

        // Reset flea interval time to out-of-raid value
        _ragfairConfig.RunIntervalSeconds = _ragfairConfig.RunIntervalValues.OutOfRaid;
        _hideoutConfig.RunIntervalSeconds = _hideoutConfig.RunIntervalValues.OutOfRaid;

        // ServerId has various info stored in it, delimited by a period
        var serverDetails = request.ServerId.Split(".");

        var locationName = serverDetails[0].ToLower();
        var isPmc = serverDetails[1].ToLower().Contains("pmc");
        var mapBase = _databaseService.GetLocation(locationName).Base;
        var isDead = IsPlayerDead(request.Results);
        var isTransfer = IsMapToMapTransfer(request.Results);
        var isSurvived = IsPlayerSurvived(request.Results);

        // Handle items transferred via BTR or transit to player mailbox
        HandleItemTransferEvent(sessionId, request);

        // Player is moving between maps
        if (isTransfer && request.LocationTransit is not null)
        {
            // Manually store the map player just left
            request.LocationTransit.SptLastVisitedLocation = locationName;
            // TODO - Persist each players last visited location history over multiple transits, e.g using InMemoryCacheService, need to take care to not let data get stored forever
            // Store transfer data for later use in `startLocalRaid()` when next raid starts
            request.LocationTransit.SptExitName = request.Results.ExitName;
            _applicationContext.AddValue(ContextVariableType.TRANSIT_INFO, request.LocationTransit);
        }

        if (!isPmc)
        {
            HandlePostRaidPlayerScav(sessionId, pmcProfile, scavProfile, isDead, isTransfer, request);

            return;
        }

        HandlePostRaidPmc(
            sessionId,
            fullProfile,
            scavProfile,
            isDead,
            isSurvived,
            isTransfer,
            request,
            locationName
        );

        // Handle car extracts
        if (ExtractWasViaCar(request.Results.ExitName))
        {
            HandleCarExtract(request.Results.ExitName, pmcProfile, sessionId);
        }

        // Handle coop exit
        if (
            request.Results.ExitName is not null &&
            ExtractTakenWasCoop(request.Results.ExitName) &&
            _traderConfig.Fence.CoopExtractGift.SendGift
        )
        {
            HandleCoopExtract(sessionId, pmcProfile, request.Results.ExitName);
            SendCoopTakenFenceMessage(sessionId);
        }
    }

    private void SendCoopTakenFenceMessage(string sessionId)
    {
        // Generate reward for taking coop extract
        var loot = _lootGenerator.CreateRandomLoot(_traderConfig.Fence.CoopExtractGift);
        var mailableLoot = new List<Item>();

        var parentId = _hashUtil.Generate();
        foreach (var itemAndChildren in loot)
        {
            // Set all root items parent to new id
            itemAndChildren[0].ParentId = parentId;
        }

        // Flatten
        mailableLoot.AddRange(loot.SelectMany(x => x));

        // Send message from fence giving player reward generated above
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            Traders.FENCE,
            MessageType.MESSAGE_WITH_ITEMS,
            _randomUtil.GetArrayValue(_traderConfig.Fence.CoopExtractGift.MessageLocaleIds),
            mailableLoot,
            _timeUtil.GetHoursAsSeconds(_traderConfig.Fence.CoopExtractGift.GiftExpiryHours)
        );
    }

    /// <summary>
    ///     Was extract by car
    /// </summary>
    /// <param name="extractName"> Name of extract </param>
    /// <returns> True if extract was by car </returns>
    protected bool ExtractWasViaCar(string extractName)
    {
        // exit name is undefined on death
        if (extractName is null)
        {
            return false;
        }

        if (extractName.ToLower().Contains("v-ex"))
        {
            return true;
        }

        return _inRaidConfig.CarExtracts.Contains(extractName.Trim());
    }

    /// <summary>
    ///     Handle when a player extracts using a car - Add rep to fence
    /// </summary>
    /// <param name="extractName"> Name of the extract used </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="sessionId"> Session ID </param>
    protected void HandleCarExtract(string extractName, PmcData pmcData, string sessionId)
    {
        pmcData.CarExtractCounts?.TryAdd(extractName, 0);

        // Increment extract count value
        pmcData.CarExtractCounts[extractName] += 1;

        var newFenceStanding = GetFenceStandingAfterExtract(
            pmcData,
            _inRaidConfig.CarExtractBaseStandingGain,
            pmcData.CarExtractCounts[extractName]
        );

        const string fenceId = Traders.FENCE;
        pmcData.TradersInfo[fenceId].Standing = newFenceStanding;

        // Check if new standing has leveled up trader
        _traderHelper.LevelUp(fenceId, pmcData);
        pmcData.TradersInfo[fenceId].LoyaltyLevel = Math.Max((int) pmcData.TradersInfo[fenceId].LoyaltyLevel, 1);

        _logger.Debug($"Car extract: {extractName} used, total times taken: {pmcData.CarExtractCounts[extractName]}");

        // Copy updated fence rep values into scav profile to ensure consistency
        var scavData = _profileHelper.GetScavProfile(sessionId);
        scavData.TradersInfo[fenceId].Standing = pmcData.TradersInfo[fenceId].Standing;
        scavData.TradersInfo[fenceId].LoyaltyLevel = pmcData.TradersInfo[fenceId].LoyaltyLevel;
    }

    /// <summary>
    ///     Handle when a player extracts using a coop extract - add rep to fence
    /// </summary>
    /// <param name="sessionId"> Session/player id </param>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="extractName"> Name of extract taken </param>
    protected void HandleCoopExtract(string sessionId, PmcData pmcData, string extractName)
    {
        pmcData.CoopExtractCounts?.TryAdd(extractName, 0);

        pmcData.CoopExtractCounts[extractName] += 1;

        var newFenceStanding = GetFenceStandingAfterExtract(
            pmcData,
            _inRaidConfig.CoopExtractBaseStandingGain,
            pmcData.CoopExtractCounts[extractName]
        );

        const string fenceId = Traders.FENCE;
        pmcData.TradersInfo[fenceId].Standing = newFenceStanding;

        // Check if new standing has leveled up trader
        _traderHelper.LevelUp(fenceId, pmcData);
        pmcData.TradersInfo[fenceId].LoyaltyLevel = Math.Max((int) pmcData.TradersInfo[fenceId].LoyaltyLevel, 1);

        _logger.Debug($"COOP extract: {extractName} used");

        // Copy updated fence rep values into scav profile to ensure consistency
        var scavData = _profileHelper.GetScavProfile(sessionId);
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
        const string fenceId = Traders.FENCE;
        var fenceStanding = pmcData.TradersInfo[fenceId].Standing;

        // get standing after taking extract x times, x.xx format, gain from extract can be no smaller than 0.01
        fenceStanding += Math.Max(baseGain / extractCount, 0.01);

        // Ensure fence loyalty level is not above/below the range -7 to 15
        var newFenceStanding = Math.Min(Math.Max((double) fenceStanding, -7), 15);
        _logger.Debug($"Old vs new fence standing: {pmcData.TradersInfo[fenceId].Standing}, {newFenceStanding}");

        return Math.Round(newFenceStanding, 2);
    }

    /// <summary>
    ///     Did player take a COOP extract
    /// </summary>
    /// <param name="extractName"> Name of extract player took </param>
    /// <returns> True if coop extract </returns>
    protected bool ExtractTakenWasCoop(string extractName)
    {
        // No extract name, not a coop extract
        if (extractName is null)
        {
            return false;
        }

        return _inRaidConfig.CoopExtracts.Contains(extractName.Trim());
    }

    protected void HandlePostRaidPlayerScav(
        string sessionId,
        PmcData pmcProfile,
        PmcData scavProfile,
        bool isDead,
        bool isTransfer,
        EndLocalRaidRequestData request)
    {
        var postRaidProfile = request.Results.Profile;

        if (isTransfer)
            // We want scav inventory to persist into next raid when pscav is moving between maps
        {
            _inRaidHelper.SetInventory(sessionId, scavProfile, postRaidProfile, true, isTransfer);
        }

        scavProfile.Info.Level = request.Results.Profile.Info.Level;
        scavProfile.Skills = request.Results.Profile.Skills;
        scavProfile.Stats = request.Results.Profile.Stats;
        scavProfile.Encyclopedia = request.Results.Profile.Encyclopedia;
        scavProfile.TaskConditionCounters = request.Results.Profile.TaskConditionCounters;
        scavProfile.SurvivorClass = request.Results.Profile.SurvivorClass;

        // Scavs dont have achievements, but copy anyway
        scavProfile.Achievements = request.Results.Profile.Achievements;

        scavProfile.Info.Experience = request.Results.Profile.Info.Experience;

        // Must occur after experience is set and stats copied over
        scavProfile.Stats.Eft.TotalSessionExperience = 0;

        ApplyTraderStandingAdjustments(scavProfile.TradersInfo, request.Results.Profile.TradersInfo);

        // Clamp fence standing within -7 to 15 range
        var fenceMax = _traderConfig.Fence.PlayerRepMax; // 15
        var fenceMin = _traderConfig.Fence.PlayerRepMin; //-7
        var currentFenceStanding = request.Results.Profile.TradersInfo[Traders.FENCE].Standing;
        scavProfile.TradersInfo[Traders.FENCE].Standing = Math.Min(Math.Max((double) currentFenceStanding, fenceMin), fenceMax);

        // Successful extract as scav, give some rep
        if (IsPlayerSurvived(request.Results) && scavProfile.TradersInfo[Traders.FENCE].Standing < fenceMax)
        {
            scavProfile.TradersInfo[Traders.FENCE].Standing += _inRaidConfig.ScavExtractStandingGain;
        }

        // Copy scav fence values to PMC profile
        pmcProfile.TradersInfo[Traders.FENCE] = scavProfile.TradersInfo[Traders.FENCE];

        if (ProfileHasConditionCounters(scavProfile))
            // Scav quest progress needs to be moved to pmc so player can see it in menu / hand them in
        {
            MigrateScavQuestProgressToPmcProfile(scavProfile, pmcProfile);
        }

        // Must occur after encyclopedia updated
        MergePmcAndScavEncyclopedias(scavProfile, pmcProfile);

        // Scav died, regen scav loadout and reset timer
        if (isDead)
        {
            _playerScavGenerator.Generate(sessionId);
        }

        // Update last played property
        pmcProfile.Info.LastTimePlayedAsSavage = _timeUtil.GetTimeStamp();

        // Force a profile save
        _saveServer.SaveProfile(sessionId);
    }

    /// <summary>
    ///     Scav quest progress isn't transferred automatically from scav to pmc, we do this manually
    /// </summary>
    /// <param name="scavProfile"> Scav profile with quest progress post-raid </param>
    /// <param name="pmcProfile"> Server pmc profile to copy scav quest progress into </param>
    private void MigrateScavQuestProgressToPmcProfile(PmcData scavProfile, PmcData pmcProfile)
    {
        foreach (var scavQuest in scavProfile.Quests)
        {
            var pmcQuest = pmcProfile.Quests.FirstOrDefault(quest => quest.QId == scavQuest.QId);
            if (pmcQuest is null)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "inraid-unable_to_migrate_pmc_quest_not_found_in_profile",
                        scavQuest.QId
                    )
                );
                continue;
            }

            // Get counters related to scav quest
            var matchingCounters = scavProfile.TaskConditionCounters.Where(counter => counter.Value.SourceId == scavQuest.QId
            );

            if (matchingCounters is null)
            {
                continue;
            }

            // insert scav quest counters into pmc profile
            foreach (var counter in matchingCounters)
            {
                pmcProfile.TaskConditionCounters[counter.Value.Id] = counter.Value;
            }

            // Find Matching PMC Quest
            // Update Status and StatusTimer properties
            pmcQuest.Status = scavQuest.Status;
            pmcQuest.StatusTimers = scavQuest.StatusTimers;
        }
    }

    /// <summary>
    ///     Does the provided profile contain any condition counters
    /// </summary>
    /// <param name="profile"> Profile to check for condition counters </param>
    /// <returns> Profile has condition counters </returns>
    protected bool ProfileHasConditionCounters(PmcData profile)
    {
        if (profile.TaskConditionCounters is null)
        {
            return false;
        }

        return profile.TaskConditionCounters.Count > 0;
    }

    /// <summary>
    ///     Handles PMC Profile after the raid
    /// </summary>
    /// <param name="sessionId"> Player id </param>
    /// <param name="fullProfile"> Pmc profile </param>
    /// <param name="scavProfile"> Scav profile </param>
    /// <param name="isDead"> Player died/got left behind in raid </param>
    /// <param name="isSurvived"> Not same as opposite of `isDead`, specific status </param>
    /// <param name="isTransfer"> Player transferred to another map </param>
    /// <param name="request"> Client request data </param>
    /// <param name="locationName"> Current finished Raid location </param>
    protected void HandlePostRaidPmc(
        string sessionId,
        SptProfile fullProfile,
        PmcData scavProfile,
        bool isDead,
        bool isSurvived,
        bool isTransfer,
        EndLocalRaidRequestData request,
        string locationName)
    {
        var pmcProfile = fullProfile.CharacterData.PmcData;
        var postRaidProfile = request.Results.Profile;
        var preRaidProfileQuestDataClone = _cloner.Clone(pmcProfile.Quests);

        // MUST occur BEFORE inventory actions (setInventory()) occur
        // Player died, get quest items they lost for use later
        var lostQuestItems = _profileHelper.GetQuestItemsInProfile(postRaidProfile);

        // Update inventory
        _inRaidHelper.SetInventory(sessionId, pmcProfile, postRaidProfile, isSurvived, isTransfer);

        pmcProfile.Info.Level = postRaidProfile.Info.Level;
        pmcProfile.Skills = postRaidProfile.Skills;
        pmcProfile.Stats.Eft = postRaidProfile.Stats.Eft;
        pmcProfile.Encyclopedia = postRaidProfile.Encyclopedia;
        pmcProfile.TaskConditionCounters = postRaidProfile.TaskConditionCounters;
        pmcProfile.SurvivorClass = postRaidProfile.SurvivorClass;

        // MUST occur prior to profile achievements being overwritten by post-raid achievements
        ProcessAchievementRewards(fullProfile, postRaidProfile.Achievements);

        pmcProfile.Achievements = postRaidProfile.Achievements;
        pmcProfile.Quests = ProcessPostRaidQuests(postRaidProfile.Quests);

        // Handle edge case - must occur AFTER processPostRaidQuests()
        LightkeeperQuestWorkaround(sessionId, postRaidProfile.Quests, preRaidProfileQuestDataClone, pmcProfile);

        pmcProfile.WishList = postRaidProfile.WishList;

        pmcProfile.Info.Experience = postRaidProfile.Info.Experience;

        ApplyTraderStandingAdjustments(pmcProfile.TradersInfo, postRaidProfile.TradersInfo);

        // Must occur AFTER experience is set and stats copied over
        pmcProfile.Stats.Eft.TotalSessionExperience = 0;

        const string fenceId = Traders.FENCE;

        // Clamp fence standing
        var currentFenceStanding = postRaidProfile.TradersInfo[fenceId].Standing;
        pmcProfile.TradersInfo[fenceId].Standing =
            Math.Min(Math.Max((double) currentFenceStanding, -7), 15); // Ensure it stays between -7 and 15

        // Copy fence values to Scav
        scavProfile.TradersInfo[fenceId] = pmcProfile.TradersInfo[fenceId];

        // MUST occur AFTER encyclopedia updated
        MergePmcAndScavEncyclopedias(pmcProfile, scavProfile);

        // Handle temp, hydration, limb hp/effects
        _healthHelper.UpdateProfileHealthPostRaid(pmcProfile, postRaidProfile.Health, sessionId, isDead);

        // This must occur _BEFORE_ `deleteInventory`, as that method clears insured items
        HandleInsuredItemLostEvent(sessionId, pmcProfile, request, locationName);

        if (isDead)
        {
            if (lostQuestItems.Count > 0)
                // MUST occur AFTER quests have post raid quest data has been merged "processPostRaidQuests()"
                // Player is dead + had quest items, check and fix any broken find item quests
            {
                CheckForAndFixPickupQuestsAfterDeath(sessionId, lostQuestItems, pmcProfile.Quests);
            }

            if (postRaidProfile.Stats.Eft.Aggressor is not null)
            {
                // get the aggressor ID from the client request body
                postRaidProfile.Stats.Eft.Aggressor.ProfileId = request.Results.KillerId;
                _pmcChatResponseService.SendKillerResponse(sessionId, pmcProfile, postRaidProfile.Stats.Eft.Aggressor);
            }

            _inRaidHelper.DeleteInventory(pmcProfile, sessionId);

            _inRaidHelper.RemoveFiRStatusFromItemsInContainer(sessionId, pmcProfile, "SecuredContainer");
        }

        // Must occur AFTER killer messages have been sent
        _matchBotDetailsCacheService.ClearCache();

        var roles = new List<string>
        {
            "pmcbear",
            "pmcusec"
        };

        var victims = postRaidProfile.Stats.Eft.Victims.Where(victim => roles.Contains(victim.Role.ToLower())
            )
            .ToList();
        if (victims?.Count > 0)
            // Player killed PMCs, send some mail responses to them
        {
            _pmcChatResponseService.SendVictimResponse(sessionId, victims, pmcProfile);
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
        string sessionId,
        List<Item> lostQuestItems,
        List<QuestStatus> profileQuests
    )
    {
        // Exclude completed quests
        var activeQuestIdsInProfile = profileQuests
            .Where(quest => quest.Status is not QuestStatusEnum.AvailableForStart and not QuestStatusEnum.Success)
            .Select(status => status.QId);

        // Get db details of quests we found above
        var questDb = _databaseService.GetQuests()
            .Values.Where(quest =>
                activeQuestIdsInProfile.Contains(quest.Id)
            );

        foreach (var lostItem in lostQuestItems)
        {
            var matchingConditionId = string.Empty;
            // Find a quest that has a FindItem condition that has the list items tpl as a target
            var matchingQuests = questDb.Where(quest =>
                    {
                        var matchingCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(questCondition =>
                            questCondition.ConditionType == "FindItem" &&
                            (questCondition.Target.IsList
                                ? questCondition.Target.List
                                : [questCondition.Target.Item]).Contains(lostItem.Template)
                        );
                        if (matchingCondition is null)
                            // Quest doesnt have a matching condition
                        {
                            return false;
                        }

                        // We found a condition, save id for later
                        matchingConditionId = matchingCondition.Id;
                        return true;
                    }
                )
                .ToList();

            // Fail if multiple were found
            if (matchingQuests.Count != 1)
            {
                _logger.Error($"Unable to fix quest item: {lostItem}, {matchingQuests.Count} matching quests found, expected 1");

                continue;
            }

            var matchingQuest = matchingQuests[0];
            // We have a match, remove the condition id from profile to reset progress and let player pick item up again
            var profileQuestToUpdate = profileQuests.FirstOrDefault(questStatus => questStatus.QId == matchingQuest.Id);
            if (profileQuestToUpdate is null)
                // Profile doesnt have a matching quest
            {
                continue;
            }

            // Filter out the matching condition we found
            profileQuestToUpdate.CompletedConditions = profileQuestToUpdate.CompletedConditions.Where(conditionId => conditionId != matchingConditionId
                )
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
        string sessionId,
        List<QuestStatus> postRaidQuests,
        List<QuestStatus> preRaidQuests,
        PmcData pmcProfile
    )
    {
        // LK quests that were not completed before raid but now are
        var newlyCompletedLightkeeperQuests = postRaidQuests
            .Where(postRaidQuest =>
                postRaidQuest.Status == QuestStatusEnum.Success && // Quest is complete
                preRaidQuests.Any(preRaidQuest =>
                    preRaidQuest.QId == postRaidQuest.QId && // Get matching pre-raid quest
                    preRaidQuest.Status != QuestStatusEnum.Success
                ) && // Completed quest was not completed before raid started
                _databaseService.GetQuests().TryGetValue(postRaidQuest.QId, out var quest) &&
                quest?.TraderId == Traders.LIGHTHOUSEKEEPER
            ) // Quest is from LK
            .ToList();


        // Run server complete quest process to ensure player gets rewards
        foreach (var questToComplete in newlyCompletedLightkeeperQuests)
        {
            _questHelper.CompleteQuest(
                pmcProfile,
                new CompleteQuestRequestData
                {
                    Action = "CompleteQuest",
                    QuestId = questToComplete.QId,
                    RemoveExcessItems = false
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
            var dbQuest = _databaseService.GetQuests()[failedQuest.QId];
            if (dbQuest is null)
            {
                continue;
            }

            if (dbQuest.Restartable is not null)
            {
                failedQuest.Status = QuestStatusEnum.Fail;
            }
        }

        return questsToProcess;
    }

    /// <summary>
    ///     Adjust server trader settings if they differ from data sent by client
    /// </summary>
    /// <param name="tradersServerProfile"> Server </param>
    /// <param name="tradersClientProfile"> Client </param>
    protected void ApplyTraderStandingAdjustments(
        Dictionary<string, TraderInfo>? tradersServerProfile,
        Dictionary<string, TraderInfo>? tradersClientProfile
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

    /// <summary>
    ///     Check if player used BTR or transit item sending service and send items to player via mail if found
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="request"> End raid request from client </param>
    protected void HandleItemTransferEvent(string sessionId, EndLocalRaidRequestData request)
    {
        var transferTypes = new List<string>
        {
            "btr",
            "transit"
        };

        foreach (var trasferType in transferTypes)
        {
            var rootId = $"{Traders.BTR}_{trasferType}";
            List<Item>? itemsToSend = null;

            // if rootId doesnt exist in TransferItems, skip
            if (!request?.TransferItems?.TryGetValue(rootId, out itemsToSend) ?? false)
            {
                continue;
            }

            // Filter out the btr container item from transferred items before delivering
            itemsToSend = itemsToSend?.Where(item => item.Id != Traders.BTR).ToList();
            if (itemsToSend?.Count == 0)
            {
                continue;
            }

            TransferItemDelivery(sessionId, Traders.BTR, itemsToSend);
        }
    }

    protected void TransferItemDelivery(string sessionId, string traderId, List<Item> items)
    {
        var serverProfile = _saveServer.GetProfile(sessionId);
        var pmcData = serverProfile.CharacterData.PmcData;

        var dialogueTemplates = _databaseService.GetTrader(traderId).Dialogue;
        if (dialogueTemplates is null)
        {
            _logger.Error(_localisationService.GetText("inraid-unable_to_deliver_item_no_trader_found", traderId));

            return;
        }

        if (!dialogueTemplates.TryGetValue("itemsDelivered", out var itemsDelivered))
        {
            _logger.Error("dialogueTemplates doesn't contain itemsDelivered");
            return;
        }

        var messageId = _randomUtil.GetArrayValue(itemsDelivered);
        var messageStoreTime = _timeUtil.GetHoursAsSeconds(_traderConfig.Fence.BtrDeliveryExpireHours);

        // Remove any items that were returned by the item delivery, but also insured, from the player's insurance list
        // This is to stop items being duplicated by being returned from both item delivery and insurance
        var deliveredItemIds = items.Select(item => item.Id);
        pmcData.InsuredItems = pmcData.InsuredItems.Where(insuredItem => !deliveredItemIds.Contains(insuredItem.ItemId)
            )
            .ToList();

        // Send the items to the player
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            traderId,
            MessageType.BTR_ITEMS_DELIVERY,
            messageId,
            items,
            messageStoreTime
        );
    }

    protected void HandleInsuredItemLostEvent(
        string sessionId,
        PmcData preRaidPmcProfile,
        EndLocalRaidRequestData request,
        string locationName
    )
    {
        if (request.LostInsuredItems?.Count > 0)
        {
            var mappedItems = _insuranceService.MapInsuredItemsToTrader(
                sessionId,
                request.LostInsuredItems,
                preRaidPmcProfile
            );

            // Is possible to have items in lostInsuredItems but removed before reaching mappedItems
            if (mappedItems.Count == 0)
            {
                return;
            }

            _insuranceService.StoreGearLostInRaidToSendLater(sessionId, mappedItems);

            _insuranceService.StartPostRaidInsuranceLostProcess(preRaidPmcProfile, sessionId, locationName);
        }
    }

    /// <summary>
    ///     Checks to see if player survives. run through will return false
    /// </summary>
    /// <param name="results"> Post raid request </param>
    /// <returns> True if survived </returns>
    protected bool IsPlayerSurvived(EndRaidResult results)
    {
        return results.Result == ExitStatus.SURVIVED;
    }

    /// <summary>
    ///     Is the player dead after a raid - dead = anything other than "survived" / "runner"
    /// </summary>
    /// <param name="results"> Post raid request </param>
    /// <returns> True if dead </returns>
    protected bool IsPlayerDead(EndRaidResult results)
    {
        var deathEnums = new List<ExitStatus>
        {
            ExitStatus.KILLED,
            ExitStatus.MISSINGINACTION,
            ExitStatus.LEFT
        };
        return deathEnums.Contains(results.Result.Value);
    }

    /// <summary>
    ///     Has the player moved from one map to another
    /// </summary>
    /// <param name="results"> Post raid request </param>
    /// <returns> True if players transferred </returns>
    protected bool IsMapToMapTransfer(EndRaidResult results)
    {
        return results.Result == ExitStatus.TRANSIT;
    }

    /// <summary>
    ///     Reset the skill points earned in a raid to 0, ready for next raid
    /// </summary>
    /// <param name="commonSkills"> Profile common skills to update </param>
    protected void ResetSkillPointsEarnedDuringRaid(List<BaseSkill> commonSkills)
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
        var mergedDicts = primary.Encyclopedia?.Union(secondary.Encyclopedia)
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Any(kvp => kvp.Value)
            );

        primary.Encyclopedia = mergedDicts;
        secondary.Encyclopedia = mergedDicts;
    }

    /// <summary>
    ///     Check for and add any rewards found via the gained achievements this raid
    /// </summary>
    /// <param name="fullProfile"> Profile to add customisations to </param>
    /// <param name="postRaidAchievements"> All profile achievements at the end of a raid </param>
    protected void ProcessAchievementRewards(SptProfile fullProfile, Dictionary<string, long>? postRaidAchievements)
    {
        var sessionId = fullProfile.ProfileInfo.ProfileId;
        var pmcProfile = fullProfile.CharacterData.PmcData;
        var preRaidAchievementIds = fullProfile.CharacterData.PmcData.Achievements;
        var postRaidAchievementIds = postRaidAchievements;
        var achievementIdsAcquiredThisRaid = postRaidAchievementIds.Where(id => !preRaidAchievementIds.Contains(id)
        );

        // Get achievement data from db
        var achievementsDb = _databaseService.GetTemplates().Achievements;

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
            var rewardItems = _rewardHelper.ApplyRewards(
                achievement.Rewards,
                CustomisationSource.ACHIEVEMENT,
                fullProfile,
                pmcProfile,
                achievement.Id
            );

            if (rewardItems?.Count > 0)
            {
                _mailSendService.SendLocalisedSystemMessageToPlayer(
                    sessionId,
                    "670547bb5fa0b1a7c30d5836 0",
                    rewardItems,
                    [],
                    _timeUtil.GetHoursAsSeconds(24 * 7)
                );
            }
        }
    }
}
