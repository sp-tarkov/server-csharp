using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services.Ragfair;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService(RagfairOfferService ragfairOfferService, PaymentHelper paymentHelper)
{
    private readonly Lock _createCacheLock = new();
    private volatile bool _cacheIsStale = true;

    /// <summary>
    /// Key = tpl, Value = offerIds
    /// </summary>
    private ConcurrentDictionary<MongoId, HashSet<MongoId>> _requiredItemsCache = new();

    /// <summary>
    /// Empty hashset to be returned when no keys found by GetRequiredOffersById (reduces memory allocations)
    /// </summary>
    private readonly IReadOnlySet<MongoId> _emptyOfferIdSet = new HashSet<MongoId>();

    /// <summary>
    /// Get the offerId of offers that require the supplied tpl
    /// </summary>
    /// <param name="tpl">Tpl to find offers ids for</param>
    /// <returns>Set of OfferIds</returns>
    public IReadOnlySet<MongoId> GetRequiredOffersById(MongoId tpl)
    {
        if (_cacheIsStale)
        {
            // Lock to prevent 2 threads building table
            lock (_createCacheLock)
            {
                // Second check in the event another thread just built table
                if (_cacheIsStale)
                {
                    BuildRequiredItemTable();
                }
            }
        }

        return _requiredItemsCache.TryGetValue(tpl, out var offerIds) ? offerIds : _emptyOfferIdSet;
    }

    /// <summary>
    /// Create a cache of offer Ids keyed against the item tpl they require
    /// </summary>
    public void BuildRequiredItemTable()
    {
        ConcurrentDictionary<MongoId, HashSet<MongoId>> newCache = new();

        foreach (var offer in ragfairOfferService.GetOffers())
        {
            if (offer.Requirements is null)
            {
                continue;
            }

            foreach (var requirement in offer.Requirements)
            {
                // Skip offers for currency, we only need barter offers as this cache is used by `GetOffersThatRequireItem`
                if (paymentHelper.IsMoneyTpl(requirement.TemplateId))
                {
                    continue;
                }

                // Ensure cache has Hashset init for this tpl
                var offerIds = newCache.GetOrAdd(requirement.TemplateId, _ => []);

                // Add offer id against the tpl key
                offerIds.Add(offer.Id);
            }
        }

        // Replace cache in one go
        _requiredItemsCache = newCache;

        // Cache is now fresh
        _cacheIsStale = false;
    }

    /// <summary>
    /// Flag the cache as stale
    /// </summary>
    public void InvalidateCache()
    {
        _cacheIsStale = true;
    }
}
