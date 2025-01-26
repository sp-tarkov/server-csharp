using System.Runtime.InteropServices.JavaScript;
using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Extensions;

namespace Core.Helpers;

[Injectable]
public class RagfairOfferHelper(
    ISptLogger<RagfairOfferHelper> logger,
    TimeUtil timeUtil,
    HashUtil hashUtil,
    EventOutputHolder eventOutputHolder,
    DatabaseService databaseService,
    TraderHelper traderHelper,
    SaveServer saveServer,
    ItemHelper itemHelper,
    BotHelper botHelper,
    PaymentHelper paymentHelper,
    PresetHelper presetHelper,
    ProfileHelper profileHelper,
    QuestHelper questHelper,
    RagfairServerHelper ragfairServerHelper,
    RagfairSortHelper ragfairSortHelper,
    RagfairHelper ragfairHelper,
    RagfairOfferService ragfairOfferService,
    RagfairRequiredItemsService ragfairRequiredItemsService,
    LocaleService localeService,
    LocalisationService localisationService,
    MailSendService mailSendService,
    ConfigServer configServer
)
{
    protected static string goodSoldTemplate = "5bdabfb886f7743e152e867e 0"; // Your {soldItem} {itemCount} items were bought by {buyerNickname}.
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();
    protected QuestConfig questConfig = configServer.GetConfig<QuestConfig>();
    protected BotConfig botConfig = configServer.GetConfig<BotConfig>();

    /**
     * Passthrough to ragfairOfferService.getOffers(), get flea offers a player should see
     * @param searchRequest Data from client
     * @param itemsToAdd ragfairHelper.filterCategories()
     * @param traderAssorts Trader assorts
     * @param pmcData Player profile
     * @returns Offers the player should see
     */
    public List<RagfairOffer> GetValidOffers(
        SearchRequestData searchRequest,
        List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        PmcData pmcData
    )
    {
        var playerIsFleaBanned = profileHelper.PlayerIsFleaBanned(pmcData);
        var tieredFlea = ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType.Keys;
        return ragfairOfferService.GetOffers().Where((offer) => {
            if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData)) {
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

            if (!isDisplayable) {
                return false;
            }

            // Not trader offer + tiered flea enabled
            if (tieredFlea.Enabled && !OfferIsFromTrader(offer)) {
                CheckAndLockOfferFromPlayerTieredFlea(tieredFlea, offer, tieredFleaLimitTypes, pmcData.Info.Level);
            }

            return true;
        });
    }

    /**
     * Disable offer if item is flagged by tiered flea config
     * @param tieredFlea Tiered flea settings from ragfair config
     * @param offer Ragfair offer to check
     * @param tieredFleaLimitTypes Dict of item types with player level to be viewable
     * @param playerLevel Level of player viewing offer
     */
    protected void CheckAndLockOfferFromPlayerTieredFlea(
        TieredFlea tieredFlea,
        RagfairOffer offer,
        List<string> tieredFleaLimitTypes,
        int playerLevel
    )
    {
        var offerItemTpl = offer.Items[0].Template;
        if (tieredFlea?.AmmoTplUnlocks != null && itemHelper.IsOfBaseclass(offerItemTpl, BaseClasses.AMMO)) {
            var unlockLevel = tieredFlea.AmmoTplUnlocks[offerItemTpl];
            if (unlockLevel != null && playerLevel < unlockLevel) {
                offer.Locked = true;
                return;
            }
        }

        // Check for a direct level requirement for the offer item
        var itemLevelRequirement = tieredFlea.UnlocksTpl[offerItemTpl];
        if (itemLevelRequirement != null) {
            if (playerLevel < itemLevelRequirement) {
                offer.Locked = true;

                return;
            }
        }

        // Optimisation - Ensure the item has at least one of the limited base types
        if (itemHelper.IsOfBaseclasses(offerItemTpl, tieredFleaLimitTypes)) {
            // Loop over all flea types to find the matching one
            foreach (var tieredItemType in tieredFleaLimitTypes) {
                if (itemHelper.IsOfBaseclass(offerItemTpl, tieredItemType)) {
                    if (playerLevel < tieredFlea.UnlocksType[tieredItemType]) {
                        offer.Locked = true;
                    }
                    break;
                }
            }
        }
    }

    /**
     * Get matching offers that require the desired item and filter out offers from non traders if player is below ragfair unlock level
     * @param searchRequest Search request from client
     * @param pmcDataPlayer profile
     * @returns Matching IRagfairOffer objects
     */
    public List<RagfairOffer> GetOffersThatRequireItem(SearchRequestData searchRequest, PmcData pmcData)
    {
        // Get all offers that requre the desired item and filter out offers from non traders if player below ragifar unlock
        var requiredOffers = ragfairRequiredItemsService.GetRequiredItemsById(searchRequest.NeededSearchId);
        var tieredFlea = ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType.Keys.ToList();

        return requiredOffers.Where(offer => {
            if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData)) {
                return false;
            }

            if (tieredFlea.Enabled && !OfferIsFromTrader(offer)) {
                CheckAndLockOfferFromPlayerTieredFlea(tieredFlea, offer, tieredFleaLimitTypes, pmcData.Info.Level.Value);
            }

            return true;
        });
    }

    /**
     * Get offers from flea/traders specifically when building weapon preset
     * @param searchRequest Search request data
     * @param itemsToAdd string array of item tpls to search for
     * @param traderAssorts All trader assorts player can access/buy
     * @param pmcData Player profile
     * @returns IRagfairOffer array
     */
    public List<RagfairOffer> GetOffersForBuild(
        SearchRequestData searchRequest,
        List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        PmcData pmcData
    )
    {
        var offersMap = new Dictionary<string, List<RagfairOffer>>();
        var offersToReturn = new List<RagfairOffer>();
        var playerIsFleaBanned = profileHelper.PlayerIsFleaBanned(pmcData);
        var tieredFlea = ragfairConfig.TieredFlea;
        var tieredFleaLimitTypes = tieredFlea.UnlocksType.Keys.ToList();

        foreach (var desiredItemTpl in searchRequest.BuildItems.Keys) {
            var matchingOffers = ragfairOfferService.GetOffersOfType(desiredItemTpl);
            if (matchingOffers == null) {
                // No offers found for this item, skip
                continue;
            }
            foreach (var offer in matchingOffers) {
                // Dont show pack offers
                if (offer.SellInOnePiece ?? false) {
                    continue;
                }

                if (!PassesSearchFilterCriteria(searchRequest, offer, pmcData)) {
                    continue;
                }

                if (!IsDisplayableOffer(
                        searchRequest,
                        itemsToAdd,
                        traderAssorts,
                        offer,
                        pmcData,
                        playerIsFleaBanned)
                ) {
                    continue;
                }

                if (OfferIsFromTrader(offer)) {
                    if (TraderBuyRestrictionReached(offer)) {
                        continue;
                    }

                    if (TraderOutOfStock(offer)) {
                        continue;
                    }

                    if (TraderOfferItemQuestLocked(offer, traderAssorts)) {
                        continue;
                    }

                    if (TraderOfferLockedBehindLoyaltyLevel(offer, pmcData)) {
                        continue;
                    }
                }

                // Tiered flea and not trader offer
                if (tieredFlea.Enabled && !OfferIsFromTrader(offer)) {
                    CheckAndLockOfferFromPlayerTieredFlea(
                        tieredFlea,
                        offer,
                        tieredFleaLimitTypes,
                        pmcData.Info.Level.Value
                    );

                    // Do not add offer to build if user does not have access to it
                    if (offer.Locked ?? false) {
                        continue;
                    }
                }

                var key = offer.Items[0].Template;
                if (!offersMap.ContainsKey(key)) {
                    offersMap.Add(key, []);
                }

                offersMap[key].Add(offer);
            }
        }

        // Get best offer for each item to show on screen
        foreach (var possibleOffers in offersMap.Values) {
            // Remove offers with locked = true (quest locked) when > 1 possible offers
            // single trader item = shows greyed out
            // multiple offers for item = is greyed out
            if (possibleOffers.Count > 1) {
                var lockedOffers = GetLoyaltyLockedOffers(possibleOffers, pmcData);

                // Exclude locked offers + above loyalty locked offers if at least 1 was found
                possibleOffers = possibleOffers.Where((offer) => !(offer.Locked || lockedOffers.includes(offer._id)));

                // Exclude trader offers over their buy restriction limit
                possibleOffers = getOffersInsideBuyRestrictionLimits(possibleOffers);
            }

            // Sort offers by price and pick the best
            var offer = ragfairSortHelper.sortOffers(possibleOffers, RagfairSort.PRICE, 0)[0];
            offersToReturn.push(offer);
        }

        return offersToReturn;
    }

    /**
     * Get offers that have not exceeded buy limits
     * @param possibleOffers offers to process
     * @returns Offers
     */
    protected List<RagfairOffer> GetOffersInsideBuyRestrictionLimits(List<RagfairOffer> possibleOffers) {
        // Check offer has buy limit + is from trader + current buy count is at or over max
        return possibleOffers.Where((offer) => {
            if (
                offer.BuyRestrictionMax != null &&
                OfferIsFromTrader(offer) &&
                offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax
            ) {
                if (offer.BuyRestrictionCurrent >= offer.BuyRestrictionMax) {
                    return false;
                }
            }

            // Doesnt have buy limits, retrun offer
            return true;
        });
    }

    /**
     * Check if offer is from trader standing the player does not have
     * @param offer Offer to check
     * @param pmcProfile Player profile
     * @returns True if item is locked, false if item is purchaseable
     */
    protected bool TraderOfferLockedBehindLoyaltyLevel(RagfairOffer offer, PmcData pmcProfile)
    {
        var userTraderSettings = pmcProfile.TradersInfo[offer.User.Id];

        return userTraderSettings.LoyaltyLevel < offer.LoyaltyLevel;
    }

    /**
     * Check if offer item is quest locked for current player by looking at sptQuestLocked property in traders barter_scheme
     * @param offer Offer to check is quest locked
     * @param traderAssorts all trader assorts for player
     * @returns true if quest locked
     */
    public bool TraderOfferItemQuestLocked(RagfairOffer offer, Dictionary<string, TraderAssort> traderAssorts)
    {
        return offer.Items?.Any(
            i =>
                traderAssorts[offer.User.Id]
                    .BarterScheme[i.Id]
                    ?.Any((bs1) => bs1?.Any((bs2) => bs2.SptQuestLocked ?? false) ?? false) ??
                false
        ) ?? false;
    }

    /**
     * Has trader offer ran out of stock to sell to player
     * @param offer Offer to check stock of
     * @returns true if out of stock
     */
    protected bool TraderOutOfStock(RagfairOffer offer)
    {
        if (offer?.Items?.Count == 0) {
            return true;
        }

        return offer.Items[0]?.Upd?.StackObjectsCount == 0;
    }

    /**
     * Check if trader offers' BuyRestrictionMax value has been reached
     * @param offer Offer to check restriction properties of
     * @returns true if restriction reached, false if no restrictions/not reached
     */
    protected bool TraderBuyRestrictionReached(RagfairOffer offer)
    {
        var traderAssorts = traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;

        // Find item being purchased from traders assorts
        var assortData = traderAssorts.FirstOrDefault(item => item.Id == offer.Items[0].Id);

        // No trader assort data
        if (assortData == null) {
            logger.Warning($"Unable to find trader: {offer.User.Nickname} assort for item: {itemHelper.GetItemName(offer.Items[0].Template)} {offer.Items[0].Template}, cannot check if buy restriction reached");

            return false;
        }

        if (assortData.Upd == null) {
            return false;
        }

        // No restriction values
        // Can't use !assortData.upd.BuyRestrictionX as value could be 0
        var assortUpd = assortData.Upd;
        if (assortUpd.BuyRestrictionMax == null || assortUpd.BuyRestrictionCurrent == null) {
            return false;
        }

        // Current equals max, limit reached
        if (assortUpd.BuyRestrictionCurrent >= assortUpd.BuyRestrictionMax) {
            return true;
        }

        return false;
    }

    /**
     * Get an array of flea offers that are inaccessible to player due to their inadequate loyalty level
     * @param offers Offers to check
     * @param pmcProfile Players profile with trader loyalty levels
     * @returns Array of offer ids player cannot see
     */
    protected List<string> GetLoyaltyLockedOffers(List<RagfairOffer> offers, PmcData pmcProfile)
    {
        var loyaltyLockedOffers = new List<string>();
        foreach (var offer in offers.Where((offer) => OfferIsFromTrader(offer))) {
            var traderDetails = pmcProfile.TradersInfo[offer.User.Id];
            if (traderDetails.LoyaltyLevel < offer.LoyaltyLevel) {
                loyaltyLockedOffers.Add(offer.Id);
            }
        }

        return loyaltyLockedOffers;
    }

    /**
     * Process all player-listed flea offers for a desired profile
     * @param sessionID Session id to process offers for
     * @returns true = complete
     */
    public bool ProcessOffersOnProfile(string sessionID)
    {
        var timestamp = timeUtil.GetTimeStamp();
        var profileOffers = GetProfileOffers(sessionID);

        // No offers, don't do anything
        if (!profileOffers?.length) {
            return true;
        }

        foreach (var offer in profileOffers.values()) {
            if (offer.sellResult?.length > 0 && timestamp >= offer.sellResult[0].sellTime) {
                // Checks first item, first is spliced out of array after being processed
                // Item sold
                var totalItemsCount = 1;
                var boughtAmount = 1;

                if (!offer.sellInOnePiece) {
                    // offer.items.reduce((sum, item) => sum + item.upd?.StackObjectsCount ?? 0, 0);
                    totalItemsCount = getTotalStackCountSize([offer.items]);
                    boughtAmount = offer.sellResult[0].amount;
                }

                var ratingToAdd = (offer.summaryCost / totalItemsCount) * boughtAmount;
                increaseProfileRagfairRating(saveServer.getProfile(sessionID), ratingToAdd);

                completeOffer(sessionID, offer, boughtAmount);
                offer.sellResult.splice(0, 1); // Remove the sell result object now its been processed
            }
        }

        return true;
    }

    /**
     * Count up all rootitem StackObjectsCount properties of an array of items
     * @param itemsInInventoryToList items to sum up
     * @returns Total stack count
     */
    public int GetTotalStackCountSize(List<List<Item>> itemsInInventoryToList)
    {
        var total = 0D;
        foreach (var itemAndChildren in itemsInInventoryToList) {
            // Only count the root items stack count in total
            var rootItem = itemAndChildren[0];
            total += rootItem.Upd?.StackObjectsCount ?? 1;
        }

        return (int) total;
    }

    /**
     * Add amount to players ragfair rating
     * @param sessionId Profile to update
     * @param amountToIncrementBy Raw amount to add to players ragfair rating (excluding the reputation gain multiplier)
     */
    public void IncreaseProfileRagfairRating(SptProfile profile, double? amountToIncrementBy)
    {
        var ragfairGlobalsConfig = databaseService.GetGlobals().Configuration.RagFair;

        profile.CharacterData.PmcData.RagfairInfo.IsRatingGrowing = true;
        if (amountToIncrementBy == null) {
            logger.Warning($"Unable to increment ragfair rating, value was not a number: {amountToIncrementBy}");

            return;
        }
        profile.CharacterData.PmcData.RagfairInfo.Rating +=
            (ragfairGlobalsConfig.RatingIncreaseCount / ragfairGlobalsConfig.RatingSumForIncrease) *
            amountToIncrementBy;
    }

    /**
     * Return all offers a player has listed on a desired profile
     * @param sessionID Session id
     * @returns Array of ragfair offers
     */
    protected List<RagfairOffer> GetProfileOffers(string sessionID)
    {
        var profile = profileHelper.GetPmcProfile(sessionID);

        if (profile.RagfairInfo == null || profile.RagfairInfo.Offers == null) {
            return [];
        }

        return profile.RagfairInfo.Offers;
    }

    /**
     * Delete an offer from a desired profile and from ragfair offers
     * @param sessionID Session id of profile to delete offer from
     * @param offerId Id of offer to delete
     */
    protected void DeleteOfferById(string sessionID, string offerId)
    {
        var profileRagfairInfo = saveServer.GetProfile(sessionID).CharacterData.PmcData.RagfairInfo;
        var index = profileRagfairInfo.Offers.FindIndex((o) => o.Id == offerId);
        profileRagfairInfo.Offers.Splice(index, 1);

        // Also delete from ragfair
        ragfairOfferService.RemoveOfferById(offerId);
    }

    /**
     * Complete the selling of players' offer
     * @param offerOwnerSessionId Session id
     * @param offer Sold offer details
     * @param boughtAmount Amount item was purchased for
     * @returns IItemEventRouterResponse
     */
    public ItemEventRouterResponse CompleteOffer(
        string offerOwnerSessionId,
        RagfairOffer offer,
        int boughtAmount
    )
    {
        var itemTpl = offer.Items[0].Template;
        var paymentItemsToSendToPlayer = new List<Item>();
        var offerStackCount = (int) offer.Items[0].Upd.StackObjectsCount;
        var sellerProfile = profileHelper.GetPmcProfile(offerOwnerSessionId);

        // Pack or ALL items of a multi-offer were bought - remove entire ofer
        if (offer.SellInOnePiece ?? false || boughtAmount == offerStackCount) {
            DeleteOfferById(offerOwnerSessionId, offer.Id);
        } else {
            var offerRootItem = offer.Items[0];

            // Reduce offer root items stack count
            offerRootItem.Upd.StackObjectsCount -= boughtAmount;
        }

        // Assemble payment to send to seller now offer was purchased
        foreach (var requirement in offer.Requirements) {
            // Create an item template item
            var requestedItem = new Item {
                Id = hashUtil.Generate(),
                Template = requirement.Template,
                Upd = new Upd() { StackObjectsCount = requirement.Count * boughtAmount }
            };

            var stacks = itemHelper.SplitStack(requestedItem);
            foreach (var item in stacks) {
                var outItems = new List<Item>() {item};

                // TODO - is this code used?, may have been when adding barters to flea was still possible for player
                if (requirement.OnlyFunctional ?? false) {
                    var presetItems = ragfairServerHelper.GetPresetItemsByTpl(item);
                    if (presetItems.Count != 0) {
                        outItems.Add(presetItems[0]);
                    }
                }

                paymentItemsToSendToPlayer = [..paymentItemsToSendToPlayer, ..outItems];
            }
        }

        var ragfairDetails = {
            offerId: offer._id,
            count: offer.sellInOnePiece ? offerStackCount : boughtAmount, // pack-offers NEED to to be the full item count otherwise it only removes 1 from the pack, leaving phantom offer on client ui
            handbookId: itemTpl,
        };

        mailSendService.SendDirectNpcMessageToPlayer(
            offerOwnerSessionId,
            traderHelper.GetTraderById(Traders.RAGMAN),
            MessageType.FLEAMARKET_MESSAGE,
            GetLocalisedOfferSoldMessage(itemTpl, boughtAmount),
            paymentItemsToSendToPlayer,
            timeUtil.GetHoursAsSeconds(questHelper.GetMailItemRedeemTimeHoursForProfile(sellerProfile)),
            null,
            ragfairDetails,
        );

        // Adjust sellers sell sum values
        sellerProfile.RagfairInfo.sellSum ||= 0;
        sellerProfile.RagfairInfo.sellSum += offer.summaryCost;

        return eventOutputHolder.getOutput(offerOwnerSessionId);
    }

    /**
     * Get a localised message for when players offer has sold on flea
     * @param itemTpl Item sold
     * @param boughtAmount How many were purchased
     * @returns Localised message text
     */
    protected getLocalisedOfferSoldMessage(itemTpl: string, boughtAmount: number): string {
        // Generate a message to inform that item was sold
        var globalLocales = localeService.getLocaleDb();
        var soldMessageLocaleGuid = globalLocales[RagfairOfferHelper.goodSoldTemplate];
        if (!soldMessageLocaleGuid) {
            logger.error(
                localisationService.getText(
                    "ragfair-unable_to_find_locale_by_key",
                    RagfairOfferHelper.goodSoldTemplate,
                ),
            );
        }

        // Used to replace tokens in sold message sent to player
        var tplVars: ISystemData = {
            soldItem: globalLocales["${itemTpl} Name"] || itemTpl,
            buyerNickname: botHelper.getPmcNicknameOfMaxLength(botConfig.botNameLengthLimit),
            itemCount: boughtAmount,
        };

        var offerSoldMessageText = soldMessageLocaleGuid.replace(/{\w+}/g, (matched) => {
            return tplVars[matched.replace(/{|}/g, "")];
        });

        return offerSoldMessageText.replace(/"/g, "");
    }

    /**
     * Check an offer passes the various search criteria the player requested
     * @param searchRequest Client search request
     * @param offer Offer to check
     * @param pmcData Player profile
     * @returns True if offer passes criteria
     */
    protected passesSearchFilterCriteria(
        searchRequest: ISearchRequestData,
        offer: IRagfairOffer,
        pmcData: IPmcData,
    ): boolean {
        var isDefaultUserOffer = offer.user.memberType === MemberCategory.DEFAULT;
        var offerRootItem = offer.items[0];
        var moneyTypeTpl = offer.requirements[0]._tpl;
        var isTraderOffer = offerIsFromTrader(offer);

        if (pmcData.Info.Level < databaseService.getGlobals().config.RagFair.minUserLevel && isDefaultUserOffer) {
            // Skip item if player is < global unlock level (default is 15) and item is from a dynamically generated source
            return false;
        }

        if (searchRequest.offerOwnerType === OfferOwnerType.TRADEROWNERTYPE && !isTraderOffer) {
            // don't include player offers
            return false;
        }

        if (searchRequest.offerOwnerType === OfferOwnerType.PLAYEROWNERTYPE && isTraderOffer) {
            // don't include trader offers
            return false;
        }

        if (
            searchRequest.oneHourExpiration &&
            offer.endTime - timeUtil.getTimestamp() > TimeUtil.ONE_HOUR_AS_SECONDS
        ) {
            // offer expires within an hour
            return false;
        }

        if (searchRequest.quantityFrom > 0 && searchRequest.quantityFrom >= offerRootItem.upd.StackObjectsCount) {
            // too little items to offer
            return false;
        }

        if (searchRequest.quantityTo > 0 && searchRequest.quantityTo <= offerRootItem.upd.StackObjectsCount) {
            // too many items to offer
            return false;
        }

        if (searchRequest.onlyFunctional && !isItemFunctional(offerRootItem, offer)) {
            // don't include non-functional items
            return false;
        }

        if (offer.items.length === 1) {
            // Single item
            if (
                isConditionItem(offerRootItem) &&
                !itemQualityInRange(offerRootItem, searchRequest.conditionFrom, searchRequest.conditionTo)
            ) {
                return false;
            }
        } else {
            var itemQualityPercent = itemHelper.getItemQualityModifierForItems(offer.items) * 100;
            if (itemQualityPercent < searchRequest.conditionFrom) {
                return false;
            }

            if (itemQualityPercent > searchRequest.conditionTo) {
                return false;
            }
        }

        if (searchRequest.currency > 0 && paymentHelper.isMoneyTpl(moneyTypeTpl)) {
            var currencies = ["all", "RUB", "USD", "EUR"];

            if (ragfairHelper.getCurrencyTag(moneyTypeTpl) !== currencies[searchRequest.currency]) {
                // don't include item paid in wrong currency
                return false;
            }
        }

        if (searchRequest.priceFrom > 0 && searchRequest.priceFrom >= offer.requirementsCost) {
            // price is too low
            return false;
        }

        if (searchRequest.priceTo > 0 && searchRequest.priceTo <= offer.requirementsCost) {
            // price is too high
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
    public isItemFunctional(offerRootItem: IItem, offer: IRagfairOffer): boolean {
        // Non-preset weapons/armor are always functional
        if (!presetHelper.hasPreset(offerRootItem._tpl)) {
            return true;
        }

        // For armor items that can hold mods, make sure the item count is atleast the amount of required plates
        if (itemHelper.armorItemCanHoldMods(offerRootItem._tpl)) {
            var offerRootTemplate = itemHelper.getItem(offerRootItem._tpl)[1];
            var requiredPlateCount = offerRootTemplate._props.Slots?.filter((item) => item._required)?.length;

            return offer.items.length > requiredPlateCount;
        }

        // For other presets, make sure the offer has more than 1 item
        return offer.items.length > 1;
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
    public isDisplayableOffer(
        searchRequest: ISearchRequestData,
        itemsToAdd: string[],
        traderAssorts: Record<string, ITraderAssort>,
        offer: IRagfairOffer,
        pmcProfile: IPmcData,
        playerIsFleaBanned?: boolean,
    ): boolean {
        var offerRootItem = offer.items[0];
        /** Currency offer is sold for */
        var moneyTypeTpl = offer.requirements[0]._tpl;
        var isTraderOffer = offer.user.id in databaseService.getTraders();

        if (!isTraderOffer && playerIsFleaBanned) {
            return false;
        }

        // Offer root items tpl not in searched for array
        if (!itemsToAdd?.includes(offerRootItem._tpl)) {
            // skip items we shouldn't include
            return false;
        }

        // Performing a required search and offer doesn't have requirement for item
        if (
            searchRequest.neededSearchId &&
            !offer.requirements.some((requirement) => requirement._tpl === searchRequest.neededSearchId)
        ) {
            return false;
        }

        // Weapon/equipment search + offer is preset
        if (
            Object.keys(searchRequest.buildItems).length === 0 && // Prevent equipment loadout searches filtering out presets
            searchRequest.buildCount &&
            presetHelper.hasPreset(offerRootItem._tpl)
        ) {
            return false;
        }

        // commented out as required search "which is for checking offers that are barters"
        // has info.removeBartering as true, this if statement removed barter items.
        if (searchRequest.removeBartering && !paymentHelper.isMoneyTpl(moneyTypeTpl)) {
            // Don't include barter offers
            return false;
        }

        if (JSType.Number.isNaN(offer.requirementsCost)) {
            // Don't include offers with undefined or NaN in it
            return false;
        }

        // Handle trader items to remove items that are not available to the user right now
        // e.g. required search for "lamp" shows 4 items, 3 of which are not available to a new player
        // filter those out
        if (isTraderOffer) {
            if (!(offer.user.id in traderAssorts)) {
                // trader not visible on flea market
                return false;
            }

            if (
                !traderAssorts[offer.user.id].items.some((item) => {
                    return item._id === offer.root;
                })
            ) {
                // skip (quest) locked items
                return false;
            }
        }

        return true;
    }

    public isDisplayableOfferThatNeedsItem(searchRequest: ISearchRequestData, offer: IRagfairOffer): boolean {
        if (offer.requirements.some((requirement) => requirement._tpl === searchRequest.neededSearchId)) {
            return true;
        }

        return false;
    }

    /**
     * Does the passed in item have a condition property
     * @param item Item to check
     * @returns True if has condition
     */
    protected isConditionItem(item: IItem): boolean {
        // thanks typescript, undefined assertion is not returnable since it
        // tries to return a multitype object
        return !!(
            item.upd.MedKit ||
            item.upd.Repairable ||
            item.upd.Resource ||
            item.upd.FoodDrink ||
            item.upd.Key ||
            item.upd.RepairKit
        );
    }

    /**
     * Is items quality value within desired range
     * @param item Item to check quality of
     * @param min Desired minimum quality
     * @param max Desired maximum quality
     * @returns True if in range
     */
    protected itemQualityInRange(item: IItem, min: number, max: number): boolean {
        var itemQualityPercentage = 100 * itemHelper.getItemQualityModifier(item);
        if (min > 0 && min > itemQualityPercentage) {
            // Item condition too low
            return false;
        }

        if (max < 100 && max <= itemQualityPercentage) {
            // Item condition too high
            return false;
        }

        return true;
    }

    /**
     * Does this offer come from a trader
     * @param offer Offer to check
     * @returns True = from trader
     */
    public offerIsFromTrader(offer: IRagfairOffer) {
        return offer.user.memberType === MemberCategory.TRADER;
    }
}
