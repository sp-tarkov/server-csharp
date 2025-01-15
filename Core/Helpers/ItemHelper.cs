using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Helpers;

[Injectable]
public class ItemHelper
{
    protected ISptLogger<ItemHelper> _logger;
    protected HashUtil _hashUtil;
    protected JsonUtil _jsonUtil;
    protected RandomUtil _randomUtil;
    protected MathUtil _mathUtil;
    protected DatabaseService _databaseService;
    protected HandbookHelper _handbookHelper;
    protected ItemBaseClassService _itemBaseClassService;
    protected ItemFilterService _itemFilterService;
    protected LocalisationService _localisationService;
    protected LocaleService _localeService;
    protected CompareUtil _compareUtil;
    protected ICloner _cloner;

    protected List<string> _defaultInvalidBaseTypes =
    [
        BaseClasses.LOOT_CONTAINER,
        BaseClasses.MOB_CONTAINER,
        BaseClasses.STASH,
        BaseClasses.SORTING_TABLE,
        BaseClasses.INVENTORY,
        BaseClasses.STATIONARY_CONTAINER,
        BaseClasses.POCKETS
    ];

    public ItemHelper
    (
        ISptLogger<ItemHelper> logger,
        HashUtil hashUtil,
        JsonUtil jsonUtil,
        RandomUtil randomUtil,
        MathUtil mathUtil,
        DatabaseService databaseService,
        HandbookHelper handbookHelper,
        ItemBaseClassService itemBaseClassService,
        ItemFilterService itemFilterService,
        LocalisationService localisationService,
        LocaleService localeService,
        CompareUtil compareUtil,
        ICloner cloner
    )
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _jsonUtil = jsonUtil;
        _randomUtil = randomUtil;
        _mathUtil = mathUtil;
        _databaseService = databaseService;
        _handbookHelper = handbookHelper;
        _itemBaseClassService = itemBaseClassService;
        _itemFilterService = itemFilterService;
        _localisationService = localisationService;
        _localeService = localeService;
        _compareUtil = compareUtil;
        _cloner = cloner;
    }

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
        return IsOfBaseclasses(itemTpl, [BaseClasses.HEADWEAR, BaseClasses.VEST, BaseClasses.ARMOR]);
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
        return _cloner.Clone(_databaseService.GetItems().Values).ToList();
    }

    /**
     * Gets item data from items.json
     * @param tpl items template id to look up
     * @returns bool - is valid + template item object as array
     */
    public KeyValuePair<bool, TemplateItem?> GetItem(string tpl)
    {
        // -> Gets item from <input: _tpl>
        if (_databaseService.GetItems().Keys.Contains(tpl))
        {
            return new(true, _databaseService.GetItems()[tpl]);
        }

        return new(false, null);
    }

    /**
     * Checks if the item has slots
     * @param itemTpl Template id of the item to check
     * @returns true if the item has slots
     */
    public bool ItemHasSlots(string itemTpl)
    {
        return GetItem(itemTpl).Value.Properties.Slots?.Count() > 0;
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
        List<string> list = [];

        foreach (var childitem in items)
        {
            if (childitem.ParentId == baseItemId)
            {
                list.AddRange(FindAndReturnChildrenByItems(items, childitem.Id));
            }
        }

        list.Add(baseItemId); // Required, push original item id onto array

        return list;
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
        List<Item> list = [];
        foreach (var childItem in items)
        {
            // Include itself
            if (childItem.Id == baseItemId)
            {
                list.Insert(0, childItem);
                continue;
            }

            // Is stored in parent and disallowed
            if (modsOnly && childItem.Location is not null)
            {
                continue;
            }

            // Items parentid matches root item AND returned items doesnt contain current child
            if (childItem.ParentId == baseItemId && !list.Any((item) => childItem.Id == item.Id))
            {
                list.AddRange(FindAndReturnChildrenAsItems(items, childItem.Id));
            }
        }

        return list;
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
        if (item.Location is null)
        {
            return item.SlotId;
        }
        
        var LocationTyped = (ItemLocation)item.Location;

        return $"{item.SlotId},{LocationTyped.X},{LocationTyped.Y}";
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
        if (itemToSplit?.Upd?.StackObjectsCount is null)
        {
            return [itemToSplit];
        }

        var maxStackSize = GetItem(itemToSplit.Template).Value.Properties.StackMaxSize;
        var remainingCount = itemToSplit.Upd.StackObjectsCount;
        List<Item> rootAndChildren = [];

        // If the current count is already equal or less than the max
        // return the item as is.
        if (remainingCount <= maxStackSize)
        {
            rootAndChildren.Add(_cloner.Clone(itemToSplit));

            return rootAndChildren;
        }

        while (remainingCount.Value != 0)
        {
            var amount = Math.Min(remainingCount ?? 0, maxStackSize ?? 0);
            var newStackClone = _cloner.Clone(itemToSplit);

            newStackClone.Id = _hashUtil.Generate();
            newStackClone.Upd.StackObjectsCount = amount;
            remainingCount -= amount;
            rootAndChildren.Add(newStackClone);
        }

        return rootAndChildren;
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
    public List<Item> ReplaceIDs(List<Item> originalItems, PmcData pmcData = null, List<InsuredItem> insuredItems = null,
        Dictionary<string, string> fastPanel = null)
    {
        var items = _cloner.Clone(originalItems);
        var serialisedInventory = _jsonUtil.Serialize(items);
        var hideoutAreaStashes = pmcData?.Inventory?.HideoutAreaStashes ?? new();

        foreach (var item in items)
        {
            if (pmcData != null)
            {
                // Insured items should not be renamed. Only works for PMCs.
                if (insuredItems?.FirstOrDefault(i => i.ItemId == item.Id) != null)
                    continue;

                // Do not replace the IDs of specific types of items.
                if (item.Id == pmcData?.Inventory?.Equipment ||
                    item.Id == pmcData?.Inventory?.QuestRaidItems ||
                    item.Id == pmcData?.Inventory?.QuestStashItems ||
                    item.Id == pmcData?.Inventory?.SortingTable ||
                    item.Id == pmcData?.Inventory?.Stash ||
                    item.Id == pmcData?.Inventory?.HideoutCustomizationStashId ||
                    (hideoutAreaStashes?.ContainsKey(item.Id) ?? false))
                {
                    continue;
                }
            }

            // Replace the ID of the item in the serialised inventory using a regular expression.
            var oldId = item.Id;
            var newId = _hashUtil.Generate();
            serialisedInventory = serialisedInventory.Replace(oldId, newId); // Node uses regex with "g" flag to replace all instances

            // Also replace in quick slot if the old ID exists.
            if (fastPanel != null)
            {
                foreach (var itemSlot in fastPanel)
                {
                    if (fastPanel[itemSlot.Key] == oldId)
                        fastPanel[itemSlot.Key] = fastPanel[itemSlot.Key].Replace(oldId, newId); // Node uses regex with "g" flag to replace all instances
                }
            }
        }

        items = _jsonUtil.Deserialize<List<Item>>(serialisedInventory);

        // fix dupe id's
        var dupes = new Dictionary<string, double?>();
        var newParents = new Dictionary<string, List<Item>>();
        var childrenMapping = new Dictionary<string, Dictionary<string, double?>>();
        var oldToNewIds = new Dictionary<string, List<string>>();

        // Finding duplicate IDs involves scanning the item three times.
        // First scan - Check which ids are duplicated.
        // Second scan - Map parents to items.
        // Third scan - Resolve IDs.
        foreach (var item in items)
        {
            if (!dupes.TryAdd(item.Id, 0))
            {
                dupes[item.Id] += 1;
            }
        }

        foreach (var item in items)
        {
            if (!(dupes[item.Id] > 1))
            {
                continue;
            }

            var newId = _hashUtil.Generate();
            if (!newParents.ContainsKey(item.ParentId))
            {
                newParents.Add(item.ParentId, []);
            }

            var newParentsItems = newParents.GetValueOrDefault(item.ParentId);
            newParentsItems.Add(item);

            if (!oldToNewIds.ContainsKey(item.Id))
            {
                oldToNewIds.Add(item.Id, []);
            }

            var oldToNewIdsItems = oldToNewIds.GetValueOrDefault(item.Id);
            oldToNewIdsItems.Add(newId);
        }

        foreach (var item in items)
        {
            if (dupes[item.Id] > 1)
            {
                var oldId = item.Id;
                var newId = oldToNewIds[oldId][0];
                oldToNewIds[oldId].RemoveAt(0);
                item.Id = newId;

                // Extract one of the children that's also duplicated.
                if (newParents.ContainsKey(oldId) && newParents[oldId].Count > 0)
                {
                    childrenMapping[newId] = new();
                    for (int i = 0; i < newParents[oldId].Count; i++)
                    {
                        // Make sure we haven't already assigned another duplicate child of
                        // same slot and location to this parent.
                        var childId = GetChildId(newParents[oldId][i]);

                        if (!childrenMapping.ContainsKey(childId))
                        {
                            childrenMapping[newId][childId] = 1;
                            newParents[oldId][i].ParentId = newId;
                            // Some very fucking sketchy stuff on this childIndex
                            // No clue wth was that childIndex supposed to be, but its not
                            newParents[oldId].RemoveAt(i);
                        }
                    }
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Mark the passed in list of items as found in raid.
    /// Modifies passed in items
    /// </summary>
    /// <param name="items">The list of items to mark as FiR</param>
    public void SetFoundInRaid(List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.Upd == null)
                item.Upd = new();

            item.Upd.SpawnedInSession = true;
        }
    }

    /// <summary>
    /// WARNING, SLOW. Recursively loop down through an items hierarchy to see if any of the ids match the supplied list, return true if any do
    /// </summary>
    /// <param name="tpl">Items tpl to check parents of</param>
    /// <param name="tplsToCheck">Tpl values to check if parents of item match</param>
    /// <returns>bool Match found</returns>
    public bool DoesItemOrParentsIdMatch(string tpl, List<string> tplsToCheck)
    {
        var itemDetails = GetItem(tpl);
        var itemExists = itemDetails.Key;
        var item = itemDetails.Value;

        // not an item, drop out
        if (!itemExists)
            return false;

        // no parent to check
        if (item.Parent == null)
            return false;

        // Does templateId match any values in tplsToCheck array
        if (tplsToCheck.Contains(item.Id))
            return true;

        // check items parent with same method
        if (tplsToCheck.Contains(item.Parent))
            return true;

        return DoesItemOrParentsIdMatch(item.Parent, tplsToCheck);
    }

    /// <summary>
    /// Check if item is quest item
    /// </summary>
    /// <param name="tpl">Items tpl to check quest status of</param>
    /// <returns>true if item is flagged as quest item</returns>
    public bool IsQuestItem(string tpl)
    {
        var itemDetails = GetItem(tpl);
        if (itemDetails.Key && itemDetails.Value.Properties.QuestItem != null)
            return true;

        return false;
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
        // This check requires the item to have the slotId property populated.
        if (item.SlotId == null)
            return null;

        var itemTemplate = GetItem(item.Template);
        var parentTemplate = GetItem(parent.Template);

        // Check for RaidModdable property on the item template.
        var isNotRaidModdable = false;
        if (itemTemplate.Key)
            isNotRaidModdable = itemTemplate.Value?.Properties?.RaidModdable == false;

        // Check to see if the slot that the item is attached to is marked as required in the parent item's template.
        var isRequiredSlot = false;
        if (parentTemplate.Key && parentTemplate.Value?.Properties?.Slots != null)
            isRequiredSlot = parentTemplate.Value?.Properties?.Slots?.Any(
                                 slot =>
                                     slot?.Name == item?.SlotId &&
                                     (slot?.Required ?? false)
                             ) ??
                             false;

        return itemTemplate.Key && parentTemplate.Key && (isNotRaidModdable || isRequiredSlot);
    }

    /// <summary>
    /// Retrieves the main parent item for a given attachment item.
    ///
    /// This method traverses up the hierarchy of items starting from a given `itemId`, until it finds the main parent
    /// item that is not an attached attachment itself. In other words, if you pass it an item id of a suppressor, it
    /// will traverse up the muzzle brake, barrel, upper receiver, and return the gun that the suppressor is ultimately
    /// attached to, even if that gun is located within multiple containers.
    ///
    /// It's important to note that traversal is expensive, so this method requires that you pass it a Map of the items
    /// to traverse, where the keys are the item IDs and the values are the corresponding Item objects. This alleviates
    /// some of the performance concerns, as it allows for quick lookups of items by ID.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item for which to find the main parent.</param>
    /// <param name="itemsMap">A Dictionary containing item IDs mapped to their corresponding Item objects for quick lookup.</param>
    /// <returns>The Item object representing the top-most parent of the given item, or null if no such parent exists.</returns>
    public Item GetAttachmentMainParent(string itemId, Dictionary<string, Item> itemsMap)
    {
        var currentItem = itemsMap.FirstOrDefault(x => x.Key == itemId).Value;

        while (currentItem != null && IsAttachmentAttached(currentItem))
        {
            currentItem = itemsMap.FirstOrDefault(x => x.Key == currentItem.ParentId).Value;
            if (currentItem == null)
                return null;
        }

        return currentItem;
    }

    /**
     * Determines if an item is an attachment that is currently attached to its parent item.
     *
     * @param item The item to check.
     * @returns true if the item is attached attachment, otherwise false.
     */
    public bool IsAttachmentAttached(Item item)
    {
        // TODO: actually implement
        return true;
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
        var ammoBoxMaxCartridgeCount = ammoBoxDetails.Properties.StackSlots[0].MaxCount;
        var cartridgeTpl = ammoBoxDetails.Properties.StackSlots[0].Props.Filters[0].Filter[0];
        var cartridgeDetails = GetItem(cartridgeTpl);
        var cartridgeMaxStackSize = cartridgeDetails.Value.Properties.StackMaxSize;

        // Exit if ammo already exists in box
        if (ammoBox.Any((item) => item.Template == cartridgeTpl))
        {
            return;
        }

        // Add new stack-size-correct items to ammo box
        double? currentStoredCartridgeCount = 0;
        var maxPerStack = Math.Min(ammoBoxMaxCartridgeCount ?? 0, cartridgeMaxStackSize ?? 0);
        // Find location based on Max ammo box size
        var location = Math.Ceiling(ammoBoxMaxCartridgeCount / maxPerStack ?? 0) - 1;

        while (currentStoredCartridgeCount < ammoBoxMaxCartridgeCount)
        {
            var remainingSpace = ammoBoxMaxCartridgeCount - currentStoredCartridgeCount;
            var cartridgeCountToAdd = remainingSpace < maxPerStack ? remainingSpace : maxPerStack;

            // Add cartridge item into items array
            var cartridgeItemToAdd = CreateCartridges(
                ammoBox[0].Id,
                cartridgeTpl,
                cartridgeCountToAdd ?? 0,
                location,
                ammoBox[0].Upd?.SpawnedInSession ?? false
            );

            // In live no ammo box has the first cartridge item with a location
            if (location == 0)
            {
                cartridgeItemToAdd.Location = null;
            }

            ammoBox.Add(cartridgeItemToAdd);

            currentStoredCartridgeCount += cartridgeCountToAdd;
            location--;
        }
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
        // Get cartridge properties and max allowed stack size
        var cartridgeDetails = GetItem(cartridgeTpl);
        if (!cartridgeDetails.Key)
        {
            _logger.Error(_localisationService.GetText("item-invalid_tpl_item", cartridgeTpl));
        }

        var cartridgeMaxStackSize = cartridgeDetails.Value.Properties?.StackMaxSize;
        if (cartridgeMaxStackSize is null)
        {
            _logger.Error($"Item with tpl: {cartridgeTpl} lacks a _props or StackMaxSize property");
        }

        // Get max number of cartridges in magazine, choose random value between min/max
        var magazineCartridgeMaxCount = IsOfBaseclass(magTemplate.Id, BaseClasses.SPRING_DRIVEN_CYLINDER)
            ? magTemplate.Properties.Slots.Count() // Edge case for rotating grenade launcher magazine
            : magTemplate.Properties.Cartridges[0]?.MaxCount;

        if (magazineCartridgeMaxCount is null)
        {
            _logger.Warning($"Magazine: {magTemplate.Id} {magTemplate.Name} lacks a Cartridges array, unable to fill magazine with ammo");

            return;
        }

        var desiredStackCount = _randomUtil.GetInt(
            (int)
            Math.Round(minSizeMultiplier * magazineCartridgeMaxCount ?? 0),
            (int)magazineCartridgeMaxCount
        );

        if (magazineWithChildCartridges.Count() > 1)
        {
            _logger.Warning($"Magazine {magTemplate.Name} already has cartridges defined,  this may cause issues");
        }

        // Loop over cartridge count and add stacks to magazine
        double? currentStoredCartridgeCount = 0;
        var location = 0;
        while (currentStoredCartridgeCount < desiredStackCount)
        {
            // Get stack size of cartridges
            var cartridgeCountToAdd =
                desiredStackCount <= cartridgeMaxStackSize ? desiredStackCount : cartridgeMaxStackSize;

            // Ensure we don't go over the max stackcount size
            var remainingSpace = desiredStackCount - currentStoredCartridgeCount;
            if (cartridgeCountToAdd > remainingSpace)
            {
                cartridgeCountToAdd = remainingSpace;
            }

            // Add cartridge item object into items array
            magazineWithChildCartridges.Add(
                CreateCartridges(
                    magazineWithChildCartridges[0].Id,
                    cartridgeTpl,
                    cartridgeCountToAdd ?? 0,
                    location,
                    magazineWithChildCartridges[0].Upd?.SpawnedInSession ?? false
                )
            );

            currentStoredCartridgeCount += cartridgeCountToAdd;
            location++;
        }

        // Only one cartridge stack added, remove location property as its only used for 2 or more stacks
        if (location == 1)
        {
            magazineWithChildCartridges[1].Location = null;
        }
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
        double stackCount,
        double location,
        bool foundInRaid = false
    )
    {
        return new () {
            Id = _hashUtil.Generate(),
            Template = ammoTpl,
            ParentId = parentId,
            SlotId = "cartridges",
            Location = location,
            Upd = new () { StackObjectsCount = stackCount, SpawnedInSession = foundInRaid },
        };
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
    // TODO: string newId used to default with _hashUtil.Generate(), Now pass this in

    public string RemapRootItemId(List<Item> itemWithChildren, string newId = null)
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

    public bool IsOfBaseclass(string tpl, string baseClassTpl)
    {
        return _itemBaseClassService.ItemHasBaseClass(tpl, [baseClassTpl]);
    }

    public bool IsOfBaseclasses(string tpl, List<string> baseClassTpls)
    {
        return _itemBaseClassService.ItemHasBaseClass(tpl, baseClassTpls);
    }
}

public class ItemSize
{
    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }
}
