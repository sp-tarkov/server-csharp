using SptCommon.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.InsuranceCallbacks)]
[Injectable(InjectableTypeOverride = typeof(InsuranceCallbacks))]
public class InsuranceCallbacks(
    InsuranceController _insuranceController,
    InsuranceService _insuranceService,
    HttpResponseUtil _httpResponseUtil,
    ConfigServer _configServer
    )
    : OnUpdate
{
    private InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();

    /// <summary>
    /// Handle client/insurance/items/list/cost
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetInsuranceCost(string url, GetInsuranceCostRequestData info, string sessionID)
    {
         return _httpResponseUtil.GetBody(_insuranceController.Cost(info, sessionID));
    }

    /// <summary>
    /// Handle Insure event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse Insure(PmcData pmcData, InsureRequestData info, string sessionID)
    {
        return _insuranceController.Insure(pmcData, info, sessionID);
    }

    public bool OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > Math.Max(_insuranceConfig.RunIntervalSeconds, 1))
        {
            // _insuranceController.ProcessReturn();
            // TODO: InsuranceController is not implemented rn
            return true;
        }

        return false;
    }

    public string GetRoute()
    {
        return "spt-insurance";
    }
}
