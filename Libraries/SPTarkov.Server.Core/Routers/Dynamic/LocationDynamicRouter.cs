using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.Dynamic;

[Injectable(InjectableTypeOverride = typeof(DynamicRouter))]
public class LocationDynamicRouter : DynamicRouter
{
    public LocationDynamicRouter(
        JsonUtil jsonUtil
    ) : base(
        jsonUtil,
        []
    )
    {
    }

    public override string GetTopLevelRoute()
    {
        return "spt-loot";
    }
}
