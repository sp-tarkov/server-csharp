using Core.Context;
using Core.Controllers;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Match;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(BotCallbacks))]
public class BotCallbacks(
    BotController _botController,
    HttpResponseUtil _httpResponseUtil,
    ApplicationContext _applicationContext
)
{
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
        var splitUrl = url.Split('/');
        var type = splitUrl[^1];
        return _httpResponseUtil.NoBody(_botController.GetBotPresetGenerationLimit(type));
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
        var splitUrl = url.Split('/');
        var type = splitUrl[^2].ToLower();
        var difficulty = splitUrl[^1];
        if (difficulty == "core")
            return _httpResponseUtil.NoBody(_botController.GetBotCoreDifficulty());

        var raidConfig = _applicationContext.GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();

        return _httpResponseUtil.NoBody(_botController.GetBotDifficulty(type, difficulty, raidConfig));
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAllBotDifficulties(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_botController.GetAllBotDifficulties());
    }

    /// <summary>
    /// Handle client/game/bot/generate
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GenerateBots(string url, GenerateBotsRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_botController.Generate(sessionID, info));
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/maxCap
    /// </summary>
    /// <returns></returns>
    public string GetBotCap(string url, EmptyRequestData info, string sessionID)
    {
        var splitUrl = url.Split('/');
        var location = splitUrl[^1];
        return _httpResponseUtil.NoBody(_botController.GetBotCap(location));
    }

    /// <summary>
    /// Handle singleplayer/settings/bot/getBotBehaviours
    /// </summary>
    /// <returns></returns>
    public string GetBotBehaviours()
    {
        return _httpResponseUtil.NoBody(_botController.GetAiBotBrainTypes());
    }
}
