using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferService
{
    /// <summary>
    /// Get all offers
    /// </summary>
    /// <returns>List of RagfairOffer</returns>
    public List<RagfairOffer> GetOffers()
    {
        throw new NotImplementedException();
    }

    public RagfairOffer? GetOfferByOfferId(string offerId)
    {
        throw new NotImplementedException();
    }

    public List<RagfairOffer> GetOffersOfType(string templateId)
    {
        throw new NotImplementedException();
    }

    public void AddOffer(RagfairOffer offer)
    {
        throw new NotImplementedException();
    }

    public void AddOfferToExpired(RagfairOffer staleOffer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get total count of current expired offers
    /// </summary>
    /// <returns>Number of expired offers</returns>
    public int GetExpiredOfferCount()
    {
        Console.WriteLine($"actually implement me plz: owo: GetExpiredOfferCount");
        return 0;
    }

    /// <summary>
    /// Get a list of lists of expired offer items + children
    /// </summary>
    /// <returns>Expired offer assorts</returns>
    public List<List<Item>> GetExpiredOfferAssorts()
    {
        Console.WriteLine($"actually implement me plz: owo: GetExpiredOfferAssorts");
        return new List<List<Item>>();
    }

    /// <summary>
    /// Clear out internal expiredOffers dictionary of all items
    /// </summary>
    public void ResetExpiredOffers()
    {
        Console.WriteLine($"actually implement me plz: owo: ResetExpiredOffers");
    }

    /// <summary>
    /// Does the offer exist on the ragfair
    /// </summary>
    /// <param name="offerId">offer id to check for</param>
    /// <returns>offer exists - true</returns>
    public bool DoesOfferExist(string offerId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove an offer from ragfair by offer id
    /// </summary>
    /// <param name="offerId">Offer id to remove</param>
    public void RemoveOfferById(string offerId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reduce size of an offer stack by specified amount
    /// </summary>
    /// <param name="offerId">Offer to adjust stack size of</param>
    /// <param name="amount">How much to deduct from offers stack size</param>
    public void RemoveOfferStack(string offerId, int amount)
    {
        throw new NotImplementedException();
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Do the trader offers on flea need to be refreshed
    /// </summary>
    /// <param name="traderID">Trader to check</param>
    /// <returns>true if they do</returns>
    public bool TraderOffersNeedRefreshing(string traderID)
    {
        Console.WriteLine($"actually implement me plz: owo: TraderOffersNeedRefreshing");
        return false;
    }

    public void AddPlayerOffers()
    {
        Console.WriteLine($"actually implement me plz: owo: AddPlayerOffers");
        // throw new NotImplementedException();
    }

    public void ExpireStaleOffers()
    {
        Console.WriteLine($"actually implement me plz: owo: ExpireStaleOffers");
    }

    /// <summary>
    /// Remove stale offer from flea
    /// </summary>
    /// <param name="staleOffer">Stale offer to process</param>
    protected void ProcessStaleOffer(RagfairOffer staleOffer)
    {
        throw new NotImplementedException();
    }

    protected void ReturnPlayerOffer(RagfairOffer playerOffer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Flea offer items are stacked up often beyond the StackMaxSize limit
    /// Unstack the items into a list of root items and their children
    /// Will create new items equal to the
    /// </summary>
    /// <param name="items">Offer items to unstack</param>
    /// <returns>Unstacked list of items</returns>
    protected List<Item> UnstackOfferItems(List<Item> items)
    {
        throw new NotImplementedException();
    }
}
