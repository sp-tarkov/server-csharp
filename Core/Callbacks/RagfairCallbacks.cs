using Core.Annotations;
using Core.Controllers;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks)]
public class RagfairCallbacks : OnLoad, OnUpdate
{
    protected HttpResponseUtil _httpResponseUtil;
    // protected RagfairServer _ragfairServer;
    protected RagfairController _ragfairController;
    protected RagfairTaxService _ragfairTaxService;
    protected ConfigServer _configServer;

    private RagfairConfig _ragfairConfig;

    public RagfairCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        // RagfairServer ragfairServer,
        RagfairController ragfairController,
        RagfairTaxService ragfairTaxService,
        ConfigServer configServer
    )
    {
        _httpResponseUtil = httpResponseUtil;
        // _ragfairServer = ragfairServer;
        _ragfairController = ragfairController;
        _ragfairTaxService = ragfairTaxService;
        _configServer = configServer;
        _ragfairConfig = _configServer.GetConfig<RagfairConfig>(ConfigTypes.RAGFAIR);
    }

    public async Task OnLoad()
    {
        // await _ragfairServer.Load();
        // TODO: implement RagfairServer
    }

    public string GetRoute()
    {
        return "spt-ragfair";
    }

    public async Task<bool> OnUpdate(long timeSinceLastRun)
    {
        // if (timeSinceLastRun > this.ragfairConfig.runIntervalSeconds) {
        //     // There is a flag inside this class that only makes it run once.
        //     this.ragfairServer.addPlayerOffers();
        //
        //     // Check player offers and mail payment to player if sold
        //     this.ragfairController.update();
        //
        //     // Process all offers / expire offers
        //     await this.ragfairServer.update();
        //
        //     return true;
        // }
        // return false;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/ragfair/search
    /// Handle client/ragfair/find
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
    /// Handle client/ragfair/itemMarketPrice
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
    /// Handle RagFairAddOffer event
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
    /// Handle RagFairRemoveOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse RemoveOffer(PmcData pmcData, RemoveOfferRequestData info, string sessionID)
    {
        return _ragfairController.RemoveOffer(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle RagFairRenewOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ExtendOffer(PmcData pmcData, ExtendOfferRequestData info, string sessionID)
    {
        return _ragfairController.ExtendOffer(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle /client/items/prices
    /// Called when clicking an item to list on flea
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
    /// Handle client/reports/ragfair/send
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
    /// Handle client/ragfair/offer/findbyid
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
