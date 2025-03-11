using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ItemFilterService(
    ISptLogger<ItemFilterService> _logger,
    ICloner _cloner,
    ConfigServer _configServer
)
{
    protected HashSet<MongoId>? _itemBlacklistCache = [];
    protected ItemConfig _itemConfig = _configServer.GetConfig<ItemConfig>();

    protected HashSet<MongoId>? _lootableItemBlacklistCache = [];

    /**
     * Check if the provided template id is blacklisted in config/item.json/blacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool ItemBlacklisted(MongoId tpl)
    {
        if (_itemBlacklistCache.Count == 0)
        {
            _itemBlacklistCache.UnionWith(_itemConfig.Blacklist);
        }

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
    public HashSet<MongoId> GetItemRewardBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemBlacklist);
    }

    /**
     * Get an array of item types that should never be given as a reward to player
     * @returns string array of item base ids
     */
    public HashSet<MongoId> GetItemRewardBaseTypeBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemTypeBlacklist);
    }

    /**
     * Return every template id blacklisted in config/item.json
     * @returns string array of blacklisted template ids
     */
    public HashSet<MongoId> GetBlacklistedItems()
    {
        return _cloner.Clone(_itemConfig.Blacklist);
    }

    /**
     * Return every template id blacklisted in config/item.json/lootableItemBlacklist
     * @returns string array of blacklisted template ids
     */
    public HashSet<MongoId> GetBlacklistedLootableItems()
    {
        return _cloner.Clone(_itemConfig.LootableItemBlacklist);
    }

    /**
     * Check if the provided template id is boss item in config/item.json
     * @param tpl template id
     * @returns true if boss item
     */
    public bool BossItem(MongoId tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /**
     * Return boss items in config/item.json
     * @returns string array of boss item template ids
     */
    public HashSet<MongoId> GetBossItems()
    {
        return _cloner.Clone(_itemConfig.BossItems).ToHashSet();
    }

    /**
     * Check if the provided template id is blacklisted in config/item.json/lootableItemBlacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool IsLootableItemBlacklisted(MongoId itemKey)
    {
        if (!_lootableItemBlacklistCache.Any())
        {
            HydrateLootableItemBlacklist();
        }

        return _lootableItemBlacklistCache.Contains(itemKey);
    }

    public bool IsItemBlacklisted(MongoId tpl)
    {
        if (!_itemBlacklistCache.Any())
        {
            HydrateBlacklist();
        }

        return _itemBlacklistCache.Contains(tpl);
    }

    protected void HydrateLootableItemBlacklist()
    {
        foreach (var item in _itemConfig.LootableItemBlacklist)
        {
            _lootableItemBlacklistCache.Add(item);
        }
    }

    protected void HydrateBlacklist()
    {
        foreach (var item in _itemConfig.Blacklist)
        {
            _itemBlacklistCache.Add(item);
        }
    }

    /**
     * Check if the provided template id is boss item in config/item.json
     * @param tpl template id
     * @returns true if boss item
     */
    public bool IsBossItem(MongoId tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /**
     * Check if item is blacklisted from being a reward for player
     * @param tpl item tpl to check is on blacklist
     * @returns True when blacklisted
     */
    public bool IsItemRewardBlacklisted(MongoId tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }
}
