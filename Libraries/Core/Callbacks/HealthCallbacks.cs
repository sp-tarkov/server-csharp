using SptCommon.Annotations;
using Core.Controllers;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Health;
using Core.Models.Eft.ItemEvent;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class HealthCallbacks(
    HttpResponseUtil _httpResponseUtil,
    ProfileHelper _profileHelper,
    HealthController _healthController
    )
{

    /// <summary>
    /// Custom spt server request found in modules/QTEPatch.cs
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">HealthListener.Instance.CurrentHealth class</param>
    /// <param name="sessionID">session id</param>
    /// <returns>empty response, no data sent back to client</returns>
    public string handleWorkoutEffects(string url, WorkoutData info, string sessionID)
    {
        _healthController.ApplyWorkoutChanges(_profileHelper.GetPmcProfile(sessionID), info, sessionID);
        return _httpResponseUtil.EmptyResponse();
    }

    /// <summary>
    /// Handle Eat
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse OffraidEat(PmcData pmcData, OffraidEatRequestData info, string sessionID)
    {
        return _healthController.OffRaidEat(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle Heal
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse OffraidHeal(PmcData pmcData, OffraidHealRequestData info, string sessionID)
    {
        return _healthController.OffRaidHeal(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle RestoreHealth
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, string sessionID)
    {
        return _healthController.HealthTreatment(pmcData, info, sessionID);
    }
}
