using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Services;

namespace Core.Generators;

[Injectable]
public class LootGenerator(
    )
{

    /// <summary>
    /// Generate a list of items based on configuration options parameter
    /// </summary>
    /// <param name="options">parameters to adjust how loot is generated</param>
    /// <returns>An array of loot items</returns>
    public List<Item> CreateRandomLoot(LootRequest options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate An array of items
    /// TODO - handle weapon presets/ammo packs
    /// </summary>
    /// <param name="forcedLootDict">Dictionary of item tpls with minmax values</param>
    /// <returns>Array of Item</returns>
    public List<Item> CreateForcedLoot(Dictionary<string, MinMax> forcedLootDict)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get pool of items from item db that fit passed in param criteria
    /// </summary>
    /// <param name="itemTplBlacklist">Prevent these items</param>
    /// <param name="itemTypeWhitelist">Only allow these items</param>
    /// <param name="useRewardItemBlacklist">Should item.json reward item config be used</param>
    /// <param name="allowBossItems">Should boss items be allowed in result</param>
    /// <returns>results of filtering + blacklist used</returns>
    protected object GetItemRewardPool(List<string> itemTplBlacklist, List<string> itemTypeWhitelist,
        bool useRewardItemBlacklist, // TODO: type fuckery, return type was { itemPool: [string, ITemplateItem][]; blacklist: Set<string> }
        bool allowBossItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter armor items by their front plates protection level - top if its a helmet
    /// </summary>
    /// <param name="armor">Armor preset to check</param>
    /// <param name="options">Loot request options - armor level etc</param>
    /// <returns>True if item has desired armor level</returns>
    protected bool ArmorOfDesiredProtectionLevel(Preset armor, LootRequest options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Construct item limit record to hold max and current item count for each item type
    /// </summary>
    /// <param name="limits">limits as defined in config</param>
    /// <returns>record, key: item tplId, value: current/max item count allowed</returns>
    protected Dictionary<string, ItemLimit> InitItemLimitCounter(Dictionary<string, int> limits)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a random item in items.json and add to result array
    /// </summary>
    /// <param name="items">items to choose from</param>
    /// <param name="itemTypeCounts">item limit counts</param>
    /// <param name="options">item filters</param>
    /// <param name="result">array to add found item to</param>
    /// <returns>true if item was valid and added to pool</returns>
    protected bool FindAndAddRandomItemToLoot(object items, object itemTypeCounts,
        LootRequest options, // TODO: items type was [string, ITemplateItem][], itemTypeCounts was Record<string, { current: number; max: number }>
        List<Item> result)
    {
        throw new NotImplementedException();
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
    protected bool FindAndAddRandomPresetToLoot(List<Preset> presetPool, object itemTypeCounts,
        List<string> itemBlacklist, // TODO: type fuckery, itemTypeCounts was Record<string, { current: number; max: number }>
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
