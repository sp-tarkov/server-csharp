using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class MatchBotDetailsCacheService(
    ISptLogger<MatchBotDetailsCacheService> _logger,
    LocalisationService _localisationService
)
{
    protected Dictionary<string, BotBase> _botDetailsCache = new();

    public void CacheBot(BotBase botToCache)
    {
        if (botToCache.Info.Nickname is null)
        {
            _logger.Warning($"Unable to cache: {botToCache.Info.Settings.Role} bot with id: {botToCache.Id} as it lacks a nickname");
            return;
        }

        var key = $"{botToCache.Info.Nickname.Trim()}{botToCache.Info.Side}";
        _botDetailsCache.TryAdd(key, botToCache);
    }

    public void ClearCache()
    {
        _botDetailsCache.Clear();
    }

    public BotBase GetBotByNameAndSide(string botName, string botSide)
    {
        var botInCache = _botDetailsCache.GetValueOrDefault($"{botName}{botSide}`", null);
        if (botInCache is null)
        {
            _logger.Warning($"Bot not found in match bot cache: {botName.ToLower()} {botSide}");
        }

        return botInCache;
    }
}
