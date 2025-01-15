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
using Condition = Core.Models.Spt.Config.Condition;
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
        _profileHelper = profileHelper;
        _configServer = configServer;
        _applicationContext = applicationContext;
        _randomUtil = randomUtil;
        _cloner = cloner;
        _botConfig = _configServer.GetConfig<BotConfig>();
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
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
        throw new NotImplementedException();
    }

    public async Task<List<BotBase>> GenerateMultipleBotsAndCache()
    {
        throw new NotImplementedException();
    }

    public GetRaidConfigurationRequestData GetMostRecentRaidSettings()
    {
        throw new NotImplementedException();
    }

    public MinMax GetPmcLevelRangeForMap(string location)
    {
        throw new NotImplementedException();
    }

    public BotGenerationDetails GetBotGenerationDetailsForWave(Condition condition, PmcData pmcProfile, bool AllPmcsHaveSameNameAsPlayer,
        GetRaidConfigurationRequestData raidSettings, int botCountToGenerate, bool generateAsPmc)
    {
        throw new NotImplementedException();
    }

    public int GetPlayerLevelFromProfile()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public object GetAiBotBrainTypes() // TODO: Returns `any` in the node server
    {
        throw new NotImplementedException();
    }
}
