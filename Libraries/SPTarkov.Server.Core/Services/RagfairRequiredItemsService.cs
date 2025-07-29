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
    protected readonly ConcurrentDictionary<MongoId, HashSet<MongoId>> _requiredItemsCache = new();

    /// <summary>
    /// Get the offerId of offers that require the supplied tpl
    /// </summary>
    /// <param name="tpl">Tpl to find offers ids for</param>
    /// <returns></returns>
    public HashSet<MongoId> GetRequiredOffersById(MongoId tpl)
    {
        if (_requiredItemsCache.TryGetValue(tpl, out var offerIds))
        {
            return offerIds;
        }

        return [];
    }

    /// <summary>
    /// Create a cache of requirements to purchase item
    /// </summary>
    public void BuildRequiredItemTable()
    {
        _requiredItemsCache.Clear();
        foreach (var offer in ragfairOfferService.GetOffers())
        foreach (var requirement in offer.Requirements)
        {
            if (paymentHelper.IsMoneyTpl(requirement.TemplateId))
            // This would just be too noisy
            {
                continue;
            }

            // Ensure key is init
            _requiredItemsCache.TryAdd(requirement.TemplateId, []);

            // Add matching offer
            _requiredItemsCache.GetValueOrDefault(requirement.TemplateId)?.Add(offer.Id);
        }
    }
}
