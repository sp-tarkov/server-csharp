using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Generators;

[Injectable]
public class PMCLootGenerator
{
    private readonly ConfigServer _configServer;
    private readonly DatabaseService _databaseService;
    private readonly ItemFilterService _itemFilterService;
    private readonly ItemHelper _itemHelper;
    private readonly ISptLogger<PMCLootGenerator> _logger;
    private readonly PmcConfig _pmcConfig;
    private readonly RagfairPriceService _ragfairPriceService;
    private readonly SeasonalEventService _seasonalEventService;
    private readonly WeightedRandomHelper _weightedRandomHelper;

    private Dictionary<string, double>? _backpackLootPool;
    private Dictionary<string, double>? _pocketLootPool;
    private Dictionary<string, double>? _vestLootPool;

    public PMCLootGenerator(
        ISptLogger<PMCLootGenerator> logger,
        DatabaseService databaseService,
        ItemHelper itemHelper,
        ItemFilterService itemFilterService,
        RagfairPriceService ragfairPriceService,
        SeasonalEventService seasonalEventService,
        WeightedRandomHelper weightedRandomHelper,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _databaseService = databaseService;
        _itemHelper = itemHelper;
        _itemFilterService = itemFilterService;
        _ragfairPriceService = ragfairPriceService;
        _seasonalEventService = seasonalEventService;
        _weightedRandomHelper = weightedRandomHelper;
        _configServer = configServer;

        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    ///     Create a List of loot items a PMC can have in their pockets
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCPocketLootPool(string botRole)
    {
        // Hydrate loot dictionary if empty
        if (_pocketLootPool is null)
        {
            _pocketLootPool = new Dictionary<string, double>();
            var items = _databaseService.GetItems();
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(botRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items.Pockets;

            var allowedItemTypeWhitelist = _pmcConfig.PocketLoot.Whitelist;

            var blacklist = GetLootBlacklist();

            var itemsToAdd = items.Where(
                item =>
                    allowedItemTypeWhitelist.Contains(item.Value.Parent) &&
                    _itemHelper.IsValidItem(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Parent) &&
                    ItemFitsInto1By2Slot(item.Value)
            ).Select(x => x.Key);

            foreach (var tpl in itemsToAdd)
                // If pmc has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var priceOverride))
                {
                    _pocketLootPool.Add(tpl, priceOverride);
                }
                else
                {
                    // Set price of item as its weight
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _pocketLootPool[tpl] = price ?? 0;
                }
            }

            var highestPrice = _pocketLootPool.Max(price => price.Value);
            foreach (var (key, _) in _pocketLootPool)
                // Invert price so cheapest has a larger weight
                // Times by highest price so most expensive item has weight of 1
            {
                _pocketLootPool[key] = Math.Round(1 / _pocketLootPool[key] * highestPrice);
            }

            _weightedRandomHelper.ReduceWeightValues(_pocketLootPool);
        }

        return _pocketLootPool;
    }

    private HashSet<string> GetLootBlacklist()
    {
        var blacklist = new HashSet<string>();
        blacklist.UnionWith(_pmcConfig.PocketLoot.Blacklist);
        blacklist.UnionWith(_pmcConfig.GlobalLootBlacklist);
        blacklist.UnionWith(_itemFilterService.GetBlacklistedItems());
        blacklist.UnionWith(_seasonalEventService.GetInactiveSeasonalEventItems());

        return blacklist;
    }

    /// <summary>
    ///     Create a List of loot items a PMC can have in their vests
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCVestLootPool(string botRole)
    {
        // Hydrate loot dictionary if empty
        if (_vestLootPool is null)
        {
            _vestLootPool = new Dictionary<string, double>();
            var items = _databaseService.GetItems();
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(botRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items.TacticalVest;

            var allowedItemTypeWhitelist = _pmcConfig.VestLoot.Whitelist;

            var blacklist = GetLootBlacklist();

            var itemsToAdd = items.Where(
                item =>
                    allowedItemTypeWhitelist.Contains(item.Value.Parent) &&
                    _itemHelper.IsValidItem(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Parent) &&
                    ItemFitsInto2By2Slot(item.Value)
            ).Select(x => x.Key);

            foreach (var tpl in itemsToAdd)
                // If pmc has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var overridePrice))
                {
                    _vestLootPool.Add(tpl, overridePrice);
                }
                else
                {
                    // Set price of item as its weight
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _vestLootPool[tpl] = price ?? 0;
                }
            }

            var highestPrice = _vestLootPool.Max(price => price.Value);
            foreach (var (key, _) in _vestLootPool)
                // Invert price so cheapest has a larger weight
                // Times by highest price so most expensive item has weight of 1
            {
                _vestLootPool[key] = Math.Round(1 / _vestLootPool[key] * highestPrice);
            }

            _weightedRandomHelper.ReduceWeightValues(_vestLootPool);
        }

        return _vestLootPool;
    }

    /// <summary>
    ///     Check if item has a width/height that lets it fit into a 2x2 slot
    ///     1x1 / 1x2 / 2x1 / 2x2
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto2By2Slot(TemplateItem item)
    {
        return item.Properties.Width <= 2 && item.Properties.Height <= 2;
    }

    /// <summary>
    ///     Check if item has a width/height that lets it fit into a 1x2 slot
    ///     1x1 / 1x2 / 2x1
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto1By2Slot(TemplateItem item)
    {
        return $"{item.Properties.Width}x{item.Properties.Height}" switch
        {
            "1x1" or "1x2" or "2x1" => true,
            _ => false
        };
    }

    /// <summary>
    ///     Create a List of loot items a PMC can have in their backpack
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCBackpackLootPool(string botRole)
    {
        // Hydrate loot dictionary if empty
        if (_backpackLootPool is null)
        {
            _backpackLootPool = new Dictionary<string, double>();
            var items = _databaseService.GetItems();
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(botRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items.Backpack;

            var allowedItemTypeWhitelist = _pmcConfig.BackpackLoot.Whitelist;

            var blacklist = GetLootBlacklist();

            var itemsToAdd = items.Where(
                item =>
                    allowedItemTypeWhitelist.Contains(item.Value.Parent) &&
                    _itemHelper.IsValidItem(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Id) &&
                    !blacklist.Contains(item.Value.Parent)
            ).Select(x => x.Key);

            foreach (var tpl in itemsToAdd)
                // If pmc has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var priceOverride))
                {
                    _backpackLootPool.Add(tpl, priceOverride);
                }
                else
                {
                    // Set price of item as its weight
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _backpackLootPool[tpl] = price ?? 0;
                }
            }

            var highestPrice = _backpackLootPool.Max(price => price.Value);
            foreach (var (key, _) in _backpackLootPool)
                // Invert price so cheapest has a larger weight
                // Times by highest price so most expensive item has weight of 1
            {
                _backpackLootPool[key] = Math.Round(1 / _backpackLootPool[key] * highestPrice);
            }

            _weightedRandomHelper.ReduceWeightValues(_backpackLootPool);
        }

        return _backpackLootPool;
    }
}
