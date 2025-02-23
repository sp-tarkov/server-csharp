using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.RagfairCallbacks)]
[Injectable(InjectableTypeOverride = typeof(IOnUpdate), TypePriority = OnUpdateOrder.RagfairCallbacks)]
[Injectable(InjectableTypeOverride = typeof(RagfairCallbacks))]
public class RagfairCallbacks(
    HttpResponseUtil _httpResponseUtil,
    RagfairServer _ragfairServer,
    RagfairController _ragfairController,
    RagfairTaxService _ragfairTaxService,
    RagfairPriceService _ragfairPriceService,
    ConfigServer _configServer
) : IOnLoad, IOnUpdate
{
    private readonly RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    public Task OnLoad()
    {
        _ragfairServer.Load();
        _ragfairPriceService.Load();
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-ragfair";
    }

    public bool OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > _ragfairConfig.RunIntervalSeconds)
        {
            // There is a flag inside this class that only makes it run once.
            _ragfairServer.AddPlayerOffers();

            // Check player offers and mail payment to player if sold
            _ragfairController.Update();

            // Process all offers / expire offers
            _ragfairServer.Update();

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Handle client/ragfair/search
    ///     Handle client/ragfair/find
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string Search(string url, SearchRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_ragfairController.GetOffers(sessionID, info));
    }

    /// <summary>
    ///     Handle client/ragfair/itemMarketPrice
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetMarketPrice(string url, GetMarketPriceRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_ragfairController.GetItemMinAvgMaxFleaPriceValues(info));
    }

    /// <summary>
    ///     Handle RagFairAddOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse AddOffer(PmcData pmcData, AddOfferRequestData info, string sessionID)
    {
        return _ragfairController.AddPlayerOffer(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle RagFairRemoveOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveOffer(PmcData pmcData, RemoveOfferRequestData info, string sessionID)
    {
        return _ragfairController.FlagOfferForRemoval(info.OfferId, sessionID);
    }

    /// <summary>
    ///     Handle RagFairRenewOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ExtendOffer(PmcData pmcData, ExtendOfferRequestData info, string sessionID)
    {
        return _ragfairController.ExtendOffer(info, sessionID);
    }

    /// <summary>
    ///     Handle /client/items/prices
    ///     Called when clicking an item to list on flea
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetFleaPrices(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_ragfairController.GetAllFleaPrices());
    }

    /// <summary>
    ///     Handle client/reports/ragfair/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SendReport(string url, SendRagfairReportRequestData info, string sessionID)
    {
        return _httpResponseUtil.NullResponse();
    }

    public string StorePlayerOfferTaxAmount(string url, StorePlayerOfferTaxAmountRequestData info, string sessionID)
    {
        _ragfairTaxService.StoreClientOfferTaxValue(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle client/ragfair/offer/findbyid
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetFleaOfferById(string url, GetRagfairOfferByIdRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_ragfairController.GetOfferById(sessionID, info));
    }
}
