using SptCommon.Annotations;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils.Cloners;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ItemFilterService(
    ISptLogger<ItemFilterService> _logger,
    ICloner _cloner,
    ConfigServer _configServer
)
{
    protected ItemConfig _itemConfig = _configServer.GetConfig<ItemConfig>();

    protected HashSet<string>? _lootableItemBlacklistCache = [];
    protected HashSet<string>? _itemBlacklistCache = [];

    /**
     * Check if the provided template id is blacklisted in config/item.json/blacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool ItemBlacklisted(string tpl)
    {
        if (_itemBlacklistCache.Count == 0)
            foreach (var item in _itemConfig.Blacklist)
                _itemBlacklistCache.Add(item);

        return _itemBlacklistCache.Contains(tpl);
    }

    /**
     * Check if item is blacklisted from being a reward for player
     * @param tpl item tpl to check is on blacklist
     * @returns True when blacklisted
     */
    public bool ItemRewardBlacklisted(string tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }

    /**
     * Get an array of items that should never be given as a reward to player
     * @returns string array of item tpls
     */
    public List<string> GetItemRewardBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemBlacklist).ToList();
    }

    /**
     * Get an array of item types that should never be given as a reward to player
     * @returns string array of item base ids
     */
    public List<string> GetItemRewardBaseTypeBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemTypeBlacklist).ToList();
    }

    /**
     * Return every template id blacklisted in config/item.json
     * @returns string array of blacklisted template ids
     */
    public List<string> GetBlacklistedItems()
    {
        return _cloner.Clone(_itemConfig.Blacklist).ToList();
    }

    /**
     * Return every template id blacklisted in config/item.json/lootableItemBlacklist
     * @returns string array of blacklisted template ids
     */
    public List<string> GetBlacklistedLootableItems()
    {
        return _cloner.Clone(_itemConfig.LootableItemBlacklist).ToList();
    }

    /**
     * Check if the provided template id is boss item in config/item.json
     * @param tpl template id
     * @returns true if boss item
     */
    public bool BossItem(string tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /**
     * Return boss items in config/item.json
     * @returns string array of boss item template ids
     */
    public List<string> GetBossItems()
    {
        return _cloner.Clone(_itemConfig.BossItems).ToList();
    }

    /**
     * Check if the provided template id is blacklisted in config/item.json/lootableItemBlacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool IsLootableItemBlacklisted(string itemKey)
    {
        if (!_lootableItemBlacklistCache.Any()) HydrateLootableItemBlacklist();

        return _lootableItemBlacklistCache.Contains(itemKey);
    }

    public bool IsItemBlacklisted(string tpl)
    {
        if (!_itemBlacklistCache.Any()) HydrateBlacklist();

        return _itemBlacklistCache.Contains(tpl);
    }

    protected void HydrateLootableItemBlacklist()
    {
        foreach (var item in _itemConfig.LootableItemBlacklist) _lootableItemBlacklistCache.Add(item);
    }

    protected void HydrateBlacklist()
    {
        foreach (var item in _itemConfig.Blacklist) _itemBlacklistCache.Add(item);
    }

    /**
     * Check if the provided template id is boss item in config/item.json
     * @param tpl template id
     * @returns true if boss item
     */
    public bool IsBossItem(string tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /**
     * Check if item is blacklisted from being a reward for player
     * @param tpl item tpl to check is on blacklist
     * @returns True when blacklisted
     */
    public bool IsItemRewardBlacklisted(string tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }
}
