using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace SPTarkov.Server.Core.DI;

// The name of this class should be ItemEventRouter, but that name is taken,
// So instead I added the definition
public abstract class ItemEventRouterDefinition : Router
{
    public abstract ItemEventRouterResponse? HandleItemEvent(string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        string sessionID,
        ItemEventRouterResponse output);
}
