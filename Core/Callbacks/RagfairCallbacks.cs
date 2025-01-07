using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class RagfairCallbacks : OnLoad, OnUpdate
{
    private RagfairConfig _ragfairConfig;

    public RagfairCallbacks()
    {
        
    }
    
    public async Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> OnUpdate(long timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetOffersResult> Search(string url, SearchRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetItemPriceResult> GetMarketPrice(string url, GetMarketPriceRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AddOffer(string url, AddOfferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse RemoveOffer(string url, RemoveOfferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public ItemEventRouterResponse ExtendOffer(string url, ExtendOfferRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<Dictionary<string, int>> GetFleaPrices(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData SendReport(string url, SendRagfairReportRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public NullResponseData StorePlayerOfferTaxAmount(string url, StorePlayerOfferTaxAmountRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<RagfairOffer> GetFleaOfferById(string url, GetRagfairOfferByIdRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}