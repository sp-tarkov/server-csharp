using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    ISptLogger<RagfairOfferHolder> logger,
    RagfairServerHelper ragfairServerHelper,
    ProfileHelper profileHelper,
    HashUtil hashUtil,
    LocalisationService localisationService,
    ConfigServer configServer)
{
    protected int _maxOffersPerTemplate = configServer.GetConfig<RagfairConfig>().Dynamic.OfferItemCount.Max;
    protected Dictionary<string, RagfairOffer> _offersById = new();
    protected readonly Lock _offersByIdLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTemplate = new(); // key = tplId, value = list of offerIds
    protected readonly Lock _offersByTemplateLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTrader = new(); // key = traderId, value = list of offerIds
    protected readonly Lock _offersByTraderLock = new();

    protected HashSet<string> _expiredOfferIds = [];
    protected readonly Lock _expiredOfferIdsLock = new();

    /// <summary>
    /// Get a ragfair offer by its id
    /// </summary>
    /// <param name="id">Ragfair offer id</param>
    /// <returns>RagfairOffer</returns>
    public RagfairOffer? GetOfferById(string id)
    {
        lock (_offersByIdLock)
        {
            return _offersById.GetValueOrDefault(id);
        }
    }

    /// <summary>
    /// Get ragfair offers that match the passed in tpl
    /// </summary>
    /// <param name="templateId">Tpl to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer>? GetOffersByTemplate(string templateId)
    {
        lock (_offersByTemplateLock)
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
    }

    /// <summary>
    /// Get all offers being sold by a trader
    /// </summary>
    /// <param name="traderId">Id of trader to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer>? GetOffersByTrader(string traderId)
    {
        lock (_offersByTraderLock)
        {
            if (!_offersByTrader.TryGetValue(traderId, out var offerIds))
            {
                return null;
            }

            return offerIds.Select(offerId => _offersById.GetValueOrDefault(offerId))
                .Where(offer => offer != null)
                .ToList();
        }
    }

    /// <summary>
    ///  Get all ragfair offers
    /// </summary>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer> GetOffers()
    {
        lock (_offersByIdLock)
        {
            if (_offersById.Count > 0)
            {
                return _offersById.Values.ToList();
            }
        }

        return [];
    }

    /// <summary>
    /// Add a collection of offers to ragfair
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
    /// Add single offer to ragfair
    /// </summary>
    /// <param name="offer">Offer to add</param>
    public void AddOffer(RagfairOffer offer)
    {
        lock (_offersByIdLock)
        {
            var sellerId = offer.User.Id;
            // Keep generating IDs until we get a unique one
            while (_offersById.ContainsKey(offer.Id))
            {
                offer.Id = new MongoId();
            }

            var itemTpl = offer.Items?.FirstOrDefault()?.Template;
            // If it is an NPC PMC offer AND we have already reached the maximum amount of possible offers
            // for this template, just don't add in more
            var sellerIsTrader = ragfairServerHelper.IsTrader(sellerId);
            if (itemTpl != null
                && !(sellerIsTrader || profileHelper.IsPlayer(sellerId))
                && _offersByTemplate.TryGetValue(itemTpl, out var offers)
                && offers?.Count >= _maxOffersPerTemplate
               )
            {
                return;
            }

            _offersById.Add(offer.Id, offer);

            if (sellerIsTrader)
            {
                AddOfferByTrader(sellerId, offer.Id);
            }

            AddOfferByTemplates(itemTpl, offer.Id);
        }
    }

    /// <summary>
    /// Remove an offer from ragfair by id
    /// </summary>
    /// <param name="offerId">Offer id to remove</param>
    /// <param name="checkTraderOffers">OPTIONAL - Should trader offers be checked for offer id</param>
    public void RemoveOffer(string offerId, bool checkTraderOffers = true)
    {
        lock (_offersByIdLock)
        {
            if (!_offersById.TryGetValue(offerId, out var offer))
            {
                logger.Warning(localisationService.GetText("ragfair-unable_to_remove_offer_doesnt_exist", offerId));
                return;
            }

            _offersById.Remove(offer.Id);

            if (checkTraderOffers)
            {
                lock (_offersByTraderLock)
                {
                    if (_offersByTrader.ContainsKey(offer.User.Id))
                    {
                        _offersByTrader[offer.User.Id].Remove(offer.Id);
                        // This was causing a memory leak, we need to make sure that we remove
                        // the user ID from the cached offers after they dont have anything else
                        // on the flea placed. We regenerate the ID for the NPC users, making it
                        // continuously grow otherwise
                        if (_offersByTrader[offer.User.Id].Count == 0)
                        {
                            _offersByTrader.Remove(offer.User.Id);
                        }
                    }
                }
            }

            lock (_offersByTemplateLock)
            {
                var firstItem = offer.Items.FirstOrDefault();
                if (_offersByTemplate.TryGetValue(firstItem.Template, out var offers))
                {
                    offers.Remove(offer.Id);
                }
            }
        }
    }

    /// <summary>
    /// Remove all offers a trader has
    /// </summary>
    /// <param name="traderId">Trader id to remove offers from</param>
    public void RemoveAllOffersByTrader(string traderId)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.TryGetValue(traderId, out var offerIdsToRemove))
            {
                foreach (var offerId in offerIdsToRemove)
                {
                    _offersById.Remove(offerId);
                }

                // Clear out linking table
                _offersByTrader[traderId].Clear();
            }
        }
    }

    /// <summary>
    /// Add offer to offersByTemplate cache
    /// </summary>
    /// <param name="template">Tpl to store offer against</param>
    /// <param name="offerId">Offer to store against tpl</param>
    protected void AddOfferByTemplates(string template, string offerId)
    {
        lock (_offersByTemplateLock)
        {
            if (_offersByTemplate.ContainsKey(template))
            {
                _offersByTemplate[template].Add(offerId);
            }
            else
            {
                _offersByTemplate.Add(template, [offerId]);
            }
        }
    }

    /// <summary>
    /// Cache an offer inside `offersByTrader` by trader id
    /// </summary>
    /// <param name="trader">Trader id to store offer against</param>
    /// <param name="offerId">Offer to store against</param>
    protected void AddOfferByTrader(string trader, string offerId)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(trader))
            {
                _offersByTrader[trader].Add(offerId);
            }
            else
            {
                _offersByTrader.Add(trader, [offerId]);
            }
        }
    }

    /// <summary>
    /// Is the passed in offer stale - end time > passed in time
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
            _expiredOfferIds.Add(staleOfferId);
        }
    }

    /// <summary>
    /// Get total count of current expired offers
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
    /// Get an array of arrays of expired offer items + children
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
                    logger.Warning($"offerId: {expiredOfferId} was not found !!");
                    continue;
                }
                if (offer?.Items?.Count == 0)
                {
                    logger.Error($"Unable to process expired offer: {expiredOfferId}, it has no items");

                    continue;
                }

                expiredItems.Add(offer.Items);
            }

            return expiredItems;
        }
    }

    /// <summary>
    /// Clear out internal expiredOffers dictionary of all items
    /// </summary>
    public void ResetExpiredOfferIds()
    {
        lock (_expiredOfferIdsLock)
        {
            _expiredOfferIds.Clear();
        }
    }

    /// <summary>
    /// Flag offers with an expiry before the passed in timestamp
    /// </summary>
    /// <param name="timestamp">Timestamp at point offer is 'expired'</param>
    public void FlagExpiredOffersAfterDate(long timestamp)
    {
        lock (_expiredOfferIdsLock)
        {
            foreach (var offer in GetOffers())
            {
                if (_expiredOfferIds.Contains(offer.Id) || ragfairServerHelper.IsTrader(offer.User.Id))
                {
                    // Already flagged or trader offer (handled separately), skip
                    continue;
                }

                if (IsStale(offer, timestamp))
                {
                    _expiredOfferIds.Add(offer.Id);
                }
            }
        }
    }

    /// <summary>
    /// Remove all offers flagged as stale/expired
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
