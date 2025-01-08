using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

public class BotCallbacks
{
    public BotCallbacks()
    {
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/limit
    /// Is called by client to define each bot roles wave limit
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetBotLimit(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/difficulty
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetBotDifficulty(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public Dictionary<string, Difficulties> GetAllBotDifficulties(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/bot/generate
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<BotBase>> GenerateBots(string url, GenerateBotsRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/maxCap
    /// </summary>
    /// <returns></returns>
    public string GetBotCap()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/getBotBehaviours
    /// </summary>
    /// <returns></returns>
    public string GetBotBehaviours()
    {
        throw new NotImplementedException();
    }
}