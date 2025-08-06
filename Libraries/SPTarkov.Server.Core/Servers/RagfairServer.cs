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
    ISptLogger<RagfairServer> _logger,
    TimeUtil timeUtil,
    RagfairOfferService _ragfairOfferService,
    RagfairCategoriesService _ragfairCategoriesService,
    RagfairRequiredItemsService _ragfairRequiredItemsService,
    ServerLocalisationService _serverLocalisationService,
    RagfairOfferGenerator _ragfairOfferGenerator,
    RagfairOfferHolder _ragfairOfferHolder,
    ConfigServer _configServer,
    ICloner cloner
)
{
    protected readonly RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    public void Load()
    {
        _logger.Info(_serverLocalisationService.GetText("ragfair-generating_offers"));
        _ragfairOfferGenerator.GenerateDynamicOffers();
        Update();
    }

    public void Update()
    {
        RefreshTraderOffers();
        ProcessExpiredFleaOffers();

        // Update requirements now the offers have been expired/regenerated to ensure they're accurate
        _ragfairRequiredItemsService.BuildRequiredItemTable();
    }

    protected void RefreshTraderOffers()
    {
        // Generate/refresh trader offers - skip fence as his offers are separately handled
        var tradersToProcess = GetUpdateableTraders().Where(trader => trader != Traders.FENCE);
        foreach (var traderId in tradersToProcess)
        {
            // Each trader has its own expiry time
            if (_ragfairOfferService.TraderOffersNeedRefreshing(traderId))
            {
                // Trader has passed its offer expiry time, update stock and reset offer times
                _ragfairOfferGenerator.GenerateFleaOffersForTrader(traderId);
            }
        }
    }

    private void ProcessExpiredFleaOffers()
    {
        // Regenerate expired offers when over timestamp threshold
        _ragfairOfferHolder.FlagExpiredOffersAfterDate(timeUtil.GetTimeStamp());

        if (!_ragfairOfferService.EnoughExpiredOffersExistToProcess())
        {
            // Not enough expired offers to process, exit
            return;
        }

        // Must occur BEFORE "RemoveExpiredOffers" + clone items as they'll be purged by `RemoveExpiredOffers()`
        var expiredOfferItemsClone = cloner.Clone(_ragfairOfferHolder.GetExpiredOfferItems());

        _ragfairOfferService.RemoveExpiredOffers();

        // Force a cleanup+compact now all the expired offers are gone
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, true);

        if (expiredOfferItemsClone is not null)
        {
            // Replace the expired offers with new ones
            _ragfairOfferGenerator.GenerateDynamicOffers(expiredOfferItemsClone);
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
        return _ragfairCategoriesService.GetCategoriesFromOffers(offers, searchRequestData, fleaUnlocked);
    }

    /// <summary>
    ///     Disable/Hide an offer from flea
    /// </summary>
    /// <param name="offerId"> OfferID to hide </param>
    public void HideOffer(MongoId offerId)
    {
        var offers = _ragfairOfferService.GetOffers();
        var offer = offers.FirstOrDefault(x => x.Id == offerId);

        if (offer is null)
        {
            _logger.Error(_serverLocalisationService.GetText("ragfair-offer_not_found_unable_to_hide", offerId));

            return;
        }

        offer.Locked = true;
    }

    public RagfairOffer? GetOffer(MongoId offerId)
    {
        return _ragfairOfferService.GetOfferByOfferId(offerId);
    }

    public List<RagfairOffer> GetOffers()
    {
        return _ragfairOfferService.GetOffers();
    }

    public void ReduceOfferQuantity(MongoId offerId, int amount)
    {
        _ragfairOfferService.ReduceOfferQuantity(offerId, amount);
    }

    public bool DoesOfferExist(MongoId offerId)
    {
        return _ragfairOfferService.DoesOfferExist(offerId);
    }

    public void AddPlayerOffers()
    {
        _ragfairOfferService.AddPlayerOffers();
    }
}
