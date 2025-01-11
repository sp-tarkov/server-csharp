using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;

namespace Core.Controllers;

public class RagfairController
{
    // TODO
    public GetOffersResult GetOffers(string sessionId, SearchRequestData info)
    {
        throw new NotImplementedException();
    }

    public GetItemPriceResult GetItemMinAvgMaxFleaPriceValues(GetMarketPriceRequestData info)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse AddPlayerOffer(PmcData pmcData, AddOfferRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse RemoveOffer(PmcData pmcData, RemoveOfferRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ExtendOffer(PmcData pmcData, ExtendOfferRequestData info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, double> GetAllFleaPrices()
    {
        throw new NotImplementedException();
    }

    public RagfairOffer GetOfferById(string sessionId, GetRagfairOfferByIdRequest info)
    {
        throw new NotImplementedException();
    }
}
