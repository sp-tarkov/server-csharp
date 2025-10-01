using System.Diagnostics;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class BotController(
    ISptLogger<BotController> logger,
    DatabaseService databaseService,
    BotGenerator botGenerator,
    BotHelper botHelper,
    BotDifficultyHelper botDifficultyHelper,
    ServerLocalisationService serverLocalisationService,
    SeasonalEventService seasonalEventService,
    MatchBotDetailsCacheService matchBotDetailsCacheService,
    ProfileHelper profileHelper,
    ConfigServer configServer,
    ProfileActivityService profileActivityService,
    RandomUtil randomUtil,
    ICloner cloner
)
{
    protected readonly BotConfig botConfig = configServer.GetConfig<BotConfig>();
    protected readonly PmcConfig pmcConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Return the number of bot load-out varieties to be generated
    /// </summary>
    /// <param name="type">bot Type we want the load-out gen count for</param>
    /// <returns>number of bots to generate</returns>
    public int GetBotPresetGenerationLimit(string type)
    {
        if (!botConfig.PresetBatch.TryGetValue(type, out var limit))
        {
            logger.Warning(serverLocalisationService.GetText("bot-bot_preset_count_value_missing", type));

            return 10;
        }

        return limit;
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulty
    ///     Get the core.json difficulty settings from database/bots
    /// </summary>
    /// <returns></returns>
    public CoreBot GetBotCoreDifficulty()
    {
        return databaseService.GetBots().Core;
    }

    /// <summary>
    ///     Get bot difficulty settings
    ///     Adjust PMC settings to ensure they engage the correct bot types
    /// </summary>
    /// <param name="sessionId">Which user is requesting his bot settings</param>
    /// <param name="type">what bot the server is requesting settings for</param>
    /// <param name="diffLevel">difficulty level server requested settings for</param>
    /// <param name="ignoreRaidSettings">OPTIONAL - should raid settings chosen pre-raid be ignored</param>
    /// <returns>Difficulty object</returns>
    public DifficultyCategories GetBotDifficulty(MongoId sessionId, string type, string diffLevel, bool ignoreRaidSettings = false)
    {
        var difficulty = diffLevel.ToLowerInvariant();

        var raidConfig = profileActivityService.GetProfileActivityRaidData(sessionId).RaidConfiguration;

        if (!(raidConfig != null || ignoreRaidSettings))
        {
            logger.Error(serverLocalisationService.GetText("bot-missing_application_context", "RAID_CONFIGURATION"));
        }

        // Check value chosen in pre-raid difficulty dropdown
        // If value is not 'asonline', change requested difficulty to be what was chosen in dropdown
        var botDifficultyDropDownValue = raidConfig?.WavesSettings?.BotDifficulty?.ToString().ToLowerInvariant() ?? "asonline";
        if (botDifficultyDropDownValue != "asonline")
        {
            difficulty = botDifficultyHelper.ConvertBotDifficultyDropdownToBotDifficulty(botDifficultyDropDownValue);
        }

        var botDb = databaseService.GetBots();
        return botDifficultyHelper.GetBotDifficultySettings(type, difficulty, botDb);
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Dictionary<string, DifficultyCategories>> GetAllBotDifficulties()
    {
        var result = new Dictionary<string, Dictionary<string, DifficultyCategories>>();

        var botTypesDb = databaseService.GetBots().Types;
        if (botTypesDb is null)
        {
            return result;
        }
        //Get all bot types as sting array
        var botTypes = Enum.GetValues<WildSpawnType>();
        foreach (var botType in botTypes)
        {
            // If bot is usec/bear, swap to different name
            var botTypeLower = botType.IsPmc()
                ? (botType.GetPmcSideByRole() ?? "usec").ToLowerInvariant()
                : botType.ToString().ToLowerInvariant();

            // Get details from db
            if (!botTypesDb.TryGetValue(botTypeLower, out var botDetails))
            {
                // No bot of this type found, copy details from assault
                result[botTypeLower] = result[Roles.Assault];
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Unable to find bot: {botTypeLower} in db, copying: '{Roles.Assault}'");
                }

                continue;
            }

            if (botDetails?.BotDifficulty is null || !botDetails.BotDifficulty.Any())
            {
                // Bot has no difficulty values, skip
                logger.Warning($"Unable to find bot: {botTypeLower} difficulty values in db, skipping");
                continue;
            }

            var botNameKey = botType.ToString().ToLowerInvariant();
            foreach (var (difficultyName, _) in botDetails.BotDifficulty)
            {
                // Bot doesn't exist in result, add
                if (!result.ContainsKey(botNameKey))
                {
                    result.TryAdd(botNameKey, new Dictionary<string, DifficultyCategories>());
                }

                // Store all difficulty values in dict keyed by difficulty type e.g. easy/normal/hard/impossible
                result[botNameKey].TryAdd(difficultyName, GetBotDifficulty(string.Empty, botNameKey, difficultyName, true));
            }
        }

        return result;
    }

    /// <summary>
    ///     Generate bots for a wave
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request"></param>
    /// <returns>List of bots</returns>
    public async Task<IEnumerable<BotBase?>> Generate(MongoId sessionId, GenerateBotsRequestData request)
    {
        var pmcProfile = profileHelper.GetPmcProfile(sessionId);

        return await GenerateBotWaves(sessionId, request, pmcProfile);
    }

    /// <summary>
    ///     Generate bots for passed in wave data
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request">Client bot generation request</param>
    /// <param name="pmcProfile">Player profile generating bots</param>
    /// <returns>List of generated bots</returns>
    protected async Task<IEnumerable<BotBase?>> GenerateBotWaves(MongoId sessionId, GenerateBotsRequestData request, PmcData? pmcProfile)
    {
        if (request.Conditions is null || !request.Conditions.Any())
        {
            return [];
        }

        var stopwatch = Stopwatch.StartNew();

        // Get chosen raid settings from app context
        var raidSettings = GetMostRecentRaidSettings(sessionId);
        var allPmcsHaveSameNameAsPlayer = randomUtil.GetChance100(pmcConfig.AllPMCsHavePlayerNameWithRandomPrefixChance);

        // Split each bot wave into its own task
        var waveGenerationTasks = request.Conditions.Select(condition =>
            Task.Run(() =>
            {
                var botWaveGenerationDetails = GetBotGenerationDetailsForWave(
                    condition,
                    pmcProfile,
                    allPmcsHaveSameNameAsPlayer,
                    raidSettings
                );

                // Add bot wave results directly to `botsInWave`
                return GenerateBotWave(sessionId, condition, botWaveGenerationDetails);
            })
        );

        // Wait for all above tasks to complete
        var results = await Task.WhenAll(waveGenerationTasks);

        stopwatch.Stop();
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Took {stopwatch.ElapsedMilliseconds}ms to GenerateMultipleBotsAndCache()");
        }

        // Merge + flatten results of all wave generations
        return results.SelectMany(botList => botList);
    }

    /// <summary>
    ///     Generate bots for a single wave request
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="generateRequest"></param>
    /// <param name="botGenerationDetails"></param>
    /// <returns>Result of generating bot wave</returns>
    protected IEnumerable<BotBase?> GenerateBotWave(
        MongoId sessionId,
        GenerateCondition generateRequest,
        BotGenerationDetails botGenerationDetails
    )
    {
        var isEventBot = generateRequest.Role?.Contains("event", StringComparison.OrdinalIgnoreCase);
        if (isEventBot.GetValueOrDefault(false))
        {
            // Add eventRole data + reassign role property to be base type
            botGenerationDetails.EventRole = generateRequest.Role;
            botGenerationDetails.Role = seasonalEventService.GetBaseRoleForEventBot(botGenerationDetails.EventRole);
        }

        // Event role must take priority to generate correctly
        var role = botGenerationDetails.EventRole ?? botGenerationDetails.Role;

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Generating wave of: {botGenerationDetails.BotCountToGenerate} bots of type: {role} {botGenerationDetails.BotDifficulty}"
            );
        }

        var generatedBots = Enumerable
            .Range(0, botGenerationDetails.BotCountToGenerate)
            .AsParallel() // Parallelize above range of values so they can each generate a bot
            .Select(i => TryGenerateSingleBot(sessionId, botGenerationDetails, i))
            .Where(bot =>
                bot is not null
            ) // Skip failed bots
        ; // Materialise parallel query into data

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Generated: {botGenerationDetails.BotCountToGenerate} {botGenerationDetails.Role}"
                    + $"({botGenerationDetails.EventRole ?? botGenerationDetails.Role}) {botGenerationDetails.BotDifficulty} bots"
            );
        }

        return generatedBots;
    }

    /// <summary>
    /// Try to generate and cache a single bot
    /// </summary>
    /// <returns>BotBase object or null.</returns>
    protected BotBase? TryGenerateSingleBot(MongoId sessionId, BotGenerationDetails generationDetails, int botIndex)
    {
        try
        {
            // Clone for thread safety TODO: confirm if clone is necessary (likely not)
            var bot = botGenerator.PrepareAndGenerateBot(sessionId, cloner.Clone(generationDetails)!);

            // Client expects Side for PMCs to be `Savage`, must be altered here before it's cached
            if (bot.Info?.Side is Sides.Bear or Sides.Usec)
            {
                bot.Info.Side = Sides.Savage;
            }

            // Store bot details in cache before returning.
            matchBotDetailsCacheService.CacheBot(bot);

            return bot;
        }
        catch (Exception e)
        {
            logger.Error($"Failed to generate bot #{botIndex + 1} ({generationDetails.Role}): {e.Message}");
            return null;
        }
    }

    /// <summary>
    ///     Pull raid settings from Application context
    /// </summary>
    /// <returns>GetRaidConfigurationRequestData if it exists</returns>
    protected GetRaidConfigurationRequestData? GetMostRecentRaidSettings(MongoId sessionId)
    {
        var raidConfiguration = profileActivityService.GetProfileActivityRaidData(sessionId).RaidConfiguration;

        if (raidConfiguration is null)
        {
            logger.Warning(serverLocalisationService.GetText("bot-unable_to_load_raid_settings_from_appcontext"));
        }

        return raidConfiguration;
    }

    /// <summary>
    ///     Get min/max level range values for a specific map
    /// </summary>
    /// <param name="location">Map name e.g. factory4_day</param>
    /// <returns>MinMax values</returns>
    protected MinMax<int>? GetPmcLevelRangeForMap(string? location)
    {
        return pmcConfig.LocationSpecificPmcLevelOverride!.GetValueOrDefault(location?.ToLowerInvariant() ?? string.Empty, null);
    }

    /// <summary>
    ///     Create a BotGenerationDetails for the bot generator to use
    /// </summary>
    /// <param name="condition">Data from client defining bot type and difficulty</param>
    /// <param name="pmcProfile">Player who is generating bots</param>
    /// <param name="allPmcsHaveSameNameAsPlayer">Should all PMCs have same name as player</param>
    /// <param name="raidSettings">Settings chosen pre-raid by player in client</param>
    /// <returns>BotGenerationDetails</returns>
    protected BotGenerationDetails GetBotGenerationDetailsForWave(
        GenerateCondition condition,
        PmcData? pmcProfile,
        bool allPmcsHaveSameNameAsPlayer,
        GetRaidConfigurationRequestData? raidSettings
    )
    {
        var generateAsPmc = botHelper.IsBotPmc(condition.Role);

        return new BotGenerationDetails
        {
            IsPmc = generateAsPmc,
            Side = generateAsPmc ? botHelper.GetPmcSideByRole(condition.Role ?? string.Empty) : "Savage",
            Role = condition.Role!,
            PlayerLevel = pmcProfile?.Info?.Level ?? 1,
            PlayerName = pmcProfile?.Info?.Nickname,
            BotRelativeLevelDeltaMax = pmcConfig.BotRelativeLevelDelta.Max,
            BotRelativeLevelDeltaMin = pmcConfig.BotRelativeLevelDelta.Min,
            BotCountToGenerate = Math.Max(GetBotPresetGenerationLimit(condition.Role!), condition.Limit), // Choose largest between value passed in from request vs what's in bot.config
            BotDifficulty = condition.Difficulty,
            LocationSpecificPmcLevelOverride = GetPmcLevelRangeForMap(raidSettings?.Location), // Min/max levels for PMCs to generate within
            IsPlayerScav = false,
            AllPmcsHaveSameNameAsPlayer = allPmcsHaveSameNameAsPlayer,
            Location = raidSettings?.Location,
        };
    }

    /// <summary>
    ///     Get the max number of bots allowed on a map
    ///     Looks up location player is entering when getting cap value
    /// </summary>
    /// <param name="location">The map location cap was requested for</param>
    /// <returns>bot cap for map</returns>
    public int GetBotCap(string location)
    {
        if (!botConfig.MaxBotCap.TryGetValue(location.ToLowerInvariant(), out var maxCap))
        {
            return botConfig.MaxBotCap["default"];
        }

        if (location == "default")
        {
            logger.Warning(serverLocalisationService.GetText("bot-no_bot_cap_found_for_location", location.ToLowerInvariant()));
        }

        return maxCap;
    }

    /// <summary>
    ///     Get weights for what each bot type should use as a brain - used by client
    /// </summary>
    /// <returns></returns>
    public AiBotBrainTypes GetAiBotBrainTypes()
    {
        return new AiBotBrainTypes
        {
            PmcType = pmcConfig.PmcType,
            Assault = botConfig.AssaultBrainType,
            PlayerScav = botConfig.PlayerScavBrainType,
        };
    }
}

public record AiBotBrainTypes
{
    [JsonPropertyName("pmc")]
    public required Dictionary<string, Dictionary<string, Dictionary<string, double>>> PmcType { get; set; }

    [JsonPropertyName("assault")]
    public required Dictionary<string, Dictionary<string, int>> Assault { get; set; }

    [JsonPropertyName("playerScav")]
    public required Dictionary<string, Dictionary<string, int>> PlayerScav { get; set; }
}
