using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotLootCacheService(
    ISptLogger<BotLootCacheService> _logger,
    ItemHelper _itemHelper,
    PMCLootGenerator _pmcLootGenerator,
    LocalisationService _localisationService,
    ICloner _cloner
)
{
    protected ConcurrentDictionary<string, BotLootCache> _lootCache = new();
    private readonly Lock _drugLock = new();
    private readonly Lock _foodLock = new();
    private readonly Lock _drinkLock = new();
    private readonly Lock _currencyLock = new();
    private readonly Lock _stimLock = new();
    private readonly Lock _grenadeLock = new();
    private readonly Lock _specialLock = new();
    private readonly Lock _healingLock = new();

    /// <summary>
    ///     Remove cached bot loot data
    /// </summary>
    public void ClearCache()
    {
        _lootCache.Clear();
    }

    /// <summary>
    ///     Get a dictionary of lootable item Tpls with their corresponding weight
    /// </summary>
    /// <param name="botRole">bot to get loot for</param>
    /// <param name="isPmc">is the bot a pmc</param>
    /// <param name="lootType">what type of loot is needed (backpack/pocket/stim/vest etc)</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="itemPriceMinMax">OPTIONAL - item price min and max value filter</param>
    /// <remarks>THIS IS NOT A THREAD SAFE METHOD</remarks>
    /// <returns>dictionary</returns>
    public Dictionary<string, double> GetLootFromCache(
        string botRole,
        bool isPmc,
        string lootType,
        BotType botJsonTemplate,
        MinMax<double>? itemPriceMinMax = null)
    {
        if (!BotRoleExistsInCache(botRole))
        {
            InitCacheForBotRole(botRole);
            AddLootToCache(botRole, isPmc, botJsonTemplate);
        }

        if(!_lootCache.TryGetValue(botRole, out var botRoleCache))
        {
            _logger.Error($"Unable to find: {botRole} in loot cache");
            return [];
        }

        Dictionary<string, double> result;
        switch (lootType)
        {
            case LootCacheType.Special:
                result = botRoleCache.SpecialItems;
                break;
            case LootCacheType.Backpack:
                result = botRoleCache.BackpackLoot;
                break;
            case LootCacheType.Pocket:
                result = botRoleCache.PocketLoot;
                break;
            case LootCacheType.Vest:
                result = botRoleCache.VestLoot;
                break;
            case LootCacheType.Secure:
                result = botRoleCache.SecureLoot;
                break;
            case LootCacheType.Combined:
                result = botRoleCache.CombinedPoolLoot;
                break;
            case LootCacheType.HealingItems:
                result = botRoleCache.HealingItems;
                break;
            case LootCacheType.GrenadeItems:
                result = botRoleCache.GrenadeItems;
                break;
            case LootCacheType.DrugItems:
                result = botRoleCache.DrugItems;
                break;
            case LootCacheType.FoodItems:
                result = botRoleCache.FoodItems;
                break;
            case LootCacheType.DrinkItems:
                result = botRoleCache.DrinkItems;
                break;
            case LootCacheType.CurrencyItems:
                result = botRoleCache.CurrencyItems;
                break;
            case LootCacheType.StimItems:
                result = botRoleCache.StimItems;
                break;
            default:
                _logger.Error(
                    _localisationService.GetText(
                        "bot-loot_type_not_found",
                        new
                        {
                            lootType,
                            botRole,
                            isPmc
                        }
                    )
                );

                return [];
        }

        if (!result.Any())
        {
            // No loot, exit
            return result;
        }

        if (itemPriceMinMax is null)
        {
            // No filtering requested, exit
            return _cloner.Clone(result);
        }

        // Filter the loot pool prior to returning
        var filteredResult = result.Where(i =>
            {
                var itemPrice = _itemHelper.GetItemPrice(i.Key);
                if (itemPriceMinMax?.Min is not null && itemPriceMinMax?.Max is not null)
                {
                    return itemPrice >= itemPriceMinMax?.Min && itemPrice <= itemPriceMinMax?.Max;
                }

                if (itemPriceMinMax?.Min is not null && itemPriceMinMax?.Max is null)
                {
                    return itemPrice >= itemPriceMinMax?.Min;
                }

                if (itemPriceMinMax?.Min is null && itemPriceMinMax?.Max is not null)
                {
                    return itemPrice <= itemPriceMinMax?.Max;
                }

                return false;
            }
        );

        return _cloner.Clone(filteredResult.ToDictionary(pair => pair.Key, pair => pair.Value));

    }

    /// <summary>
    ///     Generate loot for a bot and store inside a private class property
    /// </summary>
    /// <param name="botRole">bots role (assault / pmcBot etc)</param>
    /// <param name="isPmc">Is the bot a PMC (alters what loot is cached)</param>
    /// <param name="botJsonTemplate">db template for bot having its loot generated</param>
    protected void AddLootToCache(string botRole, bool isPmc, BotType botJsonTemplate)
    {
        // Full pool of loot we use to create the various sub-categories with
        var lootPool = botJsonTemplate.BotInventory.Items;

        // Flatten all individual slot loot pools into one big pool, while filtering out potentially missing templates
        Dictionary<string, double> specialLootPool = new();
        Dictionary<string, double> backpackLootPool = new();
        Dictionary<string, double> pocketLootPool = new();
        Dictionary<string, double> vestLootPool = new();
        Dictionary<string, double> secureLootPool = new();
        Dictionary<string, double> combinedLootPool = new();

        if (isPmc)
        {
            // Replace lootPool from bot json with our own generated list for PMCs
            lootPool.Backpack = _cloner.Clone(_pmcLootGenerator.GeneratePMCBackpackLootPool(botRole));
            lootPool.Pockets = _cloner.Clone(_pmcLootGenerator.GeneratePMCPocketLootPool(botRole));
            lootPool.TacticalVest = _cloner.Clone(_pmcLootGenerator.GeneratePMCVestLootPool(botRole));
        }

        // Backpack/Pockets etc
        var poolsToProcess =
            new Dictionary<string, Dictionary<string, double>>
            {
                { "Backpack", lootPool.Backpack },
                { "Pockets", lootPool.Pockets },
                { "SecuredContainer", lootPool.SecuredContainer },
                { "SpecialLoot", lootPool.SpecialLoot },
                { "TacticalVest", lootPool.TacticalVest }
            };


        foreach (var (containerType, itemPool) in poolsToProcess)
        {
            // No items to add, skip
            if (itemPool.Count == 0)
            {
                continue;
            }

            // Sort loot pool into separate buckets
            switch (containerType)
            {
                case "SpecialLoot":
                    AddItemsToPool(specialLootPool, itemPool);
                    break;
                case "Pockets":
                    AddItemsToPool(pocketLootPool, itemPool);
                    break;
                case "TacticalVest":
                    AddItemsToPool(vestLootPool, itemPool);
                    break;
                case "SecuredContainer":
                    AddItemsToPool(secureLootPool, itemPool);
                    break;
                case "Backpack":
                    AddItemsToPool(backpackLootPool, itemPool);
                    break;
                default:
                    _logger.Warning($"How did you get here {containerType}");
                    break;
            }

            // If pool has items and items were going into a non-secure container pool, add to combined
            if (itemPool.Count > 0 && !containerType.Equals("securedcontainer", StringComparison.OrdinalIgnoreCase))
            {
                // fill up 'combined' pool of all loot
                AddItemsToPool(combinedLootPool, itemPool);
            }
        }

        // Assign whitelisted special items to bot if any exist
        var (specialLootItems, addSpecialLootItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.SpecialItems?.Whitelist);
        if (addSpecialLootItems) // key = tpl, value = weight
        {
            // No whitelist, find and assign from combined item pool
            foreach (var itemKvP in specialLootPool)
            {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (!(IsBulletOrGrenade(itemTemplate.Properties) || IsMagazine(itemTemplate.Properties)))
                {
                    lock (_specialLock)
                    {
                        specialLootItems.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }
        }

        var (healingItemsInWhitelist, addHealingItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Healing?.Whitelist);
        var (drugItemsInWhitelist, addDrugItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Drugs?.Whitelist);
        var (foodItemsInWhitelist, addFoodItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Food?.Whitelist);
        var (drinkItemsInWhitelist, addDrinkItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Food?.Whitelist);
        var (currencyItemsInWhitelist, addCurrencyItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Currency?.Whitelist);
        var (stimItemsInWhitelist, addStimItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Stims?.Whitelist);
        var (grenadeItemsInWhitelist, addGrenadeItems) = GetGenerationWeights(botJsonTemplate.BotGeneration?.Items?.Grenades?.Whitelist);

        foreach (var itemKvP in combinedLootPool)
        {
            var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
            if (itemTemplate is null)
            {
                continue;
            }

            if (addHealingItems)
            {
                // Whitelist has no healing items, hydrate it using items from combinedLootPool that meet criteria
                if (
                    IsMedicalItem(itemTemplate.Properties) &&
                    itemTemplate.Parent != BaseClasses.STIMULATOR &&
                    itemTemplate.Parent != BaseClasses.DRUGS
                )
                {
                    lock (_healingLock)
                    {
                        healingItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addDrugItems)
            {
                if (itemTemplate.Parent == BaseClasses.DRUGS && IsMedicalItem(itemTemplate.Properties))
                {
                    lock (_drugLock)
                    {
                        drugItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addFoodItems)
            {
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.FOOD))
                {
                    lock (_foodLock)
                    {
                        foodItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addDrinkItems)
            {
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.DRINK))
                {
                    lock (_drinkLock)
                    {
                        drinkItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addCurrencyItems)
            {
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.MONEY))
                {
                    lock (_currencyLock)
                    {
                        currencyItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addStimItems)
            {
                if (itemTemplate.Parent == BaseClasses.STIMULATOR && IsMedicalItem(itemTemplate.Properties))
                {
                    lock (_stimLock)
                    {
                        stimItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }

            if (addGrenadeItems)
            {
                if (IsGrenade(itemTemplate.Properties))
                {
                    lock (_grenadeLock)
                    {
                        grenadeItemsInWhitelist.TryAdd(itemKvP.Key, itemKvP.Value);
                    }
                }
            }
        }

        // Get backpack loot (excluding magazines, bullets, grenades, drink, food and healing/stim items)
        var filteredBackpackItems = FilterItemPool(backpackLootPool, (itemTemplate) =>
            IsBulletOrGrenade(itemTemplate.Properties) ||
            IsMagazine(itemTemplate.Properties) ||
            IsMedicalItem(itemTemplate.Properties) ||
            IsGrenade(itemTemplate.Properties) ||
            IsFood(itemTemplate.Id) ||
            IsDrink(itemTemplate.Id) ||
            IsCurrency(itemTemplate.Id));

        // Get pocket loot (excluding magazines, bullets, grenades, drink, food medical and healing/stim items)
        var filteredPocketItems = FilterItemPool(pocketLootPool, (itemTemplate) =>
            IsBulletOrGrenade(itemTemplate.Properties) ||
            IsMagazine(itemTemplate.Properties) ||
            IsMedicalItem(itemTemplate.Properties) ||
            IsGrenade(itemTemplate.Properties) ||
            IsFood(itemTemplate.Id) ||
            IsDrink(itemTemplate.Id) ||
            IsCurrency(itemTemplate.Id) ||
            itemTemplate.Properties.Height is null || // lacks height
            itemTemplate.Properties.Width is null); // lacks width

        // Get vest loot (excluding magazines, bullets, grenades, medical and healing/stim items)
        var filteredVestItems = new Dictionary<string, double>();
        foreach (var itemKvP in vestLootPool)
        {
            var itemResult = _itemHelper.GetItem(itemKvP.Key);
            if (itemResult.Value is null)
            {
                continue;
            }

            var itemTemplate = itemResult.Value;
            if (
                IsBulletOrGrenade(itemTemplate.Properties) ||
                IsMagazine(itemTemplate.Properties) ||
                IsMedicalItem(itemTemplate.Properties) ||
                IsGrenade(itemTemplate.Properties) ||
                IsFood(itemTemplate.Id) ||
                IsDrink(itemTemplate.Id) ||
                IsCurrency(itemTemplate.Id)
            )
            {
                continue;
            }

            filteredVestItems.TryAdd(itemKvP.Key, itemKvP.Value);
        }

        // Get secure loot (excluding magazines, bullets)
        var filteredSecureLoot = FilterItemPool(secureLootPool, (itemTemplate) =>
            IsBulletOrGrenade(itemTemplate.Properties) ||
            IsMagazine(itemTemplate.Properties));

        if(!_lootCache.TryGetValue(botRole, out var cacheForRole))
        {
            _logger.Error($"Unable to get loot cache value using key: {botRole}");

            return;
        }

        cacheForRole.HealingItems = healingItemsInWhitelist;
        cacheForRole.DrugItems = drugItemsInWhitelist;
        cacheForRole.FoodItems = foodItemsInWhitelist;
        cacheForRole.DrinkItems = drinkItemsInWhitelist;
        cacheForRole.CurrencyItems = currencyItemsInWhitelist;
        cacheForRole.StimItems = stimItemsInWhitelist;
        cacheForRole.GrenadeItems = grenadeItemsInWhitelist;
        cacheForRole.SpecialItems = specialLootItems;
        cacheForRole.BackpackLoot = filteredBackpackItems;
        cacheForRole.PocketLoot = filteredPocketItems;
        cacheForRole.VestLoot = filteredVestItems;
        cacheForRole.SecureLoot = filteredSecureLoot;
    }

    /// <summary>
    /// Helper function - Filter out items from passed in pool based on a passed in delegate
    /// </summary>
    /// <param name="lootPool">Pool to filter</param>
    /// <param name="shouldBeSkipped">Delegate to filter pool by</param>
    /// <returns></returns>
    protected Dictionary<string, double> FilterItemPool(Dictionary<string, double> lootPool, Func<TemplateItem, bool> shouldBeSkipped)
    {
        var filteredItems = new Dictionary<string, double>();
        foreach (var (itemTpl, itemWeight) in lootPool)
        {
            var (isValidItem, itemTemplate) = _itemHelper.GetItem(itemTpl);
            if (!isValidItem)
            {
                continue;
            }

            if (shouldBeSkipped(itemTemplate))
            {
                continue;
            }

            filteredItems.TryAdd(itemTpl, itemWeight);
        }

        return filteredItems;
    }

    /// <summary>
    /// Return provided weights or an empty dictionary
    /// </summary>
    /// <param name="weights">Weights to return</param>
    /// <returns>Dictionary and should pool be hydrated by items in combined loot pool</returns>
    protected static (Dictionary<string, double>, bool populateFromCombinedPool) GetGenerationWeights(Dictionary<string, double>? weights)
    {
        var result = weights ?? [];
        return (result, !result.Any()); // empty dict = should be populated from combined pool
    }

    /// <summary>
    /// merge item tpls + weightings to passed in dictionary
    /// If exits already, skip
    /// </summary>
    /// <param name="poolToAddTo">Dictionary to add item to</param>
    /// <param name="poolOfItemsToAdd">Dictionary of items to add</param>
    protected void AddItemsToPool(Dictionary<string, double> poolToAddTo, Dictionary<string, double> poolOfItemsToAdd)
    {
        foreach (var (tpl, weight) in poolOfItemsToAdd)
        {
            // Skip adding items that already exist
            poolToAddTo.TryAdd(tpl, weight);
        }
    }

    /// <summary>
    ///     Ammo/grenades have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsBulletOrGrenade(Props props)
    {
        return props.AmmoType is not null;
    }

    /// <summary>
    ///     Internal and external magazine have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMagazine(Props props)
    {
        return props.ReloadMagType is not null;
    }

    /// <summary>
    ///     Medical use items (e.g. morphine/lip balm/grizzly)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMedicalItem(Props props)
    {
        return props.MedUseTime is not null;
    }

    /// <summary>
    ///     Grenades have this property (e.g. smoke/frag/flash grenades)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsGrenade(Props props)
    {
        return props.ThrowType is not null;
    }

    protected bool IsFood(string tpl)
    {
        return _itemHelper.IsOfBaseclass(tpl, BaseClasses.FOOD);
    }

    protected bool IsDrink(string tpl)
    {
        return _itemHelper.IsOfBaseclass(tpl, BaseClasses.DRINK);
    }

    protected bool IsCurrency(string tpl)
    {
        return _itemHelper.IsOfBaseclass(tpl, BaseClasses.MONEY);
    }

    /// <summary>
    ///     Check if a bot type exists inside the loot cache
    /// </summary>
    /// <param name="botRole">role to check for</param>
    /// <returns>true if they exist</returns>
    protected bool BotRoleExistsInCache(string botRole)
    {
        return _lootCache.ContainsKey(botRole);
    }

    /// <summary>
    ///     If lootcache is undefined, init with empty property arrays
    /// </summary>
    /// <param name="botRole">Bot role to hydrate</param>
    protected void InitCacheForBotRole(string botRole)
    {
        _lootCache.TryAdd(botRole, new BotLootCache());
    }

    /// <summary>
    ///     Compares two item prices by their flea (or handbook if that doesn't exist) price
    /// </summary>
    /// <param name="itemAPrice"></param>
    /// <param name="itemBPrice"></param>
    /// <returns></returns>
    protected int CompareByValue(int itemAPrice, int itemBPrice)
    {
        // If item A has no price, it should be moved to the back when sorting
        if (itemAPrice is 0)
        {
            return 1;
        }

        if (itemBPrice is 0)
        {
            return -1;
        }

        if (itemAPrice < itemBPrice)
        {
            return -1;
        }

        if (itemAPrice > itemBPrice)
        {
            return 1;
        }

        return 0;
    }
}
