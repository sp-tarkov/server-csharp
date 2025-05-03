using System.Text.Json.Serialization;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class LootGenerator(
    ISptLogger<LootGenerator> _logger,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    DatabaseService _databaseService,
    ItemFilterService _itemFilterService,
    LocalisationService _localisationService,
    WeightedRandomHelper _weightedRandomHelper,
    RagfairLinkedItemService _ragfairLinkedItemService,
    SeasonalEventService _seasonalEventService,
    ICloner _cloner
)
{
    /// <summary>
    ///     Generate a list of items based on configuration options parameter
    /// </summary>
    /// <param name="options">parameters to adjust how loot is generated</param>
    /// <returns>An array of loot items</returns>
    public List<List<Item>> CreateRandomLoot(LootRequest options)
    {
        var result = new List<List<Item>>();
        var itemTypeCounts = InitItemLimitCounter(options.ItemLimits);

        // Handle sealed weapon containers
        var sealedWeaponCrateCount = _randomUtil.GetInt(
            options.WeaponCrateCount.Min,
            options.WeaponCrateCount.Max
        );
        if (sealedWeaponCrateCount > 0)
        {
            // Get list of all sealed containers from db - they're all the same, just for flavor
            var itemsDb = _itemHelper.GetItems();
            var sealedWeaponContainerPool = itemsDb.Where(item =>
                item.Name.Contains("event_container_airdrop")
            );

            for (var index = 0; index < sealedWeaponCrateCount; index++)
            {
                // Choose one at random + add to results array
                var chosenSealedContainer = _randomUtil.GetArrayValue(sealedWeaponContainerPool);
                result.Add([
                    new Item
                    {
                        Id = _hashUtil.Generate(),
                        Template = chosenSealedContainer.Id,
                        Upd = new Upd
                        {
                            StackObjectsCount = 1,
                            SpawnedInSession = true
                        }
                    }
                ]);
            }
        }

        // Get items from items.json that have a type of item + not in global blacklist + base type is in whitelist
        var rewardPoolResults = GetItemRewardPool(
            options.ItemBlacklist,
            options.ItemTypeWhitelist,
            options.UseRewardItemBlacklist.GetValueOrDefault(false),
            options.AllowBossItems.GetValueOrDefault(false),
            options.BlockSeasonalItemsOutOfSeason.GetValueOrDefault(false)
        );

        // Pool has items we could add as loot, proceed
        if (rewardPoolResults.ItemPool.Count > 0)
        {
            var randomisedItemCount = _randomUtil.GetInt(options.ItemCount.Min, options.ItemCount.Max);
            for (var index = 0; index < randomisedItemCount; index++)
            {
                if (!FindAndAddRandomItemToLoot(rewardPoolResults.ItemPool, itemTypeCounts, options, result))
                    // Failed to add, reduce index so we get another attempt
                {
                    index--;
                }
            }
        }

        var globalDefaultPresets = _presetHelper.GetDefaultPresets().Values;

        // Filter default presets to just weapons
        var randomisedWeaponPresetCount = _randomUtil.GetInt(
            options.WeaponPresetCount.Min,
            options.WeaponPresetCount.Max
        );
        if (randomisedWeaponPresetCount > 0)
        {
            var weaponDefaultPresets = globalDefaultPresets.Where(preset =>
                    _itemHelper.IsOfBaseclass(preset.Encyclopedia, BaseClasses.WEAPON)
                )
                .ToList();

            if (weaponDefaultPresets.Any())
            {
                for (var index = 0; index < randomisedWeaponPresetCount; index++)
                {
                    if (
                            !FindAndAddRandomPresetToLoot(
                                weaponDefaultPresets,
                                itemTypeCounts,
                                rewardPoolResults.Blacklist,
                                result
                            )
                        )
                        // Failed to add, reduce index so we get another attempt
                    {
                        index--;
                    }
                }
            }
        }

        // Filter default presets to just armors and then filter again by protection level
        var randomisedArmorPresetCount = _randomUtil.GetInt(
            options.ArmorPresetCount.Min,
            options.ArmorPresetCount.Max
        );
        if (randomisedArmorPresetCount > 0)
        {
            var armorDefaultPresets = globalDefaultPresets.Where(preset =>
                _itemHelper.ArmorItemCanHoldMods(preset.Encyclopedia)
            );
            var levelFilteredArmorPresets = armorDefaultPresets.Where(armor =>
                    IsArmorOfDesiredProtectionLevel(armor, options)
                )
                .ToList();

            // Add some armors to rewards
            if (levelFilteredArmorPresets.Any())
            {
                for (var index = 0; index < randomisedArmorPresetCount; index++)
                {
                    if (
                            !FindAndAddRandomPresetToLoot(
                                levelFilteredArmorPresets,
                                itemTypeCounts,
                                rewardPoolResults.Blacklist,
                                result
                            )
                        )
                        // Failed to add, reduce index so we get another attempt
                    {
                        index--;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Generate An array of items
    ///     TODO - handle weapon presets/ammo packs
    /// </summary>
    /// <param name="forcedLootDict">Dictionary of item tpls with minmax values</param>
    /// <returns>Array of Item</returns>
    public List<List<Item>> CreateForcedLoot(Dictionary<string, MinMax<int>> forcedLootDict)
    {
        var result = new List<List<Item>>();

        var forcedItems = forcedLootDict;

        foreach (var forcedItemKvP in forcedItems)
        {
            var details = forcedLootDict[forcedItemKvP.Key];
            var randomisedItemCount = _randomUtil.GetInt(details.Min, details.Max);

            // Add forced loot item to result
            var newLootItem = new Item
            {
                Id = _hashUtil.Generate(),
                Template = forcedItemKvP.Key,
                Upd = new Upd
                {
                    StackObjectsCount = randomisedItemCount,
                    SpawnedInSession = true
                }
            };

            var splitResults = _itemHelper.SplitStack(newLootItem);
            result.Add(splitResults);
        }

        return result;
    }

    /// <summary>
    ///     Get pool of items from item db that fit passed in param criteria
    /// </summary>
    /// <param name="itemTplBlacklist">Prevent these items</param>
    /// <param name="itemTypeWhitelist">Only allow these items</param>
    /// <param name="useRewardItemBlacklist">Should item.json reward item config be used</param>
    /// <param name="allowBossItems">Should boss items be allowed in result</param>
    /// <param name="blockSeasonalItemsOutOfSeason">Prevent seasonal items appearing outside their defined season</param>
    /// <returns>results of filtering + blacklist used</returns>
    protected ItemRewardPoolResults GetItemRewardPool(
        HashSet<string> itemTplBlacklist,
        List<string> itemTypeWhitelist,
        bool useRewardItemBlacklist,
        bool allowBossItems,
        bool blockSeasonalItemsOutOfSeason)
    {
        var itemsDb = _databaseService.GetItems().Values;
        var itemBlacklist = new HashSet<string>();
        itemBlacklist.UnionWith([.._itemFilterService.GetBlacklistedItems(), ..itemTplBlacklist]);

        if (useRewardItemBlacklist)
        {
            var rewardItemBlacklist = _itemFilterService.GetItemRewardBlacklist();

            // Get all items that match the blacklisted types and fold into item blacklist
            var itemTypeBlacklist = _itemFilterService.GetItemRewardBaseTypeBlacklist();
            var itemsMatchingTypeBlacklist = itemsDb
                .Where(templateItem => _itemHelper.IsOfBaseclasses(templateItem.Parent, itemTypeBlacklist))
                .Select(templateItem => templateItem.Id);

            itemBlacklist.UnionWith([..rewardItemBlacklist, ..itemsMatchingTypeBlacklist]);
        }

        if (!allowBossItems)
        {
            itemBlacklist.UnionWith(_itemFilterService.GetBossItems());
        }

        if (blockSeasonalItemsOutOfSeason)
        {
            itemBlacklist.UnionWith(_seasonalEventService.GetInactiveSeasonalEventItems());
        }

        var items = itemsDb.Where(item =>
                !itemBlacklist.Contains(item.Id) &&
                string.Equals(item.Type, "item", StringComparison.OrdinalIgnoreCase) &&
                !item.Properties.QuestItem.GetValueOrDefault(false) &&
                itemTypeWhitelist.Contains(item.Parent)
            )
            .ToList();

        return new ItemRewardPoolResults
        {
            ItemPool = items,
            Blacklist = itemBlacklist
        };
    }

    /// <summary>
    ///     Filter armor items by their front plates protection level - top if it's a helmet
    /// </summary>
    /// <param name="armor">Armor preset to check</param>
    /// <param name="options">Loot request options - armor level etc</param>
    /// <returns>True if item has desired armor level</returns>
    protected bool IsArmorOfDesiredProtectionLevel(Preset armor, LootRequest options)
    {
        string[] relevantSlots = ["front_plate", "helmet_top", "soft_armor_front"];
        foreach (var slotId in relevantSlots)
        {
            var armorItem = armor.Items.FirstOrDefault(item => string.Equals(item?.SlotId, slotId));
            if (armorItem is null)
            {
                continue;
            }

            var armorDetails = _itemHelper.GetItem(armorItem.Template).Value;
            var armorClass = armorDetails.Properties.ArmorClass;

            return options.ArmorLevelWhitelist.Contains(armorClass.Value);
        }

        return false;
    }

    /// <summary>
    ///     Construct item limit record to hold max and current item count for each item type
    /// </summary>
    /// <param name="limits">limits as defined in config</param>
    /// <returns>record, key: item tplId, value: current/max item count allowed</returns>
    protected Dictionary<string, ItemLimit> InitItemLimitCounter(Dictionary<string, int> limits)
    {
        var itemTypeCounts = new Dictionary<string, ItemLimit>();
        foreach (var itemTypeId in limits)
        {
            itemTypeCounts[itemTypeId.Key] = new ItemLimit
            {
                Current = 0,
                Max = limits[itemTypeId.Key]
            };
        }

        return itemTypeCounts;
    }

    /// <summary>
    ///     Find a random item in items.json and add to result array
    /// </summary>
    /// <param name="items">items to choose from</param>
    /// <param name="itemTypeCounts">item limit counts</param>
    /// <param name="options">item filters</param>
    /// <param name="result">array to add found item to</param>
    /// <returns>true if item was valid and added to pool</returns>
    protected bool FindAndAddRandomItemToLoot(List<TemplateItem> items, Dictionary<string, ItemLimit> itemTypeCounts,
        LootRequest options,
        List<List<Item>> result)
    {
        var randomItem = _randomUtil.GetArrayValue(items);

        var itemLimitCount = itemTypeCounts.TryGetValue(randomItem.Parent, out var randomItemLimitCount);
        if (!itemLimitCount && randomItemLimitCount?.Current > randomItemLimitCount?.Max)
        {
            return false;
        }

        // Skip armors as they need to come from presets
        if (_itemHelper.ArmorItemCanHoldMods(randomItem.Id))
        {
            return false;
        }

        var newLootItem = new Item
        {
            Id = _hashUtil.Generate(),
            Template = randomItem.Id,
            Upd = new Upd
            {
                StackObjectsCount = 1,
                SpawnedInSession = true
            }
        };

        // Special case - handle items that need a stackcount > 1
        if (randomItem.Properties.StackMaxSize > 1)
        {
            newLootItem.Upd.StackObjectsCount = GetRandomisedStackCount(randomItem, options);
        }

        newLootItem.Template = randomItem.Id;
        result.Add([newLootItem]);

        if (randomItemLimitCount is not null)
            // Increment item count as it's in limit array
        {
            randomItemLimitCount.Current++;
        }

        // Item added okay
        return true;
    }

    /// <summary>
    ///     Get a randomised stack count for an item between its StackMinRandom and StackMaxSize values
    /// </summary>
    /// <param name="item">item to get stack count of</param>
    /// <param name="options">loot options</param>
    /// <returns>stack count</returns>
    protected int GetRandomisedStackCount(TemplateItem item, LootRequest options)
    {
        var min = item.Properties.StackMinRandom;
        var max = item.Properties.StackMaxSize;

        if (options.ItemStackLimits.TryGetValue(item.Id, out var itemLimits))
        {
            min = itemLimits.Min;
            max = itemLimits.Max;
        }

        return _randomUtil.GetInt(min ?? 1, max ?? 1);
    }

    /// <summary>
    ///     Find a random item in items.json and add to result list
    /// </summary>
    /// <param name="presetPool">Presets to choose from</param>
    /// <param name="itemTypeCounts">Item limit counts</param>
    /// <param name="itemBlacklist">Items to skip</param>
    /// <param name="result">List to add chosen preset to</param>
    /// <returns>true if preset was valid and added to pool</returns>
    protected bool FindAndAddRandomPresetToLoot(List<Preset> presetPool,
        Dictionary<string, ItemLimit> itemTypeCounts,
        HashSet<string> itemBlacklist,
        List<List<Item>> result)
    {
        // Choose random preset and get details from item db using encyclopedia value (encyclopedia === tplId)
        var chosenPreset = _randomUtil.GetArrayValue(presetPool);
        if (chosenPreset is null)
        {
            _logger.Warning("Unable to find random preset in given presets, skipping");

            return false;
        }

        // No `_encyclopedia` property, not possible to reliably get root item tpl
        if (chosenPreset.Encyclopedia is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Preset with id: {chosenPreset?.Id} lacks encyclopedia property, skipping");
            }

            return false;
        }

        // Get preset root item db details via its `_encyclopedia` property
        var itemDbDetails = _itemHelper.GetItem(chosenPreset.Encyclopedia);
        if (!itemDbDetails.Key)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"$Unable to find preset with tpl: {chosenPreset.Encyclopedia}, skipping");
            }

            return false;
        }

        // Skip preset if root item is blacklisted
        if (itemBlacklist.Contains(chosenPreset.Items[0].Template))
        {
            return false;
        }

        // Some custom mod items lack a parent property
        if (itemDbDetails.Value.Parent is null)
        {
            _logger.Error(_localisationService.GetText("loot-item_missing_parentid", itemDbDetails.Value?.Name));

            return false;
        }

        // Check chosen preset hasn't exceeded spawn limit
        var hasItemLimitCount = itemTypeCounts.TryGetValue(itemDbDetails.Value.Parent, out var itemLimitCount);
        if (!hasItemLimitCount && itemLimitCount?.Current > itemLimitCount?.Max)
        {
            return false;
        }

        var presetAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(chosenPreset.Items));
        _itemHelper.RemapRootItemId(presetAndMods);

        _itemHelper.SetFoundInRaid(presetAndMods);

        // Add chosen preset tpl to result array
        result.Add(presetAndMods);

        if (itemLimitCount is not null)
            // Increment item count as item has been chosen and its inside itemLimitCount dictionary
        {
            itemLimitCount.Current++;
        }

        // Item added okay
        return true;
    }

    /// <summary>
    ///     Sealed weapon containers have a weapon + associated mods inside them + assortment of other things (food/meds)
    /// </summary>
    /// <param name="containerSettings">sealed weapon container settings</param>
    /// <returns>List of items with children lists</returns>
    public List<List<Item>> GetSealedWeaponCaseLoot(SealedAirdropContainerSettings containerSettings)
    {
        List<List<Item>> itemsToReturn = [];

        // Choose a weapon to give to the player (weighted)
        var chosenWeaponTpl = _weightedRandomHelper.GetWeightedValue(
            containerSettings.WeaponRewardWeight
        );

        // Get itemDb details of weapon
        var weaponDetailsDb = _itemHelper.GetItem(chosenWeaponTpl);
        if (!weaponDetailsDb.Key)
        {
            _logger.Error(
                _localisationService.GetText("loot-non_item_picked_as_sealed_weapon_crate_reward", chosenWeaponTpl)
            );

            return itemsToReturn;
        }

        // Get weapon preset - default or choose a random one from globals.json preset pool
        var chosenWeaponPreset = containerSettings.DefaultPresetsOnly
            ? _presetHelper.GetDefaultPreset(chosenWeaponTpl)
            : _randomUtil.GetArrayValue(_presetHelper.GetPresets(chosenWeaponTpl));

        // No default preset found for weapon, choose a random one
        if (chosenWeaponPreset is null)
        {
            _logger.Warning(
                _localisationService.GetText("loot-default_preset_not_found_using_random", chosenWeaponTpl)
            );
            chosenWeaponPreset = _randomUtil.GetArrayValue(_presetHelper.GetPresets(chosenWeaponTpl));
        }

        // Clean up Ids to ensure they're all unique and prevent collisions
        var presetAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(chosenWeaponPreset.Items));
        _itemHelper.RemapRootItemId(presetAndMods);

        // Add preset to return object
        itemsToReturn.Add(presetAndMods);

        // Get a random collection of weapon mods related to chosen weawpon and add them to result array
        var linkedItemsToWeapon = _ragfairLinkedItemService.GetLinkedDbItems(chosenWeaponTpl);
        itemsToReturn.AddRange(
            GetSealedContainerWeaponModRewards(containerSettings, linkedItemsToWeapon, chosenWeaponPreset)
        );

        // Handle non-weapon mod reward types
        itemsToReturn.AddRange(GetSealedContainerNonWeaponModRewards(containerSettings, weaponDetailsDb.Value));

        return itemsToReturn;
    }

    /// <summary>
    ///     Get non-weapon mod rewards for a sealed container
    /// </summary>
    /// <param name="containerSettings">Sealed weapon container settings</param>
    /// <param name="weaponDetailsDb">Details for the weapon to reward player</param>
    /// <returns>List of item with children lists</returns>
    protected List<List<Item>> GetSealedContainerNonWeaponModRewards(SealedAirdropContainerSettings containerSettings,
        TemplateItem weaponDetailsDb)
    {
        List<List<Item>> rewards = [];

        foreach (var (rewardKey, settings) in containerSettings.RewardTypeLimits)
        {
            var rewardCount = _randomUtil.GetInt(settings.Min, settings.Max);
            if (rewardCount == 0)
            {
                continue;
            }

            // Edge case - ammo boxes
            if (rewardKey == BaseClasses.AMMO_BOX)
            {
                // Get ammo boxes from db
                var ammoBoxesDetails = containerSettings.AmmoBoxWhitelist.Select(tpl =>
                    {
                        var itemDetails = _itemHelper.GetItem(tpl);
                        return itemDetails.Value;
                    }
                );

                // Need to find boxes that matches weapons caliber
                var weaponCaliber = weaponDetailsDb.Properties.AmmoCaliber;
                var ammoBoxesMatchingCaliber = ammoBoxesDetails.Where(x =>
                    x.Properties.AmmoCaliber == weaponCaliber
                );
                if (!ammoBoxesMatchingCaliber.Any())
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"No ammo box with caliber {weaponCaliber} found, skipping");
                    }

                    continue;
                }

                for (var index = 0; index < rewardCount; index++)
                {
                    var chosenAmmoBox = _randomUtil.GetArrayValue(ammoBoxesMatchingCaliber);
                    var ammoBoxReward = new List<Item>
                    {
                        new()
                        {
                            Id = _hashUtil.Generate(),
                            Template = chosenAmmoBox.Id
                        }
                    };
                    _itemHelper.AddCartridgesToAmmoBox(ammoBoxReward, chosenAmmoBox);
                    rewards.Add(ammoBoxReward);
                }

                continue;
            }

            // Get all items of the desired type + not quest items + not globally blacklisted
            var rewardItemPool = _databaseService.GetItems()
                .Values.Where(item =>
                    item.Parent == rewardKey &&
                    string.Equals(item.Type, "item", StringComparison.OrdinalIgnoreCase) &&
                    _itemFilterService.IsItemBlacklisted(item.Id) &&
                    !(containerSettings.AllowBossItems || _itemFilterService.IsBossItem(item.Id)) &&
                    item.Properties.QuestItem is null
                );

            if (!rewardItemPool.Any())
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"No items with base type of {rewardKey} found, skipping");
                }

                continue;
            }

            for (var index = 0; index < rewardCount; index++)
            {
                // Choose a random item from pool
                var chosenRewardItem = _randomUtil.GetArrayValue(rewardItemPool);
                var rewardItem = new List<Item>
                {
                    new()
                    {
                        Id = _hashUtil.Generate(),
                        Template = chosenRewardItem.Id
                    }
                };

                rewards.Add(rewardItem);
            }
        }

        return rewards;
    }

    /// <summary>
    ///     Iterate over the container weaponModRewardLimits settings and create a list of weapon mods to reward player
    /// </summary>
    /// <param name="containerSettings">Sealed weapon container settings</param>
    /// <param name="linkedItemsToWeapon">All items that can be attached/inserted into weapon</param>
    /// <param name="chosenWeaponPreset">The weapon preset given to player as reward</param>
    /// <returns>List of item with children lists</returns>
    protected List<List<Item>> GetSealedContainerWeaponModRewards(SealedAirdropContainerSettings containerSettings, List<TemplateItem> linkedItemsToWeapon,
        Preset chosenWeaponPreset)
    {
        List<List<Item>> modRewards = [];

        foreach (var (rewardKey, settings) in containerSettings.WeaponModRewardLimits)
        {
            var rewardCount = _randomUtil.GetInt(settings.Min, settings.Max);

            // Nothing to add, skip reward type
            if (rewardCount == 0)
            {
                continue;
            }

            // Get items that fulfil reward type criteria from items that fit on gun
            var relatedItems = linkedItemsToWeapon?.Where(item => item?.Parent == rewardKey && !_itemFilterService.IsItemBlacklisted(item.Id)
            );
            if (relatedItems is null || !relatedItems.Any())
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"No items found to fulfil reward type: {rewardKey} for weapon: {chosenWeaponPreset.Name}, skipping type"
                    );
                }

                continue;
            }

            // Find a random item of the desired type and add as reward
            for (var index = 0; index < rewardCount; index++)
            {
                var chosenItem = _randomUtil.DrawRandomFromList(relatedItems.ToList());
                var reward = new List<Item>
                {
                    new()
                    {
                        Id = _hashUtil.Generate(),
                        Template = chosenItem[0].Id
                    }
                };

                modRewards.Add(reward);
            }
        }

        return modRewards;
    }

    /// <summary>
    ///     Handle event-related loot containers - currently just the halloween jack-o-lanterns that give food rewards
    /// </summary>
    /// <param name="rewardContainerDetails"></param>
    /// <returns>List of item with children lists</returns>
    public List<List<Item>> GetRandomLootContainerLoot(RewardDetails rewardContainerDetails)
    {
        List<List<Item>> itemsToReturn = [];

        // Get random items and add to newItemRequest
        for (var index = 0; index < rewardContainerDetails.RewardCount; index++)
        {
            // Pick random reward from pool, add to request object
            var chosenRewardItemTpl = PickRewardItem(rewardContainerDetails);

            if (_presetHelper.HasPreset(chosenRewardItemTpl))
            {
                var preset = _presetHelper.GetDefaultPreset(chosenRewardItemTpl);

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = _itemHelper.ReplaceIDs(preset.Items);

                _itemHelper.RemapRootItemId(presetAndMods);
                itemsToReturn.Add(presetAndMods);

                continue;
            }

            List<Item> rewardItem =
            [
                new()
                {
                    Id = _hashUtil.Generate(),
                    Template = chosenRewardItemTpl
                }
            ];
            itemsToReturn.Add(rewardItem);
        }

        return itemsToReturn;
    }

    /// <summary>
    ///     Pick a reward item based on the reward details data
    /// </summary>
    /// <param name="rewardContainerDetails"></param>
    /// <returns>Single tpl</returns>
    protected string PickRewardItem(RewardDetails rewardContainerDetails)
    {
        if (rewardContainerDetails.RewardTplPool is not null && rewardContainerDetails.RewardTplPool.Count > 0)
        {
            return _weightedRandomHelper.GetWeightedValue(rewardContainerDetails.RewardTplPool);
        }

        return _randomUtil.GetArrayValue(
            GetItemRewardPool([], rewardContainerDetails.RewardTypePool, true, true, false)
                .ItemPool.Select(item => item.Id
                )
        );
    }

    public record ItemRewardPoolResults
    {
        public List<TemplateItem> ItemPool
        {
            get;
            set;
        }

        public HashSet<string> Blacklist
        {
            get;
            set;
        }
    }
}

public class ItemLimit
{
    [JsonPropertyName("current")]
    public int Current
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public int Max
    {
        get;
        set;
    }
}
