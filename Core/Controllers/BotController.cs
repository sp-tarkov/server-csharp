using Core.Annotations;
using Core.Context;
using Core.Generators;
using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class BotController
{
    protected ILogger _logger;

    protected DatabaseService _databaseService;
    protected BotGenerator _botGenerator;
    protected BotHelper _botHelper;
    protected BotDifficultyHelper _botDifficultyHelper;
    protected WeightedRandomHelper _weightedRandomHelper;
    protected BotGenerationCacheService _botGenerationCacheService;
    protected MatchBotDeatilsCacheService _matchBotDeatilsCacheService;
    protected LocalisationService _localisationService;
    protected SeasonalEventService _seasonalEventService;
    private readonly MatchBotDetailsCacheService _matchBotDetailsCacheService;
    protected ProfileHelper _profileHelper;
    protected ConfigServer _configServer;
    protected ApplicationContext _applicationContext;
    protected RandomUtil _randomUtil;
    protected ICloner _cloner;

    protected BotConfig _botConfig;
    protected PmcConfig _pmcConfig;

    public BotController
    (
        ILogger logger,
        DatabaseService databaseService,
        BotGenerator botGenerator,
        BotHelper botHelper,
        BotDifficultyHelper botDifficultyHelper,
        WeightedRandomHelper weightedRandomHelper,
        BotGenerationCacheService botGenerationCacheService,
        MatchBotDeatilsCacheService matchBotDeatilsCacheService,
        LocalisationService localisationService,
        SeasonalEventService seasonalEventService,
        MatchBotDetailsCacheService matchBotDetailsCacheService,
        ProfileHelper profileHelper,
        ConfigServer configServer,
        ApplicationContext applicationContext,
        RandomUtil randomUtil,
        ICloner cloner
    )
    {
        _logger = logger;
        _databaseService = databaseService;
        _botGenerator = botGenerator;
        _botHelper = botHelper;
        _botDifficultyHelper = botDifficultyHelper;
        _weightedRandomHelper = weightedRandomHelper;
        _botGenerationCacheService = botGenerationCacheService;
        _matchBotDeatilsCacheService = matchBotDeatilsCacheService;
        _localisationService = localisationService;
        _seasonalEventService = seasonalEventService;
        _matchBotDetailsCacheService = matchBotDetailsCacheService;
        _profileHelper = profileHelper;
        _configServer = configServer;
        _applicationContext = applicationContext;
        _randomUtil = randomUtil;
        _cloner = cloner;
        _botConfig = _configServer.GetConfig<BotConfig>(ConfigTypes.BOT);
        _pmcConfig = _configServer.GetConfig<PmcConfig>(ConfigTypes.PMC);
    }

    public int GetBotPresetGenerationLimit(string type)
    {
        var typeInLower = type.ToLower();
        var value = (int)typeof(PresetBatch).GetProperties().First(p => p.Name.ToLower() == (typeInLower == "assaultgroup" ? "assault" : typeInLower))
            .GetValue(_botConfig.PresetBatch);

        if (value == null)
        {
            _logger.Warning(_localisationService.GetText("bot-bot_preset_count_value_missing", type));
            return 30;
        }

        return value;
    }

    public Dictionary<string, object> GetBotCoreDifficulty()
    {
        return _databaseService.GetBots().Core;
    }

    public DifficultyCategories GetBotDifficulty(string type, string diffLevel, GetRaidConfigurationRequestData raidConfig, bool ignoreRaidSettings = false)
    {
        var difficulty = diffLevel.ToLower();
        
        if (!(raidConfig != null || ignoreRaidSettings)) // TODD: this might be wrong logic
            _logger.Error(_localisationService.GetText("bot-missing_application_context", "RAID_CONFIGURATION"));
        
        // Check value chosen in pre-raid difficulty dropdown
        // If value is not 'asonline', change requested difficulty to be what was chosen in dropdown
        var botDifficultyDropDownValue = raidConfig?.WavesSettings?.BotDifficulty?.ToString().ToLower() ?? "asonline";
        if (botDifficultyDropDownValue != "asonline")
            difficulty = _botDifficultyHelper.ConvertBotDifficultyDropdownToBotDifficulty(botDifficultyDropDownValue);
        
        var botDb = _databaseService.GetBots();
        return _botDifficultyHelper.GetBotDifficultySettings(type, difficulty, botDb);
    }

    public Dictionary<string, object> GetAllBotDifficulties()
    {
        var result = new Dictionary<string, object>();
        
        var botTypesDb = _databaseService.GetBots().Types;
        // TODO: Come back to this, brainfuck

        return result;
    }

    public List<BotBase> Generate(string sessionId, GenerateBotsRequestData info)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        // Use this opportunity to create and cache bots for later retrieval
        var multipleBotTypesRequested = info.Conditions.Count > 1;
        if (multipleBotTypesRequested)
        {
            return GenerateMultipleBotsAndCache(info, pmcProfile, sessionId);
        }

        return ReturnSingleBotFromCache(sessionId, info);
    }

    public List<BotBase> GenerateMultipleBotsAndCache(GenerateBotsRequestData request, PmcData pmcProfile, string sessionId)
    {
        var raidSettings = GetMostRecentRaidSettings();

        var allPmcsHaveSameNameAsPlayer = _randomUtil.GetChance100(
            _pmcConfig.AllPMCsHavePlayerNameWithRandomPrefixChance);

        // Map conditions to promises for bot generation
        foreach (var condition in request.Conditions)
        {
            var botGenerationDetails = GetBotGenerationDetailsForWave(
                condition,
                pmcProfile,
                allPmcsHaveSameNameAsPlayer,
                raidSettings,
                _botConfig.PresetBatch.GetValueOrDefault(condition.Role, 15),
                _botHelper.IsBotPmc(condition.Role));

            // Generate bots for the current condition
            GenerateWithBotDetails(condition, botGenerationDetails, sessionId);
        }

        return [];
    }

    private void GenerateWithBotDetails(GenerateCondition condition, BotGenerationDetails botGenerationDetails, string sessionId)
    {
        var isEventBot = condition.Role.ToLower().Contains("event");
        if (isEventBot)
        {
            // Add eventRole data + reassign role property to be base type
            botGenerationDetails.EventRole = condition.Role;
            botGenerationDetails.Role = _seasonalEventService.GetBaseRoleForEventBot(
                botGenerationDetails.EventRole);
        }

        // Create a compound key to store bots in cache against
        var cacheKey = _botGenerationCacheService.CreateCacheKey(
            botGenerationDetails.EventRole ?? botGenerationDetails.Role,
            botGenerationDetails.BotDifficulty);

        // Get number of bots we have in cache
        var botCacheCount = _botGenerationCacheService.GetCachedBotCount(cacheKey);

        if (botCacheCount >= botGenerationDetails.BotCountToGenerate)
        {
            _logger.Debug($"Cache already has sufficient bots: { botCacheCount}");
            return;
        }

        // We're below desired count, add bots to cache
        var botsToGenerate = botGenerationDetails.BotCountToGenerate - botCacheCount;
        var progressWriter = new ProgressWriter(botGenerationDetails.BotCountToGenerate.Value);

        _logger.Debug($"Generating { botsToGenerate} bots for cacheKey: {cacheKey}");

        for (var i = 0; i < botsToGenerate; i++)
        {
            try
            {
                var detailsClone = _cloner.Clone(botGenerationDetails);
                GenerateSingleBotAndStoreInCache(detailsClone, sessionId, cacheKey);
                progressWriter.Increment();
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to generate bot #{i + 1}: {e.Message}");
            }
        }

        _logger.Debug($"Generated { botGenerationDetails.BotCountToGenerate} { botGenerationDetails.Role}" +
                      $"({botGenerationDetails.EventRole ?? botGenerationDetails.Role ?? ""}) { botGenerationDetails.BotDifficulty}bots");
    }

    private List<BotBase> ReturnSingleBotFromCache(string sessionId, GenerateBotsRequestData request)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);
        var requestedBot = request.Conditions[0];

        var raidSettings = GetMostRecentRaidSettings();

        // Create generation request for when cache is empty
        var condition = new GenerateCondition{
            Role = requestedBot.Role,
            Limit= 5,
            Difficulty= requestedBot.Difficulty,
        };
        var botGenerationDetails = GetBotGenerationDetailsForWave(
            condition,
            pmcProfile,
            false,
            raidSettings,
            _botConfig.PresetBatch[requestedBot.Role],
            _botHelper.IsBotPmc(requestedBot.Role)
        );

        // Event bots need special actions to occur, set data up for them
        var isEventBot = requestedBot.Role.ToLower().Contains("event");
        if (isEventBot)
        {
            // Add eventRole data + reassign role property
            botGenerationDetails.EventRole = requestedBot.Role;
            botGenerationDetails.Role = _seasonalEventService.GetBaseRoleForEventBot(
                botGenerationDetails.EventRole);
        }

        // Does non pmc bot have a chance of being converted into a pmc
        var convertIntoPmcChanceMinMax = GetPmcConversionMinMaxForLocation(
            requestedBot.Role,
            raidSettings?.Location);
        if (convertIntoPmcChanceMinMax is not null && !botGenerationDetails.IsPmc.Value)
        {
            // Bot has % chance to become pmc and isnt one pmc already
            var convertToPmc = _botHelper.RollChanceToBePmc(convertIntoPmcChanceMinMax);
            if (convertToPmc)
            {
                // Update requirements
                botGenerationDetails.IsPmc = true;
                botGenerationDetails.Role = _botHelper.GetRandomizedPmcRole();
                botGenerationDetails.Side = _botHelper.GetPmcSideByRole(botGenerationDetails.Role);
                botGenerationDetails.BotDifficulty = GetPmcDifficulty(requestedBot.Difficulty);
                botGenerationDetails.BotCountToGenerate = _botConfig.PresetBatch[botGenerationDetails.Role];
            }
        }
        // Only convert to boss when not already converted to PMC & Boss Convert is enabled
        var bossConvertEnabled = _botConfig.AssaultToBossConversion.BossConvertEnabled;
        var bossConvertMinMax = _botConfig.AssaultToBossConversion.BossConvertMinMax;
        var bossesToConvertToWeights = _botConfig.AssaultToBossConversion.BossesToConvertToWeights;
        if (bossConvertEnabled && !botGenerationDetails.IsPmc.Value)
        {
            var bossConvertPercent = bossConvertMinMax[requestedBot.Role.ToLower()];
            if (bossConvertPercent is not null)
            {
                // Roll a percentage check if we should convert scav to boss
                if (
                    _randomUtil.GetChance100(_randomUtil.GetDouble(bossConvertPercent.Min.Value, bossConvertPercent.Max.Value))
                )
                {
                    UpdateBotGenerationDetailsToRandomBoss(botGenerationDetails, bossesToConvertToWeights);
                }
            }
        }

        // Create a compound key to store bots in cache against
        var cacheKey = _botGenerationCacheService.CreateCacheKey(
            botGenerationDetails.EventRole ?? botGenerationDetails.Role,
            botGenerationDetails.BotDifficulty);

        // Check cache for bot using above key
        if (!_botGenerationCacheService.CacheHasBotWithKey(cacheKey))
        {
            // No bot in cache, generate new and store in cache
            GenerateSingleBotAndStoreInCache(botGenerationDetails, sessionId, cacheKey);

            _logger.Debug($"Generated { botGenerationDetails.BotCountToGenerate} " +
                              $"{ botGenerationDetails.Role} ({botGenerationDetails.EventRole ?? ""}) {botGenerationDetails.BotDifficulty} bots");
        }

        var desiredBot = _botGenerationCacheService.GetBot(cacheKey);
        _botGenerationCacheService.StoreUsedBot(desiredBot);

        return [desiredBot];
    }

    private void GenerateSingleBotAndStoreInCache(BotGenerationDetails botGenerationDetails, string sessionId, string cacheKey)
    {
        var botToCache = _botGenerator.PrepareAndGenerateBot(sessionId, botGenerationDetails);
        _botGenerationCacheService.StoreBots(cacheKey, [botToCache]);

        // Store bot details in cache so post-raid PMC messages can use data
        _matchBotDetailsCacheService.CacheBot(botToCache);
    }

    private void UpdateBotGenerationDetailsToRandomBoss(BotGenerationDetails botGenerationDetails, Dictionary<string, int> bossesToConvertToWeights)
    {
        // Seems Actual bosses have the same Brain issues like PMC gaining Boss Brains We can't use all bosses
        botGenerationDetails.Role = _weightedRandomHelper.GetWeightedValue(bossesToConvertToWeights);

        // Bosses are only ever 'normal'
        botGenerationDetails.BotDifficulty = "normal";
        botGenerationDetails.BotCountToGenerate = _botConfig.PresetBatch[botGenerationDetails.Role];
    }

    private string GetPmcDifficulty(string requestedBotDifficulty)
    {
        var difficulty = _pmcConfig.Difficulty.ToLower();
        return difficulty switch
        {
            "asonline" => requestedBotDifficulty,
            "random" => _botDifficultyHelper.ChooseRandomDifficulty(),
            _ => _pmcConfig.Difficulty
        };
    }

    private MinMax GetPmcConversionMinMaxForLocation(string requestedBotRole, string location)
    {
        var mapSpecificConversionValues = _pmcConfig.ConvertIntoPmcChance.GetValueOrDefault(location.ToLower(), null);
        if (mapSpecificConversionValues is null)
        {
            return _pmcConfig.ConvertIntoPmcChance["default"][requestedBotRole];
        }

        return mapSpecificConversionValues[requestedBotRole.ToLower()];
    }

    public GetRaidConfigurationRequestData GetMostRecentRaidSettings()
    {
        var raidSettings = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            .GetValue<GetRaidConfigurationRequestData>();

        if (raidSettings is null)
        {
            _logger.Warning(_localisationService.GetText("bot-unable_to_load_raid_settings_from_appcontext"));
        }

        return raidSettings;
    }

    public MinMax GetPmcLevelRangeForMap(string location)
    {
        return _pmcConfig.LocationSpecificPmcLevelOverride.GetValueOrDefault(location.ToLower(), null);
    }

    public BotGenerationDetails GetBotGenerationDetailsForWave(
        GenerateCondition condition,
        PmcData pmcProfile,
        bool allPmcsHaveSameNameAsPlayer,
        GetRaidConfigurationRequestData raidSettings,
        int botCountToGenerate,
        bool generateAsPmc)
    {
        return new BotGenerationDetails{
            IsPmc = generateAsPmc,
            Side = generateAsPmc ? _botHelper.GetPmcSideByRole(condition.Role) : "Savage",
            Role = condition.Role,
            PlayerLevel = pmcProfile.Info.Level.Value,
            PlayerName = pmcProfile.Info.Nickname,
            BotRelativeLevelDeltaMax = _pmcConfig.BotRelativeLevelDeltaMax,
            BotRelativeLevelDeltaMin = _pmcConfig.BotRelativeLevelDeltaMin,
            BotCountToGenerate = botCountToGenerate,
            BotDifficulty = condition.Difficulty,
            LocationSpecificPmcLevelOverride = this.GetPmcLevelRangeForMap(raidSettings?.Location), // Min/max levels for PMCs to generate within
            IsPlayerScav = false,
            AllPmcsHaveSameNameAsPlayer = allPmcsHaveSameNameAsPlayer,
        };
    }

    public int GetBotLimit(string type)
    {
        throw new NotImplementedException();
    }


    public bool IsBotPmc(string botRole)
    {
        throw new NotImplementedException();
    }

    public bool IsBotBoss(string botRole)
    {
        throw new NotImplementedException();
    }

    public bool IsBotFollower(string botRole)
    {
        throw new NotImplementedException();
    }

    public int GetBotCap(string location)
    {
        var botCap = _botConfig.MaxBotCap[location.ToLower()];
        if (location == "default")
        {
            _logger.Warning(
                _localisationService.GetText("bot-no_bot_cap_found_for_location", location.ToLower()));
        }

        return botCap;
    }

    public object GetAiBotBrainTypes() 
    {
        // TODO: Returns `any` in the node server
        return new {
            pmc = _pmcConfig.PmcType,
            assault = _botConfig.AssaultBrainType,
            playerScav = _botConfig.PlayerScavBrainType,
        };
    }
}
