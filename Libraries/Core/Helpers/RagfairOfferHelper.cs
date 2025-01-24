using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Utils;
using SptCommon.Extensions;
using Core.Models.Spt.Services;
using Core.Services;

namespace Core.Helpers;

[Injectable]
public class RagfairOfferHelper(
    ISptLogger<RagfairOfferHelper> _logger,
    TimeUtil _timeUtil,
    DatabaseService _databaseService,
    ProfileHelper _profileHelper)
{
    /// <summary>
    /// Passthrough to ragfairOfferService.getOffers(), get flea offers a player should see
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Disable offer if item is flagged by tiered flea config
    /// </summary>
    /// <param name="tieredFlea">Tiered flea settings from ragfair config</param>
    /// <param name="offer">Ragfair offer to check</param>
    /// <param name="tieredFleaLimitTypes">Dict of item types with player level to be viewable</param>
    /// <param name="playerLevel">Level of player viewing offer</param>
    protected void CheckAndLockOfferFromPlayerTieredFlea(
        TieredFlea tieredFlea,
        RagfairOffer offer,
        string[] tieredFleaLimitTypes,
        int playerLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get matching offers that require the desired item and filter out offers from non traders if player is below ragfair unlock level
    /// </summary>
    /// <param name="searchRequest">Search request from client</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Matching RagfairOffer objects</returns>
    public List<RagfairOffer> GetOffersThatRequireItem(SearchRequestData searchRequest, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get offers from flea/traders specifically when building weapon preset
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get offers that have not exceeded buy limits
    /// </summary>
    /// <param name="possibleOffers">offers to process</param>
    /// <returns>Offers</returns>
    protected List<RagfairOffer> GetOffersInsideBuyRestrictionLimits(List<RagfairOffer> possibleOffers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if offer is from trader standing the player does not have
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <param name="pmcProfile">Player profile</param>
    /// <returns>True if item is locked, false if item is purchaseable</returns>
    protected bool TraderOfferLockedBehindLoyaltyLevel(RagfairOffer offer, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if offer item is quest locked for current player by looking at sptQuestLocked property in traders barter_scheme
    /// </summary>
    /// <param name="offer">Offer to check is quest locked</param>
    /// <param name="traderAssorts">all trader assorts for player</param>
    /// <returns>true if quest locked</returns>
    public bool TraderOfferItemQuestLocked(RagfairOffer offer, Dictionary<string, TraderAssort> traderAssorts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Has trader offer ran out of stock to sell to player
    /// </summary>
    /// <param name="offer">Offer to check stock of</param>
    /// <returns>true if out of stock</returns>
    protected bool TraderOutOfStock(RagfairOffer offer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if trader offers' BuyRestrictionMax value has been reached
    /// </summary>
    /// <param name="offer">Offer to check restriction properties of</param>
    /// <returns>true if restriction reached, false if no restrictions/not reached</returns>
    protected bool TraderBuyRestrictionReached(RagfairOffer offer)
    {
        throw new NotImplementedException();
    }

    protected List<string> GetLoyaltyLockedOffers(List<RagfairOffer> offers, PmcData pmcProfile)
    {
        throw new NotImplementedException();
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

        foreach (var offer in profileOffers) {
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
                    boughtAmount = offer.SellResults[0].Amount.Value;
                }

                var ratingToAdd = (offer.SummaryCost / totalItemsCount) * boughtAmount;
                IncreaseProfileRagfairRating(_profileHelper.GetFullProfile(sessionId), ratingToAdd.Value);

                CompleteOffer(sessionId, offer, boughtAmount);
                offer.SellResults.Splice(0, 1); // Remove the sell result object now its been processed
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
        foreach (var itemAndChildren in itemsInInventoryToList) {
            // Only count the root items stack count in total
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
            _logger.Warning($"Unable to increment ragfair rating, value was not a number: { amountToIncrementBy}");

            return;
        }
        profile.CharacterData.PmcData.RagfairInfo.Rating +=
            (ragfairGlobalsConfig.RatingIncreaseCount / ragfairGlobalsConfig.RatingSumForIncrease) *
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
     * @param sessionID Session id of profile to delete offer from
     * @param offerId Id of offer to delete
     */
    protected void DeleteOfferById(string sessionID, string offerId)
    {
        throw new NotImplementedException();
    }

    /**
     * Complete the selling of players' offer
     * @param sessionID Session id
     * @param offer Sold offer details
     * @param boughtAmount Amount item was purchased for
     * @returns ItemEventRouterResponse
     */
    public ItemEventRouterResponse CompleteOffer(string sessionID, RagfairOffer offer, int boughtAmount)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a localised message for when players offer has sold on flea
     * @param itemTpl Item sold
     * @param boughtAmount How many were purchased
     * @returns Localised message text
     */
    protected string GetLocalisedOfferSoldMessage(string itemTpl, int boughtAmount)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /**
     * Check that the passed in offer item is functional
     * @param offerRootItem The root item of the offer
     * @param offer Flea offer to check
     * @returns True if the given item is functional
     */
    public bool IsItemFunctional(Item offerRootItem, RagfairOffer offer)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Should a ragfair offer be visible to the player
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <param name="itemsToAdd">?</param>
    /// <param name="traderAssorts">Trader assort items - used for filtering out locked trader items</param>
    /// <param name="offer">The flea offer</param>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="playerIsFleaBanned">Optional parameter</param>
    /// <returns>True = should be shown to player</returns>
    public bool DisplayableOffer(
        SearchRequestData searchRequest,
        List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        RagfairOffer offer,
        PmcData pmcProfile,
        bool? playerIsFleaBanned = null
    )
    {
        throw new NotImplementedException();
    }

    public bool DisplayableOfferThatNeedsItem(SearchRequestData searchRequest, RagfairOffer offer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the passed in item have a condition property
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <returns>True if has condition</returns>
    protected bool ConditionItem(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is items quality value within desired range
    /// </summary>
    /// <param name="item">Item to check quality of</param>
    /// <param name="min">Desired minimum quality</param>
    /// <param name="max">Desired maximum quality</param>
    /// <returns>True if in range</returns>
    protected bool ItemQualityInRange(Item item, int min, int max)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does this offer come from a trader
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <returns>True = from trader</returns>
    public bool OfferIsFromTrader(RagfairOffer offer)
    {
        return offer.User.MemberType == MemberCategory.TRADER;
    }
}
