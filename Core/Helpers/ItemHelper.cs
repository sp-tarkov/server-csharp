using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class ItemHelper
{
    /**
 * Does the provided pool of items contain the desired item
 * @param itemPool Item collection to check
 * @param item Item to look for
 * @param slotId OPTIONAL - slotid of desired item
 * @returns True if pool contains item
 */
    public bool hasItemWithTpl(List<Item> itemPool, string item, string slotId = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Get the first item from provided pool with the desired tpl
     * @param itemPool Item collection to search
     * @param item Item to look for
     * @param slotId OPTIONAL - slotid of desired item
     * @returns Item or null
     */
    public Item getItemFromPoolByTpl(List<Item> itemPool, string item, string slotId = null)
    {
        throw new NotImplementedException();
    }

    /**
     * This method will compare two items (with all its children) and see if they are equivalent.
     * This method will NOT compare IDs on the items
     * @param item1 first item with all its children to compare
     * @param item2 second item with all its children to compare
     * @param compareUpdProperties Upd properties to compare between the items
     * @returns true if they are the same, false if they aren't
     */
    public bool isSameItems(List<Item> item1, List<Item> item2, HashSet<string> compareUpdProperties = null)
    {
        throw new NotImplementedException();
    }

    /**
     * This method will compare two items and see if they are equivalent.
     * This method will NOT compare IDs on the items
     * @param item1 first item to compare
     * @param item2 second item to compare
     * @param compareUpdProperties Upd properties to compare between the items
     * @returns true if they are the same, false if they aren't
     */
    public bool isSameItem(Item item1, Item item2, HashSet<string> compareUpdProperties = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Helper method to generate a Upd based on a template
     * @param itemTemplate the item template to generate a Upd for
     * @returns A Upd with all the default properties set
     */
    public Upd generateUpdForItem(TemplateItem itemTemplate)
    {
        throw new NotImplementedException();
    }

    /**
     * Checks if a tpl is a valid item. Valid meaning that it's an item that can be stored in stash
     * Valid means:
     * Not quest item
     * 'Item' type
     * Not on the invalid base types array
     * Price above 0 roubles
     * Not on item config blacklist
     * @param tpl the template id / tpl
     * @returns boolean; true for items that may be in player possession and not quest items
     */
    public bool isValidItem(string tpl, List<string> invalidBaseTypes = null)
    {
        throw new NotImplementedException();
    }

    // Check if the tpl / template Id provided is a descendent of the baseclass
    //
    // @param   string    tpl             the item template id to check
    // @param   string    baseClassTpl    the baseclass to check for
    // @return  bool                    is the tpl a descendent?
    public bool OfBaseclass(string tpl, string baseClassTpl)
    {
        throw new NotImplementedException();
    }

    // Check if item has any of the supplied base classes
    // @param string tpl Item to check base classes of
    // @param string[] baseClassTpls base classes to check for
    // @returns true if any supplied base classes match
    public bool OfBaseclasses(string tpl, List<string> baseClassTpls)
    {
        throw new NotImplementedException();
    }

    // Does the provided item have the chance to require soft armor inserts
    // Only applies to helmets/vest/armors.
    // Not all head gear needs them
    // @param string itemTpl item to check
    // @returns Does item have the possibility ot need soft inserts
    public bool ArmorItemCanHoldMods(string itemTpl)
    {
        throw new NotImplementedException();
    }

    // Does the provided item tpl need soft/removable inserts to function
    // @param string itemTpl Armor item
    // @returns True if item needs some kind of insert
    public bool ArmorItemHasRemovableOrSoftInsertSlots(string itemTpl)
    {
        throw new NotImplementedException();
    }

    // Does the pased in tpl have ability to hold removable plate items
    // @param string itemTpl item tpl to check for plate support
    // @returns True when armor can hold plates
    public bool ArmorItemHasRemovablePlateSlots(string itemTpl)
    {
        throw new NotImplementedException();
    }

    // Does the provided item tpl require soft inserts to become a valid armor item
    // @param string itemTpl Item tpl to check
    // @returns True if it needs armor inserts
    public bool ItemRequiresSoftInserts(string itemTpl)
    {
        throw new NotImplementedException();
    }

    // Get all soft insert slot ids
    // @returns A List of soft insert ids (e.g. soft_armor_back, helmet_top)
    public List<string> GetSoftInsertSlotIds()
    {
        throw new NotImplementedException();
    }

    // Returns the items total price based on the handbook or as a fallback from the prices.json if the item is not
    // found in the handbook. If the price can't be found at all return 0
    // @param List<string> tpls item tpls to look up the price of
    // @returns Total price in roubles
    public decimal GetItemAndChildrenPrice(List<string> tpls)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
    /// found in the handbook. If the price can't be found at all return 0
    /// </summary>
    /// <param name="tpl">Item to look price up of</param>
    /// <returns>Price in roubles</returns>
    public decimal GetItemPrice(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
    /// found in the handbook. If the price can't be found at all return 0
    /// </summary>
    /// <param name="tpl">Item to look price up of</param>
    /// <returns>Price in roubles</returns>
    public decimal GetItemMaxPrice(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the static (handbook) price in roubles for an item by tpl
    /// </summary>
    /// <param name="tpl">Items tpl id to look up price</param>
    /// <returns>Price in roubles (0 if not found)</returns>
    public decimal GetStaticItemPrice(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the dynamic (flea) price in roubles for an item by tpl
    /// </summary>
    /// <param name="tpl">Items tpl id to look up price</param>
    /// <returns>Price in roubles (undefined if not found)</returns>
    public decimal GetDynamicItemPrice(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update items upd.StackObjectsCount to be 1 if its upd is missing or StackObjectsCount is undefined
    /// </summary>
    /// <param name="item">Item to update</param>
    /// <returns>Fixed item</returns>
    public Item FixItemStackCount(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get cloned copy of all item data from items.json
    /// </summary>
    /// <returns>List of TemplateItem objects</returns>
    public List<TemplateItem> GetItems()
    {
        throw new NotImplementedException();
    }

    /**
 * Gets item data from items.json
 * @param tpl items template id to look up
 * @returns bool - is valid + template item object as array
 */
    public (bool, Dictionary<string, TemplateItem>) GetItem(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Checks if the item has slots
     * @param itemTpl Template id of the item to check
     * @returns true if the item has slots
     */
    public bool ItemHasSlots(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Checks if the item is in the database
     * @param tpl Template id of the item to check
     * @returns true if the item is in the database
     */
    public bool IsItemInDb(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Calculate the average quality of an item and its children
     * @param items An offers item to process
     * @param skipArmorItemsWithoutDurability Skip over armor items without durability
     * @returns % quality modifier between 0 and 1
     */
    public double GetItemQualityModifierForItems(List<Item> items, bool? skipArmorItemsWithoutDurability = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Get normalized value (0-1) based on item condition
     * Will return -1 for base armor items with 0 durability
     * @param item Item to check
     * @param skipArmorItemsWithoutDurability return -1 for armor items that have max durability of 0
     * @returns Number between 0 and 1
     */
    public double GetItemQualityModifier(Item item, bool? skipArmorItemsWithoutDurability = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a quality value based on a repairable item's current state between current and max durability
     * @param itemDetails Db details for item we want quality value for
     * @param repairable Repairable properties
     * @param item Item quality value is for
     * @returns A number between 0 and 1
     */
    protected double GetRepairableItemQualityValue(
        Dictionary<string, TemplateItem> itemDetails,
        UpdRepairable repairable,
        Item item
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Recursive function that looks at every item from parameter and gets their children's Ids + includes parent item in results
     * @param items List of items (item + possible children)
     * @param baseItemId Parent item's id
     * @returns a list of strings
     */
    public List<string> FindAndReturnChildrenByItems(List<Item> items, string baseItemId)
    {
        throw new NotImplementedException();
    }

    /**
     * A variant of FindAndReturnChildren where the output is list of item objects instead of their ids.
     * @param items List of items (item + possible children)
     * @param baseItemId Parent item's id
     * @param modsOnly Include only mod items, exclude items stored inside root item
     * @returns A list of Item objects
     */
    public List<Item> FindAndReturnChildrenAsItems(List<Item> items, string baseItemId, bool modsOnly = false)
    {
        throw new NotImplementedException();
    }

    /**
     * Find children of the item in a given assort (weapons parts for example, need recursive loop function)
     * @param itemIdToFind Template id of item to check for
     * @param assort List of items to check in
     * @returns List of children of requested item
     */
    public List<Item> FindAndReturnChildrenByAssort(string itemIdToFind, List<Item> assort)
    {
        throw new NotImplementedException();
    }

    /**
     * Check if the passed in item has buy count restrictions
     * @param itemToCheck Item to check
     * @returns true if it has buy restrictions
     */
    public bool HasBuyRestrictions(Item itemToCheck)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the passed template id is a dog tag.
    /// </summary>
    /// <param name="tpl">Template id to check.</param>
    /// <returns>True if it is a dogtag.</returns>
    public bool IsDogtag(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the identifier for a child using slotId, locationX and locationY.
    /// </summary>
    /// <param name="item">Item.</param>
    /// <returns>SlotId OR slotid, locationX, locationY.</returns>
    public string GetChildId(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the passed item can be stacked.
    /// </summary>
    /// <param name="tpl">Item to check.</param>
    /// <returns>True if it can be stacked.</returns>
    public bool IsItemTplStackable(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Splits the item stack if it exceeds its items StackMaxSize property into child items of the passed parent.
    /// </summary>
    /// <param name="itemToSplit">Item to split into smaller stacks.</param>
    /// <returns>List of root item + children.</returns>
    public List<Item> SplitStack(Item itemToSplit)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Turns items like money into separate stacks that adhere to max stack size.
    /// </summary>
    /// <param name="itemToSplit">Item to split into smaller stacks.</param>
    /// <returns>List of separate item stacks.</returns>
    public List<List<Item>> SplitStackIntoSeparateItems(Item itemToSplit)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Finds Barter items from a list of items.
    /// </summary>
    /// <param name="by">Tpl or id.</param>
    /// <param name="itemsToSearch">Array of items to iterate over.</param>
    /// <param name="desiredBarterItemIds">Desired barter item ids.</param>
    /// <returns>List of Item objects.</returns>
    public List<Item> FindBarterItems(string by, List<Item> itemsToSearch, string desiredBarterItemIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Replaces the _id value for the base item + all children that are children of it.
    /// REPARENTS ROOT ITEM ID, NOTHING ELSE.
    /// </summary>
    /// <param name="itemWithChildren">Item with mods to update.</param>
    /// <param name="newId">New id to add on children of base item.</param>
    public void ReplaceRootItemID(List<Item> itemWithChildren, string newId = "")
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Regenerate all GUIDs with new IDs, for the exception of special item types (e.g. quest, sorting table, etc.) This
    /// function will not mutate the original items list, but will return a new list with new GUIDs.
    /// </summary>
    /// <param name="originalItems">Items to adjust the IDs of</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="insuredItems">Insured items that should not have their IDs replaced</param>
    /// <param name="fastPanel">Quick slot panel</param>
    /// <returns>List<Item></returns>
    public List<Item> ReplaceIDs(
        List<Item> originalItems,
        PmcData pmcData = null,
        List<InsuredItem> insuredItems = null,
        object fastPanel = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Mark the passed in list of items as found in raid.
    /// Modifies passed in items
    /// </summary>
    /// <param name="items">The list of items to mark as FiR</param>
    public void SetFoundInRaid(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// WARNING, SLOW. Recursively loop down through an items hierarchy to see if any of the ids match the supplied list, return true if any do
    /// </summary>
    /// <param name="tpl">Items tpl to check parents of</param>
    /// <param name="tplsToCheck">Tpl values to check if parents of item match</param>
    /// <returns>bool Match found</returns>
    public bool DoesItemOrParentsIdMatch(string tpl, List<string> tplsToCheck)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item is quest item
    /// </summary>
    /// <param name="tpl">Items tpl to check quest status of</param>
    /// <returns>true if item is flagged as quest item</returns>
    public bool IsQuestItem(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks to see if the item is *actually* moddable in-raid. Checks include the items existence in the database, the
    /// parent items existence in the database, the existence (and value) of the items RaidModdable property, and that
    /// the parents slot-required property exists, matches that of the item, and its value.
    /// </summary>
    /// <param name="item">The item to be checked</param>
    /// <param name="parent">The parent of the item to be checked</param>
    /// <returns>True if the item is actually moddable, false if it is not, and null if the check cannot be performed.</returns>
    public bool? IsRaidModdable(Item item, Item parent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves the main parent item for a given attachment item.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item for which to find the main parent.</param>
    /// <param name="itemsMap">A Dictionary containing item IDs mapped to their corresponding Item objects for quick lookup.</param>
    /// <returns>The Item object representing the top-most parent of the given item, or null if no such parent exists.</returns>
    public Item GetAttachmentMainParent(string itemId, Dictionary<string, Item> itemsMap)
    {
        throw new NotImplementedException();
    }

    /**
 * Determines if an item is an attachment that is currently attached to its parent item.
 *
 * @param item The item to check.
 * @returns true if the item is attached attachment, otherwise false.
 */
    public bool IsAttachmentAttached(Item item)
    {
        throw new NotImplementedException();
    }

    /**
     * Retrieves the equipment parent item for a given item.
     *
     * This method traverses up the hierarchy of items starting from a given `itemId`, until it finds the equipment
     * parent item. In other words, if you pass it an item id of a suppressor, it will traverse up the muzzle brake,
     * barrel, upper receiver, gun, nested backpack, and finally return the backpack Item that is equipped.
     *
     * It's important to note that traversal is expensive, so this method requires that you pass it a Dictionary of the items
     * to traverse, where the keys are the item IDs and the values are the corresponding Item objects. This alleviates
     * some of the performance concerns, as it allows for quick lookups of items by ID.
     *
     * @param itemId - The unique identifier of the item for which to find the equipment parent.
     * @param itemsMap - A Dictionary containing item IDs mapped to their corresponding Item objects for quick lookup.
     * @returns The Item object representing the equipment parent of the given item, or `null` if no such parent exists.
     */
    public Item GetEquipmentParent(string itemId, Dictionary<string, Item> itemsMap)
    {
        throw new NotImplementedException();
    }

    /**
     * Get the inventory size of an item
     * @param items Item with children
     * @param rootItemId
     * @returns ItemSize object (width and height)
     */
    public ItemSize GetItemSize(List<Item> items, string rootItemId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a random cartridge from an items Filter property
     * @param item Db item template to look up Cartridge filter values from
     * @returns Caliber of cartridge
     */
    public string GetRandomCompatibleCaliberTemplateId(TemplateItem item)
    {
        throw new NotImplementedException();
    }

    /**
     * Add cartridges to the ammo box with correct max stack sizes
     * @param ammoBox Box to add cartridges to
     * @param ammoBoxDetails Item template from items db
     */
    public void AddCartridgesToAmmoBox(List<Item> ammoBox, TemplateItem ammoBoxDetails)
    {
        throw new NotImplementedException();
    }

    /**
     * Add a single stack of cartridges to the ammo box
     * @param ammoBox Box to add cartridges to
     * @param ammoBoxDetails Item template from items db
     */
    public void AddSingleStackCartridgesToAmmoBox(List<Item> ammoBox, TemplateItem ammoBoxDetails)
    {
        throw new NotImplementedException();
    }

    /**
     * Check if item is stored inside of a container
     * @param itemToCheck Item to check is inside of container
     * @param desiredContainerSlotId Name of slot to check item is in e.g. SecuredContainer/Backpack
     * @param items Inventory with child parent items to check
     * @returns True when item is in container
     */
    public bool ItemIsInsideContainer(Item itemToCheck, string desiredContainerSlotId, List<Item> items)
    {
        throw new NotImplementedException();
    }

    /**
     * Add child items (cartridges) to a magazine
     * @param magazine Magazine to add child items to
     * @param magTemplate Db template of magazine
     * @param staticAmmoDist Cartridge distribution
     * @param caliber Caliber of cartridge to add to magazine
     * @param minSizePercent % the magazine must be filled to
     * @param defaultCartridgeTpl Cartridge to use when none found
     * @param weapon Weapon the magazine will be used for (if passed in uses Chamber as whitelist)
     */
    public void FillMagazineWithRandomCartridge(
        List<Item> magazine,
        TemplateItem magTemplate,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string caliber = null,
        double minSizePercent = 0.25,
        string defaultCartridgeTpl = null,
        TemplateItem weapon = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add child items to a magazine of a specific cartridge
    /// </summary>
    /// <param name="magazineWithChildCartridges">Magazine to add child items to</param>
    /// <param name="magTemplate">Db template of magazine</param>
    /// <param name="cartridgeTpl">Cartridge to add to magazine</param>
    /// <param name="minSizeMultiplier">% the magazine must be filled to</param>
    public void FillMagazineWithCartridge(
        List<Item> magazineWithChildCartridges,
        TemplateItem magTemplate,
        string cartridgeTpl,
        double minSizeMultiplier = 0.25
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a random bullet type from the list of possible a magazine has
    /// </summary>
    /// <param name="magTemplate">Magazine template from Db</param>
    /// <returns>Tpl of cartridge</returns>
    protected string GetRandomValidCaliber(TemplateItem magTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Chose a randomly weighted cartridge that fits
    /// </summary>
    /// <param name="caliber">Desired caliber</param>
    /// <param name="staticAmmoDist">Cartridges and their weights</param>
    /// <param name="fallbackCartridgeTpl">If a cartridge cannot be found in the above staticAmmoDist param, use this instead</param>
    /// <param name="cartridgeWhitelist">OPTIONAL whitelist for cartridges</param>
    /// <returns>Tpl of cartridge</returns>
    protected string? DrawAmmoTpl(
        string caliber,
        Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string fallbackCartridgeTpl,
        List<string>? cartridgeWhitelist = null
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a basic cartridge object
    /// </summary>
    /// <param name="parentId">container cartridges will be placed in</param>
    /// <param name="ammoTpl">Cartridge to insert</param>
    /// <param name="stackCount">Count of cartridges inside parent</param>
    /// <param name="location">Location inside parent (e.g. 0, 1)</param>
    /// <param name="foundInRaid">OPTIONAL - Are cartridges found in raid (SpawnedInSession)</param>
    /// <returns>Item</returns>
    public Item CreateCartridges(
        string parentId,
        string ammoTpl,
        int stackCount,
        int location,
        bool foundInRaid = false
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the size of a stack, return 1 if no stack object count property found
    /// </summary>
    /// <param name="item">Item to get stack size of</param>
    /// <returns>size of stack</returns>
    public int GetItemStackSize(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the name of an item from the locale file using the item tpl
    /// </summary>
    /// <param name="itemTpl">Tpl of item to get name of</param>
    /// <returns>Full name, short name if not found</returns>
    public string GetItemName(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all item tpls with a desired base type
    /// </summary>
    /// <param name="desiredBaseType">Item base type wanted</param>
    /// <returns>Array of tpls</returns>
    public List<string> GetItemTplsOfBaseType(string desiredBaseType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add child slot items to an item, chooses random child item if multiple choices exist
    /// </summary>
    /// <param name="itemToAdd">array with single object (root item)</param>
    /// <param name="itemToAddTemplate">Db template for root item</param>
    /// <param name="modSpawnChanceDict">Optional dictionary of mod name + % chance mod will be included in item (e.g. front_plate: 100)</param>
    /// <param name="requiredOnly">Only add required mods</param>
    /// <returns>Item with children</returns>
    public List<Item> AddChildSlotItems(
        List<Item> itemToAdd,
        TemplateItem itemToAddTemplate,
        Dictionary<string, double>? modSpawnChanceDict = null,
        bool requiredOnly = false
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a compatible tpl from the array provided where it is not found in the provided incompatible mod tpls parameter
    /// </summary>
    /// <param name="possibleTpls">Tpls to randomly choose from</param>
    /// <param name="incompatibleModTpls">Incompatible tpls to not allow</param>
    /// <returns>Chosen tpl or undefined</returns>
    public string? GetCompatibleTplFromArray(List<string> possibleTpls, HashSet<string> incompatibleModTpls)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided item._props.Slots._name property a plate slot
    /// </summary>
    /// <param name="slotName">Name of slot (_name) of Items Slot array</param>
    /// <returns>True if its a slot that holds a removable plate</returns>
    public bool IsRemovablePlateSlot(string slotName)
    {
        throw new NotImplementedException();
    }

    // Get a list of slot names that hold removable plates
// Returns Array of slot ids (e.g. front_plate)
    public List<string> GetRemovablePlateSlotIds()
    {
        throw new NotImplementedException();
    }

// Generate new unique ids for child items while preserving hierarchy
// Base/primary item
// Primary item + children of primary item
// Returns Item array with updated IDs
    public List<Item> ReparentItemAndChildren(Item rootItem, List<Item> itemWithChildren)
    {
        throw new NotImplementedException();
    }

// Update a root items _id property value to be unique
// Item to update root items _id property
// Optional: new id to use
// Returns New root id
    public string RemapRootItemId(List<Item> itemWithChildren, string newId) // TODO: string newId = this.hashUtil.Generate()
    {
        throw new NotImplementedException();
    }

// Adopts orphaned items by resetting them as root "hideout" items. Helpful in situations where a parent has been
// deleted from a group of items and there are children still referencing the missing parent. This method will
// remove the reference from the children to the parent and set item properties to root values.
//
// The ID of the "root" of the container.
// Array of Items that should be adjusted.
// Returns Array of Items that have been adopted.
    public List<Item> AdoptOrphanedItems(string rootId, List<Item> items)
    {
        throw new NotImplementedException();
    }

// Populate a Map object of items for quick lookup using their ID.
//
// An array of Items that should be added to a Map.
// Returns A Map where the keys are the item IDs and the values are the corresponding Item objects.
    public Dictionary<string, Item> GenerateItemsMap(List<Item> items)
    {
        throw new NotImplementedException();
    }

// Add a blank upd object to passed in item if it does not exist already
// item to add upd to
// text to write to log when upd object was not found
// Returns True when upd object was added
    public bool AddUpdObjectToItem(Item item, string warningMessageWhenMissing = null)
    {
        throw new NotImplementedException();
    }

// Return all tpls from Money enum
// Returns string tpls
    public List<string> GetMoneyTpls()
    {
        throw new NotImplementedException();
    }

// Get a randomised stack size for the passed in ammo
// Ammo to get stack size for
// Default: Limit to 60 to prevent crazy values when players use stack increase mods
// Returns number
    public int GetRandomisedAmmoStackSize(TemplateItem ammoItemTemplate, int maxLimit = 60)
    {
        throw new NotImplementedException();
    }

    public void GetItemBaseType(string tpl, bool rootOnly = true)
    {
        throw new NotImplementedException();
    }

// Remove FiR status from passed in items
// Items to update FiR status of
    public void RemoveSpawnedInSessionPropertyFromItems(List<Item> items)
    {
        throw new NotImplementedException();
    }
}

public class ItemSize
{
    [JsonPropertyName("width")]
    public double Width { get; set; }
    
    [JsonPropertyName("height")]
    public double Height { get; set; }
}
