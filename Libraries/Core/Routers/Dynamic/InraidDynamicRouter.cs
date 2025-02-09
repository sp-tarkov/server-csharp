using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.InRaid;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class InraidDynamicRouter : DynamicRouter
{
    public InraidDynamicRouter(
        JsonUtil jsonUtil,
        InraidCallbacks inraidCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "/client/location/getLocalloot",
                (
                    url,
                    info,
                    sessionID,
                    output
                ) => inraidCallbacks.RegisterPlayer(url, info as RegisterPlayerRequestData, sessionID),
                typeof(RegisterPlayerRequestData)
            )
        ]
    )
    {
    }

    public override string GetTopLevelRoute()
    {
        return "spt-name";
    }
}
