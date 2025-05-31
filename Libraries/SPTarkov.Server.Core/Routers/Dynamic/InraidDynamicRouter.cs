using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable]
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
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => await inraidCallbacks.RegisterPlayer(url, info as RegisterPlayerRequestData, sessionID),
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
