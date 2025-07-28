using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class HealthCallbacks(HttpResponseUtil httpResponseUtil, ProfileHelper profileHelper, HealthController healthController)
{
    /// <summary>
    ///     Custom spt server request found in modules/QTEPatch.cs
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">HealthListener.Instance.CurrentHealth class</param>
    /// <param name="sessionID">session id</param>
    /// <returns>empty response, no data sent back to client</returns>
    public ValueTask<string> HandleWorkoutEffects(string url, WorkoutData info, MongoId sessionID)
    {
        healthController.ApplyWorkoutChanges(profileHelper.GetPmcProfile(sessionID), info, sessionID);
        return new ValueTask<string>(httpResponseUtil.EmptyResponse());
    }

    /// <summary>
    ///     Handle Eat
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse OffraidEat(PmcData pmcData, OffraidEatRequestData info, MongoId sessionID)
    {
        return healthController.OffRaidEat(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle Heal
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse OffraidHeal(PmcData pmcData, OffraidHealRequestData info, MongoId sessionID)
    {
        return healthController.OffRaidHeal(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RestoreHealth
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, MongoId sessionID)
    {
        return healthController.HealthTreatment(pmcData, info, sessionID);
    }
}
