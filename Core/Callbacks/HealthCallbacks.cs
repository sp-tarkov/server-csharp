using Core.Models.Eft.Common;
using Core.Models.Eft.Health;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;

namespace Core.Callbacks;

public class HealthCallbacks
{
    public HealthCallbacks()
    {
    }

    /// <summary>
    /// Custom spt server request found in modules/QTEPatch.cs
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">HealthListener.Instance.CurrentHealth class</param>
    /// <param name="sessionID">session id</param>
    /// <returns>empty response, no data sent back to client</returns>
    public GetBodyResponseData<string> handleWorkoutEffects(string url, WorkoutData info, string sessionID)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}