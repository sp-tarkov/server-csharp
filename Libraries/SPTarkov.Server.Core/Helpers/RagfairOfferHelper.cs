using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairOfferHelper(
    ISptLogger<RagfairOfferHelper> logger,
    TimeUtil timeUtil,
    BotHelper botHelper,
    RagfairSortHelper ragfairSortHelper,
    PresetHelper presetHelper,
    RagfairHelper ragfairHelper,
    PaymentHelper paymentHelper,
    TraderHelper traderHelper,
    QuestHelper questHelper,
    RagfairServerHelper ragfairServerHelper,
    ItemHelper itemHelper,
    DatabaseService databaseService,
    RagfairOfferService ragfairOfferService,
    LocaleService localeService,
    ServerLocalisationService serverLocalisationService,
    MailSendService mailSendService,
    RagfairRequiredItemsService ragfairRequiredItemsService,
    ProfileHelper profileHelper,
    EventOutputHolder eventOutputHolder,
    ConfigServer configServer
)
{
    protected const string GoodSoldTemplate = "5bdabfb886f7743e152e867e 0"; // Your {soldItem} {itemCount} items were bought by {buyerNickname}.
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly RagfairConfig RagfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    ///     Pass through to ragfairOfferService.getOffers(), get flea offers a player should see
    /// </summary>
    /// <param name="searchRequest">Data from client</param>
    /// <param name="itemsToAdd">ragfairHelper.filterCategories()</param>
    /// <param name="traderAssorts">Trader assorts</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Offers the player should see</returns>
    public List<RagfairOffer> GetValidOffers(
        SearchRequestData searchRequest,
        HashSet<MongoId> itemsToAdd,
        Dictionary<MongoId, TraderAssort> traderAssorts,
        PmcData pmcData
    )
    {
        var playerIsFleaBanned = pmcData.PlayerIsFleaBanned(timeUtil.GetTimeStamp());
        var tieredFlea = RagfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;
        return ragfairOfferService
            .GetOffers()
            .Where(offer =>
            {
                var offerRootItem = offer.Items.FirstOrDefault();
                if (!PassesSearchFilterCriteria(searchRequest, offer, offerRootItem, pmcData))
                {
                    return false;
                }

                var isDisplayable = IsDisplayableOffer(
                    searchRequest,
                    itemsToAdd,
                    traderAssorts,
                    offer,
                    offerRootItem,
                    pmcData,
                    playerIsFleaBanned
                );

                if (!isDisplayable)
                {
                    return false;
                }

                // Not trader offer + tiered flea enabled
                if (tieredFlea.Enabled && !offer.IsTraderOffer())
                {
                    CheckAndLockOfferFromPlayerTieredFlea(
                        tieredFlea,
                        offer,
                        tieredFleaLimitTypes.Keys.ToHashSet(),
                        pmcData.Info.Level.Value
                    );
                }

                return true;
            })
            .ToList();
    }

    /// <summary>
    ///     Disable offer if item is flagged by tiered flea config based on player level
    /// </summary>
    /// <param name="tieredFlea">Tiered flea settings from ragfair config</param>
    /// <param name="offer">Ragfair offer to evaluate</param>
    /// <param name="tieredFleaLimitTypes">List of item types flagged with a required player level</param>
    /// <param name="playerLevel">Current level of player viewing offer</param>
    protected void CheckAndLockOfferFromPlayerTieredFlea(
        TieredFlea tieredFlea,
        RagfairOffer offer,
        HashSet<MongoId> tieredFleaLimitTypes,
        int playerLevel
    )
    {
        var offerItemTpl = offer.Items.FirstOrDefault().Template;

        // Check if offer item is ammo
        if (tieredFlea.AmmoTplUnlocks is not null && itemHelper.IsOfBaseclass(offerItemTpl, BaseClasses.AMMO))
        {
            // Check if ammo is flagged with a level requirement
            if (tieredFlea.AmmoTplUnlocks.TryGetValue(offerItemTpl, out var unlockLevel) && playerLevel < unlockLevel)
            {
                // Lock the offer if player's level is below the ammo's unlock requirement
                offer.Locked = true;

                return;
            }
        }

        // Check for a direct level requirement for the offer item
        if (tieredFlea.UnlocksTpl.TryGetValue(offerItemTpl, out var itemLevelRequirement))
        {
            if (playerLevel < itemLevelRequirement)
            {
                // Lock the offer if player's level is below the item's specific requirement
                offer.Locked = true;

                return;
            }
        }

        // Optimisation - Skip further checks if the item type isn't in the restricted types list
        if (!itemHelper.IsOfBaseclasses(offerItemTpl, tieredFleaLimitTypes))
        {
            return;
        }

        // Check if the item belongs to any restricted type and if player level is insufficient
        if (
            tieredFleaLimitTypes
                .Where(tieredItemType => itemHelper.IsOfBaseclass(offerItemTpl, tieredItemType))
                .Any(tieredItemType => playerLevel < tieredFlea.UnlocksType[tieredItemType])
        )
        {
            // Players level is below matching types requirement, flag as locked
            offer.Locked = true;
        }
    }

    /// <summary>
    ///     Get matching offers that require the desired item and filter out offers from non traders if player is below ragfair
    ///     unlock level
    /// </summary>
    /// <param name="searchRequest">Search request from client</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Matching RagfairOffer objects</returns>
    public List<RagfairOffer> GetOffersThatRequireItem(SearchRequestData searchRequest, PmcData pmcData)
    {
        // Get all offers that require the desired item and filter out offers from non traders if player below ragfair unlock
        var offerIDsForItem = ragfairRequiredItemsService.GetRequiredOffersById(searchRequest.NeededSearchId.Value);

        var tieredFlea = RagfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;
        var tieredFleaKeys = tieredFleaLimitTypes.Keys.ToHashSet();

        var result = new List<RagfairOffer>();
        foreach (
            var offer in offerIDsForItem
                .Select(ragfairOfferService.GetOfferByOfferId)
                .Where(offer => PassesSearchFilterCriteria(searchRequest, offer, offer.Items.FirstOrDefault(), pmcData))
        )
        {
            if (tieredFlea.Enabled && !offer.IsTraderOffer())
            {
                CheckAndLockOfferFromPlayerTieredFlea(tieredFlea, offer, tieredFleaKeys, pmcData.Info.Level.Value);
            }

            result.Add(offer);
        }

        return result;
    }

    /// <summary>
    ///     Get offers from flea/traders specifically when building weapon preset
    /// </summary>
    /// <param name="searchRequest">Search request data</param>
    /// <param name="itemsToAdd">string array of item tpls to search for</param>
    /// <param name="traderAssorts">All trader assorts player can access/buy</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>RagfairOffer array</returns>
    public List<RagfairOffer> GetOffersForBuild(
        SearchRequestData searchRequest,
        HashSet<MongoId> itemsToAdd,
        Dictionary<MongoId, TraderAssort> traderAssorts,
        PmcData pmcData
    )
    {
        var offersMap = new Dictionary<MongoId, List<RagfairOffer>>();
        var offersToReturn = new List<RagfairOffer>();
        var playerIsFleaBanned = pmcData.PlayerIsFleaBanned(timeUtil.GetTimeStamp());
        var tieredFlea = RagfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;

        foreach (var desiredItemTpl in searchRequest.BuildItems)
        {
            var matchingOffers = ragfairOfferService.GetOffersOfType(desiredItemTpl.Key);
            if (matchingOffers is null)
            // No offers found for this item, skip
            {
                continue;
            }

            foreach (var offer in matchingOffers)
            {
                // Don't show pack offers
                if (offer.SellInOnePiece.GetValueOrDefault(false))
                {
                    continue;
                }

                var rootOfferItem = offer.Items.FirstOrDefault();
                if (!PassesSearchFilterCriteria(searchRequest, offer, rootOfferItem, pmcData))
                {
                    continue;
                }

                if (!IsDisplayableOffer(searchRequest, itemsToAdd, traderAssorts, offer, rootOfferItem, pmcData, playerIsFleaBanned))
                {
                    continue;
                }

                if (offer.IsTraderOffer())
                {
                    if (TraderBuyRestrictionReached(offer))
                    {
                        continue;
                    }

                    if (TraderOutOfStock(offer))
                    {
                        continue;
                    }

                    if (TraderOfferItemQuestLocked(offer, traderAssorts))
                    {
                        continue;
                    }

                    if (TraderOfferLockedBehindLoyaltyLevel(offer, pmcData))
                    {
                        continue;
                    }
                }

                // Tiered flea and not trader offer
                if (tieredFlea.Enabled && !offer.IsTraderOffer())
                {
                    CheckAndLockOfferFromPlayerTieredFlea(
                        tieredFlea,
                        offer,
                        tieredFleaLimitTypes.Keys.ToHashSet(),
                        pmcData.Info.Level.Value
                    );

                    // Do not add offer to build if user does not have access to it
                    if (offer.Locked.GetValueOrDefault(false))
                    {
                        continue;
                    }
                }

                var key = offer.Items[0].Template;
                if (!offersMap.ContainsKey(key))
                {
                    offersMap.Add(key, []);
                }

                offersMap[key].Add(offer);
            }
        }

        // Get best offer for each item to show on screen
        var offersToSort = new List<RagfairOffer>();
        foreach (var possibleOffers in offersMap.Values)
        {
            // prepare temp list for offers
            offersToSort.Clear();
            offersToSort.AddRange(possibleOffers);

            // Remove offers with locked = true (quest locked) when > 1 possible offers
            // single trader item = shows greyed out
            // multiple offers for item = is greyed out
            if (possibleOffers.Count > 1)
            {
                var lockedOffers = GetLoyaltyLockedOffers(possibleOffers, pmcData);

                // Exclude locked offers + above loyalty locked offers if at least 1 was found
                offersToSort = possibleOffers
                    .Where(offer => !(offer.Locked.GetValueOrDefault(false) || lockedOffers.Contains(offer.Id)))
                    .ToList();

                // Exclude trader offers over their buy restriction limit
                offersToSort = GetOffersInsideBuyRestrictionLimits(offersToSort);
            }

            // Sort offers by price and pick the best
            var offer = ragfairSortHelper.SortOffers(offersToSort, RagfairSort.PRICE)[0];
            offersToReturn.Add(offer);
        }

        return offersToReturn;
    }

    /// <summary>
    /// Should a ragfair offer be visible to the player
    /// </summary>
    /// <param name="searchRequest">Client request</param>
    /// <param name="itemsToAdd"></param>
    /// <param name="traderAssorts">Trader assort items - used for filtering out locked trader items</param>
    /// <param name="offer">Flea offer</param>
    /// <param name="offerRootItem">Root offer item</param>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="playerIsFleaBanned">Player cannot view flea yet/ever</param>
    /// <returns>True = should be shown to player</returns>
    protected bool IsDisplayableOffer(
        SearchRequestData searchRequest,
        HashSet<MongoId> itemsToAdd,
        Dictionary<MongoId, TraderAssort> traderAssorts,
        RagfairOffer offer,
        Item offerRootItem,
        PmcData pmcProfile,
        bool playerIsFleaBanned = false
    )
    {
        var isTraderOffer = offer.IsTraderOffer();
        if (!isTraderOffer && playerIsFleaBanned)
        {
            return false;
        }

        // Offer root items tpl not in searched for array
        if (!itemsToAdd.Contains(offerRootItem.Template))
        // skip items we shouldn't include
        {
            return false;
        }

        // Performing a required search and offer doesn't have requirement for item
        if (
            !searchRequest.NeededSearchId.HasValue
            && !searchRequest.NeededSearchId.Value.IsEmpty
            && !offer.Requirements.Any(requirement => requirement.TemplateId == searchRequest.NeededSearchId)
        )
        {
            return false;
        }

        // Weapon/equipment search + offer is preset
        if (
            searchRequest.BuildItems.Count == 0
            && // Prevent equipment loadout searches filtering out presets
            searchRequest.BuildCount.GetValueOrDefault(0) > 0
            && presetHelper.HasPreset(offerRootItem.Template)
        )
        {
            return false;
        }

        // Currency offer is sold for
        var moneyTypeTpl = offer.Requirements.FirstOrDefault().TemplateId;
        // commented out as required search "which is for checking offers that are barters"
        // has info.removeBartering as true, this if statement removed barter items.
        if (searchRequest.RemoveBartering.GetValueOrDefault(false) && !paymentHelper.IsMoneyTpl(moneyTypeTpl))
        // Don't include barter offers
        {
            return false;
        }

        if (offer.RequirementsCost is null)
        // Don't include offers with undefined or NaN in it
        {
            return false;
        }

        // Handle trader items to remove items that are not available to the user right now
        // e.g. required search for "lamp" shows 4 items, 3 of which are not available to a new player
        // filter those out
        if (isTraderOffer)
        {
            if (!traderAssorts.TryGetValue(offer.User.Id, out var assort))
            // Trader not visible on flea market
            {
                return false;
            }

            if (!assort.Items.Any(item => item.Id == offer.Root))
            // skip (quest) locked items
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Get offers that have not exceeded buy limits
    /// </summary>
    /// <param name="possibleOffers">offers to process</param>
    /// <returns>Offers</returns>
    protected List<RagfairOffer> GetOffersInsideBuyRestrictionLimits(List<RagfairOffer> possibleOffers)
    {
        // Check offer has buy limit + is from trader + current buy count is at or over max
        return possibleOffers
            .Where(offer =>
            {
                if (offer.BuyRestrictionMax is null && offer.IsTraderOffer() && offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax)
                {
                    if (offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax)
                    {
                        return false;
                    }
                }

                // Doesn't have buy limits, return offer
                return true;
            })
            .ToList();
    }

    /// <summary>
    ///     Check if offer is from trader standing the player does not have
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <param name="pmcProfile">Player profile</param>
    /// <returns>True if item is locked, false if item is purchaseable</returns>
    protected bool TraderOfferLockedBehindLoyaltyLevel(RagfairOffer offer, PmcData pmcProfile)
    {
        if (!pmcProfile.TradersInfo.TryGetValue(offer.User.Id, out var userTraderSettings))
        {
            logger.Warning($"Trader: {offer.User.Id} not found in profile, assuming offer is not locked being loyalty level");
            return false;
        }

        return userTraderSettings.LoyaltyLevel < offer.LoyaltyLevel;
    }

    /// <summary>
    ///     Check if offer item is quest locked for current player by looking at sptQuestLocked property in traders
    ///     barter_scheme
    /// </summary>
    /// <param name="offer">Offer to check is quest locked</param>
    /// <param name="traderAssorts">all trader assorts for player</param>
    /// <returns>true if quest locked</returns>
    public bool TraderOfferItemQuestLocked(RagfairOffer offer, Dictionary<MongoId, TraderAssort> traderAssorts)
    {
        var itemIds = offer.Items.Select(x => x.Id).ToHashSet();
        //foreach (var item in offer.Items)
        //{
        //    traderAssorts.TryGetValue(offer.User.Id, out var assorts);
        //    foreach (var barterKvP in assorts.BarterScheme.Where(x => itemIds.Contains(x.Key)))
        //    {
        //        foreach (var subBarter in barterKvP.Value)
        //        {
        //            if (subBarter.Any(subBarter => subBarter.SptQuestLocked.GetValueOrDefault(false)))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //}

        foreach (var _ in offer.Items)
        {
            traderAssorts.TryGetValue(offer.User.Id, out var assorts);
            if (
                assorts
                    .BarterScheme.Where(x => itemIds.Contains(x.Key))
                    .Any(barterKvP =>
                        barterKvP.Value.Any(subBarter => subBarter.Any(subBarter => subBarter.SptQuestLocked.GetValueOrDefault(false)))
                    )
            )
            {
                return true;
            }
        }

        // Fallback, nothing found
        return false;
    }

    /// <summary>
    ///     Has trader offer ran out of stock to sell to player
    /// </summary>
    /// <param name="offer">Offer to check stock of</param>
    /// <returns>true if out of stock</returns>
    protected bool TraderOutOfStock(RagfairOffer offer)
    {
        if (offer.Items?.Count == 0)
        {
            return true;
        }

        return offer.Items.FirstOrDefault()?.Upd?.StackObjectsCount == 0;
    }

    /// <summary>
    ///     Check if trader offers' BuyRestrictionMax value has been reached
    /// </summary>
    /// <param name="offer">Offer to check restriction properties of</param>
    /// <returns>true if restriction reached, false if no restrictions/not reached</returns>
    protected bool TraderBuyRestrictionReached(RagfairOffer offer)
    {
        var traderAssorts = traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;

        // Find item being purchased from traders assorts
        var assortData = traderAssorts.FirstOrDefault(item => item.Id == offer.Items[0].Id);
        if (assortData is null)
        {
            // No trader assort data
            logger.Warning(
                $"Unable to find trader: "
                    + $"${offer.User.Nickname}assort for item: {itemHelper.GetItemName(offer.Items[0].Template)} "
                    + $"{offer.Items[0].Template}, cannot check if buy restriction reached"
            );

            return false;
        }

        if (assortData.Upd is null)
        // No Upd = no chance of limits
        {
            return false;
        }

        // No restriction values
        // Can't use !assortData.upd.BuyRestrictionX as value could be 0
        if (assortData.Upd.BuyRestrictionMax is null || assortData.Upd.BuyRestrictionCurrent is null)
        {
            return false;
        }

        // Current equals max, limit reached
        if (assortData.Upd.BuyRestrictionCurrent >= assortData.Upd.BuyRestrictionMax)
        {
            return true;
        }

        return false;
    }

    protected HashSet<MongoId> GetLoyaltyLockedOffers(IEnumerable<RagfairOffer> offers, PmcData pmcProfile)
    {
        var loyaltyLockedOffers = new HashSet<MongoId>();
        foreach (var offer in offers.Where(x => x.IsTraderOffer()))
        {
            if (pmcProfile.TradersInfo.TryGetValue(offer.User.Id, out var traderDetails) && traderDetails.LoyaltyLevel < offer.LoyaltyLevel)
            {
                loyaltyLockedOffers.Add(offer.Id);
            }
        }

        return loyaltyLockedOffers;
    }

    /// <summary>
    /// Process all player-listed flea offers for a desired profile
    /// </summary>
    /// <param name="sessionId">Session id to process offers for</param>
    /// <returns>true = complete</returns>
    public bool ProcessOffersOnProfile(MongoId sessionId)
    {
        var currentTimestamp = timeUtil.GetTimeStamp();
        var profileOffers = GetProfileOffers(sessionId);

        // No offers, don't do anything
        if (!profileOffers.Any())
        {
            return true;
        }

        // Index backwards as CompleteOffer() can delete offer object
        for (var index = profileOffers.Count - 1; index >= 0; index--)
        {
            var offer = profileOffers[index];
            if (currentTimestamp > offer.EndTime)
            {
                // Offer has expired before selling, skip as it will be processed in RemoveExpiredOffers()
                continue;
            }

            if (offer.SellResults is null || !offer.SellResults.Any() || currentTimestamp < offer.SellResults.FirstOrDefault()?.SellTime)
            {
                // Not sold / too early to check
                continue;
            }

            var firstSellResult = offer.SellResults?.FirstOrDefault();
            if (firstSellResult is null)
            {
                continue;
            }

            // Checks first item, first is spliced out of array after being processed
            // Item sold
            var totalItemsCount = 1d;
            var boughtAmount = 1;

            // Does item need to be re-stacked
            if (!offer.SellInOnePiece.GetValueOrDefault(false))
            {
                // offer.items.reduce((sum, item) => sum + item.upd?.StackObjectsCount ?? 0, 0);
                totalItemsCount = GetTotalStackCountSize([offer.Items]);
                boughtAmount = firstSellResult.Amount ?? boughtAmount;
            }

            var ratingToAdd = offer.SummaryCost / totalItemsCount * boughtAmount;
            IncreaseProfileRagfairRating(profileHelper.GetFullProfile(sessionId), ratingToAdd.Value);

            // Remove the sell result object now it has been processed
            offer.SellResults.Remove(firstSellResult);

            // Can delete offer object, must run last
            CompleteOffer(sessionId, offer, boughtAmount);
        }

        return true;
    }

    /// <summary>
    /// Count up all root item StackObjectsCount properties of an array of items
    /// </summary>
    /// <param name="itemsInInventoryToSumStackCount">items to sum up</param>
    /// <returns>Total stack count</returns>
    public double GetTotalStackCountSize(IEnumerable<List<Item>> itemsInInventoryToSumStackCount)
    {
        return itemsInInventoryToSumStackCount.Sum(itemAndChildren =>
            itemAndChildren.FirstOrDefault()?.Upd?.StackObjectsCount.GetValueOrDefault(1) ?? 1
        );
    }

    /// <summary>
    /// Add amount to players ragfair rating
    /// </summary>
    /// <param name="profile">Profile to update</param>
    /// <param name="amountToIncrementBy">Raw amount to add to players ragfair rating (excluding the reputation gain multiplier)</param>
    public void IncreaseProfileRagfairRating(SptProfile profile, double? amountToIncrementBy)
    {
        var ragfairGlobalsConfig = databaseService.GetGlobals().Configuration.RagFair;

        profile.CharacterData.PmcData.RagfairInfo.IsRatingGrowing = true;
        if (amountToIncrementBy is null)
        {
            logger.Warning($"Unable to increment ragfair rating, value was not a number: {amountToIncrementBy}");

            return;
        }

        profile.CharacterData.PmcData.RagfairInfo.Rating +=
            ragfairGlobalsConfig.RatingIncreaseCount / ragfairGlobalsConfig.RatingSumForIncrease * amountToIncrementBy;
    }

    /// <summary>
    /// Return all offers a player has listed on a desired profile
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>List of ragfair offers</returns>
    protected List<RagfairOffer> GetProfileOffers(MongoId sessionId)
    {
        var profile = profileHelper.GetPmcProfile(sessionId);

        if (profile.RagfairInfo?.Offers is null)
        {
            return [];
        }

        return profile.RagfairInfo.Offers;
    }

    /// <summary>
    /// Delete an offer from a desired profile and from ragfair offers
    /// </summary>
    /// <param name="sessionId">Session id of profile to delete offer from</param>
    /// <param name="offerId">Id of offer to delete</param>
    protected void DeleteOfferById(MongoId sessionId, MongoId offerId)
    {
        var profileRagfairInfo = profileHelper.GetPmcProfile(sessionId).RagfairInfo;
        var offerIndex = profileRagfairInfo.Offers.FindIndex(o => o.Id == offerId);
        if (offerIndex == -1)
        {
            logger.Warning($"Unable to find offer: {offerId} in profile: {sessionId}, unable to delete");
        }

        if (offerIndex >= 0)
        {
            profileRagfairInfo.Offers.Splice(offerIndex, 1);
        }

        // Also delete from ragfair
        ragfairOfferService.RemoveOfferById(offerId);
    }

    /// <summary>
    /// Complete the selling of players' offer
    /// </summary>
    /// <param name="offerOwnerSessionId">Session/Player id</param>
    /// <param name="offer">Sold offer details</param>
    /// <param name="boughtAmount">Amount item was purchased for</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse CompleteOffer(MongoId offerOwnerSessionId, RagfairOffer offer, int boughtAmount)
    {
        // Pack or ALL items of a multi-offer were bought - remove entire offer
        if (offer.SellInOnePiece.GetValueOrDefault(false) || boughtAmount == offer.Quantity)
        {
            DeleteOfferById(offerOwnerSessionId, offer.Id);
        }
        else
        {
            // Partial purchase, reduce quantity by amount purchased
            offer.Quantity -= boughtAmount;
        }

        // Assemble payment to send to seller now offer was purchased
        var sellerProfile = profileHelper.GetPmcProfile(offerOwnerSessionId);
        var rootItem = offer.Items.FirstOrDefault();
        var itemTpl = rootItem.Template;
        var offerStackCount = rootItem.Upd.StackObjectsCount;
        var paymentItemsToSendToPlayer = new List<Item>();
        foreach (var requirement in offer.Requirements)
        {
            // Create an item template item
            var requestedItem = new Item
            {
                Id = new MongoId(),
                Template = requirement.TemplateId,
                Upd = new Upd { StackObjectsCount = requirement.Count * boughtAmount },
            };

            var stacks = itemHelper.SplitStack(requestedItem);
            foreach (var item in stacks)
            {
                var outItems = new List<Item> { item };

                // TODO - is this code used?, may have been when adding barters to flea was still possible for player
                if (requirement.OnlyFunctional.GetValueOrDefault(false))
                {
                    var presetItems = ragfairServerHelper.GetPresetItemsByTpl(item);
                    if (presetItems.Count > 0)
                    {
                        outItems.Add(presetItems[0]);
                    }
                }

                paymentItemsToSendToPlayer.AddRange(outItems);
            }
        }

        var ragfairDetails = new MessageContentRagfair
        {
            OfferId = offer.Id,
            // pack-offers NEED to be the full item count,
            // otherwise it only removes 1 from the pack, leaving phantom offer on client ui
            Count = offer.SellInOnePiece.GetValueOrDefault(false) ? offerStackCount.Value : boughtAmount,
            HandbookId = itemTpl,
        };

        var storageTimeSeconds = timeUtil.GetHoursAsSeconds((int)questHelper.GetMailItemRedeemTimeHoursForProfile(sellerProfile));
        mailSendService.SendDirectNpcMessageToPlayer(
            offerOwnerSessionId,
            Traders.RAGMAN,
            MessageType.FleamarketMessage,
            GetLocalisedOfferSoldMessage(itemTpl, boughtAmount),
            paymentItemsToSendToPlayer,
            storageTimeSeconds,
            null,
            ragfairDetails
        );

        // Adjust sellers sell sum values
        sellerProfile.RagfairInfo.SellSum ??= 0;
        sellerProfile.RagfairInfo.SellSum += offer.SummaryCost;

        return eventOutputHolder.GetOutput(offerOwnerSessionId);
    }

    /// <summary>
    /// Get a localised message for when players offer has sold on flea
    /// </summary>
    /// <param name="itemTpl">Item sold</param>
    /// <param name="boughtAmount"></param>
    /// <returns>Localised string</returns>
    protected string GetLocalisedOfferSoldMessage(MongoId itemTpl, int boughtAmount)
    {
        // Generate a message to inform that item was sold
        var globalLocales = localeService.GetLocaleDb();
        if (!globalLocales.TryGetValue(GoodSoldTemplate, out var soldMessageLocaleGuid))
        {
            logger.Error(serverLocalisationService.GetText("ragfair-unable_to_find_locale_by_key", GoodSoldTemplate));
        }

        // Used to replace tokens in sold message sent to player
        var messageKey = $"{itemTpl.ToString()} Name";
        var hasKey = globalLocales.TryGetValue(messageKey, out var value);

        var tplVars = new SystemData
        {
            SoldItem = hasKey ? value : itemTpl,
            BuyerNickname = botHelper.GetPmcNicknameOfMaxLength(BotConfig.BotNameLengthLimit),
            ItemCount = boughtAmount,
        };

        // Node searches for anything inside {property}: e.g.: "Your {soldItem} {itemCount} items were bought by {buyerNickname}."
        // each part the takes the inside "Key" and gets it from the tplVars object
        // 'Your Kalashnikov AKS-74U 5.45x39 assault rifle 1 items were bought by HB.'
        // then seems to replace any " with nothing

        // Seems to be much simpler just replacing each key like this.
        soldMessageLocaleGuid = soldMessageLocaleGuid.Replace("{soldItem}", tplVars.SoldItem);
        soldMessageLocaleGuid = soldMessageLocaleGuid.Replace("{itemCount}", tplVars.ItemCount.ToString());
        soldMessageLocaleGuid = soldMessageLocaleGuid.Replace("{buyerNickname}", tplVars.BuyerNickname);
        return soldMessageLocaleGuid;
    }

    /// <summary>
    /// Check an offer passes the various search criteria the player requested
    /// </summary>
    /// <param name="searchRequest">Client search request</param>
    /// <param name="offer">Offer to check</param>
    /// <param name="offerRootItem">root item of offer</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>True if offer passes criteria</returns>
    protected bool PassesSearchFilterCriteria(SearchRequestData searchRequest, RagfairOffer offer, Item offerRootItem, PmcData pmcData)
    {
        var isDefaultUserOffer = offer.User.MemberType == MemberCategory.Default;
        if (pmcData.Info.Level < databaseService.GetGlobals().Configuration.RagFair.MinUserLevel && isDefaultUserOffer)
        // Skip item if player is < global unlock level (default is 15) and item is from a dynamically generated source
        {
            return false;
        }

        var isTraderOffer = offer.IsTraderOffer();
        if (searchRequest.OfferOwnerType == OfferOwnerType.TraderOwnerType && !isTraderOffer)
        // don't include player offers
        {
            return false;
        }

        if (searchRequest.OfferOwnerType == OfferOwnerType.PlayerOwnerType && isTraderOffer)
        // don't include trader offers
        {
            return false;
        }

        if (searchRequest.OneHourExpiration.GetValueOrDefault(false) && offer.EndTime - timeUtil.GetTimeStamp() > TimeUtil.OneHourAsSeconds)
        // offer expires within an hour
        {
            return false;
        }

        if (searchRequest.QuantityFrom > 0 && offerRootItem.Upd.StackObjectsCount < searchRequest.QuantityFrom)
        // Too few items to offer
        {
            return false;
        }

        if (searchRequest.QuantityTo > 0 && offerRootItem.Upd.StackObjectsCount > searchRequest.QuantityTo)
        // Too many items to offer
        {
            return false;
        }

        if (searchRequest.OnlyFunctional.GetValueOrDefault(false) && !IsItemFunctional(offerRootItem, offer))
        // Don't include non-functional items
        {
            return false;
        }

        if (offer.Items.Count == 1)
        {
            // Counts quality % using the offer items current durability compared to its possible max, not current max
            // Single item
            if (
                IsConditionItem(offerRootItem)
                && !ItemQualityInRange(offerRootItem, searchRequest.ConditionFrom.Value, searchRequest.ConditionTo.Value)
            )
            {
                return false;
            }
        }
        else
        {
            var itemQualityPercent = itemHelper.GetItemQualityModifierForItems(offer.Items) * 100;
            if (itemQualityPercent < searchRequest.ConditionFrom)
            {
                return false;
            }

            if (itemQualityPercent > searchRequest.ConditionTo)
            {
                return false;
            }
        }

        if (searchRequest.Currency > 0)
        {
            var offerMoneyTypeTpl = offer.Requirements.FirstOrDefault().TemplateId;
            if (paymentHelper.IsMoneyTpl(offerMoneyTypeTpl))
            {
                // Only want offers with specific currency
                if (
                    ragfairHelper.GetCurrencyTag(offerMoneyTypeTpl)
                    != ragfairHelper.GetCurrencyTag(searchRequest.Currency.GetValueOrDefault(0))
                )
                {
                    // Offer is for different currency to what search params allow, skip
                    return false;
                }
            }
        }

        if (searchRequest.PriceFrom > 0 && searchRequest.PriceFrom >= offer.RequirementsCost)
        // price is too low
        {
            return false;
        }

        if (searchRequest.PriceTo > 0 && searchRequest.PriceTo <= offer.RequirementsCost)
        // price is too high
        {
            return false;
        }

        // Passes above checks, search criteria filters have not filtered offer out
        return true;
    }

    /// <summary>
    /// Check that the passed in offer item is functional
    /// </summary>
    /// <param name="offerRootItem">The root item of the offer</param>
    /// <param name="offer">Flea offer to check</param>
    /// <returns>True if the given item is functional</returns>
    public bool IsItemFunctional(Item offerRootItem, RagfairOffer offer)
    {
        // Non-preset weapons/armor are always functional
        if (!presetHelper.HasPreset(offerRootItem.Template))
        {
            return true;
        }

        // For armor items that can hold mods, make sure the item count is at least the amount of required plates
        if (itemHelper.ArmorItemCanHoldMods(offerRootItem.Template))
        {
            var offerRootTemplate = itemHelper.GetItem(offerRootItem.Template).Value;
            var requiredPlateCount = offerRootTemplate.Properties.Slots?.Where(item => item.Required.GetValueOrDefault(false)).Count();

            return offer.Items.Count > requiredPlateCount;
        }

        // For other presets, make sure the offer has more than 1 item
        return offer.Items.Count > 1;
    }

    /// <summary>
    ///     Does the passed in item have a condition property
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <returns>True if has condition</returns>
    protected bool IsConditionItem(Item item)
    {
        // thanks typescript, undefined assertion is not returnable since it
        // tries to return a multi-type object
        if (item.Upd is null)
        {
            return false;
        }

        return item.Upd.MedKit is not null
            || item.Upd.Repairable is not null
            || item.Upd.Resource is not null
            || item.Upd.FoodDrink is not null
            || item.Upd.Key is not null
            || item.Upd.RepairKit is not null;
    }

    /// <summary>
    ///     Is items quality value within desired range
    /// </summary>
    /// <param name="item">Item to check quality of</param>
    /// <param name="min">Desired minimum quality</param>
    /// <param name="max">Desired maximum quality</param>
    /// <returns>True if in range</returns>
    protected bool ItemQualityInRange(Item item, int min, int max)
    {
        var itemQualityPercentage = 100 * itemHelper.GetItemQualityModifier(item);
        if (min > 0 && min > itemQualityPercentage)
        // Item condition too low
        {
            return false;
        }

        if (max < 100 && max <= itemQualityPercentage)
        // Item condition too high
        {
            return false;
        }

        return true;
    }
}
