using Core.Helpers;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferHolder(
    ISptLogger<RagfairOfferHolder> _logger,
    RagfairServerHelper ragfairServerHelper,
    ProfileHelper profileHelper,
    HashUtil hashUtil,
    LocalisationService _localisationService,
    ConfigServer configServer)
{
    protected Dictionary<string, RagfairOffer> _offersById = new();
    protected object _offersByIdLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTemplate = new(); // key = tplId, value = list of offerIds
    protected object _offersByTemplateLock = new();
    protected Dictionary<string, HashSet<string>> _offersByTrader = new(); // key = traderId, value = list of offerIds
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
            // Get the offerIds we want to return
            if (!_offersByTemplate.TryGetValue(templateId, out var offerIds))
            {
                return null;
            }

            var result = _offersById
                .Where(x => offerIds.Contains(x.Key))
                .Select(x => x.Value).ToList();

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
            if (itemTpl != null
                && !(ragfairServerHelper.IsTrader(sellerId) || profileHelper.IsPlayer(sellerId))
                && _offersByTemplate.TryGetValue(itemTpl, out var offers)
                && offers?.Count >= _maxOffersPerTemplate
               )
            {
                return;
            }

            _offersById.Add(offerId, offer);

            AddOfferByTrader(sellerId, offer);
            AddOfferByTemplates(itemTpl, offer);
        }
    }

    /**
     * Purge offer from offer cache
     * @param offer Offer to remove
     */
    public void RemoveOffer(string offerId)
    {
        lock (_offersByIdLock)
        {
            if (!_offersById.TryGetValue(offerId, out var offer))
            {
                _logger.Warning(_localisationService.GetText("ragfair-unable_to_remove_offer_doesnt_exist", offerId));
                return;
            }

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
                var firstItem = offer.Items.FirstOrDefault();
                if (_offersByTemplate.ContainsKey(firstItem.Template))
                {
                    _offersByTemplate[firstItem.Template].Remove(offer.Id);
                }
            }
        }
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        lock (_offersByTraderLock)
        {
            if (_offersByTrader.ContainsKey(traderId))
            {
                var offerIdsToRemove = _offersByTrader[traderId];
                foreach (var offerId in offerIdsToRemove)
                {
                    _offersById.Remove(offerId);
                }

                // Clear out linking table
                _offersByTrader[traderId].Clear();
            }
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
        if (offer is null) return false;

        return offer.EndTime < time || (offer.Items.FirstOrDefault().Upd?.StackObjectsCount ?? 0) < 1;
    }
}
