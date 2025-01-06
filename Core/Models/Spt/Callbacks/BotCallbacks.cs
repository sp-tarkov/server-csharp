using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Callbacks;

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

    public GetBodyResponseData<List<BotBase>> GenerateBots(string url, GenerateBotsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBotCap()
    {
        throw new NotImplementedException();
    }
}