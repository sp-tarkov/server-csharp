using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class InsuranceItemEventRouter(InsuranceCallbacks insuranceCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<InsureRequestData>(
            ItemEventActions.INSURE,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await insuranceCallbacks.Insure(pmcData, body, sessionID)
        ),
    ]) { }
