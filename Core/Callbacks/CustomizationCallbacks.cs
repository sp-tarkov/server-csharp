using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;

namespace Core.Callbacks;

public class CustomizationCallbacks
{
    public CustomizationCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle client/trading/customization/storage
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetSuitsResponse> GetSuits(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/trading/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<Suit>> GetTraderSuits(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle CustomizationBuy event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse BuyClothing(PmcData pmcData, BuyClothingRequestData body, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<HideoutCustomisation> GetHideoutCustomisation(PmcData pmcData, EmptyRequestData body, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/customization/storage
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<List<CustomisationStorage>> GetStorage(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle CustomizationSet
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SetClothing(PmcData pmcData, CustomizationSetRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

}