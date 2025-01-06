using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

public class BotCallbacks
{
    public string GetBotLimit(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBotDifficulty(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, Difficulties> GetAllBotDifficulties(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<BotBase>> GenerateBots(string url, GenerateBotsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBotCap()
    {
        throw new NotImplementedException();
    }

    public string GetBotBehaviours()
    {
        throw new NotImplementedException();
    }
}