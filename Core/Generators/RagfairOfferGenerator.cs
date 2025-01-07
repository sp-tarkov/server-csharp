using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Models.Spt.Ragfair;

namespace Core.Generators;

public class RagfairOfferGenerator
{
    public RagfairOfferGenerator()
    {
    }

    /// <summary>
    /// Create a flea offer and store it in the Ragfair server offers list
    /// </summary>
    /// <param name="userID">Owner of the offer</param>
    /// <param name="time">Time offer is listed at</param>
    /// <param name="items">Items in the offer</param>
    /// <param name="barterScheme">Cost of item (currency or barter)</param>
    /// <param name="loyalLevel">Loyalty level needed to buy item</param>
    /// <param name="sellInOnePiece">Flags sellInOnePiece to be true</param>
    /// <returns>Created flea offer</returns>
    public RagfairOffer CreateAndAddFleaOffer(
        string userID,
        double time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        bool sellInOnePiece = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create an offer object ready to send to ragfairOfferService.addOffer()
    /// </summary>
    /// <param name="userID">Owner of the offer</param>
    /// <param name="time">Time offer is listed at</param>
    /// <param name="items">Items in the offer</param>
    /// <param name="barterScheme">Cost of item (currency or barter)</param>
    /// <param name="loyalLevel">Loyalty level needed to buy item</param>
    /// <param name="isPackOffer">Is offer being created flagged as a pack</param>
    /// <returns>RagfairOffer</returns>
    protected RagfairOffer CreateOffer(
        string userID,
        double time,
        List<Item> items,
        List<BarterScheme> barterScheme,
        int loyalLevel,
        bool isPackOffer = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create the user object stored inside each flea offer object
    /// </summary>
    /// <param name="userID">user creating the offer</param>
    /// <param name="isTrader">Is the user creating the offer a trader</param>
    /// <returns>RagfairOfferUser</returns>
    protected RagfairOfferUser CreateUserDataForFleaOffer(string userID, bool isTrader)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate the offer price that's listed on the flea listing
    /// </summary>
    /// <param name="offerRequirements">barter requirements for offer</param>
    /// <returns>rouble cost of offer</returns>
    protected decimal ConvertOfferRequirementsIntoRoubles(List<OfferRequirement> offerRequirements)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get avatar url from trader table in db
    /// </summary>
    /// <param name="isTrader">Is user we're getting avatar for a trader</param>
    /// <param name="userId">persons id to get avatar of</param>
    /// <returns>url of avatar</returns>
    protected string GetAvatarUrl(bool isTrader, string userId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a count of currency into roubles
    /// </summary>
    /// <param name="currencyCount">amount of currency to convert into roubles</param>
    /// <param name="currencyType">Type of currency (euro/dollar/rouble)</param>
    /// <returns>count of roubles</returns>
    protected double CalculateRoublePrice(decimal currencyCount, string currencyType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check userId, if its a player, return their pmc _id, otherwise return userId parameter
    /// </summary>
    /// <param name="userId">Users Id to check</param>
    /// <returns>Users Id</returns>
    protected string GetTraderId(string userId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a flea trading rating for the passed in user
    /// </summary>
    /// <param name="userId">User to get flea rating of</param>
    /// <returns>Flea rating value</returns>
    protected double GetRating(string userId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the offers user rating growing
    /// </summary>
    /// <param name="userID">user to check rating of</param>
    /// <returns>true if its growing</returns>
    protected bool GetRatingGrowing(string userID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get number of section until offer should expire
    /// </summary>
    /// <param name="userID">Id of the offer owner</param>
    /// <param name="time">Time the offer is posted</param>
    /// <returns>number of seconds until offer expires</returns>
    protected double GetOfferEndTime(string userID, double time)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create multiple offers for items by using a unique list of items we've generated previously
    /// </summary>
    /// <param name="expiredOffers">optional, expired offers to regenerate</param>
    public async Task GenerateDynamicOffers(List<List<Item>> expiredOffers = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="assortItemWithChildren">Item with its children to process into offers</param>
    /// <param name="isExpiredOffer">is an expired offer</param>
    /// <param name="config">Ragfair dynamic config</param>
    protected async Task CreateOffersFromAssort(List<Item> assortItemWithChildren, bool isExpiredOffer, Dynamic config)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// iterate over an items chidren and look for plates above desired level and remove them
    /// </summary>
    /// <param name="presetWithChildren">preset to check for plates</param>
    /// <param name="plateSettings">Settings</param>
    /// <returns>True if plate removed</returns>
    protected bool RemoveBannedPlatesFromPreset(List<Item> presetWithChildren, ArmorPlateBlacklistSettings plateSettings)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create one flea offer for a specific item
    /// </summary>
    /// <param name="sellerId">Id of seller</param>
    /// <param name="itemWithChildren">Item to create offer for</param>
    /// <param name="isPreset">Is item a weapon preset</param>
    /// <param name="itemToSellDetails">Raw db item details</param>
    /// <returns>Item array</returns>
    protected async Task CreateSingleOfferForItem(string sellerId, List<Item> itemWithChildren, bool isPreset, TemplateItem itemToSellDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate trader offers on flea using the traders assort data
    /// </summary>
    /// <param name="traderID">Trader to generate offers for</param>
    public void GenerateFleaOffersForTrader(string traderID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get array of an item with its mods + condition properties (e.g durability)
    /// Apply randomisation adjustments to condition if item base is found in ragfair.json/dynamic/condition
    /// </summary>
    /// <param name="userID">id of owner of item</param>
    /// <param name="itemWithMods">Item and mods, get condition of first item (only first array item is modified)</param>
    /// <param name="itemDetails">db details of first item</param>
    protected void RandomiseOfferItemUpdProperties(string userID, List<Item> itemWithMods, TemplateItem itemDetails)
    {
        throw new NotImplementedException();
    }

    protected string? GetDynamicConditionIdForTpl(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Alter an items condition based on its item base type
     * @param conditionSettingsId also the parentId of item being altered
     * @param itemWithMods Item to adjust condition details of
     * @param itemDetails db item details of first item in array
     */
    protected void RandomiseItemCondition(string conditionSettingsId, List<Item> itemWithMods, TemplateItem itemDetails)
    {
        throw new NotImplementedException();
    }

    /**
     * Adjust an items durability/maxDurability value
     * @param item item (weapon/armor) to Adjust
     * @param itemDbDetails Weapon details from db
     * @param maxMultiplier Value to multiply max durability by
     * @param currentMultiplier Value to multiply current durability by
     */
    protected void RandomiseWeaponDurability(Item item, TemplateItem itemDbDetails, double maxMultiplier, double currentMultiplier)
    {
        throw new NotImplementedException();
    }

    /**
     * Randomise the durability values for an armors plates and soft inserts
     * @param armorWithMods Armor item with its child mods
     * @param currentMultiplier Chosen multiplier to use for current durability value
     * @param maxMultiplier Chosen multiplier to use for max durability value
     */
    protected void RandomiseArmorDurabilityValues(List<Item> armorWithMods, double currentMultiplier, double maxMultiplier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add missing conditions to an item if needed
    /// Durability for repairable items
    /// HpResource for medical items
    /// </summary>
    /// <param name="item">item to add conditions to</param>
    protected void AddMissingConditions(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a barter-based barter scheme, if not possible, fall back to making barter scheme currency based
    /// </summary>
    /// <param name="offerItems">Items for sale in offer</param>
    /// <param name="barterConfig">Barter config from ragfairConfig.dynamic.barter</param>
    /// <returns>Barter scheme</returns>
    protected List<BarterScheme> CreateBarterBarterScheme(List<Item> offerItems, BarterDetails barterConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of flea prices + item tpl, cached in generator class inside `allowedFleaPriceItemsForBarter`
    /// </summary>
    /// <returns>list with tpl/price values</returns>
    protected List<TplWithFleaPrice> GetFleaPricesAsList()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a random currency-based barter scheme for a list of items
    /// </summary>
    /// <param name="offerWithChildren">Items on offer</param>
    /// <param name="isPackOffer">Is the barter scheme being created for a pack offer</param>
    /// <param name="multipler">What to multiply the resulting price by</param>
    /// <returns>Barter scheme for offer</returns>
    protected List<BarterScheme> CreateCurrencyBarterScheme(List<Item> offerWithChildren, bool isPackOffer, double multipler = 1)
    {
        throw new NotImplementedException();
    }
}