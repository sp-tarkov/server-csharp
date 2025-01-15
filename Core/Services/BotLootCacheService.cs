using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotLootCacheService
{
    private readonly ILogger _logger;
    private readonly ItemHelper _itemHelper;
    private readonly PMCLootGenerator _pmcLootGenerator;
    private readonly RagfairPriceService _ragfairPriceService;
    private readonly ICloner _cloner;

    private readonly Dictionary<string, BotLootCache> _lootCache = new();

    public BotLootCacheService(
        ILogger logger,
        ItemHelper itemHelper,
        PMCLootGenerator pmcLootGenerator,
        RagfairPriceService ragfairPriceService,
        ICloner cloner
        )
    {
        _logger = logger;
        _itemHelper = itemHelper;
        _pmcLootGenerator = pmcLootGenerator;
        _ragfairPriceService = ragfairPriceService;
        _cloner = cloner;
    }

    /// <summary>
    /// Remove cached bot loot data
    /// </summary>
    public void ClearCache()
    {
        _lootCache.Clear();
    }

    /// <summary>
    /// Get the fully created loot array, ordered by price low to high
    /// </summary>
    /// <param name="botRole">bot to get loot for</param>
    /// <param name="isPmc">is the bot a pmc</param>
    /// <param name="lootType">what type of loot is needed (backpack/pocket/stim/vest etc)</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <returns>Dictionary<string, int></returns>
    public Dictionary<string, int> GetLootFromCache(
        string botRole,
        bool isPmc,
        string lootType,
        BotType botJsonTemplate,
        MinMax? itemPriceMinMax = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate loot for a bot and store inside a private class property
    /// </summary>
    /// <param name="botRole">bots role (assault / pmcBot etc)</param>
    /// <param name="isPmc">Is the bot a PMC (alteres what loot is cached)</param>
    /// <param name="botJsonTemplate">db template for bot having its loot generated</param>
    protected void AddLootToCache(string botRole, bool isPmc, BotType botJsonTemplate)
    {
        // Full pool of loot we use to create the various sub-categories with
        var lootPool = botJsonTemplate.BotInventory.Items;

        // Flatten all individual slot loot pools into one big pool, while filtering out potentially missing templates
        Dictionary<string, double> specialLootPool = new();
        Dictionary<string, double> backpackLootPool= new();
        Dictionary<string, double> pocketLootPool = new();
        Dictionary<string, double> vestLootPool = new();
        Dictionary<string, double> secureLootTPool = new();
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
        

        foreach (var kvp in poolsToProcess)
        {
            // No items to add, skip
            if (kvp.Value.Count == 0)
            {
                continue;
            }

            // Sort loot pool into separate buckets
            switch (kvp.Key)
            {
                case "specialloot":
                    AddItemsToPool(specialLootPool, kvp.Value);
                    break;
                case "pockets":
                    AddItemsToPool(pocketLootPool, kvp.Value);
                    break;
                case "tacticalvest":
                    AddItemsToPool(vestLootPool, kvp.Value);
                    break;
                case "securedcontainer":
                    AddItemsToPool(secureLootTPool, kvp.Value);
                    break;
                case "backpack":
                    AddItemsToPool(backpackLootPool, kvp.Value);
                    break;
                default:
                    _logger.Warning($"How did you get here {kvp.Key}");
                    break;
            }

            // Add all items (if any) to combined pool (excluding secure)
            if (kvp.Value.Count > 0 && kvp.Key.ToLower() != "securedcontainer")
            {
                AddItemsToPool(combinedLootPool, kvp.Value);
            }
        }

        // Assign whitelisted special items to bot if any exist
        var specialLootItems =
            botJsonTemplate.BotGeneration.Items.SpecialItems.Whitelist.Count > 0
                ? botJsonTemplate.BotGeneration.Items.SpecialItems.Whitelist
                : new Dictionary<string, double>();

        // no whitelist, find and assign from combined item pool
        if (!specialLootItems.Any())
        {
            // key = tpl, value = weight
            foreach (var itemKvP in specialLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (!(IsBulletOrGrenade(itemTemplate.Properties) || IsMagazine(itemTemplate.Properties)))
                {
                    specialLootItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted healing items to bot if any exist
        var healingItems =
            botJsonTemplate.BotGeneration.Items.Healing.Whitelist.Count > 0
                ? botJsonTemplate.BotGeneration.Items.Healing.Whitelist
                : new Dictionary<string, double>();

        // No whitelist, find and assign from combined item pool
        if (!healingItems.Any())
        {
            // key = tpl, value = weight
            foreach (var itemKvP in combinedLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (
                    IsMedicalItem(itemTemplate.Properties) &&
                    itemTemplate.Parent != BaseClasses.STIMULATOR &&
                    itemTemplate.Parent != BaseClasses.DRUGS
                )
                {
                    healingItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted drugs to bot if any exist
        var drugItems = botJsonTemplate.BotGeneration.Items.Drugs.Whitelist ?? new Dictionary<string, double>();
        // no drugs whitelist, find and assign from combined item pool
        if (!drugItems.Any())
        {
            foreach (var itemKvP in (combinedLootPool)) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (IsMedicalItem(itemTemplate.Properties) && itemTemplate.Parent == BaseClasses.DRUGS)
                {
                    drugItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted food to bot if any exist
        var foodItems = botJsonTemplate.BotGeneration.Items.Food.Whitelist ?? new Dictionary<string, double>();
        // No food whitelist, find and assign from combined item pool
        if (!foodItems.Any())
        {
            foreach (var itemKvP in (combinedLootPool)) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.FOOD))
                {
                    foodItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted drink to bot if any exist
        var drinkItems = botJsonTemplate.BotGeneration.Items.Food.Whitelist ?? new Dictionary<string, double>();
        // No drink whitelist, find and assign from combined item pool
        if (!drinkItems.Any())
        {
            foreach (var itemKvP in combinedLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.DRINK))
                {
                    drinkItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted currency to bot if any exist
        var currencyItems = botJsonTemplate.BotGeneration.Items.Currency.Whitelist ?? new Dictionary<string, double>();
        // No currency whitelist, find and assign from combined item pool
        if (!currencyItems.Any())
        {
            foreach (var itemKvP in combinedLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.MONEY))
                {
                    currencyItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted stims to bot if any exist
        var stimItems = botJsonTemplate.BotGeneration.Items.Stims.Whitelist ?? new Dictionary<string, double>();
        // No whitelist, find and assign from combined item pool
        if (!stimItems.Any())
        {
            foreach (var itemKvP in combinedLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (IsMedicalItem(itemTemplate.Properties) && itemTemplate.Parent == BaseClasses.STIMULATOR)
                {
                    stimItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Assign whitelisted grenades to bot if any exist
        var grenadeItems = botJsonTemplate.BotGeneration.Items.Grenades.Whitelist ?? new Dictionary<string, double>();
        // no whitelist, find and assign from combined item pool
        if (!grenadeItems.Any())
        {
            foreach (var itemKvP in combinedLootPool) {
                var itemTemplate = _itemHelper.GetItem(itemKvP.Key).Value;
                if (IsGrenade(itemTemplate.Properties))
                {
                    grenadeItems[itemKvP.Key] = itemKvP.Value;
                }
            }
        }

        // Get backpack loot (excluding magazines, bullets, grenades, drink, food and healing/stim items)
        var filteredBackpackItems = new Dictionary<string, double>();
        foreach (var itemKvP in backpackLootPool) {
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
                // Is type we don't want as backpack loot, skip
                continue;
            }

            filteredBackpackItems[itemKvP.Key] = itemKvP.Value;
        }

        // Get pocket loot (excluding magazines, bullets, grenades, drink, food medical and healing/stim items)
        var filteredPocketItems = new Dictionary<string, double>();
        foreach (var itemKvP in pocketLootPool) {
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
                IsCurrency(itemTemplate.Id) || 
                itemTemplate.Properties.Height is null || // lacks height
                itemTemplate.Properties.Width is null // lacks width
            ) {
                continue;
            }

            filteredPocketItems[itemKvP.Key] = itemKvP.Value;
        }

        // Get vest loot (excluding magazines, bullets, grenades, medical and healing/stim items)
        var filteredVestItems = new Dictionary<string, double>();
        foreach (var itemKvP in vestLootPool) {
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

            filteredVestItems[itemKvP.Key] = itemKvP.Value;
        }

        var cacheForRole = _lootCache[botRole];

        cacheForRole.HealingItems = healingItems;
        cacheForRole.DrugItems = drugItems;
        cacheForRole.FoodItems = foodItems;
        cacheForRole.DrinkItems = drinkItems;
        cacheForRole.CurrencyItems = currencyItems;
        cacheForRole.StimItems = stimItems;
        cacheForRole.GrenadeItems = grenadeItems;

        cacheForRole.SpecialItems = specialLootItems;
        cacheForRole.BackpackLoot = filteredBackpackItems;
        cacheForRole.PocketLoot = filteredPocketItems;
        cacheForRole.VestLoot = filteredVestItems;
        cacheForRole.SecureLoot = secureLootTPool;
    }

    /// <summary>
    /// Add unique items into combined pool
    /// </summary>
    /// <param name="poolToAddTo">Pool of items to add to</param>
    /// <param name="itemsToAdd">items to add to combined pool if unique</param>
    protected void AddUniqueItemsToPool(List<TemplateItem> poolToAddTo, List<TemplateItem> itemsToAdd)
    {
        throw new NotImplementedException();
    }

    protected void AddItemsToPool(Dictionary<string, double> poolToAddTo, Dictionary<string, double> poolOfItemsToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ammo/grenades have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsBulletOrGrenade(Props props)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Internal and external magazine have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMagazine(Props props)
    {
        return props.ReloadMagType is not null;
    }

    /// <summary>
    /// Medical use items (e.g. morphine/lip balm/grizzly)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMedicalItem(Props props)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Grenades have this property (e.g. smoke/frag/flash grenades)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsGrenade(Props props)
    {
        throw new NotImplementedException();
    }

    protected bool IsFood(string tpl)
    {
        throw new NotImplementedException();
    }

    protected bool IsDrink(string tpl)
    {
        throw new NotImplementedException();
    }

    protected bool IsCurrency(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if a bot type exists inside the loot cache
    /// </summary>
    /// <param name="botRole">role to check for</param>
    /// <returns>true if they exist</returns>
    protected bool BotRoleExistsInCache(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// If lootcache is undefined, init with empty property arrays
    /// </summary>
    /// <param name="botRole">Bot role to hydrate</param>
    protected void InitCacheForBotRole(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Compares two item prices by their flea (or handbook if that doesnt exist) price
    /// </summary>
    /// <param name="itemAPrice"></param>
    /// <param name="itemBPrice"></param>
    /// <returns></returns>
    protected int CompareByValue(int itemAPrice, int itemBPrice)
    {
        throw new NotImplementedException();
    }
}
