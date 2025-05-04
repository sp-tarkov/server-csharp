using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairSortHelper(LocaleService _localeService)
{
    /**
     * Sort a list of ragfair offers by something (id/rating/offer name/price/expiry time)
     * @param offers Offers to sort
     * @param type How to sort it
     * @param direction Ascending/descending
     * @returns Sorted offers
     */
    public List<RagfairOffer> SortOffers(
        List<RagfairOffer> offers,
        RagfairSort type,
        int direction = 0
    )
    {
        // Sort results
        switch (type)
        {
            case RagfairSort.ID:
                offers.Sort(SortOffersByID);
                break;

            case RagfairSort.BARTER:
                offers.Sort(SortOffersByBarter);
                break;

            case RagfairSort.RATING:
                offers.Sort(SortOffersByRating);
                break;

            case RagfairSort.OFFER_TITLE:
                offers.Sort(
                    (a, b) =>
                    {
                        return SortOffersByName(a, b);
                    }
                );
                break;

            case RagfairSort.PRICE:
                offers.Sort(SortOffersByPrice);
                break;

            case RagfairSort.EXPIRY:
                offers.Sort(SortOffersByExpiry);
                break;
        }

        // 0=ASC 1=DESC
        if (direction == 1)
        {
            offers.Reverse();
        }

        return offers;
    }

    protected int SortOffersByID(RagfairOffer a, RagfairOffer b)
    {
        return a.InternalId.Value - b.InternalId.Value;
    }

    protected int SortOffersByBarter(RagfairOffer a, RagfairOffer b)
    {
        var aIsOnlyMoney =
            a.Requirements.Count == 1 && Money.GetMoneyTpls().Contains(a.Requirements[0].Template)
                ? 1
                : 0;
        var bIsOnlyMoney =
            b.Requirements.Count == 1 && Money.GetMoneyTpls().Contains(b.Requirements[0].Template)
                ? 1
                : 0;

        return aIsOnlyMoney - bIsOnlyMoney;
    }

    protected int SortOffersByRating(RagfairOffer a, RagfairOffer b)
    {
        return (int)(a.User.Rating.Value - b.User.Rating.Value);
    }

    protected int SortOffersByName(RagfairOffer a, RagfairOffer b)
    {
        var locale = _localeService.GetLocaleDb();

        var tplA = a.Items[0].Template;
        var tplB = b.Items[0].Template;
        var nameA = locale.GetValueOrDefault($"{tplA} Name", tplA);
        var nameB = locale.GetValueOrDefault($"{tplB} Name", tplB);

        return string.Compare(nameA, nameB);
    }

    /**
     * Order two offers by rouble price value
     * @param a Offer a
     * @param b Offer b
     * @returns
     */
    protected int SortOffersByPrice(RagfairOffer a, RagfairOffer b)
    {
        return (int)(a.RequirementsCost.Value - b.RequirementsCost.Value);
    }

    protected int SortOffersByExpiry(RagfairOffer a, RagfairOffer b)
    {
        return (int)(a.EndTime - b.EndTime);
    }
}
