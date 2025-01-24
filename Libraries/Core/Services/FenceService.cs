using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Fence;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class FenceService(
    ISptLogger<FenceService> logger,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    DatabaseService databaseService,
    HandbookHelper handbookHelper,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    LocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected TraderConfig traderConfig = configServer.GetConfig<TraderConfig>();

    /** Time when some items in assort will be replaced  */
    protected long nextPartialRefreshTimestamp;

    /** Main assorts you see at all rep levels */
    protected TraderAssort? fenceAssort = null;

    /** Assorts shown on a separate tab when you max out fence rep */
    protected TraderAssort? fenceDiscountAssort = null;

    /** Desired baseline counts - Hydrated on initial assort generation as part of generateFenceAssorts() */
    protected FenceAssortGenerationValues desiredAssortCounts;

    protected HashSet<string> fenceItemUpdCompareProperties =
    [
        "Buff",
        "Repairable",
        "RecodableComponent",
        "Key",
        "Resource",
        "MedKit",
        "FoodDrink",
        "Dogtag",
        "RepairKit",
    ];


    /**
     * Replace main fence assort with new assort
     * @param assort New assorts to replace old with
     */
    public void SetFenceAssort(TraderAssort assort)
    {
        fenceAssort = assort;
    }

    /**
     * Replace discount fence assort with new assort
     * @param assort New assorts to replace old with
     */
    public void SetDiscountFenceAssort(TraderAssort assort)
    {
        fenceDiscountAssort = assort;
    }

    /**
     * Get main fence assort
     * @return ITraderAssort
     */
    public TraderAssort? GetMainFenceAssort()
    {
        return fenceAssort;
    }

    /**
     * Get discount fence assort
     * @return ITraderAssort
     */
    public TraderAssort? GetDiscountFenceAssort()
    {
        return fenceDiscountAssort;
    }

    /**
     * Replace high rep level fence assort with new assort
     * @param discountAssort New assorts to replace old with
     */
    public void SetFenceDiscountAssort(TraderAssort discountAssort)
    {
        fenceDiscountAssort = discountAssort;
    }

    /**
     * Get assorts player can purchase
     * Adjust prices based on fence level of player
     * @param pmcProfile Player profile
     * @returns ITraderAssort
     */
    public TraderAssort GetFenceAssorts(PmcData pmcProfile)
    {
        if (traderConfig.Fence.RegenerateAssortsOnRefresh)
        {
            // Using base assorts made earlier, do some alterations and store in fenceAssort
            GenerateFenceAssorts();
        }

        // Clone assorts so we can adjust prices before sending to client
        var assort = cloner.Clone(fenceAssort);
        AdjustAssortItemPricesByConfigMultiplier(assort, 1, traderConfig.Fence.PresetPriceMult);

        // merge normal fence assorts + discount assorts if player standing is large enough
        if (pmcProfile.TradersInfo[Traders.FENCE].Standing >= 6)
        {
            var discountAssort = cloner.Clone(fenceDiscountAssort);
            AdjustAssortItemPricesByConfigMultiplier(
                discountAssort,
                traderConfig.Fence.DiscountOptions.ItemPriceMult,
                traderConfig.Fence.DiscountOptions.PresetPriceMult
            );
            var mergedAssorts = MergeAssorts(assort, discountAssort);

            return mergedAssorts;
        }

        return assort;
    }

    /**
     * Adds to fence assort a single item (with its children)
     * @param items the items to add with all its childrens
     * @param mainItem the most parent item of the array
     */
    public void AddItemsToFenceAssort(List<Item> items, Item mainItem)
    {
        // HUGE THANKS TO LACYWAY AND LEAVES FOR PROVIDING THIS SOLUTION FOR SPT TO IMPLEMENT!!
        // Copy the item and its children
        var clonedItems = cloner.Clone(itemHelper.FindAndReturnChildrenAsItems(items, mainItem.Id));
        var root = clonedItems[0];

        var cost = GetItemPrice(root.Template, clonedItems);

        // Fix IDs
        clonedItems = itemHelper.ReparentItemAndChildren(root, clonedItems);
        root.ParentId = "hideout";
        if (root.Upd?.SpawnedInSession != null)
        {
            root.Upd.SpawnedInSession = false;
        }

        // Clean up the items
        // We may need to find an alternative to nodes: delete root.location;
        root.Location = null;

        var createAssort = new CreateFenceAssortsResult()
            { SptItems = [], BarterScheme = new(), LoyalLevelItems = new() };
        createAssort.BarterScheme[root.Id] = [[new BarterScheme() { Count = cost, Template = Money.ROUBLES }]];
        createAssort.SptItems.Add(clonedItems);
        createAssort.LoyalLevelItems[root.Id] = 1;

        UpdateFenceAssorts(createAssort, fenceAssort);
    }

    /**
     * Calculates the overall price for an item (with all its children)
     * @param itemTpl the item tpl to calculate the fence price for
     * @param items the items (with its children) to calculate fence price for
     * @returns the fence price of the item
     */
    public double? GetItemPrice(string itemTpl, List<Item> items)
    {
        return itemHelper.IsOfBaseclass(itemTpl, BaseClasses.AMMO_BOX)
            ? GetAmmoBoxPrice(items) * traderConfig.Fence.ItemPriceMult
            : handbookHelper.GetTemplatePrice(itemTpl) * traderConfig.Fence.ItemPriceMult;
    }

    /**
     * Calculate the overall price for an ammo box, where only one item is
     * the ammo box itself and every other items are the bullets in that box
     * @param items the ammo box (and all its children ammo items)
     * @returns the price of the ammo box
     */
    protected double? GetAmmoBoxPrice(List<Item> items)
    {
        double? total = 0D;
        foreach (var item in items)
        {
            if (itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO))
            {
                total += handbookHelper.GetTemplatePrice(item.Template) * (item.Upd?.StackObjectsCount ?? 1);
            }
        }

        return total;
    }

    /**
     * Adjust all items contained inside an assort by a multiplier
     * @param assort (clone)Assort that contains items with prices to adjust
     * @param itemMultipler multipler to use on items
     * @param presetMultiplier preset multipler to use on presets
     */
    protected void AdjustAssortItemPricesByConfigMultiplier(
        TraderAssort assort,
        double itemMultipler,
        double presetMultiplier
    )
    {
        foreach (var item in assort.Items)
        {
            // Skip sub-items when adjusting prices
            if (item.SlotId != "hideout")
            {
                continue;
            }

            AdjustItemPriceByModifier(item, assort, itemMultipler, presetMultiplier);
        }
    }

    /**
     * Merge two trader assort files together
     * @param firstAssort assort 1#
     * @param secondAssort  assort #2
     * @returns merged assort
     */
    protected TraderAssort MergeAssorts(TraderAssort firstAssort, TraderAssort secondAssort)
    {
        foreach (var itemId in secondAssort.BarterScheme.Keys)
        {
            firstAssort.BarterScheme[itemId] = secondAssort.BarterScheme[itemId];
        }

        foreach (var item in secondAssort.Items)
        {
            firstAssort.Items.Add(item);
        }

        foreach (var itemId in secondAssort.LoyalLevelItems.Keys)
        {
            firstAssort.LoyalLevelItems[itemId] = secondAssort.LoyalLevelItems[itemId];
        }

        return firstAssort;
    }

    /**
     * Adjust assorts price by a modifier
     * @param item assort item details
     * @param assort assort to be modified
     * @param modifier value to multiply item price by
     * @param presetModifier value to multiply preset price by
     */
    protected void AdjustItemPriceByModifier(
        Item item,
        TraderAssort assort,
        double modifier,
        double presetModifier
    )
    {
        // Is preset
        if (item.Upd?.SptPresetId != null)
        {
            if (assort.BarterScheme?[item.Id] != null)
            {
                assort.BarterScheme[item.Id][0][0].Count *= presetModifier;
            }
        }
        else if (assort.BarterScheme?[item.Id] != null)
        {
            assort.BarterScheme[item.Id][0][0].Count *= modifier;
        }
        else
        {
            logger.Warning($"adjustItemPriceByModifier() - no action taken for item: {item.Template}");
        }
    }

    /**
     * Get fence assorts with no price adjustments based on fence rep
     * @returns ITraderAssort
     */
    public TraderAssort GetRawFenceAssorts()
    {
        return MergeAssorts(cloner.Clone(fenceAssort), cloner.Clone(fenceDiscountAssort));
    }

    /**
     * Does fence need to perform a partial refresh because its passed the refresh timer defined in trader.json
     * @returns true if it needs a partial refresh
     */
    public bool NeedsPartialRefresh()
    {
        return timeUtil.GetTimeStamp() > nextPartialRefreshTimestamp;
    }

    /**
     * Replace a percentage of fence assorts with freshly generated items
     */
    public void PerformPartialRefresh()
    {
        var itemCountToReplace = GetCountOfItemsToReplace(traderConfig.Fence.AssortSize);
        var discountItemCountToReplace = GetCountOfItemsToReplace(
            traderConfig.Fence.DiscountOptions.AssortSize
        );

        // Simulate players buying items
        DeleteRandomAssorts(itemCountToReplace, fenceAssort);
        DeleteRandomAssorts(discountItemCountToReplace, fenceDiscountAssort);

        var normalItemCountsToGenerate = GetItemCountsToGenerate(
            fenceAssort.Items,
            desiredAssortCounts.Normal
        );
        var newItems = CreateAssorts(normalItemCountsToGenerate, 1);

        // Push newly generated assorts into existing data
        UpdateFenceAssorts(newItems, fenceAssort);

        var discountItemCountsToGenerate = GetItemCountsToGenerate(
            fenceDiscountAssort.Items,
            desiredAssortCounts.Discount
        );
        var newDiscountItems = CreateAssorts(discountItemCountsToGenerate, 2);

        // Push newly generated discount assorts into existing data
        UpdateFenceAssorts(newDiscountItems, fenceDiscountAssort);

        // Add new barter items to fence barter scheme
        foreach (var barterItemKey in newItems.BarterScheme.Keys)
        {
            fenceAssort.BarterScheme[barterItemKey] = newItems.BarterScheme[barterItemKey];
        }

        // Add loyalty items to fence assorts loyalty object
        foreach (var loyaltyItemKey in newItems.LoyalLevelItems.Keys)
        {
            fenceAssort.LoyalLevelItems[loyaltyItemKey] = newItems.LoyalLevelItems[loyaltyItemKey];
        }

        // Add new barter items to fence assorts discounted barter scheme
        foreach (var barterItemKey in newDiscountItems.BarterScheme.Keys)
        {
            fenceDiscountAssort.BarterScheme[barterItemKey] = newDiscountItems.BarterScheme[barterItemKey];
        }

        // Add loyalty items to fence discount assorts loyalty object
        foreach (var loyaltyItemKey in newDiscountItems.LoyalLevelItems.Keys)
        {
            fenceDiscountAssort.LoyalLevelItems[loyaltyItemKey] = newDiscountItems.LoyalLevelItems[loyaltyItemKey];
        }

        // Reset the clock
        IncrementPartialRefreshTime();
    }

    /**
     * Handle the process of folding new assorts into existing assorts, when a new assort exists already, increment its StackObjectsCount instead
     * @param newFenceAssorts Assorts to fold into existing fence assorts
     * @param existingFenceAssorts Current fence assorts new assorts will be added to
     */
    protected void UpdateFenceAssorts(
        CreateFenceAssortsResult newFenceAssorts,
        TraderAssort existingFenceAssorts
    )
    {
        foreach (var itemWithChildren in newFenceAssorts.SptItems)
        {
            // Find the root item
            var newRootItem = itemWithChildren.FirstOrDefault((item) => item.SlotId == "hideout");
            if (newRootItem == null)
            {
                var firstItem = itemWithChildren.FirstOrDefault((x) => x != null);
                logger.Error(
                    $"Unable to process fence assort as root item is missing, {firstItem?.Template}, skipping"
                );
                continue;
            }

            // Find a matching root item with same tpl in existing assort
            var existingRootItem = existingFenceAssorts.Items.FirstOrDefault(
                (item) => item.Template == newRootItem.Template && item.SlotId == "hideout"
            );

            // Check if same type of item exists + its on list of item types to always stack
            if (existingRootItem != null && ItemInPreventDupeCategoryList(newRootItem.Template))
            {
                var existingFullItemTree = itemHelper.FindAndReturnChildrenAsItems(
                    existingFenceAssorts.Items,
                    existingRootItem.Id
                );
                if (itemHelper.isSameItems(itemWithChildren, existingFullItemTree, fenceItemUpdCompareProperties))
                {
                    // Guard against a missing stack count
                    if (existingRootItem.Upd?.StackObjectsCount == null)
                    {
                        existingRootItem.Upd.StackObjectsCount = 1;
                    }

                    // Merge new items count into existing, dont add new loyalty/barter data as it already exists
                    existingRootItem.Upd.StackObjectsCount += newRootItem?.Upd?.StackObjectsCount ?? 1;

                    continue;
                }
            }

            // if the Upd doesnt exist just initialize it
            if (newRootItem.Upd == null)
            {
                newRootItem.Upd = new();
            }

            // New assort to be added to existing assorts
            existingFenceAssorts.Items.AddRange(itemWithChildren);
            existingFenceAssorts.BarterScheme[newRootItem.Id] = newFenceAssorts.BarterScheme[newRootItem.Id];
            existingFenceAssorts.LoyalLevelItems[newRootItem.Id] = newFenceAssorts.LoyalLevelItems[newRootItem.Id];
        }
    }

    /**
     * Increment fence next refresh timestamp by current timestamp + partialRefreshTimeSeconds from config
     */
    protected void IncrementPartialRefreshTime()
    {
        nextPartialRefreshTimestamp = timeUtil.GetTimeStamp() + traderConfig.Fence.PartialRefreshTimeSeconds;
    }

    /**
     * Get values that will hydrate the passed in assorts back to the desired counts
     * @param assortItems Current assorts after items have been removed
     * @param generationValues Base counts assorts should be adjusted to
     * @returns IGenerationAssortValues object with adjustments needed to reach desired state
     */
    protected GenerationAssortValues GetItemCountsToGenerate(
        List<Item> assortItems,
        GenerationAssortValues generationValues
    )
    {
        var allRootItems = assortItems.Where((item) => item.SlotId == "hideout");
        var rootPresetItems = allRootItems.Where((item) => item?.Upd?.SptPresetId != null);

        // Get count of weapons
        var currentWeaponPresetCount = rootPresetItems.Aggregate(
            0,
            (count, item) => itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON) ? count + 1 : count
        );

        // Get count of equipment
        var currentEquipmentPresetCount = rootPresetItems.Aggregate(
            0,
            (count, item) => itemHelper.ArmorItemCanHoldMods(item.Template) ? count + 1 : count
        );

        // Normal item count is total count minus weapon + armor count
        var nonPresetItemAssortCount = allRootItems.Count() - (currentWeaponPresetCount + currentEquipmentPresetCount);

        // Get counts of items to generate, never var values fall below 0
        var itemCountToGenerate = Math.Max(generationValues.Item.Value - nonPresetItemAssortCount, 0);
        var weaponCountToGenerate = Math.Max(generationValues.WeaponPreset.Value - currentWeaponPresetCount, 0);
        var equipmentCountToGenerate = Math.Max(
            generationValues.EquipmentPreset.Value - currentEquipmentPresetCount,
            0
        );

        return new GenerationAssortValues
        {
            Item = itemCountToGenerate,
            WeaponPreset = weaponCountToGenerate,
            EquipmentPreset = equipmentCountToGenerate
        };
    }

    /**
     * Delete desired number of items from assort (including children)
     * @param itemCountToReplace
     * @param discountItemCountToReplace
     */
    protected void DeleteRandomAssorts(int itemCountToReplace, TraderAssort assort)
    {
        if (assort?.Items?.Count > 0)
        {
            var rootItems = assort.Items.Where((item) => item.SlotId == "hideout").ToList();
            for (var index = 0; index < itemCountToReplace; index++)
            {
                RemoveRandomItemFromAssorts(assort, rootItems);
            }
        }
    }

    /**
     * Choose an item at random and remove it + mods from assorts
     * @param assort Trader assort to remove item from
     * @param rootItems Pool of root items to pick from to remove
     */
    protected void RemoveRandomItemFromAssorts(TraderAssort assort, List<Item> rootItems)
    {
        var rootItemToAdjust = randomUtil.GetArrayValue(rootItems);

        // Items added by mods may not have a Upd object, assume item stack size is 1
        var stackSize = rootItemToAdjust.Upd?.StackObjectsCount ?? 1;

        // Get a random count of the chosen item to remove
        var itemCountToRemove = randomUtil.GetInt(1, (int)stackSize);

        var isEntireStackToBeRemoved = Math.Abs(itemCountToRemove - stackSize) < 0.1;

        // Partial stack reduction
        if (!isEntireStackToBeRemoved)
        {
            if (rootItemToAdjust.Upd == null)
            {
                logger.Warning($"Fence Item: {rootItemToAdjust.Template} lacks a Upd object, adding");
                rootItemToAdjust.Upd = new();
            }

            // Reduce stack to at smallest, 1
            rootItemToAdjust.Upd.StackObjectsCount -= Math.Max(1, itemCountToRemove);

            return;
        }

        // Remove item + child mods (if any)
        var itemWithChildren = itemHelper.FindAndReturnChildrenAsItems(assort.Items, rootItemToAdjust.Id);
        foreach (var itemToDelete in itemWithChildren)
        {
            // Delete item from assort items array
            assort.Items.Splice(assort.Items.IndexOf(itemToDelete), 1);
        }

        // Need to remove item from all areas of trader assort
        // delete assort.barter_scheme[rootItemToAdjust._id];
        // delete assort.loyal_level_items[rootItemToAdjust._id];
        assort.BarterScheme.Remove(rootItemToAdjust.Id);
        assort.LoyalLevelItems.Remove(rootItemToAdjust.Id);
    }

    /**
     * Get an integer rounded count of items to replace based on percentrage from traderConfig value
     * @param totalItemCount total item count
     * @returns rounded int of items to replace
     */
    protected int GetCountOfItemsToReplace(int totalItemCount)
    {
        return (int)Math.Round(totalItemCount * (traderConfig.Fence.PartialRefreshChangePercent / 100));
    }

    /**
     * Get the count of items fence offers
     * @returns number
     */
    public int GetOfferCount()
    {
        if ((fenceAssort?.Items?.Count ?? 0) == 0)
        {
            return 0;
        }

        return fenceAssort.Items.Count;
    }

    /**
     * Create trader assorts for fence and store in fenceService cache
     * Uses fence base cache generatedon server start as a base
     */
    public void GenerateFenceAssorts()
    {
        // Reset refresh time now assorts are being generated
        IncrementPartialRefreshTime();

        // Choose assort counts using config
        CreateInitialFenceAssortGenerationValues();

        // Create basic fence assort
        var assorts = CreateAssorts(desiredAssortCounts.Normal, 1);

        // Store in fenceAssort
        SetFenceAssort(ConvertIntoFenceAssort(assorts));

        // Create level 2 assorts accessible at rep level 6
        var discountAssorts = CreateAssorts(desiredAssortCounts.Discount, 2);

        // Store in fenceDiscountAssort
        SetFenceDiscountAssort(ConvertIntoFenceAssort(discountAssorts));
    }

    /**
     * Convert the intermediary assort data generated into format client can process
     * @param intermediaryAssorts Generated assorts that will be converted
     * @returns ITraderAssort
     */
    protected TraderAssort ConvertIntoFenceAssort(CreateFenceAssortsResult intermediaryAssorts)
    {
        var result = CreateFenceAssortSkeleton();
        foreach (var itemWithChilden in intermediaryAssorts.SptItems)
        {
            result.Items.AddRange(itemWithChilden);
        }

        result.BarterScheme = intermediaryAssorts.BarterScheme;
        result.LoyalLevelItems = intermediaryAssorts.LoyalLevelItems;

        return result;
    }

    /**
     * Create object that contains calculated fence assort item values to make based on config
     * Stored in desiredAssortCounts
     */
    protected void CreateInitialFenceAssortGenerationValues()
    {
        var result = new FenceAssortGenerationValues()
        {
            Normal = new GenerationAssortValues() { Item = 0, WeaponPreset = 0, EquipmentPreset = 0 },
            Discount = new GenerationAssortValues() { Item = 0, WeaponPreset = 0, EquipmentPreset = 0 }
        };

        result.Normal.Item = traderConfig.Fence.AssortSize;

        result.Normal.WeaponPreset = randomUtil.GetInt(
            (int)traderConfig.Fence.WeaponPresetMinMax.Min,
            (int)traderConfig.Fence.WeaponPresetMinMax.Max
        );

        result.Normal.EquipmentPreset = randomUtil.GetInt(
            (int)traderConfig.Fence.EquipmentPresetMinMax.Min,
            (int)traderConfig.Fence.EquipmentPresetMinMax.Max
        );

        result.Discount.Item = traderConfig.Fence.DiscountOptions.AssortSize;

        result.Discount.WeaponPreset = randomUtil.GetInt(
            (int)traderConfig.Fence.DiscountOptions.WeaponPresetMinMax.Min,
            (int)traderConfig.Fence.DiscountOptions.WeaponPresetMinMax.Max
        );

        result.Discount.EquipmentPreset = randomUtil.GetInt(
            (int)traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax.Min,
            (int)traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax.Max
        );

        desiredAssortCounts = result;
    }

    /**
     * Create skeleton to hold assort items
     * @returns ITraderAssort object
     */
    protected TraderAssort CreateFenceAssortSkeleton()
    {
        return new TraderAssort()
        {
            Items = [],
            BarterScheme = new(),
            LoyalLevelItems = new(),
            NextResupply = GetNextFenceUpdateTimestamp(),
        };
    }

    /**
     * Hydrate assorts parameter object with generated assorts
     * @param assortCount Number of assorts to generate
     * @param assorts object to add created assorts to
     */
    protected CreateFenceAssortsResult CreateAssorts(GenerationAssortValues itemCounts, int loyaltyLevel)
    {
        var result = new CreateFenceAssortsResult() { SptItems = [], BarterScheme = new(), LoyalLevelItems = new() };

        var baseFenceAssortClone = cloner.Clone(databaseService.GetTrader(Traders.FENCE).Assort);
        var itemTypeLimitCounts = InitItemLimitCounter(traderConfig.Fence.ItemTypeLimits);

        if (itemCounts.Item > 0)
        {
            AddItemAssorts(itemCounts.Item, result, baseFenceAssortClone, itemTypeLimitCounts, loyaltyLevel);
        }

        if (itemCounts.WeaponPreset > 0 || itemCounts.EquipmentPreset > 0)
        {
            // Add presets
            AddPresetsToAssort(
                itemCounts.WeaponPreset,
                itemCounts.EquipmentPreset,
                result,
                baseFenceAssortClone,
                loyaltyLevel
            );
        }

        return result;
    }

    /**
     * Add item assorts to existing assort data
     * @param assortCount Number to add
     * @param assorts Assorts data to add to
     * @param baseFenceAssortClone Base data to draw from
     * @param itemTypeLimits
     * @param loyaltyLevel Loyalty level to set new item to
     */
    protected void AddItemAssorts(
        int? assortCount,
        CreateFenceAssortsResult assorts,
        TraderAssort baseFenceAssortClone,
        Dictionary<string, (int current, int max)> itemTypeLimits,
        int loyaltyLevel
    )
    {
        var priceLimits = traderConfig.Fence.ItemCategoryRoublePriceLimit;
        var assortRootItems = baseFenceAssortClone.Items
            .Where(item => item.ParentId == "hideout" && item.Upd?.SptPresetId == null)
            .ToList();
        if (assortRootItems.Count == 0)
        {
            logger.Error("Unable to add assorts to Fence as no root items exist in items being added");
            return;
        }

        for (var i = 0; i < assortCount; i++)
        {
            var chosenBaseAssortRoot = randomUtil.GetArrayValue(assortRootItems);
            if (chosenBaseAssortRoot == null)
            {
                logger.Error(localisationService.GetText("fence-unable_to_find_assort_by_id"));
                continue;
            }

            var desiredAssortItemAndChildrenClone = cloner.Clone(
                itemHelper.FindAndReturnChildrenAsItems(baseFenceAssortClone.Items, chosenBaseAssortRoot.Id)
            );

            var itemDbDetails = itemHelper.GetItem(chosenBaseAssortRoot.Template).Value;
            var itemLimitCount = GetMatchingItemLimit(itemTypeLimits, itemDbDetails.Id);
            if (itemLimitCount?.current >= itemLimitCount?.max)
            {
                // Skip adding item as assort as limit reached, decrement i counter so we still get another item
                i--;
                continue;
            }

            var itemIsPreset = presetHelper.IsPreset(chosenBaseAssortRoot.Id);

            var price = baseFenceAssortClone.BarterScheme?[chosenBaseAssortRoot.Id][0][0].Count;
            if (price == 0 || (price == 1 && !itemIsPreset) || price == 100)
            {
                // Don't allow "special" items / presets
                i--;
                continue;
            }

            if (price > priceLimits[itemDbDetails.Parent])
            {
                // Too expensive for fence, try another item
                i--;
                continue;
            }

            // Increment count as item is being added
            if (itemLimitCount.HasValue)
            {
                var value = itemLimitCount.Value;
                value.current += 1;
            }

            // MUST randomise Ids as its possible to add the same base fence assort twice = duplicate IDs = dead client
            desiredAssortItemAndChildrenClone = itemHelper.ReplaceIDs(desiredAssortItemAndChildrenClone);
            itemHelper.RemapRootItemId(desiredAssortItemAndChildrenClone);

            var rootItemBeingAdded = desiredAssortItemAndChildrenClone[0];

            // Set stack size based on possible overrides, e.g. ammos, otherwise set to 1
            rootItemBeingAdded.Upd.StackObjectsCount = GetSingleItemStackCount(itemDbDetails);

            // Only randomise Upd values for single
            var isSingleStack = Math.Abs((rootItemBeingAdded.Upd?.StackObjectsCount ?? 0) - 1) < 0.1;
            if (isSingleStack)
            {
                RandomiseItemUpdProperties(itemDbDetails, rootItemBeingAdded);
            }

            // Skip items already in the assort if it exists in the prevent duplicate list
            var existingItemThatMatches = GetMatchingItem(rootItemBeingAdded, itemDbDetails, assorts.SptItems);
            var shouldBeStacked = ItemShouldBeForceStacked(existingItemThatMatches, itemDbDetails);
            if (shouldBeStacked && existingItemThatMatches != null)
            {
                // Decrement loop counter so another items gets added
                i--;
                existingItemThatMatches.Upd.StackObjectsCount++;

                continue;
            }

            // Add mods to armors so they dont show as red in the trade screen
            if (itemHelper.ItemRequiresSoftInserts(rootItemBeingAdded.Template))
            {
                RandomiseArmorModDurability(desiredAssortItemAndChildrenClone, itemDbDetails);
            }

            assorts.SptItems.Add(desiredAssortItemAndChildrenClone);

            assorts.BarterScheme[rootItemBeingAdded.Id] =
                cloner.Clone(baseFenceAssortClone.BarterScheme[chosenBaseAssortRoot.Id]);

            // Only adjust item price by quality for solo items, never multi-stack
            if (isSingleStack)
            {
                AdjustItemPriceByQuality(assorts.BarterScheme, rootItemBeingAdded, itemDbDetails);
            }

            assorts.LoyalLevelItems[rootItemBeingAdded.Id] = loyaltyLevel;
        }
    }

    /**
     * Find an assort item that matches the first parameter, also matches based on Upd properties
     * e.g. salewa hp resource units left
     * @param rootItemBeingAdded item to look for a match against
     * @param itemDbDetails Db details of matching item
     * @param itemsWithChildren Items to search through
     * @returns Matching assort item
     */
    protected Item? GetMatchingItem(
        Item rootItemBeingAdded,
        TemplateItem itemDbDetails,
        List<List<Item>> itemsWithChildren
    )
    {
        // Get matching root items
        var matchingItems = itemsWithChildren
            .Where(
                (itemWithChildren) => itemWithChildren.FirstOrDefault(
                                          (item) => item.Template == rootItemBeingAdded.Template &&
                                                    item.ParentId == "hideout"
                                      ) !=
                                      null
            )
            .SelectMany(i => i)
            .ToList();
        if (matchingItems.Count == 0)
        {
            // Nothing matches by tpl and is root item, exit early
            return null;
        }

        var isMedical = itemHelper.IsOfBaseclasses(
            rootItemBeingAdded.Template,
            [
                BaseClasses.MEDICAL,
                BaseClasses.MEDKIT
            ]
        );
        var isGearAndHasSlots =
            itemHelper.IsOfBaseclasses(
                rootItemBeingAdded.Template,
                [
                    BaseClasses.ARMORED_EQUIPMENT,
                    BaseClasses.SEARCHABLE_ITEM
                ]
            ) &&
            (itemDbDetails.Properties.Slots?.Count ?? 0) > 0;

        // Only one match and its not medical or armored gear
        if (matchingItems.Count == 1 && !(isMedical || isGearAndHasSlots))
        {
            return matchingItems[0];
        }

        // Items have sub properties that need to be checked against
        foreach (var item in matchingItems)
        {
            if (isMedical && rootItemBeingAdded.Upd?.MedKit?.HpResource == item.Upd?.MedKit?.HpResource)
            {
                // e.g. bandages with multiple use
                // Both undefined === both max resoruce left
                return item;
            }

            // Armors/helmets etc
            if (
                isGearAndHasSlots &&
                rootItemBeingAdded.Upd.Repairable?.Durability == item.Upd.Repairable?.Durability &&
                rootItemBeingAdded.Upd.Repairable?.MaxDurability == item.Upd.Repairable?.MaxDurability
            )
            {
                return item;
            }
        }

        return null;
    }

    /**
     * Should this item be forced into only 1 stack on fence
     * @param existingItem Existing item from fence assort
     * @param itemDbDetails Item we want to add db details
     * @returns True item should be force stacked
     */
    protected bool ItemShouldBeForceStacked(Item? existingItem, TemplateItem itemDbDetails)
    {
        // No existing item in assort
        if (existingItem == null)
        {
            return false;
        }

        // Don't stack child items, only root items
        if (existingItem.ParentId != "hideout")
        {
            return false;
        }

        return ItemInPreventDupeCategoryList(itemDbDetails.Id);
    }

    protected bool ItemInPreventDupeCategoryList(string tpl)
    {
        // Item type in config list
        return itemHelper.IsOfBaseclasses(tpl, traderConfig.Fence.PreventDuplicateOffersOfCategory);
    }

    /**
     * Adjust price of item based on what is left to buy (resource/uses left)
     * @param barterSchemes All barter scheme for item having price adjusted
     * @param itemRoot Root item having price adjusted
     * @param itemTemplate Db template of item
     */
    protected void AdjustItemPriceByQuality(
        Dictionary<string, List<List<BarterScheme>>> barterSchemes,
        Item itemRoot,
        TemplateItem itemTemplate
    )
    {
        // Healing items
        if (itemRoot.Upd?.MedKit != null)
        {
            var itemTotalMax = itemTemplate.Properties.MaxHpResource;
            var current = itemRoot.Upd.MedKit.HpResource;

            // Current and max match, no adjustment necessary
            if (itemTotalMax == current)
            {
                return;
            }

            var multipler = current / itemTotalMax;

            // Multiply item cost by desired multiplier
            var basePrice = barterSchemes[itemRoot.Id][0][0].Count;
            barterSchemes[itemRoot.Id][0][0].Count = Math.Round((double)(basePrice * multipler));

            return;
        }

        // Adjust price based on durability
        if (itemRoot.Upd?.Repairable != null || itemHelper.IsOfBaseclass(itemRoot.Template, BaseClasses.KEY_MECHANICAL))
        {
            var itemQualityModifier = itemHelper.GetItemQualityModifier(itemRoot);
            var basePrice = barterSchemes[itemRoot.Id][0][0].Count;
            barterSchemes[itemRoot.Id][0][0].Count = Math.Round((double)basePrice * itemQualityModifier);
        }
    }

    protected (int current, int max)? GetMatchingItemLimit(
        Dictionary<string, (int current, int max)> itemTypeLimits,
        string itemTpl
    )
    {
        foreach (var baseTypeKey in itemTypeLimits.Keys)
        {
            if (itemHelper.IsOfBaseclass(itemTpl, baseTypeKey))
            {
                return itemTypeLimits[baseTypeKey];
            }
        }

        return null;
    }

    /**
     * Find presets in base fence assort and add desired number to 'assorts' parameter
     * @param desiredWeaponPresetsCount
     * @param assorts Assorts to add preset to
     * @param baseFenceAssort Base data to draw from
     * @param loyaltyLevel Which loyalty level is required to see/buy item
     */
    protected void AddPresetsToAssort(
        int? desiredWeaponPresetsCount,
        int? desiredEquipmentPresetsCount,
        CreateFenceAssortsResult assorts,
        TraderAssort baseFenceAssort,
        int loyaltyLevel
    )
    {
        var weaponPresetsAddedCount = 0;
        if (desiredWeaponPresetsCount > 0)
        {
            var weaponPresetRootItems = baseFenceAssort.Items.Where(
                item => item.Upd?.SptPresetId != null && itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON)
            );
            while (weaponPresetsAddedCount < desiredWeaponPresetsCount)
            {
                var randomPresetRoot = randomUtil.GetArrayValue(weaponPresetRootItems);
                if (traderConfig.Fence.Blacklist.Contains(randomPresetRoot.Template))
                {
                    continue;
                }

                var rootItemDb = itemHelper.GetItem(randomPresetRoot.Template).Value;

                var presetWithChildrenClone = cloner.Clone(
                    itemHelper.FindAndReturnChildrenAsItems(baseFenceAssort.Items, randomPresetRoot.Id)
                );

                RandomiseItemUpdProperties(rootItemDb, presetWithChildrenClone[0]);

                RemoveRandomModsOfItem(presetWithChildrenClone);

                // Check chosen item is below price cap
                var priceLimitRouble = traderConfig.Fence.ItemCategoryRoublePriceLimit[rootItemDb.Parent];
                var itemPrice =
                    handbookHelper.GetTemplatePriceForItems(presetWithChildrenClone) *
                    itemHelper.GetItemQualityModifierForItems(presetWithChildrenClone);
                if (priceLimitRouble != null)
                {
                    if (itemPrice > priceLimitRouble)
                    {
                        // Too expensive, try again
                        continue;
                    }
                }

                // MUST randomise Ids as its possible to add the same base fence assort twice = duplicate IDs = dead client
                itemHelper.ReparentItemAndChildren(presetWithChildrenClone[0], presetWithChildrenClone);
                itemHelper.RemapRootItemId(presetWithChildrenClone);

                // Remapping IDs causes parentid to be altered
                presetWithChildrenClone[0].ParentId = "hideout";

                assorts.SptItems.Add(presetWithChildrenClone);

                // Set assort price
                // Must be careful to use correct id as the item has had its IDs regenerated
                assorts.BarterScheme[presetWithChildrenClone[0].Id] =
                [
                    [
                        new BarterScheme()
                        {
                            Template = Money.ROUBLES,
                            Count = Math.Round(itemPrice),
                        }
                    ]
                ];
                assorts.LoyalLevelItems[presetWithChildrenClone[0].Id] = loyaltyLevel;

                weaponPresetsAddedCount++;
            }
        }

        var equipmentPresetsAddedCount = 0;
        if (desiredEquipmentPresetsCount <= 0)
        {
            return;
        }

        var equipmentPresetRootItems = baseFenceAssort.Items.Where(
            (item) => item.Upd?.SptPresetId != null && itemHelper.ArmorItemCanHoldMods(item.Template)
        );
        while (equipmentPresetsAddedCount < desiredEquipmentPresetsCount)
        {
            var randomPresetRoot = randomUtil.GetArrayValue(equipmentPresetRootItems);
            var rootItemDb = itemHelper.GetItem(randomPresetRoot.Template).Value;

            var presetWithChildrenClone = cloner.Clone(
                itemHelper.FindAndReturnChildrenAsItems(baseFenceAssort.Items, randomPresetRoot.Id)
            );

            // Need to add mods to armors so they dont show as red in the trade screen
            if (itemHelper.ItemRequiresSoftInserts(randomPresetRoot.Template))
            {
                RandomiseArmorModDurability(presetWithChildrenClone, rootItemDb);
            }

            RemoveRandomModsOfItem(presetWithChildrenClone);

            // Check chosen item is below price cap
            var priceLimitRouble = traderConfig.Fence.ItemCategoryRoublePriceLimit[rootItemDb.Parent];
            var itemPrice =
                handbookHelper.GetTemplatePriceForItems(presetWithChildrenClone) *
                itemHelper.GetItemQualityModifierForItems(presetWithChildrenClone);
            if (priceLimitRouble != null)
            {
                if (itemPrice > priceLimitRouble)
                {
                    // Too expensive, try again
                    continue;
                }
            }

            // MUST randomise Ids as its possible to add the same base fence assort twice = duplicate IDs = dead client
            itemHelper.ReparentItemAndChildren(presetWithChildrenClone[0], presetWithChildrenClone);
            itemHelper.RemapRootItemId(presetWithChildrenClone);

            // Remapping IDs causes parentid to be altered
            presetWithChildrenClone[0].ParentId = "hideout";

            assorts.SptItems.Add(presetWithChildrenClone);

            // Set assort price
            // Must be careful to use correct id as the item has had its IDs regenerated
            assorts.BarterScheme[presetWithChildrenClone[0].Id] =
            [
                [
                    new BarterScheme()
                    {
                        Template = Money.ROUBLES,
                        Count = Math.Round(itemPrice),
                    }
                ]
            ];
            assorts.LoyalLevelItems[presetWithChildrenClone[0].Id] = loyaltyLevel;

            equipmentPresetsAddedCount++;
        }
    }

    /**
     * Adjust plate / soft insert durability values
     * @param armor Armor item array to add mods into
     * @param itemDbDetails Armor items db template
     */
    protected void RandomiseArmorModDurability(List<Item> armor, TemplateItem itemDbDetails)
    {
        // Armor has no mods, nothing to randomise
        if (itemDbDetails.Properties.Slots == null)
        {
            return;
        }

        // Check for and adjust soft insert durability values
        var requiredSlots = itemDbDetails.Properties.Slots?.Where(slot => slot.Required ?? false).ToList();
        if ((requiredSlots?.Count ?? 0) > 1)
        {
            RandomiseArmorSoftInsertDurabilities(requiredSlots, armor);
        }

        // Check for and adjust plate durability values
        var plateSlots = itemDbDetails.Properties.Slots?.Where(slot => itemHelper.IsRemovablePlateSlot(slot.Name))
            .ToList();
        if ((plateSlots?.Count ?? 0) > 1)
        {
            RandomiseArmorInsertsDurabilities(plateSlots, armor);
        }
    }

    /**
     * Randomise the durability values of items on armor with a passed in slot
     * @param softInsertSlots Slots of items to randomise
     * @param armorItemAndMods Array of armor + inserts to get items from
     */
    protected void RandomiseArmorSoftInsertDurabilities(List<Slot> softInsertSlots, List<Item> armorItemAndMods)
    {
        foreach (var requiredSlot in softInsertSlots)
        {
            var modItemDbDetails = itemHelper.GetItem(requiredSlot.Props.Filters[0].Plate).Value;
            var durabilityValues = GetRandomisedArmorDurabilityValues(
                modItemDbDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );
            var plateTpl =
                requiredSlot.Props.Filters[0].Plate ??
                string.Empty; // "Plate" property appears to be the 'default' item for slot
            if (plateTpl == "")
            {
                // Some bsg plate properties are empty, skip mod
                continue;
            }

            // Find items mod to apply dura changes to
            var modItemToAdjust =
                armorItemAndMods.FirstOrDefault(mod => mod.SlotId.ToLower() == requiredSlot.Name.ToLower());

            itemHelper.AddUpdObjectToItem(modItemToAdjust);

            if (modItemToAdjust.Upd.Repairable == null)
            {
                modItemToAdjust.Upd.Repairable = new UpdRepairable()
                {
                    Durability = modItemDbDetails.Properties.MaxDurability,
                    MaxDurability = modItemDbDetails.Properties.MaxDurability
                };
            }

            modItemToAdjust.Upd.Repairable.Durability = durabilityValues.Durability;
            modItemToAdjust.Upd.Repairable.MaxDurability = durabilityValues.MaxDurability;

            // 25% chance to add shots to visor items when its below max durability
            if (randomUtil.GetChance100(25) &&
                modItemToAdjust.ParentId == BaseClasses.ARMORED_EQUIPMENT &&
                modItemToAdjust.SlotId == "mod_equipment_000" &&
                modItemToAdjust.Upd.Repairable.Durability < modItemDbDetails.Properties.MaxDurability)
            {
                // Is damaged
                modItemToAdjust.Upd.FaceShield = new UpdFaceShield() { Hits = randomUtil.GetInt(1, 3) };
            }
        }
    }

    /**
     * Randomise the durability values of plate items in armor
     * Has chance to remove plate
     * @param plateSlots Slots of items to randomise
     * @param armorItemAndMods Array of armor + inserts to get items from
     */
    protected void RandomiseArmorInsertsDurabilities(List<Slot> plateSlots, List<Item> armorItemAndMods)
    {
        foreach (var plateSlot in plateSlots)
        {
            var plateTpl = plateSlot.Props.Filters[0].Plate;
            if (plateTpl == null)
            {
                // Bsg data lacks a default plate, skip randomisng for this mod
                continue;
            }

            var armorWithMods = armorItemAndMods;

            var modItemDbDetails = itemHelper.GetItem(plateTpl).Value;

            // Chance to remove plate
            var plateExistsChance =
                traderConfig.Fence.ChancePlateExistsInArmorPercent[
                    modItemDbDetails?.Properties?.ArmorClass?.ToString() ?? "3"];
            if (!randomUtil.GetChance100(plateExistsChance))
            {
                // Remove plate from armor
                armorWithMods = armorItemAndMods.Where(item => item.SlotId.ToLower() != plateSlot.Name.ToLower())
                    .ToList();

                continue;
            }

            var durabilityValues = GetRandomisedArmorDurabilityValues(
                modItemDbDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );

            // Find items mod to apply dura changes to
            var modItemToAdjust = armorWithMods.FirstOrDefault(mod => mod.SlotId.ToLower() == plateSlot.Name.ToLower());

            if (modItemToAdjust == null)
            {
                logger.Warning(
                    $"Unable to randomise armor items {armorWithMods[0].Template} ${plateSlot.Name} slot as it cannot be found, skipping"
                );
                continue;
            }

            itemHelper.AddUpdObjectToItem(modItemToAdjust);

            if (modItemToAdjust?.Upd?.Repairable == null)
            {
                modItemToAdjust.Upd.Repairable = new UpdRepairable()
                {
                    Durability = modItemDbDetails.Properties.MaxDurability,
                    MaxDurability = modItemDbDetails.Properties.MaxDurability
                };
            }

            modItemToAdjust.Upd.Repairable.Durability = durabilityValues.Durability;
            modItemToAdjust.Upd.Repairable.MaxDurability = durabilityValues.MaxDurability;
        }
    }

    /**
     * Get stack size of a singular item (no mods)
     * @param itemDbDetails item being added to fence
     * @returns Stack size
     */
    protected int GetSingleItemStackCount(TemplateItem itemDbDetails)
    {
        MinMax? overrideValues;
        if (itemHelper.IsOfBaseclass(itemDbDetails.Id, BaseClasses.AMMO))
        {
            overrideValues = traderConfig.Fence.ItemStackSizeOverrideMinMax[itemDbDetails.Parent];
            if (overrideValues != null)
            {
                return randomUtil.GetInt((int)overrideValues.Min, (int)overrideValues.Max);
            }

            // No override, use stack max size from item db
            return itemDbDetails.Properties.StackMaxSize == 1
                ? 1
                : randomUtil.GetInt(
                    (int)itemDbDetails.Properties.StackMinRandom,
                    (int)itemDbDetails.Properties.StackMaxRandom
                );
        }

        // Check for override in config, use values if exists
        overrideValues = traderConfig.Fence.ItemStackSizeOverrideMinMax[itemDbDetails.Id];
        if (overrideValues != null)
        {
            return randomUtil.GetInt((int)overrideValues.Min, (int)overrideValues.Max);
        }

        // Check for parent override
        overrideValues = traderConfig.Fence.ItemStackSizeOverrideMinMax[itemDbDetails.Parent];
        if (overrideValues != null)
        {
            return randomUtil.GetInt((int)overrideValues.Min, (int)overrideValues.Max);
        }

        return 1;
    }

    /**
     * Remove parts of a weapon prior to being listed on flea
     * @param itemAndMods Weapon to remove parts from
     */
    protected void RemoveRandomModsOfItem(List<Item> itemAndMods)
    {
        // Items to be removed from inventory
        var toDelete = new List<string>();

        // Find mods to remove from item that could've been scavenged by other players in-raid
        foreach (var itemMod in itemAndMods)
        {
            if (PresetModItemWillBeRemoved(itemMod, toDelete))
            {
                // Skip if not an item
                var itemDbDetails = itemHelper.GetItem(itemMod.Template);
                if (!itemDbDetails.Key)
                {
                    continue;
                }

                // Remove item and its sub-items to prevent orphans
                toDelete.AddRange(itemHelper.FindAndReturnChildrenByItems(itemAndMods, itemMod.Id));
            }
        }

        // Reverse loop and remove items
        for (var index = itemAndMods.Count - 1; index >= 0; --index)
        {
            if (toDelete.Contains(itemAndMods[index].Id))
            {
                itemAndMods.Splice(index, 1);
            }
        }
    }

    /**
     * Roll % chance check to see if item should be removed
     * @param weaponMod Weapon mod being checked
     * @param itemsBeingDeleted Current list of items on weapon being deleted
     * @returns True if item will be removed
     */
    protected bool PresetModItemWillBeRemoved(Item weaponMod, List<string> itemsBeingDeleted)
    {
        var slotIdsThatCanFail = traderConfig.Fence.PresetSlotsToRemoveChancePercent;
        var removalChance = slotIdsThatCanFail[weaponMod.SlotId];
        if (removalChance is null or 0.0)
        {
            return false;
        }

        // Roll from 0 to 9999, then divide it by 100: 9999 =  99.99%
        var randomChance = randomUtil.GetInt(0, 9999) / 100;

        return removalChance > randomChance && !itemsBeingDeleted.Contains(weaponMod.Id);
    }

    /**
     * Randomise items' Upd properties e.g. med packs/weapons/armor
     * @param itemDetails Item being randomised
     * @param itemToAdjust Item being edited
     */
    protected void RandomiseItemUpdProperties(TemplateItem itemDetails, Item itemToAdjust)
    {
        if (itemDetails.Properties == null)
        {
            logger.Error(
                $"Item {itemDetails.Name} lacks a _props field, unable to randomise item: {itemToAdjust.Id}"
            );
            return;
        }

        // Randomise hp resource of med items
        if (itemDetails.Properties.MaxHpResource != null && (itemDetails.Properties.MaxHpResource ?? 0) > 0)
        {
            itemToAdjust.Upd.MedKit = new UpdMedKit()
                { HpResource = randomUtil.GetInt(1, (int)itemDetails.Properties.MaxHpResource) };
        }

        // Randomise armor durability
        if (
            (itemDetails.Parent == BaseClasses.ARMORED_EQUIPMENT ||
             itemDetails.Parent == BaseClasses.FACECOVER ||
             itemDetails.Parent == BaseClasses.ARMOR_PLATE) &&
            (itemDetails.Properties.MaxDurability ?? 0) > 0
        )
        {
            var values = GetRandomisedArmorDurabilityValues(
                itemDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );
            itemToAdjust.Upd.Repairable = new UpdRepairable()
                { Durability = values.Durability, MaxDurability = values.MaxDurability };

            return;
        }

        // Randomise Weapon durability
        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.WEAPON))
        {
            var weaponDurabilityLimits = traderConfig.Fence.WeaponDurabilityPercentMinMax;
            var maxDuraMin = (weaponDurabilityLimits.Max.Min / 100) * itemDetails.Properties.MaxDurability;
            var maxDuraMax = (weaponDurabilityLimits.Max.Max / 100) * itemDetails.Properties.MaxDurability;
            var chosenMaxDurability = randomUtil.GetInt((int)maxDuraMin, (int)maxDuraMax);

            var currentDuraMin = (weaponDurabilityLimits.Current.Min / 100) * itemDetails.Properties.MaxDurability;
            var currentDuraMax = (weaponDurabilityLimits.Current.Max / 100) * itemDetails.Properties.MaxDurability;
            var currentDurability = Math.Min(
                randomUtil.GetInt((int)currentDuraMin, (int)currentDuraMax),
                chosenMaxDurability
            );

            itemToAdjust.Upd.Repairable = new UpdRepairable
                { Durability = currentDurability, MaxDurability = chosenMaxDurability };

            return;
        }

        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.REPAIR_KITS))
        {
            itemToAdjust.Upd.RepairKit = new UpdRepairKit
            {
                Resource = randomUtil.GetInt(1, (int)itemDetails.Properties.MaxRepairResource),
            };

            return;
        }

        // Mechanical key + has limited uses
        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.KEY_MECHANICAL) &&
            (itemDetails.Properties.MaximumNumberOfUsage ?? 0) > 1)
        {
            itemToAdjust.Upd.Key = new UpdKey
            {
                NumberOfUsages = randomUtil.GetInt(0, (int)itemDetails.Properties.MaximumNumberOfUsage - 1),
            };

            return;
        }

        // Randomise items that use resources (e.g. fuel)
        if ((itemDetails.Properties.MaxResource ?? 0) > 0)
        {
            var resourceMax = itemDetails.Properties.MaxResource;
            var resourceCurrent = randomUtil.GetInt(1, (int)itemDetails.Properties.MaxResource);

            itemToAdjust.Upd.Resource = new UpdResource
                { Value = resourceMax - resourceCurrent, UnitsConsumed = resourceCurrent };
        }
    }

    /**
     * Generate a randomised current and max durabiltiy value for an armor item
     * @param itemDetails Item to create values for
     * @param equipmentDurabilityLimits Max durabiltiy percent min/max values
     * @returns Durability + MaxDurability values
     */
    protected UpdRepairable GetRandomisedArmorDurabilityValues(
        TemplateItem itemDetails,
        ItemDurabilityCurrentMax equipmentDurabilityLimits
    )
    {
        var maxDuraMin = (equipmentDurabilityLimits.Max.Min / 100) * itemDetails.Properties.MaxDurability;
        var maxDuraMax = (equipmentDurabilityLimits.Max.Max / 100) * itemDetails.Properties.MaxDurability;
        var chosenMaxDurability = randomUtil.GetInt((int)maxDuraMin, (int)maxDuraMax);

        var currentDuraMin = (equipmentDurabilityLimits.Current.Min / 100) * itemDetails.Properties.MaxDurability;
        var currentDuraMax = (equipmentDurabilityLimits.Current.Max / 100) * itemDetails.Properties.MaxDurability;
        var chosenCurrentDurability = Math.Min(
            randomUtil.GetInt((int)currentDuraMin, (int)currentDuraMax),
            chosenMaxDurability
        );

        return new UpdRepairable() { Durability = chosenCurrentDurability, MaxDurability = chosenMaxDurability };
    }

    /**
     * Construct item limit record to hold max and current item count
     * @param limits limits as defined in config
     * @returns record, key: item tplId, value: current/max item count allowed
     */
    protected Dictionary<string, (int current, int max)> InitItemLimitCounter(Dictionary<string, int> limits)
    {
        var itemTypeCounts = new Dictionary<string, (int current, int max)>();

        foreach (var x in limits.Keys)
        {
            itemTypeCounts[x] = new() { current = 0, max = limits[x] };
        }

        return itemTypeCounts;
    }

    /**
     * Get the next Update timestamp for fence
     * @returns future timestamp
     */
    public long GetNextFenceUpdateTimestamp()
    {
        var time = timeUtil.GetTimeStamp();
        var UpdateSeconds = GetFenceRefreshTime();
        return time + UpdateSeconds;
    }

    /**
     * Get fence refresh time in seconds
     * @returns Refresh time in seconds
     */
    protected int GetFenceRefreshTime()
    {
        var fence = traderConfig.UpdateTime.FirstOrDefault((x) => x.TraderId == Traders.FENCE).Seconds;

        return randomUtil.GetInt((int)fence.Min, (int)fence.Max);
    }

    /**
     * Get fence level the passed in profile has
     * @param pmcData Player profile
     * @returns FenceLevel object
     */
    public FenceLevel GetFenceInfo(PmcData pmcData)
    {
        var fenceSettings = databaseService.GetGlobals().Configuration.FenceSettings;
        var pmcFenceInfo = pmcData.TradersInfo[fenceSettings.FenceIdentifier];

        if (pmcFenceInfo == null)
        {
            return fenceSettings.Levels["0"];
        }

        var fenceLevels = fenceSettings.Levels.Keys.Select(int.Parse);
        var minLevel = fenceLevels.Min();
        var maxLevel = fenceLevels.Max();
        var pmcFenceLevel = Math.Floor((double)pmcFenceInfo.Standing);

        if (pmcFenceLevel < minLevel)
        {
            return fenceSettings.Levels[minLevel.ToString()];
        }

        if (pmcFenceLevel > maxLevel)
        {
            return fenceSettings.Levels[maxLevel.ToString()];
        }

        return fenceSettings.Levels[pmcFenceLevel.ToString()];
    }

    /**
     * Remove or lower stack size of an assort from fence by id
     * @param assortId assort id to adjust
     * @param buyCount Count of items bought
     */
    public void AmendOrRemoveFenceOffer(string assortId, int buyCount)
    {
        var isNormalAssort = true;
        var fenceAssortItem = fenceAssort.Items.FirstOrDefault((item) => item.Id == assortId);
        if (fenceAssortItem == null)
        {
            // Not in main assorts, check secondary section
            fenceAssortItem = fenceDiscountAssort.Items.FirstOrDefault((item) => item.Id == assortId);
            if (fenceAssortItem == null)
            {
                logger.Error(localisationService.GetText("fence-unable_to_find_offer_by_id", assortId));

                return;
            }

            isNormalAssort = false;
        }

        // Player wants to buy whole stack, delete stack
        if ((fenceAssortItem.Upd?.StackObjectsCount ?? 0) == buyCount)
        {
            DeleteOffer(assortId, isNormalAssort ? fenceAssort.Items : fenceDiscountAssort.Items);
            return;
        }

        // Adjust stack size
        fenceAssortItem.Upd.StackObjectsCount -= buyCount;
    }

    protected void DeleteOffer(string assortId, List<Item> assorts)
    {
        // Assort could have child items, remove those too
        var itemWithChildrenToRemove = itemHelper.FindAndReturnChildrenAsItems(assorts, assortId);
        foreach (var itemToRemove in itemWithChildrenToRemove)
        {
            var indexToRemove = assorts.FindIndex((item) => item.Id == itemToRemove.Id);

            // No offer found in main assort, check discount items
            if (indexToRemove == -1)
            {
                indexToRemove = fenceDiscountAssort.Items.FindIndex((item) => item.Id == itemToRemove.Id);
                fenceDiscountAssort.Items.Splice(indexToRemove, 1);

                if (indexToRemove == -1)
                {
                    logger.Warning(
                        $"unable to remove fence assort item: {itemToRemove.Id} tpl: {itemToRemove.Template}"
                    );
                }

                return;
            }

            // Remove offer from assort
            assorts.Splice(indexToRemove, 1);
        }
    }
}
