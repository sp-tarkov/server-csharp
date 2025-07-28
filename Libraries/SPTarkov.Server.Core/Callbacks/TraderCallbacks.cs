using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks)]
public class TraderCallbacks(HttpResponseUtil httpResponseUtil, TraderController traderController, ConfigServer configServer)
    : IOnLoad,
        IOnUpdate
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();

    public Task OnLoad()
    {
        traderController.Load();
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdate(long _)
    {
        traderController.Update();

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle client/trading/api/traderSettings
    /// </summary>
    public ValueTask<string> GetTraderSettings(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(traderController.GetAllTraders(sessionID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTrader
    /// </summary>
    public ValueTask<string> GetTrader(string url, EmptyRequestData _, MongoId sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTrader/", "");
        return new ValueTask<string>(httpResponseUtil.GetBody(traderController.GetTrader(sessionID, traderID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetAssort(string url, EmptyRequestData _, MongoId sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTraderAssort/", "");
        return new ValueTask<string>(httpResponseUtil.GetBody(traderController.GetAssort(sessionID, traderID)));
    }

    /// <summary>
    ///     Handle /singleplayer/moddedTraders
    /// </summary>
    /// <param name="url"></param>
    /// <param name="_"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetModdedTraderData(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(_traderConfig.ModdedTraders));
    }
}
