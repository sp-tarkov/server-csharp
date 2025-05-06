using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    ISptLogger<RagfairOfferHolder> _logger,
    RagfairServerHelper _ragfairServerHelper,
    ProfileHelper _profileHelper,
    HashUtil _hashUtil,
    LocalisationService _localisationService,
    ConfigServer _configServer
)
{
    protected readonly Lock _expiredOfferIdsLock = new();
    protected readonly Lock _ragfairOperationLock = new();

    protected HashSet<string> _expiredOfferIds = [];
    protected int _maxOffersPerTemplate = _configServer.GetConfig<RagfairConfig>().Dynamic.OfferItemCount.Max;
    protected ConcurrentDictionary<string, RagfairOffer> _offersById = new();
    protected ConcurrentDictionary<string, HashSet<string>> _offersByTemplate = new(); // key = tplId, value = list of offerIds
    protected ConcurrentDictionary<string, HashSet<string>> _offersByTrader = new(); // key = traderId, value = list of offerIds

    /// <summary>
    ///     Get a ragfair offer by its id
    /// </summary>
    /// <param name="id">Ragfair offer id</param>
    /// <returns>RagfairOffer</returns>
    public RagfairOffer? GetOfferById(string id)
    {
        return _offersById.GetValueOrDefault(id);
    }

    /// <summary>
    ///     Get ragfair offers that match the passed in tpl
    /// </summary>
    /// <param name="templateId">Tpl to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer>? GetOffersByTemplate(string templateId)
    {
        // Get the offerIds we want to return
        if (!_offersByTemplate.TryGetValue(templateId, out var offerIds))
        {
            return null;
        }

        var result = _offersById
            .Where(x => offerIds.Contains(x.Key))
            .Select(x => x.Value)
            .ToList();

        return result;
    }

    /// <summary>
    ///     Get all offers being sold by a trader
    /// </summary>
    /// <param name="traderId">Id of trader to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer>? GetOffersByTrader(string traderId)
    {
        if (!_offersByTrader.TryGetValue(traderId, out var offerIds))
        {
            return null;
        }

        return offerIds
            .Select(offerId => _offersById.GetValueOrDefault(offerId))
            .Where(offer => offer != null)
            .ToList();
    }

    /// <summary>
    ///     Get all ragfair offers
    /// </summary>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer> GetOffers()
    {
        if (_offersById.Count > 0)
        {
            return _offersById.Values.ToList();
        }

        return [];
    }

    /// <summary>
    ///     Add a collection of offers to ragfair
    /// </summary>
    /// <param name="offers">Offers to add</param>
    public void AddOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers)
        {
            AddOffer(offer);
        }
    }

    /// <summary>
    ///     Add single offer to ragfair
    /// </summary>
    /// <param name="offer">Offer to add</param>
    public void AddOffer(RagfairOffer offer)
    {
        lock (_ragfairOperationLock)
        {
            var sellerId = offer.User.Id;
            // Keep generating IDs until we get a unique one
            while (_offersById.ContainsKey(offer.Id))
            {
                offer.Id = _hashUtil.Generate();
            }

            var itemTpl = offer.Items?.FirstOrDefault()?.Template;
            // If it is an NPC PMC offer AND we have already reached the maximum amount of possible offers
            // for this template, just don't add in more
            var sellerIsTrader = _ragfairServerHelper.IsTrader(sellerId);
            if (
                itemTpl != null
                && !(sellerIsTrader || _profileHelper.IsPlayer(sellerId))
                && _offersByTemplate.TryGetValue(itemTpl, out var offers)
                && offers?.Count >= _maxOffersPerTemplate
            )
            {
                return;
            }

            if (!_offersById.TryAdd(offer.Id, offer))
            {
                _logger.Warning($"Offer: {offer.Id} already exists");
            }

            if (sellerIsTrader)
            {
                AddOfferByTrader(sellerId, offer.Id);
            }

            AddOfferByTemplates(itemTpl, offer.Id);
        }
    }

    /// <summary>
    ///     Remove an offer from ragfair by id
    /// </summary>
    /// <param name="offerId">Offer id to remove</param>
    /// <param name="checkTraderOffers">OPTIONAL - Should trader offers be checked for offer id</param>
    public void RemoveOffer(string offerId, bool checkTraderOffers = true)
    {
        if (!_offersById.TryGetValue(offerId, out var offer))
        {
            _logger.Warning(_localisationService.GetText("ragfair-unable_to_remove_offer_doesnt_exist", offerId));
            return;
        }

        if (!_offersById.TryRemove(offer.Id, out _))
        {
            _logger.Warning($"Unable to remove offer: {offer.Id}");
        }

        if (checkTraderOffers && _offersByTrader.ContainsKey(offer.User.Id))
        {
            _offersByTrader[offer.User.Id].Remove(offer.Id);
            // This was causing a memory leak, we need to make sure that we remove
            // the user ID from the cached offers after they dont have anything else
            // on the flea placed. We regenerate the ID for the NPC users, making it
            // continuously grow otherwise
            if (_offersByTrader[offer.User.Id].Count == 0)
            {
                if (!_offersByTrader.TryRemove(offer.User.Id, out _))
                {
                    _logger.Warning($"Unable to remove Trader offer: {offer.Id}");
                }
            }
        }

        var firstItem = offer.Items.FirstOrDefault();
        if (_offersByTemplate.TryGetValue(firstItem.Template, out var offers))
        {
            offers.Remove(offer.Id);
        }
    }

    /// <summary>
    ///     Remove all offers a trader has
    /// </summary>
    /// <param name="traderId">Trader id to remove offers from</param>
    public void RemoveAllOffersByTrader(string traderId)
    {
        if (_offersByTrader.TryGetValue(traderId, out var offerIdsToRemove))
        {
            foreach (var offerId in offerIdsToRemove)
            {
                if (!_offersById.TryRemove(offerId, out _))
                {
                    _logger.Warning($"Unable to remove offer: {offerId}");
                }
            }

            // Clear out linking table
            _offersByTrader[traderId].Clear();
        }
    }

    /// <summary>
    ///     Add offer to offersByTemplate cache
    /// </summary>
    /// <param name="template">Tpl to store offer against</param>
    /// <param name="offerId">Offer to store against tpl</param>
    protected void AddOfferByTemplates(string template, string offerId)
    {
        if (_offersByTemplate.ContainsKey(template))
        {
            _offersByTemplate[template].Add(offerId);
            return;
        }

        if (!_offersByTemplate.TryAdd(template, [offerId]))
        {
            _logger.Warning($"Unable to add offer: {offerId} to offersByTemplate");
        }
    }

    /// <summary>
    ///     Cache an offer inside `offersByTrader` by trader id
    /// </summary>
    /// <param name="trader">Trader id to store offer against</param>
    /// <param name="offerId">Offer to store against</param>
    protected void AddOfferByTrader(string trader, string offerId)
    {
        if (_offersByTrader.ContainsKey(trader))
        {
            _offersByTrader[trader].Add(offerId);
            return;
        }

        if (!_offersByTrader.TryAdd(trader, [offerId]))
        {
            _logger.Error($"Unable to add offer: {offerId} to offersByTrader");
        }
    }

    /// <summary>
    ///     Is the passed in offer stale - end time > passed in time
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <param name="time">Time to check offer against</param>
    /// <returns>True - offer is stale</returns>
    protected bool IsStale(RagfairOffer? offer, long time)
    {
        if (offer is null)
        {
            return false;
        }

        return offer.EndTime < time || (offer.Quantity ?? 0) < 1;
    }

    /// <summary>
    ///     Add a stale offers id to collection for later use
    /// </summary>
    /// <param name="staleOfferId">Id of offer to add to stale collection</param>
    public void FlagOfferAsExpired(string staleOfferId)
    {
        lock (_expiredOfferIdsLock)
        {
            if (!_expiredOfferIds.Add(staleOfferId))
            {
                _logger.Warning($"Unable to add offer: {staleOfferId} to expired offers");
            }
        }
    }

    /// <summary>
    ///     Get total count of current expired offers
    /// </summary>
    /// <returns>Number of expired offers</returns>
    public int GetExpiredOfferCount()
    {
        lock (_expiredOfferIdsLock)
        {
            return _expiredOfferIds.Count;
        }
    }

    /// <summary>
    ///     Get an array of arrays of expired offer items + children
    /// </summary>
    /// <returns>Expired offer assorts</returns>
    public List<List<Item>> GetExpiredOfferItems()
    {
        lock (_expiredOfferIdsLock)
        {
            // list of lists of item+children
            var expiredItems = new List<List<Item>>();
            foreach (var expiredOfferId in _expiredOfferIds)
            {
                var offer = GetOfferById(expiredOfferId);
                if (offer is null)
                {
                    _logger.Warning($"offerId: {expiredOfferId} was not found !!");
                    continue;
                }

                if (offer?.Items?.Count == 0)
                {
                    _logger.Error($"Unable to process expired offer: {expiredOfferId}, it has no items");
                    continue;
                }

                expiredItems.Add(offer.Items);
            }

            return expiredItems;
        }
    }

    /// <summary>
    ///     Clear out internal expiredOffers dictionary of all items
    /// </summary>
    public void ResetExpiredOfferIds()
    {
        lock (_expiredOfferIdsLock)
        {
            _expiredOfferIds.Clear();
        }
    }

    /// <summary>
    ///     Flag offers with an expiry before the passed in timestamp
    /// </summary>
    /// <param name="timestamp">Timestamp at point offer is 'expired'</param>
    public void FlagExpiredOffersAfterDate(long timestamp)
    {
        lock (_expiredOfferIdsLock)
        {
            foreach (var offer in GetOffers())
            {
                if (_expiredOfferIds.Contains(offer.Id) || _ragfairServerHelper.IsTrader(offer.User.Id))
                {
                    // Already flagged or trader offer (handled separately), skip
                    continue;
                }

                if (IsStale(offer, timestamp))
                {
                    if (!_expiredOfferIds.Add(offer.Id))
                    {
                        _logger.Warning($"Unable to add offer: {offer.Id} to expired offers");
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Remove all offers flagged as stale/expired
    /// </summary>
    public void RemoveExpiredOffers()
    {
        lock (_expiredOfferIdsLock)
        {
            foreach (var expiredOfferId in _expiredOfferIds)
            {
                RemoveOffer(expiredOfferId, false);
            }
        }
    }
}
