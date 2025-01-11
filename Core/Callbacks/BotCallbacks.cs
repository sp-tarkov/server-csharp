using Core.Annotations;
using Core.Context;
using Core.Controllers;
using Core.Models.Eft.Bot;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Match;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class BotCallbacks
{
    protected BotController _botController;
    protected HttpResponseUtil _httpResponseUtil;
    protected ApplicationContext _applicationContext;

    public BotCallbacks
    (
        BotController botController,
        HttpResponseUtil httpResponseUtil,
        ApplicationContext applicationContext
    )
    {
        _botController = botController;
        _httpResponseUtil = httpResponseUtil;
        _applicationContext = applicationContext;
    }

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
        var type = splitUrl[splitUrl.Length - 1];
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
        var type = splitUrl[splitUrl.Length - 2].ToLower();
        var difficulty = splitUrl[splitUrl.Length - 1];
        if (difficulty == "core")
            return _httpResponseUtil.NoBody(_botController.GetBotCoreDifficulty());

        var raidConfig = (GetRaidConfigurationRequestData)_applicationContext.GetLatestValue(ContextVariableType.RAID_CONFIGURATION)?.Value;

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
        var location = splitUrl[splitUrl.Length - 1];
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
