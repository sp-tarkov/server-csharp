using System.Diagnostics;
using System.Text.Json.Serialization;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Context;
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
    ISptLogger<BotController> _logger,
    DatabaseService _databaseService,
    BotGenerator _botGenerator,
    BotHelper _botHelper,
    BotDifficultyHelper _botDifficultyHelper,
    LocalisationService _localisationService,
    SeasonalEventService _seasonalEventService,
    MatchBotDetailsCacheService _matchBotDetailsCacheService,
    ProfileHelper _profileHelper,
    ConfigServer _configServer,
    ApplicationContext _applicationContext,
    RandomUtil _randomUtil,
    ICloner _cloner
)
{
    private readonly BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    private readonly PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();
    private static readonly Lock _botListLock = new();

    /// <summary>
    ///     Return the number of bot load-out varieties to be generated
    /// </summary>
    /// <param name="type">bot Type we want the load-out gen count for</param>
    /// <returns>number of bots to generate</returns>
    public int GetBotPresetGenerationLimit(string type)
    {
        if (!_botConfig.PresetBatch.TryGetValue(type, out var limit))
        {
            _logger.Warning(
                _localisationService.GetText("bot-bot_preset_count_value_missing", type)
            );

            return 10;
        }

        return limit;
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulty
    ///     Get the core.json difficulty settings from database/bots
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> GetBotCoreDifficulty()
    {
        return _databaseService.GetBots().Core!;
    }

    /// <summary>
    ///     Get bot difficulty settings
    ///     Adjust PMC settings to ensure they engage the correct bot types
    /// </summary>
    /// <param name="type">what bot the server is requesting settings for</param>
    /// <param name="diffLevel">difficulty level server requested settings for</param>
    /// <param name="raidConfig">OPTIONAL - applicationContext Data stored at start of raid</param>
    /// <param name="ignoreRaidSettings">OPTIONAL - should raid settings chosen pre-raid be ignored</param>
    /// <returns>Difficulty object</returns>
    public DifficultyCategories GetBotDifficulty(
        string type,
        string diffLevel,
        GetRaidConfigurationRequestData? raidConfig,
        bool ignoreRaidSettings = false
    )
    {
        var difficulty = diffLevel.ToLower();

        if (!(raidConfig != null || ignoreRaidSettings))
        {
            _logger.Error(
                _localisationService.GetText(
                    "bot-missing_application_context",
                    "RAID_CONFIGURATION"
                )
            );
        }

        // Check value chosen in pre-raid difficulty dropdown
        // If value is not 'asonline', change requested difficulty to be what was chosen in dropdown
        var botDifficultyDropDownValue =
            raidConfig?.WavesSettings?.BotDifficulty?.ToString().ToLower() ?? "asonline";
        if (botDifficultyDropDownValue != "asonline")
        {
            difficulty = _botDifficultyHelper.ConvertBotDifficultyDropdownToBotDifficulty(
                botDifficultyDropDownValue
            );
        }

        var botDb = _databaseService.GetBots();
        return _botDifficultyHelper.GetBotDifficultySettings(type, difficulty, botDb);
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Dictionary<string, DifficultyCategories>> GetAllBotDifficulties()
    {
        var result = new Dictionary<string, Dictionary<string, DifficultyCategories>>();

        var botTypesDb = _databaseService.GetBots().Types;
        //Get all bot types as sting array
        var botTypes = Enum.GetValues<WildSpawnType>().Select(item => item.ToString()).ToList();
        foreach (var botType in botTypes)
        {
            if (botTypesDb is null)
            {
                continue;
            }

            // If bot is usec/bear, swap to different name
            var botTypeLower = _botHelper.IsBotPmc(botType)
                ? _botHelper.GetPmcSideByRole(botType).ToLower()
                : botType.ToLower();

            // Get details from db
            if (!botTypesDb.TryGetValue(botTypeLower, out var botDetails))
            {
                // No bot of this type found, copy details from assault
                result[botTypeLower] = result[Roles.Assault];
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to find bot: {botTypeLower} in db, copying '{Roles.Assault}'"
                    );
                }

                continue;
            }

            if (botDetails?.BotDifficulty is null)
            {
                // Bot has no difficulty values, skip
                _logger.Warning(
                    $"Unable to find bot: {botTypeLower} difficulty values in db, skipping"
                );
                continue;
            }

            var botNameKey = botType.ToLower();
            foreach (var (difficultyName, _) in botDetails.BotDifficulty)
            {
                // Bot doesn't exist in result, add
                if (!result.ContainsKey(botNameKey))
                {
                    result.TryAdd(botNameKey, new Dictionary<string, DifficultyCategories>());
                }

                // Store all difficulty values in dict keyed by difficulty type e.g. easy/normal/impossible
                result[botNameKey]
                    .Add(difficultyName, GetBotDifficulty(botNameKey, difficultyName, null, true));
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
    public List<BotBase> Generate(string sessionId, GenerateBotsRequestData request)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        return GenerateBotWaves(request, pmcProfile, sessionId);
    }

    /// <summary>
    ///     Generate bots for passed in wave data
    /// </summary>
    /// <param name="request"></param>
    /// <param name="pmcProfile">Player generating bots</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>List of generated bots</returns>
    protected List<BotBase> GenerateBotWaves(
        GenerateBotsRequestData request,
        PmcData? pmcProfile,
        string sessionId
    )
    {
        var generatedBotList = new List<BotBase>();
        var raidSettings = GetMostRecentRaidSettings();
        var allPmcsHaveSameNameAsPlayer = _randomUtil.GetChance100(
            _pmcConfig.AllPMCsHavePlayerNameWithRandomPrefixChance
        );

        var stopwatch = Stopwatch.StartNew();
        // Map conditions to promises for bot generation

        Task.WaitAll(
            (request.Conditions ?? [])
                .Select(condition =>
                    Task.Factory.StartNew(() =>
                    {
                        var botWaveGenerationDetails = GetBotGenerationDetailsForWave(
                            condition,
                            pmcProfile,
                            allPmcsHaveSameNameAsPlayer,
                            raidSettings
                        );

                        GenerateBotWave(
                            condition,
                            botWaveGenerationDetails,
                            generatedBotList,
                            sessionId
                        );
                    })
                )
                .ToArray()
        );

        stopwatch.Stop();

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Took {stopwatch.ElapsedMilliseconds}ms to GenerateMultipleBotsAndCache()"
            );
        }

        return generatedBotList;
    }

    /// <summary>
    ///     Generate bots for a single wave request
    /// </summary>
    /// <param name="generateRequest"></param>
    /// <param name="botGenerationDetails"></param>
    /// <param name="botList">List of bots to fill</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    protected void GenerateBotWave(
        GenerateCondition generateRequest,
        BotGenerationDetails botGenerationDetails,
        List<BotBase> botList,
        string sessionId
    )
    {
        var isEventBot = generateRequest.Role?.Contains(
            "event",
            StringComparison.OrdinalIgnoreCase
        );
        if (isEventBot.GetValueOrDefault(false))
        {
            // Add eventRole data + reassign role property to be base type
            botGenerationDetails.EventRole = generateRequest.Role;
            botGenerationDetails.Role = _seasonalEventService.GetBaseRoleForEventBot(
                botGenerationDetails.EventRole
            );
        }

        var role = botGenerationDetails.EventRole ?? botGenerationDetails.Role;

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Generating wave of: {botGenerationDetails.BotCountToGenerate} bots of type: {role} {botGenerationDetails.BotDifficulty}"
            );
        }

        Parallel.For(
            0,
            botGenerationDetails.BotCountToGenerate.Value,
            (i) =>
            {
                BotBase bot = null;

                try
                {
                    bot = _botGenerator.PrepareAndGenerateBot(
                        sessionId,
                        _cloner.Clone(botGenerationDetails)
                    );
                }
                catch (Exception e)
                {
                    _logger.Error(
                        $"Failed to generate bot: {botGenerationDetails.Role} #{i + 1}: {e.Message} {e.StackTrace}"
                    );
                    return;
                }

                // The client expects the Side for PMCs to be `Savage`
                // We do this here so it's after we cache the bot in the match details lookup, as when you die, they will have the right side
                if (bot.Info.Side is Sides.Bear or Sides.Usec)
                {
                    bot.Info.Side = Sides.Savage;
                }

                lock (_botListLock)
                {
                    botList.Add(bot);
                }

                // Store bot details in cache so post-raid PMC messages can use data
                _matchBotDetailsCacheService.CacheBot(bot);
            }
        );

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Generated: {botGenerationDetails.BotCountToGenerate} {botGenerationDetails.Role}"
                    + $"({botGenerationDetails.EventRole ?? botGenerationDetails.Role ?? ""}) {botGenerationDetails.BotDifficulty} bots"
            );
        }
    }

    /// <summary>
    ///     Pull raid settings from Application context
    /// </summary>
    /// <returns>GetRaidConfigurationRequestData if it exists</returns>
    protected GetRaidConfigurationRequestData? GetMostRecentRaidSettings()
    {
        var raidSettings = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();

        if (raidSettings is null)
        {
            _logger.Warning(
                _localisationService.GetText("bot-unable_to_load_raid_settings_from_appcontext")
            );
        }

        return raidSettings;
    }

    /// <summary>
    ///     Get min/max level range values for a specific map
    /// </summary>
    /// <param name="location">Map name e.g. factory4_day</param>
    /// <returns>MinMax values</returns>
    protected MinMax<int> GetPmcLevelRangeForMap(string? location)
    {
        return _pmcConfig.LocationSpecificPmcLevelOverride!.GetValueOrDefault(
            location?.ToLower() ?? "",
            null
        );
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
        var generateAsPmc = _botHelper.IsBotPmc(condition.Role);

        return new BotGenerationDetails
        {
            IsPmc = generateAsPmc,
            Side = generateAsPmc
                ? _botHelper.GetPmcSideByRole(condition.Role ?? string.Empty)
                : "Savage",
            Role = condition.Role,
            PlayerLevel = pmcProfile?.Info?.Level ?? 1,
            PlayerName = pmcProfile?.Info?.Nickname,
            BotRelativeLevelDeltaMax = _pmcConfig.BotRelativeLevelDeltaMax,
            BotRelativeLevelDeltaMin = _pmcConfig.BotRelativeLevelDeltaMin,
            BotCountToGenerate = Math.Max(
                GetBotPresetGenerationLimit(condition.Role),
                condition.Limit
            ), // Choose largest between value passed in from request vs what's in bot.config
            BotDifficulty = condition.Difficulty,
            LocationSpecificPmcLevelOverride = GetPmcLevelRangeForMap(raidSettings?.Location), // Min/max levels for PMCs to generate within
            IsPlayerScav = false,
            AllPmcsHaveSameNameAsPlayer = allPmcsHaveSameNameAsPlayer,
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
        if (!_botConfig.MaxBotCap.TryGetValue(location.ToLower(), out var maxCap))
        {
            return _botConfig.MaxBotCap["default"];
        }

        if (location == "default")
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-no_bot_cap_found_for_location",
                    location.ToLower()
                )
            );
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
            PmcType = _pmcConfig.PmcType,
            Assault = _botConfig.AssaultBrainType,
            PlayerScav = _botConfig.PlayerScavBrainType,
        };
    }
}

public record AiBotBrainTypes
{
    [JsonPropertyName("pmc")]
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> PmcType { get; set; }

    [JsonPropertyName("assault")]
    public Dictionary<string, Dictionary<string, int>> Assault { get; set; }

    [JsonPropertyName("playerScav")]
    public Dictionary<string, Dictionary<string, int>> PlayerScav { get; set; }
}
