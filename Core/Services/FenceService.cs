using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Fence;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class FenceService
{
    /// <summary>
    /// Replace main fence assort with new assort
    /// </summary>
    /// <param name="assort">New assorts to replace old with</param>
    public void SetFenceAssort(TraderAssort assort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Replace discount fence assort with new assort
    /// </summary>
    /// <param name="assort">New assorts to replace old with</param>
    public void SetDiscountFenceAssort(TraderAssort assort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get main fence assort
    /// </summary>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetMainFenceAssort()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get discount fence assort
    /// </summary>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetDiscountFenceAssort()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Replace high rep level fence assort with new assort
    /// </summary>
    /// <param name="discountAssort">New assorts to replace old with</param>
    public void SetFenceDiscountAssort(TraderAssort discountAssort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get assorts player can purchase
    /// Adjust prices based on fence level of player
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetFenceAssorts(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds to fence assort a single item (with its children)
    /// </summary>
    /// <param name="items">the items to add with all its childrens</param>
    /// <param name="mainItem">the most parent item of the array</param>
    public void AddItemsToFenceAssort(List<Item> items, Item mainItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates the overall price for an item (with all its children)
    /// </summary>
    /// <param name="itemTpl">the item tpl to calculate the fence price for</param>
    /// <param name="items">the items (with its children) to calculate fence price for</param>
    /// <returns>the fence price of the item</returns>
    public double GetItemPrice(string itemTpl, List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate the overall price for an ammo box, where only one item is
    /// the ammo box itself and every other items are the bullets in that box
    /// </summary>
    /// <param name="items">the ammo box (and all its children ammo items)</param>
    /// <returns>the price of the ammo box</returns>
    protected double GetAmmoBoxPrice(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust all items contained inside an assort by a multiplier
    /// </summary>
    /// <param name="assort">(clone)Assort that contains items with prices to adjust</param>
    /// <param name="itemMultipler">multipler to use on items</param>
    /// <param name="presetMultiplier">preset multipler to use on presets</param>
    protected void AdjustAssortItemPricesByConfigMultiplier(
        TraderAssort assort,
        double itemMultipler,
        double presetMultiplier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Merge two trader assort files together
    /// </summary>
    /// <param name="firstAssort">assort 1#</param>
    /// <param name="secondAssort">assort #2</param>
    /// <returns>merged assort</returns>
    protected TraderAssort MergeAssorts(TraderAssort firstAssort, TraderAssort secondAssort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust assorts price by a modifier
    /// </summary>
    /// <param name="item">assort item details</param>
    /// <param name="assort">assort to be modified</param>
    /// <param name="modifier">value to multiply item price by</param>
    /// <param name="presetModifier">value to multiply preset price by</param>
    protected void AdjustItemPriceByModifier(
        Item item,
        TraderAssort assort,
        double modifier,
        double presetModifier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get fence assorts with no price adjustments based on fence rep
    /// </summary>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetRawFenceAssorts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does fence need to perform a partial refresh because its passed the refresh timer defined in trader.json
    /// </summary>
    /// <returns>true if it needs a partial refresh</returns>
    public bool NeedsPartialRefresh()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Replace a percentage of fence assorts with freshly generated items
    /// </summary>
    public void PerformPartialRefresh()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle the process of folding new assorts into existing assorts, when a new assort exists already, increment its StackObjectsCount instead
    /// </summary>
    /// <param name="newFenceAssorts">Assorts to fold into existing fence assorts</param>
    /// <param name="existingFenceAssorts">Current fence assorts new assorts will be added to</param>
    protected void UpdateFenceAssorts(
        CreateFenceAssortsResult newFenceAssorts,
        TraderAssort existingFenceAssorts
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Increment fence next refresh timestamp by current timestamp + partialRefreshTimeSeconds from config
    /// </summary>
    protected void IncrementPartialRefreshTime()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get values that will hydrate the passed in assorts back to the desired counts
    /// </summary>
    /// <param name="assortItems">Current assorts after items have been removed</param>
    /// <param name="generationValues">Base counts assorts should be adjusted to</param>
    /// <returns>GenerationAssortValues object with adjustments needed to reach desired state</returns>
    protected GenerationAssortValues GetItemCountsToGenerate(
        Item[] assortItems,
        GenerationAssortValues generationValues
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete desired number of items from assort (including children)
    /// </summary>
    /// <param name="itemCountToReplace"></param>
    /// <param name="assort"></param>
    protected void DeleteRandomAssorts(int itemCountToReplace, TraderAssort assort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose an item at random and remove it + mods from assorts
    /// </summary>
    /// <param name="assort">Trader assort to remove item from</param>
    /// <param name="rootItems">Pool of root items to pick from to remove</param>
    protected void RemoveRandomItemFromAssorts(TraderAssort assort, Item[] rootItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an integer rounded count of items to replace based on percentrage from traderConfig value
    /// </summary>
    /// <param name="totalItemCount">total item count</param>
    /// <returns>rounded int of items to replace</returns>
    protected int GetCountOfItemsToReplace(int totalItemCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the count of items fence offers
    /// </summary>
    /// <returns>int</returns>
    public int GetOfferCount()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create trader assorts for fence and store in fenceService cache
    /// Uses fence base cache generated on server start as a base
    /// </summary>
    public void GenerateFenceAssorts()
    {
        // TODO: actually implement
        return;
    }

    /// <summary>
    /// Convert the intermediary assort data generated into format client can process
    /// </summary>
    /// <param name="intermediaryAssorts">Generated assorts that will be converted</param>
    /// <returns>TraderAssort</returns>
    protected TraderAssort ConvertIntoFenceAssort(CreateFenceAssortsResult intermediaryAssorts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create object that contains calculated fence assort item values to make based on config
    /// Stored in this.DesiredAssortCounts
    /// </summary>
    protected void CreateInitialFenceAssortGenerationValues()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create skeleton to hold assort items
    /// </summary>
    /// <returns>TraderAssort object</returns>
    protected TraderAssort CreateFenceAssortSkeleton()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Hydrate assorts parameter object with generated assorts
    /// </summary>
    /// <param name="itemCounts">Number of assorts to generate</param>
    /// <param name="loyaltyLevel">Loyalty level to set new item to</param>
    /// <returns>CreateFenceAssortsResult</returns>
    protected CreateFenceAssortsResult CreateAssorts(GenerationAssortValues itemCounts, int loyaltyLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add item assorts to existing assort data
    /// </summary>
    /// <param name="assortCount">Number to add</param>
    /// <param name="assorts">Assorts data to add to</param>
    /// <param name="baseFenceAssortClone">Base data to draw from</param>
    /// <param name="itemTypeLimits"></param>
    /// <param name="loyaltyLevel">Loyalty level to set new item to</param>
    protected void AddItemAssorts(
        int assortCount,
        CreateFenceAssortsResult assorts,
        TraderAssort baseFenceAssortClone,
        Dictionary<string, (int current, int max)> itemTypeLimits,
        int loyaltyLevel
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find an assort item that matches the first parameter, also matches based on upd properties
    /// e.g. salewa hp resource units left
    /// </summary>
    /// <param name="rootItemBeingAdded">item to look for a match against</param>
    /// <param name="itemDbDetails">Db details of matching item</param>
    /// <param name="itemsWithChildren">Items to search through</param>
    /// <returns>Matching assort item</returns>
    protected virtual Item GetMatchingItem(
        Item rootItemBeingAdded,
        TemplateItem itemDbDetails,
        List<List<Item>> itemsWithChildren)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Should this item be forced into only 1 stack on fence
    /// </summary>
    /// <param name="existingItem">Existing item from fence assort</param>
    /// <param name="itemDbDetails">Item we want to add db details</param>
    /// <returns>True item should be force stacked</returns>
    protected virtual bool ItemShouldBeForceStacked(Item existingItem, TemplateItem itemDbDetails)
    {
        throw new NotImplementedException();
    }

    protected virtual bool ItemInPreventDupeCategoryList(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust price of item based on what is left to buy (resource/uses left)
    /// </summary>
    /// <param name="barterSchemes">All barter scheme for item having price adjusted</param>
    /// <param name="itemRoot">Root item having price adjusted</param>
    /// <param name="itemTemplate">Db template of item</param>
    protected virtual void AdjustItemPriceByQuality(
        Dictionary<string, List<List<BarterScheme>>> barterSchemes,
        Item itemRoot,
        TemplateItem itemTemplate)
    {
        throw new NotImplementedException();
    }

    protected virtual Dictionary<string, (int current, int max)> GetMatchingItemLimit(
        Dictionary<string, (int current, int max)> itemTypeLimits,
        string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find presets in base fence assort and add desired number to 'assorts' parameter
    /// </summary>
    /// <param name="desiredWeaponPresetsCount"></param>
    /// <param name="assorts">Assorts to add preset to</param>
    /// <param name="baseFenceAssort">Base data to draw from</param>
    /// <param name="loyaltyLevel">Which loyalty level is required to see/buy item</param>
    protected virtual void AddPresetsToAssort(
        int desiredWeaponPresetsCount,
        int desiredEquipmentPresetsCount,
        CreateFenceAssortsResult assorts,
        TraderAssort baseFenceAssort,
        int loyaltyLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust plate / soft insert durability values
    /// </summary>
    /// <param name="armor">Armor item array to add mods into</param>
    /// <param name="itemDbDetails">Armor items db template</param>
    protected virtual void RandomiseArmorModDurability(List<Item> armor, TemplateItem itemDbDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the durability values of items on armor with a passed in slot
    /// </summary>
    /// <param name="softInsertSlots">Slots of items to randomise</param>
    /// <param name="armorItemAndMods">Array of armor + inserts to get items from</param>
    protected virtual void RandomiseArmorSoftInsertDurabilities(List<Slot> softInsertSlots, List<Item> armorItemAndMods)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the durability values of plate items in armor
    /// Has chance to remove plate
    /// </summary>
    /// <param name="plateSlots">Slots of items to randomise</param>
    /// <param name="armorItemAndMods">Array of armor + inserts to get items from</param>
    protected virtual void RandomiseArmorInsertsDurabilities(List<Slot> plateSlots, List<Item> armorItemAndMods)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get stack size of a singular item (no mods)
    /// </summary>
    /// <param name="itemDbDetails">item being added to fence</param>
    /// <returns>Stack size</returns>
    protected virtual int GetSingleItemStackCount(TemplateItem itemDbDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove parts of a weapon prior to being listed on flea
    /// </summary>
    /// <param name="itemAndMods">Weapon to remove parts from</param>
    protected virtual void RemoveRandomModsOfItem(List<Item> itemAndMods)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Roll % chance check to see if item should be removed
    /// </summary>
    /// <param name="weaponMod">Weapon mod being checked</param>
    /// <param name="itemsBeingDeleted">Current list of items on weapon being deleted</param>
    /// <returns>True if item will be removed</returns>
    protected virtual bool PresetModItemWillBeRemoved(Item weaponMod, List<string> itemsBeingDeleted)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise items' upd properties e.g. med packs/weapons/armor
    /// </summary>
    /// <param name="itemDetails">Item being randomised</param>
    /// <param name="itemToAdjust">Item being edited</param>
    protected virtual void RandomiseItemUpdProperties(TemplateItem itemDetails, Item itemToAdjust)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate a randomised current and max durabiltiy value for an armor item
    /// </summary>
    /// <param name="itemDetails">Item to create values for</param>
    /// <param name="equipmentDurabilityLimits">Max durabiltiy percent min/max values</param>
    /// <returns>Durability + MaxDurability values</returns>
    protected virtual UpdRepairable GetRandomisedArmorDurabilityValues(
        TemplateItem itemDetails,
        ItemDurabilityCurrentMax equipmentDurabilityLimits)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Construct item limit record to hold max and current item count
    /// </summary>
    /// <param name="limits">limits as defined in config</param>
    /// <returns>record, key: item tplId, value: current/max item count allowed</returns>
    protected Dictionary<string, (int current, int max)> InitItemLimitCounter(Dictionary<string, int> limits)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the next update timestamp for fence
    /// </summary>
    /// <returns>future timestamp</returns>
    public int GetNextFenceUpdateTimestamp()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get fence refresh time in seconds
    /// </summary>
    /// <returns>Refresh time in seconds</returns>
    protected int GetFenceRefreshTime()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get fence level the passed in profile has
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>FenceLevel object</returns>
    public FenceLevel GetFenceInfo(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove or lower stack size of an assort from fence by id
    /// </summary>
    /// <param name="assortId">assort id to adjust</param>
    /// <param name="buyCount">Count of items bought</param>
    public void AmendOrRemoveFenceOffer(string assortId, int buyCount)
    {
        throw new NotImplementedException();
    }

    protected void DeleteOffer(string assortId, List<Item> assorts)
    {
        throw new NotImplementedException();
    }
}
