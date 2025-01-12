using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Servers;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class CustomizationCallbacks
{
    protected CustomizationController _customizationController;
    protected SaveServer _saveServer;
    protected HttpResponseUtil _httpResponseUtil;
    
    public CustomizationCallbacks
    (
        CustomizationController customizationController,
        SaveServer saveServer,
        HttpResponseUtil httpResponseUtil
    )
    {
        _customizationController = customizationController;
        _saveServer = saveServer;
        _httpResponseUtil = httpResponseUtil;
    }

    /// <summary>
    /// Handle client/trading/customization/storage
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetSuits(string url, EmptyRequestData info, string sessionID)
    {
        var result = new GetSuitsResponse
        {
            Id = sessionID,
            Suites = _saveServer.GetProfile(sessionID).Suits
        };

        return _httpResponseUtil.GetBody(result);
    }

    /// <summary>
    /// Handle client/trading/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetTraderSuits(string url, EmptyRequestData info, string sessionID)
    {
        var splitUrl = url.Split('/');
        var traderId = splitUrl[splitUrl.Length - 3];

        return _httpResponseUtil.GetBody(_customizationController.GetTraderSuits(traderId, sessionID));
    }

    /// <summary>
    /// Handle CustomizationBuy event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse BuyClothing(PmcData pmcData, BuyClothingRequestData info, string sessionID)
    {
        return _customizationController.BuyClothing(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetHideoutCustomisation(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_customizationController.GetHideoutCustomisation(sessionID, info));
    }

    /// <summary>
    /// Handle client/customization/storage
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetStorage(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_customizationController.GetCustomisationStorage(sessionID, info));
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
        return _customizationController.SetClothing(sessionID, info, pmcData);
    }
}
