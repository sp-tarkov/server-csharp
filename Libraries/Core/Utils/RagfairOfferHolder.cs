using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Utils;

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
    protected object _offersByIdLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTemplate = new(); // key = tplId, value = list of offerIds
    protected object _offersByTemplateLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTrader = new(); // key = traderId, value = list of offerIds
    protected object _offersByTraderLock = new();

    protected HashSet<string> _expiredOfferIds = [];
    protected object _expiredOfferIdsLock = new();

    public RagfairOffer? GetOfferById(string id)
    {
        lock (_offersByIdLock)
        {
            return _offersById.GetValueOrDefault(id);
        }
    }

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

    public void AddOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers)
        {
            AddOffer(offer);
        }
    }

    public void AddOffer(RagfairOffer offer)
    {
        lock (_offersByIdLock)
        {
            var sellerId = offer.User.Id;
            // Keep generating IDs until we get a unique one
            while (_offersById.ContainsKey(offer.Id))
            {
                offer.Id = hashUtil.Generate();
            }

            var offerId = offer.Id;
            var itemTpl = offer.Items?.FirstOrDefault()?.Template;
            // If it is an NPC PMC offer AND we have already reached the maximum amount of possible offers
            // for this template, just don't add in more
            var sellerIsTrader = ragfairServerHelper.IsTrader(sellerId);
            if (itemTpl != null &&
                !(sellerIsTrader || profileHelper.IsPlayer(sellerId)) &&
                _offersByTemplate.TryGetValue(itemTpl, out var offers) &&
                offers?.Count >= _maxOffersPerTemplate
               )
            {
                return;
            }

            _offersById.Add(offerId, offer);

            if (sellerIsTrader)
            {
                AddOfferByTrader(sellerId, offer);
            }

            AddOfferByTemplates(itemTpl, offer);
        }
    }

    /**
     * Purge offer from offer cache
     * @param offer Offer to remove
     */
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

    protected void AddOfferByTemplates(string template, RagfairOffer offer)
    {
        lock (_offersByTemplateLock)
        {
            if (_offersByTemplate.ContainsKey(template))
            {
                _offersByTemplate[template].Add(offer.Id);
            }
            else
            {
                _offersByTemplate.Add(template, [offer.Id]);
            }
        }
    }

    protected void AddOfferByTrader(string trader, RagfairOffer offer)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(trader))
            {
                _offersByTrader[trader].Add(offer.Id);
            }
            else
            {
                _offersByTrader.Add(trader, [offer.Id]);
            }
        }
    }

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

    /**
     * Clear out internal expiredOffers dictionary of all items
     */
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
    /// <param name="timestamp"></param>
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
