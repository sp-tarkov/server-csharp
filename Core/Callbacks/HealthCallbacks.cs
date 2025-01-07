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

    public GetBodyResponseData<string> handleWorkoutEffects(string url, WorkoutData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse OffraidEat(PmcData pmcData, OffraidEatRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse OffraidHeal(PmcData pmcData, OffraidHealRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}