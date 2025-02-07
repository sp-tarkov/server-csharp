using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Insurance;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class InsuranceStaticRouter : StaticRouter
{
    protected static InsuranceCallbacks _insuranceCallbacks;

    public InsuranceStaticRouter(
        JsonUtil jsonUtil,
        InsuranceCallbacks insuranceCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/insurance/items/list/cost",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => _insuranceCallbacks.GetInsuranceCost(url, info as GetInsuranceCostRequestData, sessionID),
                typeof(GetInsuranceCostRequestData)
            )
        ]
    )
    {
        _insuranceCallbacks = insuranceCallbacks;
    }
}
