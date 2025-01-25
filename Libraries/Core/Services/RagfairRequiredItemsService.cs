using Core.Helpers;
using SptCommon.Annotations;
using Core.Models.Eft.Ragfair;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService(
    RagfairOfferService _ragfairOfferService,
    PaymentHelper _paymentHelper)
{

    protected Dictionary<string, List<RagfairOffer>> _requiredItemsCache;

    public List<RagfairOffer>? GetRequiredItemsById(string searchId)
    {
        _requiredItemsCache.TryGetValue(searchId, out var list);
        return list;
    }

    public void BuildRequiredItemTable()
    {
        _requiredItemsCache = new Dictionary<string, List<RagfairOffer>>();
        foreach (var offer in _ragfairOfferService.GetOffers()) {
            foreach (var requirement in offer.Requirements) {
                if (_paymentHelper.IsMoneyTpl(requirement.Template))
                {
                    // This would just be too noisy
                    continue;
                }

                // Ensure key is init
                _requiredItemsCache.TryAdd(requirement.Template, []);

                // Add matching offer
                _requiredItemsCache.GetValueOrDefault(requirement.Template)?.Add(offer);
            }
        }
    }
}
