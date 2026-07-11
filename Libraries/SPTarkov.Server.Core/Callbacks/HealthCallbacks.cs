using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class HealthCallbacks(HealthController healthController)
{
    /// <summary>
    ///     Handle Eat
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> OffraidEat(PmcData pmcData, OffraidEatRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(healthController.OffRaidEat(pmcData, info, sessionID));
    }

    /// <summary>
    ///     Handle Heal
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> OffraidHeal(PmcData pmcData, OffraidHealRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(healthController.OffRaidHeal(pmcData, info, sessionID));
    }

    /// <summary>
    ///     Handle RestoreHealth
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(healthController.HealthTreatment(pmcData, info, sessionID));
    }
}
