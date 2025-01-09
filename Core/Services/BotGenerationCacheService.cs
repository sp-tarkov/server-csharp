using Core.Models.Eft.Common.Tables;

namespace Core.Services;

public class BotGenerationCacheService
{
    /**
     * Store list of bots in cache, shuffle results before storage
     * @param botsToStore Bots we want to store in the cache
     */
    public void StoreBots(string key, List<BotBase> botsToStore)
    {
        throw new NotImplementedException();
    }

    /**
     * Find and return a bot based on its role
     * Remove bot from internal list so it can't be retrieved again
     * @param key role to retrieve (assault/bossTagilla etc)
     * @returns BotBase object
     */
    public BotBase GetBot(string key)
    {
        throw new NotImplementedException();
    }

    /**
     * Cache a bot that has been sent to the client in memory for later use post-raid to determine if player killed a traitor scav
     * @param botToStore Bot object to store
     */
    public void StoreUsedBot(BotBase botToStore)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a bot by its profileId that has been generated and sent to client for current raid
     * Cache is wiped post-raid in client/match/offline/end endOfflineRaid()
     * @param profileId Id of bot to get
     * @returns BotBase
     */
    public BotBase GetUsedBot(string profileId)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove all cached bot profiles from memory
     */
    public void ClearStoredBots()
    {
        throw new NotImplementedException();
    }

    /**
     * Does cache have a bot with requested key
     * @returns false if empty
     */
    public bool CacheHasBotWithKey(string key, int size = 0)
    {
        throw new NotImplementedException();
    }

    public int GetCachedBotCount(string key)
    {
        throw new NotImplementedException();
    }

    public string CreateCacheKey(string role, string difficulty)
    {
        throw new NotImplementedException();
    }
}
