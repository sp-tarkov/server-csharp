using System.Text.Json.Serialization;
using Core.Models.Eft.Common;

namespace Core.Models.Spt.Config;

public record ItemConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-item";

    /**
     * Items that should be globally blacklisted
     */
    [JsonPropertyName("blacklist")]
    public HashSet<string> Blacklist
    {
        get;
        set;
    }

    /**
     * Items that should not be lootable from any location
     */
    [JsonPropertyName("lootableItemBlacklist")]
    public HashSet<string> LootableItemBlacklist
    {
        get;
        set;
    }

    /**
     * items that should not be given as rewards
     */
    [JsonPropertyName("rewardItemBlacklist")]
    public HashSet<string> RewardItemBlacklist
    {
        get;
        set;
    }

    /**
     * Item base types that should not be given as rewards
     */
    [JsonPropertyName("rewardItemTypeBlacklist")]
    public HashSet<string> RewardItemTypeBlacklist
    {
        get;
        set;
    }

    /**
     * Items that can only be found on bosses
     */
    [JsonPropertyName("bossItems")]
    public HashSet<string> BossItems
    {
        get;
        set;
    }

    [JsonPropertyName("handbookPriceOverride")]
    public Dictionary<string, HandbookPriceOverride> HandbookPriceOverride
    {
        get;
        set;
    }

    /**
     * Presets to add to the globals.json `ItemPresets` dictionary on server start
     */
    [JsonPropertyName("customItemGlobalPresets")]
    public List<Preset> CustomItemGlobalPresets
    {
        get;
        set;
    }
}

public record HandbookPriceOverride
{
    /**
     * Price in roubles
     */
    [JsonPropertyName("price")]
    public double Price
    {
        get;
        set;
    }

    /**
     * NOT parentId from items.json, but handbook.json
     */
    [JsonPropertyName("parentId")]
    public string ParentId
    {
        get;
        set;
    }
}
