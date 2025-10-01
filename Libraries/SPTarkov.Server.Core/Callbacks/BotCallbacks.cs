using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class BotCallbacks(BotController botController, HttpResponseUtil httpResponseUtil)
{
    /// <summary>
    ///     Handle singleplayer/settings/bot/limit
    ///     Is called by client to define each bot roles wave limit
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBotLimit(string url, EmptyRequestData _, MongoId sessionID)
    {
        var splitUrl = url.Split('/');
        var type = splitUrl[^1];
        return new ValueTask<string>(httpResponseUtil.NoBody(botController.GetBotPresetGenerationLimit(type)));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulty
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBotDifficulty(string url, EmptyRequestData _, MongoId sessionID)
    {
        var splitUrl = url.Split('/');
        var type = splitUrl[^2].ToLowerInvariant();
        var difficulty = splitUrl[^1];
        return difficulty == "core"
            ? new ValueTask<string>(httpResponseUtil.NoBody(botController.GetBotCoreDifficulty()))
            : new ValueTask<string>(httpResponseUtil.NoBody(botController.GetBotDifficulty(sessionID, type, difficulty)));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/difficulties
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetAllBotDifficulties(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(botController.GetAllBotDifficulties()));
    }

    /// <summary>
    ///     Handle client/game/bot/generate
    /// </summary>
    /// <returns></returns>
    public async ValueTask<string> GenerateBots(string url, GenerateBotsRequestData info, MongoId sessionID)
    {
        return httpResponseUtil.GetBody(await botController.Generate(sessionID, info));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/maxCap
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBotCap(string url, EmptyRequestData _, MongoId sessionID)
    {
        var splitUrl = url.Split('/');
        var location = splitUrl[^1];
        return new ValueTask<string>(httpResponseUtil.NoBody(botController.GetBotCap(location)));
    }

    /// <summary>
    ///     Handle singleplayer/settings/bot/getBotBehaviours
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetBotBehaviours()
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(botController.GetAiBotBrainTypes()));
    }
}
