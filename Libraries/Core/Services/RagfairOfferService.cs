using System.Collections.Concurrent;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferService(
    ISptLogger<RagfairOfferService> logger,
    TimeUtil timeUtil,
    HashUtil hashUtil,
    DatabaseService databaseService,
    SaveServer saveServer,
    RagfairServerHelper ragfairServerHelper,
    ItemHelper itemHelper,
    ProfileHelper profileHelper,
    LocalisationService localisationService,
    ICloner cloner,
    RagfairOfferHolder ragfairOfferHolder
)
{
    protected ConcurrentBag<string> _expiredOfferIds = new();
    protected bool _playerOffersLoaded;

    /**
     * Get all offers
     * @returns RagfairOffer array
     */
    public List<RagfairOffer> GetOffers()
    {
        return ragfairOfferHolder.GetOffers();
    }

    public RagfairOffer? GetOfferByOfferId(string offerId)
    {
        return ragfairOfferHolder.GetOfferById(offerId);
    }

    public List<RagfairOffer>? GetOffersOfType(string templateId)
    {
        return ragfairOfferHolder.GetOffersByTemplate(templateId);
    }

    public void AddOffer(RagfairOffer offer)
    {
        ragfairOfferHolder.AddOffer(offer);
    }

    /// <summary>
    ///     Add a stale offers id to collection for later use
    /// </summary>
    /// <param name="staleOfferId">Id of offer to add to stale collection</param>
    public void AddOfferIdToExpired(string staleOfferId)
    {
        _expiredOfferIds.Add(staleOfferId);
    }

    /**
     * Get total count of current expired offers
     * @returns Number of expired offers
     */
    public int GetExpiredOfferCount()
    {
        return _expiredOfferIds.Count;
    }

    /**
     * Get an array of arrays of expired offer items + children
     * @returns Expired offer assorts
     */
    public List<List<Item>> GetExpiredOfferAssorts()
    {
        // list of lists of item+children
        var expiredItems = new List<List<Item>>();

        foreach (var expiredOfferId in _expiredOfferIds)
        {
            var offer = ragfairOfferHolder.GetOfferById(expiredOfferId);
            if (offer?.Items?.Count == 0)
            {
                logger.Error($"Unable to process expired offer: {expiredOfferId}, it has no items");

                continue;
            }

            expiredItems.Add(offer.Items);
        }

        return expiredItems;
    }

    /**
     * Clear out internal expiredOffers dictionary of all items
     */
    public void ResetExpiredOfferIds()
    {
        _expiredOfferIds.Clear();
    }

    /**
     * Does the offer exist on the ragfair
     * @param offerId offer id to check for
     * @returns offer exists - true
     */
    public bool DoesOfferExist(string offerId)
    {
        return ragfairOfferHolder.GetOfferById(offerId) != null;
    }

    /**
     * Remove an offer from ragfair by offer id
     * @param offerId Offer id to remove
     */
    public void RemoveOfferById(string offerId)
    {
        ragfairOfferHolder.RemoveOffer(offerId);
    }

    /**
     * Reduce size of an offer stack by specified amount
     * @param offerId Offer to adjust stack size of
     * @param amount How much to deduct from offers stack size
     */
    public void RemoveOfferStack(string offerId, int amount)
    {
        var offer = ragfairOfferHolder.GetOfferById(offerId);
        if (offer != null)
        {
            offer.Quantity -= amount;

            var rootItem = offer.Items.FirstOrDefault();
            rootItem.Upd.StackObjectsCount -= amount;
            if (rootItem.Upd.StackObjectsCount <= 0 || offer.Quantity <= 0)
            {
                // Reducing stack size has made it 0, offer is now 'stale'
                ProcessStaleOffer(offer);
            }
        }
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        ragfairOfferHolder.RemoveAllOffersByTrader(traderId);
    }

    /**
     * Do the trader offers on flea need to be refreshed
     * @param traderID Trader to check
     * @returns true if they do
     */
    public bool TraderOffersNeedRefreshing(string traderID)
    {
        var trader = databaseService.GetTrader(traderID);
        if (trader?.Base == null)
        {
            logger.Error(localisationService.GetText("ragfair-trader_missing_base_file", traderID));
            return false;
        }

        // No value, occurs when first run, trader offers need to be added to flea
        trader.Base.RefreshTraderRagfairOffers ??= true;

        return trader.Base.RefreshTraderRagfairOffers.Value;
    }

    public void AddPlayerOffers()
    {
        if (!_playerOffersLoaded)
        {
            foreach (var sessionID in saveServer.GetProfiles().Keys)
            {
                var pmcData = saveServer.GetProfile(sessionID)?.CharacterData?.PmcData;

                if (pmcData?.RagfairInfo == null || pmcData.RagfairInfo.Offers == null)
                    // Profile is wiped
                {
                    continue;
                }

                ragfairOfferHolder.AddOffers(pmcData.RagfairInfo.Offers);
            }

            _playerOffersLoaded = true;
        }
    }

    public void ExpireStaleOffers()
    {
        var time = timeUtil.GetTimeStamp();
        foreach (var staleOffer in ragfairOfferHolder.GetStaleOffers(time))
        {
            ProcessStaleOffer(staleOffer);
        }
    }

    /**
     * Remove stale offer from flea
     * @param staleOffer Stale offer to process
     */
    protected void ProcessStaleOffer(RagfairOffer staleOffer)
    {
        var staleOfferId = staleOffer.Id;
        var staleOfferUserId = staleOffer.User.Id;

        var isTrader = ragfairServerHelper.IsTrader(staleOfferUserId);
        var isPlayer = profileHelper.IsPlayer(staleOfferUserId.RegexReplace("^pmc", ""));

        // Skip trader offers, managed by RagfairServer.update()
        if (isTrader)
        {
            return;
        }

        // Handle dynamic offer
        if (!(isTrader || isPlayer))
            // Dynamic offer
        {
            AddOfferIdToExpired(staleOfferId);
        }

        // Handle player offer - items need returning/XP adjusting. Checking if offer has actually expired or not.
        if (isPlayer && staleOffer.EndTime <= timeUtil.GetTimeStamp())
        {
            ReturnPlayerOffer(staleOffer);
            return;
        }

        // Remove expired existing offer from global offers
        RemoveOfferById(staleOfferId);
    }

    protected void ReturnPlayerOffer(RagfairOffer playerOffer)
    {
        var pmcId = playerOffer.User.Id;
        var profile = profileHelper.GetProfileByPmcId(pmcId);
        if (profile == null)
        {
            logger.Error($"Unable to return flea offer {playerOffer.Id} as the profile: {pmcId} could not be found");
            return;
        }

        var offerinProfileIndex = profile.RagfairInfo.Offers.FindIndex(o => o.Id == playerOffer.Id);
        if (offerinProfileIndex == -1)
        {
            logger.Warning(localisationService.GetText("ragfair-unable_to_find_offer_to_remove", playerOffer.Id));
            return;
        }

        // Reduce player ragfair rep
        profile.RagfairInfo.Rating -= databaseService.GetGlobals().Configuration.RagFair.RatingDecreaseCount;
        profile.RagfairInfo.IsRatingGrowing = false;

        // Increment players 'notSellSum' value
        profile.RagfairInfo.NotSellSum ??= 0;
        profile.RagfairInfo.NotSellSum += playerOffer.SummaryCost;

        var firstOfferItem = playerOffer.Items[0];
        if (firstOfferItem.Upd.StackObjectsCount > firstOfferItem.Upd.OriginalStackObjectsCount)
        {
            playerOffer.Items[0].Upd.StackObjectsCount = firstOfferItem.Upd.OriginalStackObjectsCount;
        }

        playerOffer.Items[0].Upd.OriginalStackObjectsCount = null;
        // Remove player offer from flea
        ragfairOfferHolder.RemoveOffer(playerOffer.Id);

        // Send failed offer items to player in mail
        var unstackedItems = UnstackOfferItems(playerOffer.Items);

        // Need to regenerate Ids to ensure returned item(s) have correct parent values
        var newParentId = hashUtil.Generate();
        foreach (var item in unstackedItems)
            // Refresh root items' parentIds
        {
            if (item.ParentId == "hideout")
            {
                item.ParentId = newParentId;
            }
        }

        ragfairServerHelper.ReturnItems(profile.SessionId, unstackedItems);
        profile.RagfairInfo.Offers.Splice(offerinProfileIndex, 1);
    }

    /**
     * Flea offer items are stacked up often beyond the StackMaxSize limit
     * Un stack the items into an array of root items and their children
     * Will create new items equal to the
     * @param items Offer items to unstack
     * @returns Unstacked array of items
     */
    protected List<Item> UnstackOfferItems(List<Item> items)
    {
        var result = new List<Item>();
        var rootItem = items[0];
        var itemDetails = itemHelper.GetItem(rootItem.Template);
        var itemMaxStackSize = itemDetails.Value?.Properties?.StackMaxSize ?? 1;

        var totalItemCount = rootItem.Upd?.StackObjectsCount ?? 1;

        // Items within stack tolerance, return existing data - no changes needed
        if (totalItemCount <= itemMaxStackSize)
        {
            // Edge case - Ensure items stack count isnt < 1
            if (items[0]?.Upd?.StackObjectsCount < 1)
            {
                items[0].Upd.StackObjectsCount = 1;
            }

            return items;
        }

        // Single item with no children e.g. ammo, use existing de-stacking code
        if (items.Count == 1)
        {
            return itemHelper.SplitStack(rootItem);
        }

        // Item with children, needs special handling
        // Force new item to have stack size of 1
        for (var index = 0; index < totalItemCount; index++)
        {
            var itemAndChildrenClone = cloner.Clone(items);

            // Ensure upd object exits
            itemAndChildrenClone[0].Upd ??= new Upd();

            // Force item to be singular
            itemAndChildrenClone[0].Upd.StackObjectsCount = 1;

            // Ensure items IDs are unique to prevent collisions when added to player inventory
            var reparentedItemAndChildren = itemHelper.ReparentItemAndChildren(
                itemAndChildrenClone[0],
                itemAndChildrenClone
            );
            itemHelper.RemapRootItemId(reparentedItemAndChildren);

            result.AddRange(reparentedItemAndChildren);
        }

        return result;
    }
}
