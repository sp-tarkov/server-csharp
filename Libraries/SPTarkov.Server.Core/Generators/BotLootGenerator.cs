using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotLootGenerator(
    ISptLogger<BotLootGenerator> logger,
    RandomUtil randomUtil,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    HandbookHelper handbookHelper,
    BotGeneratorHelper botGeneratorHelper,
    BotWeaponGenerator botWeaponGenerator,
    WeightedRandomHelper weightedRandomHelper,
    BotHelper botHelper,
    BotLootCacheService botLootCacheService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();
    protected readonly PmcConfig _pmcConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns></returns>
    protected ItemSpawnLimitSettings GetItemSpawnLimitsForBot(string botRole)
    {
        var limits = GetItemSpawnLimitsForBotType(botRole);

        // Clone limits and set all values to 0 to use as a running total
        var limitsForBotDict = cloner.Clone(limits);
        // Init current count of items we want to limit
        foreach (var limit in limitsForBotDict)
        {
            limitsForBotDict[limit.Key] = 0;
        }

        return new ItemSpawnLimitSettings { CurrentLimits = limitsForBotDict, GlobalLimits = GetItemSpawnLimitsForBotType(botRole) };
    }

    /// <summary>
    ///     Add loot to bots containers
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Clone of Base JSON db file for the bot having its loot generated</param>
    /// <param name="botGenerationDetails">Details relating to generating a bot</param>
    /// <param name="isPmc">Will bot be a pmc</param>
    /// <param name="botRole">Role of bot, e.g. assault</param>
    /// <param name="botInventory">Inventory to add loot to</param>
    /// <param name="botLevel">Level of bot</param>
    public void GenerateLoot(
        MongoId sessionId,
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails,
        bool isPmc,
        string botRole,
        BotBaseInventory botInventory,
        int botLevel
    )
    {
        // Limits on item types to be added as loot
        var itemCounts = botJsonTemplate.BotGeneration?.Items;

        if (
            itemCounts?.BackpackLoot.Weights is null
            || itemCounts.PocketLoot.Weights is null
            || itemCounts.VestLoot.Weights is null
            || itemCounts.SpecialItems.Weights is null
            || itemCounts.Healing.Weights is null
            || itemCounts.Drugs.Weights is null
            || itemCounts.Food.Weights is null
            || itemCounts.Drink.Weights is null
            || itemCounts.Currency.Weights is null
            || itemCounts.Stims.Weights is null
            || itemCounts.Grenades.Weights is null
        )
        {
            logger.Warning(serverLocalisationService.GetText("bot-unable_to_generate_bot_loot", botRole));
            return;
        }

        var backpackLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.BackpackLoot.Weights);
        var pocketLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.PocketLoot.Weights);
        var vestLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.VestLoot.Weights);
        var specialLootItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.SpecialItems.Weights);
        var healingItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Healing.Weights);
        var drugItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Drugs.Weights);
        var foodItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Food.Weights);
        var drinkItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Drink.Weights);
        var currencyItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Currency.Weights);
        var stimItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Stims.Weights);
        var grenadeCount = weightedRandomHelper.GetWeightedValue(itemCounts.Grenades.Weights);

        // If bot has been flagged as not having loot, set below counts to 0
        if (_botConfig.DisableLootOnBotTypes.Contains(botRole.ToLowerInvariant()))
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
        HashSet<string> filledContainerIds = [];

        // Special items
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Special, botJsonTemplate),
            containersBotHasAvailable,
            specialLootItemCount,
            botInventory,
            botRole,
            botItemLimits,
            containersIdFull: filledContainerIds
        );

        // Healing items / Meds
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.HealingItems, botJsonTemplate),
            containersBotHasAvailable,
            healingItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        // Drugs
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrugItems, botJsonTemplate),
            containersBotHasAvailable,
            drugItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        // Food
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.FoodItems, botJsonTemplate),
            containersBotHasAvailable,
            foodItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        // Drink
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.DrinkItems, botJsonTemplate),
            containersBotHasAvailable,
            drinkItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        // Currency
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.CurrencyItems, botJsonTemplate),
            containersBotHasAvailable,
            currencyItemCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        // Stims
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.StimItems, botJsonTemplate),
            containersBotHasAvailable,
            stimItemCount,
            botInventory,
            botRole,
            botItemLimits,
            0,
            isPmc,
            filledContainerIds
        );

        // Grenades
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.GrenadeItems, botJsonTemplate),
            [EquipmentSlots.Pockets, EquipmentSlots.TacticalVest], // Can't use containersBotHasEquipped as we don't want grenades added to backpack
            grenadeCount,
            botInventory,
            botRole,
            null,
            0,
            isPmc,
            filledContainerIds
        );

        var itemPriceLimits = GetSingleItemLootPriceLimits(botLevel, isPmc);

        // Backpack - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.Backpack) && backpackLootCount > 0)
        {
            // Add randomly generated weapon to PMC backpacks
            if (isPmc && randomUtil.GetChance100(_pmcConfig.LooseWeaponInBackpackChancePercent))
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
                    filledContainerIds
                );
            }

            var backpackLootRoubleTotal = isPmc
                ? _pmcConfig.LootSettings.Backpack.GetRoubleValue(botLevel, botGenerationDetails.Location)
                : 0;

            AddLootFromPool(
                botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Backpack, botJsonTemplate, itemPriceLimits?.Backpack),
                [EquipmentSlots.Backpack],
                backpackLootCount,
                botInventory,
                botRole,
                botItemLimits,
                backpackLootRoubleTotal,
                isPmc,
                filledContainerIds
            );
        }

        var vestLootRoubleTotal = isPmc ? _pmcConfig.LootSettings.Vest.GetRoubleValue(botLevel, botGenerationDetails.Location) : 0;

        // TacticalVest - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.TacticalVest))
        // Vest
        {
            AddLootFromPool(
                botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Vest, botJsonTemplate, itemPriceLimits?.Vest),
                [EquipmentSlots.TacticalVest],
                vestLootCount,
                botInventory,
                botRole,
                botItemLimits,
                vestLootRoubleTotal,
                isPmc,
                filledContainerIds
            );
        }

        var pocketLootRoubleTotal = isPmc ? _pmcConfig.LootSettings.Pocket.GetRoubleValue(botLevel, botGenerationDetails.Location) : 0;

        // Pockets
        AddLootFromPool(
            botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Pocket, botJsonTemplate, itemPriceLimits?.Pocket),
            [EquipmentSlots.Pockets],
            pocketLootCount,
            botInventory,
            botRole,
            botItemLimits,
            pocketLootRoubleTotal,
            isPmc,
            filledContainerIds
        );

        // Secure

        // only add if not a pmc or is pmc and flag is true
        if (!isPmc || (isPmc && _pmcConfig.AddSecureContainerLootFromBotConfig))
        {
            AddLootFromPool(
                botLootCacheService.GetLootFromCache(botRole, isPmc, LootCacheType.Secure, botJsonTemplate),
                [EquipmentSlots.SecuredContainer],
                50,
                botInventory,
                botRole,
                null,
                -1,
                isPmc,
                filledContainerIds
            );
        }
    }

    protected MinMaxLootItemValue? GetSingleItemLootPriceLimits(int botLevel, bool isPmc)
    {
        // TODO - extend to other bot types
        if (!isPmc)
        {
            return null;
        }

        var matchingValue = _pmcConfig?.LootItemLimitsRub.FirstOrDefault(minMaxValue =>
            botLevel >= minMaxValue.Min && botLevel <= minMaxValue.Max
        );

        return matchingValue;
    }

    /// <summary>
    ///     Get an array of the containers a bot has on them (pockets/backpack/vest)
    /// </summary>
    /// <param name="botInventory">Bot to check</param>
    /// <returns>Array of available slots</returns>
    protected HashSet<EquipmentSlots> GetAvailableContainersBotCanStoreItemsIn(BotBaseInventory botInventory)
    {
        HashSet<EquipmentSlots> result = [EquipmentSlots.Pockets];

        if ((botInventory.Items ?? []).Any(item => item.SlotId == nameof(EquipmentSlots.TacticalVest)))
        {
            result.Add(EquipmentSlots.TacticalVest);
        }

        if ((botInventory.Items ?? []).Any(item => item.SlotId == nameof(EquipmentSlots.Backpack)))
        {
            result.Add(EquipmentSlots.Backpack);
        }

        return result;
    }

    /// <summary>
    ///     Force healing items onto bot to ensure they can heal in-raid
    /// </summary>
    /// <param name="botInventory">Inventory to add items to</param>
    /// <param name="botRole">Role of bot (pmcBEAR/pmcUSEC)</param>
    protected void AddForcedMedicalItemsToPmcSecure(BotBaseInventory botInventory, string botRole)
    {
        // surv12
        AddLootFromPool(
            new Dictionary<MongoId, double> { { ItemTpl.MEDICAL_SURV12_FIELD_SURGICAL_KIT, 1 } },
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
            new Dictionary<MongoId, double> { { ItemTpl.MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT, 1 } },
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
    ///     Take random items from a pool and add to an inventory until totalItemCount or totalValueLimit or space limit is reached
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
    protected internal void AddLootFromPool(
        Dictionary<MongoId, double> pool,
        HashSet<EquipmentSlots> equipmentSlots,
        double totalItemCount,
        BotBaseInventory inventoryToAddItemsTo,
        string botRole,
        ItemSpawnLimitSettings? itemSpawnLimits,
        double totalValueLimitRub = 0,
        bool isPmc = false,
        HashSet<string>? containersIdFull = null
    )
    {
        // Loot pool has items
        if (pool.Count <= 0)
        {
            return;
        }

        double currentTotalRub = 0;

        var fitItemIntoContainerAttempts = 0;
        for (var i = 0; i < totalItemCount; i++)
        {
            // Pool can become empty if item spawn limits keep removing items
            if (pool.Count == 0)
            {
                return;
            }

            var weightedItemTpl = weightedRandomHelper.GetWeightedValue(pool);
            var (key, itemToAddTemplate) = itemHelper.GetItem(weightedItemTpl);

            if (!key)
            {
                logger.Warning($"Unable to process item tpl: {weightedItemTpl} for slots: {equipmentSlots} on bot: {botRole}");

                continue;
            }

            if (itemSpawnLimits is not null && ItemHasReachedSpawnLimit(itemToAddTemplate, botRole, itemSpawnLimits))
            {
                // Remove item from pool to prevent it being picked again
                pool.Remove(weightedItemTpl);

                i--;
                continue;
            }

            var newRootItemId = new MongoId();
            List<Item> itemWithChildrenToAdd =
            [
                new()
                {
                    Id = newRootItemId,
                    Template = itemToAddTemplate?.Id ?? MongoId.Empty(),
                    Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(itemToAddTemplate, botRole),
                },
            ];

            // Is Simple-Wallet / WZ wallet
            if (_botConfig.WalletLoot.WalletTplPool.Contains(weightedItemTpl))
            {
                var addCurrencyToWallet = randomUtil.GetChance100(_botConfig.WalletLoot.ChancePercent);
                if (addCurrencyToWallet)
                {
                    // Create the currency items we want to add to wallet
                    var itemsToAdd = CreateWalletLoot(newRootItemId);

                    // Get the container grid for the wallet
                    var containerGrid = inventoryHelper.GetContainerSlotMap(weightedItemTpl);

                    // Check if all the chosen currency items fit into wallet
                    var canAddToContainer = inventoryHelper.CanPlaceItemsInContainer(
                        cloner.Clone(containerGrid), // MUST clone grid before passing in as function modifies grid
                        itemsToAdd
                    );
                    if (canAddToContainer)
                    {
                        // Add each currency to wallet
                        foreach (var itemToAdd in itemsToAdd)
                        {
                            inventoryHelper.PlaceItemInContainer(containerGrid, itemToAdd, itemWithChildrenToAdd[0].Id, "main");
                        }

                        itemWithChildrenToAdd.AddRange(itemsToAdd.SelectMany(x => x));
                    }
                }
            }

            // Some items (ammoBox/ammo) need extra changes
            AddRequiredChildItemsToParent(itemToAddTemplate, itemWithChildrenToAdd, isPmc, botRole);

            // Attempt to add item to container(s)
            var itemAddedResult = botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                equipmentSlots,
                newRootItemId,
                itemToAddTemplate.Id,
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
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Unable to add: {totalItemCount} items to bot as it lacks a container to include them");
                    }

                    break;
                }

                fitItemIntoContainerAttempts++;
                if (fitItemIntoContainerAttempts >= 4)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"Failed placing item: {itemToAddTemplate.Id} - {itemToAddTemplate.Name}: {i} of: {totalItemCount} items into: {botRole} "
                                + $"containers: {string.Join(",", equipmentSlots)}. Tried: {fitItemIntoContainerAttempts} "
                                + $"times, reason: {itemAddedResult}, skipping"
                        );
                    }

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
                currentTotalRub += handbookHelper.GetTemplatePrice(itemToAddTemplate.Id);
                if (currentTotalRub > totalValueLimitRub)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Adds loot to the specified Wallet
    /// </summary>
    /// <param name="walletId"> Wallet to add loot to</param>
    /// <returns>Generated list of currency stacks with the wallet as their parent</returns>
    public List<List<Item>> CreateWalletLoot(MongoId walletId)
    {
        List<List<Item>> result = [];

        // Choose how many stacks of currency will be added to wallet
        var itemCount = randomUtil.GetInt(_botConfig.WalletLoot.ItemCount.Min, _botConfig.WalletLoot.ItemCount.Max);
        for (var index = 0; index < itemCount; index++)
        {
            // Choose the size of the currency stack - default is 5k, 10k, 15k, 20k, 25k
            var chosenStackCount = weightedRandomHelper.GetWeightedValue(_botConfig.WalletLoot.StackSizeWeight);
            List<Item> items =
            [
                new()
                {
                    Id = new MongoId(),
                    Template = weightedRandomHelper.GetWeightedValue(_botConfig.WalletLoot.CurrencyWeight),
                    ParentId = walletId,
                    Upd = new Upd { StackObjectsCount = int.Parse(chosenStackCount) },
                },
            ];
            result.Add(items);
        }

        return result;
    }

    /// <summary>
    ///     Some items need child items to function, add them to the itemToAddChildrenTo array
    /// </summary>
    /// <param name="itemToAddTemplate">Db template of item to check</param>
    /// <param name="itemToAddChildrenTo">Item to add children to</param>
    /// <param name="isPmc">Is the item being generated for a pmc (affects money/ammo stack sizes)</param>
    /// <param name="botRole">role bot has that owns item</param>
    public void AddRequiredChildItemsToParent(TemplateItem? itemToAddTemplate, List<Item> itemToAddChildrenTo, bool isPmc, string botRole)
    {
        // Fill ammo box
        if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO_BOX))
        {
            itemHelper.AddCartridgesToAmmoBox(itemToAddChildrenTo, itemToAddTemplate);
        }
        // Make money a stack
        else if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.MONEY))
        {
            RandomiseMoneyStackSize(botRole, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Make ammo a stack
        else if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO))
        {
            RandomiseAmmoStackSize(isPmc, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Must add soft inserts/plates
        else if (itemHelper.ItemRequiresSoftInserts(itemToAddTemplate.Id))
        {
            itemHelper.AddChildSlotItems(itemToAddChildrenTo, itemToAddTemplate);
        }
    }

    /// <summary>
    ///     Add generated weapons to inventory as loot
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="botInventory">Inventory to add preset to</param>
    /// <param name="equipmentSlot">Slot to place the preset in (backpack)</param>
    /// <param name="templateInventory">Bots template, assault.json</param>
    /// <param name="modChances">Chances for mods to spawn on weapon</param>
    /// <param name="botRole">bots role .e.g. pmcBot</param>
    /// <param name="isPmc">are we generating for a pmc</param>
    /// <param name="botLevel"></param>
    /// <param name="containersIdFull"></param>
    public void AddLooseWeaponsToInventorySlot(
        MongoId sessionId,
        BotBaseInventory botInventory,
        EquipmentSlots equipmentSlot,
        BotTypeInventory? templateInventory,
        Dictionary<string, double> modChances,
        string botRole,
        bool isPmc,
        int botLevel,
        HashSet<string>? containersIdFull
    )
    {
        var chosenWeaponType = randomUtil.GetArrayValue<string>(
            [
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.Holster),
            ]
        );
        var randomisedWeaponCount = randomUtil.GetInt(
            _pmcConfig.LooseWeaponInBackpackLootMinMax.Min,
            _pmcConfig.LooseWeaponInBackpackLootMinMax.Max
        );

        if (randomisedWeaponCount <= 0)
        {
            return;
        }

        for (var i = 0; i < randomisedWeaponCount; i++)
        {
            var generatedWeapon = botWeaponGenerator.GenerateRandomWeapon(
                sessionId,
                chosenWeaponType,
                templateInventory,
                botInventory.Equipment.Value,
                modChances,
                botRole,
                isPmc,
                botLevel
            );

            var weaponRootItem = generatedWeapon.Weapon?.FirstOrDefault();
            if (weaponRootItem is null)
            {
                logger.Error($"Generated null loose weapon: {chosenWeaponType} for: {botRole} level: {botLevel}, skipping");

                continue;
            }
            var result = botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                [equipmentSlot],
                weaponRootItem.Id,
                weaponRootItem.Template,
                generatedWeapon.Weapon,
                botInventory,
                containersIdFull
            );

            if (result != ItemAddedResult.SUCCESS)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Failed to add additional weapon: {weaponRootItem.Id} to bot backpack, reason: {result.ToString()}");
                }
            }
        }
    }

    /// <summary>
    ///     Check if an item has reached its bot-specific spawn limit
    /// </summary>
    /// <param name="itemTemplate">Item we check to see if its reached spawn limit</param>
    /// <param name="botRole">Bot type</param>
    /// <param name="itemSpawnLimits"></param>
    /// <returns>true if item has reached spawn limit</returns>
    protected bool ItemHasReachedSpawnLimit(TemplateItem? itemTemplate, string botRole, ItemSpawnLimitSettings? itemSpawnLimits)
    {
        // PMCs and scavs have different sections of bot config for spawn limits
        if (itemSpawnLimits is not null && itemSpawnLimits.GlobalLimits?.Count == 0)
        // No items found in spawn limit, drop out
        {
            return false;
        }

        // No spawn limits, skipping
        if (itemSpawnLimits is null)
        {
            return false;
        }

        var idToCheckFor = GetMatchingIdFromSpawnLimits(itemTemplate, itemSpawnLimits.GlobalLimits);
        if (idToCheckFor is null)
        // ParentId or tplid not found in spawnLimits, not a spawn limited item, skip
        {
            return false;
        }

        // Use tryAdd to see if it exists, and automatically add 1
        if (!itemSpawnLimits.CurrentLimits.TryAdd(idToCheckFor.Value, 1))
        // if it does exist, come in here and increment item count with this bot type
        {
            itemSpawnLimits.CurrentLimits[idToCheckFor.Value]++;
        }

        // Check if over limit
        var currentLimitCount = itemSpawnLimits.CurrentLimits[idToCheckFor.Value];
        if (itemSpawnLimits.CurrentLimits[idToCheckFor.Value] > itemSpawnLimits.GlobalLimits[idToCheckFor.Value])
        {
            // Prevent edge-case of small loot pools + code trying to add limited item over and over infinitely
            if (currentLimitCount > currentLimitCount * 10)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        serverLocalisationService.GetText(
                            "bot-item_spawn_limit_reached_skipping_item",
                            new
                            {
                                botRole,
                                itemName = itemTemplate.Name,
                                attempts = currentLimitCount,
                            }
                        )
                    );
                }

                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Randomise the stack size of a money object, uses different values for pmc or scavs
    /// </summary>
    /// <param name="botRole">Role bot has that has money stack</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="moneyItem">Money item to randomise</param>
    public void RandomiseMoneyStackSize(string botRole, TemplateItem itemTemplate, Item moneyItem)
    {
        // Get all currency weights for this bot type
        if (!_botConfig.CurrencyStackSize.TryGetValue(botRole, out var currencyWeights))
        {
            currencyWeights = _botConfig.CurrencyStackSize["default"];
        }

        var currencyWeight = currencyWeights[moneyItem.Template];

        itemHelper.AddUpdObjectToItem(moneyItem);

        moneyItem.Upd.StackObjectsCount = int.Parse(weightedRandomHelper.GetWeightedValue(currencyWeight));
    }

    /// <summary>
    ///     Randomise the size of an ammo stack
    /// </summary>
    /// <param name="isPmc">Is ammo on a PMC bot</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="ammoItem">Ammo item to randomise</param>
    public void RandomiseAmmoStackSize(bool isPmc, TemplateItem itemTemplate, Item ammoItem)
    {
        var randomSize = itemHelper.GetRandomisedAmmoStackSize(itemTemplate);
        itemHelper.AddUpdObjectToItem(ammoItem);

        ammoItem.Upd.StackObjectsCount = randomSize;
    }

    /// <summary>
    ///     Get spawn limits for a specific bot type from bot.json config
    ///     If no limit found for a non pmc bot, fall back to defaults
    /// </summary>
    /// <param name="botRole">what role does the bot have</param>
    /// <returns>Dictionary of tplIds and limit</returns>
    public Dictionary<MongoId, double> GetItemSpawnLimitsForBotType(string botRole)
    {
        if (botHelper.IsBotPmc(botRole))
        {
            return _botConfig.ItemSpawnLimits["pmc"];
        }

        if (_botConfig.ItemSpawnLimits.ContainsKey(botRole.ToLowerInvariant()))
        {
            return _botConfig.ItemSpawnLimits[botRole.ToLowerInvariant()];
        }

        logger.Warning(serverLocalisationService.GetText("bot-unable_to_find_spawn_limits_fallback_to_defaults", botRole));

        return [];
    }

    /// <summary>
    ///     Get the parentId or tplId of item inside spawnLimits object if it exists
    /// </summary>
    /// <param name="itemTemplate">item we want to look for in spawn limits</param>
    /// <param name="spawnLimits">Limits to check for item</param>
    /// <returns>id as string, otherwise undefined</returns>
    public MongoId? GetMatchingIdFromSpawnLimits(TemplateItem itemTemplate, Dictionary<MongoId, double> spawnLimits)
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
