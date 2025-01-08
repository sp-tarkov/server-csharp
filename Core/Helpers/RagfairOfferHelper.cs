using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;

namespace Core.Helpers;

public class RagfairOfferHelper
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
        string[] itemsToAdd,
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
        string[] itemsToAdd,
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
     * @param sessionID Session id to process offers for
     * @returns true = complete
     */
    public bool ProcessOffersOnProfile(string sessionID)
    {
        throw new NotImplementedException();
    }

    /**
     * Count up all rootitem StackObjectsCount properties of an array of items
     * @param itemsInInventoryToList items to sum up
     * @returns Total stack count
     */
    public int GetTotalStackCountSize(List<List<Item>> itemsInInventoryToList)
    {
        throw new NotImplementedException();
    }

    /**
     * Add amount to players ragfair rating
     * @param sessionId Profile to update
     * @param amountToIncrementBy Raw amount to add to players ragfair rating (excluding the reputation gain multiplier)
     */
    public void IncreaseProfileRagfairRating(SptProfile profile, int amountToIncrementBy)
    {
        throw new NotImplementedException();
    }

    /**
     * Return all offers a player has listed on a desired profile
     * @param sessionID Session id
     * @returns List of ragfair offers
     */
    protected List<RagfairOffer> GetProfileOffers(string sessionID)
    {
        throw new NotImplementedException();
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
    public bool OfferFromTrader(RagfairOffer offer)
    {
        throw new NotImplementedException();
    }
}
