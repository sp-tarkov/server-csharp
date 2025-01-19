using SptCommon.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.TraderCallbacks)]
[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.TraderCallbacks)]
[Injectable(InjectableTypeOverride = typeof(TraderCallbacks))]
public class TraderCallbacks(
    HttpResponseUtil _httpResponseUtil,
    TraderController _traderController,
    ConfigServer _configServer
) : OnLoad, OnUpdate
{
    private readonly TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();
    
    public Task OnLoad()
    {
        _traderController.Load();
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdate(long _)
    {
        return Task.FromResult(_traderController.Update());
    }

    public string GetRoute()
    {
        return "spt-traders";
    }

    /// <summary>
    /// Handle client/trading/api/traderSettings
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTraderSettings(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_traderController.GetAllTraders(sessionID));
    }

    /// <summary>
    /// Handle client/trading/api/getTrader
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTrader(string url, EmptyRequestData info, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTrader/", "");
        return _httpResponseUtil.GetBody(_traderController.GetTrader(sessionID, traderID));
    }

    /// <summary>
    /// Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAssort(string url, EmptyRequestData info, string sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTraderAssort/", "");
        return _httpResponseUtil.GetBody(_traderController.GetAssort(sessionID, traderID));
    }

    /// <summary>
    /// Handle /singleplayer/moddedTraders
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetModdedTraderData(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_traderConfig.ModdedTraders);
    }
}
