namespace Core.Models.Spt.Callbacks;

public class HealthCallbacks
{
    public SptProfile OnLoad(string sessionID)
    {
        throw new NotImplementedException();
    }

    public object SyncHealth(string url, SyncHealthRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public object OffraidEat(PmcData pmcData, OffraidEatRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public object OffraidHeal(PmcData pmcData, OffraidHealRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public object HealthTreatment(PmcData pmcData, HealthTreatmentRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}