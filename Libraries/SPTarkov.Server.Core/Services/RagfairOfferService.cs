using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairOfferService(
    ISptLogger<RagfairOfferService> logger,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    SaveServer saveServer,
    RagfairServerHelper ragfairServerHelper,
    ItemHelper itemHelper,
    ProfileHelper profileHelper,
    ServerLocalisationService localisationService,
    ICloner cloner,
    RagfairOfferHolder ragfairOfferHolder,
    NotifierHelper notifierHelper,
    NotificationSendHelper notificationSendHelper,
    ConfigServer configServer
)
{
    private bool _playerOffersLoaded;
    protected readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    ///     Get all offers
    /// </summary>
    /// <returns> List of RagfairOffers </returns>
    public List<RagfairOffer> GetOffers()
    {
        return ragfairOfferHolder.GetOffers();
    }

    public RagfairOffer? GetOfferByOfferId(MongoId offerId)
    {
        return ragfairOfferHolder.GetOfferById(offerId);
    }

    public IEnumerable<RagfairOffer>? GetOffersOfType(MongoId templateId)
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
    public bool DoesOfferExist(MongoId offerId)
    {
        return ragfairOfferHolder.GetOfferById(offerId) != null;
    }

    /// <summary>
    ///     Remove an offer from ragfair by offer id
    /// </summary>
    /// <param name="offerId"> Offer id to remove </param>
    public void RemoveOfferById(MongoId offerId)
    {
        ragfairOfferHolder.RemoveOffer(offerId);
    }

    /// <summary>
    ///     Reduce size of an offer stack by specified amount
    /// </summary>
    /// <param name="offerId"> Offer to adjust stack size of </param>
    /// <param name="amount"> How much to deduct from offers stack size </param>
    public void ReduceOfferQuantity(MongoId offerId, int amount)
    {
        var offer = ragfairOfferHolder.GetOfferById(offerId);
        if (offer == null)
        {
            return;
        }

        offer.Quantity -= amount;
        if (offer.Quantity <= 0)
        {
            // Offer is gone and now 'stale', need to be flagged as stale or removed if PMC offer
            ProcessStaleOffer(offer.Id);
        }
    }

    /// <summary>
    /// Remove all offers from ragfair made by trader
    /// </summary>
    /// <param name="traderId">Trader to remove offers for</param>
    public void RemoveAllOffersByTrader(MongoId traderId)
    {
        ragfairOfferHolder.RemoveAllOffersByTrader(traderId);
    }

    /// <summary>
    ///     Do the trader offers on flea need to be refreshed
    /// </summary>
    /// <param name="traderID"> Trader to check </param>
    /// <returns> True if they do </returns>
    public bool TraderOffersNeedRefreshing(MongoId traderID)
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

    /// <summary>
    /// Iterate over player profiles and add offers to flea market offer cache
    /// </summary>
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
            // Profile has been wiped, ignore
            {
                continue;
            }

            if (!pmcData.RagfairInfo.Offers.Any())
            {
                continue;
            }

            foreach (var offer in pmcData.RagfairInfo.Offers)
            {
                offer.CreatedBy = OfferCreator.Player;
            }

            ragfairOfferHolder.AddOffers(pmcData.RagfairInfo.Offers);
        }

        _playerOffersLoaded = true;
    }

    /// <summary>
    ///     Process cached expired offer ids
    /// </summary>
    public void RemoveExpiredOffers()
    {
        // Gather all stale offers
        var staleOfferIds = ragfairOfferHolder.GetStaleOfferIds();
        foreach (var offerId in staleOfferIds)
        {
            ProcessStaleOffer(offerId, false);
        }

        // Clear out expired offer ids now we've processed them above
        ragfairOfferHolder.ResetExpiredOfferIds();
    }

    /// <summary>
    /// Remove stale offer from flea
    /// Send offer items back when its player offer
    /// Skip trader offers - we want those to remain in 'expired' state until trader refresh
    /// </summary>
    /// <param name="staleOfferId"> Stale offer id to process </param>
    /// <param name="flagOfferAsExpired">OPTIONAL - Flag the passed in offer as expired default = true</param>
    protected void ProcessStaleOffer(MongoId staleOfferId, bool flagOfferAsExpired = true)
    {
        var staleOffer = ragfairOfferHolder.GetOfferById(staleOfferId);
        if (staleOffer is null)
        {
            return;
        }

        // Skip trader offers, managed by RagfairServer.Update() + should remain on flea as 'expired' until trader refresh occurs
        if (staleOffer.IsTraderOffer())
        {
            return;
        }

        // Handle dynamic offer from PMCs
        var isPlayer = staleOffer.CreatedBy == OfferCreator.Player;
        if (flagOfferAsExpired && !isPlayer)
        {
            // Not trader or a player offer
            ragfairOfferHolder.FlagOfferAsExpired(staleOffer.Id);
        }

        // Handle player offer: item(s) need returning/XP/rep adjusting. Checking if offer has actually expired or not.
        if (isPlayer && staleOffer.EndTime <= timeUtil.GetTimeStamp())
        {
            ReturnUnsoldPlayerOffer(staleOffer);

            return;
        }

        // Remove expired offer from global flea pool
        RemoveOfferById(staleOffer.Id);
    }

    /// <summary>
    /// Process a player offer that didn't sell
    /// Reduce rep
    /// Send items back in mail
    /// Increment `notSellSum` value
    /// </summary>
    /// <param name="playerOffer">Offer to process</param>
    protected void ReturnUnsoldPlayerOffer(RagfairOffer playerOffer)
    {
        var offerCreatorId = playerOffer.User.Id;
        var offerCreatorProfile = profileHelper.GetProfileByPmcId(offerCreatorId);
        if (offerCreatorProfile == null)
        {
            logger.Error($"Unable to return flea offer: {playerOffer.Id} as the profile: {offerCreatorId} could not be found");

            return;
        }

        var indexOfOfferInProfile = offerCreatorProfile.RagfairInfo.Offers.FindIndex(o => o.Id == playerOffer.Id);
        if (indexOfOfferInProfile == -1)
        {
            logger.Warning(localisationService.GetText("ragfair-unable_to_find_offer_to_remove", playerOffer.Id));

            return;
        }

        // Reduce player ragfair rep
        offerCreatorProfile.RagfairInfo.Rating -= databaseService.GetGlobals().Configuration.RagFair.RatingDecreaseCount;
        offerCreatorProfile.RagfairInfo.IsRatingGrowing = false;

        // Increment players 'notSellSum' value
        offerCreatorProfile.RagfairInfo.NotSellSum ??= 0;
        offerCreatorProfile.RagfairInfo.NotSellSum += playerOffer.SummaryCost;

        var firstOfferItem = playerOffer.Items.FirstOrDefault();
        if (firstOfferItem.Upd.StackObjectsCount > firstOfferItem.Upd.OriginalStackObjectsCount)
        {
            firstOfferItem.Upd.StackObjectsCount = firstOfferItem.Upd.OriginalStackObjectsCount;
        }

        firstOfferItem.Upd.OriginalStackObjectsCount = null;

        // Remove player offer from flea
        ragfairOfferHolder.RemoveOffer(playerOffer.Id, false);

        // Send failed offer items to player in mail
        var unstackedItems = UnstackOfferItems(playerOffer.Items);

        // Need to regenerate Ids to ensure returned item(s) have correct parent values
        var newParentId = new MongoId();
        foreach (var item in unstackedItems)
        {
            // Refresh root items' parentIds
            if (string.Equals(item.ParentId, "hideout", StringComparison.OrdinalIgnoreCase))
            {
                item.ParentId = newParentId;
            }
        }

        // Send toast notification to player
        var notificationMessage = notifierHelper.CreateRagfairNewRatingNotification(
            offerCreatorProfile.RagfairInfo.Rating.Value,
            offerCreatorProfile.RagfairInfo.IsRatingGrowing.GetValueOrDefault(false)
        );
        notificationSendHelper.SendMessage(offerCreatorId, notificationMessage);

        ragfairServerHelper.ReturnItems(offerCreatorProfile.SessionId.Value, unstackedItems);
        offerCreatorProfile.RagfairInfo.Offers.Splice(indexOfOfferInProfile, 1);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Returned offer: {{playerOffer.Id}} items to player");
        }
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
            // Edge case - Ensure items stack count isn't < 1
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
            var reparentedItemAndChildren = itemHelper.ReparentItemAndChildren(itemAndChildrenClone.FirstOrDefault(), itemAndChildrenClone);
            reparentedItemAndChildren.RemapRootItemId();

            result.AddRange(reparentedItemAndChildren);
        }

        return result;
    }

    /// <summary>
    /// Have enough offers expired their sell time beyond the `ExpiredOfferThreshold` config property
    /// </summary>
    /// <returns>True if enough offers have expired</returns>
    public bool EnoughExpiredOffersExistToProcess()
    {
        return ragfairOfferHolder.GetExpiredOfferCount() >= _ragfairConfig.Dynamic.ExpiredOfferThreshold;
    }
}
