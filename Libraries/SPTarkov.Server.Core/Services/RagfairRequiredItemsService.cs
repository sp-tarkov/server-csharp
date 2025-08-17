using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService(RagfairOfferService ragfairOfferService, PaymentHelper paymentHelper)
{
    /// <summary>
    /// Key = tpl
    /// </summary>
    protected readonly ConcurrentDictionary<MongoId, HashSet<MongoId>> RequiredItemsCache = new();

    /// <summary>
    /// Empty hashset to be returned when no keys found by GetRequiredOffersById (reduces memory allocations)
    /// </summary>
    protected readonly IReadOnlySet<MongoId> EmptyOfferIdSet = new HashSet<MongoId>();

    /// <summary>
    /// Get the offerId of offers that require the supplied tpl
    /// </summary>
    /// <param name="tpl">Tpl to find offers ids for</param>
    /// <returns>Set of OfferIds</returns>
    public IReadOnlySet<MongoId> GetRequiredOffersById(MongoId tpl)
    {
        return RequiredItemsCache.TryGetValue(tpl, out var offerIds) ? offerIds : EmptyOfferIdSet;
    }

    /// <summary>
    /// Create a cache of offer Ids keyed against the item tpl they require
    /// </summary>
    public void BuildRequiredItemTable()
    {
        RequiredItemsCache.Clear();
        foreach (var offer in ragfairOfferService.GetOffers())
        {
            foreach (var requirement in offer.Requirements ?? [])
            {
                // Skip offers for currency, it's too expensive to process
                // Only process barter offers
                if (paymentHelper.IsMoneyTpl(requirement.TemplateId))
                {
                    continue;
                }

                // Ensure cache has Hashset init for this tpl
                var offerIds = RequiredItemsCache.GetOrAdd(requirement.TemplateId, _ => []);

                // Add offer id against the tpl key
                offerIds.Add(offer.Id);
            }
        }
    }
}
