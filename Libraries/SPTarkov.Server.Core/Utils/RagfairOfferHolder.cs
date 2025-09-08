using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    ISptLogger<RagfairOfferHolder> logger,
    RagfairServerHelper ragfairServerHelper,
    ServerLocalisationService serverLocalisationService,
    ItemHelper itemHelper
)
{
    /// <summary>
    /// Expired offer Ids
    /// </summary>
    private readonly ConcurrentDictionary<MongoId, byte> _expiredOfferIds = [];

    /// <summary>
    /// Ragfair offer cache, keyed by offer Id
    /// </summary>
    private readonly ConcurrentDictionary<MongoId, RagfairOffer> _offersById = new();

    /// <summary>
    /// Offer Ids keyed by tpl
    /// </summary>
    private readonly ConcurrentDictionary<MongoId, HashSet<MongoId>> _offersByTemplate = new();

    /// <summary>
    /// Offer ids keyed by trader Id
    /// </summary>
    private readonly ConcurrentDictionary<MongoId, HashSet<MongoId>> _offersByTrader = new();

    /// <summary>
    /// Fake player offer ids keyed by itemTPl
    /// </summary>
    private readonly ConcurrentDictionary<MongoId, HashSet<MongoId>> _fakePlayerOffers = new();

    private readonly Lock _processExpiredOffersLock = new();
    private readonly Lock _ragfairOperationLock = new();

    /// <summary>
    ///     Get a ragfair offer by its id
    /// </summary>
    /// <param name="id">Ragfair offer id</param>
    /// <returns>RagfairOffer</returns>
    public RagfairOffer? GetOfferById(MongoId id)
    {
        return _offersById.GetValueOrDefault(id);
    }

    /// <summary>
    ///     Get a ragfair offer by its id
    /// </summary>
    /// <returns>RagfairOffer</returns>
    public List<MongoId> GetStaleOfferIds()
    {
        lock (_processExpiredOffersLock)
        {
            return _expiredOfferIds.Keys.ToList();
        }
    }

    /// <summary>
    ///     Get ragfair offers that match the passed in tpl
    /// </summary>
    /// <param name="templateId">Tpl to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public IEnumerable<RagfairOffer>? GetOffersByTemplate(MongoId templateId)
    {
        // Get the offerIds we want to return
        if (!_offersByTemplate.TryGetValue(templateId, out var offerIds))
        {
            return null;
        }

        var result = _offersById.Where(x => offerIds.Contains(x.Key)).Select(x => x.Value);

        return result;
    }

    /// <summary>
    ///     Get all offers being sold by a trader
    /// </summary>
    /// <param name="traderId">Id of trader to get offers for</param>
    /// <returns>RagfairOffer list</returns>
    public IEnumerable<RagfairOffer> GetOffersByTrader(MongoId traderId)
    {
        if (!_offersByTrader.TryGetValue(traderId, out var offerIds))
        {
            return [];
        }

        return offerIds.Select(offerId => _offersById.GetValueOrDefault(offerId)).Where(offer => offer != null);
    }

    /// <summary>
    ///     Get all ragfair offers
    /// </summary>
    /// <returns>RagfairOffer list</returns>
    public List<RagfairOffer> GetOffers()
    {
        return _offersById.IsEmpty ? [] : _offersById.Values.ToList();
    }

    /// <summary>
    ///     Add a collection of offers to ragfair
    /// </summary>
    /// <param name="offers">Offers to add</param>
    public void AddOffers(IEnumerable<RagfairOffer> offers)
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
            // Keep generating IDs until we get a unique one
            while (_offersById.ContainsKey(offer.Id))
            {
                offer.Id = new MongoId();
            }

            var itemTpl = offer.Items?.FirstOrDefault()?.Template ?? new MongoId();
            if (
                !itemTpl.IsEmpty // Has tpl
                && offer.IsFakePlayerOffer()
                && _fakePlayerOffers.TryGetValue(itemTpl, out var offers)
                && offers?.Count >= ragfairServerHelper.GetOfferCountByBaseType(itemHelper.GetItem(itemTpl).Value.Parent)
            )
            {
                // If it is an NPC PMC offer AND we have already reached the maximum amount of possible offers
                // for this template, don't add more
                return;
            }

            if (!_offersById.TryAdd(offer.Id, offer))
            {
                logger.Warning($"Offer: {offer.Id} already exists");
            }

            if (offer.IsTraderOffer())
            {
                AddOfferByTrader(offer.User.Id, offer.Id);
            }

            if (offer.IsFakePlayerOffer())
            {
                AddFakePlayerOffer(itemTpl, offer.Id);
            }

            AddOfferByTemplates(itemTpl, offer.Id);
        }
    }

    /// <summary>
    ///     Remove an offer from ragfair by id
    /// </summary>
    /// <param name="offerId">Offer id to remove</param>
    /// <param name="checkTraderOffers">OPTIONAL - Should trader offers be checked for offer id</param>
    public void RemoveOffer(MongoId offerId, bool checkTraderOffers = true)
    {
        if (!_offersById.TryGetValue(offerId, out var offer))
        {
            logger.Warning(serverLocalisationService.GetText("ragfair-unable_to_remove_offer_doesnt_exist", offerId));

            return;
        }

        if (!_offersById.TryRemove(offer.Id, out _))
        {
            logger.Warning($"Unable to remove offer by id: {offer.Id} not found");
        }

        if (checkTraderOffers && _offersByTrader.TryGetValue(offer.User.Id, out var traderOfferIds))
        {
            traderOfferIds.Remove(offer.Id);

            if (traderOfferIds.Count == 0)
            {
                // Potential memory leak
                // Users with no offers were never cleaned up
                if (!_offersByTrader.TryRemove(offer.User.Id, out _))
                {
                    logger.Warning($"Unable to remove Trader offer: {offer.Id} not found");
                }
            }
        }

        var rootItem = offer.Items.FirstOrDefault();
        if (_offersByTemplate.TryGetValue(rootItem.Template, out var offers))
        {
            offers.Remove(offer.Id);
        }

        if (offer.IsFakePlayerOffer() && _fakePlayerOffers.TryGetValue(offer.Items.FirstOrDefault().Template, out var fakePlayerOfferIds))
        {
            fakePlayerOfferIds.Remove(offer.Id);
        }
    }

    /// <summary>
    ///     Remove all offers a trader has
    /// </summary>
    /// <param name="traderId">Trader id to remove offers from</param>
    public void RemoveAllOffersByTrader(MongoId traderId)
    {
        if (!_offersByTrader.TryGetValue(traderId, out var offerIdsToRemove))
        {
            // No trader, nothing to do
            return;
        }

        foreach (var offerId in offerIdsToRemove)
        {
            if (!_offersById.TryRemove(offerId, out _))
            {
                logger.Warning($"Unable to remove offer: {offerId}");
            }
        }

        // Clear out linking table
        _offersByTrader[traderId].Clear();
    }

    /// <summary>
    ///     Add offer to offersByTemplate cache
    /// </summary>
    /// <param name="template">Tpl to store offer against</param>
    /// <param name="offerId">Offer to store against tpl</param>
    /// <returns>True - offer was added</returns>
    protected bool AddOfferByTemplates(MongoId template, MongoId offerId)
    {
        // Look for hashset for tpl first
        if (_offersByTemplate.TryGetValue(template, out var offerIds))
        {
            offerIds.Add(offerId);

            return true;
        }

        // Add new KvP of tpl and offer id in new hashset
        if (_offersByTemplate.TryAdd(template, [offerId]))
        {
            return true;
        }

        logger.Warning($"Unable to add offer: {offerId} to _offersByTemplate");

        return false;
    }

    /// <summary>
    ///     Cache an offer inside `offersByTrader` by trader id
    /// </summary>
    /// <param name="trader">Trader id to store offer against</param>
    /// <param name="offerId">Offer to store against</param>
    /// <returns>True - offer was added</returns>
    protected bool AddOfferByTrader(MongoId trader, MongoId offerId)
    {
        // Look for hashset for trader first
        if (_offersByTrader.TryGetValue(trader, out var traderOfferIds))
        {
            traderOfferIds.Add(offerId);

            return true;
        }

        // Add new KvP of trader and offer id in new hashset
        if (_offersByTrader.TryAdd(trader, [offerId]))
        {
            return true;
        }

        logger.Error($"Unable to add offer: {offerId} to _offersByTrader");

        return false;
    }

    protected bool AddFakePlayerOffer(MongoId itemTpl, MongoId offerId)
    {
        // Look for hashset for trader first
        if (_fakePlayerOffers.TryGetValue(itemTpl, out var fakePlayerOfferIds))
        {
            fakePlayerOfferIds.Add(offerId);

            return true;
        }

        // Add new KvP of trader and offer id in new hashset
        if (_fakePlayerOffers.TryAdd(itemTpl, [offerId]))
        {
            return true;
        }

        logger.Error($"Unable to add offer: {offerId} to _fakePlayerOffers");

        return false;
    }

    /// <summary>
    ///     Add a stale offers id to _expiredOfferIds collection for later processing
    /// </summary>
    /// <param name="staleOfferId">Id of offer to add to stale collection</param>
    public void FlagOfferAsExpired(MongoId staleOfferId)
    {
        lock (_processExpiredOffersLock)
        {
            if (!_expiredOfferIds.TryAdd(staleOfferId, 0))
            {
                logger.Warning($"Unable to add offer: {staleOfferId} to expired offers");
            }
        }
    }

    /// <summary>
    ///     Get total count of current expired offers
    /// </summary>
    /// <returns>Number of expired offers</returns>
    public int GetExpiredOfferCount()
    {
        lock (_processExpiredOffersLock)
        {
            return _expiredOfferIds.Count;
        }
    }

    /// <summary>
    ///     Get an array of arrays of expired offer items + children
    /// </summary>
    /// <returns>Expired offer assorts</returns>
    public IEnumerable<List<Item>> GetExpiredOfferItems()
    {
        List<MongoId> expiredOfferIdsCopy;
        lock (_processExpiredOffersLock)
        {
            expiredOfferIdsCopy = _expiredOfferIds.Keys.ToList();
        }

        // list of lists of item+children
        var expiredItems = new List<List<Item>>();
        foreach (var expiredOfferId in expiredOfferIdsCopy)
        {
            var offer = GetOfferById(expiredOfferId);
            if (offer is null)
            {
                logger.Warning($"Expired offerId: {expiredOfferId} not found, skipping");
                continue;
            }

            if (offer.Items?.Count == 0)
            {
                logger.Error($"Expired offerId: {expiredOfferId} has no items, skipping");
                continue;
            }

            expiredItems.Add(offer.Items);
        }

        return expiredItems;
    }

    /// <summary>
    ///     Clear out internal expiredOffers dictionary of all items
    /// </summary>
    public void ResetExpiredOfferIds()
    {
        lock (_processExpiredOffersLock)
        {
            _expiredOfferIds.Clear();
        }
    }

    /// <summary>
    ///     Flag offers with an end date set before the passed in timestamp
    /// </summary>
    /// <param name="timestamp">Timestamp at point offer is 'expired'</param>
    public void FlagExpiredOffersAfterDate(long timestamp)
    {
        lock (_processExpiredOffersLock)
        {
            var offers = GetOffers();
            Parallel.ForEach(
                offers,
                offer =>
                {
                    if (_expiredOfferIds.ContainsKey(offer.Id) || offer.IsTraderOffer())
                    {
                        // Already flagged or trader offer (handled separately), skip
                        return;
                    }

                    if (!offer.IsStale(timestamp))
                    {
                        return;
                    }

                    if (!_expiredOfferIds.TryAdd(offer.Id, 0))
                    {
                        logger.Warning($"Unable to add offer: {offer.Id} to expired offers as it already exists");
                    }
                }
            );
        }
    }
}
