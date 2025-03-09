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
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using System.Collections.Frozen;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairOfferHelper(
    ISptLogger<RagfairOfferHelper> _logger,
    TimeUtil _timeUtil,
    HashUtil _hashUtil,
    BotHelper _botHelper,
    RagfairSortHelper _ragfairSortHelper,
    PresetHelper _presetHelper,
    RagfairHelper _ragfairHelper,
    PaymentHelper _paymentHelper,
    TraderHelper _traderHelper,
    QuestHelper _questHelper,
    RagfairServerHelper _ragfairServerHelper,
    ItemHelper _itemHelper,
    DatabaseService _databaseService,
    RagfairOfferService _ragfairOfferService,
    LocaleService _localeService,
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    RagfairRequiredItemsService _ragfairRequiredItemsService,
    ProfileHelper _profileHelper,
    EventOutputHolder _eventOutputHolder,
    ConfigServer _configServer)
{
    protected const string _goodSoldTemplate = "5bdabfb886f7743e152e867e 0"; // Your {soldItem} {itemCount} items were bought by {buyerNickname}.
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    protected static readonly FrozenSet<string> _currencies = ["all", "RUB", "USD", "EUR"];

    /// <summary>
    ///     Passthrough to ragfairOfferService.getOffers(), get flea offers a player should see
    /// </summary>
    /// <param name="searchRequest">Data from client</param>
    /// <param name="itemsToAdd">ragfairHelper.filterCategories()</param>
    /// <param name="traderAssorts">Trader assorts</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Offers the player should see</returns>
    public List<RagfairOffer> GetValidOffers(
        SearchRequestData searchRequest,
        List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        PmcData pmcData)
    {
        var playerIsFleaBanned = _profileHelper.PlayerIsFleaBanned(pmcData);
        var tieredFlea = _ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;
        return _ragfairOfferService.GetOffers()
            .Where(
                offer =>
                {
                    if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData))
                    {
                        return false;
                    }

                    var isDisplayable = IsDisplayableOffer(
                        searchRequest,
                        itemsToAdd,
                        traderAssorts,
                        offer,
                        pmcData,
                        playerIsFleaBanned
                    );

                    if (!isDisplayable)
                    {
                        return false;
                    }

                    // Not trader offer + tiered flea enabled
                    if (tieredFlea.Enabled && !OfferIsFromTrader(offer))
                    {
                        CheckAndLockOfferFromPlayerTieredFlea(
                            tieredFlea,
                            offer,
                            tieredFleaLimitTypes.Keys.ToList(),
                            pmcData.Info.Level.Value
                        );
                    }

                    return true;
                }
            )
            .ToList();
    }

    /// <summary>
    ///     Disable offer if item is flagged by tiered flea config
    /// </summary>
    /// <param name="tieredFlea">Tiered flea settings from ragfair config</param>
    /// <param name="offer">Ragfair offer to check</param>
    /// <param name="tieredFleaLimitTypes">Dict of item types with player level to be viewable</param>
    /// <param name="playerLevel">Level of player viewing offer</param>
    protected void CheckAndLockOfferFromPlayerTieredFlea(
        TieredFlea tieredFlea,
        RagfairOffer offer,
        List<string> tieredFleaLimitTypes,
        int playerLevel)
    {
        var offerItemTpl = offer.Items.FirstOrDefault().Template;
        if (tieredFlea.AmmoTplUnlocks is not null && _itemHelper.IsOfBaseclass(offerItemTpl, BaseClasses.AMMO))
        {
            if (tieredFlea.AmmoTplUnlocks.TryGetValue(offerItemTpl, out var unlockLevel) && playerLevel < unlockLevel)
            {
                offer.Locked = true;

                return;
            }
        }

        // Check for a direct level requirement for the offer item
        if (tieredFlea.UnlocksTpl.TryGetValue(offerItemTpl, out var itemLevelRequirement))
        {
            if (playerLevel < itemLevelRequirement)
            {
                offer.Locked = true;

                return;
            }
        }

        // Optimisation - Ensure the item has at least one of the limited base types
        if (_itemHelper.IsOfBaseclasses(offerItemTpl, tieredFleaLimitTypes))
            // Loop over flea types
        {
            foreach (var tieredItemType in tieredFleaLimitTypes
                         .Where(tieredItemType => _itemHelper.IsOfBaseclass(offerItemTpl, tieredItemType)))
            {
                if (playerLevel < tieredFlea.UnlocksType[tieredItemType])
                {
                    offer.Locked = true;
                }

                break;
            }
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
        // Get all offers that require the desired item and filter out offers from non traders if player below ragifar unlock
        var requiredOffers = _ragfairRequiredItemsService.GetRequiredItemsById(searchRequest.NeededSearchId);
        var tieredFlea = _ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;
        return requiredOffers.Where(
                offer =>
                {
                    if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData))
                    {
                        return false;
                    }

                    if (tieredFlea.Enabled && !OfferIsFromTrader(offer))
                    {
                        CheckAndLockOfferFromPlayerTieredFlea(
                            tieredFlea,
                            offer,
                            tieredFleaLimitTypes.Keys.ToList(),
                            pmcData.Info.Level.Value
                        );
                    }

                    return true;
                }
            )
            .ToList();
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
        List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        PmcData pmcData)
    {
        var offersMap = new Dictionary<string, List<RagfairOffer>>();
        var offersToReturn = new List<RagfairOffer>();
        var playerIsFleaBanned = _profileHelper.PlayerIsFleaBanned(pmcData);
        var tieredFlea = _ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType;

        foreach (var desiredItemTpl in searchRequest.BuildItems)
        {
            var matchingOffers = _ragfairOfferService.GetOffersOfType(desiredItemTpl.Key);
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

                if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData))
                {
                    continue;
                }

                if (
                    !IsDisplayableOffer(
                        searchRequest,
                        itemsToAdd,
                        traderAssorts,
                        offer,
                        pmcData,
                        playerIsFleaBanned
                    )
                )
                {
                    continue;
                }

                if (OfferIsFromTrader(offer))
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
                if (tieredFlea.Enabled && !OfferIsFromTrader(offer))
                {
                    CheckAndLockOfferFromPlayerTieredFlea(
                        tieredFlea,
                        offer,
                        tieredFleaLimitTypes.Keys.ToList(),
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
                offersToSort = possibleOffers.Where(
                        offer => !(offer.Locked.GetValueOrDefault(false) || lockedOffers.Contains(offer.Id))
                    )
                    .ToList();

                // Exclude trader offers over their buy restriction limit
                offersToSort = GetOffersInsideBuyRestrictionLimits(offersToSort);
            }

            // Sort offers by price and pick the best
            var offer = _ragfairSortHelper.SortOffers(offersToSort, RagfairSort.PRICE)[0];
            offersToReturn.Add(offer);
        }

        return offersToReturn;
    }

    /**
     * Should a ragfair offer be visible to the player
     * @param searchRequest Search request
     * @param itemsToAdd ?
     * @param traderAssorts Trader assort items - used for filtering out locked trader items
     * @param offer The flea offer
     * @param pmcProfile Player profile
     * @returns True = should be shown to player
     */
    private bool IsDisplayableOffer(SearchRequestData searchRequest, List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts, RagfairOffer offer, PmcData pmcProfile,
        bool playerIsFleaBanned = false)
    {
        var offerRootItem = offer.Items[0];
        /** Currency offer is sold for */
        var moneyTypeTpl = offer.Requirements[0].Template;
        var isTraderOffer = _databaseService.GetTraders().ContainsKey(offer.User.Id);

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
            !string.IsNullOrEmpty(searchRequest.NeededSearchId) &&
            !offer.Requirements.Any(requirement => requirement.Template == searchRequest.NeededSearchId)
        )
        {
            return false;
        }

        // Weapon/equipment search + offer is preset
        if (
            searchRequest.BuildItems.Count == 0 && // Prevent equipment loadout searches filtering out presets
            searchRequest.BuildCount.GetValueOrDefault(0) > 0 &&
            _presetHelper.HasPreset(offerRootItem.Template))
        {
            return false;
        }

        // commented out as required search "which is for checking offers that are barters"
        // has info.removeBartering as true, this if statement removed barter items.
        if (searchRequest.RemoveBartering.GetValueOrDefault(false) && !_paymentHelper.IsMoneyTpl(moneyTypeTpl))
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
            if (!traderAssorts.ContainsKey(offer.User.Id))
                // trader not visible on flea market
            {
                return false;
            }

            if (
                    !traderAssorts[offer.User.Id]
                        .Items.Any(
                            item =>
                            {
                                return item.Id == offer.Root;
                            }
                        )
                )
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
        return possibleOffers.Where(
                offer =>
                {
                    if (
                        offer.BuyRestrictionMax is null &&
                        OfferIsFromTrader(offer) &&
                        offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax
                    )
                    {
                        if (offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax)
                        {
                            return false;
                        }
                    }

                    // Doesnt have buy limits, retrun offer
                    return true;
                }
            )
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
            _logger.Warning(
                $"Trader: {offer.User.Id} not found in profile, assuming offer is not locked being loyalty level"
            );
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
    public bool TraderOfferItemQuestLocked(RagfairOffer offer, Dictionary<string, TraderAssort> traderAssorts)
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

        foreach (var item in offer.Items)
        {
            traderAssorts.TryGetValue(offer.User.Id, out var assorts);
            if (assorts.BarterScheme
                .Where(x => itemIds.Contains(x.Key))
                .Any(
                    barterKvP => barterKvP.Value
                        .Any(
                            subBarter => subBarter
                                .Any(subBarter => subBarter.SptQuestLocked.GetValueOrDefault(false))
                        )
                ))
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
        if (offer?.Items?.Count == 0)
        {
            return true;
        }

        return offer.Items[0]?.Upd?.StackObjectsCount == 0;
    }

    /// <summary>
    ///     Check if trader offers' BuyRestrictionMax value has been reached
    /// </summary>
    /// <param name="offer">Offer to check restriction properties of</param>
    /// <returns>true if restriction reached, false if no restrictions/not reached</returns>
    protected bool TraderBuyRestrictionReached(RagfairOffer offer)
    {
        var traderAssorts = _traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;

        // Find item being purchased from traders assorts
        var assortData = traderAssorts.FirstOrDefault(item => item.Id == offer.Items[0].Id);
        if (assortData is null)
        {
            // No trader assort data
            _logger.Warning(
                $"Unable to find trader: " +
                $"${offer.User.Nickname}assort for item: {_itemHelper.GetItemName(offer.Items[0].Template)} " +
                $"{offer.Items[0].Template}, cannot check if buy restriction reached"
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

    protected HashSet<string> GetLoyaltyLockedOffers(List<RagfairOffer> offers, PmcData pmcProfile)
    {
        var loyaltyLockedOffers = new HashSet<string>();
        foreach (var offer in offers.Where(offer => OfferIsFromTrader(offer)))
        {
            if (pmcProfile.TradersInfo.TryGetValue(offer.User.Id, out var traderDetails) &&
                traderDetails.LoyaltyLevel < offer.LoyaltyLevel)
            {
                loyaltyLockedOffers.Add(offer.Id);
            }
        }

        return loyaltyLockedOffers;
    }

    /**
     * Process all player-listed flea offers for a desired profile
     * @param sessionId Session id to process offers for
     * @returns true = complete
     */
    public bool ProcessOffersOnProfile(string sessionId)
    {
        var timestamp = _timeUtil.GetTimeStamp();
        var profileOffers = GetProfileOffers(sessionId);

        // No offers, don't do anything
        if (profileOffers?.Count == 0)
        {
            return true;
        }

        // Index backwards as CompleteOffer() can delete offer object
        for (var index = profileOffers.Count - 1; index >= 0; index--)
        {
            var offer = profileOffers[index];
            var firstSellResult = offer.SellResults?.FirstOrDefault();
            if (offer.SellResults?.Count > 0 && timestamp >= offer.SellResults[0].SellTime)
            {
                // Checks first item, first is spliced out of array after being processed
                // Item sold
                var totalItemsCount = 1d;
                var boughtAmount = 1;

                if (!offer.SellInOnePiece.GetValueOrDefault(false))
                {
                    // offer.items.reduce((sum, item) => sum + item.upd?.StackObjectsCount ?? 0, 0);
                    totalItemsCount = GetTotalStackCountSize([offer.Items]);
                    boughtAmount = firstSellResult.Amount.Value;
                }

                var ratingToAdd = offer.SummaryCost / totalItemsCount * boughtAmount;
                IncreaseProfileRagfairRating(_profileHelper.GetFullProfile(sessionId), ratingToAdd.Value);

                offer.SellResults.Remove(firstSellResult); // Remove the sell result object now it has been processed

                // Can delete offer object, must run last
                CompleteOffer(sessionId, offer, boughtAmount);
            }
        }

        return true;
    }

    /**
     * Count up all rootitem StackObjectsCount properties of an array of items
     * @param itemsInInventoryToList items to sum up
     * @returns Total stack count
     */
    public double GetTotalStackCountSize(List<List<Item>> itemsInInventoryToList)
    {
        var total = 0d;
        foreach (var itemAndChildren in itemsInInventoryToList)
            // Only count the root items stack count in total
        {
            total += itemAndChildren[0]?.Upd?.StackObjectsCount.GetValueOrDefault(1) ?? 1;
        }

        return total;
    }

    /**
     * Add amount to players ragfair rating
     * @param sessionId Profile to update
     * @param amountToIncrementBy Raw amount to add to players ragfair rating (excluding the reputation gain multiplier)
     */
    public void IncreaseProfileRagfairRating(SptProfile profile, double? amountToIncrementBy)
    {
        var ragfairGlobalsConfig = _databaseService.GetGlobals().Configuration.RagFair;

        profile.CharacterData.PmcData.RagfairInfo.IsRatingGrowing = true;
        if (amountToIncrementBy is null)
        {
            _logger.Warning($"Unable to increment ragfair rating, value was not a number: {amountToIncrementBy}");

            return;
        }

        profile.CharacterData.PmcData.RagfairInfo.Rating +=
            ragfairGlobalsConfig.RatingIncreaseCount /
            ragfairGlobalsConfig.RatingSumForIncrease *
            amountToIncrementBy;
    }

    /**
     * Return all offers a player has listed on a desired profile
     * @param sessionId Session id
     * @returns List of ragfair offers
     */
    protected List<RagfairOffer> GetProfileOffers(string sessionId)
    {
        var profile = _profileHelper.GetPmcProfile(sessionId);

        if (profile.RagfairInfo?.Offers is null)
        {
            return [];
        }

        return profile.RagfairInfo.Offers;
    }

    /**
     * Delete an offer from a desired profile and from ragfair offers
     * @param sessionId Session id of profile to delete offer from
     * @param offerId Id of offer to delete
     */
    protected void DeleteOfferById(string sessionId, string offerId)
    {
        var profileRagfairInfo = _profileHelper.GetPmcProfile(sessionId).RagfairInfo;
        var offerIndex = profileRagfairInfo.Offers.FindIndex(o => o.Id == offerId);
        if (offerIndex == -1)
        {
            _logger.Warning($"Unable to find offer: {offerId} in profile: {sessionId}, unable to delete");
        }

        if (offerIndex >= 0)
        {
            profileRagfairInfo.Offers.Splice(offerIndex, 1);
        }


        // Also delete from ragfair
        _ragfairOfferService.RemoveOfferById(offerId);
    }

    /**
     * Complete the selling of players' offer
     * @param sessionID Session id
     * @param offer Sold offer details
     * @param boughtAmount Amount item was purchased for
     * @returns ItemEventRouterResponse
     */
    public ItemEventRouterResponse CompleteOffer(string offerOwnerSessionId, RagfairOffer offer, int boughtAmount)
    {
        var rootItem = offer.Items.FirstOrDefault();
        var itemTpl = rootItem.Template;
        var paymentItemsToSendToPlayer = new List<Item>();
        var offerStackCount = rootItem.Upd.StackObjectsCount;
        var sellerProfile = _profileHelper.GetPmcProfile(offerOwnerSessionId);

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
        foreach (var requirement in offer.Requirements)
        {
            // Create an item template item
            var requestedItem = new Item
            {
                Id = _hashUtil.Generate(),
                Template = requirement.Template,
                Upd = new Upd
                {
                    StackObjectsCount = requirement.Count * boughtAmount
                }
            };

            var stacks = _itemHelper.SplitStack(requestedItem);
            foreach (var item in stacks)
            {
                var outItems = new List<Item>
                {
                    item
                };

                // TODO - is this code used?, may have been when adding barters to flea was still possible for player
                if (requirement.OnlyFunctional.GetValueOrDefault(false))
                {
                    var presetItems = _ragfairServerHelper.GetPresetItemsByTpl(item);
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
            HandbookId = itemTpl
        };

        _mailSendService.SendDirectNpcMessageToPlayer(
            offerOwnerSessionId,
            _traderHelper.GetTraderById(Traders.RAGMAN).ToString(),
            MessageType.FLEAMARKET_MESSAGE,
            GetLocalisedOfferSoldMessage(itemTpl, boughtAmount),
            paymentItemsToSendToPlayer,
            _timeUtil.GetHoursAsSeconds((int) _questHelper.GetMailItemRedeemTimeHoursForProfile(sellerProfile).Value),
            null,
            ragfairDetails
        );

        // Adjust sellers sell sum values
        sellerProfile.RagfairInfo.SellSum ??= 0;
        sellerProfile.RagfairInfo.SellSum += offer.SummaryCost;

        return _eventOutputHolder.GetOutput(offerOwnerSessionId);
    }

    /**
     * Get a localised message for when players offer has sold on flea
     * @param itemTpl Item sold
     * @param boughtAmount How many were purchased
     * @returns Localised message text
     */
    protected string GetLocalisedOfferSoldMessage(string itemTpl, int boughtAmount)
    {
        // Generate a message to inform that item was sold
        var globalLocales = _localeService.GetLocaleDb();
        if (!globalLocales.TryGetValue(_goodSoldTemplate, out var soldMessageLocaleGuid))
        {
            _logger.Error(
                _localisationService.GetText("ragfair-unable_to_find_locale_by_key", _goodSoldTemplate)
            );
        }

        // Used to replace tokens in sold message sent to player
        var messageKey = $"{itemTpl} Name";
        var hasKey = globalLocales.TryGetValue(messageKey, out var value);

        var tplVars = new SystemData
        {
            SoldItem = hasKey ? value : itemTpl,
            BuyerNickname = _botHelper.GetPmcNicknameOfMaxLength(_botConfig.BotNameLengthLimit),
            ItemCount = boughtAmount
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

    /**
     * Check an offer passes the various search criteria the player requested
     * @param searchRequest Client search request
     * @param offer Offer to check
     * @param pmcData Player profile
     * @returns True if offer passes criteria
     */
    protected bool PassesSearchFilterCriteria(SearchRequestData searchRequest, RagfairOffer offer, PmcData pmcData)
    {
        var isDefaultUserOffer = offer.User.MemberType == MemberCategory.Default;
        var offerRootItem = offer.Items[0];
        var offerMoneyTypeTpl = offer.Requirements[0].Template;
        var isTraderOffer = OfferIsFromTrader(offer);

        if (pmcData.Info.Level < _databaseService.GetGlobals().Configuration.RagFair.MinUserLevel && isDefaultUserOffer)
            // Skip item if player is < global unlock level (default is 15) and item is from a dynamically generated source
        {
            return false;
        }

        if (searchRequest.OfferOwnerType == OfferOwnerType.TRADEROWNERTYPE && !isTraderOffer)
            // don't include player offers
        {
            return false;
        }

        if (searchRequest.OfferOwnerType == OfferOwnerType.PLAYEROWNERTYPE && isTraderOffer)
            // don't include trader offers
        {
            return false;
        }

        if (
                searchRequest.OneHourExpiration.GetValueOrDefault(false) &&
                offer.EndTime - _timeUtil.GetTimeStamp() > TimeUtil.OneHourAsSeconds
            )
            // offer expires within an hour
        {
            return false;
        }

        if (searchRequest.QuantityFrom > 0 && offerRootItem.Upd.StackObjectsCount < searchRequest.QuantityFrom)
            // too little items to offer
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
                IsConditionItem(offerRootItem) &&
                !ItemQualityInRange(offerRootItem, searchRequest.ConditionFrom.Value, searchRequest.ConditionTo.Value)
            )
            {
                return false;
            }
        }
        else
        {
            var itemQualityPercent = _itemHelper.GetItemQualityModifierForItems(offer.Items) * 100;
            if (itemQualityPercent < searchRequest.ConditionFrom)
            {
                return false;
            }

            if (itemQualityPercent > searchRequest.ConditionTo)
            {
                return false;
            }
        }

        if (searchRequest.Currency > 0 && _paymentHelper.IsMoneyTpl(offerMoneyTypeTpl))
        {
            // Use 'currencies' as mapping for the money choice dropdown, e.g. 0 = all, 2 = "USD;
            if(!_currencies.Contains(_ragfairHelper.GetCurrencyTag(offerMoneyTypeTpl)))
                // Don't include item paid in wrong currency
            {
                return false;
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

    /**
     * Check that the passed in offer item is functional
     * @param offerRootItem The root item of the offer
     * @param offer Flea offer to check
     * @returns True if the given item is functional
     */
    public bool IsItemFunctional(Item offerRootItem, RagfairOffer offer)
    {
        // Non-preset weapons/armor are always functional
        if (!_presetHelper.HasPreset(offerRootItem.Template))
        {
            return true;
        }

        // For armor items that can hold mods, make sure the item count is at least the amount of required plates
        if (_itemHelper.ArmorItemCanHoldMods(offerRootItem.Template))
        {
            var offerRootTemplate = _itemHelper.GetItem(offerRootItem.Template).Value;
            var requiredPlateCount = offerRootTemplate.Properties.Slots
                ?.Where(item => item.Required.GetValueOrDefault(false))
                ?.Count();

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

        return item.Upd.MedKit is not null ||
               item.Upd.Repairable is not null ||
               item.Upd.Resource is not null ||
               item.Upd.FoodDrink is not null ||
               item.Upd.Key is not null ||
               item.Upd.RepairKit is not null;
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
        var itemQualityPercentage = 100 * _itemHelper.GetItemQualityModifier(item);
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

    /// <summary>
    ///     Does this offer come from a trader
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <returns>True = from trader</returns>
    public bool OfferIsFromTrader(RagfairOffer offer)
    {
        return offer.User.MemberType == MemberCategory.Trader;
    }
}
