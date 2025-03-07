using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.TraderCallbacks)]
[Injectable(InjectableTypeOverride = typeof(IOnUpdate), TypePriority = OnUpdateOrder.TraderCallbacks)]
[Injectable(InjectableTypeOverride = typeof(TraderCallbacks))]
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

    public string GetRoute()
    {
        return "spt-traders";
    }

    public bool OnUpdate(long _)
    {
        return _traderController.Update();
    }

    /// <summary>
    ///     Handle client/trading/api/traderSettings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetTraderSettings(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_traderController.GetAllTraders(sessionID));
    }

    /// <summary>
    ///     Handle client/trading/api/getTrader
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetTrader(string url, EmptyRequestData _, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTrader/", "");
        return _httpResponseUtil.GetBody(_traderController.GetTrader(sessionID, traderID));
    }

    /// <summary>
    ///     Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetAssort(string url, EmptyRequestData _, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTraderAssort/", "");
        return _httpResponseUtil.GetBody(_traderController.GetAssort(sessionID, traderID));
    }

    /// <summary>
    ///     Handle /singleplayer/moddedTraders
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetModdedTraderData(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_traderConfig.ModdedTraders);
    }
}
