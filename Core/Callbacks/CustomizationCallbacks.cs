namespace Core.Callbacks;

public class CustomizationCallbacks
{
    public GetBodyResponseData<object> GetSuits(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<Suit>> GetTraderSuits(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse WearClothing(PmcData pmcData, WearClothingRequestData body, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse BuyClothing(PmcData pmcData, BuyClothingRequestData body, string sessionID)
    {
        throw new NotImplementedException();
    }
}