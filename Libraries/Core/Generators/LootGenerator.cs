using System.Text.Json.Serialization;
using Core.Helpers;
using SptCommon.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Services;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;

namespace Core.Generators;

[Injectable]
public class LootGenerator(
    ISptLogger<LootGenerator> _logger,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    DatabaseService _databaseService,
    ItemFilterService _itemFilterService

    )
{

    /// <summary>
    /// Generate a list of items based on configuration options parameter
    /// </summary>
    /// <param name="options">parameters to adjust how loot is generated</param>
    /// <returns>An array of loot items</returns>
    public List<Item> CreateRandomLoot(LootRequest options)
    {
        var result = new List<Item>();
        var itemTypeCounts = InitItemLimitCounter(options.ItemLimits);

        // Handle sealed weapon containers
        var sealedWeaponCrateCount = _randomUtil.GetDouble(
            options.WeaponCrateCount.Min.Value,
            options.WeaponCrateCount.Max.Value);
        if (sealedWeaponCrateCount > 0) {
            // Get list of all sealed containers from db - they're all the same, just for flavor
            var itemsDb = _itemHelper.GetItems();
            var sealedWeaponContainerPool = (itemsDb).Where((item) =>
                item.Name.Contains("event_container_airdrop"));

            for (var index = 0; index < sealedWeaponCrateCount; index++) {
                // Choose one at random + add to results array
                var chosenSealedContainer = _randomUtil.GetArrayValue(sealedWeaponContainerPool);
                result.Add( new Item{
                    Id = _hashUtil.Generate(),
                    Template = chosenSealedContainer.Id,
                    Upd = new Upd{
                        StackObjectsCount = 1,
                        SpawnedInSession = true
                    },
                });
            }
        }

        // Get items from items.json that have a type of item + not in global blacklist + base type is in whitelist
        var rewardPoolResults = GetItemRewardPool(
            options.ItemBlacklist,
            options.ItemTypeWhitelist,
            options.UseRewardItemBlacklist.GetValueOrDefault(false),
            options.AllowBossItems.GetValueOrDefault(false));

        // Pool has items we could add as loot, proceed
        if (rewardPoolResults.ItemPool.Count > 0) {
            var randomisedItemCount = _randomUtil.GetDouble(options.ItemCount.Min.Value, options.ItemCount.Max.Value);
            for (var index = 0; index < randomisedItemCount; index++) {
                if (!FindAndAddRandomItemToLoot(rewardPoolResults.ItemPool, itemTypeCounts, options, result)) {
                    // Failed to add, reduce index so we get another attempt
                    index--;
                }
            }
        }

        var globalDefaultPresets = _presetHelper.GetDefaultPresets().Values;

        // Filter default presets to just weapons
        var randomisedWeaponPresetCount = _randomUtil.GetDouble(
            options.WeaponPresetCount.Min.Value,
            options.WeaponPresetCount.Max.Value);
        if (randomisedWeaponPresetCount > 0) {
            var weaponDefaultPresets = globalDefaultPresets.Where((preset) =>
                _itemHelper.IsOfBaseclass(preset.Encyclopedia, BaseClasses.WEAPON)).ToList();

            if (weaponDefaultPresets.Any()) {
                for (var index = 0; index < randomisedWeaponPresetCount; index++) {
                    if (
                        !FindAndAddRandomPresetToLoot(
                            weaponDefaultPresets,
                            itemTypeCounts,
                            rewardPoolResults.Blacklist,
                            result)
                    ) {
                        // Failed to add, reduce index so we get another attempt
                        index--;
                    }
                }
            }
        }

        // Filter default presets to just armors and then filter again by protection level
        var randomisedArmorPresetCount = _randomUtil.GetDouble(
            options.ArmorPresetCount.Min.Value,
            options.ArmorPresetCount.Max.Value);
        if (randomisedArmorPresetCount > 0) {
            var armorDefaultPresets = globalDefaultPresets.Where((preset) =>
                _itemHelper.ArmorItemCanHoldMods(preset.Encyclopedia));
            var levelFilteredArmorPresets = armorDefaultPresets.Where((armor) =>
                IsArmorOfDesiredProtectionLevel(armor, options)).ToList();

            // Add some armors to rewards
            if (levelFilteredArmorPresets.Any()) {
                for (var index = 0; index < randomisedArmorPresetCount; index++) {
                    if (
                        !FindAndAddRandomPresetToLoot(
                            levelFilteredArmorPresets,
                            itemTypeCounts,
                            rewardPoolResults.Blacklist,
                            result)
                    ) {
                        // Failed to add, reduce index so we get another attempt
                        index--;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Generate An array of items
    /// TODO - handle weapon presets/ammo packs
    /// </summary>
    /// <param name="forcedLootDict">Dictionary of item tpls with minmax values</param>
    /// <returns>Array of Item</returns>
    public List<Item> CreateForcedLoot(Dictionary<string, MinMax> forcedLootDict)
    {
        var result = new List<Item>();

        var forcedItems = forcedLootDict;

        foreach (var forcedItemKvP in forcedItems) {
            var details = forcedLootDict[forcedItemKvP.Key];
            var randomisedItemCount = _randomUtil.GetDouble(details.Min.Value, details.Max.Value);

            // Add forced loot item to result
            var newLootItem = new Item{
                Id = _hashUtil.Generate(),
                Template = forcedItemKvP.Key,
                Upd = new Upd{
                    StackObjectsCount = randomisedItemCount,
                    SpawnedInSession = true,
                },
            };

            var splitResults = _itemHelper.SplitStack(newLootItem);
            result.AddRange(splitResults);
        }

        return result;
    }

    /// <summary>
    /// Get pool of items from item db that fit passed in param criteria
    /// </summary>
    /// <param name="itemTplBlacklist">Prevent these items</param>
    /// <param name="itemTypeWhitelist">Only allow these items</param>
    /// <param name="useRewardItemBlacklist">Should item.json reward item config be used</param>
    /// <param name="allowBossItems">Should boss items be allowed in result</param>
    /// <returns>results of filtering + blacklist used</returns>
    protected ItemRewardPoolResults GetItemRewardPool(List<string> itemTplBlacklist, List<string> itemTypeWhitelist,
        bool useRewardItemBlacklist,
        bool allowBossItems)
    {
        var itemsDb = _databaseService.GetItems().Values;
        var itemBlacklist = new HashSet<string>();
        itemBlacklist.UnionWith(_itemFilterService.GetBlacklistedItems());
        itemBlacklist.UnionWith(itemTplBlacklist);

        if (useRewardItemBlacklist)
        {
            var itemsToAdd = _itemFilterService.GetItemRewardBlacklist();

            // Get all items that match the blacklisted types and fold into item blacklist
            var itemTypeBlacklist = _itemFilterService.GetItemRewardBaseTypeBlacklist();
            var itemsMatchingTypeBlacklist = (itemsDb)
                .Where((templateItem) => _itemHelper.IsOfBaseclasses(templateItem.Parent, itemTypeBlacklist))
                .Select((templateItem) => templateItem.Id);

            // Clear out blacklist
            itemBlacklist = [];
            itemBlacklist.UnionWith(itemBlacklist);
            itemBlacklist.UnionWith(itemsToAdd);
            itemBlacklist.UnionWith(itemsMatchingTypeBlacklist);
        }

        if (!allowBossItems)
        {
            foreach (var bossItem in _itemFilterService.GetBossItems()) {
                itemBlacklist.Add(bossItem);
            }
        }

        var items = itemsDb.Where(
            (item) =>
                !itemBlacklist.Contains(item.Id) &&
                    item.Type.ToLower() == "item" &&
                                                    !item.Properties.QuestItem.GetValueOrDefault(false) &&
                                                    itemTypeWhitelist.Contains(item.Parent)).ToList();

        return new ItemRewardPoolResults{ ItemPool = items, Blacklist = itemBlacklist };
    }

    public record ItemRewardPoolResults
    {
        public List<TemplateItem> ItemPool { get; set; }
        public HashSet<string> Blacklist { get; set; }
    }

    /// <summary>
    /// Filter armor items by their front plates protection level - top if it's a helmet
    /// </summary>
    /// <param name="armor">Armor preset to check</param>
    /// <param name="options">Loot request options - armor level etc</param>
    /// <returns>True if item has desired armor level</returns>
    protected bool IsArmorOfDesiredProtectionLevel(Preset armor, LootRequest options)
    {
        string[] relevantSlots = ["front_plate", "helmet_top", "soft_armor_front"];
        foreach (var slotId in relevantSlots) {
            var armorItem = armor.Items.FirstOrDefault((item) => item?.SlotId?.ToLower() == slotId);
            if (armorItem is null)
            {
                continue;
            }

            var armorDetails = _itemHelper.GetItem(armorItem.Template).Value;
            var armorClass = armorDetails.Properties.ArmorClass;

            return options.ArmorLevelWhitelist.Contains((int)armorClass.Value);
        }

        return false;
    }

    /// <summary>
    /// Construct item limit record to hold max and current item count for each item type
    /// </summary>
    /// <param name="limits">limits as defined in config</param>
    /// <returns>record, key: item tplId, value: current/max item count allowed</returns>
    private Dictionary<string, ItemLimit> InitItemLimitCounter(Dictionary<string, double> limits)
    {
        var itemTypeCounts = new Dictionary<string, ItemLimit>();
        foreach (var itemTypeId in limits) {
            itemTypeCounts[itemTypeId.Key] = new ItemLimit() { Current = 0, Max = limits[itemTypeId.Key] };
        }

        return itemTypeCounts;
    }

    /// <summary>
    /// Find a random item in items.json and add to result array
    /// </summary>
    /// <param name="items">items to choose from</param>
    /// <param name="itemTypeCounts">item limit counts</param>
    /// <param name="options">item filters</param>
    /// <param name="result">array to add found item to</param>
    /// <returns>true if item was valid and added to pool</returns>
    protected bool FindAndAddRandomItemToLoot(TemplateItem[] items, Dictionary<string, ItemLimit> itemTypeCounts,
        LootRequest options,
        List<Item> result)
    {
        var randomItem = _randomUtil.GetArrayValue(items);

        var itemLimitCount = itemTypeCounts[randomItem.Parent];
        if (itemLimitCount is not null && itemLimitCount.Current > itemLimitCount.Max) {
            return false;
        }

        // Skip armors as they need to come from presets
        if (_itemHelper.ArmorItemCanHoldMods(randomItem.Id)) {
            return false;
        }

        var newLootItem = new Item {
            Id = _hashUtil.Generate(),
            Template = randomItem.Id,
            Upd = {
                StackObjectsCount = 1,
                SpawnedInSession = true,
            },
        };

        // Special case - handle items that need a stackcount > 1
        if (randomItem.Properties.StackMaxSize > 1) {
            newLootItem.Upd.StackObjectsCount = GetRandomisedStackCount(randomItem, options);
        }

        newLootItem.Template = randomItem.Id;
        result.Add(newLootItem);

        if (itemLimitCount is not null) {
            // Increment item count as it's in limit array
            itemLimitCount.Current++;
        }

        // Item added okay
        return true;
    }

    /// <summary>
    /// Get a randomised stack count for an item between its StackMinRandom and StackMaxSize values
    /// </summary>
    /// <param name="item">item to get stack count of</param>
    /// <param name="options">loot options</param>
    /// <returns>stack count</returns>
    protected int GetRandomisedStackCount(TemplateItem item, LootRequest options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a random item in items.json and add to result list
    /// </summary>
    /// <param name="presetPool">Presets to choose from</param>
    /// <param name="itemTypeCounts">Item limit counts</param>
    /// <param name="itemBlacklist">Items to skip</param>
    /// <param name="result">List to add chosen preset to</param>
    /// <returns>true if preset was valid and added to pool</returns>
    protected bool FindAndAddRandomPresetToLoot(List<Preset> presetPool,
        Dictionary<string, ItemLimit> itemTypeCounts,
        HashSet<string> itemBlacklist,
        List<Item> result)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sealed weapon containers have a weapon + associated mods inside them + assortment of other things (food/meds)
    /// </summary>
    /// <param name="containerSettings">sealed weapon container settings</param>
    /// <returns>List of items with children lists</returns>
    public List<List<Item>> GetSealedWeaponCaseLoot(SealedAirdropContainerSettings containerSettings)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get non-weapon mod rewards for a sealed container
    /// </summary>
    /// <param name="containerSettings">Sealed weapon container settings</param>
    /// <param name="weaponDetailsDb">Details for the weapon to reward player</param>
    /// <returns>List of item with children lists</returns>
    protected List<List<Item>> GetSealedContainerNonWeaponModRewards(SealedAirdropContainerSettings containerSettings,
        TemplateItem weaponDetailsDb)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over the container weaponModRewardLimits settings and create a list of weapon mods to reward player
    /// </summary>
    /// <param name="containerSettings">Sealed weapon container settings</param>
    /// <param name="linkedItemsToWeapon">All items that can be attached/inserted into weapon</param>
    /// <param name="chosenWeaponPreset">The weapon preset given to player as reward</param>
    /// <returns>List of item with children lists</returns>
    protected List<List<Item>> GetSealedContainerWeaponModRewards(SealedAirdropContainerSettings containerSettings, List<TemplateItem> linkedItemsToWeapon,
        Preset chosenWeaponPreset)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle event-related loot containers - currently just the halloween jack-o-lanterns that give food rewards
    /// </summary>
    /// <param name="rewardContainerDetails"></param>
    /// <returns>List of item with children lists</returns>
    public List<List<Item>> GetRandomLootContainerLoot(RewardDetails rewardContainerDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Pick a reward item based on the reward details data
    /// </summary>
    /// <param name="rewardContainerDetails"></param>
    /// <returns>Single tpl</returns>
    protected string PickRewardItem(RewardDetails rewardContainerDetails)
    {
        throw new NotImplementedException();
    }
}

public class ItemLimit
{
    [JsonPropertyName("current")]
    public double Current { get; set; }

    [JsonPropertyName("max")]
    public double Max { get; set; }
}
