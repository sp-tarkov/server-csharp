using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Customization;
using Core.Models.Eft.ItemEvent;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

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
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetCustomisationUnlocks(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_saveServer.GetProfile(sessionID).CustomisationUnlocks);
    }

    /// <summary>
    ///     Handle client/trading/customization
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetTraderSuits(string url, EmptyRequestData info, string sessionID)
    {
        var splitUrl = url.Split('/');
        var traderId = splitUrl[^3];

        return _httpResponseUtil.GetBody(_customizationController.GetTraderSuits(traderId, sessionID));
    }

    /// <summary>
    ///     Handle CustomizationBuy event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse BuyCustomisation(PmcData pmcData, BuyClothingRequestData info, string sessionID)
    {
        return _customizationController.BuyCustomisation(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetHideoutCustomisation(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_customizationController.GetHideoutCustomisation(sessionID, info));
    }

    /// <summary>
    ///     Handle client/customization/storage
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public string GetStorage(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_customizationController.GetCustomisationStorage(sessionID, info));
    }

    /// <summary>
    ///     Handle CustomizationSet
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse SetCustomisation(PmcData pmcData, CustomizationSetRequest info, string sessionID)
    {
        return _customizationController.SetCustomisation(sessionID, info, pmcData);
    }
}
