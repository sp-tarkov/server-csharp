using System.Diagnostics;
using System.Text.Json.Serialization;
using Core.Context;
using Core.Generators;
using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Controllers;

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

    /**
     * Return the number of bot load-out varieties to be generated
     * @param type bot Type we want the load-out gen count for
     * @returns number of bots to generate
     */
    public int GetBotPresetGenerationLimit(string type)
    {

        if (!_botConfig.PresetBatch.TryGetValue(type.ToLower(), out var limit))
        {
            _logger.Warning(_localisationService.GetText("bot-bot_preset_count_value_missing", type));

            return 10;
        }

        return limit;

    }

    public Dictionary<string, object> GetBotCoreDifficulty()
    {
        return _databaseService.GetBots().Core!;
    }

    public DifficultyCategories GetBotDifficulty(string type, string diffLevel, GetRaidConfigurationRequestData? raidConfig, bool ignoreRaidSettings = false)
    {
        var difficulty = diffLevel.ToLower();

        if (!(raidConfig != null || ignoreRaidSettings))
        {
            _logger.Error(_localisationService.GetText("bot-missing_application_context", "RAID_CONFIGURATION"));
        }

        // Check value chosen in pre-raid difficulty dropdown
        // If value is not 'asonline', change requested difficulty to be what was chosen in dropdown
        var botDifficultyDropDownValue = raidConfig?.WavesSettings?.BotDifficulty?.ToString().ToLower() ?? "asonline";
        if (botDifficultyDropDownValue != "asonline")
        {
            difficulty = _botDifficultyHelper.ConvertBotDifficultyDropdownToBotDifficulty(botDifficultyDropDownValue);
        }

        var botDb = _databaseService.GetBots();
        return _botDifficultyHelper.GetBotDifficultySettings(type, difficulty, botDb);
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/difficulties
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
                result[botTypeLower] = result["assault"];
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Unable to find bot: {botTypeLower} in db, copying 'assault'");
                }

                continue;
            }

            if (botDetails?.BotDifficulty is null)
            {
                // Bot has no difficulty values, skip
                _logger.Warning($"Unable to find bot: {botTypeLower} difficulty values in db, skipping");
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
                result[botNameKey].Add(difficultyName, GetBotDifficulty(botNameKey, difficultyName, null, true));
            }
        }

        return result;
    }

    public List<BotBase> Generate(string sessionId, GenerateBotsRequestData info)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        return GenerateBotWaves(info, pmcProfile, sessionId);
    }

    private List<BotBase> GenerateBotWaves(GenerateBotsRequestData request, PmcData? pmcProfile, string sessionId)
    {
        var result = new List<BotBase>();

        var raidSettings = GetMostRecentRaidSettings();

        var allPmcsHaveSameNameAsPlayer = _randomUtil.GetChance100(
            _pmcConfig.AllPMCsHavePlayerNameWithRandomPrefixChance
        );
        var stopwatch = Stopwatch.StartNew();
        // Map conditions to promises for bot generation

        Task.WaitAll((request.Conditions ?? [])
            .Select(condition => Task.Factory.StartNew(() =>
        {
            var botWaveGenerationDetails = GetBotGenerationDetailsForWave(
                condition,
                pmcProfile,
                allPmcsHaveSameNameAsPlayer,
                raidSettings,
                Math.Max(GetBotPresetGenerationLimit(condition.Role), condition.Limit), // Choose largest between value passed in from request vs what's in bot.config
                _botHelper.IsBotPmc(condition.Role));

            result.AddRange(GenerateBotWave(condition, botWaveGenerationDetails, sessionId));
        })).ToArray());

        stopwatch.Stop();
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds}ms to GenerateMultipleBotsAndCache()");
        }

        return result;
    }

    private List<BotBase> GenerateBotWave(GenerateCondition condition, BotGenerationDetails botGenerationDetails, string sessionId)
    {
        var isEventBot = condition.Role?.ToLower().Contains("event");
        if (isEventBot.GetValueOrDefault(false))
        {
            // Add eventRole data + reassign role property to be base type
            botGenerationDetails.EventRole = condition.Role;
            botGenerationDetails.Role = _seasonalEventService.GetBaseRoleForEventBot(
                botGenerationDetails.EventRole
            );
        }

        var role = botGenerationDetails.EventRole ?? botGenerationDetails.Role;
        
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Generating wave of: {botGenerationDetails.BotCountToGenerate} bots of type: {role} {botGenerationDetails.BotDifficulty}");
        }

        var results = new List<BotBase>();
        for (var i = 0; i < botGenerationDetails.BotCountToGenerate; i++)
        {
            try
            {
                var bot = _botGenerator.PrepareAndGenerateBot(sessionId, _cloner.Clone(botGenerationDetails));

                // The client expects the Side for PMCs to be `Savage`
                // We do this here so it's after we cache the bot in the match details lookup, as when you die, they will have the right side
                if (bot.Info.Side is "Bear" or "Usec")
                {
                    bot.Info.Side = "Savage";
                }

                results.Add(bot);
                // Store bot details in cache so post-raid PMC messages can use data
                _matchBotDetailsCacheService.CacheBot(_cloner.Clone(bot));
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to generate bot: {botGenerationDetails.Role} #{i + 1}: {e.Message}");
            }
        }

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Generated: {botGenerationDetails.BotCountToGenerate} {botGenerationDetails.Role}" +
                $"({botGenerationDetails.EventRole ?? botGenerationDetails.Role ?? ""}) {botGenerationDetails.BotDifficulty} bots"
            );
        }

        return results;
    }

    private GetRaidConfigurationRequestData? GetMostRecentRaidSettings()
    {
        var raidSettings = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();

        if (raidSettings is null)
        {
            _logger.Warning(_localisationService.GetText("bot-unable_to_load_raid_settings_from_appcontext"));
        }

        return raidSettings;
    }

    private MinMax<int> GetPmcLevelRangeForMap(string? location)
    {
        return _pmcConfig.LocationSpecificPmcLevelOverride!.GetValueOrDefault(location?.ToLower() ?? "", null);
    }

    private BotGenerationDetails GetBotGenerationDetailsForWave(
        GenerateCondition condition,
        PmcData? pmcProfile,
        bool allPmcsHaveSameNameAsPlayer,
        GetRaidConfigurationRequestData? raidSettings,
        int? botCountToGenerate,
        bool generateAsPmc)
    {
        return new BotGenerationDetails
        {
            IsPmc = generateAsPmc,
            Side = generateAsPmc ? _botHelper.GetPmcSideByRole(condition.Role ?? string.Empty) : "Savage",
            Role = condition.Role,
            PlayerLevel = pmcProfile?.Info?.Level ?? 1,
            PlayerName = pmcProfile?.Info?.Nickname,
            BotRelativeLevelDeltaMax = _pmcConfig.BotRelativeLevelDeltaMax,
            BotRelativeLevelDeltaMin = _pmcConfig.BotRelativeLevelDeltaMin,
            BotCountToGenerate = botCountToGenerate,
            BotDifficulty = condition.Difficulty,
            LocationSpecificPmcLevelOverride = GetPmcLevelRangeForMap(raidSettings?.Location), // Min/max levels for PMCs to generate within
            IsPlayerScav = false,
            AllPmcsHaveSameNameAsPlayer = allPmcsHaveSameNameAsPlayer
        };
    }

    public int GetBotCap(string location)
    {
        var botCap = _botConfig.MaxBotCap.FirstOrDefault(x =>
            string.Equals(x.Key.ToLower(), location.ToLower(), StringComparison.OrdinalIgnoreCase));
        if (location == "default")
        {
            _logger.Warning(
                _localisationService.GetText("bot-no_bot_cap_found_for_location", location.ToLower())
            );
        }

        return botCap.Value;
    }

    public AiBotBrainTypes GetAiBotBrainTypes()
    {
        return new AiBotBrainTypes
        {
            PmcType = _pmcConfig.PmcType,
            Assault = _botConfig.AssaultBrainType,
            PlayerScav = _botConfig.PlayerScavBrainType
        };
    }
}

public record AiBotBrainTypes
{
    [JsonPropertyName("pmc")]
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> PmcType
    {
        get;
        set;
    }

    [JsonPropertyName("assault")]
    public Dictionary<string, Dictionary<string, int>> Assault
    {
        get;
        set;
    }

    [JsonPropertyName("playerScav")]
    public Dictionary<string, Dictionary<string, int>> PlayerScav
    {
        get;
        set;
    }
}
