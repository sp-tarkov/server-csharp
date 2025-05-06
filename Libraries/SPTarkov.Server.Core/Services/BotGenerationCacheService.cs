using System.Collections.Concurrent;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotGenerationCacheService(
    ISptLogger<BotGenerationCacheService> _logger,
    LocalisationService _localisationService
)
{
    protected Queue<BotBase> _activeBotsInRaid = [];
    protected ConcurrentDictionary<string, List<BotBase>> _storedBots = new();


    /// <summary>
    ///     Store list of bots in cache, shuffle results before storage
    /// </summary>
    /// <param name="key"> Role bot is stored as (assault/bossTagilla etc.) </param>
    /// <param name="botsToStore"> Bots we want to store in the cache </param>
    public void StoreBots(string key, List<BotBase> botsToStore)
    {
        foreach (var bot in botsToStore)
        {
            if (!_storedBots.TryAdd(key, [bot]))
            {
                _storedBots[key].Add(bot);
            }
        }
    }

    /// <summary>
    ///     Find and return a bot based on its role. <br />
    ///     Remove bot from internal list so it can't be retrieved again.
    /// </summary>
    /// <param name="key"> role to retrieve (assault/bossTagilla etc) </param>
    /// <returns> BotBase object </returns>
    public BotBase? GetBot(string key)
    {
        if (_storedBots.TryGetValue(key, out var bots))
        {
            if (bots.Count > 0)
            {
                try
                {
                    return bots.PopLast();
                }
                catch (Exception _)
                {
                    _logger.Error(_localisationService.GetText("bot-cache_has_zero_bots_of_requested_type", key));
                }
            }

            _logger.Error(_localisationService.GetText("bot-cache_has_zero_bots_of_requested_type", key));

            return null;
        }

        _logger.Warning(_localisationService.GetText("bot-no_bot_type_in_cache", key));
        return null;
    }

    /// <summary>
    ///     Cache a bot that has been sent to the client in memory for later use post-raid to determine if player killed a traitor scav
    /// </summary>
    /// <param name="botToStore"> Bot object to store </param>
    public void StoreUsedBot(BotBase botToStore)
    {
        _activeBotsInRaid.Enqueue(botToStore);
    }

    /// <summary>
    ///     Get a bot by its profileId that has been generated and sent to client for current raid. <br />
    ///     Cache is wiped post-raid in client/match/offline/end endOfflineRaid()
    /// </summary>
    /// <param name="profileId"> ID of bot to get </param>
    /// <returns> BotBase object </returns>
    public BotBase? GetUsedBot(string profileId)
    {
        return _activeBotsInRaid.FirstOrDefault(x => x.Id == profileId);
    }

    /// <summary>
    ///     Remove all cached bot profiles from memory
    /// </summary>
    public void ClearStoredBots()
    {
        _storedBots.Clear();
        _activeBotsInRaid = [];
    }

    /// <summary>
    ///     Does cache have a bot with requested key
    /// </summary>
    /// <returns> False if empty </returns>
    public bool CacheHasBotWithKey(string key, int size = 0)
    {
        return _storedBots.ContainsKey(key) && _storedBots[key].Count > size;
    }

    public int GetCachedBotCount(string key)
    {
        return _storedBots.TryGetValue(key, out var bot) ? bot.Count : 0;
    }

    public string CreateCacheKey(string? role, string? difficulty)
    {
        return $"{role?.ToLower()}{difficulty?.ToLower()}";
    }
}
