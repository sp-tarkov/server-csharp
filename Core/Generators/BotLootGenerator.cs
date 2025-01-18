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
public class BotLootGenerator(
    ISptLogger<BotLootGenerator> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    ItemHelper _itemHelper,
    InventoryHelper _inventoryHelper,
    DatabaseService _databaseService,
    HandbookHelper _handbookHelper,
    BotGeneratorHelper _botGeneratorHelper,
    BotWeaponGenerator _botWeaponGenerator,
    WeightedRandomHelper _weightedRandomHelper,
    BotHelper _botHelper,
    BotLootCacheService _botLootCacheService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected BotConfig _botConfig = _configServer.GetConfig<BotConfig>();
    protected PmcConfig _pmcConfig = _configServer.GetConfig<PmcConfig>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ItemSpawnLimitSettings GetItemSpawnLimitsForBot(string botRole)
    {
        // Init item limits
        Dictionary<string, double> limitsForBotDict = new();
        InitItemLimitArray(botRole, limitsForBotDict);

        return new ItemSpawnLimitSettings { CurrentLimits = limitsForBotDict, GlobalLimits = GetItemSpawnLimitsForBotType(botRole) };
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
        var itemCounts = botJsonTemplate.BotGeneration?.Items;

        if (
            itemCounts?.BackpackLoot.Weights is null ||
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

        var backpackLootCount = _weightedRandomHelper.GetWeightedValue(itemCounts.BackpackLoot.Weights);
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
            containersIdFull: containersIdFull
        );

        // Healing items / Meds
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.HealingItems, botJsonTemplate),
            containersBotHasAvailable,
            healingItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

        // Drugs
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrugItems, botJsonTemplate),
            containersBotHasAvailable,
            drugItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

        // Food
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.FoodItems, botJsonTemplate),
            containersBotHasAvailable,
            foodItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

        // Drink
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrinkItems, botJsonTemplate),
            containersBotHasAvailable,
            drinkItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

        // Currency
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.CurrencyItems, botJsonTemplate),
            containersBotHasAvailable,
            currencyItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

        // Stims
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.StimItems, botJsonTemplate),
            containersBotHasAvailable,
            stimItemCount,
            botInventory,
            botRole,
            botItemLimits,
            0,
            isPmc,
            containersIdFull
        );

        // Grenades
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.GrenadeItems, botJsonTemplate),
            [EquipmentSlots.Pockets, EquipmentSlots.TacticalVest], // Can't use containersBotHasEquipped as we don't want grenades added to backpack
            grenadeCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            containersIdFull
        );

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
                    botJsonTemplate.BotChances?.WeaponModsChances,
                    botRole,
                    isPmc,
                    botLevel,
                    containersIdFull
                );
            }

            var backpackLootRoubleTotal = GetBackpackRoubleTotalByLevel(botLevel, isPmc);
            AddLootFromPool(
                _botLootCacheService.GetLootFromCache(
                    botRole,
                    isPmc,
                    LootCacheType.Backpack,
                    botJsonTemplate,
                    itemPriceLimits?.Backpack
                ),
                [EquipmentSlots.Backpack],
                backpackLootCount,
                botInventory,
                botRole,
                botItemLimits,
                backpackLootRoubleTotal ?? 0,
                isPmc,
                containersIdFull
            );
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
                    itemPriceLimits?.Vest
                ),
                [EquipmentSlots.TacticalVest],
                vestLootCount,
                botInventory,
                botRole,
                botItemLimits,
                _pmcConfig.MaxVestLootTotalRub,
                isPmc,
                containersIdFull
            );
        }

        // Pockets
        AddLootFromPool(
            _botLootCacheService.GetLootFromCache(
                botRole,
                isPmc,
                LootCacheType.Pocket,
                botJsonTemplate,
                itemPriceLimits?.Pocket
            ),
            [EquipmentSlots.Pockets],
            pocketLootCount,
            botInventory,
            botRole,
            botItemLimits,
            _pmcConfig.MaxPocketLootTotalRub,
            isPmc,
            containersIdFull
        );

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
                -1,
                isPmc,
                containersIdFull
            );
        }
    }

    private MinMaxLootItemValue? GetSingleItemLootPriceLimits(int botLevel, bool isPmc)
    {
        // TODO - extend to other bot types
        if (!isPmc) return null;

        var matchingValue = _pmcConfig?.LootItemLimitsRub?.FirstOrDefault(
            minMaxValue => botLevel >= minMaxValue.Min && botLevel <= minMaxValue.Max
        );

        return matchingValue;
    }

    /// <summary>
    /// Gets the rouble cost total for loot in a bots backpack by the bots levl
    /// Will return 0 for non PMCs
    /// </summary>
    /// <param name="botLevel">Bots level</param>
    /// <param name="isPmc">Is the bot a PMC</param>
    /// <returns>int</returns>
    private double? GetBackpackRoubleTotalByLevel(int botLevel, bool isPmc)
    {
        if (!isPmc) return 0;

        var matchingValue = _pmcConfig.MaxBackpackLootTotalRub.FirstOrDefault(
            (minMaxValue) => botLevel >= minMaxValue.Min && botLevel <= minMaxValue.Max
        );
        return matchingValue?.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botInventory"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private List<EquipmentSlots> GetAvailableContainersBotCanStoreItemsIn(BotBaseInventory botInventory)
    {
        List<EquipmentSlots> result = [EquipmentSlots.Pockets];

        if ((botInventory.Items ?? []).Any((item) => item.SlotId == EquipmentSlots.TacticalVest.ToString()))
        {
            result.Add(EquipmentSlots.TacticalVest);
        }

        if ((botInventory.Items ?? []).Any((item) => item.SlotId == EquipmentSlots.Backpack.ToString()))
        {
            result.Add(EquipmentSlots.Backpack);
        }

        return result;
    }

    /// <summary>
    /// Force healing items onto bot to ensure they can heal in-raid
    /// </summary>
    /// <param name="botInventory">Inventory to add items to</param>
    /// <param name="botRole">Role of bot (pmcBEAR/pmcUSEC)</param>
    private void AddForcedMedicalItemsToPmcSecure(BotBaseInventory botInventory, string botRole)
    {
        // surv12
        AddLootFromPool(
            new Dictionary<string, double> { { "5d02797c86f774203f38e30a", 1 } },
            [EquipmentSlots.SecuredContainer],
            1,
            botInventory,
            botRole,
            null,
            0,
            true
        );

        // AFAK
        AddLootFromPool(
            new Dictionary<string, double> { { "60098ad7c2240c0fe85c570a", 1 } },
            [EquipmentSlots.SecuredContainer],
            10,
            botInventory,
            botRole,
            null,
            0,
            true
        );
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
    private void AddLootFromPool
    (
        Dictionary<string, double> pool,
        List<EquipmentSlots> equipmentSlots,
        double totalItemCount,
        BotBaseInventory inventoryToAddItemsTo,
        string botRole,
        ItemSpawnLimitSettings? itemSpawnLimits,
        double totalValueLimitRub = 0,
        bool isPmc = false,
        List<string>? containersIdFull = null
    )
    {
        // Loot pool has items
        var poolSize = pool.Count;

        if (poolSize <= 0) return;

        double currentTotalRub = 0;

        var fitItemIntoContainerAttempts = 0;
        for (var i = 0; i < totalItemCount; i++)
        {
            // Pool can become empty if item spawn limits keep removing items
            if (pool.Count == 0)
            {
                return;
            }

            var weightedItemTpl = _weightedRandomHelper.GetWeightedValue<string>(pool);
            var (key, itemToAddTemplate) = _itemHelper.GetItem(weightedItemTpl);

            if (!key)
            {
                _logger.Warning($"Unable to process item tpl: {weightedItemTpl} for slots: {equipmentSlots} on bot: {botRole}");

                continue;
            }

            if (itemSpawnLimits is not null)
            {
                if (ItemHasReachedSpawnLimit(itemToAddTemplate, botRole, itemSpawnLimits))
                {
                    // Remove item from pool to prevent it being picked again
                    pool.Remove(weightedItemTpl);

                    i--;
                    continue;
                }
            }

            var newRootItemId = _hashUtil.Generate();
            List<Item> itemWithChildrenToAdd =
            [
                new()
                {
                    Id = newRootItemId,
                    Template = itemToAddTemplate?.Id ?? string.Empty,
                    Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(itemToAddTemplate, botRole)
                },
            ];

            // Is Simple-Wallet / WZ wallet
            if (_botConfig.WalletLoot.WalletTplPool.Contains(weightedItemTpl))
            {
                var addCurrencyToWallet = _randomUtil.GetChance100(_botConfig.WalletLoot.ChancePercent);
                if (addCurrencyToWallet)
                {
                    // Create the currency items we want to add to wallet
                    var itemsToAdd = CreateWalletLoot(newRootItemId);

                    // Get the container grid for the wallet
                    var containerGrid = _inventoryHelper.GetContainerSlotMap(weightedItemTpl);

                    // Check if all the chosen currency items fit into wallet
                    var canAddToContainer = _inventoryHelper.CanPlaceItemsInContainer(
                        _cloner.Clone(containerGrid), // MUST clone grid before passing in as function modifies grid
                        itemsToAdd
                    );
                    if (canAddToContainer)
                    {
                        // Add each currency to wallet
                        foreach (var itemToAdd in itemsToAdd)
                        {
                            _inventoryHelper.PlaceItemInContainer(
                                containerGrid,
                                itemToAdd,
                                itemWithChildrenToAdd[0].Id,
                                "main"
                            );
                        }

                        itemWithChildrenToAdd.AddRange(itemsToAdd.SelectMany(x => x));
                    }
                }
            }

            // Some items (ammoBox/ammo) need extra changes
            AddRequiredChildItemsToParent(itemToAddTemplate, itemWithChildrenToAdd, isPmc, botRole);

            // Attempt to add item to container(s)
            var itemAddedResult = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                equipmentSlots,
                newRootItemId,
                itemToAddTemplate?.Id,
                itemWithChildrenToAdd,
                inventoryToAddItemsTo,
                containersIdFull
            );

            // Handle when item cannot be added
            if (itemAddedResult != ItemAddedResult.SUCCESS)
            {
                if (itemAddedResult == ItemAddedResult.NO_CONTAINERS)
                {
                    // Bot has no container to put item in, exit
                    _logger.Debug($"Unable to add: {totalItemCount} items to bot as it lacks a container to include them");
                    break;
                }

                fitItemIntoContainerAttempts++;
                if (fitItemIntoContainerAttempts >= 4)
                {
                    _logger.Debug(
                        $"Failed placing item: {i} of: {totalItemCount} items into: {botRole} " +
                        $"containers: {string.Join(",", equipmentSlots)}. Tried: {fitItemIntoContainerAttempts} " +
                        $"times, reason: {itemAddedResult.ToString()}, skipping"
                    );

                    break;
                }

                // Try again, failed but still under attempt limit
                continue;
            }

            // Item added okay, reset counter for next item
            fitItemIntoContainerAttempts = 0;

            // Stop adding items to bots pool if rolling total is over total limit
            if (totalValueLimitRub > 0)
            {
                currentTotalRub += _handbookHelper.GetTemplatePrice(itemToAddTemplate.Id);
                if (currentTotalRub > totalValueLimitRub)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<List<Item>> CreateWalletLoot(string walletId)
    {
        List<List<Item>> result = [];

        // Choose how many stacks of currency will be added to wallet
        var itemCount = _randomUtil.GetInt(
            (int)_botConfig.WalletLoot.ItemCount.Min,
            (int)_botConfig.WalletLoot.ItemCount.Max
        );
        for (var index = 0; index < itemCount; index++)
        {
            // Choose the size of the currency stack - default is 5k, 10k, 15k, 20k, 25k
            var chosenStackCount = _weightedRandomHelper.GetWeightedValue<string>(_botConfig.WalletLoot.StackSizeWeight);
            List<Item> items =
            [
                new Item
                {
                    Id = _hashUtil.Generate(),
                    Template = _weightedRandomHelper.GetWeightedValue<string>(_botConfig.WalletLoot.CurrencyWeight),
                    ParentId = walletId,
                    Upd = new()
                    {
                        StackObjectsCount = double.Parse(chosenStackCount)
                    }
                }
            ];
            result.Add(items);
        }

        return result;
    }

    /// <summary>
    /// Some items need child items to function, add them to the itemToAddChildrenTo array
    /// </summary>
    /// <param name="itemToAddTemplate">Db template of item to check</param>
    /// <param name="itemToAddChildrenTo">Item to add children to</param>
    /// <param name="isPmc">Is the item being generated for a pmc (affects money/ammo stack sizes)</param>
    /// <param name="botRole">role bot has that owns item</param>
    public void AddRequiredChildItemsToParent(TemplateItem? itemToAddTemplate, List<Item> itemToAddChildrenTo, bool isPmc, string botRole)
    {
        // Fill ammo box
        if (_itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO_BOX))
        {
            _itemHelper.AddCartridgesToAmmoBox(itemToAddChildrenTo, itemToAddTemplate);
        }
        // Make money a stack
        else if (_itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.MONEY))
        {
            RandomiseMoneyStackSize(botRole, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Make ammo a stack
        else if (_itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO))
        {
            RandomiseAmmoStackSize(isPmc, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Must add soft inserts/plates
        else if (_itemHelper.ItemRequiresSoftInserts(itemToAddTemplate.Id))
        {
            _itemHelper.AddChildSlotItems(itemToAddChildrenTo, itemToAddTemplate, null, false);
        }
    }

    /// <summary>
    /// Add generated weapons to inventory as loot
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="botInventory">Inventory to add preset to</param>
    /// <param name="equipmentSlot">Slot to place the preset in (backpack)</param>
    /// <param name="templateInventory">Bots template, assault.json</param>
    /// <param name="modChances">Chances for mods to spawn on weapon</param>
    /// <param name="botRole">bots role .e.g. pmcBot</param>
    /// <param name="isPmc">are we generating for a pmc</param>
    /// <param name="botLevel"></param>
    /// <param name="containersIdFull"></param>
    public void AddLooseWeaponsToInventorySlot(string sessionId,
        BotBaseInventory botInventory,
        EquipmentSlots equipmentSlot,
        BotTypeInventory? templateInventory,
        Dictionary<string, double>? modChances,
        string botRole,
        bool isPmc,
        int botLevel,
        List<string>? containersIdFull) // TODO: type for containersIdFull was Set<string>
    {
        var chosenWeaponType = _randomUtil.GetArrayValue<string>(
            [
                EquipmentSlots.FirstPrimaryWeapon.ToString(),
                EquipmentSlots.FirstPrimaryWeapon.ToString(),
                EquipmentSlots.FirstPrimaryWeapon.ToString(),
                EquipmentSlots.Holster.ToString()
            ]
        );
        var randomisedWeaponCount = _randomUtil.GetInt(
            (int)_pmcConfig.LooseWeaponInBackpackLootMinMax.Min,
            (int)_pmcConfig.LooseWeaponInBackpackLootMinMax.Max
        );

        if (randomisedWeaponCount <= 0)
        {
            return;
        }

        for (var i = 0; i < randomisedWeaponCount; i++)
        {
            var generatedWeapon = _botWeaponGenerator.GenerateRandomWeapon(
                sessionId,
                chosenWeaponType,
                templateInventory,
                botInventory.Equipment,
                modChances,
                botRole,
                isPmc,
                botLevel
            );
            var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                [equipmentSlot],
                generatedWeapon.Weapon[0].Id,
                generatedWeapon.Weapon[0].Template,
                generatedWeapon.Weapon,
                botInventory,
                containersIdFull
            );

            if (result != ItemAddedResult.SUCCESS)
            {
                _logger.Debug($"Failed to add additional weapon {generatedWeapon.Weapon[0].Id} to bot backpack, reason: {result.ToString()}");
            }
        }
    }

    /// <summary>
    /// Hydrate item limit array to contain items that have a limit for a specific bot type
    /// All values are set to 0
    /// </summary>
    /// <param name="botRole">Role the bot has</param>
    /// <param name="limitCount"></param>
    public void InitItemLimitArray(string botRole, Dictionary<string, double> limitCount)
    {
        // Init current count of items we want to limit
        var spawnLimits = GetItemSpawnLimitsForBotType(botRole);
        foreach (var limit in spawnLimits)
        {
            spawnLimits[limit.Key] = 0;
        }
    }

    /// <summary>
    /// Check if an item has reached its bot-specific spawn limit
    /// </summary>
    /// <param name="itemTemplate">Item we check to see if its reached spawn limit</param>
    /// <param name="botRole">Bot type</param>
    /// <param name="itemSpawnLimits"></param>
    /// <returns>true if item has reached spawn limit</returns>
    private bool ItemHasReachedSpawnLimit(TemplateItem? itemTemplate, string botRole, ItemSpawnLimitSettings? itemSpawnLimits)
    {
        // PMCs and scavs have different sections of bot config for spawn limits
        if (itemSpawnLimits is not null && itemSpawnLimits.GlobalLimits?.Count == 0)
        {
            // No items found in spawn limit, drop out
            return false;
        }

        // No spawn limits, skipping
        if (itemSpawnLimits is null)
        {
            return false;
        }

        var idToCheckFor = GetMatchingIdFromSpawnLimits(itemTemplate, itemSpawnLimits.GlobalLimits);
        if (idToCheckFor is null)
        {
            // ParentId or tplid not found in spawnLimits, not a spawn limited item, skip
            return false;
        }

        // Increment item count with this bot type
        itemSpawnLimits.CurrentLimits[idToCheckFor]++;

        // Check if over limit
        if (itemSpawnLimits.CurrentLimits[idToCheckFor] > itemSpawnLimits.GlobalLimits[idToCheckFor])
        {
            // Prevent edge-case of small loot pools + code trying to add limited item over and over infinitely
            if (itemSpawnLimits.CurrentLimits[idToCheckFor] > itemSpawnLimits.CurrentLimits[idToCheckFor] * 10)
            {
                _logger.Debug(
                    _localisationService.GetText(
                        "bot-item_spawn_limit_reached_skipping_item",
                        new
                        {
                            botRole = botRole,
                            itemName = itemTemplate.Name,
                            attempts = itemSpawnLimits.CurrentLimits[idToCheckFor]
                        }
                    )
                );

                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Randomise the stack size of a money object, uses different values for pmc or scavs
    /// </summary>
    /// <param name="botRole">Role bot has that has money stack</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="moneyItem">Money item to randomise</param>
    public void RandomiseMoneyStackSize(string botRole, TemplateItem itemTemplate, Item moneyItem)
    {
        // Get all currency weights for this bot type
        var currencyWeights = _botConfig.CurrencyStackSize[botRole];
        if (currencyWeights is null)
        {
            currencyWeights = new();
        }

        var currencyWeight = currencyWeights[moneyItem.Template];

        _itemHelper.AddUpdObjectToItem(moneyItem);

        moneyItem.Upd.StackObjectsCount = double.Parse(_weightedRandomHelper.GetWeightedValue(currencyWeight));
    }

    /// <summary>
    /// Randomise the size of an ammo stack
    /// </summary>
    /// <param name="isPmc">Is ammo on a PMC bot</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="ammoItem">Ammo item to randomise</param>
    public void RandomiseAmmoStackSize(bool isPmc, TemplateItem itemTemplate, Item ammoItem)
    {
        var randomSize = _itemHelper.GetRandomisedAmmoStackSize(itemTemplate);
        _itemHelper.AddUpdObjectToItem(ammoItem);

        ammoItem.Upd.StackObjectsCount = randomSize;
    }

    /// <summary>
    /// Get spawn limits for a specific bot type from bot.json config
    /// If no limit found for a non pmc bot, fall back to defaults
    /// </summary>
    /// <param name="botRole">what role does the bot have</param>
    /// <returns>Dictionary of tplIds and limit</returns>
    public Dictionary<string, double> GetItemSpawnLimitsForBotType(string botRole)
    {
        if (_botHelper.IsBotPmc(botRole))
        {
            return _botConfig.ItemSpawnLimits["pmc"];
        }

        if (_botConfig.ItemSpawnLimits[botRole.ToLower()] is not null)
        {
            return _botConfig.ItemSpawnLimits[botRole.ToLower()];
        }

        _logger.Warning(_localisationService.GetText("bot-unable_to_find_spawn_limits_fallback_to_defaults", botRole));

        return new();
    }

    /// <summary>
    /// Get the parentId or tplId of item inside spawnLimits object if it exists
    /// </summary>
    /// <param name="itemTemplate">item we want to look for in spawn limits</param>
    /// <param name="spawnLimits">Limits to check for item</param>
    /// <returns>id as string, otherwise undefined</returns>
    public string? GetMatchingIdFromSpawnLimits(TemplateItem itemTemplate, Dictionary<string, double> spawnLimits)
    {
        if (spawnLimits.ContainsKey(itemTemplate.Id))
        {
            return itemTemplate.Id;
        }

        // tplId not found in spawnLimits, check if parentId is
        if (spawnLimits.ContainsKey(itemTemplate.Parent))
        {
            return itemTemplate.Parent;
        }

        // parentId and tplId not found
        return null;
    }
}
