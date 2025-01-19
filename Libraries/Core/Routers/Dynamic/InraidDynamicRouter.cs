using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.InRaid;
using Core.Utils;

namespace Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class InraidDynamicRouter : DynamicRouter
{
    protected static InraidCallbacks _inraidCallbacks;

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
                ) => _inraidCallbacks.RegisterPlayer(url, info as RegisterPlayerRequestData, sessionID), 
                typeof(RegisterPlayerRequestData))
        ]
    )
    {
        _inraidCallbacks = inraidCallbacks;
    }

    public override string GetTopLevelRoute()
    {
        return "spt-name";
    }
}
