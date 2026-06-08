using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Services.Ragfair;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks)]
public class RagfairCallbacks(
    HttpResponseUtil httpResponseUtil,
    RagfairServer ragfairServer,
    RagfairController ragfairController,
    RagfairTaxService ragfairTaxService,
    RagfairPriceService ragfairPriceService,
    RagfairOfferService ragfairOfferService,
    RagfairConfig ragfairConfig
) : IOnLoad, IOnUpdate
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ragfairPriceService.Load();
        ragfairServer.Load();

        return Task.CompletedTask;
    }

    public Task<bool> OnUpdateAsync(long secondsSinceLastRun, CancellationToken cancellationToken)
    {
        if (secondsSinceLastRun < ragfairConfig.RunIntervalSeconds)
        {
            // Not enough time has passed since last run, exit early
            return Task.FromResult(false);
        }

        // There is a flag inside this class that only makes it run once.
        ragfairOfferService.AddPlayerOffers();

        // Check player offers and mail payment to player if sold
        ragfairController.Update();

        // Process all offers / expire offers
        ragfairServer.Update();

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle client/ragfair/search
    ///     Handle client/ragfair/find
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> Search(string url, SearchRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(ragfairController.GetOffers(sessionID, info)));
    }

    /// <summary>
    ///     Handle client/ragfair/itemMarketPrice
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetMarketPrice(string url, GetMarketPriceRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(ragfairController.GetItemMinAvgMaxFleaPriceValues(info)));
    }

    /// <summary>
    ///     Handle RagFairAddOffer event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> AddOffer(PmcData pmcData, AddOfferRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(ragfairController.AddPlayerOffer(pmcData, info, sessionID));
    }

    /// <summary>
    ///     Handle RagFairRemoveOffer event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> RemoveOffer(PmcData pmcData, RemoveOfferRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(ragfairController.FlagOfferForRemoval(info.OfferId, sessionID));
    }

    /// <summary>
    ///     Handle RagFairRenewOffer event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<ItemEventRouterResponse> ExtendOffer(PmcData pmcData, ExtendOfferRequestData info, MongoId sessionID)
    {
        return new ValueTask<ItemEventRouterResponse>(ragfairController.ExtendOffer(info, sessionID));
    }

    /// <summary>
    ///     Handle /client/items/prices
    ///     Called when clicking an item to list on flea
    /// </summary>
    /// <param name="url"></param>
    /// <param name="_"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetFleaPrices(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(ragfairController.GetAllFleaPrices()));
    }

    /// <summary>
    ///     Handle client/reports/ragfair/send
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> SendReport(string url, SendRagfairReportRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> StorePlayerOfferTaxAmount(string url, StorePlayerOfferTaxAmountRequestData info, MongoId sessionID)
    {
        ragfairTaxService.StoreClientOfferTaxValue(sessionID, info);
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle client/ragfair/offer/findbyid
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetFleaOfferById(string url, GetRagfairOfferByIdRequest info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(ragfairController.GetOfferByInternalId(sessionID, info)));
    }
}
