using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Centralise the handling of blacklisting items, uses blacklist found in config/item.json, stores items that should not be used by players / broken items
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemFilterService(ISptLogger<ItemFilterService> logger, ConfigServer configServer)
{
    protected readonly ItemConfig _itemConfig = configServer.GetConfig<ItemConfig>();

    protected readonly HashSet<MongoId> _itemBlacklistCache = [];
    protected readonly HashSet<MongoId> _lootableItemBlacklistCache = [];

    /// <summary>
    ///     Get an HashSet of items that should never be given as a reward to player
    /// </summary>
    /// <returns>HashSet of item tpls</returns>
    public HashSet<MongoId> GetItemRewardBlacklist()
    {
        return _itemConfig.RewardItemBlacklist;
    }

    /// <summary>
    ///     Get an HashSet of item types that should never be given as a reward to player
    /// </summary>
    /// <returns>HashSet of item base ids</returns>
    public HashSet<MongoId> GetItemRewardBaseTypeBlacklist()
    {
        return _itemConfig.RewardItemTypeBlacklist;
    }

    /// <summary>
    ///     Return every template id blacklisted in config/item.json
    /// </summary>
    /// <returns>HashSet of blacklisted template ids</returns>
    public HashSet<MongoId> GetBlacklistedItems()
    {
        return _itemConfig.Blacklist;
    }

    /// <summary>
    ///     Return every template id blacklisted in config/item.json/lootableItemBlacklist
    /// </summary>
    /// <returns>HashSet of blacklisted template ids</returns>
    public HashSet<MongoId> GetBlacklistedLootableItems()
    {
        return _itemConfig.LootableItemBlacklist;
    }

    /// <summary>
    ///     Return boss items in config/item.json
    /// </summary>
    /// <returns>HashSet of boss item template ids</returns>
    public HashSet<MongoId> GetBossItems()
    {
        return _itemConfig.BossItems;
    }

    /// <summary>
    /// Add MongoIds to the global lootable item blacklist cache
    /// </summary>
    /// <param name="itemTplsToBlacklist">Tpls to blacklist</param>
    public void AddItemToLootableBlacklistCache(IEnumerable<MongoId> itemTplsToBlacklist)
    {
        _lootableItemBlacklistCache.UnionWith(itemTplsToBlacklist);
    }

    /// <summary>
    ///     Check if the provided template id is blacklisted in config/item.json/lootableItemBlacklist
    /// </summary>
    /// <param name="itemKey"> Template id</param>
    /// <returns>True if blacklisted</returns>
    public bool IsLootableItemBlacklisted(MongoId itemKey)
    {
        if (!_lootableItemBlacklistCache.Any())
        {
            _lootableItemBlacklistCache.UnionWith(_itemConfig.LootableItemBlacklist);
        }

        return _lootableItemBlacklistCache.Contains(itemKey);
    }

    /// <summary>
    /// Add MongoIds to the global blacklist cache
    /// </summary>
    /// <param name="itemTplsToBlacklist">Tpls to blacklist</param>
    public void AddItemToBlacklistCache(IEnumerable<MongoId> itemTplsToBlacklist)
    {
        _itemBlacklistCache.UnionWith(itemTplsToBlacklist);
    }

    public bool IsItemBlacklisted(MongoId tpl)
    {
        if (!_itemBlacklistCache.Any())
        {
            _itemBlacklistCache.UnionWith(_itemConfig.Blacklist);
        }

        return _itemBlacklistCache.Contains(tpl);
    }

    /// <summary>
    ///     Check if the provided template id is boss item in config/item.json
    /// </summary>
    /// <param name="tpl"> Template id</param>
    /// <returns>True if boss item</returns>
    public bool IsBossItem(MongoId tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /// <summary>
    ///     Check if item is blacklisted from being a reward for player
    /// </summary>
    /// <param name="tpl"> Item tpl to check is on blacklist </param>
    /// <returns>true when blacklisted</returns>
    public bool IsItemRewardBlacklisted(MongoId tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }
}
