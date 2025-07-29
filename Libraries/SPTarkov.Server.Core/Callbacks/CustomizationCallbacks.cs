using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Customization;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class CustomizationCallbacks(
    CustomizationController customizationController,
    SaveServer saveServer,
    HttpResponseUtil httpResponseUtil
)
{
    /// <summary>
    ///     Handle client/trading/customization/storage
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCustomisationUnlocks(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(saveServer.GetProfile(sessionID).CustomisationUnlocks));
    }

    /// <summary>
    ///     Handle client/trading/customization
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetTraderSuits(string url, EmptyRequestData _, MongoId sessionID)
    {
        var splitUrl = url.Split('/');
        var traderId = splitUrl[^3];

        return new ValueTask<string>(httpResponseUtil.GetBody(customizationController.GetTraderSuits(traderId, sessionID)));
    }

    /// <summary>
    ///     Handle CustomizationBuy event
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse BuyCustomisation(PmcData pmcData, BuyClothingRequestData request, MongoId sessionID)
    {
        return customizationController.BuyCustomisation(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle client/hideout/customization/offer/list
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetHideoutCustomisation(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(customizationController.GetHideoutCustomisation(sessionID)));
    }

    /// <summary>
    ///     Handle client/customization/storage
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetStorage(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(customizationController.GetCustomisationStorage(sessionID)));
    }

    /// <summary>
    ///     Handle CustomizationSet
    /// </summary>
    /// <returns></returns>
    public ItemEventRouterResponse SetCustomisation(PmcData pmcData, CustomizationSetRequest request, MongoId sessionID)
    {
        return customizationController.SetCustomisation(sessionID, request, pmcData);
    }
}
