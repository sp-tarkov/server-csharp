using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks)]
public class TraderCallbacks(
    HttpResponseUtil _httpResponseUtil,
    TraderController _traderController,
    ConfigServer _configServer
) : IOnLoad, IOnUpdate
{
    private readonly TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();

    public Task OnLoad()
    {
        _traderController.Load();
        return Task.CompletedTask;
    }

    public Task OnUpdate(long _)
    {
        _traderController.Update();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handle client/trading/api/traderSettings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetTraderSettings(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_traderController.GetAllTraders(sessionID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTrader
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetTrader(string url, EmptyRequestData _, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTrader/", "");
        return new ValueTask<string>(_httpResponseUtil.GetBody(_traderController.GetTrader(sessionID, traderID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetAssort(string url, EmptyRequestData _, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTraderAssort/", "");
        return new ValueTask<string>(_httpResponseUtil.GetBody(_traderController.GetAssort(sessionID, traderID)));
    }

    /// <summary>
    ///     Handle /singleplayer/moddedTraders
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetModdedTraderData(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.NoBody(_traderConfig.ModdedTraders));
    }
}
