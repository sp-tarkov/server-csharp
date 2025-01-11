using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class MatchBotDeatilsCacheService
{
    /// <summary>
    /// Store a bot in the cache, keyed by its name
    /// </summary>
    /// <param name="botToCache">Bot details to cache</param>
    public void CacheBot(BotBase botToCache)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clean the cache of all bot details
    /// </summary>
    public void ClearCache()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a bot in the cache by its name and side
    /// </summary>
    /// <param name="botName">Name of bot to find</param>
    /// <param name="botSide">Side of the bot to find</param>
    /// <returns>Bot details</returns>
    public BotBase GetBotByNameAndSide(string botName, string botSide)
    {
        throw new NotImplementedException();
    }
}
