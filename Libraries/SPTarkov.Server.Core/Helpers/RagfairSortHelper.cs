using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairSortHelper(LocaleService localeService)
{
    /// <summary>
    /// Sort a list of ragfair offers by something (id/rating/offer name/price/expiry time)
    /// </summary>
    /// <param name="offers">Offers to sort</param>
    /// <param name="type">How to sort it</param>
    /// <param name="direction">Ascending/descending</param>
    /// <returns>Sorted offers</returns>
    public List<RagfairOffer> SortOffers(List<RagfairOffer> offers, RagfairSort type, int direction = 0)
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
                offers.Sort(SortOffersByName);
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
        var aIsOnlyMoney = a.Requirements.Count() == 1 && Money.GetMoneyTpls().Contains(a.Requirements.First().TemplateId) ? 1 : 0;
        var bIsOnlyMoney = b.Requirements.Count() == 1 && Money.GetMoneyTpls().Contains(b.Requirements.First().TemplateId) ? 1 : 0;

        return aIsOnlyMoney - bIsOnlyMoney;
    }

    protected int SortOffersByRating(RagfairOffer a, RagfairOffer b)
    {
        var ratingA = a.User?.Rating ?? 0.0;
        var ratingB = b.User?.Rating ?? 0.0;

        return ratingA.CompareTo(ratingB);
    }

    protected int SortOffersByName(RagfairOffer a, RagfairOffer b)
    {
        var locale = localeService.GetLocaleDb();

        var tplA = a.Items[0].Template;
        var tplB = b.Items[0].Template;
        var nameA = locale.GetValueOrDefault($"{tplA} Name", tplA);
        var nameB = locale.GetValueOrDefault($"{tplB} Name", tplB);

        return string.CompareOrdinal(nameA, nameB);
    }

    /// <summary>
    /// Order two offers by rouble price value
    /// </summary>
    /// <param name="a">Offer a</param>
    /// <param name="b">Offer b</param>
    /// <returns>-1, 0, 1</returns>
    protected int SortOffersByPrice(RagfairOffer a, RagfairOffer b)
    {
        return (int)(a.RequirementsCost.Value - b.RequirementsCost.Value);
    }

    /// <summary>
    /// Order two offers by rouble price value
    /// </summary>
    /// <param name="a">Offer a</param>
    /// <param name="b">Offer b</param>
    /// <returns>-1, 0, 1</returns>
    protected int SortOffersByExpiry(RagfairOffer a, RagfairOffer b)
    {
        return (int)((a.EndTime ?? 0) - (b.EndTime ?? 0));
    }
}
