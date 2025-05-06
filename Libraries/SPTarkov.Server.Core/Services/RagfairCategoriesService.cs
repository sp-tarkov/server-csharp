using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairCategoriesService(
    ISptLogger<RagfairCategoriesService> _logger,
    PaymentHelper _paymentHelper
)
{
    /// <summary>
    ///     Get a dictionary of each item the play can see in their flea menu, filtered by what is available for them to buy
    /// </summary>
    /// <param name="offers">All offers in flea</param>
    /// <param name="searchRequestData">Search criteria requested</param>
    /// <param name="fleaUnlocked">Can player see full flea yet (level 15 by default)</param>
    /// <returns>KVP of item tpls + count of offers</returns>
    public Dictionary<string, int> GetCategoriesFromOffers(
        List<RagfairOffer> offers,
        SearchRequestData searchRequestData,
        bool fleaUnlocked)
    {
        // Get offers valid for search request, then reduce them down to just the counts
        return offers
            .Where(offer =>
                {
                    var isTraderOffer = offer.User.MemberType == MemberCategory.Trader;

                    // Not level 15 and offer is from player, skip
                    if (!fleaUnlocked && !isTraderOffer)
                    {
                        return false;
                    }

                    // Skip items not for currency when `removeBartering` is enabled
                    if (
                        searchRequestData.RemoveBartering.GetValueOrDefault(false) &&
                        (offer.Requirements.Count > 1 || !_paymentHelper.IsMoneyTpl(offer.Requirements.FirstOrDefault().Template))
                    )
                    {
                        return false;
                    }

                    // Remove when filter set to players only + offer is from trader
                    if (searchRequestData.OfferOwnerType == OfferOwnerType.PLAYEROWNERTYPE && isTraderOffer)
                    {
                        return false;
                    }

                    // Remove when filter set to traders only + offer is not from trader
                    if (searchRequestData.OfferOwnerType == OfferOwnerType.TRADEROWNERTYPE && !isTraderOffer)
                    {
                        return false;
                    }

                    // Passed checks, it's a valid offer to process
                    return true;
                }
            )
            .GroupBy(x => x.Items.FirstOrDefault().Template)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}
