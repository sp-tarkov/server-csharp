using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(BotCallbacks))]
public class BotCallbacks(
    BotController _botController,
    HttpResponseUtil _httpResponseUtil,
    ApplicationContext _applicationContext
)
{
    /// <summary>
    ///     Handle singleplayer/settings/bot/limit
    ///     Is called by client to define each bot roles wave limit
    /// </summary>
    /// <returns></returns>
    public string GetBotLimit(string url, EmptyRequestData _, string sessionID)
    {
        var splitUrl = url.Split('/');
        var type = splitUrl[^1];
        return _httpResponseUtil.NoBody(_botController.GetBotPresetGenerationLimit(type));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulty
    /// </summary>
    /// <returns></returns>
    public string GetBotDifficulty(string url, EmptyRequestData _, string sessionID)
    {
        var splitUrl = url.Split('/');
        var type = splitUrl[^2].ToLower();
        var difficulty = splitUrl[^1];
        if (difficulty == "core")
        {
            return _httpResponseUtil.NoBody(_botController.GetBotCoreDifficulty());
        }

        var raidConfig = _applicationContext.GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();

        return _httpResponseUtil.NoBody(_botController.GetBotDifficulty(type, difficulty, raidConfig));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <returns></returns>
    public string GetAllBotDifficulties(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_botController.GetAllBotDifficulties());
    }

    /// <summary>
    ///     Handle client/game/bot/generate
    /// </summary>
    /// <returns></returns>
    public string GenerateBots(string url, GenerateBotsRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_botController.Generate(sessionID, info));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/maxCap
    /// </summary>
    /// <returns></returns>
    public string GetBotCap(string url, EmptyRequestData _, string sessionID)
    {
        var splitUrl = url.Split('/');
        var location = splitUrl[^1];
        return _httpResponseUtil.NoBody(_botController.GetBotCap(location));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/getBotBehaviours
    /// </summary>
    /// <returns></returns>
    public string GetBotBehaviours()
    {
        return _httpResponseUtil.NoBody(_botController.GetAiBotBrainTypes());
    }
}
