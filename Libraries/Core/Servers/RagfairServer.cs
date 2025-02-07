using SptCommon.Annotations;
using Core.Generators;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Services;

namespace Core.Servers;

[Injectable]
public class RagfairServer(
    ISptLogger<RagfairServer> _logger,
    RagfairOfferService _ragfairOfferService,
    RagfairCategoriesService _ragfairCategoriesService,
    RagfairRequiredItemsService _ragfairRequiredItemsService,
    LocalisationService _localisationService,
    RagfairOfferGenerator _ragfairOfferGenerator,
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
            if (traderId == Traders.FENCE) continue;

            if (_ragfairOfferService.TraderOffersNeedRefreshing(traderId)) _ragfairOfferGenerator.GenerateFleaOffersForTrader(traderId);
        }

        // Regenerate expired offers when over threshold limit
        if (_ragfairOfferService.GetExpiredOfferCount() >= _ragfairConfig.Dynamic.ExpiredOfferThreshold)
        {
            // Must occur BEFORE "ExpireStaleOffers"
            var expiredAssortsWithChildren = _ragfairOfferService.GetExpiredOfferAssorts();
            _ragfairOfferGenerator.GenerateDynamicOffers(expiredAssortsWithChildren);

            _ragfairOfferService.ExpireStaleOffers();

            // Clear out expired offers now we've regenerated them
            _ragfairOfferService.ResetExpiredOfferIds();
        }

        _ragfairRequiredItemsService.BuildRequiredItemTable();
    }

    /**
 * Get traders who need to be periodically refreshed
 * @returns string array of traders
 */
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

    /**
 * Disable/Hide an offer from flea
 * @param offerId
 */
    public void HideOffer(string offerId)
    {
        var offers = _ragfairOfferService.GetOffers();
        var offer = offers.FirstOrDefault((x) => x.Id == offerId);

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

    public void RemoveOfferStack(string offerId, int amount)
    {
        _ragfairOfferService.RemoveOfferStack(offerId, amount);
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
