using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;
using SptCommon.Extensions;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotGenerationCacheService(
    ISptLogger<BotGenerationCacheService> _logger,
    LocalisationService _localisationService
    )
{
    protected Dictionary<string, List<BotBase>> _storedBots = new Dictionary<string, List<BotBase>>();
    protected Queue<BotBase> _activeBotsInRaid = [];
    
    
    /**
     * Store list of bots in cache, shuffle results before storage
     * @param botsToStore Bots we want to store in the cache
     */
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

    /**
     * Find and return a bot based on its role
     * Remove bot from internal list so it can't be retrieved again
     * @param key role to retrieve (assault/bossTagilla etc)
     * @returns BotBase object
     */
    public BotBase? GetBot(string key)
    {
        if (_storedBots.TryGetValue(key, out var bots))
        {
            if (bots.Count > 0)
            {
                try
                {
                    return bots.Pop();
                }
                catch (Exception _)
                {
                    _logger.Error(_localisationService.GetText("bot-cache_has_zero_bots_of_requested_type", key));
                }
            }
        }
        
        _logger.Error(_localisationService.GetText("bot-no_bot_type_in_cache", key));
        return null;
    }

    /**
     * Cache a bot that has been sent to the client in memory for later use post-raid to determine if player killed a traitor scav
     * @param botToStore Bot object to store
     */
    public void StoreUsedBot(BotBase botToStore)
    {
        _activeBotsInRaid.Enqueue(botToStore);
    }

    /**
     * Get a bot by its profileId that has been generated and sent to client for current raid
     * Cache is wiped post-raid in client/match/offline/end endOfflineRaid()
     * @param profileId Id of bot to get
     * @returns BotBase
     */
    public BotBase? GetUsedBot(string profileId)
    {
        return _activeBotsInRaid.FirstOrDefault(x => x.Id == profileId);
    }

    /**
     * Remove all cached bot profiles from memory
     */
    public void ClearStoredBots()
    {
        _storedBots.Clear();
        _activeBotsInRaid = [];
    }

    /**
     * Does cache have a bot with requested key
     * @returns false if empty
     */
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
