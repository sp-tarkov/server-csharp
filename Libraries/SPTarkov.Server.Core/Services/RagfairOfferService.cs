using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services;

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
    RagfairOfferHolder ragfairOfferHolder,
    ConfigServer configServer
)
{
    protected bool _playerOffersLoaded;
    protected RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    ///     Get all offers
    /// </summary>
    /// <returns> List of RagfairOffers </returns>
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
    ///     Does the offer exist on the ragfair
    /// </summary>
    /// <param name="offerId"> Offer id to check for </param>
    /// <returns> True when offer exists </returns>
    public bool DoesOfferExist(string offerId)
    {
        return ragfairOfferHolder.GetOfferById(offerId) != null;
    }

    /// <summary>
    ///     Remove an offer from ragfair by offer id
    /// </summary>
    /// <param name="offerId"> Offer id to remove </param>
    public void RemoveOfferById(string offerId)
    {
        ragfairOfferHolder.RemoveOffer(offerId);
    }

    /// <summary>
    ///     Reduce size of an offer stack by specified amount
    /// </summary>
    /// <param name="offerId"> Offer to adjust stack size of </param>
    /// <param name="amount"> How much to deduct from offers stack size </param>
    public void ReduceOfferQuantity(string offerId, int amount)
    {
        var offer = ragfairOfferHolder.GetOfferById(offerId);
        if (offer == null)
        {
            return;
        }

        offer.Quantity -= amount;
        if (offer.Quantity <= 0)
        {
            // Reducing Quantity has made it 0 or below, offer is now 'stale' and needs to be flagged as expired so it can be removed/regenerated on the next ragfair update()
            ragfairOfferHolder.FlagOfferAsExpired(offer.Id);
        }
    }

    public void RemoveAllOffersByTrader(string traderId)
    {
        ragfairOfferHolder.RemoveAllOffersByTrader(traderId);
    }

    /// <summary>
    ///     Do the trader offers on flea need to be refreshed
    /// </summary>
    /// <param name="traderID"> Trader to check </param>
    /// <returns> True if they do </returns>
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
        if (_playerOffersLoaded)
        {
            return;
        }

        foreach (var sessionId in saveServer.GetProfiles().Keys)
        {
            var pmcData = saveServer.GetProfile(sessionId)?.CharacterData?.PmcData;

            if (pmcData?.RagfairInfo?.Offers == null)
                // Profile is wiped
            {
                continue;
            }

            ragfairOfferHolder.AddOffers(pmcData.RagfairInfo.Offers);
        }

        _playerOffersLoaded = true;
    }

    /// <summary>
    ///     Process the expired ids and remove offers
    /// </summary>
    public void RemoveExpiredOffers()
    {
        ragfairOfferHolder.RemoveExpiredOffers();

        // Clear out expired offer ids now we've regenerated them
        ragfairOfferHolder.ResetExpiredOfferIds();
    }

    /// <summary>
    ///     Remove stale offer from flea
    /// </summary>
    /// <param name="staleOffer"> Stale offer to process </param>
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
        {
            // Not trader/player offer
            ragfairOfferHolder.FlagOfferAsExpired(staleOfferId);
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
        ragfairOfferHolder.RemoveOffer(playerOffer.Id, false);

        // Send failed offer items to player in mail
        var unstackedItems = UnstackOfferItems(playerOffer.Items);

        // Need to regenerate Ids to ensure returned item(s) have correct parent values
        var newParentId = hashUtil.Generate();
        foreach (var item in unstackedItems)
        {
            // Refresh root items' parentIds
            if (string.Equals(item.ParentId, "hideout", StringComparison.OrdinalIgnoreCase))
            {
                item.ParentId = newParentId;
            }
        }

        ragfairServerHelper.ReturnItems(profile.SessionId, unstackedItems);
        profile.RagfairInfo.Offers.Splice(offerinProfileIndex, 1);
    }

    /// <summary>
    ///     Flea offer items are stacked up often beyond the StackMaxSize limit.
    ///     Unstack the items into an array of root items and their children.
    ///     Will create new items equal to the stack.
    /// </summary>
    /// <param name="items"> Offer items to unstack </param>
    /// <returns> Unstacked array of items </returns>
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

    public bool EnoughExpiredOffersExistToProcess()
    {
        return ragfairOfferHolder.GetExpiredOfferCount() >= _ragfairConfig.Dynamic.ExpiredOfferThreshold;
    }
}
