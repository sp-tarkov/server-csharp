using Core.Helpers;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Servers;
using SptCommon.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    RagfairServerHelper ragfairServerHelper,
    ProfileHelper profileHelper,
    HashUtil hashUtil,
    ConfigServer configServer)
{
    protected Dictionary<string, RagfairOffer> _offersById = new();
    protected object _offersByIdLock = new();
    protected Dictionary<string, Dictionary<string, RagfairOffer>> _offersByTemplate = new();
    protected object _offersByTemplateLock = new();
    protected Dictionary<string, Dictionary<string, RagfairOffer>> _offersByTrader = new();
    protected object _offersByTraderLock = new();
    protected int _maxOffersPerTemplate = (int)configServer.GetConfig<RagfairConfig>().Dynamic.OfferItemCount.Max;

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
            return _offersByTemplate.TryGetValue(templateId, out var value) ? value.Values.ToList() : null;
        }
    }

    public List<RagfairOffer> GetOffersByTrader(string traderId)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(traderId)) return _offersByTrader[traderId].Values.ToList();
        }

        return null;
    }

    public List<RagfairOffer> GetOffers()
    {
        lock (_offersByIdLock)
        {
            if (_offersById.Count > 0) return _offersById.Values.ToList();
        }

        return [];
    }

    public void AddOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers) AddOffer(offer);
    }

    public void AddOffer(RagfairOffer offer)
    {
        lock (_offersByIdLock)
        {
            var trader = offer.User.Id;
            // keep generating IDs until we get a new one
            while (_offersById.ContainsKey(offer.Id))
                offer.Id = hashUtil.Generate();

            var offerId = offer.Id;
            var itemTpl = offer.Items.FirstOrDefault().Template;
            // If its an NPC PMC offer AND we have already reached the maximum amount of possible offers
            // for this template, just dont add in more
            if (!(ragfairServerHelper.IsTrader(trader) || profileHelper.IsPlayer(trader)) &&
                (GetOffersByTemplate(itemTpl)?.Count ?? 0) >= _maxOffersPerTemplate
               )
                return;

            _offersById.Add(offerId, offer);
            AddOfferByTrader(trader, offer);
            AddOfferByTemplates(itemTpl, offer);
        }
    }

    /**
     * Purge offer from offer cache
     * @param offer Offer to remove
     */
    public void RemoveOffer(RagfairOffer offer)
    {
        lock (_offersByIdLock)
        {
            if (_offersById.ContainsKey(offer.Id))
            {
                _offersById.Remove(offer.Id);
                lock (_offersByTraderLock)
                {
                    if (_offersByTrader.ContainsKey(offer.User.Id))
                    {
                        _offersByTrader[offer.User.Id].Remove(offer.Id);
                        // This was causing a memory leak, we need to make sure that we remove
                        // the user ID from the cached offers after they dont have anything else
                        // on the flea placed. We regenerate the ID for the NPC users, making it
                        // continuously grow otherwise
                        if (_offersByTrader[offer.User.Id].Count == 0) _offersByTrader.Remove(offer.User.Id);
                    }
                }

                lock (_offersByTemplateLock)
                {
                    if (_offersByTemplate.ContainsKey(offer.Items.FirstOrDefault().Template)) _offersByTemplate[offer.Items[0].Template].Remove(offer.Id);
                }
            }
        }
    }

    public void RemoveOffers(List<RagfairOffer> offers)
    {
        foreach (var offer in offers) RemoveOffer(offer);
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(traderId)) RemoveOffers(_offersByTrader[traderId].Values.ToList());
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
        lock (_offersByTemplateLock)
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
    }

    protected void AddOfferByTrader(string trader, RagfairOffer offer)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(trader))
            {
                _offersByTrader[trader].Add(offer.Id, offer);
            }
            else
            {
                var valueMapped = new Dictionary<string, RagfairOffer> { { offer.Id, offer } };
                _offersByTrader.Add(trader, valueMapped);
            }
        }
    }

    protected bool IsStale(RagfairOffer? offer, long time)
    {
        if (offer is null) return false;

        return offer.EndTime < time || (offer.Items.FirstOrDefault().Upd?.StackObjectsCount ?? 0) < 1;
    }
}
