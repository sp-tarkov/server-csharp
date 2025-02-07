using Core.DI;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Dynamic;

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
