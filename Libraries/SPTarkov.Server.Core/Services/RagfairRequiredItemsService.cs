using System.Collections.Concurrent;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService(
    RagfairOfferService _ragfairOfferService,
    PaymentHelper _paymentHelper)
{
    protected ConcurrentDictionary<string, List<RagfairOffer>> _requiredItemsCache = new();

    public List<RagfairOffer>? GetRequiredItemsById(string searchId)
    {
        _requiredItemsCache.TryGetValue(searchId, out var list);

        return list;
    }

    public void BuildRequiredItemTable()
    {
        _requiredItemsCache.Clear();
        foreach (var offer in _ragfairOfferService.GetOffers())
        foreach (var requirement in offer.Requirements)
        {
            if (_paymentHelper.IsMoneyTpl(requirement.Template))
                // This would just be too noisy
            {
                continue;
            }

            // Ensure key is init
            _requiredItemsCache.TryAdd(requirement.Template, []);

            // Add matching offer
            _requiredItemsCache.GetValueOrDefault(requirement.Template)?.Add(offer);
        }
    }
}
