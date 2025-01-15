using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ItemFilterService
{
    private readonly ILogger _logger;
    private readonly ICloner _cloner;
    private readonly DatabaseServer _databaseServer;
    private readonly ConfigServer _configServer;

    private readonly HashSet<string> _lootableItemBlacklistCache = [];
    private readonly ItemConfig _itemConfig;

    public ItemFilterService(
        ILogger logger,
        ICloner cloner,
        DatabaseServer databaseServer,
        ConfigServer configServer
        )
    {
        _logger = logger;
        _cloner = cloner;
        _databaseServer = databaseServer;
        _configServer = configServer;

        _itemConfig = _configServer.GetConfig<ItemConfig>(ConfigTypes.ITEM);
    }

    /**
     * Check if the provided template id is blacklisted in config/item.json/blacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool ItemBlacklisted(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Check if the provided template id is blacklisted in config/item.json/lootableItemBlacklist
     * @param tpl template id
     * @returns true if blacklisted
     */
    public bool LootableItemBlacklisted(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Check if item is blacklisted from being a reward for player
     * @param tpl item tpl to check is on blacklist
     * @returns True when blacklisted
     */
    public bool ItemRewardBlacklisted(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Get an array of items that should never be given as a reward to player
     * @returns string array of item tpls
     */
    public List<string> GetItemRewardBlacklist()
    {
        throw new NotImplementedException();
    }

    /**
     * Get an array of item types that should never be given as a reward to player
     * @returns string array of item base ids
     */
    public List<string> GetItemRewardBaseTypeBlacklist()
    {
        throw new NotImplementedException();
    }

    /**
     * Return every template id blacklisted in config/item.json
     * @returns string array of blacklisted template ids
     */
    public List<string> GetBlacklistedItems()
    {
        throw new NotImplementedException();
    }

    /**
     * Return every template id blacklisted in config/item.json/lootableItemBlacklist
     * @returns string array of blacklisted template ids
     */
    public List<string> GetBlacklistedLootableItems()
    {
        throw new NotImplementedException();
    }

    /**
     * Check if the provided template id is boss item in config/item.json
     * @param tpl template id
     * @returns true if boss item
     */
    public bool BossItem(string tpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Return boss items in config/item.json
     * @returns string array of boss item template ids
     */
    public List<string> GetBossItems()
    {
        throw new NotImplementedException();
    }

    public bool IsLootableItemBlacklisted(string itemKey)
    {
        if (_lootableItemBlacklistCache.Count == 0)
        {
            foreach (var item in _itemConfig.LootableItemBlacklist) {
                _lootableItemBlacklistCache.Add(item);
            }
        }

        return _lootableItemBlacklistCache.Contains(itemKey);
    }
}
