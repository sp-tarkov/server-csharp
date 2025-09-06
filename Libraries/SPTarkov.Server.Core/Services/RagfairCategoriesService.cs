using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairCategoriesService(ISptLogger<RagfairCategoriesService> logger, PaymentHelper paymentHelper)
{
    /// <summary>
    ///     Get a dictionary of each item the play can see in their flea menu, filtered by what is available for them to buy
    /// </summary>
    /// <param name="offers">All offers in flea</param>
    /// <param name="searchRequestData">Search criteria requested</param>
    /// <param name="fleaUnlocked">Can player see full flea yet (level 15 by default)</param>
    /// <returns>KVP of item tpls + count of offers</returns>
    public Dictionary<MongoId, int> GetCategoriesFromOffers(
        IEnumerable<RagfairOffer> offers,
        SearchRequestData searchRequestData,
        bool fleaUnlocked
    )
    {
        // Get offers valid for search request, then reduce them down to just the counts
        return offers
            .Where(offer =>
            {
                var isTraderOffer = offer.IsTraderOffer();

                // Not level 15 and offer is from player, skip
                if (!fleaUnlocked && !isTraderOffer)
                {
                    return false;
                }

                // Skip when:
                // Not a 'required' search
                // Remove barters checkbox checked
                // Offer requirement has children or requirement is not money
                if (
                    string.IsNullOrEmpty(searchRequestData.NeededSearchId)
                    && searchRequestData.RemoveBartering.GetValueOrDefault(false)
                    && (offer.Requirements.Count() > 1 || !paymentHelper.IsMoneyTpl(offer.Requirements.FirstOrDefault().TemplateId))
                )
                {
                    return false;
                }

                // Remove when filter set to players only + offer is from trader
                if (searchRequestData.OfferOwnerType == OfferOwnerType.PlayerOwnerType && isTraderOffer)
                {
                    return false;
                }

                // Remove when filter set to traders only + offer is not from trader
                if (searchRequestData.OfferOwnerType == OfferOwnerType.TraderOwnerType && !isTraderOffer)
                {
                    return false;
                }

                // Passed checks, it's a valid offer to process
                return true;
            })
            .GroupBy(x => x.Items.FirstOrDefault().Template)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}
