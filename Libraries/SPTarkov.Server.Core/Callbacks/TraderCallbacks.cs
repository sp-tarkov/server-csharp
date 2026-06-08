using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Traders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.TraderCallbacks)]
public class TraderCallbacks(
    HttpResponseUtil httpResponseUtil,
    TraderController traderController,
    TraderHelper traderHelper,
    TraderAssortHelper traderAssortHelper
) : IOnLoad, IOnUpdate
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        traderController.Load();
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdateAsync(long _, CancellationToken cancellationToken)
    {
        traderController.Update();

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle client/trading/api/traderSettings
    /// </summary>
    public ValueTask<string> GetTraderSettings(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(traderHelper.GetAllTraders(sessionID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTrader
    /// </summary>
    public ValueTask<string> GetTrader(string url, EmptyRequestData _, MongoId sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTrader/", "");
        return new ValueTask<string>(httpResponseUtil.GetBody(traderHelper.GetTrader(sessionID, traderID)));
    }

    /// <summary>
    ///     Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetAssort(string url, EmptyRequestData _, MongoId sessionID)
    {
        var traderID = url.Replace("/client/trading/api/getTraderAssort/", "");
        return new ValueTask<string>(httpResponseUtil.GetBody(traderAssortHelper.GetAssort(sessionID, traderID)));
    }
}
