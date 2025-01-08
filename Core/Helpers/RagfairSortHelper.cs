using Core.Models.Eft.Ragfair;
using Core.Models.Enums;

namespace Core.Helpers;

public class RagfairSortHelper
{
    /**
     * Sort a list of ragfair offers by something (id/rating/offer name/price/expiry time)
     * @param offers Offers to sort
     * @param type How to sort it
     * @param direction Ascending/descending
     * @returns Sorted offers
     */
    public List<RagfairOffer> SortOffers(List<RagfairOffer> offers, RagfairSort type, int direction = 0)
    {
        throw new NotImplementedException();
    }

    protected int SortOffersByID(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }

    protected int SortOffersByBarter(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }

    protected int SortOffersByRating(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }

    protected int SortOffersByName(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }

    /**
     * Order two offers by rouble price value
     * @param a Offer a
     * @param b Offer b
     * @returns
     */
    protected int SortOffersByPrice(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }

    protected int SortOffersByExpiry(RagfairOffer a, RagfairOffer b)
    {
        throw new NotImplementedException();
    }
}
