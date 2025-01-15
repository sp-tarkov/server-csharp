using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using System.Text.RegularExpressions;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services
{
    [Injectable(InjectionType.Singleton)]
    public class MatchBotDetailsCacheService
    {
        private readonly ILogger _logger;
        private readonly LocalisationService _localisationService;

        private readonly Dictionary<string, BotBase> _botDetailsCache;

        public MatchBotDetailsCacheService(
            ILogger logger,
            LocalisationService localisationService)
        {
            _logger = logger;
            _localisationService = localisationService;

            _botDetailsCache = new();
        }

        public void CacheBot(BotBase botToCache)
        {
            if (botToCache.Info.Nickname is null)
            {
                _logger.Warning($"Unable to cache: { botToCache.Info.Settings.Role} bot with id: ${ botToCache.Id} as it lacks a nickname");
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
                _logger.Warning($"Bot not found in match bot cache: {botName.ToLower()} { botSide}");
            }

            return botInCache;
        }
    }
}
