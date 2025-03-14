using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;
/// <summary>
/// Centralise the handling of blacklisting items, uses blacklist found in config/item.json, stores items that should not be used by players / broken items
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ItemFilterService(
    ISptLogger<ItemFilterService> _logger,
    ICloner _cloner,
    ConfigServer _configServer
)
{
    protected HashSet<string>? _itemBlacklistCache = [];
    protected ItemConfig _itemConfig = _configServer.GetConfig<ItemConfig>();

    protected HashSet<string>? _lootableItemBlacklistCache = [];

    /// <summary>
    /// Check if the provided template id is blacklisted in config/item.json/blacklist
    /// </summary>
    /// <param name="tpl"> Template id</param>
    /// <returns> True if blacklisted </returns>
    public bool ItemBlacklisted(string tpl)
    {
        if (_itemBlacklistCache.Count == 0)
        {
            _itemBlacklistCache.UnionWith(_itemConfig.Blacklist);
        }

        return _itemBlacklistCache.Contains(tpl);
    }

    /// <summary>
    /// Check if item is blacklisted from being a reward for player
    /// </summary>
    /// <param name="tpl"> Item tpl to check is on blacklist </param>
    /// <returns> True when blacklisted </returns>
    public bool ItemRewardBlacklisted(string tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }

    /// <summary>
    /// Get an HashSet of items that should never be given as a reward to player
    /// </summary>
    /// <returns> HashSet of item tpls </returns>
    public HashSet<string> GetItemRewardBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemBlacklist);
    }

    /// <summary>
    /// Get an HashSet of item types that should never be given as a reward to player
    /// </summary>
    /// <returns> HashSet of item base ids </returns>
    public HashSet<string> GetItemRewardBaseTypeBlacklist()
    {
        return _cloner.Clone(_itemConfig.RewardItemTypeBlacklist);
    }

    /// <summary>
    /// Return every template id blacklisted in config/item.json
    /// </summary>
    /// <returns> HashSet of blacklisted template ids </returns>
    public HashSet<string> GetBlacklistedItems()
    {
        return _cloner.Clone(_itemConfig.Blacklist);
    }

    /// <summary>
    /// Return every template id blacklisted in config/item.json/lootableItemBlacklist
    /// </summary>
    /// <returns> HashSet of blacklisted template ids </returns>
    public HashSet<string> GetBlacklistedLootableItems()
    {
        return _cloner.Clone(_itemConfig.LootableItemBlacklist);
    }

    /// <summary>
    /// Check if the provided template id is boss item in config/item.json
    /// </summary>
    /// <param name="tpl"> template id </param>
    /// <returns> True if boss item </returns>
    public bool BossItem(string tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /// <summary>
    /// Return boss items in config/item.json
    /// </summary>
    /// <returns> HashSet of boss item template ids </returns>
    public HashSet<string> GetBossItems()
    {
        return _cloner.Clone(_itemConfig.BossItems).ToHashSet();
    }

    /// <summary>
    ///  Check if the provided template id is blacklisted in config/item.json/lootableItemBlacklist
    /// </summary>
    /// <param name="itemKey"> Template id</param>
    /// <returns> True if blacklisted </returns>
    public bool IsLootableItemBlacklisted(string itemKey)
    {
        if (!_lootableItemBlacklistCache.Any())
        {
            HydrateLootableItemBlacklist();
        }

        return _lootableItemBlacklistCache.Contains(itemKey);
    }

    public bool IsItemBlacklisted(string tpl)
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

    /// <summary>
    /// Check if the provided template id is boss item in config/item.json
    /// </summary>
    /// <param name="tpl"> Template id</param>
    /// <returns> True if boss item </returns>
    public bool IsBossItem(string tpl)
    {
        return _itemConfig.BossItems.Contains(tpl);
    }

    /// <summary>
    /// Check if item is blacklisted from being a reward for player
    /// </summary>
    /// <param name="tpl"> Item tpl to check is on blacklist </param>
    /// <returns> true when blacklisted </returns>
    public bool IsItemRewardBlacklisted(string tpl)
    {
        return _itemConfig.RewardItemBlacklist.Contains(tpl);
    }
}
