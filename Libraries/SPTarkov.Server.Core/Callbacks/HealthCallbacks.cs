using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class HealthCallbacks(
    HttpResponseUtil _httpResponseUtil,
    ProfileHelper _profileHelper,
    HealthController _healthController
)
{
    /// <summary>
    ///     Custom spt server request found in modules/QTEPatch.cs
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">HealthListener.Instance.CurrentHealth class</param>
    /// <param name="sessionID">session id</param>
    /// <returns>empty response, no data sent back to client</returns>
    public string HandleWorkoutEffects(string url, WorkoutData info, string sessionID)
    {
        _healthController.ApplyWorkoutChanges(_profileHelper.GetPmcProfile(sessionID), info, sessionID);
        return _httpResponseUtil.EmptyResponse();
    }

    /// <summary>
    ///     Handle Eat
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse OffraidEat(PmcData pmcData, OffraidEatRequestData info, string sessionID)
    {
        return _healthController.OffRaidEat(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle Heal
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse OffraidHeal(PmcData pmcData, OffraidHealRequestData info, string sessionID)
    {
        return _healthController.OffRaidHeal(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RestoreHealth
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, string sessionID)
    {
        return _healthController.HealthTreatment(pmcData, info, sessionID);
    }
}
