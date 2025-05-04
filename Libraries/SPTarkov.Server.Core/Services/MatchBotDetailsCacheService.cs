using System.Collections.Concurrent;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Cache bots in a dictionary, keyed by the bots name, keying by name isnt ideal as its not unique but this is used by the post-raid system which doesnt have any bot ids, only name
/// </summary>
[Injectable(InjectionType.Singleton)]
public class MatchBotDetailsCacheService(ISptLogger<MatchBotDetailsCacheService> _logger)
{
    private static readonly HashSet<string> _sidesToCache = ["pmcUSEC", "pmcBEAR"];

    protected readonly ConcurrentDictionary<string, BotDetailsForChatMessages> BotDetailsCache =
        new();

    /// <summary>
    ///     Store a bot in the cache, keyed by its name.
    /// </summary>
    /// <param name="botToCache"> Bot details to cache </param>
    public void CacheBot(BotBase botToCache)
    {
        if (botToCache is null || botToCache.Id is null)
        {
            return;
        }

        if (botToCache.Info?.Nickname is null)
        {
            _logger.Warning(
                $"Unable to cache: {botToCache.Info?.Settings?.Role} bot with id: {botToCache.Id} as it lacks a nickname"
            );
            return;
        }

        // If bot isn't a PMC, skip
        if (
            botToCache.Info?.Settings?.Role is null
            || !_sidesToCache.Contains(botToCache.Info.Settings.Role)
        )
        {
            return;
        }

        BotDetailsCache.TryAdd(
            botToCache.Id,
            new BotDetailsForChatMessages()
            {
                Nickname = botToCache.Info.Nickname.Trim(),
                Side = botToCache.Info.Side == "pmcUSEC" ? DogtagSide.Usec : DogtagSide.Bear,
                Aid = botToCache.Aid,
                Type = botToCache.Info.MemberCategory,
                Level = botToCache.Info.Level,
            }
        );
    }

    /// <summary>
    ///     Clean the cache of all bot details.
    /// </summary>
    public void ClearCache()
    {
        BotDetailsCache.Clear();
    }

    /// <summary>
    ///     Find a bot in the cache by its name and side.
    /// </summary>
    /// <param name="botName"> Name of bot to find </param>
    /// <param name="botSide"> Side of the bot </param>
    /// <returns></returns>
    public BotDetailsForChatMessages? GetBotById(string? id)
    {
        if (id == null)
        {
            return null;
        }

        var botInCache = BotDetailsCache.GetValueOrDefault(id, null);
        if (botInCache is null)
        {
            _logger.Warning($"Bot not found in match bot cache: {id}");
            return null;
        }

        return botInCache;
    }
}
