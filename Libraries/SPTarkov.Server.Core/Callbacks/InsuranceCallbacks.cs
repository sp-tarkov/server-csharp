using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnUpdate), TypePriority = OnUpdateOrder.InsuranceCallbacks)]
[Injectable(InjectableTypeOverride = typeof(InsuranceCallbacks))]
public class InsuranceCallbacks(
    InsuranceController _insuranceController,
    InsuranceService _insuranceService,
    HttpResponseUtil _httpResponseUtil,
    ConfigServer _configServer
)
    : IOnUpdate
{
    private readonly InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();

    public bool OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > Math.Max(_insuranceConfig.RunIntervalSeconds, 1))
        {
            _insuranceController.ProcessReturn();
            return true;
        }

        return false;
    }

    public string GetRoute()
    {
        return "spt-insurance";
    }

    /// <summary>
    ///     Handle client/insurance/items/list/cost
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetInsuranceCost(string url, GetInsuranceCostRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_insuranceController.Cost(info, sessionID));
    }

    /// <summary>
    ///     Handle Insure event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse Insure(PmcData pmcData, InsureRequestData info, string sessionID)
    {
        return _insuranceController.Insure(pmcData, info, sessionID);
    }
}
