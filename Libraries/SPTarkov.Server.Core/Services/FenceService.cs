using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Fence;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services;

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
    ICloner _cloner
)
{
    /// <summary>
    ///     Desired baseline counts - Hydrated on initial assort generation as part of generateFenceAssorts()
    /// </summary>
    protected FenceAssortGenerationValues desiredAssortCounts;

    /// <summary>
    ///     Main assorts you see at all rep levels
    /// </summary>
    protected TraderAssort? fenceAssort;

    /// <summary>
    ///     Assorts shown on a separate tab when you max out fence rep
    /// </summary>
    protected TraderAssort? fenceDiscountAssort;

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

    /// <summary>
    ///     Time when some items in assort will be replaced
    /// </summary>
    protected long nextPartialRefreshTimestamp;

    protected TraderConfig traderConfig = configServer.GetConfig<TraderConfig>();

    /// <summary>
    ///     Replace main fence assort with new assort
    /// </summary>
    /// <param name="assort"> New assorts to replace old with </param>
    public void SetFenceAssort(TraderAssort assort)
    {
        fenceAssort = assort;
    }

    /// <summary>
    ///     Replace discount fence assort with new assort
    /// </summary>
    /// <param name="discountAssort"> New assorts to replace old with </param>
    public void SetFenceDiscountAssort(TraderAssort discountAssort)
    {
        fenceDiscountAssort = discountAssort;
    }

    /// <summary>
    ///     Get main fence assort
    /// </summary>
    /// <returns> TraderAssort </returns>
    public TraderAssort? GetMainFenceAssort()
    {
        return fenceAssort;
    }

    /// <summary>
    ///     Get discount fence assort
    /// </summary>
    /// <returns> TraderAssort </returns>
    /// @return ITraderAssort
    public TraderAssort? GetDiscountFenceAssort()
    {
        return fenceDiscountAssort;
    }

    /// <summary>
    ///     Get assorts player can purchase <br />
    ///     Adjust prices based on fence level of player
    /// </summary>
    /// <param name="pmcProfile"> Player profile </param>
    /// <returns> TraderAssort </returns>
    public TraderAssort GetFenceAssorts(PmcData pmcProfile)
    {
        if (traderConfig.Fence.RegenerateAssortsOnRefresh)
        // Using base assorts made earlier, do some alterations and store in fenceAssort
        {
            GenerateFenceAssorts();
        }

        // Clone assorts so we can adjust prices before sending to client
        var assort = _cloner.Clone(fenceAssort);
        AdjustAssortItemPricesByConfigMultiplier(assort, 1, traderConfig.Fence.PresetPriceMult);

        // merge normal fence assorts + discount assorts if player standing is large enough
        if (pmcProfile.TradersInfo[Traders.FENCE].Standing >= 6)
        {
            var discountAssort = _cloner.Clone(fenceDiscountAssort);
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

    /// <summary>
    ///     Adds to fence assort a single item (with its children)
    /// </summary>
    /// <param name="items"> The items to add with all its children </param>
    /// <param name="mainItem"> The most parent item of the array </param>
    public void AddItemsToFenceAssort(List<Item> items, Item mainItem)
    {
        // HUGE THANKS TO LACYWAY AND LEAVES FOR PROVIDING THIS SOLUTION FOR SPT TO IMPLEMENT!!
        // Copy the item and its children
        var clonedItems = _cloner.Clone(
            itemHelper.FindAndReturnChildrenAsItems(items, mainItem.Id)
        );
        // I BLAME LACY FOR THIS ISSUE, I SPENT HOURS FIXING IT /s
        // i think on node the one with hideout usually came first
        var root = clonedItems.FirstOrDefault(x => x.SlotId == "hideout");

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

        var createAssort = new CreateFenceAssortsResult
        {
            SptItems = [],
            BarterScheme = new Dictionary<string, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<string, int>(),
        };
        createAssort.BarterScheme[root.Id] =
        [
            [new BarterScheme { Count = cost, Template = Money.ROUBLES }],
        ];
        createAssort.SptItems.Add(clonedItems);
        createAssort.LoyalLevelItems[root.Id] = 1;

        UpdateFenceAssorts(createAssort, fenceAssort);
    }

    /// <summary>
    ///     Calculates the overall price for an item (with all its children)
    /// </summary>
    /// <param name="itemTpl"> The item tpl to calculate the fence price for </param>
    /// <param name="items"> The items (with its children) to calculate fence price for </param>
    /// <returns> Price of the item for Fence </returns>
    public double? GetItemPrice(string itemTpl, List<Item> items)
    {
        return itemHelper.IsOfBaseclass(itemTpl, BaseClasses.AMMO_BOX)
            ? GetAmmoBoxPrice(items) * traderConfig.Fence.ItemPriceMult
            : handbookHelper.GetTemplatePrice(itemTpl) * traderConfig.Fence.ItemPriceMult;
    }

    /// <summary>
    ///     Calculate the overall price for an ammo box, where only one item is
    ///     the ammo box itself and every other items are the bullets in that box
    /// </summary>
    /// <param name="items"> The ammo box (and all its children ammo items) </param>
    /// <returns> The price of the ammo box </returns>
    protected double? GetAmmoBoxPrice(List<Item> items)
    {
        double? total = 0D;
        foreach (var item in items)
        {
            if (itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO))
            {
                total +=
                    handbookHelper.GetTemplatePrice(item.Template)
                    * (item.Upd?.StackObjectsCount ?? 1);
            }
        }

        return total;
    }

    /// <summary>
    ///     Adjust all items contained inside an assort by a multiplier
    /// </summary>
    /// <param name="assort"> (clone) Assort that contains items with prices to adjust </param>
    /// <param name="itemMultiplier"> Multiplier to use on items </param>
    /// <param name="presetMultiplier"> Multiplier to use on presets </param>
    protected void AdjustAssortItemPricesByConfigMultiplier(
        TraderAssort assort,
        double itemMultiplier,
        double presetMultiplier
    )
    {
        // Only get root items
        foreach (var item in assort.Items.Where(x => x.SlotId is "hideout"))
        {
            AdjustItemPriceByModifier(item, assort, itemMultiplier, presetMultiplier);
        }
    }

    /// <summary>
    ///     Merge two trader assort files together
    /// </summary>
    /// <param name="firstAssort"> Assort #1 </param>
    /// <param name="secondAssort"> Assort #2 </param>
    /// <returns> Merged assort </returns>
    // TODO: can be moved to a helper?
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

    /// <summary>
    ///     Adjust assorts price by a modifier
    /// </summary>
    /// <param name="item"> Assort item details</param>
    /// <param name="assort"> Assort to be modified </param>
    /// <param name="modifier"> Value to multiply item price by </param>
    /// <param name="presetModifier"> Value to multiply preset price by </param>
    protected void AdjustItemPriceByModifier(
        Item item,
        TraderAssort assort,
        double modifier,
        double presetModifier
    )
    {
        if (assort?.BarterScheme is null)
        {
            logger.Warning(
                $"Unable to adjust item: {item.Id} on assort as it lacks a barterScheme object"
            );

            return;
        }

        // Is preset
        if (item.Upd?.SptPresetId != null)
        {
            if (assort.BarterScheme.TryGetValue(item.Id, out var barterSchemeForPreset))
            {
                barterSchemeForPreset[0][0].Count *= presetModifier;
            }

            return;
        }

        // Normal item
        if (assort.BarterScheme.TryGetValue(item.Id, out var barterScheme))
        {
            barterScheme[0][0].Count *= modifier;
        }
        else
        {
            logger.Warning(
                $"adjustItemPriceByModifier() - no action taken for item: {item.Template}"
            );
        }
    }

    /// <summary>
    ///     Get fence assorts with no price adjustments based on fence rep
    /// </summary>
    /// <returns> TraderAssort </returns>
    public TraderAssort GetRawFenceAssorts()
    {
        return MergeAssorts(_cloner.Clone(fenceAssort), _cloner.Clone(fenceDiscountAssort));
    }

    /// <summary>
    ///     Does fence need to perform a partial refresh because its passed the refresh timer defined in trader.json
    /// </summary>
    /// <returns> True if it needs a partial refresh </returns>
    public bool NeedsPartialRefresh()
    {
        return timeUtil.GetTimeStamp() > nextPartialRefreshTimestamp;
    }

    /// <summary>
    ///     Replace a percentage of fence assorts with freshly generated items
    /// </summary>
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
            fenceDiscountAssort.BarterScheme[barterItemKey] = newDiscountItems.BarterScheme[
                barterItemKey
            ];
        }

        // Add loyalty items to fence discount assorts loyalty object
        foreach (var loyaltyItemKey in newDiscountItems.LoyalLevelItems.Keys)
        {
            fenceDiscountAssort.LoyalLevelItems[loyaltyItemKey] = newDiscountItems.LoyalLevelItems[
                loyaltyItemKey
            ];
        }

        // Reset the clock
        IncrementPartialRefreshTime();
    }

    /// <summary>
    ///     Handle the process of folding new assorts into existing assorts, when a new assort exists already, increment its StackObjectsCount instead
    /// </summary>
    /// <param name="newFenceAssorts"> Assorts to fold into existing fence assorts </param>
    /// <param name="existingFenceAssorts"> Current fence assorts, new assorts will be added to </param>
    protected void UpdateFenceAssorts(
        CreateFenceAssortsResult newFenceAssorts,
        TraderAssort existingFenceAssorts
    )
    {
        foreach (var itemWithChildren in newFenceAssorts.SptItems)
        {
            // Find the root item
            var newRootItem = itemWithChildren.FirstOrDefault(item => item.SlotId == "hideout");
            if (newRootItem == null)
            {
                var firstItem = itemWithChildren.FirstOrDefault(x => x != null);
                logger.Error(
                    $"Unable to process fence assort as root item is missing, {firstItem?.Template}, skipping"
                );
                continue;
            }

            // Find a matching root item with same tpl in existing assort
            var existingRootItem = existingFenceAssorts.Items.FirstOrDefault(item =>
                item.Template == newRootItem.Template && item.SlotId == "hideout"
            );

            // Check if same type of item exists + its on list of item types to always stack
            if (existingRootItem != null && ItemInPreventDupeCategoryList(newRootItem.Template))
            {
                var existingFullItemTree = itemHelper.FindAndReturnChildrenAsItems(
                    existingFenceAssorts.Items,
                    existingRootItem.Id
                );
                if (
                    itemHelper.IsSameItems(
                        itemWithChildren,
                        existingFullItemTree,
                        fenceItemUpdCompareProperties
                    )
                )
                {
                    // Guard against a missing stack count
                    if (existingRootItem.Upd?.StackObjectsCount == null)
                    {
                        existingRootItem.Upd.StackObjectsCount = 1;
                    }

                    // Merge new items count into existing, dont add new loyalty/barter data as it already exists
                    existingRootItem.Upd.StackObjectsCount +=
                        newRootItem?.Upd?.StackObjectsCount ?? 1;

                    continue;
                }
            }

            // if the Upd doesnt exist just initialize it
            if (newRootItem.Upd == null)
            {
                newRootItem.Upd = new Upd { StackObjectsCount = 1 };
            }

            // New assort to be added to existing assorts
            existingFenceAssorts.Items.AddRange(itemWithChildren);
            existingFenceAssorts.BarterScheme[newRootItem.Id] = newFenceAssorts.BarterScheme[
                newRootItem.Id
            ];
            existingFenceAssorts.LoyalLevelItems[newRootItem.Id] = newFenceAssorts.LoyalLevelItems[
                newRootItem.Id
            ];
        }
    }

    /// <summary>
    ///     Increment fence next refresh timestamp by current timestamp + partialRefreshTimeSeconds from config
    /// </summary>
    protected void IncrementPartialRefreshTime()
    {
        nextPartialRefreshTimestamp =
            timeUtil.GetTimeStamp() + traderConfig.Fence.PartialRefreshTimeSeconds;
    }

    /// <summary>
    ///     Get values that will hydrate the passed in assorts back to the desired counts
    /// </summary>
    /// <param name="assortItems"> Current assorts after items have been removed </param>
    /// <param name="generationValues"> Base counts assorts should be adjusted to </param>
    /// <returns> GenerationAssortValues object with adjustments needed to reach desired state </returns>
    protected GenerationAssortValues GetItemCountsToGenerate(
        List<Item> assortItems,
        GenerationAssortValues generationValues
    )
    {
        var allRootItems = assortItems.Where(item => item.SlotId == "hideout");
        var rootPresetItems = allRootItems.Where(item => item?.Upd?.SptPresetId != null);

        // Get count of weapons
        var currentWeaponPresetCount = rootPresetItems.Aggregate(
            0,
            (count, item) =>
                itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON) ? count + 1 : count
        );

        // Get count of equipment
        var currentEquipmentPresetCount = rootPresetItems.Aggregate(
            0,
            (count, item) => itemHelper.ArmorItemCanHoldMods(item.Template) ? count + 1 : count
        );

        // Normal item count is total count minus weapon + armor count
        var nonPresetItemAssortCount =
            allRootItems.Count() - (currentWeaponPresetCount + currentEquipmentPresetCount);

        // Get counts of items to generate, never var values fall below 0
        var itemCountToGenerate = Math.Max(
            generationValues.Item.Value - nonPresetItemAssortCount,
            0
        );
        var weaponCountToGenerate = Math.Max(
            generationValues.WeaponPreset.Value - currentWeaponPresetCount,
            0
        );
        var equipmentCountToGenerate = Math.Max(
            generationValues.EquipmentPreset.Value - currentEquipmentPresetCount,
            0
        );

        return new GenerationAssortValues
        {
            Item = itemCountToGenerate,
            WeaponPreset = weaponCountToGenerate,
            EquipmentPreset = equipmentCountToGenerate,
        };
    }

    /// <summary>
    ///     Delete desired number of items from assort (including children)
    /// </summary>
    /// <param name="itemCountToReplace"> Number of items to replace </param>
    /// <param name="assort"> Assort to adjust </param>
    protected void DeleteRandomAssorts(int itemCountToReplace, TraderAssort assort)
    {
        if (assort?.Items?.Count > 0)
        {
            var rootItems = assort.Items.Where(item => item.SlotId == "hideout").ToList();
            for (var index = 0; index < itemCountToReplace; index++)
            {
                RemoveRandomItemFromAssorts(assort, rootItems);
            }
        }
    }

    /// <summary>
    ///     Choose an item at random and remove it + mods from assorts
    /// </summary>
    /// <param name="assort"> Trader assort to remove item from </param>
    /// <param name="rootItems"> Pool of root items to pick from to remove </param>
    protected void RemoveRandomItemFromAssorts(TraderAssort assort, List<Item> rootItems)
    {
        // Pick a random root item to remove from Fence
        var rootItemToAdjust = randomUtil.GetArrayValue(rootItems);

        // Items added by mods may not have a Upd object, assume item stack size is 1
        var stackSize = rootItemToAdjust.Upd?.StackObjectsCount ?? 1;

        // Get a random count of the chosen item to remove if its > 1
        var itemCountToRemove = randomUtil.GetDouble(1, stackSize);

        // Check if we're removing all or just part of the item
        var isEntireStackToBeRemoved = Math.Abs(itemCountToRemove - stackSize) < 0.1;

        // Partial stack reduction
        if (!isEntireStackToBeRemoved)
        {
            if (rootItemToAdjust.Upd == null)
            {
                logger.Warning(
                    $"Fence Item: {rootItemToAdjust.Template} lacks a Upd object, adding"
                );
                rootItemToAdjust.Upd = new Upd();
            }

            // Reduce stack to at smallest, 1
            rootItemToAdjust.Upd.StackObjectsCount -= Math.Max(1, itemCountToRemove);

            return;
        }

        // Remove item + child mods (if any)
        var itemWithChildren = itemHelper.FindAndReturnChildrenAsItems(
            assort.Items,
            rootItemToAdjust.Id
        );
        foreach (var itemToDelete in itemWithChildren)
        // Delete item from assort items array
        {
            assort.Items.Remove(itemToDelete);
        }

        // Need to remove item from all areas of trader assort
        // delete assort.barter_scheme[rootItemToAdjust._id];
        // delete assort.loyal_level_items[rootItemToAdjust._id];
        assort.BarterScheme.Remove(rootItemToAdjust.Id);
        assort.LoyalLevelItems.Remove(rootItemToAdjust.Id);
    }

    /// <summary>
    ///     Get an integer rounded count of items to replace based on percentage from traderConfig value
    /// </summary>
    /// <param name="totalItemCount"> Total item count </param>
    /// <returns> Rounded int of items to replace </returns>
    protected int GetCountOfItemsToReplace(int totalItemCount)
    {
        return (int)
            Math.Round(totalItemCount * (traderConfig.Fence.PartialRefreshChangePercent / 100));
    }

    /// <summary>
    ///     Get the count of items fence offers
    /// </summary>
    /// <returns> Number </returns>
    public int GetOfferCount()
    {
        if ((fenceAssort?.Items?.Count ?? 0) == 0)
        {
            return 0;
        }

        return fenceAssort.Items.Count;
    }

    /// <summary>
    ///     Create trader assorts for fence and store in fenceService cache
    ///     Uses fence base cache generation server start as a base
    /// </summary>
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

    /// <summary>
    ///     Convert the intermediary assort data generated into format client can process
    /// </summary>
    /// <param name="intermediaryAssorts"> Generated assorts that will be converted </param>
    /// <returns> TraderAssort in the correct data format for Fence </returns>
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

    /// <summary>
    ///     Create object that contains calculated fence assort item values to make based on config.
    ///     Stored in desiredAssortCounts
    /// </summary>
    protected void CreateInitialFenceAssortGenerationValues()
    {
        var result = new FenceAssortGenerationValues
        {
            Normal = new GenerationAssortValues
            {
                Item = 0,
                WeaponPreset = 0,
                EquipmentPreset = 0,
            },
            Discount = new GenerationAssortValues
            {
                Item = 0,
                WeaponPreset = 0,
                EquipmentPreset = 0,
            },
        };

        result.Normal.Item = traderConfig.Fence.AssortSize;

        result.Normal.WeaponPreset = randomUtil.GetInt(
            traderConfig.Fence.WeaponPresetMinMax.Min,
            traderConfig.Fence.WeaponPresetMinMax.Max
        );

        result.Normal.EquipmentPreset = randomUtil.GetInt(
            traderConfig.Fence.EquipmentPresetMinMax.Min,
            traderConfig.Fence.EquipmentPresetMinMax.Max
        );

        result.Discount.Item = traderConfig.Fence.DiscountOptions.AssortSize;

        result.Discount.WeaponPreset = randomUtil.GetInt(
            traderConfig.Fence.DiscountOptions.WeaponPresetMinMax.Min,
            traderConfig.Fence.DiscountOptions.WeaponPresetMinMax.Max
        );

        result.Discount.EquipmentPreset = randomUtil.GetInt(
            traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax.Min,
            traderConfig.Fence.DiscountOptions.EquipmentPresetMinMax.Max
        );

        desiredAssortCounts = result;
    }

    /// <summary>
    ///     Create skeleton to hold assort items
    /// </summary>
    /// <returns> TraderAssort object </returns>
    protected TraderAssort CreateFenceAssortSkeleton()
    {
        return new TraderAssort
        {
            Items = [],
            BarterScheme = new Dictionary<string, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<string, int>(),
            NextResupply = GetNextFenceUpdateTimestamp(),
        };
    }

    /// <summary>
    ///     Hydrate assorts parameter object with generated assorts
    /// </summary>
    /// <param name="itemCounts"> Number of items to generate per type (Item, WeaponPreset, EquipmentPreset) </param>
    /// <param name="loyaltyLevel"> Loyalty level to set new item to </param>
    /// <returns> CreateFenceAssortResult object </returns>
    protected CreateFenceAssortsResult CreateAssorts(
        GenerationAssortValues itemCounts,
        int loyaltyLevel
    )
    {
        var result = new CreateFenceAssortsResult
        {
            SptItems = [],
            BarterScheme = new Dictionary<string, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<string, int>(),
        };

        var baseFenceAssortClone = _cloner.Clone(databaseService.GetTrader(Traders.FENCE).Assort);
        var itemTypeLimitCounts = InitItemLimitCounter(traderConfig.Fence.ItemTypeLimits);

        if (itemCounts.Item > 0)
        {
            AddItemAssorts(
                itemCounts.Item,
                result,
                baseFenceAssortClone,
                itemTypeLimitCounts,
                loyaltyLevel
            );
        }

        if (itemCounts.WeaponPreset > 0 || itemCounts.EquipmentPreset > 0)
        // Add presets
        {
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

    /// <summary>
    ///     Add item assorts to existing assort data
    /// </summary>
    /// <param name="assortCount"> Number to add </param>
    /// <param name="assorts"> Data to add to </param>
    /// <param name="baseFenceAssortClone"> Base data to draw from </param>
    /// <param name="itemTypeLimits"> Item limits per base class </param>
    /// <param name="loyaltyLevel"> Loyalty level to set new item to </param>
    protected void AddItemAssorts(
        int? assortCount,
        CreateFenceAssortsResult assorts,
        TraderAssort baseFenceAssortClone,
        Dictionary<string, (int current, int max)> itemTypeLimits,
        int loyaltyLevel
    )
    {
        var priceLimits = traderConfig.Fence.ItemCategoryRoublePriceLimit;
        var assortRootItems = baseFenceAssortClone
            .Items.Where(item =>
                string.Equals(item.ParentId, "hideout", StringComparison.OrdinalIgnoreCase)
                && item.Upd?.SptPresetId == null
            )
            .ToList();
        if (assortRootItems.Count == 0)
        {
            logger.Error(
                "Unable to add assorts to Fence as no root items exist in items being added"
            );
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

            // Filter out root items from pool
            var childItemsAndSingleRoot = baseFenceAssortClone
                .Items.Where(item =>
                    !string.Equals(item.ParentId, "hideout", StringComparison.Ordinal)
                    || string.Equals(item.Id, chosenBaseAssortRoot.Id, StringComparison.Ordinal)
                )
                .ToList();

            var desiredAssortItemAndChildrenClone = _cloner.Clone(
                itemHelper.FindAndReturnChildrenAsItems(
                    childItemsAndSingleRoot,
                    chosenBaseAssortRoot.Id
                )
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

            if (
                priceLimits.ContainsKey(itemDbDetails.Parent)
                && price > priceLimits[itemDbDetails.Parent]
            )
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
            desiredAssortItemAndChildrenClone = itemHelper.ReplaceIDs(
                _cloner.Clone(desiredAssortItemAndChildrenClone)
            );
            itemHelper.RemapRootItemId(desiredAssortItemAndChildrenClone);

            var rootItemBeingAdded = desiredAssortItemAndChildrenClone[0];

            // Set stack size based on possible overrides, e.g. ammos, otherwise set to 1
            rootItemBeingAdded.Upd.StackObjectsCount = GetSingleItemStackCount(itemDbDetails);

            // Only randomise Upd values for single
            var isSingleStack =
                Math.Abs((rootItemBeingAdded.Upd?.StackObjectsCount ?? 0) - 1) < 0.1;
            if (isSingleStack)
            {
                RandomiseItemUpdProperties(itemDbDetails, rootItemBeingAdded);
            }

            // Skip items already in the assort if it exists in the prevent duplicate list
            var existingItemThatMatches = GetMatchingItem(
                rootItemBeingAdded,
                itemDbDetails,
                assorts.SptItems
            );
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

            assorts.BarterScheme[rootItemBeingAdded.Id] = _cloner.Clone(
                baseFenceAssortClone.BarterScheme[chosenBaseAssortRoot.Id]
            );

            // Only adjust item price by quality for solo items, never multi-stack
            if (isSingleStack)
            {
                AdjustItemPriceByQuality(assorts.BarterScheme, rootItemBeingAdded, itemDbDetails);
            }

            assorts.LoyalLevelItems[rootItemBeingAdded.Id] = loyaltyLevel;
        }
    }

    /// <summary>
    ///     Find an assort item that matches the first parameter, also matches based on Upd properties
    ///     e.g. salewa hp resource units left
    /// </summary>
    /// <param name="rootItemBeingAdded"> item to look for a match against </param>
    /// <param name="itemDbDetails"> DB details of matching item </param>
    /// <param name="itemsWithChildren"> Items to search through </param>
    /// <returns> Matching assort item </returns>
    protected Item? GetMatchingItem(
        Item rootItemBeingAdded,
        TemplateItem itemDbDetails,
        List<List<Item>> itemsWithChildren
    )
    {
        // Get matching root items
        var matchingItems = itemsWithChildren
            .Where(itemWithChildren =>
                itemWithChildren.FirstOrDefault(item =>
                    item.Template == rootItemBeingAdded.Template
                    && string.Equals(item.ParentId, "hideout", StringComparison.OrdinalIgnoreCase)
                ) != null
            )
            .SelectMany(i => i)
            .ToList();
        if (matchingItems.Count == 0)
        // Nothing matches by tpl and is root item, exit early
        {
            return null;
        }

        var isMedical = itemHelper.IsOfBaseclasses(
            rootItemBeingAdded.Template,
            [BaseClasses.MEDICAL, BaseClasses.MEDKIT]
        );
        var isGearAndHasSlots =
            itemHelper.IsOfBaseclasses(
                rootItemBeingAdded.Template,
                [BaseClasses.ARMORED_EQUIPMENT, BaseClasses.SEARCHABLE_ITEM]
            )
            && (itemDbDetails.Properties.Slots?.Count ?? 0) > 0;

        // Only one match and its not medical or armored gear
        if (matchingItems.Count == 1 && !(isMedical || isGearAndHasSlots))
        {
            return matchingItems[0];
        }

        // Items have sub properties that need to be checked against
        foreach (var item in matchingItems)
        {
            if (
                isMedical
                && rootItemBeingAdded.Upd?.MedKit?.HpResource == item.Upd?.MedKit?.HpResource
            )
            // e.g. bandages with multiple use
            // Both undefined === both max resoruce left
            {
                return item;
            }

            // Armors/helmets etc
            if (
                isGearAndHasSlots
                && rootItemBeingAdded.Upd.Repairable?.Durability == item.Upd.Repairable?.Durability
                && rootItemBeingAdded.Upd.Repairable?.MaxDurability
                    == item.Upd.Repairable?.MaxDurability
            )
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    ///     Should this item be forced into only 1 stack on fence
    /// </summary>
    /// <param name="existingItem"> Existing item from fence assort </param>
    /// <param name="itemDbDetails"> Item we want to add DB details </param>
    /// <returns> True item should be force stacked </returns>
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

    /// <summary>
    ///     Adjust price of item based on what is left to buy (resource/uses left)
    /// </summary>
    /// <param name="barterSchemes"> All barter scheme for item having price adjusted </param>
    /// <param name="itemRoot"> Root item having price adjusted </param>
    /// <param name="itemTemplate"> DB template of item </param>
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

            var multiplier = current / itemTotalMax;

            // Multiply item cost by desired multiplier
            var basePrice = barterSchemes[itemRoot.Id][0][0].Count;
            barterSchemes[itemRoot.Id][0][0].Count = Math.Round(basePrice.Value * multiplier.Value);

            return;
        }

        // Adjust price based on durability
        if (
            itemRoot.Upd?.Repairable != null
            || itemHelper.IsOfBaseclass(itemRoot.Template, BaseClasses.KEY_MECHANICAL)
        )
        {
            var itemQualityModifier = itemHelper.GetItemQualityModifier(itemRoot);
            var basePrice = barterSchemes[itemRoot.Id][0][0].Count;
            barterSchemes[itemRoot.Id][0][0].Count = Math.Round(
                (double)basePrice * itemQualityModifier
            );
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

    /// <summary>
    ///     Find presets in base fence assort and add desired number to 'assorts' parameter
    /// </summary>
    /// <param name="desiredWeaponPresetsCount"> How many WeaponPresets to add </param>
    /// <param name="desiredEquipmentPresetsCount"> How many WeaponPresets to add </param>
    /// <param name="assorts"> Assorts to add preset to </param>
    /// <param name="baseFenceAssort"> Base data to draw from </param>
    /// <param name="loyaltyLevel"> Loyalty level to set new presets to </param>
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
            var weaponPresetRootItems = baseFenceAssort.Items.Where(item =>
                item.Upd?.SptPresetId != null
                && itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON)
            );
            while (weaponPresetsAddedCount < desiredWeaponPresetsCount)
            {
                var randomPresetRoot = randomUtil.GetArrayValue(weaponPresetRootItems);
                if (traderConfig.Fence.Blacklist.Contains(randomPresetRoot.Template))
                {
                    continue;
                }

                var rootItemDb = itemHelper.GetItem(randomPresetRoot.Template).Value;

                var presetWithChildrenClone = _cloner.Clone(
                    itemHelper.FindAndReturnChildrenAsItems(
                        baseFenceAssort.Items,
                        randomPresetRoot.Id
                    )
                );

                RandomiseItemUpdProperties(rootItemDb, presetWithChildrenClone[0]);

                RemoveRandomModsOfItem(presetWithChildrenClone);

                // Check chosen item is below price cap
                var itemPrice =
                    handbookHelper.GetTemplatePriceForItems(presetWithChildrenClone)
                    * itemHelper.GetItemQualityModifierForItems(presetWithChildrenClone);
                if (
                    traderConfig.Fence.ItemCategoryRoublePriceLimit.TryGetValue(
                        rootItemDb.Parent,
                        out var priceLimitRouble
                    )
                )
                {
                    if (itemPrice > priceLimitRouble)
                    // Too expensive, try again
                    {
                        continue;
                    }
                }

                // MUST randomise Ids as its possible to add the same base fence assort twice = duplicate IDs = dead client
                itemHelper.ReparentItemAndChildren(
                    presetWithChildrenClone[0],
                    presetWithChildrenClone
                );
                itemHelper.RemapRootItemId(presetWithChildrenClone);

                // Remapping IDs causes parentid to be altered
                presetWithChildrenClone[0].ParentId = "hideout";

                assorts.SptItems.Add(presetWithChildrenClone);

                // Set assort price
                // Must be careful to use correct id as the item has had its IDs regenerated
                assorts.BarterScheme[presetWithChildrenClone[0].Id] =
                [
                    [new BarterScheme { Template = Money.ROUBLES, Count = Math.Round(itemPrice) }],
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

        var equipmentPresetRootItems = baseFenceAssort.Items.Where(item =>
            item.Upd?.SptPresetId != null && itemHelper.ArmorItemCanHoldMods(item.Template)
        );
        while (equipmentPresetsAddedCount < desiredEquipmentPresetsCount)
        {
            var randomPresetRoot = randomUtil.GetArrayValue(equipmentPresetRootItems);
            var rootItemDb = itemHelper.GetItem(randomPresetRoot.Template).Value;

            var presetWithChildrenClone = _cloner.Clone(
                itemHelper.FindAndReturnChildrenAsItems(baseFenceAssort.Items, randomPresetRoot.Id)
            );

            // Need to add mods to armors so they dont show as red in the trade screen
            if (itemHelper.ItemRequiresSoftInserts(randomPresetRoot.Template))
            {
                RandomiseArmorModDurability(presetWithChildrenClone, rootItemDb);
            }

            RemoveRandomModsOfItem(presetWithChildrenClone);

            // Check chosen item is below price cap
            var priceLimitRouble = traderConfig.Fence.ItemCategoryRoublePriceLimit[
                rootItemDb.Parent
            ];
            var itemPrice =
                handbookHelper.GetTemplatePriceForItems(presetWithChildrenClone)
                * itemHelper.GetItemQualityModifierForItems(presetWithChildrenClone);
            if (priceLimitRouble != null)
            {
                if (itemPrice > priceLimitRouble)
                // Too expensive, try again
                {
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
                [new BarterScheme { Template = Money.ROUBLES, Count = Math.Round(itemPrice) }],
            ];
            assorts.LoyalLevelItems[presetWithChildrenClone[0].Id] = loyaltyLevel;

            equipmentPresetsAddedCount++;
        }
    }

    /// <summary>
    ///     Adjust plate / soft insert durability values
    /// </summary>
    /// <param name="armor"> Armor item array to add mods into </param>
    /// <param name="itemDbDetails"> Armor items db template </param>
    protected void RandomiseArmorModDurability(List<Item> armor, TemplateItem itemDbDetails)
    {
        // Armor has no mods, nothing to randomise
        if (itemDbDetails.Properties.Slots == null)
        {
            return;
        }

        // Check for and adjust soft insert durability values
        var requiredSlots = itemDbDetails
            .Properties.Slots?.Where(slot => slot.Required ?? false)
            .ToList();
        if ((requiredSlots?.Count ?? 0) > 1)
        {
            RandomiseArmorSoftInsertDurabilities(requiredSlots, armor);
        }

        // Check for and adjust plate durability values
        var plateSlots = itemDbDetails
            .Properties.Slots?.Where(slot => itemHelper.IsRemovablePlateSlot(slot.Name))
            .ToList();
        if ((plateSlots?.Count ?? 0) > 1)
        {
            RandomiseArmorInsertsDurabilities(plateSlots, armor);
        }
    }

    /// <summary>
    ///     Randomise the durability values of items on armor with a passed in slot
    /// </summary>
    /// <param name="softInsertSlots"> Slots of items to randomise </param>
    /// <param name="armorItemAndMods"> Array of armor + inserts to get items from </param>
    protected void RandomiseArmorSoftInsertDurabilities(
        List<Slot> softInsertSlots,
        List<Item> armorItemAndMods
    )
    {
        foreach (var requiredSlot in softInsertSlots)
        {
            var modItemDbDetails = itemHelper.GetItem(requiredSlot.Props.Filters[0].Plate).Value;

            var durabilityValues = GetRandomisedArmorDurabilityValues(
                modItemDbDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );
            var plateTpl = requiredSlot.Props.Filters[0].Plate ?? string.Empty; // "Plate" property appears to be the 'default' item for slot
            if (plateTpl == "")
            // Some bsg plate properties are empty, skip mod
            {
                continue;
            }

            // Find items mod to apply dura changes to
            var modItemToAdjust = armorItemAndMods.FirstOrDefault(mod =>
                string.Equals(
                    mod.SlotId,
                    requiredSlot.Name.ToLower(),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            itemHelper.AddUpdObjectToItem(modItemToAdjust);

            if (modItemToAdjust.Upd.Repairable == null)
            {
                modItemToAdjust.Upd.Repairable = new UpdRepairable
                {
                    Durability = modItemDbDetails.Properties.MaxDurability,
                    MaxDurability = modItemDbDetails.Properties.MaxDurability,
                };
            }

            modItemToAdjust.Upd.Repairable.Durability = durabilityValues.Durability;
            modItemToAdjust.Upd.Repairable.MaxDurability = durabilityValues.MaxDurability;

            // 25% chance to add shots to visor items when its below max durability
            if (
                randomUtil.GetChance100(25)
                && modItemToAdjust.ParentId == BaseClasses.ARMORED_EQUIPMENT
                && modItemToAdjust.SlotId == "mod_equipment_000"
                && modItemToAdjust.Upd.Repairable.Durability
                    < modItemDbDetails.Properties.MaxDurability
            )
            // Is damaged
            {
                modItemToAdjust.Upd.FaceShield = new UpdFaceShield
                {
                    Hits = randomUtil.GetInt(1, 3),
                };
            }
        }
    }

    /// <summary>
    ///     Randomise the durability values of plate items in armor <br />
    ///     Has chance to remove plate
    /// </summary>
    /// <param name="plateSlots"> Slots of items to randomise </param>
    /// <param name="armorItemAndMods"> Array of armor + inserts to get items from </param>
    protected void RandomiseArmorInsertsDurabilities(
        List<Slot> plateSlots,
        List<Item> armorItemAndMods
    )
    {
        foreach (var plateSlot in plateSlots)
        {
            var plateTpl = plateSlot.Props.Filters[0].Plate;
            if (string.IsNullOrEmpty(plateTpl))
            // Bsg data lacks a default plate, skip randomisng for this mod
            {
                continue;
            }

            var armorWithMods = armorItemAndMods;

            var modItemDbDetails = itemHelper.GetItem(plateTpl).Value;

            // Chance to remove plate
            var plateExistsChance = traderConfig.Fence.ChancePlateExistsInArmorPercent[
                modItemDbDetails?.Properties?.ArmorClass?.ToString() ?? "3"
            ];
            if (!randomUtil.GetChance100(plateExistsChance))
            {
                // Remove plate from armor
                armorItemAndMods = armorItemAndMods
                    .Where(item => item.SlotId.ToLower() != plateSlot.Name.ToLower())
                    .ToList();

                continue;
            }

            var durabilityValues = GetRandomisedArmorDurabilityValues(
                modItemDbDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );

            // Find items mod to apply durability changes to
            var modItemToAdjust = armorWithMods.FirstOrDefault(mod =>
                string.Equals(mod.SlotId, plateSlot.Name, StringComparison.OrdinalIgnoreCase)
            );

            if (modItemToAdjust == null)
            {
                logger.Warning(
                    $"Unable to randomise armor items {armorWithMods[0].Template} {plateSlot.Name} slot as it cannot be found, skipping"
                );
                continue;
            }

            itemHelper.AddUpdObjectToItem(modItemToAdjust);

            if (modItemToAdjust?.Upd?.Repairable == null)
            {
                modItemToAdjust.Upd.Repairable = new UpdRepairable
                {
                    Durability = modItemDbDetails.Properties.MaxDurability,
                    MaxDurability = modItemDbDetails.Properties.MaxDurability,
                };
            }

            modItemToAdjust.Upd.Repairable.Durability = durabilityValues.Durability;
            modItemToAdjust.Upd.Repairable.MaxDurability = durabilityValues.MaxDurability;
        }
    }

    /// <summary>
    ///     Get stack size of a singular item (no mods)
    /// </summary>
    /// <param name="itemDbDetails"> Item being added to fence </param>
    /// <returns> Stack size </returns>
    protected int GetSingleItemStackCount(TemplateItem itemDbDetails)
    {
        MinMax<int>? overrideValues;
        if (itemHelper.IsOfBaseclass(itemDbDetails.Id, BaseClasses.AMMO))
        {
            overrideValues = traderConfig.Fence.ItemStackSizeOverrideMinMax[itemDbDetails.Parent];
            if (overrideValues != null)
            {
                return randomUtil.GetInt(overrideValues.Min, overrideValues.Max);
            }

            // No override, use stack max size from item db
            return itemDbDetails.Properties.StackMaxSize == 1
                ? 1
                : randomUtil.GetInt(
                    itemDbDetails.Properties.StackMinRandom.Value,
                    itemDbDetails.Properties.StackMaxRandom.Value
                );
        }

        // Check for override in config, use values if exists
        if (
            traderConfig.Fence.ItemStackSizeOverrideMinMax.TryGetValue(
                itemDbDetails.Id,
                out overrideValues
            )
        )
        {
            return randomUtil.GetInt(overrideValues.Min, overrideValues.Max);
        }

        // Check for parent override
        if (
            traderConfig.Fence.ItemStackSizeOverrideMinMax.TryGetValue(
                itemDbDetails.Parent,
                out overrideValues
            )
        )
        {
            return randomUtil.GetInt(overrideValues.Min, overrideValues.Max);
        }

        return 1;
    }

    /// <summary>
    ///     Remove parts of a weapon prior to being listed on flea
    /// </summary>
    /// <param name="itemAndMods"> Weapon to remove parts from </param>
    protected void RemoveRandomModsOfItem(List<Item> itemAndMods)
    {
        // Items to be removed from inventory
        var toDelete = new HashSet<string>();

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
                toDelete.UnionWith(
                    itemHelper.FindAndReturnChildrenByItems(itemAndMods, itemMod.Id)
                );
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

    /// <summary>
    ///     Roll % chance check to see if item should be removed
    /// </summary>
    /// <param name="weaponMod"> Weapon mod being checked </param>
    /// <param name="itemsBeingDeleted"> Current list of items on weapon being deleted </param>
    /// <returns> True if item will be removed </returns>
    protected bool PresetModItemWillBeRemoved(Item weaponMod, HashSet<string> itemsBeingDeleted)
    {
        var slotIdsThatCanFail = traderConfig.Fence.PresetSlotsToRemoveChancePercent;
        if (
            !slotIdsThatCanFail.TryGetValue(weaponMod.SlotId, out var removalChance)
            || removalChance == 0.0
        )
        {
            return false;
        }

        // Roll from 0 to 9999, then divide it by 100: 9999 =  99.99%
        var randomChance = randomUtil.GetInt(0, 9999) / 100;

        return removalChance > randomChance && !itemsBeingDeleted.Contains(weaponMod.Id);
    }

    /// <summary>
    ///     Randomise items' Upd properties e.g. med packs/weapons/armor
    /// </summary>
    /// <param name="itemDetails"> Item being randomised </param>
    /// <param name="itemToAdjust"> Item being edited </param>
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
        if (
            itemDetails.Properties.MaxHpResource != null
            && (itemDetails.Properties.MaxHpResource ?? 0) > 0
        )
        {
            itemToAdjust.Upd.MedKit = new UpdMedKit
            {
                HpResource = randomUtil.GetInt(1, (int)itemDetails.Properties.MaxHpResource),
            };
        }

        // Randomise armor durability
        if (
            (
                itemDetails.Parent == BaseClasses.ARMORED_EQUIPMENT
                || itemDetails.Parent == BaseClasses.FACECOVER
                || itemDetails.Parent == BaseClasses.ARMOR_PLATE
            )
            && (itemDetails.Properties.MaxDurability ?? 0) > 0
        )
        {
            var values = GetRandomisedArmorDurabilityValues(
                itemDetails,
                traderConfig.Fence.ArmorMaxDurabilityPercentMinMax
            );
            itemToAdjust.Upd.Repairable = new UpdRepairable
            {
                Durability = values.Durability,
                MaxDurability = values.MaxDurability,
            };

            return;
        }

        // Randomise Weapon durability
        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.WEAPON))
        {
            var weaponDurabilityLimits = traderConfig.Fence.WeaponDurabilityPercentMinMax;
            var maxDuraMin =
                weaponDurabilityLimits.Max.Min / 100 * itemDetails.Properties.MaxDurability;
            var maxDuraMax =
                weaponDurabilityLimits.Max.Max / 100 * itemDetails.Properties.MaxDurability;
            var chosenMaxDurability = randomUtil.GetDouble(maxDuraMin.Value, maxDuraMax.Value);

            var currentDuraMin =
                weaponDurabilityLimits.Current.Min / 100 * itemDetails.Properties.MaxDurability;
            var currentDuraMax =
                weaponDurabilityLimits.Current.Max / 100 * itemDetails.Properties.MaxDurability;
            var currentDurability = Math.Min(
                randomUtil.GetDouble(currentDuraMin.Value, currentDuraMax.Value),
                chosenMaxDurability
            );

            itemToAdjust.Upd.Repairable = new UpdRepairable
            {
                Durability = currentDurability,
                MaxDurability = chosenMaxDurability,
            };

            return;
        }

        if (itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.REPAIR_KITS))
        {
            itemToAdjust.Upd.RepairKit = new UpdRepairKit
            {
                Resource = randomUtil.GetDouble(1, itemDetails.Properties.MaxRepairResource.Value),
            };

            return;
        }

        // Mechanical key + has limited uses
        if (
            itemHelper.IsOfBaseclass(itemDetails.Id, BaseClasses.KEY_MECHANICAL)
            && (itemDetails.Properties.MaximumNumberOfUsage ?? 0) > 1
        )
        {
            itemToAdjust.Upd.Key = new UpdKey
            {
                NumberOfUsages = randomUtil.GetInt(
                    0,
                    itemDetails.Properties.MaximumNumberOfUsage.Value - 1
                ),
            };

            return;
        }

        // Randomise items that use resources (e.g. fuel)
        if ((itemDetails.Properties.MaxResource ?? 0) > 0)
        {
            var resourceMax = itemDetails.Properties.MaxResource;
            var resourceCurrent = randomUtil.GetInt(1, itemDetails.Properties.MaxResource.Value);

            itemToAdjust.Upd.Resource = new UpdResource
            {
                Value = resourceMax - resourceCurrent,
                UnitsConsumed = resourceCurrent,
            };
        }
    }

    /// <summary>
    ///     Generate a randomised current and max durability value for an armor item
    /// </summary>
    /// <param name="itemDetails"> Item to create values for </param>
    /// <param name="equipmentDurabilityLimits"> Max durability percent min/max values </param>
    /// <returns> Durability + MaxDurability values </returns>
    protected UpdRepairable GetRandomisedArmorDurabilityValues(
        TemplateItem itemDetails,
        ItemDurabilityCurrentMax equipmentDurabilityLimits
    )
    {
        var maxDuraMin =
            equipmentDurabilityLimits.Max.Min / 100 * itemDetails.Properties.MaxDurability;
        var maxDuraMax =
            equipmentDurabilityLimits.Max.Max / 100 * itemDetails.Properties.MaxDurability;
        var chosenMaxDurability = randomUtil.GetDouble(maxDuraMin.Value, maxDuraMax.Value);

        var currentDuraMin =
            equipmentDurabilityLimits.Current.Min / 100 * itemDetails.Properties.MaxDurability;
        var currentDuraMax =
            equipmentDurabilityLimits.Current.Max / 100 * itemDetails.Properties.MaxDurability;
        var chosenCurrentDurability = Math.Min(
            randomUtil.GetDouble(currentDuraMin.Value, currentDuraMax.Value),
            chosenMaxDurability
        );

        return new UpdRepairable
        {
            Durability = chosenCurrentDurability,
            MaxDurability = chosenMaxDurability,
        };
    }

    /// <summary>
    ///     Construct item limit record to hold max and current item count
    /// </summary>
    /// <param name="limits"> Limits as defined in config </param>
    /// <returns> Record, key: item tplId, value: current/max item count allowed </returns>
    protected Dictionary<string, (int current, int max)> InitItemLimitCounter(
        Dictionary<string, int> limits
    )
    {
        var itemTypeCounts = new Dictionary<string, (int current, int max)>();

        foreach (var x in limits.Keys)
        {
            itemTypeCounts[x] = new ValueTuple<int, int>(0, limits[x]);
        }

        return itemTypeCounts;
    }

    /// <summary>
    ///     Get the next Update timestamp for fence
    /// </summary>
    /// <returns> Future timestamp </returns>
    public long GetNextFenceUpdateTimestamp()
    {
        var time = timeUtil.GetTimeStamp();
        var updateSeconds = GetFenceRefreshTime();
        return time + updateSeconds;
    }

    /// <summary>
    ///     Get fence refresh time in seconds
    /// </summary>
    /// <returns> Refresh time in seconds </returns>
    protected int GetFenceRefreshTime()
    {
        var fence = traderConfig
            .UpdateTime.FirstOrDefault(x => x.TraderId == Traders.FENCE)
            .Seconds;

        return randomUtil.GetInt(fence.Min, fence.Max);
    }

    /// <summary>
    ///     Get fence level the passed in profile has
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <returns> FenceLevel object </returns>
    public FenceLevel GetFenceInfo(PmcData pmcData)
    {
        var fenceSettings = databaseService.GetGlobals().Configuration.FenceSettings;
        var pmcFenceInfo = pmcData.TradersInfo[fenceSettings.FenceIdentifier];

        if (pmcFenceInfo == null)
        {
            return fenceSettings.Levels[0];
        }

        var fenceLevels = fenceSettings.Levels.Keys;
        var minLevel = fenceLevels.Min();
        var maxLevel = fenceLevels.Max();
        var pmcFenceLevel = Math.Floor((double)pmcFenceInfo.Standing);

        if (pmcFenceLevel < minLevel)
        {
            return fenceSettings.Levels[minLevel];
        }

        if (pmcFenceLevel > maxLevel)
        {
            return fenceSettings.Levels[maxLevel];
        }

        return fenceSettings.Levels[pmcFenceLevel];
    }

    /// <summary>
    ///     Remove or lower stack size of an assort from fence by id
    /// </summary>
    /// <param name="assortId"> Assort ID to adjust </param>
    /// <param name="buyCount">`Count of items bought </param>
    public void AmendOrRemoveFenceOffer(string assortId, int buyCount)
    {
        var isNormalAssort = true;
        var fenceAssortItem = fenceAssort.Items.FirstOrDefault(item => item.Id == assortId);
        if (fenceAssortItem == null)
        {
            // Not in main assorts, check secondary section
            fenceAssortItem = fenceDiscountAssort.Items.FirstOrDefault(item => item.Id == assortId);
            if (fenceAssortItem == null)
            {
                logger.Error(
                    localisationService.GetText("fence-unable_to_find_offer_by_id", assortId)
                );

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
            var indexToRemove = assorts.FindIndex(item => item.Id == itemToRemove.Id);

            // No offer found in main assort, check discount items
            if (indexToRemove == -1)
            {
                indexToRemove = fenceDiscountAssort.Items.FindIndex(item =>
                    item.Id == itemToRemove.Id
                );
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
