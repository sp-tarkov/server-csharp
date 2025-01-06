using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Controllers;

public class BotController
{
    public int GetBotLimit(string type)
    {
        throw new NotImplementedException();
    }

    public object GetBotDifficulty(string type, string difficulty) // IBotCore | IDifficultyCategories
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

    public List<BotBase> Generate(GenerateBotsRequestData info, bool playerscav)
    {
        throw new NotImplementedException();
    }

    public int GetBotCap()
    {
        throw new NotImplementedException();
    }
}