using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class InsuranceStaticRouter : StaticRouter
{
    public InsuranceStaticRouter(JsonUtil jsonUtil, InsuranceCallbacks insuranceCallbacks)
        : base(
            jsonUtil,
            [
                new RouteAction(
                    "/client/insurance/items/list/cost",
                    (url, info, sessionID, output) =>
                    {
                        return insuranceCallbacks.GetInsuranceCost(
                            url,
                            info as GetInsuranceCostRequestData,
                            sessionID
                        );
                    },
                    typeof(GetInsuranceCostRequestData)
                ),
            ]
        ) { }
}
