using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;

namespace Core.Controllers;

public class BotController
{
    private BotConfig _botConfig;
    private PmcConfig _pmcConfig;
    
    public BotController()
    {}

    public int GetBotPresetGenerationLimit(string type)
    {
        throw new NotImplementedException();
    }

    public BotCore GetBotCoreDifficulty()
    {
        throw new NotImplementedException();
    }
    
    public object GetBotDifficulty(string type, string difficulty) // TODO: return type was: IBotCore | IDifficultyCategories
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GetAllBotDifficulties()
    {
        throw new NotImplementedException();
    }
    
    public async Task<List<BotBase>> Generate(GenerateBotsRequestData info, bool playerscav)
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


    public int GetBotCap()
    {
        throw new NotImplementedException();
    }
}