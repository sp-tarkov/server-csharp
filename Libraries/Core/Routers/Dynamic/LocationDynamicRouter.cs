using SptCommon.Annotations;
using Core.DI;
using Core.Utils;

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
