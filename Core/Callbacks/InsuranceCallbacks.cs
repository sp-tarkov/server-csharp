using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class InsuranceCallbacks : OnUpdate
{
    private InsuranceConfig _insuranceConfig;

    public InsuranceCallbacks()
    {
        
    }

    /// <summary>
    /// Handle client/insurance/items/list/cost
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetInsuranceCostResponseData> GetInsuranceCost(string url, GetInsuranceCostRequestData info, string sessionID)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public Task<bool> OnUpdate(long timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}