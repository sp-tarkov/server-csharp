using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Health;
using Core.Models.Eft.ItemEvent;

namespace Core.Controllers;

[Injectable]
public class HealthController
{
    /// <summary>
    /// When healing in menu
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Healing request</param>
    /// <param name="sessionId">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidHeal(
        PmcData pmcData,
        OffraidHealRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle Eat event
    /// Consume food/water outside of a raid
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Eat request</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse OffRaidEat(
        PmcData pmcData,
        OffraidEatRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle RestoreHealth event
    /// Occurs on post-raid healing page
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <param name="request">Request data from client</param>
    /// <param name="sessionId">Session id</param>
    /// <returns></returns>
    public ItemEventRouterResponse HealthTreatment(
        PmcData pmcData,
        HealthTreatmentRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// applies skills from hideout workout.
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="info">Request data</param>
    /// <param name="sessionId">session id</param>
    public void ApplyWorkoutChanges(
        PmcData? pmcData,
        WorkoutData info,
        string sessionId)
    {
        throw new NotImplementedException();
    }
}
