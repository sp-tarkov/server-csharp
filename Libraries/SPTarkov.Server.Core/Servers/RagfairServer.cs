using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Servers;

[Injectable]
public class RagfairServer(
    ISptLogger<RagfairServer> logger,
    TimeUtil timeUtil,
    RagfairOfferService ragfairOfferService,
    RagfairCategoriesService ragfairCategoriesService,
    RagfairRequiredItemsService ragfairRequiredItemsService,
    ServerLocalisationService serverLocalisationService,
    RagfairOfferGenerator ragfairOfferGenerator,
    RagfairOfferHolder ragfairOfferHolder,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    public void Load()
    {
        logger.Info(serverLocalisationService.GetText("ragfair-generating_offers"));
        ragfairOfferGenerator.GenerateDynamicOffers();
        Update();
    }

    public void Update()
    {
        RefreshTraderOffers();
        ProcessExpiredFleaOffers();

        // Flag data as stale and in need of regeneration
        ragfairRequiredItemsService.CacheIsStale();
    }

    protected void RefreshTraderOffers()
    {
        // Generate/refresh trader offers - skip fence as his offers are separately handled
        var tradersToProcess = GetUpdateableTraders().Where(trader => trader != Traders.FENCE);
        foreach (var traderId in tradersToProcess)
        {
            // Each trader has its own expiry time
            if (ragfairOfferService.TraderOffersNeedRefreshing(traderId))
            {
                // Trader has passed its offer expiry time, update stock and reset offer times
                ragfairOfferGenerator.GenerateFleaOffersForTrader(traderId);
            }
        }
    }

    private void ProcessExpiredFleaOffers()
    {
        // Regenerate expired offers when over timestamp threshold
        ragfairOfferHolder.FlagExpiredOffersAfterDate(timeUtil.GetTimeStamp());

        if (!ragfairOfferService.EnoughExpiredOffersExistToProcess())
        {
            // Not enough expired offers to process, exit
            return;
        }

        // Must occur BEFORE "RemoveExpiredOffers" + clone items as they'll be purged by `RemoveExpiredOffers()`
        var expiredOfferItemsClone = cloner.Clone(ragfairOfferHolder.GetExpiredOfferItems());

        ragfairOfferService.RemoveExpiredOffers();

        // Force a cleanup+compact now all the expired offers are gone
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, true);

        if (expiredOfferItemsClone is not null)
        {
            // Replace the expired offers with new ones
            ragfairOfferGenerator.GenerateDynamicOffers(expiredOfferItemsClone);
        }
    }

    /// <summary>
    ///     Get traders who need to be periodically refreshed
    /// </summary>
    /// <returns> List of traders </returns>
    public List<MongoId> GetUpdateableTraders()
    {
        return _ragfairConfig.Traders.Keys.ToList();
    }

    public Dictionary<MongoId, int> GetAllActiveCategories(
        bool fleaUnlocked,
        SearchRequestData searchRequestData,
        IEnumerable<RagfairOffer> offers
    )
    {
        return ragfairCategoriesService.GetCategoriesFromOffers(offers, searchRequestData, fleaUnlocked);
    }

    /// <summary>
    ///     Disable/Hide an offer from flea
    /// </summary>
    /// <param name="offerId"> OfferID to hide </param>
    public void HideOffer(MongoId offerId)
    {
        var offers = ragfairOfferService.GetOffers();
        var offer = offers.FirstOrDefault(x => x.Id == offerId);

        if (offer is null)
        {
            logger.Error(serverLocalisationService.GetText("ragfair-offer_not_found_unable_to_hide", offerId));

            return;
        }

        offer.Locked = true;
    }

    public RagfairOffer? GetOffer(MongoId offerId)
    {
        return ragfairOfferService.GetOfferByOfferId(offerId);
    }

    public List<RagfairOffer> GetOffers()
    {
        return ragfairOfferService.GetOffers();
    }

    public void ReduceOfferQuantity(MongoId offerId, int amount)
    {
        ragfairOfferService.ReduceOfferQuantity(offerId, amount);
    }

    public bool DoesOfferExist(MongoId offerId)
    {
        return ragfairOfferService.DoesOfferExist(offerId);
    }

    public void AddPlayerOffers()
    {
        ragfairOfferService.AddPlayerOffers();
    }
}
