using System.Collections.Concurrent;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Cache bots in a dictionary, keyed by the bots name, keying by name isnt ideal as its not unique but this is used by the post-raid system which doesnt have any bot ids, only name
/// </summary>
[Injectable(InjectionType.Singleton)]
public class MatchBotDetailsCacheService(
    ISptLogger<MatchBotDetailsCacheService> _logger,
    LocalisationService _localisationService
)
{
    protected ConcurrentDictionary<string, BotBase> _botDetailsCache = new();

    /// <summary>
    ///     Store a bot in the cache, keyed by its name.
    /// </summary>
    /// <param name="botToCache"> Bot details to cache </param>
    public void CacheBot(BotBase botToCache)
    {
        if (botToCache.Info.Nickname is null)
        {
            _logger.Warning($"Unable to cache: {botToCache.Info.Settings.Role} bot with id: {botToCache.Id} as it lacks a nickname");
            return;
        }

        botToCache.Inventory = null;
        botToCache.Skills = null;
        botToCache.Stats = null;

        var key = $"{botToCache.Info.Nickname.Trim()}{botToCache.Info.Side}";
        _botDetailsCache.TryAdd(key, botToCache);
    }

    /// <summary>
    ///     Clean the cache of all bot details.
    /// </summary>
    public void ClearCache()
    {
        _botDetailsCache.Clear();
    }

    /// <summary>
    ///     Find a bot in the cache by its name and side.
    /// </summary>
    /// <param name="botName"> Name of bot to find </param>
    /// <param name="botSide"> Side of the bot </param>
    /// <returns></returns>
    public BotBase? GetBotByNameAndSide(string botName, string botSide)
    {
        var botInCache = _botDetailsCache.GetValueOrDefault($"{botName}{botSide}`", null);
        if (botInCache is null)
        {
            _logger.Warning($"Bot not found in match bot cache: {botName.ToLower()} {botSide}");

            return null;
        }

        return botInCache;
    }
}
