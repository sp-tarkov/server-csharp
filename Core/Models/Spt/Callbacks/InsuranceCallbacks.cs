namespace Core.Models.Spt.Callbacks;

public class InsuranceCallbacks
{
    public SptProfile OnLoad(string sessionID)
    {
        throw new NotImplementedException();
    }

    public object GetInsuranceCost(string url, GetInsuranceCostRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public object Insure(PmcData pmcData, InsureRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public bool Update(int secondsSinceLastRun)
    {
        throw new NotImplementedException();
    }
}