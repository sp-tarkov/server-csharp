namespace Core.Callbacks;

public class RagfairCallbacks
{
    public void Load()
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> Search(string url, SearchRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetItemPriceResult> GetMarketPrice(string url, GetMarketPriceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GetItemPrices(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AddOffer(string url, AddOfferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public ItemEventRouterResponse ExtendOffer(string url, ExtendOfferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public bool Update(int timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public bool UpdatePlayer(int timeSinceLastRun)
    {
        throw new NotImplementedException();
    }
}