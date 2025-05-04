using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Customization;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class CustomizationCallbacks(
    CustomizationController _customizationController,
    SaveServer _saveServer,
    HttpResponseUtil _httpResponseUtil
)
{
    /// <summary>
    ///     Handle client/trading/customization/storage
    /// </summary>
    /// <returns></returns>
    public string GetCustomisationUnlocks(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_saveServer.GetProfile(sessionID).CustomisationUnlocks);
    }

    /// <summary>
    ///     Handle client/trading/customization
    /// </summary>
    /// <returns></returns>
    public string GetTraderSuits(string url, EmptyRequestData _, string sessionID)
    {
        var splitUrl = url.Split('/');
        var traderId = splitUrl[^3];

        return _httpResponseUtil.GetBody(
            _customizationController.GetTraderSuits(traderId, sessionID)
        );
    }

    /// <summary>
    ///     Handle CustomizationBuy event
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse BuyCustomisation(
        PmcData pmcData,
        BuyClothingRequestData request,
        string sessionID
    )
    {
        return _customizationController.BuyCustomisation(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle client/hideout/customization/offer/list
    /// </summary>
    /// <returns></returns>
    public string GetHideoutCustomisation(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(
            _customizationController.GetHideoutCustomisation(sessionID)
        );
    }

    /// <summary>
    ///     Handle client/customization/storage
    /// </summary>
    /// <returns></returns>
    public string GetStorage(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(
            _customizationController.GetCustomisationStorage(sessionID)
        );
    }

    /// <summary>
    ///     Handle CustomizationSet
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse SetCustomisation(
        PmcData pmcData,
        CustomizationSetRequest request,
        string sessionID
    )
    {
        return _customizationController.SetCustomisation(sessionID, request, pmcData);
    }
}
