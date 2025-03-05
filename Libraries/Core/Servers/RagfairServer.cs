using Core.Generators;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Servers;

[Injectable]
public class RagfairServer(
    ISptLogger<RagfairServer> _logger,
    TimeUtil timeUtil,
    RagfairOfferService _ragfairOfferService,
    RagfairCategoriesService _ragfairCategoriesService,
    RagfairRequiredItemsService _ragfairRequiredItemsService,
    LocalisationService _localisationService,
    RagfairOfferGenerator _ragfairOfferGenerator,
    RagfairOfferHolder _ragfairOfferHolder,
    ConfigServer _configServer
)
{
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    public void Load()
    {
        _logger.Info(_localisationService.GetText("ragfair-generating_offers"));
        _ragfairOfferGenerator.GenerateDynamicOffers();
        Update();
    }

    public void Update()
    {
        // Generate trader offers
        var traders = GetUpdateableTraders();
        foreach (var traderId in traders)
        {
            // Edge case - skip generating fence offers
            if (traderId == Traders.FENCE)
            {
                continue;
            }

            if (_ragfairOfferService.TraderOffersNeedRefreshing(traderId))
            {
                _ragfairOfferGenerator.GenerateFleaOffersForTrader(traderId);
            }
        }

        // Regenerate expired offers when over threshold limit
        _ragfairOfferHolder.FlagExpiredOffersAfterDate(timeUtil.GetTimeStamp());
        if (_ragfairOfferService.EnoughExpiredOffersExistToProcess())
        {
            // Must occur BEFORE "RemoveExpiredOffers"
            var expiredAssortsWithChildren = _ragfairOfferHolder.GetExpiredOfferItems();

            // Replace the expired offers with new ones
            _ragfairOfferGenerator.GenerateDynamicOffers(expiredAssortsWithChildren);

            _ragfairOfferService.RemoveExpiredOffers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, true);
        }

        _ragfairRequiredItemsService.BuildRequiredItemTable();
    }

    /// <summary>
    /// Get traders who need to be periodically refreshed
    /// </summary>
    /// <returns> List of traders </returns>
    public List<string> GetUpdateableTraders()
    {
        return _ragfairConfig.Traders.Keys.ToList();
    }

    public Dictionary<string, int> GetAllActiveCategories(
        bool fleaUnlocked,
        SearchRequestData searchRequestData,
        List<RagfairOffer> offers
    )
    {
        return _ragfairCategoriesService.GetCategoriesFromOffers(offers, searchRequestData, fleaUnlocked);
    }

    /// <summary>
    /// Disable/Hide an offer from flea
    /// </summary>
    /// <param name="offerId"> OfferID to hide </param>
    public void HideOffer(string offerId)
    {
        var offers = _ragfairOfferService.GetOffers();
        var offer = offers.FirstOrDefault(x => x.Id == offerId);

        if (offer is null)
        {
            _logger.Error(_localisationService.GetText("ragfair-offer_not_found_unable_to_hide", offerId));

            return;
        }

        offer.Locked = true;
    }

    public RagfairOffer? GetOffer(string offerId)
    {
        return _ragfairOfferService.GetOfferByOfferId(offerId);
    }

    public List<RagfairOffer> GetOffers()
    {
        return _ragfairOfferService.GetOffers();
    }

    public void ReduceOfferQuantity(string offerId, int amount)
    {
        _ragfairOfferService.ReduceOfferQuantity(offerId, amount);
    }

    public bool DoesOfferExist(string offerId)
    {
        return _ragfairOfferService.DoesOfferExist(offerId);
    }

    public void AddPlayerOffers()
    {
        _ragfairOfferService.AddPlayerOffers();
    }
}
