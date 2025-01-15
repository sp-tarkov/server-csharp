using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Utils;
using Core.Helpers;
using Core.Models.Utils;
using Core.Services;
using Core.Servers;
using Core.Utils.Cloners;

namespace Core.Generators;

[Injectable]
public class BotLootGenerator
{
    private readonly ISptLogger<BotLootGenerator> _logger;
    private readonly HashUtil _hashUtil;
    private readonly RandomUtil _randomUtil;
    private readonly ItemHelper _itemHelper;
    private readonly InventoryHelper _inventoryHelper;
    private readonly DatabaseService _databaseService;
    private readonly HandbookHelper _handbookHelper;
    private readonly BotGeneratorHelper _botGeneratorHelper;
    private readonly BotWeaponGenerator _botWeaponGenerator;
    private readonly WeightedRandomHelper _weightedRandomHelper;
    private readonly BotHelper _botHelper;
    private readonly BotLootCacheService _botLootCacheService;
    private readonly LocalisationService _localisationService;
    private readonly ConfigServer _configServer;
    private readonly ICloner _cloner;

    private BotConfig _botConfig;
    private PmcConfig _pmcConfig;

    public BotLootGenerator(
        ISptLogger<BotLootGenerator> logger,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        ItemHelper itemHelper,
        InventoryHelper inventoryHelper,
        DatabaseService databaseService,
        HandbookHelper handbookHelper,
        BotGeneratorHelper botGeneratorHelper,
        BotWeaponGenerator botWeaponGenerator,
        WeightedRandomHelper weightedRandomHelper,
        BotHelper botHelper,
        BotLootCacheService botLootCacheService,
        LocalisationService localisationService,
        ConfigServer configServer,
        ICloner cloner)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _itemHelper = itemHelper;
        _inventoryHelper = inventoryHelper;
        _databaseService = databaseService;
        _handbookHelper = handbookHelper;
        _botGeneratorHelper = botGeneratorHelper;
        _botWeaponGenerator = botWeaponGenerator;
        _weightedRandomHelper = weightedRandomHelper;
        _botHelper = botHelper;
        _botLootCacheService = botLootCacheService;
        _localisationService = localisationService;
        _configServer = configServer;
        _cloner = cloner;

        _botConfig = _configServer.GetConfig<BotConfig>();
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemSpawnLimitSettings GetItemSpawnLimitsForBot(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add loot to bots containers
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="isPmc">Will bot be a pmc</param>
    /// <param name="botRole">Role of bot, e.g. asssult</param>
    /// <param name="botInventory">Inventory to add loot to</param>
    /// <param name="botLevel">Level of bot</param>
    public void GenerateLoot(string sessionId, BotType botJsonTemplate, bool isPmc, string botRole, BotBaseInventory botInventory, int botLevel)
    {
        // Limits on item types to be added as loot
        var itemCounts = botJsonTemplate.BotGeneration.Items;

        if (
            itemCounts.BackpackLoot.Weights is null ||
            itemCounts.PocketLoot.Weights is null ||
            itemCounts.VestLoot.Weights is null ||
            itemCounts.SpecialItems.Weights is null ||
            itemCounts.Healing.Weights is null ||
            itemCounts.Drugs.Weights is null ||
            itemCounts.Food.Weights is null ||
            itemCounts.Drink.Weights is null ||
            itemCounts.Currency.Weights is null ||
            itemCounts.Stims.Weights is null ||
            itemCounts.Grenades.Weights is null
        )
        {
            _logger.Warning(_localisationService.GetText("bot-unable_to_generate_bot_loot", botRole));
            return;
        }
        var backpackLootCount = _weightedRandomHelper.GetWeightedValue<int>(itemCounts.BackpackLoot.Weights);
        var pocketLootCount = _weightedRandomHelper.GetWeightedValue(itemCounts.PocketLoot.Weights);
        var vestLootCount = _weightedRandomHelper.GetWeightedValue(itemCounts.VestLoot.Weights);
        var specialLootItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.SpecialItems.Weights);
        var healingItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Healing.Weights);
        var drugItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Drugs.Weights);
        var foodItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Food.Weights);
        var drinkItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Drink.Weights);
        var currencyItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Currency.Weights);
        var stimItemCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Stims.Weights);
        var grenadeCount = _weightedRandomHelper.GetWeightedValue(itemCounts.Grenades.Weights);

        // If bot has been flagged as not having loot, set below counts to 0
        if (_botConfig.DisableLootOnBotTypes.Contains(botRole.ToLower()))
        {
            backpackLootCount = 0;
            pocketLootCount = 0;
            vestLootCount = 0;
            currencyItemCount = 0;
        }

        // Forced pmc healing loot into secure container
        if (isPmc && _pmcConfig.ForceHealingItemsIntoSecure)
        {
            AddForcedMedicalItemsToPmcSecure(botInventory, botRole);
        }

        var botItemLimits = GetItemSpawnLimitsForBot(botRole);

        var containersBotHasAvailable = GetAvailableContainersBotCanStoreItemsIn(botInventory);

        // This set is passed as a reference to fill up the containers that are already full, this alleviates
        // generation of the bots by avoiding checking the slots of containers we already know are full
        var containersIdFull = new List<string>();

        // Special items
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Special, botJsonTemplate),
            containersBotHasAvailable,
            specialLootItemCount,
            botInventory,
            botRole,
            botItemLimits,
            containersIdFull);

        // Healing items / Meds
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.HealingItems, botJsonTemplate),
            containersBotHasAvailable,
            healingItemCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        // Drugs
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrugItems, botJsonTemplate),
            containersBotHasAvailable,
            drugItemCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        // Food
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.FoodItems, botJsonTemplate),
            containersBotHasAvailable,
            foodItemCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        // Drink
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrinkItems, botJsonTemplate),
            containersBotHasAvailable,
            drinkItemCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        // Currency
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.CurrencyItems, botJsonTemplate),
            containersBotHasAvailable,
            currencyItemCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        // Stims
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.StimItems, botJsonTemplate),
            containersBotHasAvailable,
            stimItemCount,
            botInventory,
            botRole,
            botItemLimits,
            containersIdFull,
            0,
            isPmc);

        // Grenades
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.GrenadeItems, botJsonTemplate),
            [EquipmentSlots.Pockets, EquipmentSlots.TacticalVest], // Can't use containersBotHasEquipped as we don't want grenades added to backpack
            grenadeCount,
            botInventory,
            botRole,
            null,
            containersIdFull,
            0,
            isPmc);

        var itemPriceLimits = GetSingleItemLootPriceLimits(botLevel, isPmc);

        // Backpack - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.Backpack))
        {
            // Add randomly generated weapon to PMC backpacks
            if (isPmc && _randomUtil.GetChance100(_pmcConfig.LooseWeaponInBackpackChancePercent))
            {
                AddLooseWeaponsToInventorySlot(
                    sessionId,
                    botInventory,
                    EquipmentSlots.Backpack,
                    botJsonTemplate.BotInventory,
                    botJsonTemplate.BotChances.WeaponModsChances,
                    botRole,
                    isPmc,
                    botLevel,
                    containersIdFull);
            }

            var backpackLootRoubleTotal = GetBackpackRoubleTotalByLevel(botLevel, isPmc);
            AddLootFromPool(
                _botLootCacheService.GetLootFromCache(
                    botRole,
                    isPmc,
                    LootCacheType.Backpack,
                    botJsonTemplate,
                    itemPriceLimits?.Backpack),
                [EquipmentSlots.Backpack],
                backpackLootCount,
                botInventory,
                botRole,
                botItemLimits,
                containersIdFull,
                backpackLootRoubleTotal,
                isPmc);
        }

        // TacticalVest - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.TacticalVest))
        {
            // Vest
            AddLootFromPool(
                _botLootCacheService.GetLootFromCache(
                    botRole,
                    isPmc,
                    LootCacheType.Vest,
                    botJsonTemplate,
                    itemPriceLimits?.Vest),
                [EquipmentSlots.TacticalVest],
                vestLootCount,
                botInventory,
                botRole,
                botItemLimits,
                containersIdFull,
                _pmcConfig.MaxVestLootTotalRub,
                isPmc);
        }

        // Pockets
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(
                botRole,
                isPmc,
                LootCacheType.Pocket,
                botJsonTemplate,
                itemPriceLimits?.Pocket),
            [EquipmentSlots.Pockets],
            pocketLootCount,
            botInventory,
            botRole,
            botItemLimits,
            containersIdFull,
            _pmcConfig.MaxPocketLootTotalRub,
            isPmc);

        // Secure

        // only add if not a pmc or is pmc and flag is true
        if (!isPmc || (isPmc && _pmcConfig.AddSecureContainerLootFromBotConfig))
        {
            AddLootFromPool(
                _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Secure, botJsonTemplate),
                [EquipmentSlots.SecuredContainer],
                50,
                botInventory,
                botRole,
                null,
                containersIdFull,
                - 1,
                isPmc);
        }
    }

    private MinMaxLootItemValue? GetSingleItemLootPriceLimits(int botLevel, bool isPmc)
    {
        // TODO - extend to other bot types
        if (isPmc)
        {
            var matchingValue = _pmcConfig.LootItemLimitsRub.FirstOrDefault(
                (minMaxValue) => botLevel >= minMaxValue.Min && botLevel <= minMaxValue.Max);

            return matchingValue;
        }

        return null;
    }

    /// <summary>
    /// Gets the rouble cost total for loot in a bots backpack by the bots levl
    /// Will return 0 for non PMCs
    /// </summary>
    /// <param name="botLevel">Bots level</param>
    /// <param name="isPmc">Is the bot a PMC</param>
    /// <returns>int</returns>
    public int GetBackpackRoubleTotalByLevel(int botLevel, bool isPmc)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botInventory"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<EquipmentSlots> GetAvailableContainersBotCanStoreItemsIn(BotBaseInventory botInventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Force healing items onto bot to ensure they can heal in-raid
    /// </summary>
    /// <param name="botInventory">Inventory to add items to</param>
    /// <param name="botRole">Role of bot (pmcBEAR/pmcUSEC)</param>
    public void AddForcedMedicalItemsToPmcSecure(BotBaseInventory botInventory, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take random items from a pool and add to an inventory until totalItemCount or totalValueLimit or space limit is reached
    /// </summary>
    /// <param name="pool">Pool of items to pick from with weight</param>
    /// <param name="equipmentSlots">What equipment slot will the loot items be added to</param>
    /// <param name="totalItemCount">Max count of items to add</param>
    /// <param name="inventoryToAddItemsTo">Bot inventory loot will be added to</param>
    /// <param name="botRole">Role of the bot loot is being generated for (assault/pmcbot)</param>
    /// <param name="itemSpawnLimits">Item spawn limits the bot must adhere to</param>
    /// <param name="containersIdFull"></param>
    /// <param name="totalValueLimitRub">Total value of loot allowed in roubles</param>
    /// <param name="isPmc">Is bot being generated for a pmc</param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddLootFromPool(
        Dictionary<string, int> pool,
        List<EquipmentSlots> equipmentSlots,
        int totalItemCount,
        BotBaseInventory inventoryToAddItemsTo, // TODO: type for containersIdFull was Set<string>
        string botRole,
        ItemSpawnLimitSettings itemSpawnLimits,
        List<string> containersIdFull,
        int totalValueLimitRub = 0,
        bool isPmc = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<List<Item>> CrateWalletLoot(string walletId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Some items need child items to function, add them to the itemToAddChildrenTo array
    /// </summary>
    /// <param name="itemToAddTemplate">Db template of item to check</param>
    /// <param name="itemToAddChildrenTo">Item to add children to</param>
    /// <param name="isPmc">Is the item being generated for a pmc (affects money/ammo stack sizes)</param>
    /// <param name="botRole">role bot has that owns item</param>
    public void AddRequiredChildItemsToParent(TemplateItem itemToAddTemplate, List<Item> itemToAddChildrenTo, bool isPmc, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add generated weapons to inventory as loot
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="botInventory">inventory to add preset to</param>
    /// <param name="equipmentSlot">slot to place the preset in (backpack)</param>
    /// <param name="templateInventory">bots template, assault.json</param>
    /// <param name="modsChances">chances for mods to spawn on weapon</param>
    /// <param name="botRole">bots role .e.g. pmcBot</param>
    /// <param name="isPmc">are we generating for a pmc</param>
    /// <param name="botLevel"></param>
    /// <param name="containersIdFull"></param>
    public void AddLooseWeaponsToInventorySlot(string sessionId,
        BotBaseInventory botInventory,
        EquipmentSlots equipmentSlot,
        BotTypeInventory templateInventory, 
        Dictionary<string, double> modsChances,
        string botRole,
        bool isPmc,
        int botLevel,
        List<string>? containersIdFull) // TODO: type for containersIdFull was Set<string>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Hydrate item limit array to contain items that have a limit for a specific bot type
    /// All values are set to 0
    /// </summary>
    /// <param name="botRole">Role the bot has</param>
    /// <param name="limitCount"></param>
    public void InitItemLimitArray(string botRole, Dictionary<string, int> limitCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if an item has reached its bot-specific spawn limit
    /// </summary>
    /// <param name="itemTemplate">Item we check to see if its reached spawn limit</param>
    /// <param name="botRole">Bot type</param>
    /// <param name="itemSpawnLimits"></param>
    /// <returns>true if item has reached spawn limit</returns>
    public bool ItemHasReachedSpawnLimit(TemplateItem itemTemplate, string botRole, ItemSpawnLimitSettings itemSpawnLimits)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the stack size of a money object, uses different values for pmc or scavs
    /// </summary>
    /// <param name="botRole">Role bot has that has money stack</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="moneyItem">Money item to randomise</param>
    public void RandomiseMoneyStackSize(string botRole, TemplateItem itemTemplate, Item moneyItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the size of an ammo stack
    /// </summary>
    /// <param name="isPmc">Is ammo on a PMC bot</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="ammoItem">Ammo item to randomise</param>
    public void RandomiseAmmoStackSize(bool isPmc, TemplateItem itemTemplate, Item ammoItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get spawn limits for a specific bot type from bot.json config
    /// If no limit found for a non pmc bot, fall back to defaults
    /// </summary>
    /// <param name="botRole">what role does the bot have</param>
    /// <returns>Dictionary of tplIds and limit</returns>
    public Dictionary<string, int> GetItemSpawnLimitsForBotType(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the parentId or tplId of item inside spawnLimits object if it exists
    /// </summary>
    /// <param name="itemTemplate">item we want to look for in spawn limits</param>
    /// <param name="spawnLimits">Limits to check for item</param>
    /// <returns>id as string, otherwise undefined</returns>
    public string? GetMatchingIdFromSpawnLimits(TemplateItem itemTemplate, Dictionary<string, int> spawnLimits)
    {
        throw new NotImplementedException();
    }
}
