using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;

namespace Core.Callbacks;

public class CustomizationCallbacks
{
    public CustomizationCallbacks()
    {
        
    }
    
    public GetBodyResponseData<GetSuitsResponse> GetSuits(string url, EmptyRequestData info, string sessionID)
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