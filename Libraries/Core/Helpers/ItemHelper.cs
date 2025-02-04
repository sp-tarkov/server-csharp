using System.Text.Json.Serialization;
using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Helpers;

[Injectable]
public class ItemHelper(
    ISptLogger<ItemHelper> _logger,
    HashUtil _hashUtil,
    JsonUtil _jsonUtil,
    RandomUtil _randomUtil,
    MathUtil _mathUtil,
    DatabaseService _databaseService,
    HandbookHelper _handbookHelper,
    ItemBaseClassService _itemBaseClassService,
    ItemFilterService _itemFilterService,
    LocalisationService _localisationService,
    LocaleService _localeService,
    CompareUtil _compareUtil,
    ICloner _cloner
)
{
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

    protected List<string> _slotsAsStrings =
    [
        EquipmentSlots.Headwear.ToString(),
        EquipmentSlots.Earpiece.ToString(),
        EquipmentSlots.FaceCover.ToString(),
        EquipmentSlots.ArmorVest.ToString(),
        EquipmentSlots.Eyewear.ToString(),
        EquipmentSlots.ArmBand.ToString(),
        EquipmentSlots.TacticalVest.ToString(),
        EquipmentSlots.Pockets.ToString(),
        EquipmentSlots.Backpack.ToString(),
        EquipmentSlots.SecuredContainer.ToString(),
        EquipmentSlots.FirstPrimaryWeapon.ToString(),
        EquipmentSlots.SecondPrimaryWeapon.ToString(),
        EquipmentSlots.Holster.ToString(),
        EquipmentSlots.Scabbard.ToString()
    ];

    /**
 * Does the provided pool of items contain the desired item
 * @param itemPool Item collection to check
 * @param item Item to look for
 * @param slotId OPTIONAL - slotid of desired item
 * @returns True if pool contains item
 */
    public bool hasItemWithTpl(List<Item> itemPool, string item, string slotId = null)
    {
        // Filter the pool by slotId if provided
        var filteredPool = slotId is not null ? itemPool.Where((item) => item.SlotId?.StartsWith(slotId) ?? false) : itemPool;

        // Check if any item in the filtered pool matches the provided item
        return filteredPool.Any((poolItem) => poolItem.Template == item);
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
        // Filter the pool by slotId if provided
        var filteredPool = slotId is not null ? itemPool.Where((item) => item.SlotId?.StartsWith(slotId) ?? false) : itemPool;

        // Check if any item in the filtered pool matches the provided item
        return filteredPool.FirstOrDefault((poolItem) => poolItem.Template == item);
    }

    /**
     * This method will compare two items (with all its children) and see if they are equivalent.
     * This method will NOT compare IDs on the items
     * @param item1 first item with all its children to compare
     * @param item2 second item with all its children to compare
     * @param compareUpdProperties Upd properties to compare between the items
     * @returns true if they are the same, false if they aren't
     */
    public bool IsSameItems(List<Item> item1, List<Item> item2, HashSet<string> compareUpdProperties = null)
    {
        if (item1.Count() != item2.Count)
        {
            return false;
        }

        foreach (var itemOf1 in item1)
        {
            var itemOf2 = item2.FirstOrDefault((i2) => i2.Template == itemOf1.Template);
            if (itemOf2 is null)
            {
                return false;
            }

            if (!IsSameItem(itemOf1, itemOf2, compareUpdProperties))
            {
                return false;
            }
        }

        return true;
    }

    /**
     * This method will compare two items and see if they are equivalent.
     * This method will NOT compare IDs on the items
     * @param item1 first item to compare
     * @param item2 second item to compare
     * @param compareUpdProperties Upd properties to compare between the items
     * @returns true if they are the same, false if they aren't
     */
    public bool IsSameItem(Item item1, Item item2, HashSet<string>? compareUpdProperties = null)
    {
        // Different tpl == different item
        if (item1.Template != item2.Template)
        {
            return false;
        }

        // Both lack upd object + same tpl = same
        if (item1.Upd is null && item2.Upd is null)
        {
            return true;
        }

        // item1 lacks upd, item2 has one
        if (item1.Upd is null && item2.Upd is not null)
        {
            return false;
        }

        // item1 has upd, item2 lacks one
        if (item1.Upd is not null && item2.Upd is null)
        {
            return false;
        }

        // key = Upd property Type as string, value = comparison function that returns bool
        var comparers = new Dictionary<string, Func<Upd, Upd, bool>>
        {
            { "Key", (upd1, upd2) => upd1.Key?.NumberOfUsages == upd2.Key?.NumberOfUsages },
            { "Buff", (upd1, upd2) => upd1.Buff?.Value == upd2.Buff?.Value && upd1.Buff?.BuffType == upd2.Buff?.BuffType },
            { "CultistAmulet", (upd1, upd2) => upd1.CultistAmulet?.NumberOfUsages == upd2.CultistAmulet?.NumberOfUsages },
            { "Dogtag", (upd1, upd2) => upd1.Dogtag?.ProfileId == upd2.Dogtag?.ProfileId },
            { "FaceShield", (upd1, upd2) => upd1.FaceShield?.Hits == upd2.FaceShield?.Hits },
            { "Foldable", (upd1, upd2) => upd1.Foldable?.Folded.GetValueOrDefault(false) == upd2.Foldable?.Folded.GetValueOrDefault(false) },
            { "FoodDrink", (upd1, upd2) => upd1.FoodDrink?.HpPercent == upd2.FoodDrink?.HpPercent },
            { "MedKit", (upd1, upd2) => upd1.MedKit?.HpResource == upd2.MedKit?.HpResource },
            { "RecodableComponent", (upd1, upd2) => upd1.RecodableComponent?.IsEncoded == upd2.RecodableComponent?.IsEncoded },
            { "RepairKit", (upd1, upd2) => upd1.RepairKit?.Resource == upd2.RepairKit?.Resource },
            { "Resource", (upd1, upd2) => upd1.Resource?.UnitsConsumed == upd2.Resource?.UnitsConsumed }
        };

        // Choose above keys or passed in keys to compare items with
        var valuesToCompare = compareUpdProperties?.Count > 0 ? compareUpdProperties : comparers.Keys.ToHashSet();
        foreach (var propertyName in valuesToCompare)
        {
            if (!comparers.TryGetValue(propertyName, out var comparer))
            {
                // Key not found, skip
                continue;
            }

            if (!comparer(item1.Upd, item2.Upd))
            {
                return false;
            }
        }

        return true;
    }

    /**
     * Helper method to generate a Upd based on a template
     * @param itemTemplate the item template to generate a Upd for
     * @returns A Upd with all the default properties set
     */
    public Upd generateUpdForItem(TemplateItem itemTemplate)
    {
        Upd itemProperties = new();

        // armors, etc
        if (itemTemplate.Properties.MaxDurability is not null)
        {
            itemProperties.Repairable = new()
            {
                Durability = itemTemplate.Properties.MaxDurability,
                MaxDurability = itemTemplate.Properties.MaxDurability,
            };
        }

        if (itemTemplate.Properties.HasHinge ?? false)
        {
            itemProperties.Togglable = new() { On = true };
        }

        if (itemTemplate.Properties.Foldable ?? false)
        {
            itemProperties.Foldable = new() { Folded = false };
        }

        if (itemTemplate.Properties.WeapFireType?.Any() ?? false)
        {
            if (itemTemplate.Properties.WeapFireType.Contains("fullauto"))
            {
                itemProperties.FireMode = new() { FireMode = "fullauto" };
            }
            else
            {
                itemProperties.FireMode = new() { FireMode = _randomUtil.GetArrayValue(itemTemplate.Properties.WeapFireType) };
            }
        }

        if (itemTemplate.Properties.MaxHpResource is not null)
        {
            itemProperties.MedKit = new() { HpResource = itemTemplate.Properties.MaxHpResource };
        }

        if (itemTemplate.Properties.MaxResource is not null && itemTemplate.Properties.FoodUseTime is not null)
        {
            itemProperties.FoodDrink = new() { HpPercent = itemTemplate.Properties.MaxResource };
        }

        if (itemTemplate.Parent == BaseClasses.FLASHLIGHT)
        {
            itemProperties.Light = new() { IsActive = false, SelectedMode = 0 };
        }
        else if (itemTemplate.Parent == BaseClasses.TACTICAL_COMBO)
        {
            itemProperties.Light = new() { IsActive = false, SelectedMode = 0 };
        }

        if (itemTemplate.Parent == BaseClasses.NIGHTVISION)
        {
            itemProperties.Togglable = new() { On = false };
        }

        // Togglable face shield
        if ((itemTemplate.Properties.HasHinge ?? false) && (itemTemplate.Properties.FaceShieldComponent ?? false))
        {
            itemProperties.Togglable = new() { On = false };
        }

        return itemProperties;
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
    public bool IsValidItem(string tpl, List<string> invalidBaseTypes = null)
    {
        var baseTypes = invalidBaseTypes ?? _defaultInvalidBaseTypes;
        var itemDetails = GetItem(tpl);

        if (!itemDetails.Key)
        {
            return false;
        }

        return (
            !(itemDetails.Value.Properties.QuestItem ?? false) &&
            itemDetails.Value.Type == "Item" &&
            baseTypes.All((x) => !IsOfBaseclass(tpl, x)) &&
            GetItemPrice(tpl) > 0 &&
            !_itemFilterService.IsItemBlacklisted(tpl)
        );
    }

    // Check if the tpl / template Id provided is a descendent of the baseclass
    //
    // @param   string    tpl             the item template id to check
    // @param   string    baseClassTpl    the baseclass to check for
    // @return  bool                    is the tpl a descendent?
    public bool IsOfBaseclass(string tpl, string baseClassTpl)
    {
        return _itemBaseClassService.ItemHasBaseClass(tpl, [baseClassTpl]);
    }

    // Check if item has any of the supplied base classes
    // @param string tpl Item to check base classes of
    // @param string[] baseClassTpls base classes to check for
    // @returns true if any supplied base classes match
    public bool IsOfBaseclasses(string tpl, List<string> baseClassTpls)
    {
        return _itemBaseClassService.ItemHasBaseClass(tpl, baseClassTpls);
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
        if (!ArmorItemCanHoldMods(itemTpl))
        {
            return false;
        }

        return ArmorItemHasRemovablePlateSlots(itemTpl) || ItemRequiresSoftInserts(itemTpl);
    }

    // Does the pased in tpl have ability to hold removable plate items
    // @param string itemTpl item tpl to check for plate support
    // @returns True when armor can hold plates
    public bool ArmorItemHasRemovablePlateSlots(string itemTpl)
    {
        var itemTemplate = GetItem(itemTpl);
        var plateSlotIds = GetRemovablePlateSlotIds();

        return itemTemplate.Value.Properties.Slots.Any((slot) => plateSlotIds.Contains(slot.Name.ToLower()));
    }

    // Does the provided item tpl require soft inserts to become a valid armor item
    // @param string itemTpl Item tpl to check
    // @returns True if it needs armor inserts
    public bool ItemRequiresSoftInserts(string itemTpl)
    {
        // not a slot that takes soft-inserts
        if (!ArmorItemCanHoldMods(itemTpl))
        {
            return false;
        }

        // Check is an item
        var itemDbDetails = GetItem(itemTpl);
        if (!itemDbDetails.Key)
        {
            return false;
        }

        // Has no slots
        if (!(itemDbDetails.Value.Properties.Slots ?? []).Any())
        {
            return false;
        }

        // Check if item has slots that match soft insert name ids
        var softInsertIds = GetSoftInsertSlotIds();
        if (itemDbDetails.Value.Properties.Slots.Any((slot) => softInsertIds.Contains(slot.Name.ToLower())))
        {
            return true;
        }

        return false;
    }

    // Get all soft insert slot ids
    // @returns A List of soft insert ids (e.g. soft_armor_back, helmet_top)
    public List<string> GetSoftInsertSlotIds()
    {
        return
        [
            "groin",
            "groin_back",
            "soft_armor_back",
            "soft_armor_front",
            "soft_armor_left",
            "soft_armor_right",
            "shoulder_l",
            "shoulder_r",
            "collar",
            "helmet_top",
            "helmet_back",
            "helmet_eyes",
            "helmet_jaw",
            "helmet_ears",
        ];
    }

    // Returns the items total price based on the handbook or as a fallback from the prices.json if the item is not
    // found in the handbook. If the price can't be found at all return 0
    // @param List<string> tpls item tpls to look up the price of
    // @returns Total price in roubles
    public double GetItemAndChildrenPrice(List<string> tpls)
    {
        // Run getItemPrice for each tpl in tpls array, return sum
        return tpls.Aggregate(0, (total, tpl) => total + (int)GetItemPrice(tpl).GetValueOrDefault(0));
    }

    /// <summary>
    /// Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
    /// found in the handbook. If the price can't be found at all return 0
    /// </summary>
    /// <param name="tpl">Item to look price up of</param>
    /// <returns>Price in roubles</returns>
    public double? GetItemPrice(string tpl)
    {
        var handbookPrice = GetStaticItemPrice(tpl);
        if (handbookPrice >= 1)
        {
            return handbookPrice;
        }

        return GetDynamicItemPrice(tpl);
    }

    /// <summary>
    /// Returns the item price based on the handbook or as a fallback from the prices.json if the item is not
    /// found in the handbook. If the price can't be found at all return 0
    /// </summary>
    /// <param name="tpl">Item to look price up of</param>
    /// <returns>Price in roubles</returns>
    public double GetItemMaxPrice(string tpl)
    {
        var staticPrice = GetStaticItemPrice(tpl);
        var dynamicPrice = GetDynamicItemPrice(tpl);

        return Math.Max(staticPrice, dynamicPrice ?? 0d);
    }

    /// <summary>
    /// Get the static (handbook) price in roubles for an item by tpl
    /// </summary>
    /// <param name="tpl">Items tpl id to look up price</param>
    /// <returns>Price in roubles (0 if not found)</returns>
    public double GetStaticItemPrice(string tpl)
    {
        var handbookPrice = _handbookHelper.GetTemplatePrice(tpl);
        if (handbookPrice >= 1)
        {
            return handbookPrice;
        }

        return 0;
    }

    /// <summary>
    /// Get the dynamic (flea) price in roubles for an item by tpl
    /// </summary>
    /// <param name="tpl">Items tpl id to look up price</param>
    /// <returns>Price in roubles (undefined if not found)</returns>
    public double? GetDynamicItemPrice(string tpl)
    {
        if (_databaseService.GetPrices().TryGetValue(tpl, out var price))
        {
            return price;
        }

        return null;
    }

    /// <summary>
    /// Update items upd.StackObjectsCount to be 1 if its upd is missing or StackObjectsCount is undefined
    /// </summary>
    /// <param name="item">Item to update</param>
    /// <returns>Fixed item</returns>
    public Item FixItemStackCount(Item item)
    {
        // Ensure item has 'Upd' object
        item.Upd ??= new() { StackObjectsCount = 1 };

        // Ensure item has 'StackObjectsCount' property
        item.Upd.StackObjectsCount ??= 1;

        return item;
    }

    /// <summary>
    /// Get cloned copy of all item data from items.json
    /// </summary>
    /// <returns>List of TemplateItem objects</returns>
    public List<TemplateItem> GetItems()
    {
        return _cloner.Clone(_databaseService.GetItems().Values.ToList());
    }

    /**
     * Gets item data from items.json
     * @param itemTpl items template id to look up
     * @returns bool - is valid + template item object as array
     */
    public KeyValuePair<bool, TemplateItem?> GetItem(string itemTpl)
    {
        // -> Gets item from <input: _tpl>
        if (_databaseService.GetItems().TryGetValue(itemTpl, out var item))
        {
            return new(true, item);
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
        if (_databaseService.GetItems().TryGetValue(itemTpl, out var item))
        {
            return GetItem(itemTpl).Value.Properties?.Slots?.Count() > 0;
        }

        return false;
    }

    /**
     * Checks if the item is in the database
     * @param itemTpl Template id of the item to check
     * @returns true if the item is in the database
     */
    public bool IsItemInDb(string itemTpl)
    {
        return _databaseService.GetItems().ContainsKey(itemTpl);
    }

    /**
     * Calculate the average quality of an item and its children
     * @param itemWithChildren An offers item to process
     * @param skipArmorItemsWithoutDurability Skip over armor items without durability
     * @returns % quality modifier between 0 and 1
     */
    public double GetItemQualityModifierForItems(List<Item> itemWithChildren, bool skipArmorItemsWithoutDurability = false)
    {
        if (IsOfBaseclass(itemWithChildren[0].Template, BaseClasses.WEAPON))
        {
            return Math.Round(GetItemQualityModifier(itemWithChildren[0]), 5);
        }

        var qualityModifier = 0D;
        var itemsWithQualityCount = 0D;
        foreach (var item in itemWithChildren)
        {
            var result = GetItemQualityModifier(item, skipArmorItemsWithoutDurability);
            if (result == -1)
            {
                continue;
            }

            qualityModifier += result;
            itemsWithQualityCount++;
        }

        if (itemsWithQualityCount == 0)
        {
            // Can happen when rigs without soft inserts or plates are listed
            return 1;
        }

        return Math.Min(Math.Round(qualityModifier / itemsWithQualityCount, 5), 1);
    }

    /**
     * Get normalized value (0-1) based on item condition
     * Will return -1 for base armor items with 0 durability
     * @param item Item to check
     * @param skipArmorItemsWithoutDurability return -1 for armor items that have max durability of 0
     * @returns Number between 0 and 1
     */
    public double GetItemQualityModifier(Item item, bool skipArmorItemsWithoutDurability = false)
    {
        // Default to 100%
        var result = 1d;

        // Is armor and has 0 max durability
        var itemDetails = GetItem(item.Template).Value;
        if (itemDetails?.Properties is null)
        {
            _logger.Warning($"Item: {item.Template} lacks properties, cannot ascertain quality level, assuming 100%");

            return 1;
        }

        if (skipArmorItemsWithoutDurability
            && IsOfBaseclass(item.Template, BaseClasses.ARMOR)
            && itemDetails?.Properties?.MaxDurability == 0
           )
        {
            return -1;
        }

        if (item.Upd is not null)
        {
            if (item.Upd.MedKit is not null)
            {
                // Meds
                result = (item.Upd.MedKit.HpResource ?? 0) / (itemDetails.Properties.MaxHpResource ?? 0);
            }
            else if (item.Upd.Repairable is not null)
            {
                result = GetRepairableItemQualityValue(itemDetails, item.Upd.Repairable, item);
            }
            else if (item.Upd.FoodDrink is not null)
            {
                result = (item.Upd.FoodDrink.HpPercent ?? 0) / (itemDetails.Properties.MaxResource ?? 0);
            }
            else if (item.Upd.Key?.NumberOfUsages > 0 && itemDetails.Properties.MaximumNumberOfUsage > 0)
            {
                // keys - keys count upwards, not down like everything else
                var maxNumOfUsages = itemDetails.Properties.MaximumNumberOfUsage;
                result = (maxNumOfUsages ?? 0 - item.Upd.Key.NumberOfUsages ?? 0) / maxNumOfUsages ?? 0;
            }
            else if (item.Upd.Resource?.UnitsConsumed > 0)
            {
                // E.g. fuel tank
                result = (item.Upd.Resource.Value ?? 0) / (itemDetails.Properties.MaxResource ?? 0);
            }
            else if (item.Upd.RepairKit is not null)
            {
                result = (item.Upd.RepairKit.Resource ?? 0) / (itemDetails.Properties.MaxRepairResource ?? 0);
            }

            if (result == 0)
            {
                // make item non-zero but still very low
                result = 0.01;
            }

            return result;
        }

        return result;
    }

    /**
     * Get a quality value based on a repairable item's current state between current and max durability
     * @param itemDetails Db details for item we want quality value for
     * @param repairable Repairable properties
     * @param item Item quality value is for
     * @returns A number between 0 and 1
     */
    protected double GetRepairableItemQualityValue(TemplateItem itemDetails, UpdRepairable repairable, Item item)
    {
        // Edge case, durability above max
        if (repairable.Durability > repairable.MaxDurability)
        {
            _logger.Warning(
                $"Max durability: {repairable.MaxDurability} for item id: {item.Id} was below durability: {repairable.Durability}, adjusting values to match"
            );
            repairable.MaxDurability = repairable.Durability;
        }

        // Attempt to get the max durability from _props. If not available, use Repairable max durability value instead.
        var maxPossibleDurability = itemDetails.Properties?.MaxDurability ?? repairable.MaxDurability;
        var durability = repairable.Durability / maxPossibleDurability;

        if (durability == 0)
        {
            _logger.Error(_localisationService.GetText("item-durability_value_invalid_use_default", item.Template));

            return 1;
        }

        return Math.Sqrt(durability ?? 0);
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
        List<Item> list = [];

        foreach (var itemFromAssort in assort)
        {
            // Parent matches desired item + all items in list do not match
            if (itemFromAssort.ParentId == itemIdToFind && list.All(item => itemFromAssort.Id != item.Id))
            {
                list.Add(itemFromAssort);
                list = list.Concat(FindAndReturnChildrenByAssort(itemFromAssort.Id, assort)).ToList();
            }
        }

        return list;
    }

    /**
     * Check if the passed in item has buy count restrictions
     * @param itemToCheck Item to check
     * @returns true if it has buy restrictions
     */
    public bool HasBuyRestrictions(Item itemToCheck)
    {
        if (itemToCheck.Upd?.BuyRestrictionCurrent is not null && itemToCheck.Upd?.BuyRestrictionMax is not null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the passed template id is a dog tag.
    /// </summary>
    /// <param name="tpl">Template id to check.</param>
    /// <returns>True if it is a dogtag.</returns>
    public bool IsDogtag(string tpl)
    {
        List<string> dogTagTpls =
        [
            ItemTpl.BARTER_DOGTAG_BEAR,
            ItemTpl.BARTER_DOGTAG_BEAR_EOD,
            ItemTpl.BARTER_DOGTAG_BEAR_TUE,
            ItemTpl.BARTER_DOGTAG_USEC,
            ItemTpl.BARTER_DOGTAG_USEC_EOD,
            ItemTpl.BARTER_DOGTAG_USEC_TUE,
            ItemTpl.BARTER_DOGTAG_BEAR_PRESTIGE_1,
            ItemTpl.BARTER_DOGTAG_BEAR_PRESTIGE_2,
            ItemTpl.BARTER_DOGTAG_USEC_PRESTIGE_1,
            ItemTpl.BARTER_DOGTAG_USEC_PRESTIGE_2
        ];

        return dogTagTpls.Contains(tpl);
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
    public bool? IsItemTplStackable(string tpl)
    {
        if (!_databaseService.GetItems().TryGetValue(tpl, out var item))
        {
            return null;
        }

        return item.Properties.StackMaxSize > 1;
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
        var itemTemplate = GetItem(itemToSplit.Template).Value;
        var itemMaxStackSize = itemTemplate.Properties.StackMaxSize ?? 1;

        // item already within bounds of stack size, return it
        if (itemToSplit.Upd?.StackObjectsCount <= itemMaxStackSize)
        {
            return [[itemToSplit]];
        }

        // Split items stack into chunks
        List<List<Item>> result = [];
        var remainingCount = itemToSplit.Upd.StackObjectsCount;
        while (remainingCount != 0)
        {
            var amount = Math.Min(remainingCount ?? 0, itemMaxStackSize);
            var newItemClone = _cloner.Clone(itemToSplit);

            newItemClone.Id = _hashUtil.Generate();
            newItemClone.Upd.StackObjectsCount = amount;
            remainingCount -= amount;
            result.Add([newItemClone]);
        }

        return result;
    }

    /// <summary>
    /// Finds Barter items from a list of items.
    /// </summary>
    /// <param name="by">Tpl or id.</param>
    /// <param name="itemsToSearch">Array of items to iterate over.</param>
    /// <param name="desiredBarterItemIds">Desired barter item ids.</param>
    /// <returns>List of Item objects.</returns>
    public List<Item> FindBarterItems(string by, List<Item> itemsToSearch, object desiredBarterItemIds)
    {
        // Find required items to take after buying (handles multiple items)
        List<string> desiredBarterIds =
            desiredBarterItemIds.GetType() == typeof(string) ? [(string)desiredBarterItemIds] : (List<string>)desiredBarterItemIds;

        List<Item> matchingItems = [];
        foreach (var barterId in desiredBarterIds)
        {
            var filterResult = itemsToSearch.Where((item) => { return by == "tpl" ? item.Template == barterId : item.Id == barterId; });

            matchingItems.AddRange(filterResult);
        }

        if (matchingItems.Count == 0)
        {
            _logger.Warning($"No items found for barter Id: {desiredBarterIds}");
        }

        return matchingItems;
    }

    /// <summary>
    /// Replaces the _id value for the base item + all children that are children of it.
    /// REPARENTS ROOT ITEM ID, NOTHING ELSE.
    /// </summary>
    /// <param name="itemWithChildren">Item with mods to update.</param>
    /// <param name="newId">New id to add on children of base item.</param>
    public void ReplaceRootItemID(List<Item> itemWithChildren, string newId = "")
    {
        // original id on base item
        var oldId = itemWithChildren[0].Id;

        // Update base item to use new id
        itemWithChildren[0].Id = newId;

        // Update all parentIds of items attached to base item to use new id
        foreach (var item in itemWithChildren)
        {
            if (item.ParentId == oldId)
            {
                item.ParentId = newId;
            }
        }
    }

    public void ReplaceProfileInventoryIds(BotBaseInventory inventory, List<InsuredItem>? insuredItems = null)
    {
        // Blacklist
        var itemIdBlacklist = new HashSet<string>();
        itemIdBlacklist.UnionWith(
            new List<string>{
                inventory.Equipment,
                inventory.QuestRaidItems,
                inventory.QuestStashItems,
                inventory.SortingTable,
                inventory.Stash,
                inventory.HideoutCustomizationStashId
            });
        itemIdBlacklist.UnionWith(inventory.HideoutAreaStashes.Values);

        // Add insured items ids to blacklist
        if (insuredItems is not null)
        {
            itemIdBlacklist.UnionWith(insuredItems.Select(x => x.ItemId));
        }


        foreach (var item in inventory.Items)
        {
            if (itemIdBlacklist.Contains(item.Id))
            {
                continue;
            }

            // Generate new id
            var newId = _hashUtil.Generate();

            // Keep copy of original id
            var originalId = item.Id;

            // Update items id to new one we generated
            item.Id = newId;

            // Find all children of item and update their parent ids to match
            var childItems = inventory.Items.Where(x => x.ParentId == originalId);
            foreach (var childItem in childItems)
            {
                childItem.ParentId = newId;
            }

            // Also replace in quick slot if the old ID exists.
            if (inventory.FastPanel is null)
            {
                continue;
            }

            // Update quickslot id
            if (inventory.FastPanel.ContainsKey(originalId))
            {
                inventory.FastPanel[originalId] = newId;
            }
        }
    }

    public List<Item> ReplaceIDs(List<Item> items)
    {
        foreach (var item in items)
        {

            // Generate new id
            var newId = _hashUtil.Generate();

            // Keep copy of original id
            var originalId = item.Id;

            // Update items id to new one we generated
            item.Id = newId;

            // Find all children of item and update their parent ids to match
            var childItems = items.Where(x => x.ParentId == originalId);
            foreach (var childItem in childItems)
            {
                childItem.ParentId = newId;
            }
        }

        return items;
    }

    /// <summary>
    /// Regenerate all GUIDs with new IDs, with the exception of special item types (e.g. quest, sorting table, etc.) This
    /// function will not mutate the original items list, but will return a new list with new GUIDs.
    /// </summary>
    /// <param name="originalItems">Items to adjust the IDs of</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="insuredItems">Insured items that should not have their IDs replaced</param>
    /// <param name="fastPanel">Quick slot panel</param>
    /// <returns>List<Item></returns>
    public List<Item> ReplaceIDs(
        List<Item> originalItems,
        PmcData? pmcData = null,
        List<InsuredItem>? insuredItems = null,
        Dictionary<string, string>? fastPanel = null)
    {
        // Blacklist
        var itemIdBlacklist = new HashSet<string>();

        if (pmcData != null)
        {
            itemIdBlacklist.UnionWith(
                new List<string>{
                    pmcData.Inventory.Equipment,
                    pmcData.Inventory.QuestRaidItems,
                    pmcData.Inventory.QuestStashItems,
                    pmcData.Inventory.SortingTable,
                    pmcData.Inventory.Stash,
                    pmcData.Inventory.HideoutCustomizationStashId
                });
            itemIdBlacklist.UnionWith(pmcData.Inventory.HideoutAreaStashes.Keys);
        }
        

        // Add insured items ids to blacklist
        if (insuredItems is not null)
        {
            itemIdBlacklist.UnionWith(insuredItems.Select(x => x.ItemId));
        }


        foreach (var item in originalItems)
        {
            if (itemIdBlacklist.Contains(item.Id))
            {
                continue;
            }

            // Generate new id
            var newId = _hashUtil.Generate();

            // Keep copy of original id
            var originalId = item.Id;

            // Update items id to new one we generated
            item.Id = newId;

            // Find all children of item and update their parent ids to match
            var childItems = originalItems.Where(x => x.ParentId == originalId);
            foreach (var childItem in childItems)
            {
                childItem.ParentId = newId;
            }

            // Also replace in quick slot if the old ID exists.
            if (pmcData.Inventory.FastPanel is null)
            {
                continue;
            }

            // Update quickslot id
            if (pmcData.Inventory.FastPanel.ContainsKey(originalId))
            {
                pmcData.Inventory.FastPanel[originalId] = newId;
            }
        }

        return originalItems;
    }

    /// <summary>
    /// Mark the passed in list of items as found in raid.
    /// Modifies passed in items
    /// </summary>
    /// <param name="items">The list of items to mark as FiR</param>
    /// <param name="excludeCurrency">Skip adding FiR status to currency items</param>
    public void SetFoundInRaid(List<Item> items, bool excludeCurrency = true)
    {
        foreach (var item in items)
        {
            if (excludeCurrency && IsOfBaseclass(item.Template, BaseClasses.MONEY))
            {
                continue;
            }
            item.Upd ??= new();
            item.Upd.SpawnedInSession = true;
        }
    }

    /// <summary>
    /// Mark the passed in list of items as found in raid.
    /// Modifies passed in items
    /// </summary>
    /// <param name="item">The list of items to mark as FiR</param>
    /// <param name="excludeCurrency">Skip adding FiR status to currency items</param>
    public void SetFoundInRaid(Item item, bool excludeCurrency = true)
    {
        if (excludeCurrency && IsOfBaseclass(item.Template, BaseClasses.MONEY))
        {
            return;
        }

        item.Upd ??= new();
        item.Upd.SpawnedInSession = true;
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
        if (itemDetails.Key && itemDetails.Value.Properties.QuestItem.GetValueOrDefault(false))
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
    public Item? GetAttachmentMainParent(string itemId, Dictionary<string, Item> itemsMap)
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
        List<string> check = ["hideout", "main"];

        return !(check.Contains(item.SlotId) || _slotsAsStrings.Contains(item.SlotId) || !int.TryParse(item.SlotId, out var _));
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
    public Item? GetEquipmentParent(string itemId, Dictionary<string, Item> itemsMap)
    {
        var currentItem = itemsMap.GetValueOrDefault(itemId);

        while (currentItem is not null && !_slotsAsStrings.Contains(currentItem.SlotId))
        {
            currentItem = itemsMap.GetValueOrDefault(currentItem.ParentId);
            if (currentItem is null)
            {
                return null;
            }
        }

        return currentItem;
    }

    /**
     * Get the inventory size of an item
     * @param items Item with children
     * @param rootItemId
     * @returns ItemSize object (width and height)
     */
    public ItemSize GetItemSize(List<Item> items, string rootItemId)
    {
        var rootTemplate = GetItem(items.Where((x) => x.Id == rootItemId).ToList()[0].Template).Value;
        var width = rootTemplate.Properties.Width;
        var height = rootTemplate.Properties.Height;

        var sizeUp = 0;
        var sizeDown = 0;
        var sizeLeft = 0;
        var sizeRight = 0;

        var forcedUp = 0;
        var forcedDown = 0;
        var forcedLeft = 0;
        var forcedRight = 0;

        var children = FindAndReturnChildrenAsItems(items, rootItemId);
        foreach (var ci in children)
        {
            var itemTemplate = GetItem(ci.Template).Value;

            // Calculating child ExtraSize
            if (itemTemplate.Properties.ExtraSizeForceAdd ?? false)
            {
                forcedUp += (int)itemTemplate.Properties.ExtraSizeUp;
                forcedDown += (int)itemTemplate.Properties.ExtraSizeDown;
                forcedLeft += (int)itemTemplate.Properties.ExtraSizeLeft;
                forcedRight += (int)itemTemplate.Properties.ExtraSizeRight;
            }
            else
            {
                sizeUp = sizeUp < itemTemplate.Properties.ExtraSizeUp ? (int)itemTemplate.Properties.ExtraSizeUp : sizeUp;
                sizeDown = sizeDown < itemTemplate.Properties.ExtraSizeDown ? (int)itemTemplate.Properties.ExtraSizeDown : sizeDown;
                sizeLeft = sizeLeft < itemTemplate.Properties.ExtraSizeLeft ? (int)itemTemplate.Properties.ExtraSizeLeft : sizeLeft;
                sizeRight = sizeRight < itemTemplate.Properties.ExtraSizeRight ? (int)itemTemplate.Properties.ExtraSizeRight : sizeRight;
            }
        }

        return new()
        {
            Width = width ?? 0 + sizeLeft + sizeRight + forcedLeft + forcedRight,
            Height = height ?? 0 + sizeUp + sizeDown + forcedUp + forcedDown,
        };
    }

    /**
     * Get a random cartridge from an items Filter property
     * @param item Db item template to look up Cartridge filter values from
     * @returns Caliber of cartridge
     */
    public string? GetRandomCompatibleCaliberTemplateId(TemplateItem item)
    {
        var cartridges = item?.Properties?.Cartridges[0]?.Props?.Filters[0]?.Filter;

        if (cartridges is null)
        {
            _logger.Warning($"Failed to find cartridge for item: {item?.Id} {item?.Name}");
            return null;
        }

        return _randomUtil.GetArrayValue(cartridges);
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
                (int)cartridgeCountToAdd,
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
        var ammoBoxMaxCartridgeCount = ammoBoxDetails.Properties?.StackSlots?[0].MaxCount ?? 0;
        var cartridgeTpl = ammoBoxDetails.Properties?.StackSlots?[0].Props?.Filters?[0].Filter?[0];
        ammoBox.Add(
            CreateCartridges(
                ammoBox[0].Id,
                cartridgeTpl,
                (int)ammoBoxMaxCartridgeCount,
                0,
                ammoBox[0].Upd?.SpawnedInSession ?? false
            )
        );
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
        // Get items parent
        var parent = items.FirstOrDefault((item) => item.Id == itemToCheck.ParentId);
        if (parent is null)
        {
            // No parent, end of line, not inside container
            return false;
        }

        if (parent.SlotId == desiredContainerSlotId)
        {
            return true;
        }

        return ItemIsInsideContainer(parent, desiredContainerSlotId, items);
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
        string? caliber = null,
        double minSizePercent = 0.25,
        string? defaultCartridgeTpl = null,
        TemplateItem? weapon = null)
    {
        var chosenCaliber = caliber ?? GetRandomValidCaliber(magTemplate);

        // Edge case for the Klin pp-9, it has a typo in its ammo caliber
        if (chosenCaliber == "Caliber9x18PMM")
        {
            chosenCaliber = "Caliber9x18PM";
        }

        // Chose a randomly weighted cartridge that fits
        var cartridgeTpl = DrawAmmoTpl(
            chosenCaliber,
            staticAmmoDist,
            defaultCartridgeTpl,
            (weapon?.Properties?.Chambers?.FirstOrDefault()?.Props?.Filters?.FirstOrDefault()?.Filter) ?? null
        );
        if (cartridgeTpl is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Unable to fill item: {magazine.FirstOrDefault().Id} {magTemplate.Name} with cartridges, none found.");
            }

            return;
        }

        FillMagazineWithCartridge(magazine, magTemplate, cartridgeTpl, minSizePercent);
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
        var isUBGL = IsOfBaseclass(magTemplate.Id, BaseClasses.UBGL);
        if (isUBGL)
        {
            // UBGL don't have mags
            return;
        }

        // Get cartridge properties and max allowed stack size
        var cartridgeDetails = GetItem(cartridgeTpl);
        if (!cartridgeDetails.Key)
        {
            _logger.Error(_localisationService.GetText("item-invalid_tpl_item", cartridgeTpl));
        }

        var cartridgeMaxStackSize = cartridgeDetails.Value?.Properties?.StackMaxSize;
        if (cartridgeMaxStackSize is null)
        {
            _logger.Error($"Item with tpl: {cartridgeTpl} lacks a _props or StackMaxSize property");
        }

        // Get max number of cartridges in magazine, choose random value between min/max
        var magProps = magTemplate.Properties;
        var magazineCartridgeMaxCount = IsOfBaseclass(magTemplate.Id, BaseClasses.SPRING_DRIVEN_CYLINDER)
            ? magProps?.Slots?.Count // Edge case for rotating grenade launcher magazine
            : magProps?.Cartridges?.FirstOrDefault()?.MaxCount;

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

            // Ensure we don't go over the max stackCount size
            var remainingSpace = desiredStackCount - currentStoredCartridgeCount;
            if (cartridgeCountToAdd > remainingSpace)
            {
                cartridgeCountToAdd = (int)remainingSpace;
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

        // Only one cartridge stack added, remove location property as it's only used for 2 or more stacks
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
    protected string? GetRandomValidCaliber(TemplateItem magTemplate)
    {
        var ammoTpls = magTemplate.Properties.Cartridges[0].Props.Filters[0].Filter;
        List<string> calibers = ammoTpls
            .Where((x) => GetItem(x).Key)
            .Select((x) => GetItem(x).Value.Properties.Caliber)
            .ToList();

        return _randomUtil.DrawRandomFromList(calibers).FirstOrDefault();
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
        string? fallbackCartridgeTpl = null,
        List<string>? cartridgeWhitelist = null
    )
    {
        var ammos = staticAmmoDist.GetValueOrDefault(caliber, []);
        if (ammos.Count == 0 && fallbackCartridgeTpl is not null)
        {
            _logger.Warning($"Unable to pick a cartridge for caliber: {caliber}, staticAmmoDist has no data. using fallback value of {fallbackCartridgeTpl}");

            return fallbackCartridgeTpl;
        }

        if (ammos.Count == 0 && fallbackCartridgeTpl is null)
        {
            _logger.Warning($"Unable to pick a cartridge for caliber: {caliber}, staticAmmoDist has no data. No fallback value provided");

            return null;
        }

        var ammoArray = new ProbabilityObjectArray<string, float?>(_mathUtil, _cloner);
        foreach (var icd in ammos) {
            // Whitelist exists and tpl not inside it, skip
            // Fixes 9x18mm kedr issues
            if (cartridgeWhitelist is not null && !cartridgeWhitelist.Contains(icd.Tpl)) {
                continue;
            }
        
            ammoArray.Add(new ProbabilityObject<string, float?>(icd.Tpl, (double)icd.RelativeProbability, null));
        }

        return ammoArray.Draw(1).FirstOrDefault();
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
        string? ammoTpl,
        int? stackCount,
        double location,
        bool foundInRaid = false
    )
    {
        return new()
        {
            Id = _hashUtil.Generate(),
            Template = ammoTpl!,
            ParentId = parentId,
            SlotId = "cartridges",
            Location = location,
            Upd = new() { StackObjectsCount = stackCount, SpawnedInSession = foundInRaid },
        };
    }

    /// <summary>
    /// Get the size of a stack, return 1 if no stack object count property found
    /// </summary>
    /// <param name="item">Item to get stack size of</param>
    /// <returns>size of stack</returns>
    public int GetItemStackSize(Item item)
    {
        if (item.Upd?.StackObjectsCount is not null)
        {
            return (int)item.Upd.StackObjectsCount;
        }

        return 1;
    }

    /// <summary>
    /// Get the name of an item from the locale file using the item tpl
    /// </summary>
    /// <param name="itemTpl">Tpl of item to get name of</param>
    /// <returns>Full name, short name if not found</returns>
    public string GetItemName(string itemTpl)
    {
        var localeDb = _localeService.GetLocaleDb();
        var result = localeDb[$"{itemTpl} Name"];
        if (result?.Length > 0)
        {
            return result;
        }

        return localeDb[$"{itemTpl} ShortName"];
    }

    /// <summary>
    /// Get all item tpls with a desired base type
    /// </summary>
    /// <param name="desiredBaseType">Item base type wanted</param>
    /// <returns>Array of tpls</returns>
    public List<string> GetItemTplsOfBaseType(string desiredBaseType)
    {
        return _databaseService.GetItems()
            .Values
            .Where((item) => item.Parent == desiredBaseType)
            .Select((item) => item.Id)
            .ToList();
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
        Dictionary<string, double?>? modSpawnChanceDict = null,
        bool requiredOnly = false
    )
    {
        var result = itemToAdd;
        HashSet<string> incompatibleModTpls = new();
        foreach (var slot in itemToAddTemplate.Properties.Slots)
        {
            // If only required mods is requested, skip non-essential
            if (requiredOnly && !(slot.Required ?? false)) continue;

            // Roll chance for non-required slot mods
            if (modSpawnChanceDict is not null && !(slot.Required ?? false))
            {
                // only roll chance to not include mod if dict exists and has value for this mod type (e.g. front_plate)
                var modSpawnChance = modSpawnChanceDict[slot.Name.ToLower()];
                if (modSpawnChance is not null)
                {
                    if (!_randomUtil.GetChance100(modSpawnChance ?? 0))
                    {
                        continue;
                    }
                }
            }

            var itemPool = slot.Props.Filters[0].Filter ?? [];
            if (itemPool.Count() == 0)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to choose a mod for slot: {slot.Name} on item: {itemToAddTemplate.Id} {itemToAddTemplate.Name}, parents' 'Filter' array is empty, skipping"
                    );
                }

                continue;
            }

            var chosenTpl = GetCompatibleTplFromArray(itemPool, incompatibleModTpls);
            if (chosenTpl is null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to choose a mod for slot: {slot.Name} on item: {itemToAddTemplate.Id} {itemToAddTemplate.Name}, no compatible tpl found in pool of {itemPool.Count()}, skipping"
                    );
                }

                continue;
            }

            // Create basic item structure ready to add to weapon array
            Item modItemToAdd = new()
            {
                Id = _hashUtil.Generate(),
                Template = chosenTpl,
                ParentId = result[0].Id,
                SlotId = slot.Name,
            };

            // Add chosen item to weapon array
            result.Add(modItemToAdd);

            var modItemDbDetails = GetItem(modItemToAdd.Template).Value;

            // Include conflicting items of newly added mod in pool to be used for next mod choice
            modItemDbDetails.Properties.ConflictingItems.ForEach(item => incompatibleModTpls.Add(item));
        }

        return result;
    }

    /// <summary>
    /// Get a compatible tpl from the array provided where it is not found in the provided incompatible mod tpls parameter
    /// </summary>
    /// <param name="possibleTpls">Tpls to randomly choose from</param>
    /// <param name="incompatibleModTpls">Incompatible tpls to not allow</param>
    /// <returns>Chosen tpl or undefined</returns>
    public string GetCompatibleTplFromArray(List<string> possibleTpls, HashSet<string> incompatibleModTpls)
    {
        if (!possibleTpls.Any())
        {
            return null;
        }

        string? chosenTpl = null;
        var count = 0;
        while (chosenTpl is null)
        {
            // Loop over choosing a random tpl until one is found or count variable reaches the same size as the possible tpls array
            var tpl = _randomUtil.GetArrayValue(possibleTpls);
            if (incompatibleModTpls.Contains(tpl))
            {
                // Incompatible tpl was chosen, try again
                count++;
                if (count >= possibleTpls.Count)
                {
                    return null;
                }

                continue;
            }

            chosenTpl = tpl;
        }

        return chosenTpl;
    }

    /// <summary>
    /// Is the provided item._props.Slots._name property a plate slot
    /// </summary>
    /// <param name="slotName">Name of slot (_name) of Items Slot array</param>
    /// <returns>True if its a slot that holds a removable plate</returns>
    public bool IsRemovablePlateSlot(string slotName)
    {
        return GetRemovablePlateSlotIds().Contains(slotName.ToLower());
    }

    // Get a list of slot names that hold removable plates
    // Returns Array of slot ids (e.g. front_plate)
    public List<string> GetRemovablePlateSlotIds()
    {
        return ["front_plate", "back_plate", "left_side_plate", "right_side_plate"];
    }

    // Generate new unique ids for child items while preserving hierarchy
    // Base/primary item
    // Primary item + children of primary item
    // Returns Item array with updated IDs
    public List<Item> ReparentItemAndChildren(Item rootItem, List<Item> itemWithChildren)
    {
        var oldRootId = itemWithChildren[0].Id;
        Dictionary<string, string> idMappings = new();

        idMappings[oldRootId] = rootItem.Id;

        foreach (var mod in itemWithChildren)
        {
            if (!idMappings.ContainsKey(mod.Id))
            {
                idMappings[mod.Id] = _hashUtil.Generate();
            }

            // Has parentId + no remapping exists for its parent
            if (mod.ParentId is not null && (!idMappings.ContainsKey(mod.ParentId) || idMappings?[mod.ParentId] is null))
            {
                // Make remapping for items parentId
                idMappings[mod.ParentId] = _hashUtil.Generate();
            }

            mod.Id = idMappings[mod.Id];
            if (mod.ParentId is not null)
            {
                mod.ParentId = idMappings[mod.ParentId];
            }
        }

        // Force item's details into first location of presetItems
        if (itemWithChildren[0].Template != rootItem.Template)
        {
            _logger.Warning($"Reassigning root item from {itemWithChildren[0].Template} to {rootItem.Template}");
        }

        itemWithChildren[0] = rootItem;

        return itemWithChildren;
    }

    // Update a root items _id property value to be unique
    // Item to update root items _id property
    // Optional: new id to use
    // Returns New root id

    public string RemapRootItemId(List<Item> itemWithChildren, string? newId = null)
    {
        newId ??= _hashUtil.Generate();

        var rootItemExistingId = itemWithChildren[0].Id;

        foreach (var item in itemWithChildren)
        {
            // Root, update id
            if (item.Id == rootItemExistingId)
            {
                item.Id = newId;

                continue;
            }

            // Child with parent of root, update
            if (item.ParentId == rootItemExistingId)
            {
                item.ParentId = newId;
            }
        }

        return newId;
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
        foreach (var item in items)
        {
            // Check if the item's parent exists.
            var parentExists = items.Any((parentItem) => parentItem.Id == item.ParentId);

            // If the parent does not exist and the item is not already a 'hideout' item, adopt the orphaned item by
            // setting the parent ID to the PMCs inventory equipment ID, the slot ID to 'hideout', and remove the location.
            if (!parentExists && item.ParentId != rootId && item.SlotId != "hideout")
            {
                item.ParentId = rootId;
                item.SlotId = "hideout";
                item.Location = null;
            }
        }

        return items;
    }

    // Populate a Map object of items for quick lookup using their ID.
    //
    // An array of Items that should be added to a Map.
    // Returns A Map where the keys are the item IDs and the values are the corresponding Item objects.
    public Dictionary<string, Item> GenerateItemsMap(List<Item> items)
    {
        // Convert list to dictionary, keyed by items Id
        return items.ToDictionary(item => item.Id);
    }

    // Add a blank upd object to passed in item if it does not exist already
    // item to add upd to
    // text to write to log when upd object was not found
    // Returns True when upd object was added
    public bool AddUpdObjectToItem(Item item, string? warningMessageWhenMissing = null)
    {
        if (item.Upd is null)
        {
            item.Upd = new();

            if (warningMessageWhenMissing is not null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(warningMessageWhenMissing);
                }
            }

            return true;
        }

        return false;
    }

    // Return all tpls from Money enum
    // Returns string tpls
    public List<string> GetMoneyTpls()
    {
        return [Money.ROUBLES, Money.DOLLARS, Money.EUROS, Money.GP];
    }

    // Get a randomised stack size for the passed in ammo
    // Ammo to get stack size for
    // Default: Limit to 60 to prevent crazy values when players use stack increase mods
    // Returns number
    public int GetRandomisedAmmoStackSize(TemplateItem ammoItemTemplate, int maxLimit = 60)
    {
        return ammoItemTemplate.Properties?.StackMaxSize == 1
            ? 1
            : _randomUtil.GetInt(
                (int?)ammoItemTemplate.Properties?.StackMinRandom ?? 1,
                Math.Min((int?)ammoItemTemplate.Properties?.StackMaxRandom ?? 1, maxLimit)
            );
    }

    public string? GetItemBaseType(string tpl, bool rootOnly = true)
    {
        var result = GetItem(tpl);
        if (!result.Key)
        {
            // Not an item
            return null;
        }

        var currentItem = result.Value;
        while (currentItem is not null)
        {
            if (currentItem.Type == "Node" && !rootOnly)
            {
                // Hit first base type
                return currentItem.Id;
            }

            if (currentItem.Parent is null)
            {
                // No parent, reached root
                return currentItem.Id;
            }

            // Get parent item and start loop again
            currentItem = GetItem(tpl).Value;
        }

        return null;
    }

    // Remove FiR status from passed in items
    // Items to update FiR status of
    public void RemoveSpawnedInSessionPropertyFromItems(List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.Upd is not null)
            {
                item.Upd.SpawnedInSession = null;
            }
        }
    }
}

public class ItemSize
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
