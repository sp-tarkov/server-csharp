using Core.Helpers;
using Core.Models.Eft.Ragfair;
using SptCommon.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    int maxOffersPerTemplate,
    RagfairServerHelper ragfairServerHelper,
    ProfileHelper profileHelper)
{
    protected Dictionary<string, RagfairOffer> _offersById;
    protected Dictionary<string, Dictionary<string, RagfairOffer>> _offersByTemplate;
    protected Dictionary<string, Dictionary<string, RagfairOffer>> _offersByTrader;

    public RagfairOffer? GetOfferById(string id)
    {
        if (_offersById.ContainsKey(id))
        {
            return _offersById[id];
        }

        return null;
    }

    public List<RagfairOffer> GetOffersByTemplate(string templateId)
    {
        if (_offersByTemplate.ContainsKey(templateId))
        {
            return _offersByTemplate[templateId].Values.ToList();
        }

        return null;
    }

    public List<RagfairOffer> GetOffersByTrader(string traderId)
    {
        if (_offersByTrader.ContainsKey(traderId))
        {
            return _offersByTrader[traderId].Values.ToList();
        }

        return null;
    }

    public List<RagfairOffer> GetOffers()
    {
        if (_offersById.Count > 0)
        {
            return _offersById.Values.ToList();
        }

        return [];
    }

    public void AddOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers) AddOffer(offer);
    }

    public void AddOffer(RagfairOffer offer)
    {
        var trader = offer.User.Id;
        var offerId = offer.Id;
        var itemTpl = offer.Items.FirstOrDefault().Template;
        // If its an NPC PMC offer AND we have already reached the maximum amount of possible offers
        // for this template, just dont add in more
        if (
            !(ragfairServerHelper.IsTrader(trader) || profileHelper.IsPlayer(trader)) &&
            (GetOffersByTemplate(itemTpl)?.Count ?? 0) >= maxOffersPerTemplate
        )
        {
            return;
        }

        _offersById.Add(offerId, offer);
        AddOfferByTrader(trader, offer);
        AddOfferByTemplates(itemTpl, offer);
    }

    /**
     * Purge offer from offer cache
     * @param offer Offer to remove
     */
    public void RemoveOffer(RagfairOffer offer)
    {
        if (_offersById.ContainsKey(offer.Id))
        {
            _offersById.Remove(offer.Id);
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

            if (_offersByTemplate.ContainsKey(offer.Items.FirstOrDefault().Template))
            {
                _offersByTemplate[offer.Items[0].Template].Remove(offer.Id);
            }
        }
    }

    public void RemoveOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers) RemoveOffer(offer);
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        if (_offersByTrader.ContainsKey(traderId))
        {
            RemoveOffers(_offersByTrader[traderId].Values.ToList());
        }
    }

    /**
     * Get an array of stale offers that are still shown to player
     * @returns RagfairOffer array
     */
    public List<RagfairOffer> GetStaleOffers(long time)
    {
        return GetOffers().Where(o => IsStale(o, time)).ToList();
    }

    protected void AddOfferByTemplates(string template, RagfairOffer offer)
    {
        if (_offersByTemplate.ContainsKey(template))
        {
            _offersByTemplate[template].Add(offer.Id, offer);
        }
        else
        {
            var valueMapped = new Dictionary<string, RagfairOffer>();
            valueMapped.Add(offer.Id, offer);
            _offersByTemplate.Add(template, valueMapped);
        }
    }

    protected void AddOfferByTrader(string trader, RagfairOffer offer)
    {
        if (_offersByTrader.ContainsKey(trader))
        {
            _offersByTrader[trader].Add(offer.Id, offer);
        }
        else
        {
            var valueMapped = new Dictionary<string, RagfairOffer>();
            valueMapped.Add(offer.Id, offer);
            _offersByTrader.Add(trader, valueMapped);
        }
    }

    protected bool IsStale(RagfairOffer offer, long time)
    {
        return offer.EndTime < time || (offer.Items[0].Upd?.StackObjectsCount ?? 0) < 1;
    }
}
